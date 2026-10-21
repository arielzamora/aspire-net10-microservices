# 0001 — Usar .NET Aspire para orquestar la solución

* Estado: aceptado

## Contexto y problema

La solución tiene varias piezas que deben arrancar en orden y conocerse entre sí: Web API, cliente Blazor, `RatingService`,
SQL Server, Redis y los servicios de migración. Levantarlas a mano (o con un `docker-compose` más scripts) obliga a repetir
cadenas de conexión, puertos y secretos en cada proyecto, y no da una vista común de logs, métricas y trazas.

## Opciones consideradas

* **.NET Aspire** (AppHost + ServiceDefaults).
* `docker-compose` con todos los servicios, incluidas las aplicaciones .NET.
* Arranque manual de cada proyecto desde el IDE.

## Decisión

Se usa .NET Aspire. El `MasterNet.AppHost` declara los recursos y sus dependencias en C#:

* SQL Server (`AddSqlServer`) y Redis (`AddRedis`) como contenedores persistentes.
* `api`, `client`, `ratingservice` y `migrationSql` como proyectos, con `WithReference`, `WaitFor` y `WaitForCompletion`.
* Service discovery: la API llama al servicio de ratings como `http://ratingservice`, sin URLs fijas.
* Secretos como parámetros (ver [0008](0008-gestion-de-secretos.md)).

`MasterNet.ServiceDefaults` aporta a cada servicio OpenTelemetry, health checks, resiliencia y service discovery
(ver [0002](0002-observabilidad-opentelemetry.md)).

## Consecuencias

* Un solo comando (`aspire run`) levanta toda la topología, con dashboard de recursos, logs y trazas.
* El orden de arranque está en el código (`WaitFor`), no en documentación.
* `Aspire.Hosting.Testing` permite probar la topología real (ver [0006](0006-estrategia-de-testing.md)).
* Hay dependencia de Docker en local y de la CLI/SDK de Aspire; el modelo de despliegue a Azure se basa en `azd`
  (ver [0007](0007-sql-server-local-y-azure-sql.md)).
