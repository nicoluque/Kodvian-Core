# Finanzas

## Resumen funcional

El modulo de finanzas administra ingresos, egresos, categorias, proveedores, comprobantes y resumen mensual.

## Flujos principales

- Listar movimientos financieros.
- Crear y editar ingreso/egreso.
- Consultar detalle.
- Adjuntar comprobantes PDF.
- Descargar o eliminar comprobantes.
- Administrar categorias.
- Consultar proveedores.
- Ver resumen mensual.

## Pantallas frontend

- Ruta: `/finanzas`.
- Page: `frontend/src/app/modules/finanzas/finanzas-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/finanzas/services/finanzas.service.ts`.
- Models: `frontend/src/app/modules/finanzas/models/finanzas.models.ts`.
- Dialogs: `movimiento-form-dialog`, `categoria-form-dialog`.

## Contratos backend

Movimientos:

- `GET /api/financial-movements`.
- `GET /api/financial-movements/{id}`.
- `GET /api/financial-movements/monthly-summary`.
- `GET /api/financial-movements/lookups`.
- `POST /api/financial-movements`.
- `PUT /api/financial-movements/{id}`.

Comprobantes:

- `GET /api/financial-movements/{id}/receipts`.
- `POST /api/financial-movements/{id}/receipts`.
- `GET /api/financial-movements/{id}/receipts/{receiptId}`.
- `DELETE /api/financial-movements/{id}/receipts/{receiptId}`.

Categorias:

- `GET /api/financial-categories`.
- `POST /api/financial-categories`.
- `PUT /api/financial-categories/{id}`.

Proveedores:

- `GET /api/providers`.
- `POST /api/providers`.
- `PUT /api/providers/{id}`.

## Modelo de datos

- `FinancialMovement`.
- `FinancialCategory`.
- `Provider`.
- `Client`.
- `Project`.
- `DocumentFile`.

Enums:

- `FinancialMovementType`: ingreso/egreso.
- `FinancialMovementStatus`: pendiente, cobrado, pagado, vencido, anulado.

## Permisos

- `finances.read` para consulta.
- `finances.write` para escritura.
- El controller actualmente requiere autenticacion general.

## Estados de UI

- Loading de movimientos y resumen.
- Empty state por filtros.
- Error con snackbar.
- Dialog de movimiento.
- Dialog de categoria.
- Estados de upload/download/delete de comprobantes.

## Reglas de negocio

- Todo movimiento tiene tipo, categoria, descripcion, monto y estado.
- Ingresos pueden asociarse a cliente/proyecto.
- Egresos pueden asociarse a proveedor/proyecto.
- El resumen mensual debe respetar tipo, estado y fecha.
- Los comprobantes deben pertenecer al movimiento correcto.

## Riesgos y cuidados

- Validar precision monetaria.
- No mezclar estado `Cobrado` y `Pagado` entre ingresos/egresos.
- Evitar eliminar comprobantes por error; usar confirmacion clara.
- Revisar permisos antes de exponer finanzas a roles operativos.

## Mejoras futuras

- Conciliacion o historial de cambios.
- Exportacion CSV/PDF.
- Permisos finos por finanzas.
- Dashboard financiero por periodo.
