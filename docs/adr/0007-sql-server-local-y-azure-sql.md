# 0007 — SQL Server en contenedor en local y Azure SQL en la nube

* Estado: propuesto (el despliegue se hace en la Parte 2.3)

## Contexto y problema

En desarrollo conviene una base reproducible, sin cuentas ni costos. En la nube conviene un servicio administrado con costo
bajo para una demo.

## Opciones consideradas

* SQL Server en contenedor en ambos entornos.
* SQL Server en contenedor en local y **Azure SQL** en la nube (`AddAzureSqlServer().RunAsContainer()`).
* SQLite en local (el repositorio aún referencia el paquete de SQLite, pero el código usa SQL Server).

## Decisión

* **Local (hoy):** `AddSqlServer("server", port: 1433)` con lifetime persistente. Aspire genera la contraseña y la guarda en
  user-secrets (ver [0008](0008-gestion-de-secretos.md)).
* **Nube (propuesto):** `AddAzureSqlServer().RunAsContainer()`: el mismo modelo de AppHost corre como contenedor en local y se
  aprovisiona como **Azure SQL serverless con auto-pausa** al desplegar. Un único código de AppHost para ambos casos.

## Consecuencias

* Mismo proveedor (`Microsoft.EntityFrameworkCore.SqlServer`) en todos los entornos: no hay diferencias de dialecto.
* Azure SQL serverless con auto-pausa reduce el costo; el primer acceso tras una pausa es más lento.
* La estrategia de esquema en producción está en [0005](0005-estrategia-de-migraciones.md).
