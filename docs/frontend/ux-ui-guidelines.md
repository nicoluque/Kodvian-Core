# Guia UX/UI

Esta guia define criterios para nuevas pantallas y mejoras visuales en Kodvian Core.

## Enfoque del producto

Kodvian Core es una herramienta interna operativa, no una landing page. La UI debe favorecer velocidad, claridad y control.

## Principios

- Priorizar tareas frecuentes.
- Mostrar informacion suficiente sin saturar.
- Usar tablas/listas densas cuando la informacion sea estructurada.
- Usar dialogs para tareas cortas.
- Usar paginas dedicadas para flujos complejos.
- Mantener feedback claro para loading, error, empty y success.
- Mostrar consecuencias de acciones sensibles.
- Evitar exponer IDs, DTOs o datos tecnicos a usuarios no tecnicos.

## Paginas

Cada pagina debe tener:

- Objetivo claro.
- Accion primaria unica.
- Filtros utiles y compactos.
- Estado inicial comprensible.
- Manejo de error visible.
- Empty state accionable.
- Responsive verificable.

## Formularios

Los formularios deben:

- Agrupar campos por sentido funcional.
- Diferenciar requeridos y opcionales.
- Mostrar errores cerca del campo.
- Evitar grillas uniformes si los campos tienen distinta longitud o importancia.
- Mantener acciones `Guardar` y `Cancelar` claramente separadas.

## Tablas y listas

Las tablas/listas deben:

- Priorizar columnas segun tarea del usuario.
- Alinear numeros y fechas de forma consistente.
- Mostrar estados con chips legibles.
- Mantener acciones de fila visibles.
- Usar paginacion cuando haya volumen.
- Evitar columnas tecnicas innecesarias.

## Dialogs

Los dialogs deben:

- Tener titulo especifico.
- Tener contenido agrupado.
- Evitar scroll doble innecesario.
- Cerrar con feedback despues de accion exitosa.
- Manejar errores de API.
- Ser usables en mobile.

## Anti-patrones

- Hero sections en paginas internas.
- Cards decorativas sin decision operativa.
- Exceso de glow o gradientes.
- Botones primarios multiples en el mismo contexto.
- Formularios que replican el DTO sin criterio de usuario.
- Tablas convertidas en cards en desktop sin razon.
- Ocultar acciones criticas solo en hover.
- Dar por aprobada una UI sin verla renderizada.

## Criterio de aprobacion

Una pantalla esta lista cuando:

- El usuario entiende que hacer en menos de unos segundos.
- La accion principal esta clara.
- La informacion clave aparece antes que detalles secundarios.
- Los estados de UI estan cubiertos.
- La experiencia funciona en desktop y mobile.
- La validacion tecnica pasa.
- Si hubo cambio visual sustancial, existe revision con screenshot.
