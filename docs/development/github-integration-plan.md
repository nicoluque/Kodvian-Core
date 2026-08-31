# Plan de integracion GitHub — Mi trabajo

Plan maestro para conectar `/mi-trabajo` con repositorios e issues de GitHub.

Cada **PLAN-XX** es una unidad de trabajo independiente para un agente. Ejecutar en orden salvo indicacion contraria. No avanzar al plan siguiente hasta cumplir los criterios de aceptacion.

## Decisiones de producto (cerradas)

| Tema | Decision |
|---|---|
| Unidad en GitHub | Repositorios (no proyectos GitHub) |
| Vinculo negocio ↔ codigo | `Project` Kodvian → 1 repo GitHub (admin configura); 1 repo no puede vincularse a 2 proyectos |
| Quien vincula el repo | Solo **Administrador** desde `/proyectos` |
| Tareas internas | Conviven: `/tareas` usa `TaskItem`; `/mi-trabajo` usa issues GitHub |
| Auth GitHub | OAuth App classic por desarrollador (token no expira salvo revocacion) |
| Sync | Bidireccional (Kodvian ↔ GitHub) |
| Issues existentes | Importar desde GitHub |
| Username GitHub | Dev conecta cuenta en `/mi-perfil` |
| Permiso crear issues | `developer.issues.write` (nuevo) |

## Arquitectura objetivo

```text
Admin (/proyectos) — solo Administrador
  → vincula Project ↔ GitHub repo (unico por repo)

Dev (/mi-perfil)
  → OAuth GitHub → tokens encriptados en User

Dev (/mi-trabajo)
  → repos asignados (misma logica que MyWorkService: assignment OR contract OR task)
  → crear/listar/sync issues (GitHubIssueLink)

GitHub
  → issues reales
  → webhooks por org o por repo → Kodvian (sync inverso)
```

## Convenciones por plan

- **Depende de:** prerequisitos
- **Salida:** entregable concreto
- **Criterios de aceptacion:** checklist verificable
- **Fuera de alcance:** evitar scope creep

## Mapa de dependencias

```text
PLAN-01 (DB)
  ├── PLAN-02 (GitHub API)
  ├── PLAN-03 (Encryption)
  │     └── PLAN-04 (OAuth BE)
  │           ├── PLAN-05 (Perfil FE)
  │           └── PLAN-13 (Token helper)
  ├── PLAN-06 (Admin repo)
  │
PLAN-07 (List repos) ← PLAN-04 + PLAN-06
PLAN-09 (List issues) ← PLAN-01 + PLAN-07
PLAN-10 (Import) ← PLAN-02 + PLAN-04 + PLAN-07 + PLAN-13
PLAN-08 (Create issue) ← PLAN-02 + PLAN-04 + PLAN-07 + PLAN-13
PLAN-11 (Sync → GitHub) ← PLAN-08 + PLAN-09
PLAN-12 (Webhook) ← PLAN-10 + PLAN-11
PLAN-14 (Docs) ← todo
```

## Orden de ejecucion recomendado

| # | Plan | Nombre |
|---|---|---|
| 1 | PLAN-01 | Modelo de datos |
| 2 | PLAN-02 | GitHub API service |
| 3 | PLAN-03 | Encriptacion tokens |
| 4 | PLAN-04 | OAuth backend |
| 5 | PLAN-05 | Mi perfil frontend |
| 6 | PLAN-06 | Admin vincular repo |
| 7 | PLAN-13 | Token helper |
| 8 | PLAN-07 | Listar repos asignados |
| 9 | PLAN-09 | Listar issues (puede arrancar vacio) |
| 10 | PLAN-10 | Import issues |
| 11 | PLAN-08 | Crear issue |
| 12 | PLAN-11 | Sync estados → GitHub |
| 13 | PLAN-12 | Webhook ← GitHub |
| 14 | PLAN-14 | Documentacion |

---

## PLAN-01 — Modelo de datos y migraciones

**Depende de:** nada  
**Salida:** esquema DB listo

### Objetivo

Crear la base de datos para repos GitHub, tokens OAuth, state CSRF e issues vinculadas.

### Tareas

1. Agregar campos a `Project`:
   - `GitHubOwner`, `GitHubRepoName`, `GitHubRepoId`, `GitHubRepoUrl`
