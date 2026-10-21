# 0004 — Lifecycle eventing vs. migration worker para inicializar la base

* Estado: aceptado

## Contexto y problema

La base necesita esquema y datos semilla antes de que la API sirva tráfico. Aspire ofrece dos mecanismos:
eventos del ciclo de vida del AppHost y servicios dedicados que corren y terminan.

## Opciones consideradas

* **Eventos del AppHost** (`BeforeStartEvent`, `ResourceReadyEvent`, `ConnectionStringAvailableEvent`, ...).
* **Migration worker**: un proyecto que migra/siembra y termina, con `WaitForCompletion` en quien lo necesita.

## Decisión

* La inicialización de la base se hace con **migration workers** (`MasterNet.MigrationSqlService` está activo en el AppHost,
  ver [0005](0005-estrategia-de-migraciones.md)) y la API espera con `api.WaitForCompletion(migrationSql)`.
* Los **eventos del AppHost** se usan para observar y reaccionar sin bloquear el arranque. En el código:
  `SubscribeToResourcetEvents` (activo) registra `BeforeResourceStartedEvent`, `ResourceReadyEvent` y
  `ConnectionStringAvailableEvent` de la API; `SubscribeToAppHostEvents` (desactivado) hace lo propio con
  `BeforeStartEvent`, `ResourceEndpointsAllocatedEvent` y `AfterResourcesCreatedEvent`. Hoy solo escriben logs: son
  demostraciones del curso.

## Consecuencias

* El worker es un recurso más del dashboard: tiene logs, trazas, estado y puede reintentarse; el orden queda explícito.
* Un evento corre dentro del AppHost: un error ahí afecta al propio orquestador y no deja un servicio observable.
* Los eventos quedan reservados para integraciones ligeras (diagnóstico, notificaciones) y no para trabajo pesado o
  con efectos sobre los datos.
