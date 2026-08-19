# Sistema Visual

Kodvian Core usa una UI oscura, tecnica y operativa, con acento verde neon controlado.

## Archivos principales

- `frontend/src/styles.scss`.
- `frontend/src/app/shared/styles/_page-common.scss`.
- `frontend/src/app/shared/styles/_table-common.scss`.
- `frontend/src/app/shared/styles/_dialog-common.scss`.
- `frontend/src/assets/branding/kodvian-isotipo.svg`.
- `frontend/src/assets/branding/kodvian-wordmark.svg`.

## Marca

- El isotipo compacto usa la composicion `/k;` plana en verde y fondo transparente.
- La marca textual visible debe mostrarse como `Kodvian solutions;` o `kodvian solutions;` segun el contexto visual.
- Usar `kodvian-isotipo.svg` en espacios chicos como header o navegacion.
- Usar `kodvian-wordmark.svg` en superficies donde la marca pueda respirar, como login.
- Evitar volver al isotipo 3D anterior para piezas de interfaz.
- Evitar glow, sombras o filtros pesados sobre el logo; el asset debe mantener lectura clara en tamanos chicos.

## Tokens globales

Variables definidas en `:root`:

- Fondos: `--bg-900`, `--bg-850`, `--bg-800`.
- Superficies: `--surface-700`, `--surface-650`.
- Marca: `--brand-500`, `--brand-400`, `--brand-300`.
- Texto: `--text-100`, `--text-300`.
- Lineas: `--line-600`.
- Estados: `--danger-500`, `--warning-500`.
- Sombras: `--shadow-soft`, `--shadow-glow`.

## Tipografia

Stack actual:

```css
'Montserrat', 'Poppins', 'Segoe UI', sans-serif
```

Reglas:

- Usar jerarquia clara sin tamanos exagerados.
- Evitar multiples niveles con el mismo peso visual.
- Mantener labels, ayudas y errores legibles.
- No exponer nombres tecnicos a usuarios de negocio.

## Angular Material

Se usa Angular Material con overrides globales para:

- Snackbars.
- Botones primary.
- Cards.
- Select panels.
- Autocomplete panels.
- Menus.
- Datepicker/calendar.
- Sidenav layout.
- Scrollbars.

## Patrones compartidos

`_page-common.scss` debe agrupar patrones de pagina:

- Headers.
- Filtros.
- Contenedores.
- Grillas de secciones.

`_table-common.scss` debe agrupar patrones de tablas/listas:

- Shell de tabla.
- Acciones de fila.
- Status chips.
- Estados de tabla.

`_dialog-common.scss` debe agrupar patrones de dialogs:

- Header.
- Body.
- Footer.
- Grillas responsivas de formulario.

## Reglas visuales

- Usar superficies neutras como base.
- Reservar verde brillante para accion primaria, foco, estados activos o marca.
- Evitar brillos decorativos excesivos.
- Evitar gradientes arbitrarios que bajen legibilidad.
- Mantener contraste fuerte en texto, tablas y formularios.
- Evitar estetica generica SaaS o dashboard generado.

## Criterios de aprobacion visual

- La tarea principal debe ser evidente.
- La densidad debe servir al trabajo diario.
- Las acciones deben estar priorizadas.
- Las tablas deben poder escanearse rapido.
- Los formularios deben agrupar campos por sentido funcional, no por orden de DTO.
- El resultado debe verificarse en screenshots para cambios visuales importantes.
