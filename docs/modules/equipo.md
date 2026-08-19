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

Desde `/equipo`, el boton `Nuevo analista` crea un usuario del sistema con rol `Analista` y un perfil `Developer` asociado para acuerdos economicos y pagos. Este perfil remunerable permite que el analista a cargo aparezca en contratos asociados de proyecto.

El perfil remunerable del analista es interno: no aparece en la grilla de desarrolladores ni en el selector de desarrolladores operativos.

## Desarrolladores

El boton `Nuevo desarrollador` crea un `Developer`. Si se habilita `Permitir acceso al sistema`, tambien se crea o actualiza un usuario asociado con rol `Desarrollador`.

La grilla de desarrolladores muestra solo desarrolladores reales. Excluye perfiles `Developer` asociados a usuarios con rol `Analista`.

## Endpoints de usuarios de equipo

- `GET /api/team/users/analysts`: lista analistas.
- `POST /api/team/users/analysts`: crea analista con contraseña inicial obligatoria.
- `PUT /api/team/users/analysts/{id}`: edita analista y permite cambiar contraseña opcionalmente.

## Asignacion a proyectos

La asignacion operativa de desarrolladores a proyectos se realiza con `ProjectDeveloperAssignment` desde el dialog de Equipo del proyecto. Esta asignacion no incluye monto, porcentaje, modalidad de pago ni pagos.

El analista a cargo se asigna desde el mismo dialog usando `Project.ResponsableId`. Si tiene perfil remunerable asociado, puede crear acuerdos y pagos desde contratos asociados.

## Datos economicos

Contratos de desarrolladores o analistas, pagos, comprobantes de pago, ledger y resumen de contratos quedan restringidos a `Administrador`.
