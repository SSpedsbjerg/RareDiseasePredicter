using RareDiseasePredicter.Controller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using RareDiseasePredicter;
using System.Configuration;
using DotNetEnv;

internal class Program {
    private static async Task Main(string[] args) {

        Console.WriteLine("Database Username:");
        DatabaseController.UserName = Console.ReadLine();
        Console.WriteLine("Database Password:");
        DatabaseController.Password = Console.ReadLine();
        
        Console.WriteLine("Starting...");

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();

        var MyAllowSpecificOrigins = "AllowCore";

        bool databaseSuccess = await DatabaseController.Start();
        if (!databaseSuccess) {
            _ = Log.Error(new Exception("Could not start database"), "Program", "Unknow reason for not being able to start database");
            Console.WriteLine("Could not start database");
            return;
            }
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy => {
                    policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(origin => true);
                });
            
        });


        builder.Services.AddControllers().AddNewtonsoftJson();

        // Add services to the container.

        builder.Services.AddControllers();

        Env.Load();

        builder.Configuration.AddEnvironmentVariables();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseCors(MyAllowSpecificOrigins);

        app.UseAuthorization();

        app.MapControllers();

        app.MapControllers();
        Console.WriteLine(app.MapControllers());
        app.Run();
    }
}