# 0008 — Gestión de secretos

* Estado: aceptado para desarrollo; Azure propuesto (Parte 2.3)

## Contexto y problema

La solución necesita una contraseña de SQL Server, una clave de firma de JWT, la contraseña de Redis y la contraseña de los
usuarios de prueba. Ninguna debe estar en el repositorio, y alguien que clone el proyecto debería poder correrlo con el mínimo
de pasos.

## Decisión

Los secretos son **parámetros de Aspire** que se resuelven según su tipo (`MasterNet.AppHost/AppHost.cs`):

| Secreto | Cómo se resuelve | Dónde queda |
|---|---|---|
| `server-password` (SQL Server) | Lo genera `AddSqlServer` | user-secrets del AppHost |
| `cache-password` (Redis) | Lo genera `AddRedis` | user-secrets del AppHost |
| `token-key` (firma JWT) | `CreateGeneratedParameter` (48 caracteres) | user-secrets del AppHost |
| `seed-admin-password` | Sin valor por defecto: el dashboard lo pide en el primer arranque | user-secrets del AppHost |

La API recibe `token-key` y `seed-admin-password` como variables de entorno (`TokenKey`, `SeedAdmin__Password`).
La contraseña del admin no se genera al azar porque la persona que usa la app necesita conocerla para iniciar sesión.

En modo publicación (Azure) `token-key` y la contraseña de Redis (`cache_password`) pasan a ser parámetros del despliegue
(`AZURE_TOKEN_KEY` y `AZURE_CACHE_PASSWORD` en `infra/main.parameters.json`) y la API usa identidad administrada para Azure SQL.
`seed-admin-password` no se publica: el seed de usuarios solo corre en Development.

Los tests no dependen de secretos: generan claves aleatorias por ejecución.
`docker-compose` lee `SA_PASSWORD` de un `.env` local que no se versiona (`.env.example` documenta la variable).

## Consecuencias

* En local no hay que configurar nada salvo elegir la contraseña del admin la primera vez (documentado en el README).
* En Azure los parámetros pasan a ser parámetros del despliegue con `azd`, y las credenciales de servicios usarán
  Managed Identity cuando aplique.
* El `TokenKey` anterior sigue en el historial de git local (primer commit); debe considerarse comprometido y rotado.
