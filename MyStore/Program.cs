using MyStore.Data;
using MyStore.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyStore.Services.Users;
using MyStore.Repository.Users;
using MyStore.Services.SendMail;
using MyStore.Services.Auth;
using MyStore.Services.Caching;
using MyStore.Services.Brands;
using MyStore.Services.Categories;
using MyStore.Repository.CategoryRepository;
using MyStore.Mapping;
using MyStore.Storage;
using MyStore.Repository.BrandRepository;
using MyStore.Services.Products;
using MyStore.Repository.ProductRepository;
using MyStore.Repository.ImageRepository;
using MyStore.Repository.CartItemRepository;
using MyStore.Services.Carts;
using MyStore.DataSeeding;
using MyStore.Services.Payments;
using MyStore.Services.Orders;
using MyStore.Repository.OrderRepository;
using MyStore.Constant;
using Net.payOS;
using MyStore.Repository.StockReceiptRepository;
using MyStore.Services.StockReceipts;
using MyStore.Services.Expenses;
using MyStore.Services.Statistics;
using MyStore.Services.Reviews;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CompanyDBContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<CompanyDBContext>()
    .AddTokenProvider("MyStore", typeof(DataProtectorTokenProvider<User>));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICachingService, CachingService>();
PayOS payOS = new(builder.Configuration["PayOS:clientId"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL),
                        builder.Configuration["PayOS:apiKey"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL),
                        builder.Configuration["PayOS:checksumKey"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL));
builder.Services.AddSingleton(payOS);

builder.Services.AddScoped<IFileStorage, FileStorage>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartItemService, CartItemService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IStockReceiptService, StockReceiptService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IDeliveryAdressRepository, DeliveryAddressRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository,  OrderDetailRepository>();
builder.Services.AddScoped<IProductFavoriteRepository, ProductFavoriteRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IStockReceiptRepository, StockReceiptRepository>();
builder.Services.AddScoped<IStockReceiptDetailRepository, StockReceiptDetailRepository>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddSingleton<ISendMailService, SendMailService>();


builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidAudience = builder.Configuration.GetSection("JWT:Audience").Value,
        ValidIssuer = builder.Configuration.GetSection("JWT:Issuer").Value,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JWT:Key").Value ?? ""))
    };
});

builder.Services.AddAutoMapper(typeof(Mapping));

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("MyCors", opt =>
    {
        opt.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});


var app = builder.Build();

DataSeeding.Initialize(app.Services).Wait();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MyCors");
app.UseAuthentication();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();
