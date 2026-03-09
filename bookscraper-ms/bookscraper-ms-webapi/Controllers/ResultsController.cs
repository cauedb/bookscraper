using BookScraper.Application.DTOs;
using BookScraper.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bookscraper_ms_webapi.Controllers;

[ApiController]
[Route("results")]
public class ResultsController : ControllerBase
{
    private readonly IScraperService _scraperService;
    private readonly ILogger<ResultsController> _logger;

    public ResultsController(IScraperService scraperService, ILogger<ResultsController> logger)
    {
        _scraperService = scraperService;
        _logger = logger;
    }

    [HttpGet("latest")]
    public IActionResult GetLatest()
    {
        _logger.LogInformation("Requisição GET /results/latest recebida.");

        var books = _scraperService.GetCachedBooks();

        if (books.Count == 0)
            return NotFound(new { error = "Nenhum dado disponível. Execute POST /scrape primeiro." });

        return Ok(books.ToResponseList());
    }

    [HttpGet("latest-by-category/{category}")]
    public IActionResult GetLatestByCategory(string category)
    {
        _logger.LogInformation("Requisição GET /results/latest-by-category/{Category} recebida.", category);

        try
        {
            var books = _scraperService.GetCachedBooksByCategory(category);
            return Ok(books.ToResponseList());
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
