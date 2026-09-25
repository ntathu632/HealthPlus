using FluentValidation;
using HealthPlus.Application.Interfaces;
using HealthPlus.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HealthPlus.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IHealthRecordService, HealthRecordService>();
        services.AddScoped<IMedicalHistoryService, MedicalHistoryService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IVaccineService, VaccineService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IHospitalService, HospitalService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAiChatService, AiChatService>();

        return services;
    }
}
