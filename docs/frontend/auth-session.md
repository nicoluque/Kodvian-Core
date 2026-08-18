# Auth Y Sesion Frontend

La sesion frontend se basa en cookie HTTP-only administrada por backend y estado en memoria dentro de Angular.

## Archivos principales

- `frontend/src/app/login-page.component.ts|html|scss`.
- `frontend/src/app/core/auth/auth-api.service.ts`.
- `frontend/src/app/core/auth/auth-session.service.ts`.
- `frontend/src/app/core/auth/auth.models.ts`.
- `frontend/src/app/core/http/auth.interceptor.ts`.
- `frontend/src/app/core/guards/auth.guard.ts`.
- `frontend/src/app/core/guards/administration.guard.ts`.

## Login

`LoginPageComponent` captura email/password y llama a `AuthSessionService.login`.

El backend crea la cookie `auth_token`. El frontend no persiste token en `localStorage` ni `sessionStorage`.

## Estado de sesion

`AuthSessionService` mantiene el usuario actual en un `BehaviorSubject<CurrentUser | null>`.

La sesion es en memoria. Al recargar la pagina, se llama `/api/auth/me` para reconstruir el usuario desde la cookie.

## Interceptor

`auth.interceptor.ts` agrega `withCredentials: true` a cada request.

Esto permite que el navegador envie la cookie `auth_token` al backend.

Ante un HTTP 401 en rutas no-login, el interceptor limpia sesion y navega a `/login`.

## Guards

`authGuard`:

- Ejecuta `ensureSessionLoaded()`.
- Permite acceso si hay usuario.
- Redirige a `/login` si no hay sesion valida.

`administrationGuard`:

- Exige permiso `administration.read`.
- Redirige a `/dashboard` si no esta permitido.

## Permisos en UI

El frontend puede ocultar o deshabilitar acciones segun permisos, pero la autorizacion real debe validarse en backend.

Permisos actualmente usados en UI:

- `administration.read`.
- `projects.documents.read`.
- `projects.documents.write`.
- `projects.documents.delete`.

## Cuidados

- `LoginResponse.accessToken` existe en modelos, pero la app opera con cookie.
- No agregar almacenamiento local del JWT sin redisenar el modelo de seguridad.
- Si se agrega frontend en otro dominio, revisar CORS, cookies y `SameSite`.
- Filtrar menu por permisos si se quiere evitar rutas visibles que luego bloquea el guard.
