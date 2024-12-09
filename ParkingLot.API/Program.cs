using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ParkingLot.Infrastructure.Data;
using ParkingLot.Infrastructure.Services;
using ParkingLot.Core.Interfaces;
using ParkingLot.Core.Entities;
using Serilog;
using Serilog.Events;
using Prometheus;
using ParkingLot.API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Runtime.CompilerServices;
using ParkingLot.Application.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ParkingLot.Infrastructure.Repositories; // Add this

[assembly: InternalsVisibleTo("ParkingLot.IntegrationTests")]

public partial class Program
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        ConfigureServices(builder);
        
        var app = builder.Build();
        
        // Configure the HTTP request pipeline.
        ConfigureMiddleware(app);
        
        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        // Move all service configuration here
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/parkinglot-.txt", rollingInterval: RollingInterval.Day);
        });

        // Add services to the container.
        builder.Services.AddControllers();

        // Add AutoMapper configuration - Thêm đoạn này sau builder.Services.AddControllers();
        builder.Services.AddAutoMapper(typeof(Program).Assembly, 
            typeof(ParkingLot.Application.DTOs.ParkingLotDto).Assembly);

        builder.Services.AddEndpointsApiExplorer();

        // Thay thế phần cấu hình Swagger cũ bằng cấu hình mới
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "ParkingLot API", 
                Version = "v1",
                Description = "API for managing parking lots",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@parkinglot.com"
                }
            });

            // Enable XML comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Security definition (existing code)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Update CORS configuration
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.SetIsOriginAllowedToAllowWildcardSubdomains()
                       .SetIsOriginAllowed(_ => true)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });

        // Add Goong Map configuration
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IGoongMapService, GoongMapService>();

        // Add DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add Identity with default configuration
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
        {
            // Password settings based on best practices
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Set to true if email confirmation is needed
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? 
            throw new InvalidOperationException("JWT SecretKey is not configured"));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero,  // Loại bỏ thời gian chênh lệch
                NameClaimType = ClaimTypes.Name,                 // Thêm mapping cho claims
                RoleClaimType = ClaimTypes.Role                  // Thêm mapping cho roles
            };
            // Add events for debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var user = context.Principal;
                    logger.LogInformation($"Token validated for user: {user?.Identity?.Name}");
                    var roles = user?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
                    logger.LogInformation($"User roles: {string.Join(", ", roles)}");
                    return Task.CompletedTask;
                }
            };
        });

        // Add Authorization with default policy
        builder.Services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add application services
        builder.Services.AddApplicationServices();

        // Add Application Services
        builder.Services.AddScoped<IParkingLotService, ParkingLotService>();

        // Fix the repository registration
        builder.Services.AddScoped<IRepository<ParkingLot.Core.Entities.ParkingLot>, Repository<ParkingLot.Core.Entities.ParkingLot>>();
    }

    private static void ConfigureMiddleware(WebApplication app)
    {
        // Move all middleware configuration here
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Important: UseCors must be called before UseRouting and authentication middleware
        app.UseCors();
        
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        // Thêm middleware để log request
        app.UseSerilogRequestLogging();

        // Thêm middleware cho metrics
        app.UseMetricServer();
        app.UseHttpMetrics();

        app.MapControllers();
    }

    public static async Task Main(string[] args)
    {
        var app = CreateApp(args);
        await app.RunAsync();
    }
}
