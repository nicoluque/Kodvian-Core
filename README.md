# Kodvian Core

Base inicial de un sistema interno liviano para gestión de empresa de software.

## Estructura del repositorio

- `backend/`: solución .NET 8 Web API por capas
  - `src/Kodvian.Core.Api`
  - `src/Kodvian.Core.Application`
  - `src/Kodvian.Core.Domain`
  - `src/Kodvian.Core.Infrastructure`
  - `tests/Kodvian.Core.Application.Tests`
- `frontend/`: Angular standalone + Angular Material
  - `src/app/core`
  - `src/app/shared`
  - `src/app/layout`
  - `src/app/modules/dashboard`
  - `src/app/modules/clientes`
  - `src/app/modules/proyectos`
  - `src/app/modules/tareas`
  - `src/app/modules/finanzas`
  - `src/app/modules/administracion`
- `docs/railway-readiness.md`: checklist de deploy en Railway

## Requisitos

- .NET SDK 8
- Node.js 20+
- npm 10+
- PostgreSQL 15+

## Backend

Desde `backend/`:

```bash
dotnet restore
dotnet build Kodvian.Core.slnx
dotnet run --project src/Kodvian.Core.Api
```

Connection string inicial en `backend/src/Kodvian.Core.Api/appsettings.json`.

Nota: al iniciar la API, EF Core aplica automaticamente las migraciones pendientes y ejecuta el seeding inicial.

## Frontend

Desde `frontend/`:

```bash
npm install
npm start
```

Para compilación de producción:

```bash
npm run build
```

## Ejecucion local en VS Code

Se incluye configuracion en `.vscode/` para correr backend y frontend sin pasos manuales extra.

Prerequisitos:

- PostgreSQL activo en `localhost:5432`
- Credenciales por defecto segun `appsettings.json` (`postgres` / `1234`)

Pasos sugeridos:

1. Abrir la carpeta raiz del repositorio en VS Code.
2. Ejecutar la tarea **Terminal > Run Task > Setup: install dependencies**.
3. Ejecutar la tarea **Terminal > Run Task > Start: full stack**.
4. Abrir `http://localhost:4200`.

Para debug, usar **Run and Debug** con el perfil `Full Stack (API + Angular)`.

## Convenciones base

- Código en inglés para clases y capas.
- Texto visible al usuario en español latino.
- Respuestas API con formato `success`, `message`, `data`.
- DTOs separados de entidades.
- Listados preparados para paginación (`pageNumber`, `pageSize`).

## Deploy en Railway

Arquitectura: **un solo servicio**. El Dockerfile multi-stage construye Angular y la API; la API sirve el SPA desde `wwwroot` (mismo origen). Los archivos van a object storage S3-compatible.

Checklist detallado: [`docs/railway-readiness.md`](docs/railway-readiness.md).

### Pasos

1. Crear un servicio en Railway apuntando a este repo (usa `Dockerfile` / `railway.toml`).
2. Adjuntar plugin **PostgreSQL** (inyecta `DATABASE_URL`).
3. Configurar las variables de entorno abajo **antes** del primer deploy.
4. Crear un bucket S3-compatible (R2, S3, MinIO, etc.) y cargar sus credenciales.

### Variables de entorno

**Runtime**

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}` (el entrypoint del contenedor también usa `PORT`)
- `DATABASE_URL` (plugin Postgres)

**Auth / JWT**

- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key` (mínimo 32 caracteres; no usar placeholder `SET_`)
- `Jwt__ExpirationMinutes` (opcional; default 30)
- `AuthSeed__AdminEmail`
- `AuthSeed__AdminPassword` (no usar `Admin123*`)
- `AuthSeed__AdminFullName`

**CORS** (opcional con mismo origen)

- `Cors__AllowedOrigins` (ejemplo: `https://tu-dominio.app`)

**Storage (Production)**

- `Storage__Provider=S3`
- `Storage__Bucket`
- `Storage__AccessKey`
- `Storage__SecretKey`
- `Storage__ServiceUrl` (endpoint del proveedor)
- `Storage__Region` (ej. `auto` en Cloudflare R2)
- `Storage__ForcePathStyle=true` (recomendado para R2/MinIO)
- `Storage__MaxPdfSizeMb` (opcional; default 10)

**GitHub (opcional; arranque degradado si `Enabled=false`)**

- `GitHub__Enabled` (`true` para activar la integración)
- `GitHub__ClientId` / `GitHub__ClientSecret` (OAuth App; obligatorios si Enabled en Production)
- `GitHub__CallbackUrl` (ej. `https://tu-dominio.app/api/profile/github/callback`)
- `GitHub__ServiceToken` (PAT recomendado para validar repos como admin)
- `GitHub__WebhookSecret` (webhooks)
- `GitHub__DefaultLabel` (opcional; default `kodvian`)
- `GitHub__ApiBaseUrl` (opcional; default `https://api.github.com`)
- `TokenEncryption__Key` (obligatoria si `GitHub__Enabled=true` en Production; Base64 de 32 bytes o secreto ≥32 caracteres)

En Development el storage es local (`Storage__Provider=Local`, `App_Data/files`).

### Build local del contenedor

```bash
docker build -t kodvian-core .
docker run --rm -p 8080:8080 \
  -e PORT=8080 \
  -e DATABASE_URL=... \
  -e Jwt__Issuer=... \
  -e Jwt__Audience=... \
  -e Jwt__Key=... \
  -e AuthSeed__AdminEmail=... \
  -e AuthSeed__AdminPassword=... \
  -e AuthSeed__AdminFullName=... \
  -e Storage__Provider=S3 \
  -e Storage__Bucket=... \
  -e Storage__AccessKey=... \
  -e Storage__SecretKey=... \
  -e Storage__ServiceUrl=... \
  -e Storage__Region=auto \
  -e Storage__ForcePathStyle=true \
  kodvian-core
```

### Verificaciones post deploy

- `GET /` → SPA
- `GET /api/health`
- `GET /healthz`
- Login con el admin configurado en `AuthSeed`
- Navegación de módulos principales
- Upload de un PDF (documento o recibo) contra S3
