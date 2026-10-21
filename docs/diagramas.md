# Diagramas

Diagramas en Mermaid (GitHub los renderiza). Siguen la notación de los niveles C4 (contexto y contenedores) con diagramas de flujo.

## C4 — Contexto

```mermaid
flowchart LR
    user(["Persona usuaria<br/>consulta el catálogo"])
    admin(["Administrador<br/>crea, edita y borra cursos"])

    subgraph sys["Sistema MasterNet (.NET Aspire)"]
        app["Catálogo de cursos"]
    end

    ide["Dashboard de Aspire<br/>logs · métricas · trazas"]
    cloudinary["Cloudinary<br/>fotos de cursos (opcional)"]

    user -->|navega| app
    admin -->|inicia sesión y gestiona| app
    app -->|OTLP| ide
    app -.->|sube fotos| cloudinary
```

## C4 — Contenedores

```mermaid
flowchart TB
    browser(["Navegador"])

    subgraph aspire["AppHost de Aspire"]
        client["client<br/>Blazor WebAssembly + Refit"]
        api["api<br/>ASP.NET Core Web API<br/>Clean Architecture + CQRS<br/>JWT + políticas"]
        rating["ratingservice<br/>Minimal API"]
        migration["migrationSql<br/>worker: ejecuta el script SQL"]
        sql[("server / MasterNetDB<br/>SQL Server")]
        redis[("cache<br/>Redis")]
    end

    dashboard["Dashboard de Aspire"]

    browser -->|HTTP| client
    client -->|"HTTP + Bearer (Refit)"| api
    api -->|EF Core| sql
    api -->|"HTTP · service discovery<br/>http://ratingservice"| rating
    rating -->|StackExchange.Redis| redis
    migration -->|"CreateDatabaseAndSeed.sql"| sql
    api -. "espera migrationSql<br/>(WaitForCompletion)" .-> migration

    api -.->|OTLP| dashboard
    rating -.->|OTLP| dashboard
```

## Local vs. Azure

El lado Azure está **preparado** (`infra/`, `azure.yaml` y `deploy.yml`) pero **todavía no se desplegó**.

```mermaid
flowchart LR
    subgraph local["Local (hoy)"]
        direction TB
        l_apphost["AppHost de Aspire<br/>aspire run"]
        l_apps["api · client · ratingservice<br/>procesos .NET"]
        l_sql[("SQL Server<br/>contenedor Docker")]
        l_redis[("Redis<br/>contenedor Docker")]
        l_dash["Dashboard de Aspire"]
        l_secrets["user-secrets<br/>(generados o pedidos)"]

        l_apphost --> l_apps
        l_apps --> l_sql
        l_apps --> l_redis
        l_apps -.->|OTLP| l_dash
        l_secrets -.-> l_apphost
    end

    subgraph azure["Azure (preparado, sin desplegar)"]
        direction TB
        gh["GitHub Actions<br/>OIDC federado + azd"]
        aca["Azure Container Apps<br/>api · ratingservice · client (nginx)<br/>escalan a cero · probes /alive y /health"]
        a_sql[("Azure SQL<br/>serverless, auto-pausa<br/>solo Entra ID")]
        a_redis[("Redis<br/>contenedor en ACA (demo)")]
        a_mon["Azure Monitor /<br/>Application Insights"]
        a_params["Parámetros de azd<br/>e identidad administrada"]

        gh -->|"build · tests · migraciones · deploy"| aca
        aca --> a_sql
        aca --> a_redis
        aca -.->|OTLP| a_mon
        a_params -.-> aca
    end
```
