using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;
using PatientHealthRecord.Infrastructure.Services;

namespace PatientHealthRecord.Tests.Services;

public class AccessRequestServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AccessRequestService _service;
    private readonly Guid _adminUserId;
    private readonly Guid _doctorUserId;
    private readonly Guid _nurseUserId;
    private readonly Guid _healthRecordId;

    public AccessRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new AccessRequestService(_context);

        _adminUserId = Guid.NewGuid();
        _doctorUserId = Guid.NewGuid();
        _nurseUserId = Guid.NewGuid();
        _healthRecordId = Guid.NewGuid();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var viewPermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.ViewPatientRecords };
        var approvePermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.ApproveAccessRequests };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = Roles.Admin };
        var doctorRole = new Role { Id = Guid.NewGuid(), Name = Roles.Doctor };
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = Roles.Nurse };

        adminRole.RolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = adminRole.Id, PermissionId = viewPermission.Id, Permission = viewPermission },
            new RolePermission { RoleId = adminRole.Id, PermissionId = approvePermission.Id, Permission = approvePermission }
        };
        var adminUser = new User
        {
            Id = _adminUserId,
            Username = "admin",
            Email = "admin@test.com",
            PasswordHash = "hash",
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = _adminUserId, RoleId = adminRole.Id, Role = adminRole }
            }
        };

        var doctorUser = new User
        {
            Id = _doctorUserId,
            Username = "doctor",
            Email = "doctor@test.com",
            PasswordHash = "hash",
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = _doctorUserId, RoleId = doctorRole.Id, Role = doctorRole }
            }
        };

        var nurseUser = new User
        {
            Id = _nurseUserId,
            Username = "nurse",
            Email = "nurse@test.com",
            PasswordHash = "hash",
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { UserId = _nurseUserId, RoleId = nurseRole.Id, Role = nurseRole }
            }
        };
        var healthRecord = new HealthRecord
        {
            Id = _healthRecordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.Permissions.AddRange(viewPermission, approvePermission);
        _context.Roles.AddRange(adminRole, doctorRole, nurseRole);
        _context.Users.AddRange(adminUser, doctorUser, nurseUser);
        _context.HealthRecords.Add(healthRecord);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateAccessRequest()
    {
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = _healthRecordId,
            Reason = "Need access for consultation"
        };
        var result = await _service.CreateAsync(request, _nurseUserId);
        result.Should().NotBeNull();
        result.HealthRecordId.Should().Be(_healthRecordId);
        result.RequestingUserId.Should().Be(_nurseUserId);
        result.Status.Should().Be(AccessRequestStatus.Pending);
        result.Reason.Should().Be("Need access for consultation");

        var accessRequest = await _context.AccessRequests.FindAsync(result.Id);
        accessRequest.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_UserWithViewPermission_ShouldThrowValidationException()
    {
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = _healthRecordId,
            Reason = "Need access"
        };
        await _service.Invoking(s => s.CreateAsync(request, _adminUserId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*view all patient records permission*");
    }

    [Fact]
    public async Task CreateAsync_RequestingOwnRecord_ShouldThrowValidationException()
    {
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = _healthRecordId,
            Reason = "Need access"
        };
        await _service.Invoking(s => s.CreateAsync(request, _doctorUserId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*cannot request access to your own health records*");
    }

    [Fact]
    public async Task CreateAsync_DuplicatePendingRequest_ShouldThrowConflictException()
    {
        var existingRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "First request",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(existingRequest);
        await _context.SaveChangesAsync();

        var newRequest = new CreateAccessRequestRequest
        {
            HealthRecordId = _healthRecordId,
            Reason = "Second request"
        };
        await _service.Invoking(s => s.CreateAsync(newRequest, _nurseUserId))
            .Should().ThrowAsync<ConflictException>()
            .WithMessage("*already have a pending access request*");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentHealthRecord_ShouldThrowNotFoundException()
    {
        var request = new CreateAccessRequestRequest
        {
            HealthRecordId = Guid.NewGuid(),
            Reason = "Need access"
        };
        await _service.Invoking(s => s.CreateAsync(request, _nurseUserId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ApproveAsync_WithValidData_ShouldApproveRequest()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow,
            AccessEndDateTime = DateTime.UtcNow.AddHours(2)
        };
        var result = await _service.ApproveAsync(accessRequestId, approveRequest, _adminUserId);
        result.Should().NotBeNull();
        result.Status.Should().Be(AccessRequestStatus.Approved);
        result.ReviewedBy.Should().Be(_adminUserId);
        result.AccessStartDateTime.Should().NotBeNull();
        result.AccessEndDateTime.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveAsync_WithoutApprovePermission_ShouldThrowForbiddenException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow,
            AccessEndDateTime = DateTime.UtcNow.AddHours(2)
        };
        await _service.Invoking(s => s.ApproveAsync(accessRequestId, approveRequest, _doctorUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ApproveAsync_NonPendingRequest_ShouldThrowValidationException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Approved
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow,
            AccessEndDateTime = DateTime.UtcNow.AddHours(2)
        };
        await _service.Invoking(s => s.ApproveAsync(accessRequestId, approveRequest, _adminUserId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*Cannot approve access request with status*");
    }

    [Fact]
    public async Task ApproveAsync_WithInvalidTimeRange_ShouldThrowValidationException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow.AddHours(2),
            AccessEndDateTime = DateTime.UtcNow.AddHours(1)
        };
        await _service.Invoking(s => s.ApproveAsync(accessRequestId, approveRequest, _adminUserId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*must be before*");
    }

    [Fact]
    public async Task ApproveAsync_WithPastEndDate_ShouldThrowValidationException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var approveRequest = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow.AddHours(-3),
            AccessEndDateTime = DateTime.UtcNow.AddHours(-1)
        };
        await _service.Invoking(s => s.ApproveAsync(accessRequestId, approveRequest, _adminUserId))
            .Should().ThrowAsync<ValidationException>()
            .WithMessage("*must be in the future*");
    }

    [Fact]
    public async Task DeclineAsync_WithValidData_ShouldDeclineRequest()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var declineRequest = new DeclineAccessRequestRequest
        {
            ReviewComment = "Insufficient justification"
        };
        var result = await _service.DeclineAsync(accessRequestId, declineRequest, _adminUserId);
        result.Should().NotBeNull();
        result.Status.Should().Be(AccessRequestStatus.Declined);
        result.ReviewedBy.Should().Be(_adminUserId);
        result.ReviewComment.Should().Be("Insufficient justification");
    }

    [Fact]
    public async Task DeclineAsync_WithoutApprovePermission_ShouldThrowForbiddenException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();

        var declineRequest = new DeclineAccessRequestRequest
        {
            ReviewComment = "Declined"
        };
        await _service.Invoking(s => s.DeclineAsync(accessRequestId, declineRequest, _nurseUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetAllAsync_AsRegularUser_ShouldReturnOnlyOwnRequests()
    {
        var nurseRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Nurse request",
            Status = AccessRequestStatus.Pending
        };

        var doctorRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _doctorUserId,
            Reason = "Doctor request",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.AddRange(nurseRequest, doctorRequest);
        await _context.SaveChangesAsync();
        var result = await _service.GetAllAsync(_nurseUserId, isApprover: false);
        result.Should().HaveCount(1);
        result.First().RequestingUserId.Should().Be(_nurseUserId);
    }

    [Fact]
    public async Task GetAllAsync_AsApprover_ShouldReturnAllRequests()
    {
        var nurseRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Nurse request",
            Status = AccessRequestStatus.Pending
        };

        var doctorRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _doctorUserId,
            Reason = "Doctor request",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.AddRange(nurseRequest, doctorRequest);
        await _context.SaveChangesAsync();
        var result = await _service.GetAllAsync(_adminUserId, isApprover: true);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_AsOwner_ShouldReturnRequest()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();
        var result = await _service.GetByIdAsync(accessRequestId, _nurseUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(accessRequestId);
    }

    [Fact]
    public async Task GetByIdAsync_NotOwnerNotApprover_ShouldThrowForbiddenException()
    {
        var accessRequestId = Guid.NewGuid();
        var accessRequest = new AccessRequest
        {
            Id = accessRequestId,
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access",
            Status = AccessRequestStatus.Pending
        };

        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();
        await _service.Invoking(s => s.GetByIdAsync(accessRequestId, _doctorUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetPendingAsync_ShouldReturnOnlyPendingRequests()
    {
        var pendingRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _nurseUserId,
            Reason = "Pending request",
            Status = AccessRequestStatus.Pending
        };

        var approvedRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = _healthRecordId,
            RequestingUserId = _doctorUserId,
            Reason = "Approved request",
            Status = AccessRequestStatus.Approved,
            ReviewedBy = _adminUserId
        };

        _context.AccessRequests.AddRange(pendingRequest, approvedRequest);
        await _context.SaveChangesAsync();

        var result = await _service.GetPendingAsync(_adminUserId);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(AccessRequestStatus.Pending);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}