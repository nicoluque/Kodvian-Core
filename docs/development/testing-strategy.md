# Estrategia De Testing

Kodvian Core debe evolucionar con tests automaticos para todos los circuitos, flujos y operaciones relevantes del sistema.

## Estado actual

Backend:

- Existe el proyecto `backend/tests/Kodvian.Core.Application.Tests`.
- Usa xUnit, `Microsoft.NET.Test.Sdk` y `coverlet.collector`.
- La cobertura actual es placeholder: `UnitTest1.cs` contiene un test vacio.

Frontend:

- Existe `frontend/src/app/app.component.spec.ts`.
- La cobertura actual valida solo la creacion de `AppComponent`.
- No hay specs reales detectadas para modulos, servicios, guards, interceptor, dialogs o flujos funcionales.

## Regla obligatoria

Ninguna funcionalidad nueva se considera terminada si no incluye tests automaticos proporcionales al cambio.

Si una mejora agrega, modifica o corrige comportamiento, debe incluir tests que validen ese comportamiento.

Si no es posible automatizar una validacion, debe quedar documentado el motivo y el procedimiento manual minimo en el documento del modulo o en el checklist de validacion.

## Backend

Los cambios backend deben cubrirse con tests segun el tipo de comportamiento.

### Unit tests

Usar para:

- Reglas de negocio puras.
- Validaciones de request DTOs cuando no requieran HTTP real.
- Calculos financieros.
- Transiciones de estado.
- Mapeos de DTOs.
- Permission maps y reglas de roles.
- Servicios cuando puedan aislarse con dependencias fake/mock.

### Integration tests

Usar para:

- Controllers y endpoints HTTP.
- Autenticacion y autorizacion.
- Contratos request/response.
- Validacion de status codes.
- Persistencia EF Core cuando importe la query real.
- Upload/download de archivos.
- Flujos que crucen controller, service y DbContext.

### Persistencia y datos

Cubrir cuando haya:

- Nuevas entidades.
- Nuevas relaciones.
- Nuevos indices o constraints relevantes.
- Migrations con transformacion de datos.
- Queries con filtros, paginacion, ordenamiento o includes.

### Seguridad

Cubrir:

- Login exitoso y fallido.
- Sesion actual `/api/auth/me`.
- Logout.
- Policies y permisos.
- Acceso denegado para roles sin permiso.
- Acceso permitido para roles correctos.
- Rutas sensibles sin autenticacion.

### Storage

Cubrir:

- PDF valido.
- Archivo no PDF.
- Archivo excedido de tamano.
- Descarga de archivo existente.
- Eliminacion de comprobante.
- Owner correcto del archivo.

## Frontend

Los cambios frontend deben cubrirse con specs segun el tipo de comportamiento.

### Servicios API

Cubrir:

- URL correcta.
- Metodo HTTP correcto.
- Query params.
- Body enviado.
- Mapeo de respuesta.
- Manejo de errores esperados.

### Auth, guards e interceptor

Cubrir:

- Login exitoso.
- Login fallido.
- Restauracion de sesion con `/api/auth/me`.
- Redireccion a login si no hay sesion.
- Redireccion a dashboard si falta permiso de administracion.
- `withCredentials` aplicado por interceptor.
- Limpieza de sesion ante HTTP 401.

### Componentes y paginas

Cubrir:

- Render inicial.
- Loading state.
- Empty state.
- Error state.
- Success state.
- Apertura/cierre de dialogs.
- Acciones principales.
- Filtros.
- Paginacion.
- Cambio de vista cuando exista, como listado/kanban.

### Formularios y dialogs

Cubrir:

- Validaciones requeridas.
- Estados invalidos.
- Submit correcto.
- Payload enviado al service.
- Errores de API.
- Cancelacion/cierre.
- Campos dependientes.

### Permisos visibles

Cubrir:

- Acciones visibles para rol autorizado.
- Acciones ocultas o deshabilitadas para rol no autorizado.
- Mensajes o redirecciones esperadas.

## Cobertura minima por flujo

Cada flujo funcional debe tener al menos:

- Un test de caso exitoso.
- Un test de validacion o error esperado.
- Un test de permiso cuando el flujo sea sensible.
- Un test de integracion si cruza API y persistencia.

## Flujos prioritarios

Priorizar cobertura en este orden:

1. Auth y sesion.
2. Permisos y administracion.
3. Clientes.
4. Proyectos y documentos.
5. Tareas.
6. Finanzas y comprobantes.
7. Desarrolladores, contratos y pagos.
8. Dashboard.
9. Locations y lookups.

## Regla para bugs

Toda correccion de bug debe agregar un test que falle antes del fix y pase despues.

Si el bug fue visual y no puede testearse unitariamente, agregar test de componente cuando sea posible y documentar evidencia visual requerida.

## Definicion de terminado

Una mejora esta terminada cuando:

- Compila backend y frontend si fueron afectados.
- Ejecutan los tests automaticos relevantes.
- Los nuevos comportamientos tienen cobertura.
- Los tests existentes siguen pasando.
- La documentacion del modulo fue actualizada.
- El checklist de validacion fue completado para el alcance del cambio.

## Comandos base

Backend:

```bash
dotnet test backend/Kodvian.Core.slnx
```

Frontend:

```bash
npm test
```

Build frontend:

```bash
npm run build
```

## Trabajo pendiente recomendado

- Reemplazar `UnitTest1.cs` por tests reales.
- Agregar tests backend para servicios y endpoints criticos.
- Evaluar un proyecto separado de integration tests si la suite crece.
- Agregar specs Angular para servicios, guards, interceptor, paginas y dialogs.
- Evitar que nuevas mejoras aumenten deuda de cobertura.
