# Plan de Pruebas Funcionales Manuales - Kodvian Core

Este plan esta orientado a ejecucion manual para detectar fallas funcionales reales antes de salir a QA formal o despliegue.

## Alcance

- Login
- Cierre de sesion
- Navegacion
- Clientes
- Proyectos
- Tareas
- Finanzas
- Dashboard
- Permisos basicos

## Datos de prueba base

- Usuario administrador:
  - Correo: `admin@kodvian.local`
  - Contrasena: `Admin12345!`

## Criterio de aprobacion

- Casos **Alta**: 100% aprobados.
- Casos **Media**: minimo 90% aprobados y sin defectos criticos abiertos.
- Casos **Baja**: se registran como mejoras, no bloquean salida.

## Formato de ejecucion sugerido

- Resultado: `Pass` o `Fail`
- Evidencia: captura, video corto o log
- Observaciones: comportamiento observado + pasos de reproduccion

## Casos de prueba

> Version en CSV: `scripts/qa-plan-pruebas-funcionales.csv`

### Login y sesion

1) **LG-01 (Alta)** - Login exitoso
- Pasos: abrir `/login`, ingresar credenciales validas, enviar.
- Esperado: redirige a dashboard con sesion activa.

2) **LG-02 (Alta)** - Credenciales invalidas
- Pasos: ingresar contrasena incorrecta.
- Esperado: mensaje de error y sin sesion.

3) **LG-03 (Alta)** - Campos obligatorios
- Pasos: enviar formulario vacio.
- Esperado: validaciones visibles en correo y contrasena.

4) **LG-04 (Media)** - Correo invalido
- Pasos: ingresar correo con formato incorrecto.
- Esperado: validacion de formato.

5) **LO-01 (Alta)** - Cerrar sesion
- Pasos: login, click en logout.
- Esperado: redirige a `/login` y cierra sesion.

6) **LO-02 (Alta)** - Bloqueo sin sesion
- Pasos: sin sesion, abrir `/dashboard`.
- Esperado: redireccion a `/login`.

### Navegacion

7) **NAV-01 (Alta)** - Menu principal
- Pasos: recorrer Dashboard, Clientes, Proyectos, Tareas, Finanzas y Administracion.
- Esperado: todas las vistas cargan sin error.

8) **NAV-02 (Media)** - Ruta invalida
- Pasos: abrir URL inexistente.
- Esperado: redireccion controlada.

9) **NAV-03 (Media)** - Refresh con sesion
- Pasos: con sesion activa refrescar en modulo interno.
- Esperado: mantiene sesion y estado estable.

### Clientes

10) **CLI-01 (Alta)** - Alta valida de cliente
- Pasos: crear cliente con campos minimos validos.
- Esperado: guarda y aparece en listado.

11) **CLI-02 (Alta)** - Validaciones de cliente
- Pasos: intentar guardar sin nombre o email invalido.
- Esperado: bloquea guardado con mensajes.

12) **CLI-03 (Media)** - Filtros y paginacion
- Pasos: buscar, filtrar por estado y cambiar pagina.
- Esperado: resultados coherentes con filtro/paginacion.

### Proyectos

13) **PRO-01 (Alta)** - Alta valida de proyecto
- Pasos: crear proyecto con cliente y datos minimos.
- Esperado: guarda y figura en listado.

14) **PRO-02 (Alta)** - Validacion de fechas
- Pasos: fecha de entrega/cierre menor a inicio.
- Esperado: bloquea guardado con mensaje.

15) **PRO-03 (Media)** - Filtros de proyecto
- Pasos: aplicar filtros por estado/prioridad y limpiar.
- Esperado: aplica y reinicia correctamente.

### Tareas

16) **TAR-01 (Alta)** - Alta valida de tarea
- Pasos: crear tarea con datos minimos.
- Esperado: guarda y aparece en lista.

17) **TAR-02 (Alta)** - Fechas invalidas en tarea
- Pasos: vencimiento menor a inicio.
- Esperado: validacion y no guarda.

18) **TAR-03 (Media)** - Vista lista/tablero
- Pasos: alternar lista y tablero.
- Esperado: ambas vistas estables.

19) **TAR-04 (Media)** - Rango de fechas invalido en filtros
- Pasos: desde mayor que hasta, aplicar.
- Esperado: mensaje de error y no rompe pantalla.

### Finanzas

20) **FIN-01 (Alta)** - Alta de ingreso y egreso
- Pasos: crear un ingreso y un egreso validos.
- Esperado: ambos se guardan y listan.

21) **FIN-02 (Alta)** - Fecha de vencimiento invalida
- Pasos: vencimiento menor a fecha de movimiento.
- Esperado: validacion y bloqueo de guardado.

22) **FIN-03 (Alta)** - Categorias semilla disponibles
- Pasos: abrir formulario de movimiento y revisar selector de categorias.
- Esperado: aparecen categorias base requeridas.

23) **FIN-04 (Media)** - Filtros y paginacion de movimientos
- Pasos: filtrar por tipo/estado/fecha y paginar.
- Esperado: resultados consistentes.

### Dashboard

24) **DAS-01 (Alta)** - Carga general
- Pasos: abrir dashboard con sesion activa.
- Esperado: KPIs y tablas cargan sin error.

25) **DAS-02 (Media)** - Estado vacio
- Pasos: probar con datos limitados.
- Esperado: mensaje vacio correcto sin fallas visuales.

### Permisos basicos

26) **PER-01 (Alta)** - Acceso administracion con admin
- Pasos: login admin, abrir `/administracion`.
- Esperado: acceso permitido.

27) **PER-02 (Alta)** - Bloqueo administracion sin permiso
- Pasos: login con usuario sin permiso, abrir `/administracion`.
- Esperado: redireccion o bloqueo de acceso.

28) **PER-03 (Media)** - Acceso directo por URL protegida
- Pasos: sin sesion, abrir rutas privadas por URL.
- Esperado: redireccion a login.
