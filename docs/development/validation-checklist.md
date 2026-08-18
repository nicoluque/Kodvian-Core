# Checklist De Validacion

Usar este checklist antes de dar por cerrada una mejora.

Toda mejora debe incluir tests automaticos para los circuitos, flujos y operaciones que agrega o modifica. Si una validacion no puede automatizarse, debe quedar justificada y documentada.

## Backend

Desde `backend/`:

```bash
dotnet restore
dotnet build Kodvian.Core.slnx
dotnet test Kodvian.Core.slnx
```

Validar manualmente si aplica:

- Endpoint nuevo aparece en Swagger en Development.
- Request DTO valida campos requeridos.
- Response respeta `success`, `message`, `data`.
- Errores se manejan con formato consistente.
- Permisos se validan en backend.
- EF Core migration fue revisada.
- Consultas frecuentes tienen indices adecuados.
- Upload/download funciona si toca archivos.

Tests requeridos si aplica:

- Unit tests para reglas de negocio, validaciones, calculos y transiciones de estado.
- Tests de permisos para acceso permitido y denegado.
- Integration tests para endpoints nuevos o modificados.
- Tests de persistencia para queries, filtros, paginacion, ordenamiento, relaciones y constraints.
- Tests de storage para upload, download, eliminacion y validacion de PDF.

## Frontend

Desde `frontend/`:

```bash
npm run build
npm test
```

Validar manualmente si aplica:

- Login y restauracion de sesion.
- Navegacion desde sidebar.
- Permisos visibles y guards.
- Loading, empty, error y success states.
- Formularios con validaciones.
- Dialogs en desktop y mobile.
- Tablas/listas con datos largos.
- Acciones destructivas con confirmacion.

Tests requeridos si aplica:

- Specs de servicios API para URL, metodo, params, body y respuesta.
- Specs de guards e interceptor para sesion, permisos, `withCredentials` y HTTP 401.
- Specs de paginas para render, carga, error, empty state y acciones principales.
- Specs de dialogs/formularios para validaciones, submit, payload y cierre.
- Specs de permisos visibles para acciones habilitadas, ocultas o denegadas.

## Integracion

- Frontend llama rutas reales documentadas en `docs/backend/api-reference.md`.
- Modelos frontend coinciden con DTOs backend.
- No se exponen entidades backend completas si no corresponde.
- No se duplican reglas de negocio solo en frontend.
- Permisos frontend coinciden con backend.
- Cada flujo nuevo tiene al menos un caso exitoso y un caso de error/validacion.
- Cada flujo sensible tiene test de permiso/autorizacion.

## UX/UI

Validar en:

- `1366x768`.
- `1440x900`.
- `1920x1080`.
- `1024x768`.
- `390x844`.

Checklist:

- Accion primaria clara.
- Jerarquia visual clara.
- Densidad adecuada para uso interno.
- No hay overflow de pagina completa.
- Focus visible.
- Labels y errores legibles.
- Contraste suficiente en dark UI.

## Documentacion

Actualizar:

- Documento del modulo en `docs/modules`.
- API reference si cambia endpoint.
- Backend docs si cambia auth, datos, storage o migrations.
- Frontend docs si cambia ruta, layout, auth, sistema visual o patrones.
- `docs/development/testing-strategy.md` si cambia el criterio de testing.
- `docs/index.md` si se agrega un documento nuevo.

## Definicion de terminado

Una tarea no esta terminada si falta la cobertura automatica esperada para el cambio.

La excepcion debe ser explicita, justificada y acompanada por una validacion manual documentada.
