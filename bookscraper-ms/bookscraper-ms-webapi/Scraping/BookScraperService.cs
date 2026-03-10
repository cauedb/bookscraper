using OpenQA.Selenium;

namespace BookScraper.Scraping;

public class BookScraperService
{
    private static readonly Dictionary<string, List<Book>> _booksCache = new(StringComparer.OrdinalIgnoreCase);
    private static List<string>? _cachedCategories;

    private static readonly Dictionary<string, int> _ratingMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["One"] = 1, ["Two"] = 2, ["Three"] = 3, ["Four"] = 4, ["Five"] = 5
    };

    private readonly string _targetUrl;
    private readonly ILogger<BookScraperService> _logger;

    public BookScraperService(IConfiguration configuration, ILogger<BookScraperService> logger)
    {
        _targetUrl = configuration["ScraperSettings:TargetUrl"]
            ?? throw new InvalidOperationException("ScraperSettings:TargetUrl não configurado.");
        _logger = logger;
    }

    public Task<List<Book>> ScrapeAsync(string category)
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
        }

        var books = CollectAllBooksFromCurrentPage(driver);

        _booksCache[category] = books;
        _logger.LogInformation("Coleta concluída. {Count} livros armazenados para a categoria '{Category}'.", books.Count, category);
        return Task.FromResult(books);
    }

    public Task<List<string>> GetCategoriesAsync()
    {
        if (_cachedCategories is not null)
        {
            _logger.LogInformation("Retornando {Count} categorias do cache.", _cachedCategories.Count);
            return Task.FromResult(_cachedCategories);
        }

        _logger.LogInformation("Coletando categorias de {Url}", _targetUrl);

        using var driver = SeleniumDriverFactory.Create();
        driver.Navigate().GoToUrl(_targetUrl);

        var categoryElements = driver.FindElements(By.CssSelector("div.side_categories ul li a"));

        var categories = categoryElements
            .Select(el => el.Text.Trim())
            .Where(text => !string.IsNullOrEmpty(text))
            .ToList();

        categories.Insert(0, "All");

        _cachedCategories = categories;
        _logger.LogInformation("{Count} categorias encontradas.", categories.Count);
        return Task.FromResult(_cachedCategories);
    }

    public List<Book> GetCachedBooks() =>
        _booksCache.Values.SelectMany(books => books).ToList();

    public List<Book> GetCachedBooksByCategory(string category)
    {
        if (!_booksCache.TryGetValue(category, out var books))
            throw new InvalidOperationException($"Nenhum resultado encontrado para a categoria '{category}'. Execute o scraping primeiro.");

        return books;
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

                var ratingClass = bookElement
                    .FindElement(By.CssSelector("p[class*='star-rating']"))
                    .GetAttribute("class") ?? string.Empty;

                var ratingText = ratingClass.Split(' ').Last();

                if (!_ratingMap.TryGetValue(ratingText, out var rating))
                    throw new InvalidOperationException($"Valor de rating inválido encontrado: '{ratingText}'");

                books.Add(new Book(title, price, availability, rating));
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
