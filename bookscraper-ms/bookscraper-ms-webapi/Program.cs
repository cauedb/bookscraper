using BookScraper;
using BookScraper.Scraping;
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

    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000", "http://localhost:5173"];

    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

    builder.Services.AddSingleton<BookScraperService>();
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
    {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new { error = "Erro interno do servidor." });
    }));

    app.UseCors();
    app.MapHealthChecks("/health");

    app.MapGet("/categories", async (BookScraperService scraper, ILogger<Program> logger) =>
    {
        logger.LogInformation("GET /categories");
        try
        {
            var categories = await scraper.GetCategoriesAsync();
            return Results.Ok(categories);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Erro ao carregar categorias.");
            return Results.Json(new { error = ex.Message }, statusCode: 503);
        }
    });

    app.MapPost("/scrape", async (BookScraperService scraper, ILogger<Program> logger, string category = "All") =>
    {
        logger.LogInformation("POST /scrape. Categoria: '{Category}'", category);
        try
        {
            var books = await scraper.ScrapeAsync(category);
            return Results.Ok(new { message = "Scraping concluído com sucesso.", category, totalBooks = books.Count });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Categoria inválida: '{Category}'", category);
            return Results.Json(new { error = ex.Message }, statusCode: 404);
        }
    });

    app.MapGet("/results/latest", (BookScraperService scraper, int page = 1, int pageSize = 20) =>
    {
        var books = scraper.GetCachedBooks();
        return Results.Ok(Paginate(books, page, pageSize));
    });

    app.MapGet("/results/latest-by-category/{category}", (BookScraperService scraper, ILogger<Program> logger, string category, int page = 1, int pageSize = 20) =>
    {
        try
        {
            var books = scraper.GetCachedBooksByCategory(category);
            return Results.Ok(Paginate(books, page, pageSize));
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Categoria não encontrada no cache: '{Category}'", category);
            return Results.Json(new { error = ex.Message }, statusCode: 404);
        }
    });

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

static object Paginate(List<Book> books, int page, int pageSize)
{
    var items = books
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return new { items, totalCount = books.Count, page, pageSize };
}
