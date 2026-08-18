# Autenticacion Y Autorizacion

Kodvian Core usa JWT bearer authentication con soporte para cookie HTTP-only.

## Archivos principales

- `backend/src/Kodvian.Core.Api/Program.cs`
- `backend/src/Kodvian.Core.Api/Controllers/AuthController.cs`
- `backend/src/Kodvian.Core.Application/Auth/**`
- `backend/src/Kodvian.Core.Application/Common/Security/**`
- `backend/src/Kodvian.Core.Infrastructure/Auth/JwtOptions.cs`
- `backend/src/Kodvian.Core.Infrastructure/Services/AuthService.cs`
- `backend/src/Kodvian.Core.Infrastructure/Services/TokenService.cs`
- `backend/src/Kodvian.Core.Infrastructure/Services/PasswordHasherService.cs`

## Login

Flujo:

1. El frontend llama `POST /api/auth/login`.
2. Backend valida credenciales.
3. Backend genera JWT.
4. Backend guarda el token en cookie `auth_token`.
5. El response devuelve informacion del usuario y permisos.

La cookie configurada por backend es:

- `HttpOnly = true`.
- `SameSite = Strict`.
- `Secure = true` fuera de Development o si la request es HTTPS.
- `Path = /`.

## Sesion actual

Endpoint:

- `GET /api/auth/me`

Devuelve el usuario autenticado y sus permisos. El frontend lo usa para restaurar sesion al recargar.

## Logout

Endpoint:

- `POST /api/auth/logout`

Elimina la cookie `auth_token`.

## Configuracion JWT

En `Program.cs`, el backend valida al iniciar:

- `Jwt:Key` requerido.
- `Jwt:Key` debe tener al menos 32 caracteres.
- En produccion no puede empezar con `SET_`.
- `Jwt:Issuer` requerido.
- `Jwt:Audience` requerido.

La validacion del token incluye issuer, audience, lifetime y signing key. El `ClockSkew` configurado es de 1 minuto.

## Roles

Definidos en `RoleNames.cs`:

- `Administrador`.
- `Operativo`.
- `Solo lectura`.

## Permisos

Definidos en `PermissionCodes.cs`:

- `dashboard.read`.
- `clients.read`.
- `clients.write`.
- `projects.read`.
- `projects.write`.
- `projects.documents.read`.
- `projects.documents.write`.
- `projects.documents.delete`.
- `tasks.read`.
- `tasks.write`.
- `finances.read`.
- `finances.write`.
- `administration.read`.
- `administration.write`.

## Mapa rol-permisos

Definido en `RolePermissionMap.cs`.

`Administrador`:

- Acceso completo a todos los permisos listados.

`Operativo`:

- Dashboard read.
- Clientes read/write.
- Proyectos read/write.
- Documentos de proyecto read/write/delete.
- Tareas read/write.
- Finanzas read.

`Solo lectura`:

- Dashboard read.
- Clientes read.
- Proyectos read.
- Documentos de proyecto read.
- Tareas read.
- Finanzas read.
- Administracion read.

## Policies backend

Policies declaradas en `Program.cs`:

- `AdministrationRead`.
- `ProjectsDocumentsRead`.
- `ProjectsDocumentsWrite`.
- `ProjectsDocumentsDelete`.

## Caveats

- Muchos controllers usan solo `[Authorize]`, sin policy especifica por modulo.
- La visibilidad de botones en frontend no reemplaza autorizacion backend.
- Para nuevas acciones de escritura o datos sensibles, crear policy o verificar permiso explicitamente.
- `UsersController` requiere `AdministrationRead` y role `Administrador`, pero actualmente devuelve un resultado vacio.
