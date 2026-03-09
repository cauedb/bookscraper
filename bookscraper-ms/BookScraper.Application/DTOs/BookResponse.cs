namespace BookScraper.Application.DTOs;

public class BookResponse
{
    public string Title { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public bool Availability { get; set; }
    public int Rating { get; set; }
}
