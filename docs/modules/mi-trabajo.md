# Mi Trabajo

## Resumen funcional

Modulo para usuarios con rol `Desarrollador`. Muestra proyectos asociados y tareas asignadas al desarrollador autenticado.

## Flujos principales

- Ver resumen de proyectos y tareas propias.
- Listar proyectos asociados por contrato o tarea asignada.
- Listar tareas asignadas.
- Ver tareas en tablero kanban.
- Consultar detalle de una tarea propia.
- Cambiar estado de una tarea propia.

## Pantallas frontend

- Ruta: `/mi-trabajo`.
- Page: `frontend/src/app/modules/mi-trabajo/mi-trabajo-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/mi-trabajo/services/mi-trabajo.service.ts`.
- Models: `frontend/src/app/modules/mi-trabajo/models/mi-trabajo.models.ts`.

## Contratos backend

- `GET /api/my-work/overview`.
- `GET /api/my-work/projects`.
- `GET /api/my-work/tasks`.
- `GET /api/my-work/tasks/kanban`.
- `GET /api/my-work/tasks/{id}`.
- `PATCH /api/my-work/tasks/{id}/status`.

Archivos:

- Controller: `MyWorkController.cs`.
- Service: `MyWorkService.cs`.
- Abstraction: `IMyWorkService.cs`.
- DTOs: `backend/src/Kodvian.Core.Application/MyWork/**`.

## Permisos

- `developer.work.read`: lectura del modulo.
- `developer.tasks.status.write`: cambio de estado de tareas propias.

## Reglas de negocio

- El usuario debe tener `DeveloperId` vinculado.
- Los proyectos visibles salen de contratos `ProjectDeveloperContracts` o de tareas asignadas.
- Las tareas visibles deben estar asignadas al `DeveloperId` autenticado.
- El cambio de estado solo aplica sobre tareas propias.

## Tests requeridos

- Backend: filtros de `MyWorkService` por `DeveloperId`.
- Backend: `PATCH /api/my-work/tasks/{id}/status` no debe afectar tareas de otro desarrollador.
- Frontend: carga de resumen, estados vacios y error.
