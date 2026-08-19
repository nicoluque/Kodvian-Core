# Routing Y Navegacion

Las rutas principales estan definidas en `frontend/src/app/app.routes.ts`.

## Rutas

| Ruta | Componente/modulo | Guards |
|---|---|---|
| `/login` | `LoginPageComponent` | Ninguno |
| `/` | Redirige a `/dashboard` dentro del shell | `authGuard` |
| `/dashboard` | `DashboardPageComponent` | `authGuard` heredado |
| `/mi-trabajo` | `MiTrabajoPageComponent` | `authGuard` heredado + `permissionGuard` con `developer.work.read` |
| `/clientes` | `ClientesPageComponent` | `authGuard` heredado |
| `/desarrolladores` | `DesarrolladoresPageComponent` | `authGuard` heredado |
| `/proyectos` | `ProyectosPageComponent` | `authGuard` heredado |
| `/tareas` | `TareasPageComponent` | `authGuard` heredado |
| `/finanzas` | `FinanzasPageComponent` | `authGuard` heredado |
| `/administracion` | `AdministracionPageComponent` | `authGuard` heredado + `administrationGuard` |
| `**` | Redirige a `/login` | Ninguno |

## Layout autenticado

Archivos:

- `frontend/src/app/layout/main-layout.component.ts`.
- `frontend/src/app/layout/header.component.ts`.
- `frontend/src/app/layout/sidebar.component.ts`.

`MainLayoutComponent` contiene:

- `mat-sidenav`.
- Header superior.
- Sidebar lateral.
- `router-outlet` para paginas internas.

## Comportamiento responsive del layout

- Desktop: sidebar en modo `side`.
- Mobile/tablet: sidebar en modo `over`.
- El breakpoint actual se evalua con `window.matchMedia('(max-width: 960px)')`.
- Al seleccionar item en mobile, el sidebar se cierra.

## Navegacion

La fuente del menu esta en:

- `frontend/src/app/core/services/navigation.service.ts`.

Cada item puede declarar `permission`. `NavigationService` filtra el menu segun permisos del usuario autenticado, para que el rol `Desarrollador` vea solo `Mi trabajo`.

## Administracion

La ruta `/administracion` usa `administrationGuard`, que exige el permiso `administration.read`. Si el usuario no tiene permiso, se redirige a `/dashboard`.

## Mi trabajo

La ruta `/mi-trabajo` usa `permissionGuard` y exige `developer.work.read`. Despues del login, un usuario con `developerId` se redirige automaticamente a esta ruta.

## Cuidados

- El uso directo de `window.matchMedia` puede ser sensible a SSR/prerender.
- Las rutas comodin redirigen a login; verificar que esto no oculte errores de rutas internas durante desarrollo.
