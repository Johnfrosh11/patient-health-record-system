using AutoMapper;
using PatientHealthRecord.Application.DTOs.AccessRequests;
using PatientHealthRecord.Application.DTOs.HealthRecords;
using PatientHealthRecord.Domain.Entities;
using PatientHealthRecord.Domain.Enums;

namespace PatientHealthRecord.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<HealthRecord, HealthRecordResponse>()
            .ForMember(dest => dest.CreatedByUsername, opt => opt.MapFrom(src => src.Creator.Username));

        CreateMap<CreateHealthRecordRequest, HealthRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Creator, opt => opt.Ignore())
            .ForMember(dest => dest.AccessRequests, opt => opt.Ignore());

        CreateMap<UpdateHealthRecordRequest, HealthRecord>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Creator, opt => opt.Ignore())
            .ForMember(dest => dest.AccessRequests, opt => opt.Ignore());

        CreateMap<AccessRequest, AccessRequestResponse>()
            .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.HealthRecord.PatientName))
            .ForMember(dest => dest.RequestingUsername, opt => opt.MapFrom(src => src.RequestingUser.Username))
            .ForMember(dest => dest.ReviewedByUsername, opt => opt.MapFrom(src => src.Reviewer != null ? src.Reviewer.Username : null))
            .ForMember(dest => dest.IsAccessActive, opt => opt.MapFrom(src =>
                src.Status == AccessRequestStatus.Approved &&
                src.AccessStartDateTime != null &&
                src.AccessEndDateTime != null &&
                DateTime.UtcNow >= src.AccessStartDateTime &&
                DateTime.UtcNow <= src.AccessEndDateTime));

        CreateMap<CreateAccessRequestRequest, AccessRequest>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RequestingUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AccessRequestStatus.Pending))
            .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewComment, opt => opt.Ignore())
            .ForMember(dest => dest.AccessStartDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.AccessEndDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.HealthRecord, opt => opt.Ignore())
            .ForMember(dest => dest.RequestingUser, opt => opt.Ignore())
            .ForMember(dest => dest.Reviewer, opt => opt.Ignore());
    }
}
