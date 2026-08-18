# Tareas

## Resumen funcional

El modulo de tareas organiza el trabajo operativo asociado a proyectos. Permite listar, filtrar, ver detalle, crear, editar, cambiar estado y visualizar tareas en kanban.

## Flujos principales

- Listar tareas.
- Filtrar por estado, prioridad, proyecto, responsable u otros criterios disponibles.
- Crear tarea.
- Editar tarea.
- Cambiar estado.
- Consultar detalle.
- Visualizar kanban.

## Pantallas frontend

- Ruta: `/tareas`.
- Page: `frontend/src/app/modules/tareas/tareas-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/tareas/services/tareas.service.ts`.
- Models: `frontend/src/app/modules/tareas/models/tareas.models.ts`.
- Dialogs: `tarea-form-dialog`, `tarea-detail-dialog`, `tarea-status-dialog`.

## Contratos backend

- `GET /api/tasks`.
- `GET /api/tasks/{id}`.
- `GET /api/tasks/kanban`.
- `GET /api/tasks/lookups`.
- `POST /api/tasks`.
- `PUT /api/tasks/{id}`.
- `PATCH /api/tasks/{id}/status`.

Archivos:

- Controller: `TasksController.cs`.
- Service: `TaskService.cs`.
- Abstraction: `ITaskService.cs`.
- DTOs/requests: `backend/src/Kodvian.Core.Application/Tasks/**`.

## Modelo de datos

- Entity: `TaskItem`.
- Relaciones: proyecto requerido, desarrollador opcional, responsable opcional, creador requerido.
- Enums: `TaskStatus`, `TaskPriority`.

Estados:

- `Pendiente`.
- `EnCurso`.
- `Bloqueada`.
- `Finalizada`.
- `Cancelada`.

Prioridades:

- `Baja`.
- `Media`.
- `Alta`.
- `Urgente`.

## Permisos

- `tasks.read` para consulta.
- `tasks.write` para alta, edicion y cambio de estado.
- El controller actualmente requiere autenticacion general.

## Estados de UI

- Loading de listado/kanban.
- Empty state.
- Error con snackbar.
- Dialog de detalle.
- Dialog de cambio de estado.

## Reglas de negocio

- Toda tarea pertenece a un proyecto.
- Estado y prioridad deben expresarse con etiquetas claras.
- Las horas estimadas/reales usan precision decimal.
- Kanban depende de estado y orden.

## Riesgos y cuidados

- Cuidar transiciones de estado invalidas si se agregan reglas.
- Evitar cambiar orden kanban sin persistencia consistente.
- No mostrar campos tecnicos al usuario final.

## Tests requeridos

- Backend: tests de `TaskService` para listado, detalle, kanban, lookups, alta, edicion y cambio de estado.
- Backend: tests de validacion para proyecto requerido, prioridad, estado, horas y fechas.
- Backend: tests de transiciones de estado cuando se agreguen reglas.
- Backend: integration tests de `GET`, `POST`, `PUT`, `PATCH /api/tasks/{id}/status` y `GET /api/tasks/kanban`.
- Frontend: specs de `TareasService` para rutas, filtros, params y payloads.
- Frontend: specs de pagina para listado, kanban, filtros, loading, empty, error y acciones.
- Frontend: specs de dialogs de form, detail y status para validacion, submit y cierre.

## Mejoras futuras

- Drag and drop kanban con persistencia de orden.
- Historial de cambios.
- Comentarios o actividad por tarea.
- Permisos backend finos por `tasks.read/write`.
