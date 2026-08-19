# Equipo

## Resumen funcional

El modulo Equipo gestiona desarrolladores, analistas y accesos operativos. Es el punto de trabajo para administradores y analistas sobre el equipo tecnico, sin exponer contratos, pagos ni arreglos economicos al rol `Analista`.

## Ruta frontend

- Ruta visible: `/equipo`.
- Ruta legacy: `/desarrolladores` redirige a `/equipo`.
- Implementacion actual: `frontend/src/app/modules/desarrolladores/**`.

## Permisos

- `team.read`: ver equipo.
- `team.write`: crear y editar desarrolladores, analistas y accesos.

## Rol Analista

Puede gestionar clientes, proyectos, documentos, equipo y tareas. No puede ver finanzas, contratos economicos, pagos, cobros ni ledger.

Desde `/equipo`, el boton `Nuevo analista` crea un usuario del sistema con rol `Analista`. Este usuario no queda vinculado a un `Developer` y no se puede asignar como desarrollador a proyectos o tareas tecnicas.

## Desarrolladores

El boton `Nuevo desarrollador` crea un `Developer`. Si se habilita `Permitir acceso al sistema`, tambien se crea o actualiza un usuario asociado con rol `Desarrollador`.

## Endpoints de usuarios de equipo

- `GET /api/team/users/analysts`: lista analistas.
- `POST /api/team/users/analysts`: crea analista con contraseña inicial obligatoria.
- `PUT /api/team/users/analysts/{id}`: edita analista y permite cambiar contraseña opcionalmente.

## Asignacion a proyectos

La asignacion operativa de desarrolladores a proyectos se realiza con `ProjectDeveloperAssignment` desde el dialog de Equipo del proyecto. Esta asignacion no incluye monto, porcentaje, modalidad de pago ni pagos.

## Datos economicos

Contratos de desarrollador, pagos, comprobantes de pago, ledger y resumen de contratos quedan restringidos a `Administrador`.