2. Agregar campos a `User` (MVP OAuth App classic — **sin refresh token**):
   - `GitHubUsername`, `GitHubUserId`
   - `GitHubAccessTokenEncrypted`
   - `GitHubConnectedAt`
   - No agregar `GitHubRefreshTokenEncrypted` ni `GitHubTokenExpiresAt` en MVP
3. Crear entidad `GitHubIssueLink`:
   - `ProjectId`, `DeveloperId`
   - `GitHubIssueNumber`, `GitHubIssueNodeId`, `GitHubIssueUrl`
   - `Title`, `Description`, `Status`, `Priority`
   - `AssignedGitHubUsername`, `LastSyncedAt`, `SyncDirection`
4. Crear entidad `GitHubOAuthState` (CSRF OAuth — **persistido en DB**, no memoria):
   - `StateToken`, `UserId`, `ExpiresAt`, `CreatedAt`
5. Crear enums `GitHubIssueStatus` (Open, Closed) y `SyncDirection` (None, FromGitHub, FromKodvian)
6. Configurar `KodvianDbContext`: relaciones, longitudes, indices
   - Unique: `(ProjectId, GitHubIssueNumber)`
   - Unique: `GitHubIssueNodeId`
   - Unique: `(GitHubOwner, GitHubRepoName)` en `Project` cuando repo configurado — **un repo no puede vincularse a dos proyectos**
   - Index: `(DeveloperId, Status)`
   - Index: `GitHubOAuthState.StateToken`
7. Generar migration EF Core
8. Actualizar `docs/backend/data-model.md`

### Fuera de alcance

- Servicios, controllers, UI

### Criterios de aceptacion

- [ ] `dotnet build backend/Kodvian.Core.slnx` OK
- [ ] Migration aplica sin errores
- [ ] FKs e indices correctos incluyendo unique repo por proyecto
- [ ] `data-model.md` actualizado

---

## PLAN-02 — Infraestructura GitHub API

**Depende de:** PLAN-01  
**Salida:** servicio HTTP reutilizable para GitHub REST API

### Objetivo

Capa de integracion GitHub sin exponer endpoints de negocio aun.

### Tareas

1. Crear `GitHubOptions`:
   - `Enabled` (bool, default false en Development)
   - `ClientId`, `ClientSecret`, `CallbackUrl`, `WebhookSecret`, `ApiBaseUrl`, `DefaultLabel`, `ServiceToken`
2. Crear `IGitHubApiService` en Application:
   - `ValidateRepositoryAsync(owner, repo, token?)`
   - `GetRepositoryAsync(owner, repo, token)`
   - `CreateIssueAsync(...)`, `UpdateIssueAsync(...)`, `ListIssuesAsync(...)`
   - `GetAuthenticatedUserAsync(token)`
   - `ExchangeCodeForTokenAsync(code)`
3. Implementar `GitHubApiService` con typed HttpClient
4. DTOs GitHub en `Application/Integrations/GitHub/**`
5. Registrar DI en `DependencyInjectionExtensions`
6. **Arranque degradado:** si `GitHub__Enabled=false` o faltan credenciales, la app arranca igual; endpoints GitHub devuelven error claro. Fail-fast solo si `GitHub__Enabled=true` y faltan credenciales obligatorias en Production
7. Documentar variables en `README.md` y `docs/railway-readiness.md`

### Notas tecnicas

- Manejar 401, 403, 404, 422, 429 (rate limit)
- User-Agent identificable para GitHub API

### Criterios de aceptacion

- [ ] Servicio registrado en DI
- [ ] App arranca sin credenciales GitHub cuando `Enabled=false`
- [ ] Unit tests con handler HTTP mock (minimo: validate repo, create issue, list issues)
- [ ] Variables documentadas en Railway readiness

---

## PLAN-03 — Encriptacion de tokens OAuth

**Depende de:** PLAN-01  
**Salida:** utilidad para persistir tokens sin plain text

### Tareas

1. `ITokenEncryptionService` + `TokenEncryptionService` (AES-256)
2. `TokenEncryption__Key` en configuracion
3. Fail-fast en Production si `GitHub__Enabled=true` y falta key
4. Unit tests: round-trip, key invalida

### Criterios de aceptacion

- [ ] Ningun token OAuth se guarda en plain text
- [ ] Tests pasan
- [ ] Key documentada en Railway readiness

---

## PLAN-04 — OAuth GitHub backend

**Depende de:** PLAN-02, PLAN-03  
**Salida:** conectar/desconectar cuenta GitHub del dev

### Tareas

