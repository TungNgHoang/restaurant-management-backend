using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.AutoMapperProfile;
using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Interfaces;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Core.ApiModels;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RestaurantManagement.DataAccess.Infrastructure;
using RestaurantManagement.Api.Middlewares;
using RestaurantManagement.DataAccess.DbContexts;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi.Models;
using RestaurantManagement.DataAccess.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection?.Get<AppSettings>();
builder.Services.AddSingleton(appSettings ?? new AppSettings());

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddDbContext<RestaurantDBContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRepository<TblOrderInfo>, Repository<TblOrderInfo>>();
builder.Services.AddScoped<IRepository<TblOrderDetail>, Repository<TblOrderDetail>>();
builder.Services.AddScoped<IRepository<TblMenu>, Repository<TblMenu>>();
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IRepository<TblTableInfo>, Repository<TblTableInfo>>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRepository<TblPayment>, Repository<TblPayment>>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
////Addcors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Restaurent API", Version = "v1", Description = $"Last updated at {DateTimeOffset.UtcNow}" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
builder.Services.AddAutoMapper(typeof(ProjectProfile));
// Lấy key từ cấu hình
var jwtSettings = builder.Configuration.GetSection("AppSettings:Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<RestaurantDBContext>();
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Kiểm tra token có tồn tại trong database không
                var isValid = await dbContext.TblBlackListTokens.AnyAsync(t => t.Token == token);
                if (isValid)
                {
                    context.Fail("Token is invalid.");
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//Lấy thông tin từ app.JSON
builder.Configuration
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();

//app.UseStaticFiles();
//Khai báo DataSeeder
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedDataAsync(scope.ServiceProvider);
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
//testcheckoutbytung
