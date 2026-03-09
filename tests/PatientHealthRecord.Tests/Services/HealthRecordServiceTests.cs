using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Application.Services.HealthRecords;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Tests.Helpers;

namespace PatientHealthRecord.Tests.Services;

public class HealthRecordServiceTests
{
    private readonly Guid _testUserId;
    private readonly Guid _testOrgId;
    private readonly Mock<IAuthUser> _mockAuthUser;
    private readonly Mock<ILogger<HealthRecordService>> _mockLogger;

    public HealthRecordServiceTests()
    {
        _testUserId = Guid.NewGuid();
        _testOrgId = Guid.NewGuid();
        _mockAuthUser = TestHelpers.CreateMockAuthUser(_testUserId, _testOrgId);
        _mockLogger = TestHelpers.CreateMockLogger<HealthRecordService>();
    }

    [Fact]
    public async Task CreateHealthRecordAsync_WithValidData_ShouldCreateRecord()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        // Create a user for the record creator
        var user = new TUser
        {
            UserId = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        var service = new HealthRecordService(db, _mockAuthUser.Object, _mockLogger.Object);
        var createDto = new CreateHealthRecordDto
        {
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 15),
            Diagnosis = "Common Cold",
            TreatmentPlan = "Rest and fluids",
            MedicalHistory = "No significant history"
        };

        // Act
        var result = await service.CreateHealthRecordAsync(createDto);

        // Assert
        result.success.Should().BeTrue();
        result.code.Should().Be("00");
        result.data.Should().NotBeNull();
        result.data!.PatientName.Should().Be("John Doe");
        result.data.Diagnosis.Should().Be("Common Cold");
    }

    [Fact]
    public async Task GetHealthRecordByIdAsync_WhenRecordExists_ShouldReturnRecord()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var user = new TUser
        {
            UserId = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var record = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Jane Doe",
            DateOfBirth = new DateTime(1985, 5, 20),
            Diagnosis = "Flu",
            TreatmentPlan = "Antiviral medication",
            CreatedByUserId = _testUserId,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = _testUserId.ToString()
        };
        
        await db.Users.AddAsync(user);
        await db.HealthRecords.AddAsync(record);
        await db.SaveChangesAsync();

        var service = new HealthRecordService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.GetHealthRecordByIdAsync(record.HealthRecordId);

        // Assert
        result.success.Should().BeTrue();
        result.data.Should().NotBeNull();
        result.data!.PatientName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task GetHealthRecordByIdAsync_WhenRecordNotFound_ShouldReturnFailure()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new HealthRecordService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.GetHealthRecordByIdAsync(Guid.NewGuid());

        // Assert
        result.success.Should().BeFalse();
        result.code.Should().Be("99");
    }

    [Fact]
    public async Task UpdateHealthRecordAsync_WhenUserIsCreator_ShouldUpdate()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var user = new TUser
        {
            UserId = _testUserId,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var record = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Original Name",
            DateOfBirth = new DateTime(1985, 5, 20),
            Diagnosis = "Original Diagnosis",
            TreatmentPlan = "Original Plan",
            CreatedByUserId = _testUserId,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = _testUserId.ToString()
        };
        
        await db.Users.AddAsync(user);
        await db.HealthRecords.AddAsync(record);
        await db.SaveChangesAsync();

        var service = new HealthRecordService(db, _mockAuthUser.Object, _mockLogger.Object);
        var updateDto = new UpdateHealthRecordDto
        {
            HealthRecordId = record.HealthRecordId,
            PatientName = "Updated Name",
            DateOfBirth = new DateTime(1985, 5, 20),
            Diagnosis = "Updated Diagnosis",
            TreatmentPlan = "Updated Plan"
        };

        // Act
        var result = await service.UpdateHealthRecordAsync(updateDto);

        // Assert
        result.success.Should().BeTrue();
        result.data.Should().NotBeNull();
        result.data!.PatientName.Should().Be("Updated Name");
        result.data.Diagnosis.Should().Be("Updated Diagnosis");
    }

    [Fact]
    public async Task DeleteHealthRecordAsync_WhenUserIsCreator_ShouldSoftDelete()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var record = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "To Delete",
            DateOfBirth = new DateTime(1985, 5, 20),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedByUserId = _testUserId,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = _testUserId.ToString()
        };
        
        await db.HealthRecords.AddAsync(record);
        await db.SaveChangesAsync();

        var service = new HealthRecordService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.DeleteHealthRecordAsync(record.HealthRecordId);

        // Assert
        result.success.Should().BeTrue();
        
        // Verify soft delete
        var deletedRecord = await db.HealthRecords
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.HealthRecordId == record.HealthRecordId);
        deletedRecord.Should().NotBeNull();
        deletedRecord!.IsActive.Should().BeFalse();
    }
}
