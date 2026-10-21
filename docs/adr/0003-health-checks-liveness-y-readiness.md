# 0003 — Health checks: liveness vs. readiness y su exposición

* Estado: propuesto (la exposición en producción se resuelve en la Parte 2.3)

## Contexto y problema

Los orquestadores necesitan saber si un servicio está vivo (liveness) y si puede recibir tráfico (readiness).
El template de ServiceDefaults mapea `/health` y `/alive` **solo en Development**, porque exponerlos en producción
tiene implicaciones de seguridad (pueden filtrar el estado de dependencias). Azure Container Apps usa probes HTTP,
así que hay que decidir cómo exponerlos sin filtrar información.

## Estado actual (verificado en el código)

* `/alive`: solo corren los checks con la etiqueta `live` (el check `self`, siempre sano). `MyHealtCheck` se agrega esa
  etiqueta a sí mismo al ejecutarse (`context.Registration.Tags.Add("live")`), de modo que desde su primera ejecución
  también cuenta para `/alive`.
* `/health`: todos los checks deben pasar. Incluye el check de la base de datos que registra el cliente de Aspire y el
  check de demostración `MyHealtCheck` de la API, que se puede poner sano o no sano con `GET /make-healthy` y
  `GET /make-unhealthy` (parte de las demos del curso).
* Ambos endpoints solo existen cuando `IsDevelopment()`.

## Decisión propuesta

* **Liveness (`/alive`)** responde solo "el proceso responde", sin consultar dependencias.
* **Readiness (`/health`)** incluye las dependencias (base de datos), pero no devuelve detalle: solo el código HTTP.
* En Azure se exponen únicamente para las probes de Container Apps, sin detalle en el cuerpo, y sin publicar los
  endpoints de demo (`make-healthy`, `make-unhealthy`) en producción.

## Consecuencias

* La implementación (condicionar `MapDefaultEndpoints` y configurar las probes) se hace en la Parte 2.3.
* Mientras tanto, los endpoints de demo siguen disponibles porque forman parte del material del curso.
