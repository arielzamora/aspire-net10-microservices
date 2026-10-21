# MasterNet — catálogo de cursos con .NET Aspire

Solución .NET 10 orquestada con .NET Aspire: Web API (Clean Architecture + CQRS), cliente Blazor WebAssembly,
SQL Server, Redis y un servicio de calificaciones. Desde el cliente se pueden listar, crear, editar y borrar cursos.

> Este README es la versión mínima de "cómo correrlo". Arquitectura, ADRs y despliegue a Azure están en preparación.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop **en ejecución** (Aspire levanta SQL Server y Redis como contenedores)
- Opcional: la CLI de Aspire (comando `aspire`) para usar `aspire run`. Sin la CLI alcanza con `dotnet run` o con F5 en Visual Studio.

## Cómo correrlo

```bash
aspire run
```

Alternativas equivalentes: `dotnet run --project src/MasterNet.AppHost`, o abrir `MasterNet.sln` en Visual Studio y ejecutar el proyecto `MasterNet.AppHost`.

### El único paso manual (solo la primera vez)

La contraseña del usuario administrador de prueba **no está en el repositorio**: la elegís vos.

1. Al arrancar, el dashboard de Aspire (la URL aparece en la consola) muestra el aviso **"Parámetros sin resolver"**.
2. Pulsá **"Especifique los valores"**, escribí una contraseña para `seed-admin-password`
   (mínimo 6 caracteres, con mayúscula, minúscula y número) y marcá **"Guardar en secretos de usuario"**.
3. Pulsá **Guardar**. La API arranca y no te lo vuelve a pedir.

Equivalente por consola, antes de arrancar:

```bash
dotnet user-secrets set "Parameters:seed-admin-password" "<tu-contraseña>" --project src/MasterNet.AppHost
```

Todo lo demás **se genera solo** y queda en los user-secrets del AppHost: la contraseña de SQL Server (`server-password`),
la de Redis (`cache-password`) y la clave de firma de los JWT (`token-key`). No hace falta tocar ningún archivo de configuración.

## Usar la aplicación

| Qué | Dónde |
|---|---|
| Dashboard de Aspire (recursos, logs, trazas) | URL de la consola al arrancar |
| Cliente Blazor | recurso `client` del dashboard (normalmente `http://localhost:5289`) |
| API | `http://localhost:5001/api/courses` |

Usuarios de prueba (se crean al arrancar la API en Development, con la contraseña que elegiste):

| Usuario | Rol | Puede |
|---|---|---|
| `azamora@gmail.com` | ADMIN | listar, crear, editar y borrar cursos |
| `john.doe@gmail.com` | CLIENT | solo lectura |

En el cliente entrá por **Sign in**; con el admin aparecen los botones **New course**, **Edit** y **Delete**.

> La base de datos se **recrea en cada arranque** del AppHost (`Database/CreateDatabaseAndSeed.sql`), así que los cursos que crees no persisten entre ejecuciones.

## Tests

```bash
dotnet test tests/MasterNet.Application.UnitTests
dotnet test tests/MasterNet.IntegrationTests
```

- `MasterNet.IntegrationTests` incluye dos enfoques: tests de la API con `WebApplicationFactory` (rápidos, en memoria, sin Docker)
  y un test de punta a punta con `Aspire.Hosting.Testing` (levanta toda la topología, **requiere Docker**; la primera vez puede tardar por la descarga de imágenes).
- Solo los de la API: `dotnet test tests/MasterNet.IntegrationTests --filter "FullyQualifiedName~CoursesEndpointsTests"`.
- No necesitan configuración previa: generan sus propias claves.

## Configuración opcional

- **Fotos de cursos (Cloudinary):** sin credenciales el alta funciona, pero no sube fotos.
  `dotnet user-secrets set "CloudinarySettings:CloudName" "..." --project src/MasterNet.WebApi` (idem `ApiKey` y `ApiSecret`).
- **SQL Server con `docker-compose`** (sin Aspire): copiá `.env.example` a `.env`, poné un valor en `SA_PASSWORD` y ejecutá `docker compose up`.
- **Ejecutar `MasterNet.WebApi` solo, sin el AppHost:** necesita `TokenKey` y `SeedAdmin:Password` en sus user-secrets
  (`dotnet user-secrets set "TokenKey" "<32+ caracteres>" --project src/MasterNet.WebApi`) y una cadena de conexión `MasterNetDB`.

## Secretos

No hay secretos versionados. En desarrollo se guardan en user-secrets (los genera o los pide Aspire); en Azure serán parámetros del despliegue.
