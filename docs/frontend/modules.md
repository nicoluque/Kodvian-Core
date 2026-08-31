# Modulos Frontend

Este documento mapea los modulos Angular con sus archivos principales.

## Dashboard

- Ruta: `/dashboard`.
- Route file: `frontend/src/app/modules/dashboard/dashboard.routes.ts`.
- Page: `dashboard-page.component.ts|html|scss`.
- Service: `services/dashboard.service.ts`.
- Models: `models/dashboard.models.ts`.
- Endpoint principal: `GET /api/dashboard/overview`.

## Mi trabajo

- Ruta: `/mi-trabajo`.
- Route file: `frontend/src/app/modules/mi-trabajo/mi-trabajo.routes.ts`.
- Page: `mi-trabajo-page.component.ts|html|scss`.
- Service: `services/mi-trabajo.service.ts`.
- Models: `models/mi-trabajo.models.ts`.
- Dialog: `components/nueva-tarea-github-dialog/` (crear issue).
- Endpoints: `/api/my-work` (overview, repositories, issues, sync, tasks legacy).
- Repos: solo proyectos Kodvian con GitHub vinculado y acceso del desarrollador (assignment/contrato/tarea).
- Issues: desde `GitHubIssueLink`; botones Sincronizar y Nueva tarea (segun permisos).
- Cambio estado issue: select Abierta/Cerrada con confirmacion al cerrar (`developer.tasks.status.write`).
- Empty state sin GitHub: CTA a `/mi-perfil`.
- Audiencia: usuarios con rol `Desarrollador`.

## Mi perfil

- Ruta: `/mi-perfil`.
- Route file: `frontend/src/app/modules/mi-perfil/mi-perfil.routes.ts`.
- Page: `mi-perfil-page.component.ts|html|scss`.
- Service: `services/mi-perfil.service.ts`.
- Models: `models/mi-perfil.models.ts`.
- Endpoints: `/api/profile`, connect/disconnect GitHub.
- Audiencia: usuarios con `developer.work.read` (rol Desarrollador) via sidebar.

## Clientes

- Ruta: `/clientes`.
- Route file: `frontend/src/app/modules/clientes/clientes.routes.ts`.
- Page: `clientes-page.component.ts|html|scss`.
- Service: `services/clientes.service.ts`.
- Models: `models/clientes.models.ts`.
- Dialogs: form, detail y status.
- Endpoints: `/api/clients`.

## Equipo

- Ruta: `/equipo`.
- Redirect legacy: `/desarrolladores` -> `/equipo`.
- Route file: `frontend/src/app/modules/desarrolladores/desarrolladores.routes.ts`.
- Page: `desarrolladores-page.component.ts|html|scss`.
- Service: `services/desarrolladores.service.ts`.
- Models: `models/desarrolladores.models.ts`.
- Dialogs: desarrollador form y analista form.
- Endpoints: `/api/developers`, `/api/team/users/analysts`.
- Permite crear desarrolladores, crear analistas y habilitar, deshabilitar o restablecer acceso al sistema para el desarrollador.
- Usa permisos `team.read` y `team.write`.

## Proyectos

- Ruta: `/proyectos`.
- Route file: `frontend/src/app/modules/proyectos/proyectos.routes.ts`.
- Page: `proyectos-page.component.ts|html|scss`.
- Service: `services/proyectos.service.ts`.
- Models: `models/proyectos.models.ts`.
- Dialogs: proyecto form/detail, equipo asignado, contratos, pagos, ledger.
- Endpoints: `/api/projects`, documentos, asignaciones operativas, contratos, pagos y comprobantes.
- Vincular/desvincular/validar repo GitHub: solo Administrador (`PUT|DELETE|POST .../github-repo`).
- Contratos, pagos y ledger solo deben estar disponibles para administrador.

## Tareas

- Ruta: `/tareas`.
- Route file: `frontend/src/app/modules/tareas/tareas.routes.ts`.
- Page: `tareas-page.component.ts|html|scss`.
- Service: `services/tareas.service.ts`.
- Models: `models/tareas.models.ts`.
- Dialogs: form, detail y status.
- Endpoints: `/api/tasks`.

## Finanzas

- Ruta: `/finanzas`.
- Route file: `frontend/src/app/modules/finanzas/finanzas.routes.ts`.
- Page: `finanzas-page.component.ts|html|scss`.
- Service: `services/finanzas.service.ts`.
- Models: `models/finanzas.models.ts`.
- Dialogs: movimiento y categoria.
- Endpoints: movimientos, categorias, proveedores, comprobantes y resumen mensual.

## Administracion

- Ruta: `/administracion`.
- Route file: `frontend/src/app/modules/administracion/administracion.routes.ts`.
- Page: `administracion-page.component.ts|html|scss`.
- Guard: `administrationGuard`.
- Endpoint: `/api/users`.

## Regla para nuevos modulos

1. Crear carpeta en `frontend/src/app/modules/<modulo>`.
2. Crear `*.routes.ts`.
3. Crear page standalone.
4. Crear service para API.
5. Crear models del modulo.
6. Agregar ruta en `app.routes.ts`.
7. Agregar item en `NavigationService` si corresponde.
8. Documentar en `docs/modules/<modulo>.md`.
