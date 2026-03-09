import type { Book } from '../types/book'
import './BookTable.css'

interface Props {
  books: Book[]
  loading: boolean
}

function RatingStars({ rating }: { rating: number }) {
  return (
    <span className="rating">
      {Array.from({ length: 5 }, (_, i) => (
        <span key={i} className={i < rating ? 'star filled' : 'star'}>★</span>
      ))}
    </span>
  )
}

export function BookTable({ books, loading }: Props) {
  if (loading) {
    return (
      <div className="table-feedback">
        <div className="spinner" />
        <p>Carregando livros...</p>
      </div>
    )
  }

  if (books.length === 0) {
    return null
  }

  return (
    <div className="table-wrapper">
      <p className="table-count">{books.length} livro(s) encontrado(s)</p>
      <table className="book-table">
        <thead>
          <tr>
            <th>Título</th>
            <th>Preço</th>
            <th>Disponibilidade</th>
            <th>Avaliação</th>
          </tr>
        </thead>
        <tbody>
          {books.map((book, index) => (
            <tr key={index}>
              <td className="col-title">{book.title}</td>
              <td className="col-price">{book.price}</td>
              <td className="col-availability">
                <span className={book.availability ? 'badge in-stock' : 'badge out-of-stock'}>
                  {book.availability ? 'Em estoque' : 'Indisponível'}
                </span>
              </td>
              <td className="col-rating">
                <RatingStars rating={book.rating} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
