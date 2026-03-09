using BookScraper.Domain.Entities;
using BookScraper.Domain.Enums;
using BookScraper.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace BookScraper.Infrastructure.Scraping;

public class BookScraperService : IScraperService
{
    private static string? _cachedPageSource;
    private static readonly Dictionary<string, List<Book>> _booksCache = new(StringComparer.OrdinalIgnoreCase);

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

    public Task<List<Book>> GetAllBooksFromCategoryAsync(string category)
    {
        if (_booksCache.TryGetValue(category, out var cached))
        {
            _logger.LogInformation("Retornando {Count} livros da categoria '{Category}' do cache.", cached.Count, category);
            return Task.FromResult(cached);
        }

        _logger.LogInformation("Iniciando coleta de livros. Categoria: '{Category}'", category);

        using var driver = SeleniumDriverFactory.Create();
        driver.Navigate().GoToUrl(_targetUrl);

        if (!category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var categoryLinks = driver.FindElements(By.CssSelector("div.side_categories ul li a"));

            var categoryLink = categoryLinks.FirstOrDefault(el =>
                el.Text.Trim().Equals(category, StringComparison.OrdinalIgnoreCase));

            if (categoryLink is null)
                throw new InvalidOperationException($"Categoria '{category}' não encontrada.");

            categoryLink.Click();
            _logger.LogInformation("Navegando para a categoria '{Category}'.", category);
        }

        var books = CollectAllBooksFromCurrentPage(driver);

        _booksCache[category] = books;
        _logger.LogInformation("Coleta concluída. {Count} livros armazenados para a categoria '{Category}'.", books.Count, category);
        return Task.FromResult(books);
    }

    private List<Book> CollectAllBooksFromCurrentPage(IWebDriver driver)
    {
        var books = new List<Book>();
        var pageNumber = 1;

        while (true)
        {
            _logger.LogInformation("Coletando livros da página {Page}", pageNumber);

            var bookElements = driver.FindElements(By.CssSelector("ol.row li article.product_pod"));

            foreach (var bookElement in bookElements)
            {
                var title = bookElement
                    .FindElement(By.CssSelector("h3 > a"))
                    .GetAttribute("title") ?? string.Empty;

                var price = bookElement
                    .FindElement(By.CssSelector("p.price_color"))
                    .Text;

                var availabilityText = bookElement
                    .FindElement(By.CssSelector("p.availability"))
                    .Text.Trim();

                var availability = availabilityText.Contains("In stock", StringComparison.OrdinalIgnoreCase);

                var ratingAttribute = bookElement
                    .FindElement(By.CssSelector("p[class*='star-rating']"))
                    .GetAttribute("class") ?? string.Empty;

                var ratingText = ratingAttribute.Split(' ').Last();

                if (!Enum.TryParse<eRating>(ratingText, out var rating))
                    throw new InvalidOperationException($"Valor de rating inválido encontrado: '{ratingText}'");

                books.Add(new Book
                {
                    Title = title,
                    Price = price,
                    Availability = availability,
                    Rating = rating
                });
            }

            var nextLink = driver.FindElements(By.CssSelector("li.next a"));
            if (nextLink.Count == 0)
                break;

            var nextHref = nextLink[0].GetAttribute("href") ?? string.Empty;
            var nextUrl = new Uri(new Uri(driver.Url), nextHref).ToString();
            driver.Navigate().GoToUrl(nextUrl);
            pageNumber++;
        }

        return books;
    }
}
