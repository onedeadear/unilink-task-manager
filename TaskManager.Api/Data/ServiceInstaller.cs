namespace TaskManager.Api.Data
{
    public static class ServiceInstaller
    {
        public static IServiceCollection AddRepositoryServices(this IServiceCollection services)
        {
            // Probably overkill for this small project, but good practice if the project scales
            services.AddScoped<ITaskRepository, TaskRepository>();
            
            return services;
        }
    }
}
