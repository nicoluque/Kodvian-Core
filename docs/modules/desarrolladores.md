# Desarrolladores

## Resumen funcional

El modulo de desarrolladores gestiona colaboradores o proveedores tecnicos que participan en proyectos y pueden tener contratos y pagos asociados.

## Flujos principales

- Listar desarrolladores.
- Crear desarrollador.
- Editar datos de contacto/fiscales.
- Consultar resumen de contratos por anio.

## Pantallas frontend

- Ruta: `/desarrolladores`.
- Page: `frontend/src/app/modules/desarrolladores/desarrolladores-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/desarrolladores/services/desarrolladores.service.ts`.
- Models: `frontend/src/app/modules/desarrolladores/models/desarrolladores.models.ts`.
- Dialog: `desarrollador-form-dialog`.

## Contratos backend

- `GET /api/developers`.
- `GET /api/developers/{id}/contracts-summary`.
- `POST /api/developers`.
- `PUT /api/developers/{id}`.

Archivos:

- Controller: `DevelopersController.cs`.
- Service: `DeveloperService.cs`.
- Abstraction: `IDeveloperService.cs`.
- DTOs/requests: `backend/src/Kodvian.Core.Application/Developers/**`.

## Modelo de datos

- Entity: `Developer`.
- Relaciones: tareas asignadas y contratos por proyecto.
- Contratos relacionados: `ProjectDeveloperContract`.
- Pagos relacionados: `DeveloperPayment`.

## Permisos

- No hay permission code especifico para desarrolladores en el mapa actual.
- El controller requiere autenticacion general.
- En mejoras futuras, evaluar permiso dedicado si el modulo maneja informacion sensible de pagos.

## Estados de UI

- Loading de listado.
- Dialog de alta/edicion.
- Error con snackbar.
- Empty state cuando no hay desarrolladores.

## Reglas de negocio

- Un desarrollador puede participar en multiples proyectos.
- Los datos de contacto y fiscales deben estar disponibles para pagos/contratos.
- El resumen anual debe ser consistente con contratos y pagos.

## Riesgos y cuidados

- Evitar duplicar desarrolladores por email o tax id si se agregan restricciones futuras.
- Cuidar exposicion de informacion fiscal.
- Al editar, verificar impacto en contratos historicos.

## Tests requeridos

- Backend: tests de `DeveloperService` para listado, alta, edicion y resumen de contratos.
- Backend: tests de validacion para datos de contacto/fiscales y duplicados si se agregan reglas.
- Backend: integration tests de `GET`, `POST`, `PUT` y `GET /api/developers/{id}/contracts-summary`.
- Backend: tests de permisos si se agregan permisos dedicados para desarrolladores.
- Frontend: specs de `DesarrolladoresService` para endpoints y query params.
- Frontend: specs de pagina para loading, empty, error, alta y edicion.
- Frontend: specs del dialog de desarrollador para validaciones y payload enviado.

## Mejoras futuras

- Estado del desarrollador: activo/inactivo.
- Perfil detallado con proyectos, tareas, contratos y pagos.
- Permisos especificos de lectura/escritura.
