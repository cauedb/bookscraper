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
    public IActionResult GetLatest([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Requisição GET /results/latest recebida. Page={Page}, PageSize={PageSize}", page, pageSize);

        var books = _scraperService.GetCachedBooks();

        if (books.Count == 0)
            return NotFound(new { error = "Nenhum dado disponível. Execute POST /scrape primeiro." });

        var items = books.Skip((page - 1) * pageSize).Take(pageSize).ToResponseList();

        return Ok(new PagedResult<BookResponse>
        {
            Items = items,
            TotalCount = books.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("latest-by-category/{category}")]
    public IActionResult GetLatestByCategory(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        _logger.LogInformation("Requisição GET /results/latest-by-category/{Category} recebida. Page={Page}, PageSize={PageSize}", category, page, pageSize);

        try
        {
            var books = _scraperService.GetCachedBooksByCategory(category);
            var items = books.Skip((page - 1) * pageSize).Take(pageSize).ToResponseList();

            return Ok(new PagedResult<BookResponse>
            {
                Items = items,
                TotalCount = books.Count,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
