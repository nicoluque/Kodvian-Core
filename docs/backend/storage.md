# Storage

Kodvian Core soporta almacenamiento local en desarrollo y almacenamiento S3-compatible para produccion.

## Archivos principales

- `backend/src/Kodvian.Core.Application/Common/Files/IFileStorageService.cs`.
- `backend/src/Kodvian.Core.Infrastructure/Services/LocalFileStorageService.cs`.
- `backend/src/Kodvian.Core.Infrastructure/Services/S3FileStorageService.cs`.
- `backend/src/Kodvian.Core.Infrastructure/Storage/StorageOptions.cs`.
- `backend/src/Kodvian.Core.Infrastructure/Extensions/DependencyInjectionExtensions.cs`.

## Providers

### Local

Usado en Development.

Guarda archivos en disco local, por defecto bajo `App_Data/files`.

No usar como storage productivo en contenedores, porque el filesystem puede ser efimero.

### S3

Usado para Production.

Compatible con S3, Cloudflare R2 o MinIO segun configuracion.

Variables relevantes:

- `Storage__Provider=S3`.
- `Storage__Bucket`.
- `Storage__AccessKey`.
- `Storage__SecretKey`.
- `Storage__ServiceUrl`.
- `Storage__Region`.
- `Storage__ForcePathStyle`.
- `Storage__MaxPdfSizeMb`.

## Validacion de archivos

Los uploads actuales estan orientados a PDFs:

- Se valida tamano maximo.
- Se valida content type.
- Se valida magic header `%PDF`.
- Se calcula y guarda SHA-256.

## Casos de uso

- Documentos de proyecto.
- Versiones de documentos de proyecto.
- Comprobantes de movimientos financieros.
- Comprobantes de pagos a desarrolladores.

## Modelo relacionado

`DocumentFile` guarda metadata del archivo:

- Nombre original.
- Nombre almacenado.
- Content type.
- Storage path.
- SHA-256.
- Usuario que subio el archivo.
- Owner funcional.

## Cuidados

- No confiar solo en extension de archivo.
- No exponer paths internos de storage al frontend.
- Para produccion, configurar S3 antes del primer deploy.
- Eliminar archivo fisico/remoto cuando el flujo realmente borre comprobantes.
- Diferenciar baja logica de documentos de proyecto vs eliminacion de comprobantes.
