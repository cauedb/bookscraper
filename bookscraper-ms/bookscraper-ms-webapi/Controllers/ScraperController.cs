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
    public async Task<IActionResult> Scrape()
    {
        _logger.LogInformation("Requisição POST /scrape recebida.");

        var books = await _scraperService.GetAllBooksFromCategoryAsync();

        return Ok(new
        {
            message = "Scraping concluído com sucesso.",
            totalBooks = books.Count
        });
    }
}
