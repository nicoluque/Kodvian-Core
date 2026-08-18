# Dashboard

## Resumen funcional

El dashboard resume el estado operativo de Kodvian Core. Permite ver rapidamente clientes activos, proyectos en curso, tareas vencidas, tareas del dia, ingresos, egresos, resultado mensual, cobranzas pendientes y pagos pendientes.

## Flujos principales

- Consultar KPIs generales.
- Revisar tareas prioritarias.
- Detectar cobranzas proximas.
- Revisar movimientos financieros recientes.
- Navegar hacia modulos operativos desde tarjetas o acciones relacionadas.

## Pantallas frontend

- Ruta: `/dashboard`.
- Route file: `frontend/src/app/modules/dashboard/dashboard.routes.ts`.
- Page: `frontend/src/app/modules/dashboard/dashboard-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/dashboard/services/dashboard.service.ts`.
- Models: `frontend/src/app/modules/dashboard/models/dashboard.models.ts`.

## Contratos backend

- `GET /api/dashboard/overview`.
- Controller: `backend/src/Kodvian.Core.Api/Controllers/DashboardController.cs`.
- Service abstraction: `IDashboardService`.
- Service implementation: `DashboardService`.
- DTOs: `DashboardOverviewDto`, `DashboardKpisDto`, `DashboardPriorityTaskDto`, `DashboardUpcomingCollectionDto`, `DashboardRecentMovementDto`.

## Modelo de datos

El dashboard consulta informacion agregada desde:

- `Client`.
- `Project`.
- `TaskItem`.
- `FinancialMovement`.

## Permisos

- Permiso funcional: `dashboard.read`.
- El controller requiere usuario autenticado.

## Estados de UI

- Loading inicial.
- Error con snackbar si falla la carga.
- Valores por defecto en cero/listas vacias antes de la respuesta.

## Reglas de negocio

- Los KPIs deben reflejar datos actuales y consistentes.
- Las metricas financieras deben respetar estados y fechas del mes consultado.
- Las tareas prioritarias deben ayudar a decidir accion inmediata.

## Riesgos y cuidados

- Evitar que el dashboard oculte errores silenciosamente.
- No mezclar ingresos/egresos con signos ambiguos.
- Si se agregan filtros por periodo, documentar exactamente como impactan los KPIs.

## Tests requeridos

- Backend: tests de `DashboardService` para KPIs, tareas prioritarias, cobranzas proximas y movimientos recientes.
- Backend: integration test de `GET /api/dashboard/overview` autenticado.
- Backend: tests de calculos financieros por periodo cuando se modifiquen reglas de resumen.
- Frontend: specs de `DashboardService` para URL y mapeo de respuesta.
- Frontend: specs de `DashboardPageComponent` para loading, success, error y listas vacias.

## Mejoras futuras

- Filtros por periodo.
- Health operativo de cobranzas/pagos.
- Accesos directos configurables por rol.
