# Mi Trabajo

## Resumen funcional

Modulo para usuarios con rol `Desarrollador`. Muestra repositorios GitHub vinculados a proyectos Kodvian a los que el dev tiene acceso, e issues sincronizadas desde GitHub.

Convive con el modulo `/tareas` (tareas internas `TaskItem`). La UI de `/mi-trabajo` usa **issues de GitHub** (`GitHubIssueLink`), no tareas internas.

## Flujos principales

- Ver KPIs: repos asignados, issues totales y abiertas.
- Listar repositorios GitHub de proyectos accesibles (assignment, contrato o tarea).
- Listar issues sincronizadas de esos repos.
- **Sincronizar** issues desde GitHub (import/update manual).
- **Crear issue** en GitHub desde un repo asignado (permiso `developer.issues.write`).
- **Cambiar estado** Abierta/Cerrada en Kodvian y reflejar en GitHub (permiso `developer.tasks.status.write`).
- Conectar cuenta GitHub en `/mi-perfil` (requisito para sync y crear issues).

## Pantallas frontend

- Ruta: `/mi-trabajo`.
- Page: `frontend/src/app/modules/mi-trabajo/mi-trabajo-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/mi-trabajo/services/mi-trabajo.service.ts`.
- Models: `frontend/src/app/modules/mi-trabajo/models/mi-trabajo.models.ts`.
- Dialog crear issue: `components/nueva-tarea-github-dialog/`.

## Contratos backend

### Issues y repos GitHub (UI actual)

| Metodo | Ruta | Policy | Descripcion |
|---|---|---|---|
| GET | `/api/my-work/overview` | `DeveloperWorkRead` | KPIs + preview repos/issues. Flag `githubNotConnected`. |
| GET | `/api/my-work/repositories` | `DeveloperWorkRead` | Repos paginados de proyectos accesibles con GitHub. |
| GET | `/api/my-work/issues` | `DeveloperWorkRead` | Issues paginadas desde `GitHubIssueLink`. |
| POST | `/api/my-work/issues` | `DeveloperIssuesWrite` | Crea issue en GitHub + persiste link local. |
| PATCH | `/api/my-work/issues/{id}/status` | `DeveloperTasksStatusWrite` | `{id}` = GUID de `GitHubIssueLink`. Sync a GitHub. |
| POST | `/api/my-work/sync` | `DeveloperWorkRead` | Import/update issues asignadas al dev en GitHub. |

### Tareas internas (legacy, sin UI en `/mi-trabajo`)

| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/my-work/projects` | Proyectos asociados. |
| GET | `/api/my-work/tasks` | Tareas `TaskItem` asignadas. |
| GET | `/api/my-work/tasks/kanban` | Kanban de tareas internas. |
| GET | `/api/my-work/tasks/{id}` | Detalle tarea interna. |
| PATCH | `/api/my-work/tasks/{id}/status` | Cambio estado tarea interna. |

### Perfil y OAuth

Ver `docs/modules/mi-perfil` implicito en frontend: `GET /api/profile`, `GET /api/profile/github/connect`, `GET /api/profile/github/callback` (anonimo), `DELETE /api/profile/github/disconnect`.

### Webhook (sync inverso)

`POST /api/webhooks/github` — anonimo, valida `X-Hub-Signature-256`. Ver `docs/backend/api-reference.md`.

Archivos backend:

- Controller: `MyWorkController.cs`, `ProfileController.cs`, `GitHubWebhookController.cs`.
- Services: `MyWorkService.cs`, `GitHubIssueSyncService.cs`, `GitHubWebhookService.cs`, `ProfileService.cs`.
- Abstractions: `IMyWorkService.cs`, `IGitHubIssueSyncService.cs`, `IGitHubWebhookService.cs`.
- DTOs: `backend/src/Kodvian.Core.Application/MyWork/**`, `Integrations/GitHub/**`.

## Permisos

| Codigo | Uso |
|---|---|
| `developer.work.read` | Acceso al modulo, listar repos/issues, sincronizar. |
| `developer.issues.write` | Boton "Nueva tarea" y `POST /api/my-work/issues`. |
| `developer.tasks.status.write` | Select estado issue y `PATCH /api/my-work/issues/{id}/status`. |

## Reglas de negocio

- El usuario debe tener `DeveloperId` vinculado (`developer_id` claim).
- **Acceso a proyecto:** assignment activo OR contrato activo OR tarea activa asignada al dev.
- **Repos visibles:** proyecto accesible + `GitHubOwner`/`GitHubRepoName` configurados.
- **Issues visibles:** `GitHubIssueLink` activas en proyectos accesibles (mismo criterio para listado y cambio de estado).
- **Cambio de estado:** cualquier dev con acceso al proyecto y permiso `developer.tasks.status.write` puede cerrar/reabrir; `DeveloperId` en el link solo indica quién importó/creó el registro.
- **Crear/sync:** requiere GitHub conectado en `/mi-perfil` (token OAuth del dev).
- **Anti-loop sync:** `SyncDirection` + `LastSyncedAt` (ventana 30s) evita eco entre Kodvian y webhook.
- Un repo GitHub solo puede vincularse a un proyecto Kodvian.
- **Prioridad (`Priority`):** metadata solo en Kodvian; GitHub Issues no tiene campo nativo de prioridad en el MVP (no se envía al crear la issue).

## Sincronizacion

| Direccion | Mecanismo |
|---|---|
| GitHub → Kodvian | Webhook `issues.*` o boton **Sincronizar** (`POST /api/my-work/sync`). |
| Kodvian → GitHub | Crear issue (`POST /api/my-work/issues`) o cambiar estado (`PATCH .../status`). |
| Post-OAuth | Sync automatico al conectar GitHub en perfil (fallo no bloquea la conexion; el dev puede usar **Sincronizar**). |

Fallback si el webhook no esta configurado: boton **Sincronizar** en la UI.

## Tests

- Backend: `MyWorkIssuesTests`, `MyWorkRepositoriesTests`, `MyWorkCreateIssueTests`, `MyWorkUpdateIssueStatusTests`, `GitHubIssueSyncServiceTests`, `GitHubWebhookServiceTests`, `GitHubWebhookControllerIntegrationTests`.
- Frontend: `mi-trabajo.service.spec.ts`, `nueva-tarea-github-dialog.component.spec.ts`.

## Referencias

- Plan maestro: [github-integration-plan.md](../development/github-integration-plan.md).
- Checklist manual: [validation-checklist.md](../development/validation-checklist.md).
