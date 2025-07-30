using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using RestaurantManagement.Api.AutoMapperProfile;
using RestaurantManagement.Api.Middlewares;
using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.DataAccess.Infrastructure;
using System.Text.Json.Serialization;

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
builder.Services.AddScoped<ITableService, RestaurantManagement.Service.Implementation.TableService>();
builder.Services.AddScoped<IRepository<TblTableInfo>, Repository<TblTableInfo>>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRepository<TblPayment>, Repository<TblPayment>>();
builder.Services.AddScoped<IInvoiceService, RestaurantManagement.Service.Implementation.InvoiceService>();
builder.Services.AddScoped<IStatisticService, RestaurantManagement.Service.Implementation.StatisticService>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPromotionRepository, PromotionRepository>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IStaffService, StaffService>();
////Addcors
var allowedFrontendOrigin = "https://pizzadaay.ric.vn";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(allowedFrontendOrigin)
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Nếu dùng cookie hoặc JWT Auth
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
builder.Services.AddHttpContextAccessor();
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            RoleClaimType = ClaimTypes.Role
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

builder.Services.AddAuthorizationBuilder()

    // 1. Public
    .AddPolicy("PublicAccess", policy =>
    {
        policy.RequireAssertion(_ => true); // Ai cũng truy cập
    })

    // 2. Customer
    .AddPolicy("CustomerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("User");
    })

    // 3. Staff chung (gồm tất cả nhân viên)
    .AddPolicy("StaffPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Manager", "Receptionist", "Waiter", "Cashier");
    })

    // 4. Từng nhóm nhân viên cụ thể
    .AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    })

    .AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Manager");
    })

    .AddPolicy("AdminOrManagerPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin", "Manager");
    })

    .AddPolicy("ReceptionistPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Receptionist");
    })

    .AddPolicy("WaiterPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Waiter");
    })

    .AddPolicy("CashierPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Cashier");
    })

    // 5. Nhóm ghép đặc biệt (ví dụ cho thanh toán)
    .AddPolicy("BillingPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Cashier", "Receptionist");
    });




builder.Services.AddControllers();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts(); // Tăng bảo mật HTTP
}
//Lấy thông tin từ app.JSON
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Luôn load file gốc
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true) // Ghi đè theo môi trường
    .AddEnvironmentVariables();


//app.UseStaticFiles();
//Khai báo DataSeeder
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedDataAsync(scope.ServiceProvider);
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors(MyAllowSpecificOrigins); // PHẢI trước Auth

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

//testcheckoutbytung