1. `IProfileService` / `ProfileService`
2. `ProfileController`:
   - `GET /api/profile` — `[Authorize]`; datos usuario + estado GitHub
   - `GET /api/profile/github/connect` — `[Authorize]`; genera `state` firmado con `userId`, persiste en `GitHubOAuthState`, redirect a GitHub
   - `GET /api/profile/github/callback` — **`[AllowAnonymous]`**; valida `state` desde DB (contiene `userId`), intercambia code, guarda token encriptado + username en `User`; **no depender de cookie JWT** (SameSite=Strict bloquea cookie en redirect cross-site desde GitHub)
   - `DELETE /api/profile/github/disconnect` — `[Authorize]`
3. Persistir tokens encriptados y metadata en `User` identificado por `userId` del state
4. Redirect post-callback a frontend `/mi-perfil?connected=true`
5. Eliminar `GitHubOAuthState` usado tras callback exitoso
6. **Hook post-conexion:** invocar `SyncIssuesFromGitHubAsync` (implementado en PLAN-10) al finalizar callback exitoso; si PLAN-10 no existe aun, dejar interfaz `IGitHubIssueSyncService` stub y conectar en PLAN-10
7. **Tests unitarios:** connect, disconnect, state invalido/expirado
8. **Tests integracion HTTP:** callback anonimo con state valido persiste token; callback sin state → 400; connect requiere auth

### Notas tecnicas

- Callback es ruta API (`/api/profile/github/callback`); no la intercepta el SPA fallback
- Scope OAuth: `read:user repo`
- Registrar GitHub OAuth App con callback URL prod y dev
- Ver `docs/backend/authentication-authorization.md`: cookie `auth_token` SameSite=Strict

### Criterios de aceptacion

- [ ] Flujo OAuth completo sin depender de cookie en callback
- [ ] State persistido en DB, no memoria
- [ ] Disconnect limpia campos GitHub del usuario
- [ ] Integration tests HTTP del flujo OAuth pasan

---

## PLAN-05 — Pantalla Mi perfil (frontend)

**Depende de:** PLAN-04  
**Salida:** dev conecta/desconecta GitHub desde UI

### Tareas

1. Modulo `mi-perfil`: page, service, models
2. Registrar ruta lazy `/mi-perfil` en [`app.routes.ts`](frontend/src/app/app.routes.ts) con `authGuard`
3. Actualizar [`navigation.service.ts`](frontend/src/app/core/services/navigation.service.ts): link "Mi perfil" visible para rol `Desarrollador`
4. UI: estado conexion, @username, conectar (redirect a `/api/profile/github/connect`), desconectar
5. Manejo de `?connected=true` post-OAuth
6. Specs: service + estados UI

### Criterios de aceptacion

- [x] Ruta `/mi-perfil` registrada y navegable
- [x] Conectar/desconectar GitHub funciona
- [x] Textos en espanol latino
- [x] Specs pasan

---

## PLAN-06 — Admin: vincular repo GitHub a proyecto

**Depende de:** PLAN-02  
**Salida:** solo administrador configura repo por proyecto Kodvian

### Backend

1. Extender `ProjectService`:
   - `LinkGitHubRepositoryAsync` — validar que `(owner, repo)` no este ya vinculado a otro proyecto
   - `UnlinkGitHubRepositoryAsync`
   - `ValidateGitHubRepositoryAsync`
2. Endpoints en `ProjectsController`:
   - `PUT /api/projects/{id}/github-repo`
   - `DELETE /api/projects/{id}/github-repo`
   - `POST /api/projects/{id}/github-repo/validate`
3. Policy **`AdministratorOnly`** (no `projects.write` — Operativo/Analista no deben vincular repos)
4. Validacion remota con `GitHub__ServiceToken`

### Frontend

1. Seccion "Repositorio GitHub" en `proyecto-form-dialog` — **visible solo si rol Administrador**
2. Mostrar repo en `proyecto-detail-dialog`
3. Actualizar models y `proyectos.service.ts`

### Criterios de aceptacion

- [x] Solo Administrador puede vincular/desvincular/validar
- [x] Rechaza vincular repo ya usado por otro proyecto
- [x] Validacion rechaza repo inexistente
- [x] Tests backend de autorizacion pasan

---

## PLAN-13 — Token helper para llamadas GitHub

**Depende de:** PLAN-03, PLAN-04  
**Salida:** helper reutilizable para obtener token valido del dev

### Tareas

