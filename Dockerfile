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

# Copy only the solution subset we need from the monorepo
COPY FCG.Jogos/ ./FCG.Jogos/

# Restore and publish
RUN dotnet restore "FCG.Jogos/FCG.Jogos.API/FCG.Jogos.API.csproj"
RUN dotnet publish "FCG.Jogos/FCG.Jogos.API/FCG.Jogos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.Jogos.API.dll"]
