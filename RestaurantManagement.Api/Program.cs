using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.AutoMapperProfile;
using RestaurantManagement.Api.Models;
using RestaurantManagement.DataAccess.Implementation;
using RestaurantManagement.DataAccess.Interfaces;
using RestaurantManagement.Service.Interfaces;
using RestaurantManagement.Service.Implementation;
using RestaurantManagement.Core.ApiModels;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings = appSettingsSection?.Get<AppSettings>();
builder.Services.AddSingleton(appSettings ?? new AppSettings());

builder.Services.AddControllers();//.AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddDbContext<RestaurantDBContext>(
        options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IReservationService, ReservationService>();
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
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(ProjectProfile));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseStaticFiles();


//app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
//testcheckoutbytung