using AutoMapper;
using developers.Models;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
CreateMap<Developer, DeveloperDTO>()
            .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.ID))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.user.Name));

        CreateMap<ProjectDeveloper, ProjectDeveloperDTO>();
        CreateMap<ProjectDeveloperDTO, ProjectDeveloper>();


CreateMap<TaskDto, TaskCard>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TaskId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.TaskDescription))
            .ForMember(dest => dest.ProjectID, opt => opt.MapFrom(src => src.ProjectId));

  CreateMap<TaskDto, TaskCard>()

            .ForMember(dest=> dest.Id , opt=>opt.MapFrom(src=>src.TaskId))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.TaskName))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.TaskDescription))
            .ForMember(dest => dest.ProjectID, opt => opt.MapFrom(src => src.ProjectId));

        CreateMap<TaskCard, TaskDto>()
            .ForMember(dest=> dest.TaskId , opt=>opt.MapFrom(src=>src.Id))
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.TaskDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectID));

    }


}


    




    


    
