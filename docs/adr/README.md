# Architecture Decision Records

Decisiones de arquitectura de la solución, en formato [MADR](https://adr.github.io/madr/) y numeradas.

| # | Decisión | Estado |
|---|---|---|
| [0001](0001-usar-dotnet-aspire.md) | Usar .NET Aspire para orquestar la solución | Aceptado |
| [0002](0002-observabilidad-opentelemetry.md) | Observabilidad con OpenTelemetry y exportador OTLP | Aceptado |
| [0003](0003-health-checks-liveness-y-readiness.md) | Health checks: liveness vs. readiness y su exposición | Aceptado |
| [0004](0004-lifecycle-eventing-vs-migration-worker.md) | Lifecycle eventing vs. migration worker | Aceptado |
| [0005](0005-estrategia-de-migraciones.md) | Estrategia de migraciones de base de datos | Aceptado (desarrollo) / Propuesto (producción) |
| [0006](0006-estrategia-de-testing.md) | Testing: integración tradicional vs. `Aspire.Hosting.Testing` | Aceptado |
| [0007](0007-sql-server-local-y-azure-sql.md) | SQL Server en contenedor local y Azure SQL en la nube | Propuesto |
| [0008](0008-gestion-de-secretos.md) | Gestión de secretos | Aceptado (desarrollo) / Propuesto (Azure) |
| [0009](0009-autenticacion-y-autorizacion.md) | Autenticación y autorización | Aceptado |

"Propuesto" indica que la decisión está tomada pero su implementación pertenece al despliegue a Azure, aún pendiente.
