# Clientes

## Resumen funcional

El modulo de clientes administra la informacion comercial y operativa de los clientes de la empresa.

## Flujos principales

- Listar clientes con filtros y paginacion.
- Crear cliente.
- Editar datos comerciales, legales y de contacto.
- Ver detalle.
- Cambiar estado comercial.

## Pantallas frontend

- Ruta: `/clientes`.
- Page: `frontend/src/app/modules/clientes/clientes-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/clientes/services/clientes.service.ts`.
- Models: `frontend/src/app/modules/clientes/models/clientes.models.ts`.
- Dialogs: `cliente-form-dialog`, `cliente-detail-dialog`, `cliente-status-dialog`.

## Contratos backend

- `GET /api/clients`.
- `GET /api/clients/{id}`.
- `POST /api/clients`.
- `PUT /api/clients/{id}`.
- `PATCH /api/clients/{id}/status`.

Archivos:

- Controller: `ClientsController.cs`.
- Service: `ClientService.cs`.
- Abstraction: `IClientService.cs`.
- DTOs/requests: `backend/src/Kodvian.Core.Application/Clients/**`.

## Modelo de datos

- Entity: `Client`.
- Enum: `ClientStatus`.
- Relacion: un cliente puede tener muchos proyectos.

Estados:

- `Prospecto`.
- `Activo`.
- `Pausado`.
- `Finalizado`.
- `Presupuestado`.

## Permisos

- `clients.read` para consulta.
- `clients.write` para alta, edicion y cambio de estado.
- El controller actualmente requiere autenticacion general.

## Estados de UI

- Loading de listado.
- Empty state sin clientes o sin resultados de filtros.
- Error de carga o guardado con snackbar.
- Dialog de confirmacion/cambio de estado.

## Reglas de negocio

- `CommercialName` es el dato central del cliente.
- El estado comercial debe guiar visibilidad y priorizacion.
- Datos fiscales/contacto deben mantenerse separados de informacion operativa.

## Riesgos y cuidados

- No eliminar o modificar clientes sin revisar impacto en proyectos.
- Evitar exponer IDs internos en UI.
- Verificar que filtros frontend coincidan con `ClientListRequestDto`.

## Tests requeridos

- Backend: tests de `ClientService` para listado, detalle, alta, edicion y cambio de estado.
- Backend: tests de validacion para campos requeridos, limites y estados validos.
- Backend: integration tests de `GET`, `POST`, `PUT` y `PATCH /api/clients/{id}/status`.
- Backend: tests de permisos cuando se agreguen policies para `clients.read/write`.
- Frontend: specs de `ClientesService` para rutas, params, body y respuesta.
- Frontend: specs de pagina para filtros, paginacion, loading, empty, error y apertura de dialogs.
- Frontend: specs de dialogs de form, detail y status para validaciones, submit y cierre.

## Mejoras futuras

- Historial de cambios de estado.
- Vista 360 del cliente con proyectos, facturacion y tareas abiertas.
- Permisos backend finos para `clients.read/write`.
