import type { Book, PagedResult } from '../types/book'

const BASE_URL = import.meta.env.VITE_API_URL as string

async function fetchWithTimeout(url: string, options: RequestInit = {}, timeoutMs: number): Promise<Response> {
  const controller = new AbortController()
  const timer = setTimeout(() => controller.abort(), timeoutMs)
  try {
    return await fetch(url, { ...options, signal: controller.signal })
  } catch (err) {
    if (err instanceof DOMException && err.name === 'AbortError')
      throw new Error(`A requisição excedeu o tempo limite de ${Math.round(timeoutMs / 1000)}s.`)
    throw err
  } finally {
    clearTimeout(timer)
  }
}

export async function getCategories(): Promise<string[]> {
  const response = await fetchWithTimeout(`${BASE_URL}/categories`, {}, 30_000)

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao carregar categorias.')
  }

  return response.json() as Promise<string[]>
}

export async function scrapeByCategory(category: string): Promise<void> {
  const response = await fetchWithTimeout(
    `${BASE_URL}/scrape?category=${encodeURIComponent(category)}`,
    { method: 'POST' },
    300_000
  )

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao iniciar scraping.')
  }
}

export async function getLatestBooks(page: number, pageSize: number, category: string): Promise<PagedResult<Book>> {
  const url = category.toLowerCase() === 'all'
    ? `${BASE_URL}/results/latest?page=${page}&pageSize=${pageSize}`
    : `${BASE_URL}/results/latest-by-category/${encodeURIComponent(category)}?page=${page}&pageSize=${pageSize}`

  const response = await fetchWithTimeout(url, {}, 30_000)

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao recuperar livros.')
  }

  return response.json() as Promise<PagedResult<Book>>
}
