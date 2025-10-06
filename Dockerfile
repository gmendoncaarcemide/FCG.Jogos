# syntax=docker/dockerfile:1

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
# Expose HTTP only; see note in README about HTTPS redirection in Program.cs
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy solution and project files for better layer caching
COPY FCG.Jogos.sln ./
COPY FCG.Jogos.API/FCG.Jogos.API.csproj FCG.Jogos.API/
COPY FCG.Jogos.Application/FCG.Jogos.Application.csproj FCG.Jogos.Application/
COPY FCG.Jogos.Domain/FCG.Jogos.Domain.csproj FCG.Jogos.Domain/
COPY FCG.Jogos.Infrastructure/FCG.Jogos.Infrastructure.csproj FCG.Jogos.Infrastructure/

# Restore dependencies
RUN dotnet restore FCG.Jogos.API/FCG.Jogos.API.csproj

# Copy the rest of the source
COPY . .
WORKDIR /src/FCG.Jogos.API

# Build
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FCG.Jogos.API.dll"]
