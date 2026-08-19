# Proyectos

## Resumen funcional

El modulo de proyectos centraliza la gestion de trabajos para clientes: estado, prioridad, responsable, avance, documentos, equipo asignado y, solo para administradores, contratos y pagos asociados.

## Flujos principales

- Listar proyectos.
- Crear y editar proyecto.
- Ver detalle.
- Gestionar documentos y versiones.
- Asignar desarrolladores operativamente sin datos economicos.
- Registrar pagos a desarrolladores solo como administrador.
- Consultar ledger de contrato solo como administrador.

## Pantallas frontend

- Ruta: `/proyectos`.
- Page: `frontend/src/app/modules/proyectos/proyectos-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/proyectos/services/proyectos.service.ts`.
- Models: `frontend/src/app/modules/proyectos/models/proyectos.models.ts`.
- Dialogs: `proyecto-form-dialog`, `proyecto-detail-dialog`, `proyecto-developers-dialog`, `contrato-desarrollador-form-dialog`, `pago-desarrollador-form-dialog`, `contrato-ledger-dialog`.

## Contratos backend

Proyectos:

- `GET /api/projects`.
- `GET /api/projects/{id}`.
- `GET /api/projects/lookups`.
- `POST /api/projects`.
- `PUT /api/projects/{id}`.

Documentos:

- `GET /api/projects/document-types`.
- `GET /api/projects/{id}/documents`.
- `POST /api/projects/{id}/documents`.
- `POST /api/projects/{id}/documents/{documentId}/versions`.
- `GET /api/projects/{id}/documents/{documentId}/versions`.
- `GET /api/projects/{id}/documents/{documentId}`.
- `GET /api/projects/{id}/documents/{documentId}/versions/{versionId}`.
- `DELETE /api/projects/{id}/documents/{documentId}`.

Contratos y pagos:

- Solo administrador.

- `GET /api/projects/{projectId}/developer-contracts`.
- `POST /api/projects/{projectId}/developer-contracts`.
- `PUT /api/developer-contracts/{id}`.
- `GET /api/developer-contracts/{id}/ledger`.
- `GET /api/developer-contracts/{contractId}/payments`.
- `POST /api/developer-contracts/{contractId}/payments`.
- Comprobantes bajo `/api/developer-payments/{paymentId}/receipts`.

## Modelo de datos

- `Project`.
- `Client`.
- `TaskItem`.
- `ProjectDocument`.
- `ProjectDocumentVersion`.
- `DocumentFile`.
- `Developer`.
- `ProjectDeveloperAssignment`.
- `ProjectDeveloperContract`.
- `DeveloperPayment`.

Enums:

- `ProjectStatus`.
- `ProjectPriority`.
- `ProjectDocumentType`.
- `ContractPaymentMode`.

## Permisos

- `projects.read` y `projects.write` para proyectos.
- `projects.documents.read`, `projects.documents.write`, `projects.documents.delete` para documentos.
- `ProjectDeveloperAssignment` usa permisos operativos de proyectos, sin datos economicos.
- Contratos economicos, pagos y ledger son solo administrador.
- El frontend evalua permisos de documentos en el detalle.
- El backend tiene policies especificas para documentos de proyecto.

## Estados de UI

- Listado con filtros/paginacion.
- Dialog de detalle.
- Estados de documentos: sin documentos, versiones, descarga, upload, delete.
- Estados de contratos/pagos/comprobantes.
- Errores de upload/download.

## Reglas de negocio

- Todo proyecto pertenece a un cliente.
- El porcentaje de avance debe ser coherente con estado y tareas.
- Los documentos de proyecto se versionan, no se reemplazan silenciosamente.
- La asignacion operativa de equipo no contiene montos, porcentajes ni modalidad de pago.
- Los contratos pueden ser por porcentaje o monto fijo.
- Los pagos deben asociarse a un contrato.

## Riesgos y cuidados

- Documento de proyecto usa baja logica; no confundir con eliminacion fisica de comprobantes.
- Uploads solo deben aceptar PDFs validos.
- Validar permisos backend para acciones sensibles.
- Evitar inconsistencias entre presupuesto de proyecto, contratos y pagos.

## Tests requeridos

- Backend: tests de `ProjectService` para listado, detalle, lookups, alta y edicion.
- Backend: tests de documentos para alta, versionado, descarga, listado y baja logica.
- Backend: tests de permisos para `projects.documents.read/write/delete` con acceso permitido y denegado.
- Backend: tests de contratos para porcentaje, monto fijo, constraints, edicion y ledger.
- Backend: tests de pagos y comprobantes para alta, upload, descarga y eliminacion.
- Backend: integration tests de endpoints principales de proyectos, documentos, contratos y pagos.
- Frontend: specs de `ProyectosService` para endpoints, FormData, downloads y params.
- Frontend: specs de pagina para filtros, paginacion, loading, empty, error y alta/edicion.
- Frontend: specs de dialogs de proyecto, detalle, documentos, contratos, pagos y ledger.
- Frontend: specs de permisos visibles para acciones de documentos.

## Mejoras futuras

- Vista de timeline de proyecto.
- Historial de estado/avance.
- Documentos con preview controlado.
- Alertas por pagos pendientes o presupuesto excedido.
