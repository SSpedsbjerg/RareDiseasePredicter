using RareDiseasePredicter.Controller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

internal class Program {
    private static void Main(string[] args) {

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();

        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy => {
                                  policy.WithOrigins("http://localhost:57694",
                                                      "http://localhost:57693");
                              });
        });

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