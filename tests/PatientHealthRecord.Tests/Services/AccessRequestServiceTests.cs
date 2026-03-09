using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Application.Services.AccessRequests;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Tests.Helpers;

namespace PatientHealthRecord.Tests.Services;

public class AccessRequestServiceTests
{
    private readonly Guid _testUserId;
    private readonly Guid _testOrgId;
    private readonly Mock<IAuthUser> _mockAuthUser;
    private readonly Mock<ILogger<AccessRequestService>> _mockLogger;

    public AccessRequestServiceTests()
    {
        _testUserId = Guid.NewGuid();
        _testOrgId = Guid.NewGuid();
        _mockAuthUser = TestHelpers.CreateMockAuthUser(_testUserId, _testOrgId);
        _mockLogger = TestHelpers.CreateMockLogger<AccessRequestService>();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateRequest()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var healthRecord = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Test Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedByUserId = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        await db.HealthRecords.AddAsync(healthRecord);
        await db.SaveChangesAsync();

        var service = new AccessRequestService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = healthRecord.HealthRecordId,
            Reason = "Need to review patient history"
        };

        // Act
        var result = await service.CreateAsync(request, _testUserId);

        // Assert
        result.Should().NotBeNull();
        result.Reason.Should().Be("Need to review patient history");
        result.Status.Should().Be(AccessRequestStatus.Pending);
        result.RequestingUserId.Should().Be(_testUserId);
    }

    [Fact]
    public async Task CreateAsync_WithNonexistentHealthRecord_ShouldThrowException()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        var service = new AccessRequestService(db, _mockAuthUser.Object, _mockLogger.Object);
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = Guid.NewGuid(),
            Reason = "Test reason"
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            service.CreateAsync(request, _testUserId));
    }

    [Fact]
    public async Task ApproveAsync_WithValidRequest_ShouldApproveWithTimeRange()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var requestingUserId = Guid.NewGuid();
        var reviewerId = _testUserId;
        
        var requestingUser = new TUser
        {
            UserId = requestingUserId,
            Username = "requester",
            Email = "requester@example.com",
            PasswordHash = "hash",
            FirstName = "Requester",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var healthRecord = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Test Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedByUserId = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var accessRequest = new TAccessRequest
        {
            AccessRequestId = Guid.NewGuid(),
            HealthRecordId = healthRecord.HealthRecordId,
            RequestingUserId = requestingUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = requestingUserId.ToString()
        };
        
        await db.Users.AddAsync(requestingUser);
        await db.HealthRecords.AddAsync(healthRecord);
        await db.AccessRequests.AddAsync(accessRequest);
        await db.SaveChangesAsync();

        var service = new AccessRequestService(db, _mockAuthUser.Object, _mockLogger.Object);
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddDays(1);
        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = startTime,
            AccessEndDateTime = endTime,
            ReviewComment = "Approved for one day"
        };

        // Act
        var result = await service.ApproveAsync(accessRequest.AccessRequestId, approveRequest, reviewerId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AccessRequestStatus.Approved);
        result.ReviewedBy.Should().Be(reviewerId);
        result.AccessStartDateTime.Should().Be(startTime);
        result.AccessEndDateTime.Should().Be(endTime);
    }

    [Fact]
    public async Task DeclineAsync_WithValidRequest_ShouldDecline()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var requestingUserId = Guid.NewGuid();
        var reviewerId = _testUserId;
        
        var requestingUser = new TUser
        {
            UserId = requestingUserId,
            Username = "requester",
            Email = "requester@example.com",
            PasswordHash = "hash",
            FirstName = "Requester",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var healthRecord = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Test Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedByUserId = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var accessRequest = new TAccessRequest
        {
            AccessRequestId = Guid.NewGuid(),
            HealthRecordId = healthRecord.HealthRecordId,
            RequestingUserId = requestingUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = requestingUserId.ToString()
        };
        
        await db.Users.AddAsync(requestingUser);
        await db.HealthRecords.AddAsync(healthRecord);
        await db.AccessRequests.AddAsync(accessRequest);
        await db.SaveChangesAsync();

        var service = new AccessRequestService(db, _mockAuthUser.Object, _mockLogger.Object);
        var declineRequest = new DeclineAccessRequestRequest
        {
            ReviewComment = "Access not needed at this time"
        };

        // Act
        var result = await service.DeclineAsync(accessRequest.AccessRequestId, declineRequest, reviewerId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(AccessRequestStatus.Rejected);
        result.ReviewedBy.Should().Be(reviewerId);
        result.ReviewComment.Should().Be("Access not needed at this time");
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPendingRequests()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDbContext();
        
        var requestingUser = new TUser
        {
            UserId = Guid.NewGuid(),
            Username = "requester",
            Email = "requester@example.com",
            PasswordHash = "hash",
            FirstName = "Requester",
            LastName = "User",
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var healthRecord = new THealthRecord
        {
            HealthRecordId = Guid.NewGuid(),
            PatientName = "Test Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedByUserId = Guid.NewGuid(),
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = "system"
        };
        
        var pendingRequest = new TAccessRequest
        {
            HealthRecordId = healthRecord.HealthRecordId,
            RequestingUserId = requestingUser.UserId,
            Reason = "Pending request",
            Status = AccessRequestStatus.Pending,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = requestingUser.UserId.ToString()
        };
        
        var approvedRequest = new TAccessRequest
        {
            HealthRecordId = healthRecord.HealthRecordId,
            RequestingUserId = requestingUser.UserId,
            Reason = "Approved request",
            Status = AccessRequestStatus.Approved,
            OrganizationId = _testOrgId,
            IsActive = true,
            CreatedBy = requestingUser.UserId.ToString()
        };
        
        await db.Users.AddAsync(requestingUser);
        await db.HealthRecords.AddAsync(healthRecord);
        await db.AccessRequests.AddRangeAsync(pendingRequest, approvedRequest);
        await db.SaveChangesAsync();

        var service = new AccessRequestService(db, _mockAuthUser.Object, _mockLogger.Object);

        // Act
        var result = await service.GetPendingAsync(_testUserId);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(AccessRequestStatus.Pending);
    }
}
