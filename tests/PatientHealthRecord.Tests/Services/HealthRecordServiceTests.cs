using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain.Constants;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;
using PatientHealthRecord.Domain.Exceptions;
using PatientHealthRecord.Infrastructure.Data;
using PatientHealthRecord.Infrastructure.Services;

namespace PatientHealthRecord.Tests.Services;

public class HealthRecordServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly HealthRecordService _service;
    private readonly Guid _adminUserId;
    private readonly Guid _doctorUserId;
    private readonly Guid _nurseUserId;

    public HealthRecordServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new HealthRecordService(_context);

        _adminUserId = Guid.NewGuid();
        _doctorUserId = Guid.NewGuid();
        _nurseUserId = Guid.NewGuid();

        SeedTestData();
    }

    private void SeedTestData()
    {
        var viewPermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.ViewPatientRecords };
        var createPermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.CreatePatientRecords };
        var updatePermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.UpdatePatientRecords };
        var deletePermission = new Permission { Id = Guid.NewGuid(), Name = Permissions.DeletePatientRecords };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = Roles.Admin };
        var doctorRole = new Role { Id = Guid.NewGuid(), Name = Roles.Doctor };
        var nurseRole = new Role { Id = Guid.NewGuid(), Name = Roles.Nurse };

        adminRole.RolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = adminRole.Id, PermissionId = viewPermission.Id, Permission = viewPermission },
            new RolePermission { RoleId = adminRole.Id, PermissionId = createPermission.Id, Permission = createPermission },
            new RolePermission { RoleId = adminRole.Id, PermissionId = updatePermission.Id, Permission = updatePermission },
            new RolePermission { RoleId = adminRole.Id, PermissionId = deletePermission.Id, Permission = deletePermission }
        };

        doctorRole.RolePermissions = new List<RolePermission>
        {
            new RolePermission { RoleId = doctorRole.Id, PermissionId = createPermission.Id, Permission = createPermission },
            new RolePermission { RoleId = doctorRole.Id, PermissionId = updatePermission.Id, Permission = updatePermission },
            new RolePermission { RoleId = doctorRole.Id, PermissionId = deletePermission.Id, Permission = deletePermission }
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

        _context.Permissions.AddRange(viewPermission, createPermission, updatePermission, deletePermission);
        _context.Roles.AddRange(adminRole, doctorRole, nurseRole);
        _context.Users.AddRange(adminUser, doctorUser, nurseUser);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateHealthRecord()
    {
        var request = new CreateHealthRecordRequest
        {
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment plan",
            MedicalHistory = "Test medical history"
        };
        var result = await _service.CreateAsync(request, _doctorUserId);
        result.Should().NotBeNull();
        result.PatientName.Should().Be("John Doe");
        result.CreatedBy.Should().Be(_doctorUserId);
        result.IsDeleted.Should().BeFalse();

        var record = await _context.HealthRecords.FindAsync(result.Id);
        record.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithoutPermission_ShouldThrowForbiddenException()
    {
        var request = new CreateHealthRecordRequest
        {
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment plan",
            MedicalHistory = "Test medical history"
        };
        await _service.Invoking(s => s.CreateAsync(request, _nurseUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetByIdAsync_AsCreator_ShouldReturnHealthRecord()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        var result = await _service.GetByIdAsync(recordId, _doctorUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(recordId);
        result.PatientName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WithViewPermission_ShouldReturnHealthRecord()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        var result = await _service.GetByIdAsync(recordId, _adminUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(recordId);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutAccess_ShouldThrowForbiddenException()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        await _service.Invoking(s => s.GetByIdAsync(recordId, _nurseUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetByIdAsync_WithApprovedAccessRequest_ShouldReturnHealthRecord()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            HealthRecordId = recordId,
            RequestingUserId = _nurseUserId,
            Reason = "Need access for treatment",
            Status = AccessRequestStatus.Approved,
            AccessStartDateTime = DateTime.UtcNow.AddMinutes(-10),
            AccessEndDateTime = DateTime.UtcNow.AddHours(1),
            ReviewedBy = _adminUserId
        };

        healthRecord.AccessRequests = new List<AccessRequest> { accessRequest };

        _context.HealthRecords.Add(healthRecord);
        _context.AccessRequests.Add(accessRequest);
        await _context.SaveChangesAsync();
        var result = await _service.GetByIdAsync(recordId, _nurseUserId);
        result.Should().NotBeNull();
        result.Id.Should().Be(recordId);
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedRecord_ShouldThrowNotFoundException()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = true
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        await _service.Invoking(s => s.GetByIdAsync(recordId, _doctorUserId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_AsCreator_ShouldUpdateHealthRecord()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Old diagnosis",
            TreatmentPlan = "Old treatment",
            MedicalHistory = "Old history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateHealthRecordRequest
        {
            PatientName = "John Doe Updated",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "New diagnosis",
            TreatmentPlan = "New treatment",
            MedicalHistory = "New history"
        };
        var result = await _service.UpdateAsync(recordId, updateRequest, _doctorUserId);
        result.Should().NotBeNull();
        result.PatientName.Should().Be("John Doe Updated");
        result.Diagnosis.Should().Be("New diagnosis");
        result.LastModifiedBy.Should().Be(_doctorUserId);
        result.LastModifiedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_NotCreator_ShouldThrowForbiddenException()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateHealthRecordRequest
        {
            PatientName = "John Doe Updated",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "New diagnosis",
            TreatmentPlan = "New treatment",
            MedicalHistory = "New history"
        };
        await _service.Invoking(s => s.UpdateAsync(recordId, updateRequest, _adminUserId))
            .Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*only update records you created*");
    }

    [Fact]
    public async Task DeleteAsync_AsCreator_ShouldSoftDeleteRecord()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        await _service.DeleteAsync(recordId, _doctorUserId);
        var deletedRecord = await _context.HealthRecords.FindAsync(recordId);
        deletedRecord.Should().NotBeNull();
        deletedRecord!.IsDeleted.Should().BeTrue();
        deletedRecord.DeletedBy.Should().Be(_doctorUserId);
        deletedRecord.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_NotCreator_ShouldThrowForbiddenException()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test diagnosis",
            TreatmentPlan = "Test treatment",
            MedicalHistory = "Test history",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        await _service.Invoking(s => s.DeleteAsync(recordId, _nurseUserId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetAllAsync_WithViewPermission_ShouldReturnAllRecords()
    {
        var record1 = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Patient 1",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Diagnosis 1",
            TreatmentPlan = "Treatment 1",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        var record2 = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Patient 2",
            DateOfBirth = new DateTime(1995, 1, 1),
            Diagnosis = "Diagnosis 2",
            TreatmentPlan = "Treatment 2",
            CreatedBy = _nurseUserId,
            IsDeleted = false
        };

        _context.HealthRecords.AddRange(record1, record2);
        await _context.SaveChangesAsync();
        var result = await _service.GetAllAsync(_adminUserId, page: 1, pageSize: 10);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_WithoutViewPermission_ShouldReturnOnlyAccessibleRecords()
    {
        var doctorRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Doctor's Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Diagnosis",
            TreatmentPlan = "Treatment",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        var nurseRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Nurse's Patient",
            DateOfBirth = new DateTime(1995, 1, 1),
            Diagnosis = "Diagnosis",
            TreatmentPlan = "Treatment",
            CreatedBy = _nurseUserId,
            IsDeleted = false
        };

        _context.HealthRecords.AddRange(doctorRecord, nurseRecord);
        await _context.SaveChangesAsync();
        var result = await _service.GetAllAsync(_doctorUserId, page: 1, pageSize: 10);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().PatientName.Should().Be("Doctor's Patient");
    }

    [Fact]
    public async Task GetMyRecordsAsync_ShouldReturnOnlyUserRecords()
    {
        var doctorRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Doctor's Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Diagnosis",
            TreatmentPlan = "Treatment",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        var nurseRecord = new HealthRecord
        {
            Id = Guid.NewGuid(),
            PatientName = "Nurse's Patient",
            DateOfBirth = new DateTime(1995, 1, 1),
            Diagnosis = "Diagnosis",
            TreatmentPlan = "Treatment",
            CreatedBy = _nurseUserId,
            IsDeleted = false
        };

        _context.HealthRecords.AddRange(doctorRecord, nurseRecord);
        await _context.SaveChangesAsync();
        var result = await _service.GetMyRecordsAsync(_doctorUserId);
        result.Should().HaveCount(1);
        result.First().PatientName.Should().Be("Doctor's Patient");
    }

    [Fact]
    public async Task CanUserAccessRecordAsync_AsCreator_ShouldReturnTrue()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        var result = await _service.CanUserAccessRecordAsync(recordId, _doctorUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserAccessRecordAsync_WithViewPermission_ShouldReturnTrue()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();
        var result = await _service.CanUserAccessRecordAsync(recordId, _adminUserId);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanUserAccessRecordAsync_WithoutAccess_ShouldReturnFalse()
    {
        var recordId = Guid.NewGuid();
        var healthRecord = new HealthRecord
        {
            Id = recordId,
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Diagnosis = "Test",
            TreatmentPlan = "Test",
            CreatedBy = _doctorUserId,
            IsDeleted = false
        };

        _context.HealthRecords.Add(healthRecord);
        await _context.SaveChangesAsync();

        var result = await _service.CanUserAccessRecordAsync(recordId, _nurseUserId);

        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }}