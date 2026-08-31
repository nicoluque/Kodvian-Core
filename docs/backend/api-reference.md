# API Reference

Esta referencia lista los endpoints detectados en `backend/src/Kodvian.Core.Api/Controllers`.

## Auth

Controller: `backend/src/Kodvian.Core.Api/Controllers/AuthController.cs`

Base route: `/api/auth`

| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/auth/login` | Login anonimo, rate-limited, crea cookie `auth_token`. |
| POST | `/api/auth/logout` | Cierra sesion y elimina cookie. |
| GET | `/api/auth/me` | Devuelve usuario actual y permisos. |

## Health

Controller: `backend/src/Kodvian.Core.Api/Controllers/HealthController.cs`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/health` | Health response de API. |
| GET | `/healthz` | Healthcheck mapeado en `Program.cs` para Railway. |

## Dashboard

Controller: `backend/src/Kodvian.Core.Api/Controllers/DashboardController.cs`

Base route: `/api/dashboard`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/dashboard/overview` | KPIs, tareas prioritarias, cobranzas proximas y movimientos recientes. |

## Clients

Controller: `backend/src/Kodvian.Core.Api/Controllers/ClientsController.cs`

Base route: `/api/clients`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/clients` | Listado paginado de clientes. |
| GET | `/api/clients/{id}` | Detalle de cliente. |
| POST | `/api/clients` | Alta de cliente. |
| PUT | `/api/clients/{id}` | Edicion de cliente. |
| PATCH | `/api/clients/{id}/status` | Cambio de estado. |

## Projects

Controller: `backend/src/Kodvian.Core.Api/Controllers/ProjectsController.cs`

Base route: `/api/projects`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/projects` | Listado paginado de proyectos. |
| GET | `/api/projects/{id}` | Detalle de proyecto. |
| GET | `/api/projects/lookups` | Datos auxiliares para formularios. `responsibles` devuelve analistas activos con `developerId` remunerable. |
| POST | `/api/projects` | Alta de proyecto. |
| PUT | `/api/projects/{id}` | Edicion de proyecto. |
| GET | `/api/projects/document-types` | Tipos de documento de proyecto. |
| GET | `/api/projects/{id}/documents` | Documentos del proyecto. |
| POST | `/api/projects/{id}/documents` | Alta de documento con PDF. |
| POST | `/api/projects/{id}/documents/{documentId}/versions` | Nueva version de documento. |
| GET | `/api/projects/{id}/documents/{documentId}/versions` | Versiones de un documento. |
| GET | `/api/projects/{id}/documents/{documentId}` | Descarga version vigente. |
| GET | `/api/projects/{id}/documents/{documentId}/versions/{versionId}` | Descarga version especifica. |
| DELETE | `/api/projects/{id}/documents/{documentId}` | Baja logica de documento. |

## Tasks

Controller: `backend/src/Kodvian.Core.Api/Controllers/TasksController.cs`

Base route: `/api/tasks`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/tasks` | Listado paginado de tareas. |
| GET | `/api/tasks/{id}` | Detalle de tarea. |
| GET | `/api/tasks/kanban` | Tareas agrupadas para kanban. |
| GET | `/api/tasks/lookups` | Datos auxiliares para formularios. |
| POST | `/api/tasks` | Alta de tarea. |
| PUT | `/api/tasks/{id}` | Edicion de tarea. |
| PATCH | `/api/tasks/{id}/status` | Cambio de estado. |

## My Work

Controller: `backend/src/Kodvian.Core.Api/Controllers/MyWorkController.cs`

Base route: `/api/my-work`

Endpoints para usuarios con rol `Desarrollador`, claim `developer_id` y (donde aplica) `userId` (`NameIdentifier`).

### GitHub — repos e issues

