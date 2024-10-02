using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Encodings;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Enumerations;
using MyStore.Models;
using MyStore.Repository.ImageRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Storage;
using System.Linq.Expressions;

namespace MyStore.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageRepository _imageRepository;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private readonly string path = "assets/images/products";

        public ProductService(IProductRepository productRepository, IImageRepository imageRepository, IMapper mapper, IFileStorage fileStorage)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _mapper = mapper;
            _fileStorage = fileStorage;
        }

        public async Task<ProductDTO> CreatedProductAsync(ProductRequest request, IFormFileCollection images)
        {
            try 
            {
                var product = _mapper.Map<Product>(request);


                await _productRepository.AddAsync(product);

                IList<string> fileNames = new List<string>();
                var imgs = images.Select(file =>
                {
                    var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    fileNames.Add(name);
                    var image = new Image()
                    {
                        ProductId = product.Id,
                        ImageUrl = Path.Combine(path, name)
                    };
                    return image;
                });
                await _imageRepository.AddAsync(imgs);
                await _fileStorage.SaveAsync(path, images, fileNames);

                var res = _mapper.Map<ProductDTO>(product);
                var image = await _imageRepository.GetFirstImageByProductAsync(product.Id);
                if (image != null)
                {
                    res.ImageUrl = image.ImageUrl;
                }
                return res;
            } catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.FindAsync(id);
            if(product != null)
            {
                var images = await _imageRepository.GetImageProductAsync(id);
                _fileStorage.Delete(images.Select(e => e.ImageUrl));

                await _productRepository.DeleteAsync(product);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<PagedResponse<ProductDTO>> GetAllProductAsync(int page, int pageSize, string? search)
        {
            try
            {
                int totalProduct;
                IEnumerable<Product> products;
                if (string.IsNullOrEmpty(search))
                {
                    totalProduct = await _productRepository.CountAsync();
                    products = await _productRepository.GetPageOrderByDescendingAsync(page, pageSize, null, e => e.CreatedAt);
                }
                else
                {
                    Expression<Func<Product, bool>> expression = e => 
                        e.Name.Contains(search)
                        || e.Sold.ToString().Equals(search)
                        || e.Price.ToString().Equals(search);
                    totalProduct = await _productRepository.CountAsync(expression);
                    products = await _productRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
                }
                var res = _mapper.Map<IEnumerable<ProductDTO>>(products);
                foreach (var product in res)
                {
                    var image = await _imageRepository.GetFirstImageByProductAsync(product.Id);
                    if (image != null)
                    {
                        product.ImageUrl = image.ImageUrl;
                    }
                }
                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalProduct
                };
            } catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        private Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = expr1.Parameters[0];
            var body = Expression.AndAlso(expr1.Body, Expression.Invoke(expr2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public async Task<PagedResponse<ProductDTO>> GetFilterProductsAsync(Filters filters)
        {
            try
            {
                int totalProduct = 0;
                IEnumerable<Product> products = [];
                Expression<Func<Product, bool>> expression = e => e.Enable;

                Expression<Func<Product, double>> priceExp = e => e.Price - (e.Price * (e.Discount / 100 ));

                if(filters.Sorter > Enum.GetNames(typeof(SortEnum)).Length - 1)
                {
                    throw new ArgumentException(ErrorMessage.INVALID);
                }

                if(filters.MinPrice != null)
                {
                    expression = CombineExpressions(expression, e => (e.Price - (e.Price * (e.Discount / 100))) >= filters.MinPrice);
                }
                if (filters.MaxPrice != null)
                {
                    expression = CombineExpressions(expression, e => (e.Price - (e.Price * (e.Discount / 100))) <= filters.MaxPrice);
                }
                if(filters.Discount != null && filters.Discount == true)
                {
                    expression = CombineExpressions(expression, e => e.Discount > 0);
                }
                if(filters.CategoryIds != null && filters.CategoryIds.Count()>0)
                {
                    expression = CombineExpressions(expression, e => filters.CategoryIds.Contains(e.CategoryId));
                }
                if (filters.BrandIds != null && filters.BrandIds.Count() > 0)
                {
                    expression = CombineExpressions(expression, e => filters.BrandIds.Contains(e.BrandId));
                }

                //Đánh giá...Rating????

                totalProduct = await _productRepository.CountAsync(expression);

                var sorter = (SortEnum) filters.Sorter;

                switch(sorter)
                {
                    case SortEnum.SOLD:
                        products = await _productRepository
                            .GetPageOrderByDescendingAsync(filters.page, filters.pageSize, expression, e => e.Sold);
                        break;
                    case SortEnum.PRICE_ASC:
                        products = await _productRepository
                            .GetPagedAsync(filters.page, filters.pageSize, expression, priceExp);
                        break;
                    case SortEnum.PRICE_DESC:
                        products = await _productRepository
                            .GetPageOrderByDescendingAsync(filters.page, filters.pageSize, expression, priceExp);
                        break;
                    case SortEnum.NEWEST:
                        products = await _productRepository
                            .GetPageOrderByDescendingAsync(filters.page, filters.pageSize, expression, e => e.CreatedAt);
                        break;
                    default:
                        products = await _productRepository
                            .GetPageOrderByDescendingAsync(filters.page, filters.pageSize, expression, e => e.CreatedAt);
                        break;
                }

                var res = _mapper.Map<IEnumerable<ProductDTO>>(products).ToList();

                foreach (var product in res)
                {
                    var image = await _imageRepository.GetFirstImageByProductAsync(product.Id);
                    if(image != null)
                    {
                        product.ImageUrl = image.ImageUrl;
                    }
                }

                return new PagedResponse<ProductDTO>
                {
                    Items = res,
                    Page = filters.page,
                    PageSize = filters.pageSize,
                    TotalItems = totalProduct
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<ProductDetailResponse> GetProductById(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product != null)
            {
                var res = _mapper.Map<ProductDetailResponse>(product);
                res.ImageUrls = product.Images.Select(x => x.ImageUrl);
                return res;
            }
            else throw new ArgumentException($"Id {id}" + ErrorMessage.NOT_FOUND);
        }

        public async Task<ProductDTO> UpdateProduct(int id, ProductRequest request, IFormFileCollection images)
        {
            var product = await _productRepository.FindAsync(id);
            if (product != null)
            {
                try
                {
                    product.Name = request.Name;
                    product.Price = request.Price;
                    product.Quantity = request.Quantity;
                    product.Discount = request.Discount;
                    product.Description = request.Description;

                    var oldImgs = await _imageRepository.GetImageProductAsync(id);
                    List<Image> imageDelete = new();
                    if (request.ImageUrls.IsNullOrEmpty())
                    {
                        imageDelete.AddRange(oldImgs);
                    }
                    else
                    {
                        var imgsDelete = oldImgs.Where(old => !request.ImageUrls.Contains(old.ImageUrl));
                        imageDelete.AddRange(imgsDelete);
                    }
                    _fileStorage.Delete(imageDelete.Select(e => e.ImageUrl));
                    await _imageRepository.DeleteAsync(imageDelete);

                    if (images.Count > 0)
                    {
                        List<string> fileNames = new();
                        var imgs = images.Select(file =>
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            fileNames.Add(name);
                            var image = new Image()
                            {
                                ProductId = product.Id,
                                ImageUrl = Path.Combine(path, name)
                            };
                            return image;
                        });
                        await _imageRepository.AddAsync(imgs);
                        await _fileStorage.SaveAsync(path, images, fileNames);
                    }
                    await _productRepository.UpdateAsync(product);
                    return _mapper.Map<ProductDTO>(product);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.InnerException?.Message ?? ex.Message);
                }
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<bool> UpdateProductEnableAsync(int id, UpdateEnableRequest request)
        {
            var product = await _productRepository.FindAsync(id);
            if (product != null)
            {
                product.Enable = request.Enable;
                await _productRepository.UpdateAsync(product);
                return product.Enable;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
