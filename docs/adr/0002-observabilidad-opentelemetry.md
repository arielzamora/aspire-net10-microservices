# 0002 — Observabilidad con OpenTelemetry y exportador OTLP

* Estado: aceptado

## Contexto y problema

Con varios servicios hace falta correlacionar logs, métricas y trazas de una petición que atraviesa
cliente → API → `RatingService` → Redis/SQL, sin atar el código a un backend de telemetría concreto.

## Opciones consideradas

* OpenTelemetry con exportador OTLP (backend intercambiable).
* SDK específico de un proveedor (por ejemplo, Application Insights directo).

## Decisión

Se usa OpenTelemetry configurado una sola vez en `MasterNet.ServiceDefaults/Extensions.cs`:

* **Logs:** `ILogger` → OpenTelemetry (`IncludeFormattedMessage`, `IncludeScopes`).
* **Métricas:** ASP.NET Core, `HttpClient`, runtime y el medidor propio `MasterNet.Course`.
* **Trazas:** ASP.NET Core, `HttpClient`, `SqlClient` y la fuente propia `MasterNet.Course`
  (por ejemplo, el span "Obtengo las Calificaciones" de `GetCoursesQuery`). Se excluyen `/health` y `/alive`.
* **Exportador:** `UseOtlpExporter()` solo si existe `OTEL_EXPORTER_OTLP_ENDPOINT`. En local lo define Aspire y el
  destino es el dashboard. El exportador de Azure Monitor está comentado en el template, listo para activarse
  (`APPLICATIONINSIGHTS_CONNECTION_STRING`) cuando se despliegue.

## Consecuencias

* El mismo código sirve con el dashboard de Aspire en local y con Azure Monitor / Application Insights en Azure.
* Cada servicio obtiene la configuración con `AddServiceDefaults()`.
* Limitación verificada: las `ActivitySource` de los workers de migración (`Migrations`, `SQLMigrations`) no están
  agregadas con `AddSource` en ServiceDefaults, por lo que sus spans propios no se exportan. Queda como mejora.
