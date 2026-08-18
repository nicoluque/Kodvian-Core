# Riesgos Operativos Backend

Este documento lista riesgos tecnicos conocidos para considerar al implementar mejoras.

## Autorizacion parcial

Muchos controllers usan `[Authorize]` sin policies finas por permiso.

Riesgo:

- Usuarios autenticados podrian acceder a operaciones que deberian estar limitadas por modulo si no se agrega control adicional.

Mitigacion:

- Agregar policies o checks de permisos para nuevos endpoints sensibles.
- No confiar en que el frontend oculte botones.

## Migraciones automaticas al startup

`Program.cs` aplica migrations pendientes al iniciar.

Riesgo:

- Una migration destructiva puede ejecutarse automaticamente en produccion.
- Startups concurrentes pueden generar conflictos si se escala a multiples replicas.

Mitigacion:

- Revisar migrations antes del deploy.
- Considerar pipeline controlado de migrations si el sistema crece.

## Storage local en contenedores

Development usa storage local.

Riesgo:

- En Railway o contenedores, el disco local puede ser efimero.

Mitigacion:

- Usar `Storage__Provider=S3` en produccion.
- Verificar upload/download post-deploy.

## Validacion de PDFs basica

El backend valida content type, tamano y magic header.

Riesgo:

- No reemplaza antivirus, sanitizacion ni analisis profundo de PDF.

Mitigacion:

- Si se aceptan archivos externos no confiables, agregar scanning o procesamiento seguro.

## Healthcheck superficial

`/healthz` valida disponibilidad del proceso.

Riesgo:

- Puede responder OK aunque PostgreSQL o S3 tengan problemas.

Mitigacion:

- Agregar health checks profundos si produccion lo requiere.

## UsersController incompleto

`GET /api/users` existe, pero actualmente devuelve un resultado vacio.

Riesgo:

- La UI de administracion puede parecer funcional aunque no muestre usuarios reales.

Mitigacion:

- Implementar listado real antes de depender de administracion de usuarios.

## CORS opcional

Si `Cors:AllowedOrigins` no esta configurado, no se registra policy CORS.

Riesgo:

- Correcto para SPA mismo origen, pero bloquea clientes externos.

Mitigacion:

- Configurar origins cuando el frontend viva en otro dominio.

## Secretos y seed productivo

Produccion requiere JWT y admin seed validos.

Riesgo:

- Faltantes causan fail-fast al iniciar.
- Valores debiles comprometen seguridad.

Mitigacion:

- Setear variables antes del primer deploy.
- No usar placeholders ni passwords por defecto.
