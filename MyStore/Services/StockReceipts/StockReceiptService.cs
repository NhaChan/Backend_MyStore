using AutoMapper;
using MyStore.Constant;
using MyStore.DTO;
using MyStore.Models;
using MyStore.Repository.ProductRepository;
using MyStore.Repository.StockReceiptRepository;
using MyStore.Request;
using MyStore.Response;
using System.Globalization;
using System.Linq.Expressions;

namespace MyStore.Services.StockReceipts
{
    public class StockReceiptService : IStockReceiptService
    {
        private readonly IStockReceiptRepository _stockReceiptRepository;
        private readonly IStockReceiptDetailRepository _stockReceiptDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public StockReceiptService(IStockReceiptRepository stockReceiptRepository, IStockReceiptDetailRepository stockReceiptDetailRepository, IProductRepository productRepository, IMapper mapper)
        {
            _stockReceiptRepository = stockReceiptRepository;
            _stockReceiptDetailRepository = stockReceiptDetailRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<StockReceiptDTO> CreateStockReceipt(string userId, StockReceiptRequest request)
        {
            var recceipt = new StockReceipt
            {
                UserId = userId,
                Note = request.Note,
                Total = request.Total,
                EntryDate = request.EntryDate
            };
            await _stockReceiptRepository.AddAsync(recceipt);

            var listProductUpdate = new List<Product>();
            var listStockReceiptDetail = new List<StockReceiptDetail>();

            foreach (var item in request.StockReceiptProducts)
            {
                var product = await _productRepository
                    .SingleOrDefaultAsync(e => e.Id == item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                    listProductUpdate.Add(product);
                }
                var stockReceiptDetail = new StockReceiptDetail
                {
                    StockReceiptId = recceipt.Id,
                    ProductId = item.ProductId,

                    Quantity = item.Quantity,
                    OriginPrice = item.OriginPrice,
                };
                listStockReceiptDetail.Add(stockReceiptDetail);
            }
            await _productRepository.UpdateAsync(listProductUpdate);
            await _stockReceiptDetailRepository.AddAsync(listStockReceiptDetail);

            return _mapper.Map<StockReceiptDTO>(recceipt);
        }

        public async Task<PagedResponse<StockReceiptDTO>> GetAllStock(int page, int pageSize, string? search)
        {
            int totalReceipt;
            IEnumerable<StockReceipt> stockReceipts;

            if (string.IsNullOrEmpty(search))
            {
                totalReceipt = await _stockReceiptRepository.CountAsync();
                stockReceipts = await _stockReceiptRepository.GetPageOrderByDescendingAsync(page, pageSize, null, x => x.CreatedAt);
            }
            else
            {
                bool isLong = long.TryParse(search, out long isSearch);
                DateTime dateSearch;
                bool isDate = DateTime.TryParseExact(
                    search,
                    "HH:mm:ss dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out dateSearch);

                Expression<Func<StockReceipt, bool>> expression = e => e.Id.Equals(isSearch)
                    || (!isLong && isDate && e.CreatedAt.Date == dateSearch.Date);

                totalReceipt = await _stockReceiptRepository.CountAsync(expression);
                stockReceipts = await _stockReceiptRepository.GetPageOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }

            var items = _mapper.Map<IEnumerable<StockReceiptDTO>>(stockReceipts);
            return new PagedResponse<StockReceiptDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalReceipt,
            };
        }

        public async Task<IEnumerable<StockReceiptDetailResponse>> GetStockDetails(long stockId)
        {
            var stock = await _stockReceiptDetailRepository.GetAsync(e => e.StockReceiptId == stockId);
            if (stock != null)
            {
                return _mapper.Map<IEnumerable<StockReceiptDetailResponse>>(stock);
            }
            throw new InvalidOperationException(ErrorMessage.NOT_FOUND);
        }

        public async Task<ExpenseReponse> GetExpenseDate(DateTime from, DateTime to)
        {
            var result = await _stockReceiptRepository.GetAsync(e => e.EntryDate >= from && e.EntryDate <= to.AddDays(1));

            var expenseList = result.Select(e => new StockReceiptDTO
            {
                Id = e.Id,
                UserName = e.User?.UserName ?? "",
                Note = e.Note,
                Total = e.Total,
                EntryDate = e.EntryDate,
                CreatedAt = e.CreatedAt
            }).ToList();

            var totalExpense = expenseList.Sum(e => e.Total);

            return new ExpenseReponse
            {
                ExpenseList = expenseList,
                Total = totalExpense,
            };
        }

        public async Task<ExpenseYearMonthReponse> GetExpenseYearMonth(int year, int? month)
        {
            var result = await _stockReceiptRepository.GetExpenseByMonthYear(year, month);

            return new ExpenseYearMonthReponse
            {
                ExpenseList = result,
                Total = result.Sum(e => e.Total),
            };
        }

        public async Task<ExpenseByProductReponse> GetProductExpenseYear(int productId, int year, int? month)
        {
            var result = await _stockReceiptRepository.GetStatisticProductExpenseByYear(productId, year, month);

            return new ExpenseByProductReponse
            {
                ExpenseListProduct = result,
                Total = result.Sum(e => e.Total),
            };
        }

        public async Task<ExpenseByProductReponse> GetProductExpenseDate(int productId, DateTime from, DateTime to)
        {
            var result = await _stockReceiptRepository.GetStatisticProductExpenseByDate(productId, from, to);

            return new ExpenseByProductReponse
            {
                ExpenseListProduct = result,
                Total = result.Sum(e => e.Total),
            };
        }
    }
}
