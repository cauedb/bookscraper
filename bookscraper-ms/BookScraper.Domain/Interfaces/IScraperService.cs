using BookScraper.Domain.Entities;

namespace BookScraper.Domain.Interfaces;

public interface IScraperService
{
    Task<string> ScrapePageSourceAsync();
    Task<List<Book>> GetAllBooksFromCategoryAsync(string category);
}
