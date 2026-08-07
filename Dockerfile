# syntax=docker/dockerfile:1

# ---- Frontend (Angular) ----
FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

# ---- Backend (.NET) ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src

COPY backend/src/Kodvian.Core.Domain/Kodvian.Core.Domain.csproj backend/src/Kodvian.Core.Domain/
COPY backend/src/Kodvian.Core.Application/Kodvian.Core.Application.csproj backend/src/Kodvian.Core.Application/
COPY backend/src/Kodvian.Core.Infrastructure/Kodvian.Core.Infrastructure.csproj backend/src/Kodvian.Core.Infrastructure/
COPY backend/src/Kodvian.Core.Api/Kodvian.Core.Api.csproj backend/src/Kodvian.Core.Api/

RUN dotnet restore backend/src/Kodvian.Core.Api/Kodvian.Core.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/Kodvian.Core.Api/Kodvian.Core.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend/dist/frontend/browser ./wwwroot
COPY docker-entrypoint.sh /app/docker-entrypoint.sh
RUN chmod +x /app/docker-entrypoint.sh

EXPOSE 8080

ENTRYPOINT ["/app/docker-entrypoint.sh"]