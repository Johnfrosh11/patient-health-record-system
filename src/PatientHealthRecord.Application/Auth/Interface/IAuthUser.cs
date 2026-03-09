namespace PatientHealthRecord.Application.Auth.Interface;

/// <summary>
/// Authentication context - provides current user information from JWT claims
/// </summary>
public interface IAuthUser
{
    Guid   UserId         { get; }
    Guid   OrganizationId { get; }   // isolation key — used on every query
    string Email          { get; }
    string FullName       { get; }
    string Username       { get; }
    string CorrelationId  { get; }   // for structured logging + request tracing
    bool   Authenticated  { get; }
    string IpAddress      { get; }   // client IP for audit logging
}
