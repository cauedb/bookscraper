import { useState, useEffect } from 'react'
import { getCategories, scrapeByCategory, getLatestBooks } from './services/api'
import { BookTable } from './components/BookTable'
import type { Book } from './types/book'
import './App.css'

type ScrapeStatus = 'idle' | 'loading' | 'success' | 'error'
type FetchStatus = 'idle' | 'loading' | 'success' | 'error'

function App() {
  const [categories, setCategories] = useState<string[]>([])
  const [selectedCategory, setSelectedCategory] = useState<string>('All')

  const [scrapeStatus, setScrapeStatus] = useState<ScrapeStatus>('idle')
  const [scrapeError, setScrapeError] = useState<string | null>(null)

  const [fetchStatus, setFetchStatus] = useState<FetchStatus>('idle')
  const [fetchError, setFetchError] = useState<string | null>(null)
  const [books, setBooks] = useState<Book[]>([])

  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch(() => setCategories(['All']))
  }, [])

  async function handleScrape() {
    setScrapeStatus('loading')
    setScrapeError(null)
    try {
      await scrapeByCategory(selectedCategory)
      setScrapeStatus('success')
    } catch (err) {
      console.error('[Scrape] Erro retornado pela API:', err)
      setScrapeStatus('error')
      setScrapeError('Erro ao coletar os dados.')
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
      console.error('[Fetch] Erro retornado pela API:', err)
      setFetchStatus('error')
      setFetchError('Erro ao recuperar os dados.')
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
            <select
              className="category-select"
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
              disabled={scrapeStatus === 'loading' || categories.length === 0}
            >
              {categories.length === 0
                ? <option value="All">Carregando categorias...</option>
                : categories.map((cat) => (
                    <option key={cat} value={cat}>{cat}</option>
                  ))
              }
            </select>

            <button
              className="btn btn-primary"
              onClick={handleScrape}
              disabled={scrapeStatus === 'loading' || categories.length === 0}
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
