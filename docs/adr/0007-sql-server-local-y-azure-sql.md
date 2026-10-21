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

* Se usa `AddAzureSqlServer("server").RunAsContainer(...)` en el AppHost: el mismo modelo corre como **contenedor** en local
  (puerto 1433, lifetime persistente; Aspire genera la contraseña y la guarda en user-secrets, ver
  [0008](0008-gestion-de-secretos.md)) y se aprovisiona como **Azure SQL serverless con auto-pausa** al publicar.
  Un único código de AppHost para ambos casos.
* El modo local está en uso y probado (los tests de Aspire levantan ese contenedor). La parte de Azure está generada pero sin desplegar.

## Implementación (generada, sin desplegar)

`infra/server/server.module.bicep` (generado con `azd infra gen`) define la base `MasterNetDB` con `useFreeLimit: true`,
`freeLimitExhaustionBehavior: 'AutoPause'` y SKU `GP_S_Gen5_2` (serverless). El servidor solo admite autenticación Entra ID y la
API se conecta con identidad administrada (`Authentication="Active Directory Default"`), sin contraseña. Todo esto está validado
con `az bicep build`, pero **no se desplegó**: el estado de la decisión sigue siendo "propuesto".

## Consecuencias

* Mismo proveedor (`Microsoft.EntityFrameworkCore.SqlServer`) en todos los entornos: no hay diferencias de dialecto.
* Azure SQL serverless con auto-pausa reduce el costo; el primer acceso tras una pausa es más lento.
* La estrategia de esquema en producción está en [0005](0005-estrategia-de-migraciones.md).
