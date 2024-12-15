using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParkingLotAPI.Data;
using ParkingLotAPI.Mappings;
using ParkingLotAPI.Services;
using ParkingLotAPI.Models;
using System.Text.Json.Serialization;
using ParkingLotAPI.Options;
using Microsoft.Extensions.FileProviders;

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

// Đăng ký services
builder.Services.AddScoped<IParkingLotService, ParkingLotService>(); 
builder.Services.AddScoped<IGoongMapService, GoongMapService>();

// Configure CORS
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

builder.Services.AddHttpClient();

// Cấu hình Goong API
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

var app = builder.Build();

// Tạo thư mục wwwroot nếu chưa tồn tại
var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

// Tạo thư mục uploads trong wwwroot
var uploadPath = Path.Combine(webRootPath, "uploads", "parkinglots");
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Parking Lot API V1");
    });
}

// Enable serving static files
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "-1");
    }
});

// Serve static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.Run();
