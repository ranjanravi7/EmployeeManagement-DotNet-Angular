using DbFirstCore.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace DbFirstCore.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Controllers
            builder.Services.AddControllers();

            // Allow CORS for local Angular dev server
            builder.Services.AddCors(options =>
            {
                // In Development allow all origins (convenience for local dev + ports)
                if (builder.Environment.IsDevelopment())
                {
                    options.AddPolicy("AllowAngularDev", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                }
                else
                {
                    options.AddPolicy("AllowAngularDev", policy =>
                    {
                        policy.WithOrigins("http://localhost:4200")
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                }
            });

            // Register DbContext and repository
            // Note: DataAccessLayer project contains AppDbContext and repositories
            builder.Services.AddDbContext<DbFirstCore.DataAccessLayer.AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<DbFirstCore.DataAccessLayer.Repositories.IEmployeeRepository, DbFirstCore.DataAccessLayer.Repositories.EmployeeRepository>();

            // Add Swagger
            builder.Services.AddEndpointsApiExplorer();
            // Register Swagger generator (no explicit OpenApiInfo type required)
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable Swagger and serve the UI at the application root (/)
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                // Serve Swagger UI at application root so visiting https://localhost:7066/ shows the UI
                c.RoutePrefix = string.Empty;
                // Point the UI to the generated Swagger JSON
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DbFirstCore API v1");
            });

            // Enable CORS policy (used by Angular dev server)
            // Call UseCors early so CORS headers are applied to API endpoints.
            app.UseCors("AllowAngularDev");

            // Do not seed or create schema with hard-coded values here.
            // The API will read live data from the configured database via AppDbContext and repositories.
            // Optionally validate connectivity at startup and log a warning if the database is unavailable.
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DbFirstCore.DataAccessLayer.AppDbContext>();
                try
                {
                    // Diagnostic: print connection string and employee count so you can verify the API is using the expected database.
                    var conn = db.Database.GetDbConnection().ConnectionString;
                    int count = db.Employees.AsNoTracking().Count();
                    Console.WriteLine($"DB Connection: {conn}");
                    Console.WriteLine($"Employees table row count: {count}");

                    if (!db.Database.CanConnect())
                    {
                        Console.Error.WriteLine("Warning: Unable to connect to the database. Ensure connection string and database are correct.");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Database connectivity check failed: {ex.Message}");
                }
            }

            // HTTPS Redirection
            app.UseHttpsRedirection();

            // Serve static files (for production Angular build) if present
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Authorization
            app.UseAuthorization();

            // Map Controllers
            app.MapControllers();

            // SPA fallback: serve index.html for client-side routes when a physical file is not found
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}