1. `IGitHubTokenProvider` / `GitHubTokenProvider`
2. `GetValidTokenAsync(userId)`:
   - Desencriptar `GitHubAccessTokenEncrypted`
   - OAuth App classic: no hay refresh; si token ausente o revocado (401 de GitHub) → marcar desconectado y error accionable
3. Usar en todos los servicios que llamen GitHub en nombre del dev

### Criterios de aceptacion

- [x] Servicios de negocio no desencriptan tokens directamente
- [x] Token revocado → mensaje "Reconectá GitHub en Mi perfil"
- [x] Tests pasan

---

## PLAN-07 — `/mi-trabajo`: listar repos asignados

**Depende de:** PLAN-01, PLAN-04, PLAN-06  
**Salida:** "Mis proyectos" muestra repos filtrados por Kodvian

### Backend

1. DTO `MyWorkRepositoryListItemDto`
2. `GetAssignedRepositoriesAsync(developerId, userId)` en `MyWorkService`
3. **Filtro alineado con `MyWorkService.BuildProjectsQuery` actual:**
   - Proyecto activo con repo GitHub configurado Y
   - (`ProjectDeveloperAssignment` activa OR `ProjectDeveloperContract` activo OR tarea `TaskItem` asignada al dev)
4. `GET /api/my-work/repositories` (paginado)
5. Flag `githubNotConnected` si el `User` (via `userId` del claim `NameIdentifier`) no tiene `GitHubConnectedAt`
6. Extender `MyWorkController`: `TryGetUserId()` ademas de `TryGetDeveloperId()`
7. Tests: dev con assignment ve repo; dev con solo contrato ve repo; dev sin acceso no ve; sin repo configurado no aparece

### Frontend

1. Tabla repos: Repo, Cliente, Estado, Issues abiertas, Ver en GitHub
2. Empty states: sin GitHub → CTA `/mi-perfil`; sin repos → mensaje claro

### Criterios de aceptacion

- [x] Misma logica de acceso que MyWorkService actual
- [x] No lista todos los repos del colaborador en GitHub
- [x] Tests pasan

---

## PLAN-09 — Listar issues en `/mi-trabajo`

**Depende de:** PLAN-01, PLAN-07  
**Salida:** tabla Mis tareas con issues GitHub (puede estar vacia hasta PLAN-10)

### Backend

1. DTO `MyWorkIssueListItemDto`
2. `GetIssuesAsync(developerId, filters)` paginado desde `GitHubIssueLink`
3. `GET /api/my-work/issues`
4. Actualizar `GET /api/my-work/overview` para repos + issues GitHub
5. Mantener endpoints legacy `/api/my-work/tasks/*` sin romper contratos

### Frontend

1. Tabla: Tarea, Repo, Estado (Abierta/Cerrada), Prioridad, Creada, Acciones
2. KPIs: repos, issues totales, issues abiertas
3. Remover uso de `TaskItem` en UI de `/mi-trabajo`

### Criterios de aceptacion

- [x] Tabla desde DB (vacía OK antes de import)
- [x] KPIs correctos
- [x] Overview actualizado
- [x] Tests pasan

---

## PLAN-10 — Importar issues existentes

**Depende de:** PLAN-02, PLAN-04, PLAN-07, PLAN-09, PLAN-13  
**Salida:** issues historicas visibles en Kodvian

### Backend

1. `IGitHubIssueSyncService` / `GitHubIssueSyncService` con `SyncIssuesFromGitHubAsync(developerId, userId, projectId?)`
2. Listar issues por repo asignado: `assignee={username}`, `state=all`
3. Ignorar items con `pull_request`
4. Upsert por `GitHubIssueNodeId`
5. `POST /api/my-work/sync`
6. **Conectar hook de PLAN-04:** invocar sync en callback OAuth exitoso (primera conexion)
7. Tests: import, update, ignora PRs, ignora repos no asignados

### Frontend

1. Boton "Sincronizar"
2. Loading + snackbar con resultado

### Criterios de aceptacion

- [x] Issues existentes importadas
- [x] Sync automatico post-OAuth funciona
- [x] Sin duplicados
- [x] Tests pasan

---

## PLAN-08 — Crear issue desde `/mi-trabajo`

**Depende de:** PLAN-02, PLAN-04, PLAN-07, PLAN-13  
**Salida:** crear issue en GitHub desde Kodvian

### Backend

