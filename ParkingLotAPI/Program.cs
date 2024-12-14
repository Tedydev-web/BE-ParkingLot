using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParkingLotAPI.Data;
using ParkingLotAPI.Mappings;
using ParkingLotAPI.Services;
using ParkingLotAPI.Models;
using System.Text.Json.Serialization;
using ParkingLotAPI.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Parking Lot API", 
        Version = "v1",
        Description = "API for managing parking lots"
    });
});

// Thêm vào phần đăng ký services
builder.Services.AddScoped<IParkingLotService, ParkingLotService>(); 
builder.Services.AddScoped<IGoongMapService, GoongMapService>();

// Thêm vào phần ConfigureServices
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApp",
        builder => builder
            .WithOrigins("http://localhost:3000") // URL của Client
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

builder.Services.AddHttpClient();

// Thêm cấu hình Goong API
builder.Services.Configure<GoongApiOptions>(
    builder.Configuration.GetSection("GoongApi")
);

// Cấu hình HttpClient cho Goong API
builder.Services.AddHttpClient<IGoongMapService, GoongMapService>();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parking Lot API V1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();
