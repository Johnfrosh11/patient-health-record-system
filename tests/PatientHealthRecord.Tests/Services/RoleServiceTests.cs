using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.Roles;
using PatientHealthRecord.Application.Services.Roles;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Tests.Helpers;

namespace PatientHealthRecord.Tests.Services;

public class RoleServiceTests
{
    private readonly Guid _testUserId;
    private readonly Guid _testOrgId;
    private readonly Mock<IAuthUser> _mockAuthUser;
    private readonly Mock<ILogger<RoleService>> _mockLogger;

    public RoleServiceTests()
    {
        _testUserId = Guid.NewGuid();
        _testOrgId = Guid.NewGuid();
        _mockAuthUser = TestHelpers.CreateMockAuthUser(_testUserId, _testOrgId);
        _mockLogger = TestHelpers.CreateMockLogger<RoleService>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateRole()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateRoleRequest
        {
            Name = "NewRole",
            Description = "A new test role"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("NewRole");
        result.Description.Should().Be("A new test role");
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var existingRole = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "ExistingRole",
            Description = "Existing",
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Roles.AddAsync(existingRole);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateRoleRequest
        {
            Name = "ExistingRole",
            Description = "Try to create duplicate"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.CreateAsync(request));
    }

    [Fact]
    public async Task AssignPermissionAsync_WithValidData_ShouldAssignPermission()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TestRole",
            Description = "Test",
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
        
        await db.Roles.AddAsync(role);
        await db.Permissions.AddAsync(permission);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.AssignPermissionAsync(role.RoleId, permission.PermissionId);

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be("TestRole");
        result.PermissionName.Should().Be("testPermission");
        result.Message.Should().Contain("assigned");
    }

    [Fact]
    public async Task RemovePermissionAsync_WithAssignedPermission_ShouldRemove()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "TestRole",
            Description = "Test",
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
        
        var rolePermission = new TRolePermission
        {
            RoleId = role.RoleId,
            PermissionId = permission.PermissionId
        };
        
        await db.Roles.AddAsync(role);
        await db.Permissions.AddAsync(permission);
        await db.RolePermissions.AddAsync(rolePermission);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.RemovePermissionAsync(role.RoleId, permission.PermissionId);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("removed");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveRoles()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var activeRole = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "ActiveRole",
            Description = "Active",
            IsActive = true,
            CreatedBy = "system"
        };
        
        var inactiveRole = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "InactiveRole",
            Description = "Inactive",
            IsActive = false,
            CreatedBy = "system"
        };
        
        await db.Roles.AddRangeAsync(activeRole, inactiveRole);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("ActiveRole");
    }

    [Fact]
    public async Task DeleteAsync_WithNoAssignedUsers_ShouldSoftDelete()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "ToDelete",
            Description = "Will be deleted",
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Roles.AddAsync(role);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.DeleteAsync(role.RoleId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("ToDelete");
        result.Message.Should().Contain("deleted");
    }

    [Fact]
    public async Task DeleteAsync_WithAssignedUsers_ShouldThrowException()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var role = new TRole
        {
            RoleId = Guid.NewGuid(),
            RoleName = "RoleWithUsers",
            Description = "Has users",
            IsActive = true,
            CreatedBy = "system"
        };
        
        var user = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "user",
            Email = "user@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var userRole = new TUserRole
        {
            UserId = user.UserId,
            RoleId = role.RoleId
        };
        
        await db.Roles.AddAsync(role);
        await db.Users.AddAsync(user);
        await db.UserRoles.AddAsync(userRole);
        await db.SaveChangesAsync();

        var service = new RoleService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.DeleteAsync(role.RoleId));
    }
}
