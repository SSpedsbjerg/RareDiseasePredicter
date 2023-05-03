using RareDiseasePredicter.Controller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using RareDiseasePredicter;

internal class Program {
    private static void Main(string[] args) {

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();

        var MyAllowSpecificOrigins = "AllowCore";

        bool databaseSuccess = DatabaseController.Start();
        if (!databaseSuccess) {
            _ = Log.Error(new Exception("Could not start database"), "Program", "Unknow reason for not being able to start database");
            Console.WriteLine("Could not start database");
            return;
            }

        builder.Services.AddCors(options =>
        {
            /*options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy => {
                                  policy.WithOrigins("http://83.92.23.39/:3000", "http://83.92.23.39/:8080").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                              });*/
            options.AddPolicy(name: MyAllowSpecificOrigins,
                policy => {
                    policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(origin => true);
                });
            
        });


        builder.Services.AddControllers().AddNewtonsoftJson();
        
        // Add services to the container.

        builder.Services.AddControllers();

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