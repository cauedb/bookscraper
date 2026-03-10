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

# ─── Estágio 2: Runtime ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "bookscraper-ms-webapi.dll"]
