# ─── Estágio 1: Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia todos os .csproj e restaura dependências (camadas de cache)
COPY ["bookscraper-ms-webapi/bookscraper-ms-webapi.csproj", "bookscraper-ms-webapi/"]
COPY ["BookScraper.Domain/BookScraper.Domain.csproj", "BookScraper.Domain/"]
COPY ["BookScraper.Application/BookScraper.Application.csproj", "BookScraper.Application/"]
COPY ["BookScraper.Infrastructure/BookScraper.Infrastructure.csproj", "BookScraper.Infrastructure/"]
RUN dotnet restore "bookscraper-ms-webapi/bookscraper-ms-webapi.csproj"

# Copia o restante do código e publica
COPY . .
WORKDIR "/src/bookscraper-ms-webapi"
RUN dotnet publish "./bookscraper-ms-webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ─── Estágio 2: Runtime + Chrome ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Instala dependências do Chrome Headless
RUN apt-get update && apt-get install -y --no-install-recommends \
    wget \
    unzip \
    ca-certificates \
    fonts-liberation \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libatspi2.0-0 \
    libcups2 \
    libdbus-1-3 \
    libdrm2 \
    libgbm1 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libwayland-client0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    libxshmfence1 \
    xdg-utils \
    && rm -rf /var/lib/apt/lists/*

# Instala Chrome for Testing 145.0.7632.117 + ChromeDriver compatível
RUN wget -q "https://storage.googleapis.com/chrome-for-testing-public/145.0.7632.117/linux64/chrome-linux64.zip" \
    && unzip chrome-linux64.zip \
    && mv chrome-linux64 /opt/chrome \
    && ln -s /opt/chrome/chrome /usr/local/bin/google-chrome \
    && rm chrome-linux64.zip

RUN wget -q "https://storage.googleapis.com/chrome-for-testing-public/145.0.7632.117/linux64/chromedriver-linux64.zip" \
    && unzip chromedriver-linux64.zip \
    && mv chromedriver-linux64/chromedriver /usr/local/bin/chromedriver \
    && chmod +x /usr/local/bin/chromedriver \
    && rm -rf chromedriver-linux64.zip chromedriver-linux64

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "bookscraper-ms-webapi.dll"]
