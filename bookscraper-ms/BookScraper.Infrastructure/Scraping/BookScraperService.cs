using BookScraper.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookScraper.Infrastructure.Scraping;

public class BookScraperService : IScraperService
{
    private static string? _cachedPageSource;

    private readonly string _targetUrl;
    private readonly ILogger<BookScraperService> _logger;

    public BookScraperService(IConfiguration configuration, ILogger<BookScraperService> logger)
    {
        _targetUrl = configuration["ScraperSettings:TargetUrl"]
            ?? throw new InvalidOperationException("ScraperSettings:TargetUrl não configurado.");
        _logger = logger;
    }

    public Task<string> ScrapePageSourceAsync()
    {
        if (_cachedPageSource is not null)
        {
            _logger.LogInformation("Retornando página do cache.");
            return Task.FromResult(_cachedPageSource);
        }

        _logger.LogInformation("Iniciando scraping de {Url}", _targetUrl);

        using var driver = SeleniumDriverFactory.Create();

        driver.Navigate().GoToUrl(_targetUrl);
        _cachedPageSource = driver.PageSource;

        _logger.LogInformation("Scraping concluído. HTML armazenado em memória.");

        return Task.FromResult(_cachedPageSource);
    }
}
