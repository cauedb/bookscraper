export interface Book {
  title: string
  price: string
  availability: boolean
  rating: number
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
