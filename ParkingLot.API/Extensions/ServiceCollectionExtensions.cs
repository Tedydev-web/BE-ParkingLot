using Microsoft.Extensions.DependencyInjection;
using ParkingLot.Application.Services;
using ParkingLot.Core.Interfaces;
using AutoMapper;

namespace ParkingLot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add AutoMapper
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            
            services.AddScoped<IParkingLotService, ParkingLotService>();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            
            return services;
        }
    }
}
