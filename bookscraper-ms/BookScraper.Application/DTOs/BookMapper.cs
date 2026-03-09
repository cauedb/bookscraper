using BookScraper.Domain.Entities;

namespace BookScraper.Application.DTOs;

public static class BookMapper
{
    public static BookResponse ToResponse(this Book book) => new()
    {
        Title = book.Title,
        Price = book.Price,
        Availability = book.Availability,
        Rating = (int)book.Rating
    };

    public static List<BookResponse> ToResponseList(this IEnumerable<Book> books) =>
        books.Select(ToResponse).ToList();
}
