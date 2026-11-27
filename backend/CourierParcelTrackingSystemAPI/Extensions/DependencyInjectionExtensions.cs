using CourierParcelTrackingSystem.API.Entities;
using CourierParcelTrackingSystem.API.Repositories;
using CourierParcelTrackingSystem.API.Repositories.Implementations;
using CourierParcelTrackingSystem.API.Repositories.Interfaces;
using CourierParcelTrackingSystem.API.Services;
using CourierParcelTrackingSystem.API.Services.Implementations;
using CourierParcelTrackingSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CourierParcelTrackingSystem.API.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IHubRepository, HubRepository>();
        services.AddScoped<IParcelStatusRepository, ParcelStatusRepository>();
        services.AddScoped<IParcelRepository, ParcelRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IParcelBookingRepository, ParcelBookingRepository>();
        services.AddScoped<IParcelTrackingRepository, ParcelTrackingRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IReportsRepository, ReportsRepository>();
        services.AddScoped<ICourierRepository, CourierRepository>(); 
        services.AddScoped<IParcelAssignmentRepository, ParcelAssignmentRepository>();
        services.AddScoped<IScreenAssignmentRepository, ScreenAssignmentRepository>();
        services.AddScoped<IParcelDelayPredictionRepository, ParcelDelayPredictionRepository>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IHubService, HubService>();
        services.AddScoped<IParcelStatusService, ParcelStatusService>();
        services.AddScoped<IParcelService, ParcelService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRouteService, RouteService>();
        services.AddScoped<IParcelBookingService, ParcelBookingService>();
        services.AddScoped<IParcelTrackingService, ParcelTrackingService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ICustomerDashboardService, CustomerDashboardService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<ICourierService, CourierService>();
        services.AddScoped<IParcelAssignmentService, ParcelAssignmentService>();
        services.AddScoped<IScreenAssignmentService, ScreenAssignmentService>();
        services.AddScoped<IParcelDelayPredictionService, ParcelDelayPredictionService>();

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        return services;
    }
}
