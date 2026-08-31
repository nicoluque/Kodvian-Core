# Modelo De Datos

El modelo de datos vive en `backend/src/Kodvian.Core.Domain` y se configura en `backend/src/Kodvian.Core.Infrastructure/Persistence/KodvianDbContext.cs`.

## BaseEntity

Campos compartidos:

- `Id`.
- `FechaCreacion`.
- `FechaActualizacion`.
- `Activo`.

## Entidades principales

### Client

Representa un cliente comercial.

Relaciones:

- Tiene muchos `Project`.

Campos relevantes:

- `CommercialName` requerido.
- Datos legales/contacto.
- Estado comercial.
- Servicio, monto mensual, dia de facturacion y notas.

### Project

Representa un proyecto para un cliente.

Relaciones:

- Requiere `ClienteId` hacia `Client` con delete restrict.
- Puede tener `ResponsableId` hacia `User` con set null; funcionalmente representa el analista a cargo y debe apuntar a un usuario activo con rol `Analista` y `DeveloperId` asociado.
- Tiene muchas `TaskItem`.
- Tiene muchas `ProjectDeveloperAssignment` operativas.
- Tiene muchos `ProjectDeveloperContract`.
- Tiene documentos versionados con `ProjectDocument`.
- Tiene documentos legacy o comprobantes relacionados via `DocumentFile`.
- Puede tener muchas `GitHubIssueLink` cuando el proyecto esta vinculado a un repo GitHub.

Campos GitHub opcionales:

- `GitHubOwner`, `GitHubRepoName`, `GitHubRepoId`, `GitHubRepoUrl`.
- Unicidad filtrada: `(GitHubOwner, GitHubRepoName)` unica cuando ambos no son null (un repo no puede vincularse a dos proyectos).

### TaskItem

Representa una tarea operativa.

Relaciones:

- Requiere `ProyectoId` hacia `Project` con delete restrict.
- Puede tener `DeveloperId` hacia `Developer` con set null.
- Puede tener `ResponsableId` hacia `User` con set null.
- Requiere `CreadoPorId` hacia `User` con delete restrict.

### Developer

Representa un desarrollador externo, colaborador o analista remunerable asociado a proyectos/tareas.

Relaciones:

- Tiene muchos `ProjectDeveloperContract`.
- Tiene muchas `TaskItem`.
- Tiene muchas `ProjectDeveloperAssignment`.
- Puede estar vinculado a un `User` analista para permitir acuerdos y pagos al analista a cargo.
- Los perfiles `Developer` vinculados a usuarios con rol `Analista` son remunerables internos y no se listan como desarrolladores operativos.

### ProjectDeveloperAssignment

Representa asignacion operativa de un desarrollador a un proyecto, sin informacion economica.

Relaciones:

- Requiere `ProjectId` hacia `Project` con delete restrict.
- Requiere `DeveloperId` hacia `Developer` con delete restrict.

Restricciones:

- `(ProjectId, DeveloperId)` es unico.
- No guarda montos, porcentajes, modalidad de pago, pagos ni ledger.

### ProjectDeveloperContract

Representa el acuerdo economico entre un proyecto y un desarrollador.

Relaciones:

- Requiere `ProjectId` hacia `Project` con delete restrict.
- Requiere `DeveloperId` hacia `Developer` con delete restrict.
- Tiene muchos `DeveloperPayment` con cascade desde contrato a pagos.

Restricciones:

- `Percentage` entre 0 y 100 si existe.
- `AgreedAmount` mayor a 0 si existe.

### DeveloperPayment

Representa un pago realizado a un desarrollador por un contrato.

Relaciones:

- Requiere `ContractId`.
- Tiene comprobantes `DocumentFile`.

### FinancialCategory

Representa una categoria financiera para ingresos o egresos.

Relaciones:

- Tiene muchos `FinancialMovement`.

### Provider

Representa un proveedor.

Relaciones:

- Tiene muchos `FinancialMovement`.

### FinancialMovement

Representa un ingreso o egreso.

Relaciones:

- Requiere `CategoryId` hacia `FinancialCategory` con restrict.
- Puede relacionarse con `Client`, `Provider` y `Project` con set null.
- Requiere `CreatedById` hacia `User` con restrict.
- Tiene comprobantes `DocumentFile`.

