using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using PatientHealthRecord.Application.Common.Interfaces;
using PatientHealthRecord.Application.Common.Models;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;
using PatientHealthRecord.Infrastructure.Services;
using System.Security.Claims;

namespace PatientHealthRecord.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _tokenServiceMock = new Mock<ITokenService>();
        
        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "this-is-a-test-secret-key-32-characters-long!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        });
        
        _authService = new AuthService(_context, _tokenServiceMock.Object, jwtSettings);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var adminRole = new Role { Id = Guid.NewGuid(), Name = Roles.Admin };
        var doctorRole = new Role { Id = Guid.NewGuid(), Name = Roles.Doctor };
        var viewPermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.ViewPatientRecords };
        var createPermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.CreatePatientRecords };
        adminRole.RolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = adminRole.Id, PermissionId = viewPermission.Id, Permission = viewPermission },
            new RolePermission { RoleId = adminRole.Id, PermissionId = createPermission.Id, Permission = createPermission }
        };

        _context.Roles.AddRange(adminRole, doctorRole);
        _context.Permissions.AddRange(viewPermission, createPermission);
        _context.SaveChanges();
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        };

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake_access_token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("fake_refresh_token");
        var result = await _authService.RegisterAsync(request);
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.AccessToken.Should().BeEmpty();
        result.RefreshToken.Should().BeEmpty();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
        user.Should().NotBeNull();
        user!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ShouldThrowConflictException()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
            IsActive = true
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "existinguser",
            Email = "newemail@example.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        };
        await _authService.Invoking(s => s.RegisterAsync(request))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*username*already exists*");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowConflictException()
    {
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123"),
            IsActive = true
        };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Username = "newusername",
            Email = "existing@example.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        };
        await _authService.Invoking(s => s.RegisterAsync(request))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*email*already exists*");
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        var userId = Guid.NewGuid();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test@123");
        var role = await _context.Roles.FirstAsync();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = role.Id, Role = role }
            }
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "Test@123"
        };

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns("fake_access_token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("fake_refresh_token");
        var result = await _authService.LoginAsync(request);
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.AccessToken.Should().Be("fake_access_token");
        result.RefreshToken.Should().Be("fake_refresh_token");
        result.Roles.Should().Contain(role.Name);

        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId);
        refreshToken.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ShouldThrowUnauthorizedException()
    {
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Test@123"
        };
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword@123"),
            IsActive = true
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "WrongPassword@123"
        };
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid username or password*");
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "inactiveuser",
            Email = "inactive@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = false
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Username = "inactiveuser",
            Password = "Test@123"
        };
        await _authService.Invoking(s => s.LoginAsync(request))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*User account is inactive*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewAccessToken()
    {
        var userId = Guid.NewGuid();
        var role = await _context.Roles.FirstAsync();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = role.Id, Role = role }
            }
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid_refresh_token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            User = user
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = "valid_refresh_token" };

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<IEnumerable<Claim>>()))
            .Returns("new_access_token");
        var result = await _authService.RefreshTokenAsync(request);
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.RefreshToken.Should().Be("valid_refresh_token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ShouldThrowUnauthorizedException()
    {
        var request = new RefreshTokenRequest { RefreshToken = "invalid_token" };
        await _authService.Invoking(s => s.RefreshTokenAsync(request))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*Invalid refresh token*");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldThrowUnauthorizedException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired_token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            User = user
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var request = new RefreshTokenRequest { RefreshToken = "expired_token" };
        await _authService.Invoking(s => s.RefreshTokenAsync(request))
            .Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldRevokeToken()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true
        };

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid_token",
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        await _authService.LogoutAsync("valid_token");
        var token = await _context.RefreshTokens.FirstAsync(rt => rt.Token == "valid_token");
        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ShouldNotThrowException()
    {
        var invalidToken = "invalid_token";
        await _authService.LogoutAsync(invalidToken);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithValidUserId_ShouldReturnUserInfo()
    {
        var userId = Guid.NewGuid();
        var role = await _context.Roles.FirstAsync();

        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = userId, RoleId = role.Id, Role = role }
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var result = await _authService.GetCurrentUserAsync(userId);
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
        result.Roles.Should().Contain(role.Name);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInvalidUserId_ShouldReturnNull()
    {
        var invalidUserId = Guid.NewGuid();

        var result = await _authService.GetCurrentUserAsync(invalidUserId);

        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }}