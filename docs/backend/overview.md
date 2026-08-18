# Backend Overview

El backend de Kodvian Core es una Web API en .NET 8 con arquitectura por capas, EF Core y PostgreSQL.

## Solucion

Archivo principal:

- `backend/Kodvian.Core.slnx`

Proyectos:

- `backend/src/Kodvian.Core.Api`
- `backend/src/Kodvian.Core.Application`
- `backend/src/Kodvian.Core.Domain`
- `backend/src/Kodvian.Core.Infrastructure`
- `backend/tests/Kodvian.Core.Application.Tests`

## Responsabilidades por capa

### API

Ruta: `backend/src/Kodvian.Core.Api`

Contiene:

- `Program.cs`: composition root, pipeline HTTP, auth, CORS, Swagger, rate limiting, static files y migrations al startup.
- `Controllers/**`: endpoints HTTP.
- `Middleware/ErrorHandlingMiddleware.cs`: manejo centralizado de errores.
- `Validation/RequestValidation.cs`: utilidades de validacion de request.

### Application

Ruta: `backend/src/Kodvian.Core.Application`

Contiene:

- DTOs de lectura y detalle.
- Request DTOs para altas, ediciones, filtros y cambios de estado.
- Interfaces de servicios.
- Modelos comunes como `ApiResponseDto`, `PagedRequestDto` y `PagedResultDto`.
- Constantes de seguridad: roles, permisos y claim types.
- Contrato de storage `IFileStorageService`.

### Domain

Ruta: `backend/src/Kodvian.Core.Domain`

Contiene:

- Entidades persistidas.
- Enums de negocio.
- `BaseEntity` con campos compartidos.

### Infrastructure

Ruta: `backend/src/Kodvian.Core.Infrastructure`

Contiene:

- `Persistence/KodvianDbContext.cs`.
- `Persistence/DbSeeder.cs`.
- `Migrations/**`.
- Servicios concretos por modulo.
- `TokenService`, `PasswordHasherService`, storage local y storage S3.
- Extension de DI en `Extensions/DependencyInjectionExtensions.cs`.

## Convenciones backend

- Controllers exponen rutas bajo `/api/...`.
- DTOs no deben exponer entidades directamente al frontend.
- Listados deben usar paginacion cuando aplique.
- Respuestas deben seguir la forma `success`, `message`, `data`.
- Validaciones de negocio deben vivir en servicios o requests, no en componentes frontend.
- Permisos sensibles deben verificarse en backend aunque el frontend oculte acciones.

## Comandos utiles

Desde `backend/`:

```bash
dotnet restore
dotnet build Kodvian.Core.slnx
dotnet test Kodvian.Core.slnx
dotnet run --project src/Kodvian.Core.Api
```

## Documentacion relacionada

- [API reference](api-reference.md)
- [Autenticacion y autorizacion](authentication-authorization.md)
- [Modelo de datos](data-model.md)
- [Modulos backend](modules.md)
- [Storage](storage.md)
- [Migraciones y seeding](migrations-seeding.md)
- [Riesgos operativos](operational-risks.md)
