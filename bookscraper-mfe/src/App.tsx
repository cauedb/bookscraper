import { useState } from 'react'
import { scrapeAll, getLatestBooks } from './services/api'
import { BookTable } from './components/BookTable'
import type { Book } from './types/book'
import './App.css'

type ScrapeStatus = 'idle' | 'loading' | 'success' | 'error'
type FetchStatus = 'idle' | 'loading' | 'success' | 'error'

function App() {
  const [scrapeStatus, setScrapeStatus] = useState<ScrapeStatus>('idle')
  const [scrapeError, setScrapeError] = useState<string | null>(null)

  const [fetchStatus, setFetchStatus] = useState<FetchStatus>('idle')
  const [fetchError, setFetchError] = useState<string | null>(null)
  const [books, setBooks] = useState<Book[]>([])

  async function handleScrape() {
    setScrapeStatus('loading')
    setScrapeError(null)
    try {
      await scrapeAll()
      setScrapeStatus('success')
    } catch (err) {
      setScrapeStatus('error')
      setScrapeError(err instanceof Error ? err.message : 'Erro desconhecido.')
    }
  }

  async function handleFetch() {
    setFetchStatus('loading')
    setFetchError(null)
    setBooks([])
    try {
      const data = await getLatestBooks()
      setBooks(data)
      setFetchStatus('success')
    } catch (err) {
      setFetchStatus('error')
      setFetchError(err instanceof Error ? err.message : 'Erro desconhecido.')
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>BookScraper</h1>
        <p>Coleta e visualização de livros de <strong>books.toscrape.com</strong></p>
      </header>

      <main className="app-main">
        <section className="actions-bar">
          <div className="action-group">
            <button
              className="btn btn-primary"
              onClick={handleScrape}
              disabled={scrapeStatus === 'loading'}
            >
              Iniciar coleta de dados
            </button>
            {scrapeStatus === 'loading' && (
              <span className="status-text loading">Coletando Dados....</span>
            )}
            {scrapeStatus === 'success' && (
              <span className="status-text success">Dados coletados com sucesso</span>
            )}
            {scrapeStatus === 'error' && (
              <span className="status-text error">{scrapeError}</span>
            )}
          </div>

          <div className="action-group">
            <button
              className="btn btn-secondary"
              onClick={handleFetch}
              disabled={fetchStatus === 'loading'}
            >
              Recuperar dados coletados
            </button>
            {fetchStatus === 'error' && (
              <span className="status-text error">{fetchError}</span>
            )}
          </div>
        </section>

        <section className="results-section">
          <BookTable books={books} loading={fetchStatus === 'loading'} />
        </section>
      </main>
    </div>
  )
}

export default App
