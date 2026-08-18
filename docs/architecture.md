# Arquitectura General

Kodvian Core usa una arquitectura full-stack con frontend Angular y backend ASP.NET Core. En produccion, el backend sirve la API y tambien el build estatico del SPA Angular desde el mismo servicio.

## Vista de alto nivel

```text
Usuario
  -> Angular SPA
  -> ASP.NET Core API
  -> Servicios de infraestructura
  -> EF Core
  -> PostgreSQL
  -> Storage local o S3-compatible
```

## Repositorio

```text
backend/
  Kodvian.Core.slnx
  src/Kodvian.Core.Api
  src/Kodvian.Core.Application
  src/Kodvian.Core.Domain
  src/Kodvian.Core.Infrastructure
  tests/Kodvian.Core.Application.Tests

frontend/
  src/app/core
  src/app/shared
  src/app/layout
  src/app/modules

docs/
  backend
  frontend
  modules
  development
```

## Backend por capas

- `Kodvian.Core.Api`: controllers, middleware, startup, autenticacion, autorizacion, Swagger, health checks y pipeline HTTP.
- `Kodvian.Core.Application`: DTOs, request models, service abstractions, modelos comunes, permisos y contratos de aplicacion.
- `Kodvian.Core.Domain`: entidades y enums de negocio.
- `Kodvian.Core.Infrastructure`: EF Core, DbContext, migrations, seeding, servicios concretos, storage, hashing y token service.

## Frontend

- Angular 19 con standalone components.
- Rutas lazy por modulo.
- Layout autenticado con header, sidebar y `router-outlet`.
- Servicios por modulo consumen endpoints relativos `/api/...`.
- La sesion se basa en cookie HTTP-only enviada con `withCredentials`.

## Contrato API

Convencion base:

- Respuestas con `success`, `message`, `data`.
- DTOs separados de entidades.
- Listados preparados para paginacion con `pageNumber` y `pageSize`.
- Texto visible al usuario en espanol latino.

## Runtime backend

`backend/src/Kodvian.Core.Api/Program.cs` configura:

- Controllers.
- Swagger en desarrollo.
- Health checks.
- JWT bearer authentication.
- Policies de autorizacion.
- CORS opcional.
- Rate limiter para login.
- Forwarded headers para proxy Railway.
- Headers de seguridad y CSP.
- Archivos estaticos y fallback SPA.
- Migraciones EF Core al iniciar.
- Seeding de usuario admin y categorias financieras.

## Deploy

La estrategia acordada es un solo servicio:

- Docker build compila Angular.
- Docker publish compila la API .NET.
- El build Angular se copia a `wwwroot`.
- La API sirve la SPA con `UseStaticFiles` y `MapFallbackToFile("index.html")`.
- Railway expone `/healthz` como healthcheck.

Detalle operativo: [Railway readiness](railway-readiness.md).

## Riesgos transversales

- Migraciones automaticas al startup requieren cuidado en produccion.
- Storage local no es persistente para contenedores productivos.
- Authorization fina esta aplicada solo en algunas rutas.
- Algunos flujos de UI dependen de permisos del frontend, pero la autorizacion real debe permanecer en backend.
