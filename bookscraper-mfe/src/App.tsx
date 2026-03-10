import { useState, useEffect, useRef } from 'react'
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
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(20)

  const hasFetchedRef = useRef(false)

  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch((err) => {
        console.error('[Categories] Erro ao carregar categorias:', err)
        setCategories(['All'])
      })
  }, [])

  async function fetchBooks(p: number, ps: number) {
    setFetchStatus('loading')
    setFetchError(null)
    setBooks([])
    try {
      const data = await getLatestBooks(p, ps)
      setBooks(data.items)
      setTotalCount(data.totalCount)
      setFetchStatus('success')
      hasFetchedRef.current = true
    } catch (err) {
      console.error('[Fetch] Erro retornado pela API:', err)
      setFetchStatus('error')
      setFetchError('Erro ao recuperar os dados.')
    }
  }

  useEffect(() => {
    if (hasFetchedRef.current) {
      fetchBooks(page, pageSize)
    }
  }, [page, pageSize])

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

  function handleFetch() {
    hasFetchedRef.current = false
    setPage(1)
    fetchBooks(1, pageSize)
  }

  function handlePageSizeChange(newSize: number) {
    setPage(1)
    setPageSize(newSize)
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
          <BookTable
            books={books}
            loading={fetchStatus === 'loading'}
            page={page}
            pageSize={pageSize}
            totalCount={totalCount}
            onPageChange={setPage}
            onPageSizeChange={handlePageSizeChange}
          />
        </section>
      </main>
    </div>
  )
}

export default App
