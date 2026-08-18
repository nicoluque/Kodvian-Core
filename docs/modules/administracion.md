# Administracion

## Resumen funcional

El modulo de administracion concentra operaciones internas de administracion del sistema. En el estado actual, la ruta y pantalla existen, pero el endpoint de usuarios devuelve un resultado vacio.

## Flujos principales

- Acceder a la pantalla de administracion con permiso correspondiente.
- Consultar listado de usuarios cuando el backend este implementado.

## Pantallas frontend

- Ruta: `/administracion`.
- Page: `frontend/src/app/modules/administracion/administracion-page.component.ts|html|scss`.
- Route file: `frontend/src/app/modules/administracion/administracion.routes.ts`.
- Guard: `administrationGuard`.

## Contratos backend

- `GET /api/users`.

Archivos:

- Controller: `UsersController.cs`.
- DTO: `UserListItemDto.cs`.
- Entities: `User`, `Role`.

## Modelo de datos

- `User`.
- `Role`.
- Permisos derivados de `RolePermissionMap`.

## Permisos

- Frontend requiere `administration.read`.
- Backend requiere policy `AdministrationRead` y role `Administrador`.

## Estados de UI

- Acceso permitido para usuario con permiso.
- Redireccion a dashboard si no tiene permiso.
- Listado vacio segun estado actual del endpoint.

## Reglas de negocio

- Administracion no debe ser accesible sin permiso.
- La gestion de usuarios debe evitar exponer password hashes o datos sensibles.
- Roles y permisos deben derivar del modelo backend.

## Riesgos y cuidados

- No asumir que el modulo esta completo.
- Evitar UI de escritura hasta que existan endpoints backend seguros.
- Definir reglas de alta, baja, cambio de rol y reset de password antes de implementar.

## Mejoras futuras

- Listado real de usuarios.
- Alta/edicion de usuarios.
- Cambio de rol.
- Activar/desactivar usuarios.
- Reset de password seguro.
- Auditoria de cambios administrativos.
