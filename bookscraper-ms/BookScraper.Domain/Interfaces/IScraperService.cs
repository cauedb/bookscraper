using BookScraper.Domain.Entities;

namespace BookScraper.Domain.Interfaces;

public interface IScraperService
{
    Task<string> ScrapePageSourceAsync();
    Task<List<Book>> GetAllBooksFromCategoryAsync(string category);
    List<Book> GetCachedBooks();
    List<Book> GetCachedBooksByCategory(string category);
}
