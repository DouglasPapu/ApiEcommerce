using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class CategoryProfile : Profile
{

    public CategoryProfile()
    {
        //This means that we are mapping the Category model to the CategoryDto model and vice versa. The ReverseMap() method allows us to map in both directions.
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
    }

}