# 0009 — Autenticación y autorización

* Estado: aceptado

## Contexto y problema

Crear, editar y borrar cursos es una operación restringida; el listado y la lectura por id son públicos. El cliente Blazor
necesita autenticarse para usar las operaciones de escritura.

## Decisión

* **API:** JWT firmado con HMAC-SHA256. El token incluye un claim `POLICIES` por cada política del rol del usuario
  (tomadas de `AspNetRoleClaims`). Las políticas (`COURSE_WRITE`, `COURSE_UPDATE`, `COURSE_DELETE`, ...) se definen en
  `PoliciesConfiguration` y se aplican con `[Authorize(Policy = ...)]` en los controllers.
  `GET /api/courses` y `GET /api/courses/{id}` son `[AllowAnonymous]`.
* **Roles sembrados** (`CreateDatabaseAndSeed.sql`): `ADMIN` con todas las políticas de cursos, instructores y ratings;
  `CLIENT` con lectura y `RATING_CREATE`. El registro de usuarios asigna siempre `CLIENT`.
* **Usuarios de prueba:** la API los crea al arrancar **solo en Development** (admin `azamora@gmail.com` y cliente
  `john.doe@gmail.com`), con la contraseña que aporta el parámetro `seed-admin-password` (ver [0008](0008-gestion-de-secretos.md)).
  Si el parámetro no existe, el seed se omite.
* **Cliente Blazor:** login mínimo en `/login` contra `POST /api/account/login`. El token se guarda en `sessionStorage`
  (una sesión por pestaña) y un `DelegatingHandler` lo agrega como `Bearer` a las llamadas de Refit. Los botones de alta,
  edición y baja solo se muestran con sesión iniciada; la API sigue siendo quien decide con `401` y `403`.

## Alternativas descartadas

* Relajar las políticas en Development: dejaba la API sin autorización real justo donde se prueba.
* Guardar el token en `localStorage`: más persistente, pero más expuesto; para esta demo se prefirió la sesión por pestaña.

## Consecuencias y límites conocidos

* El token vence a los 7 días y no hay refresh token ni cierre de sesión en el servidor.
* La validación de JWT no comprueba emisor ni audiencia (`ValidateIssuer` y `ValidateAudience` en `false`).
* CORS permite cualquier origen (`WithOrigins("*")`).
* Son aceptables para un demo; para producción habría que endurecerlos (ver el README, "Qué cambiaría para producción").
