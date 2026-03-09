namespace BookScraper.Domain.Interfaces;

public interface IScraperService
{
    Task<string> ScrapePageSourceAsync();
}