| Metodo | Ruta | Policy | Descripcion |
|---|---|---|---|
| GET | `/api/my-work/overview` | `DeveloperWorkRead` | KPIs, preview de repos e issues. Incluye `githubNotConnected`. |
| GET | `/api/my-work/repositories` | `DeveloperWorkRead` | Repos GitHub de proyectos accesibles (paginado). |
| GET | `/api/my-work/issues` | `DeveloperWorkRead` | Issues desde `GitHubIssueLink` (paginado, filtros). |
| POST | `/api/my-work/issues` | `DeveloperIssuesWrite` | Crea issue en GitHub y persiste `GitHubIssueLink`. |
| PATCH | `/api/my-work/issues/{id}/status` | `DeveloperTasksStatusWrite` | `{id}` = GUID de `GitHubIssueLink`. Cierra/reabre en GitHub. |
| POST | `/api/my-work/sync` | `DeveloperWorkRead` | Import/update issues asignadas al dev. Body opcional: `{ "projectId": "guid" }`. |

### Tareas internas (legacy)

| Metodo | Ruta | Policy | Descripcion |
|---|---|---|---|
| GET | `/api/my-work/projects` | `DeveloperWorkRead` | Proyectos asociados por contrato, assignment o tarea. |
| GET | `/api/my-work/tasks` | `DeveloperWorkRead` | Tareas `TaskItem` asignadas al desarrollador. |
| GET | `/api/my-work/tasks/kanban` | `DeveloperWorkRead` | Tareas propias agrupadas para tablero kanban. |
| GET | `/api/my-work/tasks/{id}` | `DeveloperWorkRead` | Detalle de tarea propia. |
| PATCH | `/api/my-work/tasks/{id}/status` | `DeveloperTasksStatusWrite` | Cambio de estado de tarea interna propia. |

## Profile

Controller: `backend/src/Kodvian.Core.Api/Controllers/ProfileController.cs`

Base route: `/api/profile`

| Metodo | Ruta | Auth | Descripcion |
|---|---|---|---|
| GET | `/api/profile` | JWT | Perfil del usuario + estado GitHub. |
| GET | `/api/profile/github/connect` | JWT | Redirect a GitHub OAuth (genera `GitHubOAuthState`). |
| GET | `/api/profile/github/callback` | Anonimo | Callback OAuth; resuelve usuario por `state` en DB. |
| DELETE | `/api/profile/github/disconnect` | JWT | Limpia tokens y username GitHub. |

## Webhooks

Controller: `backend/src/Kodvian.Core.Api/Controllers/GitHubWebhookController.cs`

Base route: `/api/webhooks`

| Metodo | Ruta | Auth | Descripcion |
|---|---|---|---|
| POST | `/api/webhooks/github` | Anonimo + firma | Recibe eventos `issues` de GitHub. Valida `X-Hub-Signature-256` con `GitHub__WebhookSecret`. Firma invalida → `401`. |

Eventos procesados: `opened`, `closed`, `reopened`, `edited`. Ignora PRs y repos no vinculados a un `Project`.

## Financial Movements

Controller: `backend/src/Kodvian.Core.Api/Controllers/FinancialMovementsController.cs`

Base route: `/api/financial-movements`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/financial-movements` | Listado paginado de movimientos. |
| GET | `/api/financial-movements/{id}` | Detalle de movimiento. |
| GET | `/api/financial-movements/monthly-summary` | Resumen mensual financiero. |
| GET | `/api/financial-movements/lookups` | Datos auxiliares de finanzas. |
| POST | `/api/financial-movements` | Alta de movimiento. |
| PUT | `/api/financial-movements/{id}` | Edicion de movimiento. |
| GET | `/api/financial-movements/{id}/receipts` | Comprobantes del movimiento. |
| POST | `/api/financial-movements/{id}/receipts` | Upload de comprobante PDF. |
| GET | `/api/financial-movements/{id}/receipts/{receiptId}` | Descarga de comprobante. |
| DELETE | `/api/financial-movements/{id}/receipts/{receiptId}` | Eliminacion de comprobante. |

## Financial Categories

Controller: `backend/src/Kodvian.Core.Api/Controllers/FinancialCategoriesController.cs`

Base route: `/api/financial-categories`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/financial-categories` | Lista categorias financieras. |
| POST | `/api/financial-categories` | Alta de categoria. |
| PUT | `/api/financial-categories/{id}` | Edicion de categoria. |

## Providers

Controller: `backend/src/Kodvian.Core.Api/Controllers/ProvidersController.cs`

