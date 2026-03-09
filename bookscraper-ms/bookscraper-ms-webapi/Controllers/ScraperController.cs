using BookScraper.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bookscraper_ms_webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class ScraperController : ControllerBase
{
    private readonly IScraperService _scraperService;
    private readonly ILogger<ScraperController> _logger;

    public ScraperController(IScraperService scraperService, ILogger<ScraperController> logger)
    {
        _scraperService = scraperService;
        _logger = logger;
    }

    [HttpPost("/scrape")]
    public async Task<IActionResult> Scrape([FromQuery] string category = "All")
    {
        _logger.LogInformation("Requisição POST /scrape recebida. Categoria: '{Category}'", category);

        try
        {
            var books = await _scraperService.GetAllBooksFromCategoryAsync(category);

            return Ok(new
            {
                message = "Scraping concluído com sucesso.",
                category,
                totalBooks = books.Count
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
