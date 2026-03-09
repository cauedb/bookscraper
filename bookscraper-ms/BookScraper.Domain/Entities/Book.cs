using BookScraper.Domain.Enums;

namespace BookScraper.Domain.Entities;

public class Book
{
    public string Title { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public bool Availability { get; set; }
    public eRating Rating { get; set; }
}
