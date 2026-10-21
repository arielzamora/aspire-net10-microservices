targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention, the name of the resource group for your application will use this name, prefixed with rg-')
param environmentName string

@minLength(1)
@description('The location used for all deployed resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''

@metadata({azd: {
  type: 'generate'
  config: {length:22,noSpecial:true}
  }
})
@secure()
param cache_password string
@metadata({azd: {
  type: 'generate'
  config: {length:48,noSpecial:true}
  }
})
@secure()
param token_key string

var tags = {
  'azd-env-name': environmentName
}

resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: 'rg-${environmentName}'
  location: location
  tags: tags
}

module api_identity 'api-identity/api-identity.module.bicep' = {
  name: 'api-identity'
  scope: rg
  params: {
    location: location
  }
}
module api_roles_server 'api-roles-server/api-roles-server.module.bicep' = {
  name: 'api-roles-server'
  scope: rg
  params: {
    location: location
    principalId: api_identity.outputs.principalId
    principalName: api_identity.outputs.principalName
    server_outputs_name: server.outputs.name
    server_outputs_sqlserveradminname: server.outputs.sqlServerAdminName
  }
}
module env 'env/env.module.bicep' = {
  name: 'env'
  scope: rg
  params: {
    env_acr_outputs_name: env_acr.outputs.name
    location: location
    userPrincipalId: principalId
  }
}
module env_acr 'env-acr/env-acr.module.bicep' = {
  name: 'env-acr'
  scope: rg
  params: {
    location: location
  }
}
module server 'server/server.module.bicep' = {
  name: 'server'
  scope: rg
  params: {
    location: location
  }
}
output API_IDENTITY_CLIENTID string = api_identity.outputs.clientId
output API_IDENTITY_ID string = api_identity.outputs.id
output AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_DEFAULT_DOMAIN
output ENV_AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = env.outputs.AZURE_CONTAINER_APPS_ENVIRONMENT_ID
output ENV_AZURE_CONTAINER_REGISTRY_ENDPOINT string = env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
output ENV_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID
output SERVER_SQLSERVERFQDN string = server.outputs.sqlServerFqdn
