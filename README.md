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

Nota: al iniciar la API en entorno `Development`, EF Core aplica automaticamente las migraciones pendientes y ejecuta el seeding inicial.

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

### Variables de entorno backend

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:${PORT}`
- `DATABASE_URL` (provista por Railway Postgres)
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__Key` (minimo 32 caracteres)
- `AuthSeed__AdminEmail`
- `AuthSeed__AdminPassword`
- `AuthSeed__AdminFullName`
- `Cors__AllowedOrigins` (ejemplo: `https://tu-dominio.app`)

### Comandos sugeridos

Backend:

```bash
dotnet publish backend/src/Kodvian.Core.Api/Kodvian.Core.Api.csproj -c Release -o /app/publish
dotnet /app/publish/Kodvian.Core.Api.dll
```

Frontend:

```bash
npm ci --prefix frontend
npm run build --prefix frontend
```

### Verificaciones post deploy

- `GET /api/health`
- `GET /healthz`
- Login con `admin@kodvian.local`
- Navegacion de modulos principales
