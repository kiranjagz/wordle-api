# syntax=docker/dockerfile:1
# Build context is the repository root: docker build -f WordleApi.Host/Dockerfile .

# ── Build ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY WordleApi.Host/WordleApi.Host.csproj WordleApi.Host/
RUN dotnet restore WordleApi.Host/WordleApi.Host.csproj

COPY WordleApi.Host/ WordleApi.Host/
RUN dotnet publish WordleApi.Host/WordleApi.Host.csproj -c Release -o /app

# ── Runtime ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:5050

EXPOSE 5050
ENTRYPOINT ["dotnet", "WordleApi.Host.dll"]
