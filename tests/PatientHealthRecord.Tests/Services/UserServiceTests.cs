using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Users;
using PatientHealthRecord.Application.Services.Users;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Tests.Helpers;

namespace PatientHealthRecord.Tests.Services;

public class UserServiceTests
{
    private readonly Guid _testUserId;
    private readonly Guid _testOrgId;
    private readonly Mock<IAuthUser> _mockAuthUser;
    private readonly Mock<ILogger<UserService>> _mockLogger;

    public UserServiceTests()
    {
        _testUserId = Guid.NewGuid();
        _testOrgId = Guid.NewGuid();
        _mockAuthUser = TestHelpers.CreateMockAuthUser(_testUserId, _testOrgId);
        _mockLogger = TestHelpers.CreateMockLogger<UserService>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.Email.Should().Be("newuser@example.com");
        result.FirstName.Should().Be("New");
        result.LastName.Should().Be("User");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateUsername_ShouldThrowException()
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
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Users.AddAsync(existingUser);
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateUserRequest
        {
            Username = "existinguser",
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.CreateAsync(request));
    }

    [Fact]
    public async Task AssignRoleAsync_WithValidData_ShouldAssignRole()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var user = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TestRole",
            Description = "Test",
            IsActive = true,
            CreatedBy = "system"
        };
        
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.AssignRoleAsync(user.UserId, role.RoleId);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("testuser");
        result.RoleName.Should().Be("TestRole");
        result.Message.Should().Contain("assigned");
    }

    [Fact]
    public async Task RemoveRoleAsync_WithAssignedRole_ShouldRemove()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var user = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TestRole",
            Description = "Test",
            IsActive = true,
            CreatedBy = "system"
        };
        
        var userRole = new TUserRole
        {
            UserId = user.UserId,
            RoleId = role.RoleId
        };
        
        await db.Users.AddAsync(user);
        await db.Roles.AddAsync(role);
        await db.UserRoles.AddAsync(userRole);
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.RemoveRoleAsync(user.UserId, role.RoleId);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("removed");
    }

    [Fact]
    public async Task DeactivateAsync_WithValidUser_ShouldDeactivate()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var userToDeactivate = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "todeactivate",
            Email = "deactivate@example.com",
            PasswordHash = "hash",
            FirstName = "To",
            LastName = "Deactivate",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Users.AddAsync(userToDeactivate);
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.DeactivateAsync(userToDeactivate.UserId);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateAsync_SelfDeactivation_ShouldThrowException()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var currentUser = new TUser
        {
            UserId = _testUserId,
            Username = "currentuser",
            Email = "current@example.com",
            PasswordHash = "hash",
            FirstName = "Current",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Users.AddAsync(currentUser);
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.DeactivateAsync(_testUserId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPaginatedUsers()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        for (int i = 1; i <= 15; i++)
        {
            await db.Users.AddAsync(new TUser
            {
                UserId = Guid.NewGuid(),
                Username = $"user{i}",
                Email = $"user{i}@example.com",
                PasswordHash = "hash",
                FirstName = "User",
                LastName = $"{i}",
                OrganizationId = _testOrgId,
                IsActive = true,
                CreatedBy = "system"
            });
        }
        await db.SaveChangesAsync();

        var service = new UserService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.GetAllAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }
}
