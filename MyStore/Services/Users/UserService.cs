using Microsoft.AspNetCore.Identity;
using MyStore.Repository.Users;
using MyStore.Response;
using MyStore.Models;
using AutoMapper;
using MyStore.DTO;
using MyStore.Constant;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Storage;
using MyStore.Repository.TransactionRepository;
using System.Linq.Expressions;
using MyStore.Enumerations;

namespace MyStore.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IDeliveryAdressRepository _deliveryAdressRepository;
        private readonly IProductFavoriteRepository _productFavoriteRepository;
        private readonly IFileStorage _fileStorage;
        private readonly string path = "assets/images/avatars";
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPasswordHasher<User> _passageHasher;

        public UserService(UserManager<User> userManager, IUserRepository userRepository,
            IMapper mapper, IDeliveryAdressRepository deliveryAdressRepository,
            IProductFavoriteRepository productFavoriteRepository, IFileStorage fileStorage,
            ITransactionRepository transactionRepository, IPasswordHasher<User> passwordHasher)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _deliveryAdressRepository = deliveryAdressRepository;
            _productFavoriteRepository = productFavoriteRepository;
            _fileStorage = fileStorage;
            _transactionRepository = transactionRepository;
            _passageHasher = passwordHasher;
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


        public async Task<PagedResponse<UserResponse>> GetAllUserAsync(int page, int pageSize, string? keySearch, RolesEnum role)
        {
            int totalUser;
            IEnumerable<User> users;
            if (string.IsNullOrEmpty(keySearch))
            {
                totalUser = await _userRepository.CountAsync(e => e.UserRoles
                    .Any(e => !string.IsNullOrEmpty(e.Role.Name) && e.Role.Name.Equals(role.ToString())));
                users = await _userRepository.GetPageOrderByDescendingAsync(page, pageSize,
                   e => e.UserRoles.Any(e => !string.IsNullOrEmpty(e.Role.Name) && e.Role.Name.Equals(role.ToString())), e => e.CreatedAt);
            }
            else
            {
                Expression<Func<User, bool>> expression = e => (e.Id.Contains(keySearch)
                    || (e.FullName != null && e.FullName.Contains(keySearch))
                    || (e.Email != null && e.Email.Contains(keySearch))
                    || e.PhoneNumber != null && e.PhoneNumber.Contains(keySearch))
                    && e.UserRoles.Any(e => !string.IsNullOrEmpty(e.Role.Name) && e.Role.Name.Equals(role.ToString()));

                totalUser = await _userRepository.CountAsync(expression);
                users = await _userRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }


            var items = _mapper.Map<IEnumerable<UserResponse>>(users).Select(e =>
            {
                e.LockedOut = e.LockoutEnd > DateTime.Now;
                e.LockoutEnd = e.LockoutEnd > DateTime.Now ? e.LockoutEnd : null;
                return e;
            });

            //for (int i = 0; i < users.Count(); i++)
            //{
            //    var roles = await _userManager.GetRolesAsync(users[i]);
            //    if (roles != null)
            //    {
            //        items[i].Roles = roles;
            //    }
            //}
            return new PagedResponse<UserResponse>
            {
                Items = items,
                TotalItems = totalUser,
                Page = page,
                PageSize = pageSize,
            };
        }

        //public async Task<PagedResponse<UserResponse>> GetAllEmployeeAsync(int page, int pageSize, string? keySearch)
        //{
        //    int totalUser;
        //    IList<User> users;
        //    if (keySearch == null)
        //    {
        //        totalUser = await _userRepository.CountAsync();
        //        users = (await _userRepository.GetAllUserAsync(page, pageSize)).ToList();
        //    }
        //    else
        //    {
        //        totalUser = await _userRepository.CountAsync();
        //        users = (await _userRepository.GetAllUserAsync(page, pageSize, keySearch)).ToList();
        //    }
        //    var items = new List<UserResponse>();

        //    foreach (var user in users)
        //    {
        //        var roles = await _userManager.GetRolesAsync(user);

        //        if (roles.Count > 0)
        //        {
        //            var userResponse = _mapper.Map<UserResponse>(user);
        //            userResponse.Roles = roles;
        //            items.Add(userResponse);
        //        }
        //    }

        //    return new PagedResponse<UserResponse>
        //    {
        //        TotalItems = items.Count,
        //        Items = items,
        //        Page = page,
        //        PageSize = pageSize
        //    };
        //}

        public async Task<IEnumerable<int>> GetFavorites(string userId)
        {
            var favorites = await _productFavoriteRepository.GetAsync(e => e.UserId == userId);
            return favorites.Select(e => e.ProductId);
        }

        public async Task<ImageDTO> GetImage(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return _mapper.Map<ImageDTO>(user);
            }
            else { throw new ArgumentException(ErrorMessage.NOT_FOUND + " người dùng"); }
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
            if (delivery != null)
            {
                return _mapper.Map<AddressDTO>(delivery);
            }
            return null;
        }

        public async Task<UserDTO> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var res = _mapper.Map<UserDTO>(user);
                return res;
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND_USER);
        }

        public async Task<UserDTO> UpdateAvt(string userId, IFormFile image)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (user.ImageUrl != null)
                {
                    _fileStorage.Delete(user.ImageUrl);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                user.ImageUrl = Path.Combine(path, fileName);

                await _fileStorage.SaveAsync(path, image, fileName);

                await _userManager.UpdateAsync(user);

                return _mapper.Map<UserDTO>(user);
            }
            else { throw new ArgumentException(ErrorMessage.NOT_FOUND); }
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
                    delivery.ProvinceID = address.ProvinceID;
                    delivery.ProvinceName = address.ProvinceName;
                    delivery.DistrictID = address.DistrictID;
                    delivery.DistrictName = address.DistrictName;
                    delivery.WardID = address.WardID;
                    delivery.WardName = address.WardName;

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
                        ProvinceID = address.ProvinceID,
                        ProvinceName = address.ProvinceName,
                        DistrictID = address.DistrictID,
                        DistrictName = address.DistrictName,
                        WardID = address.WardID,
                        WardName = address.WardName
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
                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                await _userManager.UpdateAsync(user);
                return _mapper.Map<UserDTO>(user);
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND_USER);
        }

        public async Task<UserDTO> AddUser(UserCreateDTO user)
        {
            using (await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    var existingUser = await _userManager.FindByEmailAsync(user.Email);
                    if (existingUser != null)
                    {
                        throw new InvalidDataException(ErrorMessage.EXISTED); // You can customize the message as needed
                    }
                    var User = new User()
                    {
                        Email = user.Email,
                        NormalizedEmail = user.Email,
                        UserName = user.Email,
                        NormalizedUserName = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        FullName = user.FullName,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()

                    };
                    var result = await _userManager.CreateAsync(User, user.Password);

                    //var email = await _userManager.FindByEmailAsync(user.Email);
                    //if (email != null)
                    //{
                    //    throw new InvalidDataException(ErrorMessage.EXISTED);
                    //}
                    if (!result.Succeeded)
                    {
                        throw new Exception(ErrorMessage.INVALID);
                    }
                    var roleResult = await _userManager.AddToRolesAsync(User, user.Roles);
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(ErrorMessage.INVALID);
                    }
                    await _transactionRepository.CommitTransactionAsync();
                    return new UserDTO
                    {
                        Id = User.Id,
                        Email = User.Email,
                        PhoneNumber = User.PhoneNumber,
                        FullName = User.FullName,
                        Roles = user.Roles,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    };
                }
                catch (Exception ex)
                {
                    await _transactionRepository.RollbackTransactionAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            else throw new Exception($"ID {userId} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<UserDTO> UpdateUser(string userId, UserUpdateDTO user)
        {
            using (await _transactionRepository.BeginTransactionAsync())
            {
                try
                {
                    var existingUser = await _userManager.FindByIdAsync(userId);
                    if (existingUser != null)
                    {
                        existingUser.Email = user.Email;
                        existingUser.PhoneNumber = user.PhoneNumber;
                        existingUser.FullName = user.FullName;

                        //if (user.Password != null)
                        //{
                        //    existingUser.PasswordHash = _passageHasher.HashPassword(existingUser, user.Password);
                        //}

                        //var email = await _userManager.FindByEmailAsync(user.Email);
                        //if (email != null)
                        //{
                        //    throw new InvalidDataException(ErrorMessage.EXISTED);
                        //}

                        if (!string.IsNullOrEmpty(user.Password) && _passageHasher != null)
                        {
                            existingUser.PasswordHash = _passageHasher.HashPassword(existingUser, user.Password);
                        }

                        await _userManager.UpdateAsync(existingUser);

                        var currentRole = await _userManager.GetRolesAsync(existingUser);
                        var removeRole = await _userManager.RemoveFromRolesAsync(existingUser, currentRole);
                        if (!removeRole.Succeeded)
                        {
                            throw new Exception("Xóa role thất bại");
                        }

                        var addRole = await _userManager.AddToRolesAsync(existingUser, user.Roles);
                        if (!addRole.Succeeded)
                        {
                            throw new Exception("Thêm role không thành công!");
                        }

                        await _transactionRepository.CommitTransactionAsync();

                        return _mapper.Map<UserDTO>(existingUser);
                    }
                    else throw new Exception(ErrorMessage.NOT_FOUND);
                }
                catch (Exception ex)
                {
                    await _transactionRepository.RollbackTransactionAsync();
                    throw new Exception(ex.Message);
                }
            }
        }

        public async Task<UserDTO> GetUserId(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new Exception(ErrorMessage.NOT_FOUND);

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task LockOut(string id, DateTimeOffset? endDate)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user != null)
            {
                if(endDate != null)
                    user.LockoutEnd = endDate.Value.AddDays(1);
                else user.LockoutEnd = endDate;

                await _userManager.UpdateAsync(user);
            }
            else throw new Exception($"ID {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}