### DocumentFile

Representa un archivo fisico o remoto persistido por el storage.

Puede pertenecer a exactamente uno de:

- `ProjectId`.
- `FinancialMovementId`.
- `DeveloperPaymentId`.

La constraint `CK_DocumentFiles_Owner` evita archivos sin owner o con multiples owners.

### ProjectDocument

Representa un documento funcional de proyecto con versionado.

Relaciones:

- Requiere `ProjectId`.
- Requiere `CreatedById`.
- Puede tener `DeletedById`.
- Tiene muchas `ProjectDocumentVersion`.

Usa baja logica con `Activo`, `DeletedAt` y `DeletedById`.

### ProjectDocumentVersion

Representa una version de un documento de proyecto.

Relaciones:

- Requiere `ProjectDocumentId`.
- Requiere `DocumentFileId`, unico.
- Requiere `UploadedById`.

Restricciones:

- `(ProjectDocumentId, VersionNumber)` es unico.

### Role y User

`Role` define perfiles de acceso.

`User` representa usuarios autenticables:

- Email unico.
- Password hash requerido.
- Role requerido.
- Puede vincularse a un `Developer` mediante `DeveloperId` para acceso al portal de trabajo asignado.
- Campos OAuth GitHub (MVP OAuth App classic, sin refresh token):
  - `GitHubUsername`, `GitHubUserId`.
  - `GitHubAccessTokenEncrypted`.
  - `GitHubConnectedAt`.

Relaciones nuevas:

- `Developer` tiene muchos `User` asociados.
- `User.DeveloperId` usa delete behavior `SetNull`.
- `User` tiene muchos `GitHubOAuthState` temporales para CSRF OAuth.

### GitHubIssueLink

Puente entre Kodvian y una issue de GitHub (dominio de `/mi-trabajo`).

Relaciones:

- Requiere `ProjectId` hacia `Project` con delete restrict.
- Requiere `DeveloperId` hacia `Developer` con delete restrict.

Campos relevantes:

- `GitHubIssueNumber`, `GitHubIssueNodeId`, `GitHubIssueUrl`.
- `Title`, `Description`, `Status` (`GitHubIssueStatus`), `Priority` (`TaskPriority` opcional).
- `AssignedGitHubUsername`, `LastSyncedAt`, `SyncDirection`.

Restricciones:

- `(ProjectId, GitHubIssueNumber)` unico.
- `GitHubIssueNodeId` unico.
- Indice `(DeveloperId, Status)`.

### GitHubOAuthState

Estado CSRF temporal del flujo OAuth GitHub (persistido en DB, no memoria).

Campos:

- `StateToken` unico.
- `UserId` hacia `User` con cascade.
- `ExpiresAt`, `CreatedAt`.

No hereda `BaseEntity`: es efimero y se elimina tras el callback.

## Enums

- `ClientStatus`: `Prospecto`, `Activo`, `Pausado`, `Finalizado`, `Presupuestado`.
- `ProjectStatus`: `Planificacion`, `EnCurso`, `Pausado`, `Finalizado`, `Cancelado`, `Presupuestado`.
- `ProjectPriority`: `Baja`, `Media`, `Alta`, `Urgente`.
- `TaskStatus`: `Pendiente`, `EnCurso`, `Bloqueada`, `Finalizada`, `Cancelada`.
- `TaskPriority`: `Baja`, `Media`, `Alta`, `Urgente`.
- `FinancialMovementType`: `Ingreso`, `Egreso`.
- `FinancialMovementStatus`: `Pendiente`, `Cobrado`, `Pagado`, `Vencido`, `Anulado`.
- `ContractPaymentMode`: `Percentage`, `FixedAmount`.
- `ProjectDocumentType`: `Contract`, `Scope`, `Proposal`, `Deliverable`, `Legal`, `Invoice`, `General`.
- `GitHubIssueStatus`: `Open`, `Closed`.
- `SyncDirection`: `None`, `FromGitHub`, `FromKodvian`.

## Cuidados

- Revisar delete behavior antes de eliminar entidades relacionadas.
- Mantener precision decimal en campos monetarios.
- Agregar indices cuando se incorporen filtros frecuentes.
- Mantener DTOs separados de entidades para evitar exponer relaciones internas.
