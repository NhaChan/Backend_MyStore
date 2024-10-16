﻿using Microsoft.AspNetCore.Identity;
using MyStore.Repository.Users;
using MyStore.Response;
using MyStore.Models;
using AutoMapper;
using MyStore.DTO;
using MyStore.Constant;
using MyStore.Repository.ProductRepository;
using MyStore.Request;

namespace MyStore.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IDeliveryAdressRepository _deliveryAdressRepository;
        private readonly IProductFavoriteRepository _productFavoriteRepository;

        public UserService(UserManager<User> userManager, IUserRepository userRepository,
            IMapper mapper, IDeliveryAdressRepository deliveryAdressRepository,
            IProductFavoriteRepository productFavoriteRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _deliveryAdressRepository = deliveryAdressRepository;
            _productFavoriteRepository = productFavoriteRepository;
        }

        public async Task AddProductFavorite(string userId, int productId)
        {
            var favorites = new ProductFavorite
            {
                UserId = userId,
                ProductId = productId,
            };
            await _productFavoriteRepository.AddAsync(favorites);
        }

        public async Task DeleteProductFavotite(string userId, int productId)
            => await _productFavoriteRepository.DeleteAsync(userId, productId);
        

        public async Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch)
        {
            int totalUser;
            IList<User> users;
            if(keySearch == null)
            {
                totalUser = await _userRepository.CountAsync();
                users = (await _userRepository.GetAllUserAsync(page, pageSize)).ToList();
            }
            else
            {
                totalUser = await _userRepository.CountAsync();
                users = (await _userRepository.GetAllUserAsync(page, pageSize, keySearch)).ToList();
            }
            //var items = users
            //    .Select(x => new UserResponse
            //    {
            //        Id = x.Id,
            //        FullName = x.FullName,
            //        Email = x.Email,
            //        PhoneNumber = x.PhoneNumber,
            //    });
            var items = _mapper.Map<IList<UserResponse>>(users);
            for (int i=0; i < users.Count(); i++)
            {
                var roles = await _userManager.GetRolesAsync(users[i]);
                if (roles != null)
                {
                    items[i].Roles = roles;
                }
            }

            return new PagedResponse<UserResponse>
            {
                TotalItems = totalUser,
                Items = items,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResponse<ProductDTO>> GetProductFavorite(string userId, PageRequest request)
        {
            var favorites = await _productFavoriteRepository
                .GetPagedAsync(request.page, request.pageSize, e => e.UserId == userId, e => e.CreatedAt);
            var total = await _productFavoriteRepository.CountAsync(e => e.UserId == userId);

            var products = favorites.Select(e => e.Product).ToList();

            var items = _mapper.Map<IEnumerable<ProductDTO>>(products).Select(x =>
            {
                var image = products.Single(e => e.Id == x.Id).Images.FirstOrDefault();
                if (image != null)
                {
                    x.ImageUrl = image.ImageUrl;
                }
                return x;
            });
            return new PagedResponse<ProductDTO>
            {
                Items = items,
                Page = request.page,
                PageSize = request.pageSize,
                TotalItems = total
            };
        }

        public async Task<AddressDTO?> GetUserAddress(string userId)
        {
            var delivery = await _deliveryAdressRepository.SingleOrDefaultAsync(e => e.User.Id == userId);
            if(delivery != null)
            {
                return _mapper.Map<AddressDTO>(delivery);
            }
            return null;
        }

        public async Task<UserDTO> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user != null)
            {
                var res = _mapper.Map<UserDTO>(user);
                return res;
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND_USER);
        }

        public async Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address)
        {
            try
            {
                var delivery = await _deliveryAdressRepository.SingleOrDefaultAsync(e => e.UserId == userId);
                if (delivery != null)
                {
                    delivery.Name = address.Name;
                    delivery.PhoneNumber = address.PhoneNumber;
                    delivery.Detail = address.Detail;
                    delivery.Province_id = address.Province_id;
                    delivery.Province_name = address.Province_name;
                    delivery.District_id = address.District_id;
                    delivery.District_name = address.District_name;
                    delivery.Ward_id = address.Ward_id;
                    delivery.Ward_name = address.Ward_name;

                    await _deliveryAdressRepository.UpdateAsync(delivery);
                    
                }
                else
                {
                    delivery = new DeliveryAddress
                    {
                        UserId = userId,
                        Name = address.Name,
                        PhoneNumber = address.PhoneNumber,
                        Detail = address.Detail,
                        Province_id = address.Province_id,
                        Province_name = address.Province_name,
                        District_id = address.District_id,
                        District_name = address.District_name,
                        Ward_id = address.Ward_id,
                        Ward_name = address.Ward_name
                    };

                    await _deliveryAdressRepository.AddAsync(delivery);
                }
                return _mapper.Map<AddressDTO?>(delivery);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserDTO> UpdateUserInfo(string userId, UserDTO request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.FullName = request.Fullname;
                user.PhoneNumber = request.PhoneNumber;
                await _userManager.UpdateAsync(user);
                return _mapper.Map<UserDTO>(user);
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND_USER);
        }
    }
}