using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PatientHealthRecord.Application.Auth.Interface;
using PatientHealthRecord.Repository;

namespace PatientHealthRecord.Tests.Helpers;

/// <summary>
/// Test helper for creating in-memory database and mock dependencies
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    public static PatientHealthRecordDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<PatientHealthRecordDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new PatientHealthRecordDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a mock IAuthUser with standard test values
    /// </summary>
    public static Mock<IAuthUser> CreateMockAuthUser(
        Guid? userId = null, 
        Guid? organizationId = null)
    {
        var mock = new Mock<IAuthUser>();
        var testUserId = userId ?? Guid.NewGuid();
        var testOrgId = organizationId ?? Guid.NewGuid();

        mock.Setup(x => x.UserId).Returns(testUserId);
        mock.Setup(x => x.OrganizationId).Returns(testOrgId);
        mock.Setup(x => x.Email).Returns("test@example.com");
        mock.Setup(x => x.FullName).Returns("Test User");
        mock.Setup(x => x.Username).Returns("testuser");
        mock.Setup(x => x.CorrelationId).Returns(Guid.NewGuid().ToString());
        mock.Setup(x => x.Authenticated).Returns(true);

        return mock;
    }

    /// <summary>
    /// Creates a mock logger for any type
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }
}
