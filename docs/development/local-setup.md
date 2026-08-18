# Setup Local

## Requisitos

- .NET SDK 8.
- Node.js 20+.
- npm 10+.
- PostgreSQL 15+.

## Base de datos local

Prerequisitos actuales:

- PostgreSQL activo en `localhost:5432`.
- Credenciales por defecto segun `backend/src/Kodvian.Core.Api/appsettings.json`.

La API aplica migrations pendientes al iniciar y ejecuta seeding inicial.

## Backend

Desde `backend/`:

```bash
dotnet restore
dotnet build Kodvian.Core.slnx
dotnet run --project src/Kodvian.Core.Api
```

La API expone:

- `/api/health`.
- `/healthz`.
- Swagger en Development.

## Frontend

Desde `frontend/`:

```bash
npm install
npm start
```

El frontend usa `proxy.conf.json` para enviar `/api/...` a la API local.

Abrir:

```text
http://localhost:4200
```

## VS Code

El repo incluye configuracion en `.vscode/`.

Flujo sugerido:

1. Abrir la raiz del repo en VS Code.
2. Ejecutar tarea `Setup: install dependencies`.
3. Ejecutar tarea `Start: full stack`.
4. Abrir `http://localhost:4200`.

## Build productivo local

Frontend:

```bash
npm run build
```

Backend:

```bash
dotnet build Kodvian.Core.slnx
```

Docker desde raiz:

```bash
docker build -t kodvian-core .
```

## Troubleshooting

- Si falla login, revisar API, PostgreSQL, seed admin y cookie `auth_token`.
- Si el frontend no llega a backend, revisar `frontend/proxy.conf.json` y puerto local de API.
- Si falla startup backend en Production, revisar `Jwt__*`, `AuthSeed__*`, `DATABASE_URL` y `Storage__*`.
- Si falla upload de PDF, revisar provider de storage, tamano maximo y content type.
