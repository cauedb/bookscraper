import type { Book, PagedResult } from '../types/book'

const BASE_URL = import.meta.env.VITE_API_URL as string

export async function getCategories(): Promise<string[]> {
  const response = await fetch(`${BASE_URL}/categories`)

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao carregar categorias.')
  }

  return response.json() as Promise<string[]>
}

export async function scrapeByCategory(category: string): Promise<void> {
  const response = await fetch(`${BASE_URL}/scrape?category=${encodeURIComponent(category)}`, {
    method: 'POST',
  })

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao iniciar scraping.')
  }
}

export async function getLatestBooks(page: number, pageSize: number): Promise<PagedResult<Book>> {
  const response = await fetch(`${BASE_URL}/results/latest?page=${page}&pageSize=${pageSize}`)

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error((body as { error?: string }).error ?? 'Erro ao recuperar livros.')
  }

  return response.json() as Promise<PagedResult<Book>>
}
