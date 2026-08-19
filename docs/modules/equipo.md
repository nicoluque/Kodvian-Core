# Equipo

## Resumen funcional

El modulo Equipo gestiona desarrolladores y accesos operativos. Es el punto de trabajo para administradores y analistas sobre el equipo tecnico, sin exponer contratos, pagos ni arreglos economicos al rol `Analista`.

## Ruta frontend

- Ruta visible: `/equipo`.
- Ruta legacy: `/desarrolladores` redirige a `/equipo`.
- Implementacion actual: `frontend/src/app/modules/desarrolladores/**`.

## Permisos

- `team.read`: ver equipo.
- `team.write`: crear y editar desarrolladores y accesos.

## Rol Analista

Puede gestionar clientes, proyectos, documentos, equipo y tareas. No puede ver finanzas, contratos economicos, pagos, cobros ni ledger.

## Asignacion a proyectos

La asignacion operativa de desarrolladores a proyectos se realiza con `ProjectDeveloperAssignment` desde el dialog de Equipo del proyecto. Esta asignacion no incluye monto, porcentaje, modalidad de pago ni pagos.

## Datos economicos

Contratos de desarrollador, pagos, comprobantes de pago, ledger y resumen de contratos quedan restringidos a `Administrador`.
