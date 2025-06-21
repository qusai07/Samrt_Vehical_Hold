using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Linq;
using AutoMapper;

namespace Samrt_Vehical_Hold.Mapper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //CreateMap<LookUp, LookUpDto>().ReverseMap();

            //CreateMap<UserDto, User>().ReverseMap()
            //     .ForMember(dest => dest.AcademicLevelId, opt => opt.MapFrom(src => src.LevelId))
            //     .ForMember(dest => dest.SubAcademicLevelsId, opt => opt.MapFrom(src => src.SubLevelId))
            //     .ForMember(dest => dest.SubAcademicLevelsId, opt => opt.MapFrom(src => src.SubLevelId))
            //     .ReverseMap()
              //  ;
       
        }
    }
}
