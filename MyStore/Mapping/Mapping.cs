using AutoMapper;
using MyStore.DTO;
using MyStore.Models;

namespace MyStore.Mapping
{
    public class Mapping : Profile
    {
        public Mapping() 
        { 
            CreateMap<Category, CategoryDTO>().ReverseMap();
        }
    }
}
