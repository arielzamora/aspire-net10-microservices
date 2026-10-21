# 0005 — Estrategia de migraciones de base de datos

* Estado: aceptado para desarrollo; producción propuesta (se implementa en la Parte 2.3)

## Contexto y problema

El curso muestra tres formas de crear el esquema y sembrar datos. Hay que dejar claro cuál usa la solución, para qué sirve
cada una y qué se usaría fuera de desarrollo.

## Las tres formas (verificadas en el código)

| Forma | Dónde | Cómo funciona | Estado |
|---|---|---|---|
| **Scripts SQL** | `Database/CreateDatabaseAndSeed.sql` + `MasterNet.MigrationSqlService` | Un worker ejecuta el script contra `master`, partido por `GO`. El script **borra y recrea** `MasterNetDb` y siembra roles, precios, instructores, cursos y ratings. | **Activo** en el AppHost |
| **EF Core** | `MasterNet.MigrationService` + `Persistence/Migrations` | Un worker crea la base si no existe, aplica `Database.MigrateAsync()` (migración `InitialMigration`) y siembra con C#. | Disponible, **comentado** en el AppHost |
| **Docker** | `docker-compose.yml` + `Database/Dockerfile` + `wait-and-run.sh` | Un contenedor espera a SQL Server y ejecuta el mismo script con `sqlcmd`. La contraseña viene de `.env`. | Alternativa sin Aspire |

## Decisión

* **Desarrollo:** migration worker con script SQL (`MigrationSqlService`), por ser determinista y dar una base limpia en cada
  arranque. La API espera con `WaitForCompletion` (ver [0004](0004-lifecycle-eventing-vs-migration-worker.md)).
* **EF Core** se conserva como alternativa documentada para equipos que prefieren el modelo como fuente de verdad.
* **Producción (propuesto, Parte 2.3):** no se puede usar el script actual, porque borra la base. Se usará un script
  **idempotente** (por ejemplo, generado con `dotnet ef migrations script --idempotent`) o un **bundle** de EF Core
  (`dotnet ef migrations bundle`), ejecutado como paso del pipeline antes del despliegue.

## Consecuencias

* En desarrollo los datos creados no persisten entre arranques del AppHost.
* Hay dos fuentes de verdad del esquema (el script SQL y las migraciones de EF): deben mantenerse alineadas si el modelo cambia.
* Los usuarios no se siembran en el script: los crea la API en Development (ver [0009](0009-autenticacion-y-autorizacion.md)).
