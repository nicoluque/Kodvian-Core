# Desarrolladores

## Resumen funcional

El modulo de desarrolladores gestiona colaboradores o proveedores tecnicos que participan en proyectos y pueden tener contratos y pagos asociados.

## Flujos principales

- Listar desarrolladores.
- Crear desarrollador.
- Editar datos de contacto/fiscales.
- Habilitar o deshabilitar acceso al sistema para un desarrollador.
- Restablecer password de acceso del desarrollador.
- Consultar resumen de contratos por anio.

## Pantallas frontend

- Ruta: `/desarrolladores`.
- Page: `frontend/src/app/modules/desarrolladores/desarrolladores-page.component.ts|html|scss`.
- Service: `frontend/src/app/modules/desarrolladores/services/desarrolladores.service.ts`.
- Models: `frontend/src/app/modules/desarrolladores/models/desarrolladores.models.ts`.
- Dialog: `desarrollador-form-dialog`.

## Contratos backend

- `GET /api/developers`: lista desarrolladores e indica si tienen acceso al sistema.
- `GET /api/developers/{id}/contracts-summary`.
- `POST /api/developers`: alta de desarrollador, opcionalmente con usuario de acceso.
- `PUT /api/developers/{id}`: edicion de desarrollador y configuracion de acceso.

Archivos:

- Controller: `DevelopersController.cs`.
- Service: `DeveloperService.cs`.
- Abstraction: `IDeveloperService.cs`.
- DTOs/requests: `backend/src/Kodvian.Core.Application/Developers/**`.

## Modelo de datos

- Entity: `Developer`.
- Relaciones: tareas asignadas, contratos por proyecto y usuarios vinculados.
- Contratos relacionados: `ProjectDeveloperContract`.
- Pagos relacionados: `DeveloperPayment`.
- Acceso relacionado: `User.DeveloperId`.

## Permisos

- La gestion de desarrolladores requiere `projects.write`.
- Los usuarios generados para desarrolladores reciben rol `Desarrollador`.
- El rol `Desarrollador` tiene `developer.work.read` y `developer.tasks.status.write`.

## Estados de UI

- Loading de listado.
- Dialog de alta/edicion.
- Error con snackbar.
- Empty state cuando no hay desarrolladores.

## Reglas de negocio

- Un desarrollador puede participar en multiples proyectos.
- Los datos de contacto y fiscales deben estar disponibles para pagos/contratos.
- El resumen anual debe ser consistente con contratos y pagos.
- El email del desarrollador se usa como email del usuario de acceso cuando se habilita el acceso al sistema.
- Para crear acceso nuevo se requiere password.
- Si se deshabilita acceso, el usuario vinculado queda inactivo.

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
