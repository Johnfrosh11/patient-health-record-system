using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Application.Services.Auth;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Tests.Helpers;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IAuthUser> _mockAuthUser;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly IOptions<JwtSettings> _jwtOptions;

    public AuthServiceTests()
    {
        _mockAuthUser = TestHelpers.CreateMockAuthUser();
        _mockLogger = TestHelpers.CreateMockLogger<AuthService>();
        _jwtOptions = Options.Create(new JwtSettings
        {
            Key = "s/gUgkan5/nMUKcruWZz2lPnxdiPOntDWSPqlw7+pMDlM84QfMWk2RoMb5uVB2uTT97hof9k0FEgHcpWE8uzww==",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            JwtExpires = 3600
        });
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var orgId = Guid.NewGuid();
        
        var user = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 12),
            FirstName = "Test",
            LastName = "User",
            OrganizationId = orgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TestRole",
            Description = "Test Role",
            IsActive = true,
            CreatedBy = "system"
        };

        var permission = new TPermission
        {
            PermissionId = Guid.NewGuid(),
            PermissionName = "testPermission",
            Description = "Test Permission",
            IsActive = true,
            CreatedBy = "system"
        };

        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.Permissions.AddAsync(permission);
        await db.UserRoles.AddAsync(new TUserRole { UserId = user.UserId, RoleId = role.RoleId });
        await db.RolePermissions.AddAsync(new TRolePermission { RoleId = role.RoleId, PermissionId = permission.PermissionId });
        await db.SaveChangesAsync();

        var service = new AuthService(db, _mockAuthUser.Object, _jwtOptions, _mockLogger.Object);
        var loginDto = new LoginDto { Username = "testuser", Password = "Password123!" };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.success.Should().BeTrue();
        result.code.Should().Be("00");
        result.data.Should().NotBeNull();
        result.data!.Username.Should().Be("testuser");
        result.data.AccessToken.Should().NotBeNullOrEmpty();
        result.data.RefreshToken.Should().NotBeNullOrEmpty();
        result.data.Roles.Should().Contain("TestRole");
        result.data.Permissions.Should().Contain("testPermission");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var user = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!", 12),
            FirstName = "Test",
            LastName = "User",
            OrganizationId = Guid.NewGuid(),
            IsActive = true,
            CreatedBy = "system"
        };
        
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var service = new AuthService(db, _mockAuthUser.Object, _jwtOptions, _mockLogger.Object);
        var loginDto = new LoginDto { Username = "testuser", Password = "WrongPassword" };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.success.Should().BeFalse();
        result.code.Should().Be("99");
        result.message.Should().Contain("Invalid");
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentUser_ShouldReturnFailure()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new AuthService(db, _mockAuthUser.Object, _jwtOptions, _mockLogger.Object);
        var loginDto = new LoginDto { Username = "nonexistent", Password = "Password123!" };

        // Act
        var result = await service.LoginAsync(loginDto);

        // Assert
        result.success.Should().BeFalse();
        result.code.Should().Be("99");
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new AuthService(db, _mockAuthUser.Object, _jwtOptions, _mockLogger.Object);
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await service.RegisterAsync(registerDto);

        // Assert
        result.success.Should().BeTrue();
        result.code.Should().Be("00");
        result.data.Should().NotBeNull();
        result.data!.Username.Should().Be("newuser");
        
        // Verify user was persisted
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ShouldReturnFailure()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var existingUser = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash",
            FirstName = "Existing",
            LastName = "User",
            OrganizationId = Guid.NewGuid(),
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Users.AddAsync(existingUser);
        await db.SaveChangesAsync();

        var service = new AuthService(db, _mockAuthUser.Object, _jwtOptions, _mockLogger.Object);
        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "new@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await service.RegisterAsync(registerDto);

        // Assert
        result.success.Should().BeFalse();
        result.message.Should().Contain("Username");
    }
}