1. `CreateMyWorkIssueRequestDto`
2. `CreateIssueAsync(developerId, userId, request)` en `MyWorkService`
3. `POST /api/my-work/issues`
4. **Permiso `developer.issues.write` — pasos obligatorios:**
   - Agregar constante en [`PermissionCodes.cs`](backend/src/Kodvian.Core.Application/Common/Security/PermissionCodes.cs)
   - Agregar a rol `Desarrollador` en [`RolePermissionMap.cs`](backend/src/Kodvian.Core.Application/Common/Security/RolePermissionMap.cs)
   - Crear policy `DeveloperIssuesWrite` en [`Program.cs`](backend/src/Kodvian.Core.Api/Program.cs)
   - Aplicar `[Authorize(Policy = "DeveloperIssuesWrite")]` en endpoint
   - Actualizar [`RolePermissionMapTests.cs`](backend/tests/Kodvian.Core.Application.Tests/RolePermissionMapTests.cs) — Desarrollador pasa de 2 a 3 permisos
   - Frontend: validar permiso antes de mostrar boton "Nueva tarea"
5. `MyWorkController`: resolver `userId` (`NameIdentifier`) + `developerId` (`developer_id` claim)
6. Tests: exito, no asignado, sin GitHub, sin permiso

### Frontend

1. Dialog `nueva-tarea-github-dialog`
2. Boton "Nueva tarea" (solo con `developer.issues.write`)
3. Specs

### Criterios de aceptacion

- [x] Issue creada en GitHub y en DB
- [x] Permiso wired end-to-end (backend + frontend + tests)
- [x] Tests pasan

---

## PLAN-11 — Sync estados Kodvian → GitHub

**Depende de:** PLAN-08, PLAN-09  
**Salida:** cerrar/reabrir issue en Kodvian refleja en GitHub

### Backend

1. `UpdateMyWorkIssueStatusRequestDto`
2. `UpdateIssueStatusAsync(developerId, userId, issueLinkId, request)`
3. **`{id}` en ruta = GUID de `GitHubIssueLink.Id`** (no issue number ni projectId)
4. `PATCH /api/my-work/issues/{id}/status` donde `{id:guid}` es `GitHubIssueLink.Id`
5. Policy `developer.tasks.status.write`
6. Anti-loop: `SyncDirection = FromKodvian` + ventana 30s en `LastSyncedAt`
7. Tests incluyendo ownership por `DeveloperId`

### Frontend

1. Select/toggle Abierta/Cerrada
2. Confirmacion al cerrar

### Criterios de aceptacion

- [x] Close/reopen sincroniza a GitHub
- [x] `{id}` documentado y consistente como GUID de GitHubIssueLink
- [x] Anti-loop funciona
- [x] Tests pasan

---

## PLAN-12 — Webhook GitHub → Kodvian

**Depende de:** PLAN-10, PLAN-11  
**Salida:** sync inverso desde GitHub

### Backend

1. `GitHubWebhookController`: `POST /api/webhooks/github`
2. `[AllowAnonymous]` + validacion `X-Hub-Signature-256`
3. Eventos: `issues.opened`, `issues.closed`, `issues.reopened`, `issues.edited`
4. Resolver `Project` por `(GitHubOwner, GitHubRepoName)` del payload
5. Anti-loop con `SyncDirection`
6. **Tests integracion HTTP:** signature valida/invalida, cada evento, repo no vinculado ignorado

### Pasos operativos (documentar en PLAN-14)

GitHub requiere registrar webhook **por repositorio** (o usar **org webhook** si todos los repos estan en la misma org):

1. En GitHub org `kodvian-solutions` (o cada repo vinculado): Settings → Webhooks → Add
2. Payload URL: `https://{dominio-railway}/api/webhooks/github`
3. Content type: `application/json`
4. Secret: valor de `GitHub__WebhookSecret`
5. Events: `Issues`
6. Al vincular nuevo repo en PLAN-06, checklist manual: verificar que el repo esta cubierto por org webhook o agregar webhook al repo

### Criterios de aceptacion

- [x] Cambios en GitHub se reflejan en Kodvian
- [x] Signature invalida → 401
- [x] Integration tests HTTP pasan
- [x] Pasos operativos de webhook documentados

---

## PLAN-14 — Documentacion y checklist final

**Depende de:** todos
**Salida:** feature documentada para deploy

### Tareas