Base route: `/api/providers`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/providers` | Lista proveedores. |
| POST | `/api/providers` | Alta de proveedor. |
| PUT | `/api/providers/{id}` | Edicion de proveedor. |

## Developers

Controller: `backend/src/Kodvian.Core.Api/Controllers/DevelopersController.cs`

Base route: `/api/developers`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/developers` | Lista desarrolladores reales, incluyendo estado de acceso al sistema. Excluye perfiles remunerables de analistas. |
| GET | `/api/developers/{id}/contracts-summary` | Resumen anual de contratos por desarrollador. Solo administrador. |
| POST | `/api/developers` | Alta de desarrollador, opcionalmente con usuario de acceso. |
| PUT | `/api/developers/{id}` | Edicion de desarrollador y configuracion de acceso. |

## Team Users

Controller: `backend/src/Kodvian.Core.Api/Controllers/TeamUsersController.cs`

Base route: `/api/team/users`

Usuarios internos del modulo Equipo. Actualmente expone gestion de analistas y crea/sincroniza su perfil `Developer` remunerable.

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/team/users/analysts` | Lista usuarios con rol Analista, incluyendo `developerId` si tiene perfil remunerable. |
| POST | `/api/team/users/analysts` | Crea usuario Analista con contraseña inicial obligatoria y perfil remunerable asociado. |
| PUT | `/api/team/users/analysts/{id}` | Edita usuario Analista, permite cambiar contraseña y sincroniza/crea el perfil remunerable. |

## Developer Assignments

Controller: `backend/src/Kodvian.Core.Api/Controllers/ProjectDeveloperAssignmentsController.cs`

Base route: `/api`

Asignaciones operativas de equipo a proyecto, sin montos, porcentajes, modalidad de pago ni ledger.

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/projects/{projectId}/developer-assignments` | Equipo operativo asignado al proyecto. |
| POST | `/api/projects/{projectId}/developer-assignments` | Asigna un desarrollador al proyecto sin informacion economica. |
| DELETE | `/api/project-developer-assignments/{id}` | Baja logica de una asignacion operativa. |

## Developer Contracts

Controller: `backend/src/Kodvian.Core.Api/Controllers/ProjectDeveloperContractsController.cs`

Base route: `/api`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/projects/{projectId}/developer-contracts` | Contratos economicos de desarrolladores del proyecto. Solo administrador. |
| POST | `/api/projects/{projectId}/developer-contracts` | Alta de contrato. |
| PUT | `/api/developer-contracts/{id}` | Edicion de contrato. |
| GET | `/api/developer-contracts/{id}/ledger` | Ledger mensual del contrato. |

## Developer Payments

Controller: `backend/src/Kodvian.Core.Api/Controllers/DeveloperPaymentsController.cs`

Base route: `/api`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/developer-contracts/{contractId}/payments` | Pagos de un contrato. Solo administrador. |
| POST | `/api/developer-contracts/{contractId}/payments` | Alta de pago. |
| GET | `/api/developer-payments/{paymentId}/receipts` | Comprobantes de pago. |
| POST | `/api/developer-payments/{paymentId}/receipts` | Upload de comprobante PDF. |
| GET | `/api/developer-payments/{paymentId}/receipts/{receiptId}` | Descarga de comprobante. |
| DELETE | `/api/developer-payments/{paymentId}/receipts/{receiptId}` | Eliminacion de comprobante. |

## Locations

Controller: `backend/src/Kodvian.Core.Api/Controllers/LocationsController.cs`

Base route: `/api/locations`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/locations/countries` | Lista paises. |
| GET | `/api/locations/regions` | Lista regiones por pais. |
| GET | `/api/locations/cities` | Lista ciudades por pais y region. |

## Users

Controller: `backend/src/Kodvian.Core.Api/Controllers/UsersController.cs`

Base route: `/api/users`

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/users` | Listado paginado de usuarios. Actualmente devuelve resultado vacio. |

## Notas de seguridad

- Los controllers principales usan policies por modulo.
- El rol `Desarrollador` consume `/api/my-work`; no debe usar endpoints generales de gestion.
- El rol `Analista` no accede a finanzas, contratos economicos, pagos ni ledger.
- Para nuevos endpoints sensibles, agregar policy backend aunque el frontend oculte botones.
