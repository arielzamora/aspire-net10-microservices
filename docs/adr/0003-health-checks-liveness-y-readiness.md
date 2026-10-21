# 0003 — Health checks: liveness vs. readiness y su exposición

* Estado: aceptado e implementado (se comprobará en producción cuando se despliegue a Azure)

## Contexto y problema

Los orquestadores necesitan saber si un servicio está vivo (liveness) y si puede recibir tráfico (readiness).
El template de ServiceDefaults mapea `/health` y `/alive` **solo en Development**, porque exponerlos en producción
tiene implicaciones de seguridad (pueden filtrar el estado de dependencias). Azure Container Apps usa probes HTTP,
así que hay que decidir cómo exponerlos sin filtrar información.

## Punto de partida (verificado en el código antes del cambio)

* `/alive`: solo corren los checks con la etiqueta `live` (el check `self`, siempre sano). `MyHealtCheck` se agrega esa
  etiqueta a sí mismo al ejecutarse (`context.Registration.Tags.Add("live")`), de modo que desde su primera ejecución
  también cuenta para `/alive`.
* `/health`: todos los checks deben pasar. Incluye el check de la base de datos que registra el cliente de Aspire y el
  check de demostración `MyHealtCheck` de la API, que se puede poner sano o no sano con `GET /make-healthy` y
  `GET /make-unhealthy` (parte de las demos del curso).
* Ambos endpoints solo existen cuando `IsDevelopment()`.

## Decisión

* **Liveness (`/alive`)** responde solo "el proceso responde", sin consultar dependencias.
* **Readiness (`/health`)** incluye las dependencias (base de datos), pero no devuelve detalle: solo el código HTTP.
* En Azure se exponen únicamente para las probes de Container Apps, sin detalle en el cuerpo, y sin publicar los
  endpoints de demo (`make-healthy`, `make-unhealthy`) en producción.

## Implementación

* `MasterNet.ServiceDefaults`: fuera de Development, `MapDefaultEndpoints` solo mapea `/health` y `/alive` si
  `HealthChecks:Expose` es `true`. La respuesta es solo el estado (`Healthy` / `Unhealthy`).
* `MasterNet.AppHost` (modo publicación): define `HealthChecks__Expose=true` y las probes HTTP de la API y del
  `RatingService` (`/alive` como liveness y `/health` como readiness) con `WithHttpProbe`, una API experimental de Aspire
  (`ASPIREPROBES001`) que se acepta de forma explícita. Las probes quedan en el Bicep generado (`infra/`).
* `HealthToogleController`: `make-healthy` y `make-unhealthy` responden `404` fuera de Development, porque permitirían a
  cualquiera poner la app "no sana".
* Tests: `HealthEndpointsTests` comprueba que sin la opción los endpoints responden `404`, que con la opción responden
  solo el estado, y que las demos no existen fuera de Development.

## Consecuencias

* El cliente Blazor, servido por nginx, no tiene probes: Container Apps usa las de TCP por defecto.
* Los endpoints de demo del curso en `/Demo/...` no están protegidos; no exponen secretos, pero conviene retirarlos si se publican.
