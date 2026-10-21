# 0006 — Estrategia de testing: integración tradicional vs. `Aspire.Hosting.Testing`

* Estado: aceptado

## Contexto y problema

Hay que probar la lógica y el contrato de la API, y también el cableado real de la solución (service discovery,
configuración, connection strings, orden de arranque). Un único tipo de test no cubre ambos niveles.

## Decisión

No es uno u otro: cada enfoque cubre un nivel distinto de la pirámide.

| | Tradicional (`WebApplicationFactory`) | `Aspire.Hosting.Testing` |
|---|---|---|
| Alcance | Un servicio, in-process | Toda la topología del AppHost, out-of-process |
| Dependencias | Reemplazables: DI, mocks, base en memoria o Testcontainers | Reales: los mismos contenedores que en local |
| Control interno | Alto (se puede intervenir el contenedor de DI) | Caja negra: solo por HTTP y estado de los recursos |
| Velocidad | Rápido | Más lento; requiere Docker |
| Qué valida | Lógica y contrato de una API | Cableado real y orden de arranque |

Cómo está implementado:

* `tests/MasterNet.Application.UnitTests`: handlers de cursos (Moq y EF InMemory), validadores y `ValidationBehavior`.
* `tests/MasterNet.IntegrationTests/Traditional`: endpoints de cursos con `WebApplicationFactory`. La base se reemplaza por
  EF InMemory, el servicio de ratings por un fake, y los tokens JWT se generan en el test con una clave aleatoria.
  Cubren casos felices y errores `400`, `401`, `403` y `404`.
* `tests/MasterNet.IntegrationTests/AspireTesting`: levanta el AppHost completo una vez por clase (`AppHostFixture`) y
  recorre el flujo listar → crear → editar → ver. El parámetro secreto `seed-admin-password` lo aporta el propio test.

## Consecuencias

* Muchos tests rápidos para la lógica y los contratos; pocos tests lentos para los flujos de punta a punta.
* Los tests de Aspire requieren Docker y el primer arranque puede tardar por la descarga de imágenes (timeout de 180 s).
* Ambos enfoques conviven en un solo proyecto, separados por carpeta y namespace.
