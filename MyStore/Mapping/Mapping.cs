﻿using AutoMapper;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Request;
using MyStore.Response;

namespace MyStore.Mapping
{
    public class Mapping : Profile
    {
        public Mapping() 
        { 
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<Brand, BrandDTO>().ReverseMap();
            CreateMap<ProductRequest, Product>().ReverseMap();
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(des => des.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(des => des.CategoryName, opt => opt.MapFrom(src => src.Caterory.Name));

            CreateMap<ProductRequest, Product>();
            CreateMap<Product, ProductDetailResponse>();
            CreateMap<AddressDTO, DeliveryAddress>().ReverseMap();

        }
    }
}
