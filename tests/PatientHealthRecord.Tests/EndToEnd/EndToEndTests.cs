using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Application.DTOs.Auth;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Repository;
using PatientHealthRecord.Utilities;

namespace PatientHealthRecord.Tests.EndToEnd;

/// <summary>
/// Collection definition to ensure E2E tests run sequentially
/// </summary>
[CollectionDefinition("E2E Tests", DisableParallelization = true)]
public class E2ETestCollection : ICollectionFixture<EndToEndTests.CustomWebApplicationFactory>
{
}

/// <summary>
/// End-to-end tests for the Patient Health Record API
/// Tests the complete user flow from registration to health record management
/// </summary>
[Collection("E2E Tests")]
public class EndToEndTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly Guid TestOrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public EndToEndTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        // Clear any existing auth header from previous tests
        _client.DefaultRequestHeaders.Authorization = null;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    #region Authentication Tests

    [Fact]
    public async Task E2E_01_Register_NewUser_ReturnsSuccess()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "newuser@testhospital.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User",
            OrganizationId = TestOrgId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<AuthViewModel>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data.Should().NotBeNull();
        content.data!.AccessToken.Should().NotBeNullOrEmpty();
        content.data.Username.Should().Be("newuser");
    }

    [Fact]
    public async Task E2E_02_Register_DuplicateUsername_ReturnsBadRequest()
    {
        // Arrange - First register
        var registerDto = new RegisterDto
        {
            Username = "duplicateuser",
            Email = "duplicate@testhospital.com",
            Password = "Password123!",
            FirstName = "Duplicate",
            LastName = "User",
            OrganizationId = TestOrgId
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Act - Try to register again with same username
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task E2E_03_Login_ValidCredentials_ReturnsTokens()
    {
        // Arrange - Login with pre-seeded admin
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "Admin123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<AuthViewModel>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data.Should().NotBeNull();
        content.data!.AccessToken.Should().NotBeNullOrEmpty();
        content.data.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task E2E_04_Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task E2E_05_AccessProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/healthrecords/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Health Records Tests

    [Fact]
    public async Task E2E_06_CreateHealthRecord_WithValidToken_ReturnsSuccess()
    {
        // Arrange - Login first
        var token = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateHealthRecordRequest
        {
            PatientName = "John Doe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Diagnosis = "Routine checkup",
            TreatmentPlan = "Annual physical exam",
            MedicalHistory = "No significant medical history"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data.Should().NotBeNull();
        content.data!.PatientName.Should().Be("John Doe");
    }

    [Fact]
    public async Task E2E_07_GetHealthRecord_ById_ReturnsRecord()
    {
        // Arrange - Login and create a record
        var token = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateHealthRecordRequest
        {
            PatientName = "Jane Smith",
            DateOfBirth = new DateTime(1985, 3, 20),
            Diagnosis = "Hypertension",
            TreatmentPlan = "Medication and lifestyle changes"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        var recordId = created!.data!.HealthRecordId;

        // Act
        var response = await _client.GetAsync($"/api/v1/healthrecords/{recordId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data!.PatientName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task E2E_08_GetAllHealthRecords_WithPagination_ReturnsRecords()
    {
        // Arrange - Login and create multiple records
        var token = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a few records
        for (int i = 1; i <= 3; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/healthrecords/create", new CreateHealthRecordRequest
            {
                PatientName = $"Patient {i}",
                DateOfBirth = new DateTime(1990, 1, i),
                Diagnosis = $"Diagnosis {i}",
                TreatmentPlan = $"Treatment {i}"
            });
        }

        // Act
        var response = await _client.GetAsync("/api/v1/healthrecords/all?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<List<HealthRecordResponse>>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task E2E_09_UpdateHealthRecord_ReturnsUpdatedRecord()
    {
        // Arrange - Login and create a record
        var token = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateHealthRecordRequest
        {
            PatientName = "Update Test",
            DateOfBirth = new DateTime(1995, 6, 10),
            Diagnosis = "Initial Diagnosis",
            TreatmentPlan = "Initial Plan"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        var recordId = created!.data!.HealthRecordId;

        // Act
        var updateDto = new UpdateHealthRecordRequest
        {
            HealthRecordId = recordId,
            PatientName = "Update Test",
            DateOfBirth = new DateTime(1995, 6, 10),
            Diagnosis = "Updated Diagnosis",
            TreatmentPlan = "Updated Plan"
        };
        var response = await _client.PutAsJsonAsync("/api/v1/healthrecords/update", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data!.Diagnosis.Should().Be("Updated Diagnosis");
        content.data.TreatmentPlan.Should().Be("Updated Plan");
    }

    [Fact]
    public async Task E2E_10_DeleteHealthRecord_ReturnsSuccess()
    {
        // Arrange - Login and create a record
        var token = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new CreateHealthRecordRequest
        {
            PatientName = "Delete Test",
            DateOfBirth = new DateTime(1992, 8, 25),
            Diagnosis = "To be deleted",
            TreatmentPlan = "N/A"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createDto);
        var created = await createResponse.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        var recordId = created!.data!.HealthRecordId;

        // Act
        var response = await _client.DeleteAsync($"/api/v1/healthrecords/{recordId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Access Request Flow Tests

    [Fact]
    public async Task E2E_11_CompleteAccessRequestFlow_CreateApproveAccess()
    {
        // Arrange - Create two users: record owner and requestor
        var adminToken = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Admin creates a health record
        var createRecordDto = new CreateHealthRecordRequest
        {
            PatientName = "Protected Patient",
            DateOfBirth = new DateTime(1988, 4, 12),
            Diagnosis = "Sensitive diagnosis",
            TreatmentPlan = "Confidential treatment"
        };
        var createRecordResponse = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createRecordDto);
        var createdRecord = await createRecordResponse.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        var healthRecordId = createdRecord!.data!.HealthRecordId;

        // Register a new user who will request access
        _client.DefaultRequestHeaders.Authorization = null;
        var registerDto = new RegisterDto
        {
            Username = "requestoruser",
            Email = "requestor@testhospital.com",
            Password = "Requestor123!",
            FirstName = "Access",
            LastName = "Requestor",
            OrganizationId = TestOrgId
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Login as requestor
        var requestorToken = await LoginAndGetTokenAsync("requestoruser", "Requestor123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requestorToken);

        // Create access request
        var accessRequestDto = new CreateAccessRequestRequest
        {
            HealthRecordId = healthRecordId,
            Reason = "Need to review patient history for consultation"
        };
        var accessRequestResponse = await _client.PostAsJsonAsync("/api/v1/accessrequests", accessRequestDto);
        accessRequestResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var accessRequest = await accessRequestResponse.Content.ReadFromJsonAsync<AccessRequestResponse>(_jsonOptions);
        var accessRequestId = accessRequest!.Id;

        // Switch to admin to approve the request
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Get pending requests
        var pendingResponse = await _client.GetAsync("/api/v1/accessrequests/pending");
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Approve the request with time-bound access
        var approveDto = new ApproveAccessRequestRequest
        {
            AccessStartDateTime = DateTime.UtcNow,
            AccessEndDateTime = DateTime.UtcNow.AddHours(24),
            ReviewComment = "Approved for consultation"
        };
        var approveResponse = await _client.PostAsJsonAsync($"/api/v1/accessrequests/{accessRequestId}/approve", approveDto);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify the request is now approved
        var getRequestResponse = await _client.GetAsync($"/api/v1/accessrequests/{accessRequestId}");
        var approvedRequest = await getRequestResponse.Content.ReadFromJsonAsync<AccessRequestResponse>(_jsonOptions);
        approvedRequest!.Status.Should().Be(AccessRequestStatus.Approved);
        approvedRequest.IsAccessActive.Should().BeTrue();
    }

    [Fact]
    public async Task E2E_12_AccessRequestFlow_DeclineRequest()
    {
        // Arrange - Admin creates a record
        var adminToken = await LoginAndGetTokenAsync("admin", "Admin123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createRecordDto = new CreateHealthRecordRequest
        {
            PatientName = "Another Patient",
            DateOfBirth = new DateTime(1975, 11, 30),
            Diagnosis = "Restricted information",
            TreatmentPlan = "Specialized treatment"
        };
        var createRecordResponse = await _client.PostAsJsonAsync("/api/v1/healthrecords/create", createRecordDto);
        var createdRecord = await createRecordResponse.Content.ReadFromJsonAsync<ResponseModel<HealthRecordResponse>>(_jsonOptions);
        var healthRecordId = createdRecord!.data!.HealthRecordId;

        // Register another requestor
        _client.DefaultRequestHeaders.Authorization = null;
        var registerDto = new RegisterDto
        {
            Username = "declineduser",
            Email = "declined@testhospital.com",
            Password = "Declined123!",
            FirstName = "Declined",
            LastName = "User",
            OrganizationId = TestOrgId
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        // Login and create access request
        var requestorToken = await LoginAndGetTokenAsync("declineduser", "Declined123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", requestorToken);

        var accessRequestDto = new CreateAccessRequestRequest
        {
            HealthRecordId = healthRecordId,
            Reason = "Insufficient justification"
        };
        var accessRequestResponse = await _client.PostAsJsonAsync("/api/v1/accessrequests", accessRequestDto);
        var accessRequest = await accessRequestResponse.Content.ReadFromJsonAsync<AccessRequestResponse>(_jsonOptions);
        var accessRequestId = accessRequest!.Id;

        // Admin declines the request
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var declineDto = new DeclineAccessRequestRequest
        {
            ReviewComment = "Request does not meet access criteria"
        };
        var declineResponse = await _client.PostAsJsonAsync($"/api/v1/accessrequests/{accessRequestId}/decline", declineDto);
        declineResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify declined status
        var getRequestResponse = await _client.GetAsync($"/api/v1/accessrequests/{accessRequestId}");
        var declinedRequest = await getRequestResponse.Content.ReadFromJsonAsync<AccessRequestResponse>(_jsonOptions);
        declinedRequest!.Status.Should().Be(AccessRequestStatus.Rejected);
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task E2E_13_RefreshToken_ValidToken_ReturnsNewTokens()
    {
        // Arrange - Login to get tokens
        var loginDto = new LoginDto
        {
            Username = "admin",
            Password = "Admin123!"
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ResponseModel<AuthViewModel>>(_jsonOptions);
        var refreshToken = loginResult!.data!.RefreshToken;

        // Act - Use refresh token
        var refreshDto = new RefreshTokenDto { RefreshToken = refreshToken };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", refreshDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<AuthViewModel>>(_jsonOptions);
        content.Should().NotBeNull();
        content!.success.Should().BeTrue();
        content.data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task E2E_14_AccessAdminEndpoint_WithoutPermission_ReturnsForbidden()
    {
        // Arrange - Register a regular user (no admin permissions)
        var registerDto = new RegisterDto
        {
            Username = "regularuser",
            Email = "regular@testhospital.com",
            Password = "Regular123!",
            FirstName = "Regular",
            LastName = "User",
            OrganizationId = TestOrgId
        };
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerDto);

        var token = await LoginAndGetTokenAsync("regularuser", "Regular123!");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act - Try to access admin-only endpoint (approve access requests)
        var response = await _client.GetAsync("/api/v1/accessrequests/pending");

        // Assert - Should be forbidden (no approveAccessRequests permission)
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Helper Methods

    private async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var loginDto = new LoginDto { Username = username, Password = password };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginDto);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<ResponseModel<AuthViewModel>>(_jsonOptions);
        return content!.data!.AccessToken;
    }

    #endregion

    #region Custom WebApplicationFactory

    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private static bool _isSeeded = false;
        private static readonly object _seedLock = new();
        private static readonly InMemoryDatabaseRoot _databaseRoot = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Set environment to Testing - this will load appsettings.Testing.json automatically
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove ALL DbContext and DbContextOptions related registrations
                var descriptorsToRemove = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<PatientHealthRecordDbContext>) ||
                    d.ServiceType == typeof(PatientHealthRecordDbContext) ||
                    d.ServiceType.Name.Contains("DbContextOptions")).ToList();
                
                foreach (var descriptor in descriptorsToRemove)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing - shared across all tests using InMemoryDatabaseRoot
                services.AddDbContext<PatientHealthRecordDbContext>(options =>
                {
                    options.UseInMemoryDatabase("E2ETestDatabase", _databaseRoot);
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Seed data once after host is created
            lock (_seedLock)
            {
                if (!_isSeeded)
                {
                    using var scope = host.Services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PatientHealthRecordDbContext>();
                    
                    // Only seed if the database is empty
                    if (!db.Users.Any())
                    {
                        SeedTestData(db);
                    }
                    _isSeeded = true;
                }
            }

            return host;
        }

        private static void SeedTestData(PatientHealthRecordDbContext db)
        {
            var testOrgId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            // Create permissions
            var permissions = new List<TPermission>
            {
                new() { PermissionId = Guid.NewGuid(), PermissionName = "viewPatientRecords", Description = "View patient records", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "createPatientRecords", Description = "Create patient records", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "editPatientRecords", Description = "Edit patient records", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "deletePatientRecords", Description = "Delete patient records", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "approveAccessRequests", Description = "Approve access requests", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "manageUsers", Description = "Manage users", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "manageRoles", Description = "Manage roles", CreatedBy = "system" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "viewAuditLogs", Description = "View audit logs", CreatedBy = "system" }
            };
            db.Permissions.AddRange(permissions);

            // Create Admin role
            var adminRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var adminRole = new TRole
            {
                RoleId = adminRoleId,
                RoleName = "Admin",
                Description = "Administrator with full access",
                CreatedBy = "system"
            };
            db.Roles.Add(adminRole);

            // Create Doctor role
            var doctorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var doctorRole = new TRole
            {
                RoleId = doctorRoleId,
                RoleName = "Doctor",
                Description = "Doctor role",
                CreatedBy = "system"
            };
            db.Roles.Add(doctorRole);

            // Create Patient role
            var patientRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var patientRole = new TRole
            {
                RoleId = patientRoleId,
                RoleName = "Patient",
                Description = "Patient role",
                CreatedBy = "system"
            };
            db.Roles.Add(patientRole);

            db.SaveChanges();

            // Assign permissions to Admin role
            foreach (var permission in permissions)
            {
                db.RolePermissions.Add(new TRolePermission
                {
                    RoleId = adminRoleId,
                    PermissionId = permission.PermissionId
                });
            }

            // Assign limited permissions to Doctor role
            var doctorPermissions = permissions.Where(p =>
                p.PermissionName == "viewPatientRecords" ||
                p.PermissionName == "createPatientRecords" ||
                p.PermissionName == "editPatientRecords");
            foreach (var permission in doctorPermissions)
            {
                db.RolePermissions.Add(new TRolePermission
                {
                    RoleId = doctorRoleId,
                    PermissionId = permission.PermissionId
                });
            }

            db.SaveChanges();

            // Create admin user
            var adminUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var adminUser = new TUser
            {
                UserId = adminUserId,
                Username = "admin",
                Email = "admin@testhospital.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "System",
                LastName = "Administrator",
                OrganizationId = testOrgId,
                IsActive = true,
                CreatedBy = "system"
            };
            db.Users.Add(adminUser);
            db.SaveChanges();

            // Assign Admin role to admin user
            db.UserRoles.Add(new TUserRole
            {
                UserId = adminUserId,
                RoleId = adminRoleId
            });

            db.SaveChanges();
        }
    }

    #endregion
}
