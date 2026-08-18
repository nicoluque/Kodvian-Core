# Frontend Overview

El frontend de Kodvian Core es una aplicacion Angular 19 con standalone components, Angular Material y SCSS.

## Archivos principales

- `frontend/src/main.ts`.
- `frontend/src/app/app.config.ts`.
- `frontend/src/app/app.routes.ts`.
- `frontend/src/styles.scss`.
- `frontend/proxy.conf.json`.
- `frontend/angular.json`.

## Estructura

```text
frontend/src/app/
  core/
    auth/
    guards/
    http/
    services/
  layout/
  modules/
    dashboard/
    clientes/
    desarrolladores/
    proyectos/
    tareas/
    finanzas/
    administracion/
  shared/
```

## Arquitectura frontend

- `LoginPageComponent` vive fuera del layout autenticado.
- `MainLayoutComponent` envuelve las rutas autenticadas.
- Cada modulo define su archivo `*.routes.ts`.
- Los servicios por modulo consumen endpoints relativos `/api/...`.
- Los modelos frontend viven junto al modulo que los usa.
- Estilos compartidos viven en `frontend/src/app/shared/styles`.

## Comunicacion con backend

- En desarrollo, `npm start` usa `proxy.conf.json` hacia `http://localhost:5297`.
- En produccion, la API sirve el SPA y se usan rutas relativas bajo el mismo origen.
- `auth.interceptor.ts` agrega `withCredentials: true` a las requests para enviar la cookie `auth_token`.

## Dependencias principales

- Angular 19.
- Angular Material 19.
- RxJS 7.8.
- TypeScript 5.7.
- Karma/Jasmine para tests.

## Comandos utiles

Desde `frontend/`:

```bash
npm install
npm start
npm run build
npm test
```

## Cuidados

- Mantener texto visible en espanol latino.
- No duplicar contratos backend si ya existen modelos compartidos por modulo.
- Validar responsive en desktop y mobile.
- Para cambios visuales relevantes, revisar screenshots antes de aprobar.
