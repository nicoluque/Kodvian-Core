# Modulos Frontend

Este documento mapea los modulos Angular con sus archivos principales.

## Dashboard

- Ruta: `/dashboard`.
- Route file: `frontend/src/app/modules/dashboard/dashboard.routes.ts`.
- Page: `dashboard-page.component.ts|html|scss`.
- Service: `services/dashboard.service.ts`.
- Models: `models/dashboard.models.ts`.
- Endpoint principal: `GET /api/dashboard/overview`.

## Clientes

- Ruta: `/clientes`.
- Route file: `frontend/src/app/modules/clientes/clientes.routes.ts`.
- Page: `clientes-page.component.ts|html|scss`.
- Service: `services/clientes.service.ts`.
- Models: `models/clientes.models.ts`.
- Dialogs: form, detail y status.
- Endpoints: `/api/clients`.

## Desarrolladores

- Ruta: `/desarrolladores`.
- Route file: `frontend/src/app/modules/desarrolladores/desarrolladores.routes.ts`.
- Page: `desarrolladores-page.component.ts|html|scss`.
- Service: `services/desarrolladores.service.ts`.
- Models: `models/desarrolladores.models.ts`.
- Dialog: form.
- Endpoints: `/api/developers`.

## Proyectos

- Ruta: `/proyectos`.
- Route file: `frontend/src/app/modules/proyectos/proyectos.routes.ts`.
- Page: `proyectos-page.component.ts|html|scss`.
- Service: `services/proyectos.service.ts`.
- Models: `models/proyectos.models.ts`.
- Dialogs: proyecto form/detail, desarrolladores, contratos, pagos, ledger.
- Endpoints: `/api/projects`, documentos, contratos, pagos y comprobantes.

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
