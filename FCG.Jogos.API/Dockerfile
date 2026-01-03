# syntax=docker/dockerfile:1

# Imagem base para runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Imagem para build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia apenas os arquivos de projeto e solução para cache de dependências
COPY FCG.Jogos.sln ./
COPY FCG.Jogos.API/FCG.Jogos.API.csproj FCG.Jogos.API/
COPY FCG.Jogos.Application/FCG.Jogos.Application.csproj FCG.Jogos.Application/
COPY FCG.Jogos.Domain/FCG.Jogos.Domain.csproj FCG.Jogos.Domain/
COPY FCG.Jogos.Infrastructure/FCG.Jogos.Infrastructure.csproj FCG.Jogos.Infrastructure/

# Restaura dependências
RUN dotnet restore FCG.Jogos.API/FCG.Jogos.API.csproj

# Copia o restante do código
COPY . .

# Publica a aplicação
RUN dotnet publish FCG.Jogos.API/FCG.Jogos.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Imagem final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Jogos.API.dll"]