1. Reescribir `docs/modules/mi-trabajo.md`
2. Actualizar `docs/backend/api-reference.md`
3. Actualizar `docs/backend/modules.md`, `docs/frontend/modules.md`
4. Actualizar `docs/backend/authentication-authorization.md` (OAuth callback anonimo + SameSite)
5. Checklist manual en `docs/development/validation-checklist.md`:
   - Conectar GitHub (OAuth)
   - Vincular repo (admin)
   - Import sync
   - Crear issue
   - Cerrar en Kodvian → GitHub
   - Cerrar en GitHub → Kodvian (webhook)
   - Setup webhook org/repo
6. Variables Railway completas incluyendo `GitHub__Enabled`

### Criterios de aceptacion

- [x] Docs reflejan comportamiento real
- [x] Checklist manual usable
- [x] Env vars documentadas

---

## Variables de entorno (consolidado)

| Variable | Requerida (prod) | Descripcion |
|---|---|---|
| `GitHub__Enabled` | Si | `true` para activar integracion |
| `GitHub__ClientId` | Si (si Enabled) | OAuth App client ID |
| `GitHub__ClientSecret` | Si (si Enabled) | OAuth App client secret |
| `GitHub__CallbackUrl` | Si (si Enabled) | `https://dominio/api/profile/github/callback` |
| `GitHub__WebhookSecret` | Si (con webhooks) | Secret del webhook |
| `GitHub__ServiceToken` | Recomendado | PAT para validacion admin de repos |
| `GitHub__DefaultLabel` | No | Default `kodvian` |
| `TokenEncryption__Key` | Si (si Enabled) | Key AES para tokens OAuth |

---

## Riesgos y mitigaciones

| Riesgo | Mitigacion |
|---|---|
| OAuth callback + SameSite Strict | Callback `[AllowAnonymous]` + state con userId en DB (PLAN-04) |
| State CSRF en memoria Railway | Entidad `GitHubOAuthState` en DB (PLAN-01/04) |
| Dev sin GitHub conectado | Empty state + CTA perfil; flag `githubNotConnected` |
| Repo en 2 proyectos | Unique `(GitHubOwner, GitHubRepoName)` (PLAN-01) |
| Fail-fast bloquea deploy | `GitHub__Enabled` + arranque degradado (PLAN-02) |
| Operativo vincula repos | Policy `AdministratorOnly` (PLAN-06) |
| Filtro repos incompleto | Misma logica que `BuildProjectsQuery` (PLAN-07) |
| Loop sync bidireccional | `SyncDirection` + ventana 30s |
| Webhook no configurado | Checklist operativo por org/repo (PLAN-12/14) |
| Webhook no llega en Railway | Boton Sincronizar como fallback |
| Tokens en DB | Encriptacion at-rest (PLAN-03) |
| Endpoints legacy | Mantener `/api/my-work/tasks/*`; UI migra a issues |

---

## Como ejecutar con un agente

```text
Ejecuta PLAN-XX del documento docs/development/github-integration-plan.md.
Cumple tareas y criterios de aceptacion. No avances al plan siguiente.
```

---

## Revision del plan

### Verificacion inicial (2026-08-25)

- [x] Sin integracion GitHub previa
- [x] `ProjectDeveloperAssignment` existe
- [x] Login JWT/cookie propio + OAuth como capa adicional
- [x] Railway monolito

### Correcciones Bugbot (2026-08-25)

- [x] OAuth callback anonimo — no depender de cookie SameSite=Strict
- [x] State CSRF persistido en DB (`GitHubOAuthState`)
- [x] Permiso `developer.issues.write` con pasos explicitos en PLAN-08
- [x] Filtro repos alineado con `BuildProjectsQuery` (assignment + contract + task)
- [x] Webhook: pasos operativos org/repo en PLAN-12/14
- [x] Unique repo por proyecto — evitar colisiones NodeId
- [x] `GitHub__Enabled` — arranque degradado sin bloquear deploy
- [x] PLAN-06 usa `AdministratorOnly`
- [x] Sync post-OAuth: hook en PLAN-04, implementacion en PLAN-10
- [x] `MyWorkController` resuelve `userId` + `developerId`
- [x] PLAN-11: `{id}` = GUID de `GitHubIssueLink`
- [x] Integration tests OAuth/webhook en PLAN-04/12
- [x] Sin campos refresh token en MVP
- [x] PLAN-05 incluye routing y navigation
- [x] PLAN-09 depende de PLAN-01 + PLAN-07 (no PLAN-08)
