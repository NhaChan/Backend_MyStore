using Microsoft.AspNetCore.Identity;
using MyStore.Repository.Users;
using MyStore.Response;
using MyStore.Models;
using AutoMapper;
using MyStore.DTO;
using MyStore.Constant;

namespace MyStore.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IDeliveryAdressRepository _deliveryAdressRepository;

        public UserService(UserManager<User> userManager, IUserRepository userRepository, IMapper mapper, IDeliveryAdressRepository deliveryAdressRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _deliveryAdressRepository = deliveryAdressRepository;
        }
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

        public async Task<AddressDTO?> GetUserAddress(string userId)
        {
            var delivery = await _deliveryAdressRepository.SingleOrDefaultAsync(e => e.User.Id == userId);
            if(delivery != null)
            {
                return _mapper.Map<AddressDTO>(delivery);
            }
            return null;
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
    }
}