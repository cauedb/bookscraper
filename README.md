# BookScraper

Aplicação full-stack que coleta informações de livros do site público [books.toscrape.com](https://books.toscrape.com) via web scraping e as exibe em uma interface web.

---

## Funcionalidades

- **Scraping sob demanda** — dispara a coleta de livros por categoria (ou todas) via botão na interface
- **Filtro por categoria** — lista as categorias disponíveis no site e permite coletar apenas os livros de uma delas
- **Cache em memória** — resultados ficam armazenados no backend após o scraping; consultas GET nunca re-disparam o Selenium
- **Paginação automática** — o scraper navega por todas as páginas de cada categoria até coletar todos os livros
- **Tabela de resultados** — exibe título, preço, avaliação (1–5) e disponibilidade de cada livro
- **Feedback de erros** — mensagens amigáveis na UI, com detalhes técnicos apenas no console do browser

---

## Arquitetura

```
bookscraper/
├── bookscraper-ms/          # Backend .NET (Clean Architecture)
│   ├── BookScraper.Domain/          # Entidades e interfaces (sem dependências externas)
│   ├── BookScraper.Application/     # DTOs e mapeamento
│   ├── BookScraper.Infrastructure/  # Selenium (RemoteWebDriver / ChromeDriver)
│   └── bookscraper-ms-webapi/       # API REST (ASP.NET Core)
├── bookscraper-mfe/         # Frontend React + TypeScript + Vite
└── docker-compose.yml
```

### Backend
- **Stack:** .NET 10 · ASP.NET Core · Selenium 4
- **Padrão:** Clean Architecture — Domain → Application → Infrastructure → API
- **Rotas:**

| Método | Rota | Descrição |
|---|---|---|
| GET | `/health` | Health check |
| POST | `/scrape?category=All` | Inicia scraping (categoria opcional, padrão `All`) |
| GET | `/results/latest` | Retorna todos os livros coletados |
| GET | `/results/latest-by-category/{category}` | Retorna livros de uma categoria |

### Frontend
- **Stack:** React 18 · TypeScript · Vite · Nginx (produção)
- Consome a API REST do backend e exibe os dados em tabela

---

## Rodando com Docker

### Pré-requisitos
- [Docker](https://www.docker.com/) e Docker Compose instalados

### Passos

```bash
# Clone o repositório
git clone <url-do-repositorio>
cd bookscraper

# Suba os serviços
docker compose up --build
```

> **Atenção:** o primeiro build pode demorar alguns minutos, pois a imagem `selenium/standalone-chrome` é baixada do Docker Hub (~1 GB). Nas execuções seguintes o processo é significativamente mais rápido.

Após a inicialização:

| Serviço | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Backend (API) | http://localhost:5000 |

Para parar os serviços:

```bash
docker compose down
```

O projeto utiliza três containers:
- **selenium** — `selenium/standalone-chrome`, responsável por executar o Chrome headless
- **backend** — API .NET que se conecta ao Selenium via `RemoteWebDriver`
- **frontend** — aplicação React servida pelo Nginx

---

## Rodando sem Docker

### Pré-requisitos

| Requisito | Versão |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ |
| [Node.js](https://nodejs.org/) | 18+ |
| Google Chrome | qualquer versão recente (ChromeDriver é gerenciado automaticamente) |

### Backend

```bash
cd bookscraper-ms

# Restaura dependências e inicia a API
dotnet run --project bookscraper-ms-webapi
```

A API ficará disponível em `http://localhost:5000`.

> A URL do site alvo pode ser alterada em `bookscraper-ms-webapi/appsettings.json`, chave `ScraperSettings:TargetUrl`.

### Frontend

Em outro terminal:

```bash
cd bookscraper-mfe

# Instala dependências
npm install

# Inicia o servidor de desenvolvimento
npm run dev
```

O frontend ficará disponível em `http://localhost:5173`.

> Por padrão, o frontend aponta para `http://localhost:5000`. Para alterar, defina a variável de ambiente `VITE_API_URL` antes de executar o comando.
