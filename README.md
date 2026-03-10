# BookScraper

Aplicação full-stack que coleta informações de livros do site público [books.toscrape.com](https://books.toscrape.com) via web scraping e as exibe em uma interface web.

---

## Funcionalidades

- **Scraping sob demanda** — dispara a coleta de livros por categoria (ou todas) via botão na interface
- **Filtro por categoria** — lista as categorias disponíveis no site e permite coletar apenas os livros de uma delas
- **Cache em memória** — resultados ficam armazenados no backend após o scraping; consultas GET nunca re-disparam o Selenium
- **Paginação automática** — o scraper navega por todas as páginas de cada categoria até coletar todos os livros
- **Tabela paginada** — exibe título, preço, avaliação (1–5) e disponibilidade; navegação por páginas com 20, 40 ou 50 itens por página
- **Feedback de erros** — mensagens amigáveis na UI, com detalhes técnicos apenas no console do browser
- **Log em arquivo** — logs do backend gravados em arquivo rotativo diário, acessíveis na pasta `./logs/`

---

## Arquitetura

```
bookscraper/
├── bookscraper-ms/                  # Backend .NET (projeto único)
│   └── bookscraper-ms-webapi/
│       ├── Models/                  # Book record
│       ├── Scraping/                # BookScraperService + SeleniumDriverFactory
│       └── Program.cs               # Minimal API — todos os endpoints
├── bookscraper-mfe/                 # Frontend React + TypeScript + Vite
├── logs/                            # Logs do backend (gerado automaticamente)
└── docker-compose.yml
```

### Backend
- **Stack:** .NET 10 · ASP.NET Core Minimal API · Selenium 4 · Serilog
- **Padrão:** projeto único e plano — sem camadas, sem controllers
- **Rotas:**

| Método | Rota | Descrição |
|---|---|---|
| GET | `/health` | Health check |
| POST | `/scrape?category=All` | Inicia scraping (categoria opcional, padrão `All`) |
| GET | `/results/latest?page=1&pageSize=20` | Retorna livros paginados |
| GET | `/results/latest-by-category/{category}` | Retorna livros de uma categoria paginados |

### Frontend
- **Stack:** React 18 · TypeScript · Vite · Nginx (produção)
- Consome a API REST do backend e exibe os dados em tabela paginada

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

## Logs

O backend utiliza **Serilog** com dois destinos simultâneos: console e arquivo.

### Com Docker

Os logs são gravados no container em `/app/logs/` e mapeados via volume para a pasta `./logs/` no host. Os arquivos são rotativos por dia e mantidos por 7 dias.

```bash
# Listar arquivos de log gerados
ls logs/

# Acompanhar o log em tempo real
tail -f logs/bookscraper-<YYYYMMDD>.log

# Ou via Docker (saída do container)
docker compose logs -f backend
```

### Sem Docker

Os logs são gravados na pasta `logs/` dentro do diretório `bookscraper-ms/bookscraper-ms-webapi/`, criada automaticamente na primeira execução.

```bash
# Acompanhar o log em tempo real
tail -f bookscraper-ms/bookscraper-ms-webapi/logs/bookscraper-<YYYYMMDD>.log
```

> O path do arquivo de log pode ser alterado em `appsettings.json`, chave `Serilog.WriteTo[File].Args.path`.

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
