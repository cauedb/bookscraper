import type { Book } from '../types/book'
import './BookTable.css'

interface Props {
  books: Book[]
  loading: boolean
  page: number
  pageSize: number
  totalCount: number
  onPageChange: (page: number) => void
  onPageSizeChange: (size: number) => void
}

const PAGE_SIZE_OPTIONS = [20, 40, 50]

function RatingStars({ rating }: { rating: number }) {
  return (
    <span className="rating">
      {Array.from({ length: 5 }, (_, i) => (
        <span key={i} className={i < rating ? 'star filled' : 'star'}>★</span>
      ))}
    </span>
  )
}

export function BookTable({ books, loading, page, pageSize, totalCount, onPageChange, onPageSizeChange }: Props) {
  if (loading) {
    return (
      <div className="table-feedback">
        <div className="spinner" />
        <p>Carregando livros...</p>
      </div>
    )
  }

  if (totalCount === 0) {
    return null
  }

  const totalPages = Math.ceil(totalCount / pageSize)
  const rangeStart = (page - 1) * pageSize + 1
  const rangeEnd = Math.min(page * pageSize, totalCount)

  return (
    <div className="table-wrapper">
      <div className="table-toolbar">
        <span className="table-count">
          Exibindo {rangeStart} – {rangeEnd} de {totalCount} resultados
        </span>
        <div className="page-size-selector">
          <label htmlFor="page-size">Itens por página:</label>
          <select
            id="page-size"
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
          >
            {PAGE_SIZE_OPTIONS.map((opt) => (
              <option key={opt} value={opt}>{opt}</option>
            ))}
          </select>
        </div>
      </div>

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

      <div className="pagination">
        <button
          className="btn-page"
          onClick={() => onPageChange(1)}
          disabled={page === 1}
        >
          «
        </button>
        <button
          className="btn-page"
          onClick={() => onPageChange(page - 1)}
          disabled={page === 1}
        >
          ‹
        </button>

        <span className="page-info">Página {page} de {totalPages}</span>

        <button
          className="btn-page"
          onClick={() => onPageChange(page + 1)}
          disabled={page === totalPages}
        >
          ›
        </button>
        <button
          className="btn-page"
          onClick={() => onPageChange(totalPages)}
          disabled={page === totalPages}
        >
          »
        </button>
      </div>
    </div>
  )
}
