# GitHub Integration — Issues de review (borrador)

Orden de ataque: mayor severidad → menor. Marcar `[x]` al resolver.

---

## Alta

| # | Plan | Issue | Estado |
|---|------|-------|--------|
| 1 | PLAN-10 | Sync update reasigna `DeveloperId` al dev que sincroniza; puede robar ownership | [x] |

---

## Media

| # | Plan | Issue | Estado |
|---|------|-------|--------|
| 2 | PLAN-09 | Listado de issues por proyecto accesible, pero PATCH status exige `DeveloperId` — ver vs. editar inconsistente | [x] |
| 3 | PLAN-04 | Faltan integration tests HTTP OAuth (callback anónimo, connect auth, state inválido) | [x] |
| 4 | PLAN-12 | Tests HTTP webhook solo cubren `closed`; faltan `reopened` y `edited` a nivel HTTP | [x] |
| 5 | PLAN-07 | Falta test de acceso a repos solo vía `TaskItem` (assignment/contract sí cubiertos) | [x] |

---

## Baja

| # | Plan | Issue | Estado |
|---|------|-------|--------|
| 6 | PLAN-08 | `Priority` se guarda en DB pero no se envía a GitHub al crear issue | [x] |
| 7 | PLAN-08 | Sin test HTTP 403 en `POST /api/my-work/issues` sin permiso | [x] |
| 8 | PLAN-08 | Si GitHub crea issue y falla `SaveChanges`, queda issue huérfana en GitHub | [x] |
| 9 | PLAN-04 | Callback inválido redirige a frontend en lugar de HTTP 400 (desvío vs plan) | [x] |
| 10 | PLAN-04 | Sync post-OAuth falla → token guardado pero UX `connected=false&error=github` | [x] |
| 11 | PLAN-05 | `/mi-perfil` sin `permissionGuard` (cualquier autenticado puede entrar por URL) | [x] |
| 12 | PLAN-06 | Vincular/validar repo falla con `GitHub__Enabled=false` | [x] |
| 13 | PLAN-02 | Tests API sin cubrir 403/404/422/429 | [x] |
| 14 | PLAN-11 | Sin test HTTP 403 en PATCH status; anti-loop E2E no probado | [x] |
| 15 | PLAN-13 | Sin test de decrypt fallido que limpia conexión | [x] |

---

## Notas de decisión

- **#2 (PLAN-09):** Resuelto — modelo "issues del proyecto"; cualquier dev con acceso al proyecto puede cambiar estado (opción B).

### Baja — resoluciones

| # | Resolución |
|---|------------|
| 6 | **Por diseño (MVP).** GitHub Issues no tiene prioridad nativa; `Priority` es metadata Kodvian en `GitHubIssueLink`. Documentado en `mi-trabajo.md` + test unitario. |
| 7 | **Implementado.** `MyWorkControllerIntegrationTests.PostIssue_ReturnsForbidden_WithoutIssuesWritePermission`. |
| 8 | **Riesgo aceptado (MVP).** Compensación (rollback en GitHub) fuera de alcance; probabilidad baja. Mitigación: sync manual importa el link. |
| 9 | **Por diseño.** OAuth callback es flujo browser; redirect a `/mi-perfil?error=oauth` es UX correcta (PLAN-04 nota SameSite). HTTP 400 del borrador inicial no aplica a redirect cross-site. |
| 10 | **Corregido.** `ProfileService` trata sync post-OAuth como no fatal; conexión exitosa → `connected=true` aunque falle el sync inicial. |
| 11 | **Por diseño (PLAN-05).** Ruta con `authGuard` heredado; cualquier usuario autenticado puede gestionar su perfil/GitHub. Nav limita visibilidad a devs con `developer.work.read`. |
| 12 | **Por diseño (PLAN-02).** Admin repo requiere `GitHub__Enabled=true` + `ServiceToken`; test `ValidateGitHubRepositoryAsync_Throws_WhenGitHubIntegrationDisabled`. |
| 13 | **Implementado.** Tests 403/404/422/429 en `GitHubApiServiceTests`. |
| 14 | **Implementado.** HTTP 403 PATCH en `MyWorkControllerIntegrationTests`; anti-loop HTTP en `PostGitHubWebhook_IgnoresUpdate_WhenAntiLoopActive`. |
| 15 | **Implementado.** `GetValidTokenAsync_ClearsConnection_WhenDecryptFails` en `GitHubTokenProviderTests`. |
