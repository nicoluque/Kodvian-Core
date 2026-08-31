# Railway readiness — Kodvian Core

Checklist de pendientes e implementación para desplegar Kodvian Core en Railway.

**Arquitectura acordada**

- 1 servicio: la API .NET sirve el SPA Angular desde `wwwroot` (mismo origen).
- Archivos: object storage S3-compatible en Production; local en Development.
- Build: Dockerfile multi-stage en la raíz del repo.

## Estado

| # | Ítem | Estado | Notas |
|---|------|--------|-------|
| 1 | Empaquetado / Dockerfile / `.dockerignore` / `railway.toml` | Hecho | Multi-stage Node + .NET; entrypoint respeta `PORT` |
| 2 | SPA servido por la API (static + fallback) | Hecho | `wwwroot` + `MapFallbackToFile` |
| 3 | Proxy, HTTPS, CSP y cookies de auth | Hecho | Forwarded headers, sin HTTPS redirect en prod, CSP SPA |
| 4 | Storage S3-compatible | Hecho | `Provider=Local\|S3`, fail-fast al startup |
| 5 | Variables de entorno documentadas | Hecho | README + esta sección |
| 6 | Postgres + migraciones/seed | Listo (código) | Adjuntar plugin en Railway al deployar |
| 7 | Verificaciones packaging | Hecho (local) | Imagen `kodvian-core:local` con SPA en `/app/wwwroot` |

## 1. Empaquetado

- [x] `Dockerfile` multi-stage (Node 20 → Angular build → .NET publish → aspnet runtime)
- [x] `.dockerignore` (excluir `node_modules`, `bin`, `obj`, `.git`, etc.)
- [x] `railway.toml` con builder Dockerfile y healthcheck `/healthz`
- [x] `docker-entrypoint.sh` setea `ASPNETCORE_URLS` con `PORT`
- Artefacto Angular: `frontend/dist/frontend/browser` → `/app/wwwroot`

## 2. SPA servido por la API

- [x] `UseDefaultFiles` + `UseStaticFiles`
- [x] `MapFallbackToFile("index.html")` después de controllers y `/healthz`
- Los clients Angular ya usan paths relativos `/api/...` (sin cambios)

## 3. Proxy, HTTPS, CSP y cookies

- [x] `ForwardedHeaders` con `KnownNetworks` / `KnownProxies` vacíos
- [x] Production: no `UseHttpsRedirection` (TLS en Railway)
- [x] CSP compatible con SPA + Google Fonts / Material Icons
- [x] Cookie `auth_token` con `Secure=true` fuera de Development

## 4. Storage S3-compatible

- [x] Extender `StorageOptions` (`Provider`, bucket, creds, `ServiceUrl`, `Region`, `ForcePathStyle`)
- [x] `S3FileStorageService` + DI según `Storage:Provider`
- [x] Fail-fast al startup si `Provider=S3` y faltan settings
- [x] Development: `Local` + `App_Data/files`

## 5. Variables de entorno

### Runtime

| Variable | Requerida | Descripción |
|----------|-----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Sí | `Production` |
| `ASPNETCORE_URLS` | Sí | `http://0.0.0.0:${PORT}` (el entrypoint también usa `PORT`) |
| `DATABASE_URL` | Sí | Connection string del plugin Postgres Railway |

### Auth / JWT

| Variable | Requerida | Descripción |
|----------|-----------|-------------|
| `Jwt__Issuer` | Sí | Emisor JWT |
| `Jwt__Audience` | Sí | Audiencia JWT |
| `Jwt__Key` | Sí | ≥32 caracteres; no usar placeholder `SET_` |
| `Jwt__ExpirationMinutes` | No | Default 30 |
| `AuthSeed__AdminEmail` | Sí (prod) | Email admin inicial |
| `AuthSeed__AdminPassword` | Sí (prod) | Password seguro (no `Admin123*`) |
| `AuthSeed__AdminFullName` | Sí (prod) | Nombre del admin |

### CORS (opcional con mismo origen)

| Variable | Requerida | Descripción |
|----------|-----------|-------------|
| `Cors__AllowedOrigins` | No | Orígenes separados por coma |

### Storage

