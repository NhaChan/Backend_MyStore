using AutoMapper;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.ImageRepository;
using MyStore.Repository.ProductRepository;
using MyStore.Request;
using MyStore.Response;
using MyStore.Storage;

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

        public async Task<PagedResponse<ProductDTO>> GetAllProductAsync(int page, int pageSize, string? search)
        {
            try
            {
                int totalProduct;
                IEnumerable<Product> products;
                if (string.IsNullOrEmpty(search))
                {
                    totalProduct = await _productRepository.CountAsync();
                    products = await _productRepository.GetPageProductAsync(page, pageSize);
                }
                else
                {
                    totalProduct = await _productRepository.CountAsync(search);
                    products = await _productRepository.GetPageProductAsync(page, pageSize, search);
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

    }
}
