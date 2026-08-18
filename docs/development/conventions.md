# Convenciones

## Generales

- Codigo en ingles para clases, servicios, DTOs y capas nuevas.
- Texto visible al usuario en espanol latino.
- Mantener nombres existentes aunque mezclen ingles/espanol si ya forman parte del modulo.
- Preferir cambios pequenos y seguros.
- No agregar abstracciones sin reutilizacion clara.

## Backend

- Controllers en `Kodvian.Core.Api`.
- DTOs y request models en `Kodvian.Core.Application`.
- Interfaces de servicio en Application.
- Implementaciones en Infrastructure.
- Entidades y enums en Domain.
- EF mappings, relaciones e indices en `KodvianDbContext`.
- Migrations en Infrastructure.

## API

- Rutas bajo `/api/...`.
- Respuesta con `success`, `message`, `data`.
- Listados paginados con `pageNumber` y `pageSize`.
- No devolver entidades EF directamente.
- Usar status codes coherentes con la operacion.
- Validar permisos en backend.

## Frontend

- Usar standalone components.
- Mantener servicios por modulo para API.
- Mantener modelos frontend junto al modulo.
- Usar rutas lazy por modulo.
- Reutilizar estilos compartidos cuando el patron ya existe.
- Evitar meter logica de negocio compleja en templates.

## UX/UI

- Preservar dark UI de Kodvian salvo pedido explicito.
- Usar verde neon con moderacion.
- Priorizar legibilidad sobre decoracion.
- Evitar hero sections y layouts de marketing en pantallas internas.
- Tablas/listas para informacion operativa estructurada.
- Dialogs para tareas cortas.
- Paginas dedicadas para flujos complejos.

## Seguridad

- No persistir JWT en localStorage sin redisenar seguridad.
- La cookie `auth_token` es HTTP-only y se envia con `withCredentials`.
- No confiar en ocultar botones como autorizacion.
- Agregar policy/check backend para acciones sensibles.
- No documentar secretos reales.

## Documentacion

- Cada modulo debe tener vista funcional y tecnica.
- Documentar comportamiento existente, no aspiracional, salvo en seccion `Mejoras futuras`.
- Usar rutas y nombres de archivo reales.
- Mantener `docs/index.md` como punto de entrada.
- Si cambia un contrato, actualizar backend, frontend y modulo afectado.