| Variable | Requerida | Descripción |
|----------|-----------|-------------|
| `Storage__Provider` | Sí (prod) | `S3` en Railway |
| `Storage__Bucket` | Si S3 | Nombre del bucket |
| `Storage__AccessKey` | Si S3 | Access key |
| `Storage__SecretKey` | Si S3 | Secret key |
| `Storage__ServiceUrl` | Si S3 | Endpoint (R2/MinIO/S3) |
| `Storage__Region` | Si S3 | Región (ej. `auto` en R2) |
| `Storage__ForcePathStyle` | Recomendado | `true` para R2/MinIO |
| `Storage__MaxPdfSizeMb` | No | Default 10 |

### GitHub (opcional)

| Variable | Requerida | Descripción |
|----------|-----------|-------------|
| `GitHub__Enabled` | No | `false` por defecto; la app arranca sin integración |
| `GitHub__ClientId` | Si Enabled en prod | OAuth App client ID |
| `GitHub__ClientSecret` | Si Enabled en prod | OAuth App client secret |
| `GitHub__CallbackUrl` | Si Enabled en prod | `https://dominio/api/profile/github/callback` |
| `GitHub__ServiceToken` | Recomendado | PAT para validación admin de repos |
| `GitHub__WebhookSecret` | Si webhooks | Secret del webhook |
| `GitHub__DefaultLabel` | No | Default `kodvian` |
| `GitHub__ApiBaseUrl` | No | Default `https://api.github.com` |
| `TokenEncryption__Key` | Si Enabled en prod | Base64 de 32 bytes o secreto ≥32 caracteres para AES-256-GCM |

Con `GitHub__Enabled=false` (o ausente) la API arranca normal. Con `Enabled=true` en Production faltan ClientId/ClientSecret/CallbackUrl/TokenEncryption__Key → fail-fast al startup.

### Webhook GitHub (operativo)

Registrar en la org `kodvian-solutions` (webhook de org) o en cada repo vinculado:

1. GitHub → Settings → Webhooks → Add webhook.
2. Payload URL: `https://{dominio-railway}/api/webhooks/github`
3. Content type: `application/json`
4. Secret: mismo valor que `GitHub__WebhookSecret`
5. Events: **Issues** (`opened`, `closed`, `reopened`, `edited`)
6. Al vincular un repo nuevo en Kodvian, verificar que queda cubierto por el org webhook o agregar webhook al repo.

Si el webhook no llega, el dev puede usar **Sincronizar** en `/mi-trabajo` como fallback.

## 6. Postgres + migraciones / seed

- [x] Parseo de `DATABASE_URL` en Infrastructure
- [x] `MigrateAsync()` al arrancar
- [x] `AuthSeed` obligatorio fuera de Development
- [ ] En Railway: adjuntar plugin Postgres (inyecta `DATABASE_URL`) — **paso manual al deployar**
- [ ] Setear `AuthSeed__*` y `Jwt__*` **antes** del primer deploy — **paso manual**
- Nota: el admin por defecto de Development (`admin@kodvian.local`) **no** aplica en Production

## 7. Verificaciones

### Packaging local (hecho)

- [x] `docker build -t kodvian-core:local .` exitoso
- [x] Imagen contiene `/app/wwwroot/index.html` y `Kodvian.Core.Api.dll`
- [x] Fail-fast de JWT en Production (placeholder / key corta)

### Post-deploy en Railway (manual)

- [ ] `GET /` → SPA (index.html)
- [ ] `GET /healthz` → 200
- [ ] `GET /api/health` → success
- [ ] Login con el admin de `AuthSeed`
- [ ] Navegación de módulos principales
- [ ] Upload de PDF (documento/recibo) contra S3
- [ ] Integracion GitHub: OAuth en `/mi-perfil`, repo vinculado, sync y webhook (ver checklist en `docs/development/validation-checklist.md`)

## Fuera de alcance (después)

- CI/CD GitHub → Railway
- Custom domain / CDN detallado
- Migración de archivos ya existentes en disco local → S3
- Healthcheck profundo (Postgres/S3)
- Backups de bucket / multi-región
- Tests automatizados específicos de `S3FileStorageService`
