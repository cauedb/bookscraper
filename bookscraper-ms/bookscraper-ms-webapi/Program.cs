using BookScraper.Domain.Interfaces;
using BookScraper.Infrastructure.Scraping;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services));

    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000", "http://localhost:5173"];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks();
    builder.Services.AddScoped<IScraperService, BookScraperService>();

    var app = builder.Build();

    app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new { error = "Erro interno do servidor." });
    }));

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
