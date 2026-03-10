using BookScraper.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace bookscraper_ms_webapi.Controllers;

[ApiController]
[Route("categories")]
public class CategoriesController : ControllerBase
{
    private readonly IScraperService _scraperService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IScraperService scraperService, ILogger<CategoriesController> logger)
    {
        _scraperService = scraperService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        _logger.LogInformation("Requisição GET /categories recebida.");

        try
        {
            var categories = await _scraperService.GetCategoriesAsync();
            return Ok(categories);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Erro ao coletar categorias.");
            return StatusCode(503, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao coletar categorias.");
            return StatusCode(500, new { error = "Erro interno ao coletar categorias." });
        }
    }
}
