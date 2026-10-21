@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param env_outputs_azure_container_apps_environment_default_domain string

param env_outputs_azure_container_apps_environment_id string

param api_containerimage string

param api_identity_outputs_id string

param api_containerport string

param server_outputs_sqlserverfqdn string

@secure()
param token_key_value string

param api_identity_outputs_clientid string

param env_outputs_azure_container_registry_endpoint string

param env_outputs_azure_container_registry_managed_identity_id string

resource api 'Microsoft.App/containerApps@2025-10-02-preview' = {
  name: 'api'
  location: location
  properties: {
    configuration: {
      secrets: [
        {
          name: 'tokenkey'
          value: token_key_value
        }
      ]
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: int(api_containerport)
        transport: 'http'
      }
      registries: [
        {
          server: env_outputs_azure_container_registry_endpoint
          identity: env_outputs_azure_container_registry_managed_identity_id
        }
      ]
      runtime: {
        dotnet: {
          autoConfigureDataProtection: true
        }
      }
    }
    environmentId: env_outputs_azure_container_apps_environment_id
    template: {
      containers: [
        {
          probes: [
            {
              failureThreshold: 3
              httpGet: {
                path: '/alive'
                port: int(api_containerport)
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 5
              successThreshold: 1
              timeoutSeconds: 1
              type: 'Liveness'
            }
            {
              failureThreshold: 3
              httpGet: {
                path: '/health'
                port: int(api_containerport)
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 5
              successThreshold: 1
              timeoutSeconds: 1
              type: 'Readiness'
            }
          ]
          image: api_containerimage
          name: 'api'
          env: [
            {
              name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
              value: 'in_memory'
            }
            {
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'HTTP_PORTS'
              value: api_containerport
            }
            {
              name: 'ConnectionStrings__MasterNetDB'
              value: 'Server=tcp:${server_outputs_sqlserverfqdn},1433;Encrypt=True;Authentication="Active Directory Default";Database=MasterNetDB'
            }
            {
              name: 'MASTERNETDB_HOST'
              value: server_outputs_sqlserverfqdn
            }
            {
              name: 'MASTERNETDB_PORT'
              value: '1433'
            }
            {
              name: 'MASTERNETDB_URI'
              value: 'mssql://${server_outputs_sqlserverfqdn}:1433/MasterNetDB'
            }
            {
              name: 'MASTERNETDB_JDBCCONNECTIONSTRING'
              value: 'jdbc:sqlserver://${server_outputs_sqlserverfqdn}:1433;database=MasterNetDB;encrypt=true;trustServerCertificate=false'
            }
            {
              name: 'MASTERNETDB_DATABASENAME'
              value: 'MasterNetDB'
            }
            {
              name: 'RATINGSERVICE_HTTP'
              value: 'https://ratingservice.internal.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ratingservice__http__0'
              value: 'https://ratingservice.internal.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'RATINGSERVICE_HTTPS'
              value: 'https://ratingservice.internal.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'services__ratingservice__https__0'
              value: 'https://ratingservice.internal.${env_outputs_azure_container_apps_environment_default_domain}'
            }
            {
              name: 'TokenKey'
              secretRef: 'tokenkey'
            }
            {
              name: 'HealthChecks__Expose'
              value: 'true'
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: api_identity_outputs_clientid
            }
            {
              name: 'AZURE_TOKEN_CREDENTIALS'
              value: 'ManagedIdentityCredential'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
      }
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${api_identity_outputs_id}': { }
      '${env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}