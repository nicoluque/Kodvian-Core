# Responsive Y Accesibilidad

## Viewports objetivo

Validar cambios relevantes al menos en:

- `1366x768`.
- `1440x900`.
- `1920x1080`.
- `1024x768`.
- `390x844`.

## Responsive actual

- El layout cambia sidebar a modo mobile bajo `960px`.
- Header reduce informacion bajo `800px`.
- Dialog grids compartidos colapsan bajo `900px`.
- Tablas usan overflow horizontal cuando no entran columnas.

## Criterios responsive

- No debe haber overflow horizontal de pagina completa.
- Tablas pueden tener scroll horizontal controlado dentro de su contenedor.
- Acciones primarias deben seguir visibles en mobile.
- Dialogs deben poder leerse y cerrarse en pantallas chicas.
- Filtros deben colapsar sin perder jerarquia.
- Textos largos de cliente, proyecto o desarrollador no deben romper layout.

## Accesibilidad minima

- Mantener `html lang="es"`.
- Botones icon-only deben tener `aria-label`.
- Inputs deben tener labels visibles.
- Errores deben estar asociados visualmente al campo.
- Dialogs deben conservar foco y cierre por Escape cuando Material lo permita.
- Focus visible no debe ser eliminado por estilos.
- Contraste de texto y estados debe ser suficiente sobre fondos oscuros.
- No usar color como unico indicador de estado.

## Checklist por cambio visual

1. Navegar con teclado.
2. Verificar foco en botones, links, inputs y dialogs.
3. Verificar contraste en textos secundarios.
4. Probar mobile y desktop.
5. Probar loading, error, empty y datos reales/largos.
6. Confirmar que acciones destructivas son distinguibles.

## Riesgos actuales

- Uso de `::ng-deep` y overrides globales puede ser fragil ante upgrades de Material.
- Algunas cadenas visibles omiten acentos.
- `window.confirm` y `window.open` reducen consistencia UX y testabilidad.
- No hay una estrategia ARIA amplia documentada mas alla de usos puntuales.
