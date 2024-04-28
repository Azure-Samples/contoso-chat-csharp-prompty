targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name which is used to generate a short unique hash for each resource')
param name string

@minLength(1)
@description('Primary location for all resources')
param location string

param openAiResourceName string = ''
param openAiResourceGroupName string = ''
param openAiResourceGroupLocation string = ''
param openAiSkuName string = ''
param openAiApiVersion string = ''
param openAiType string = 'azure'
param searchServiceName string = ''
param cosmosAccountName string = ''
param openAiEmbeddingDeploymentName string = ''
param aiSearchIndexName string = ''
param cosmosDatabaseName string = ''
param cosmosContainerName string = ''

var resourceToken = toLower(uniqueString(subscription().id, name, location))
var tags = { 'azd-env-name': name }

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${name}-rg'
  location: location
  tags: tags
}

resource openAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(openAiResourceGroupName)) {
  name: !empty(openAiResourceGroupName) ? openAiResourceGroupName : resourceGroup.name
}

var prefix = '${name}-${resourceToken}'

module managedIdentity 'core/security/managed-identity.bicep' = {
  name: 'managed-identity'
  scope: resourceGroup
  params: {
    name: 'id-${resourceToken}'
    location: location
    tags: tags
  }
}

var openAiDeploymentName = 'chatgpt'
var openAiEmbeddingsDeploymentName = 'text-embedding-ada-002'
module openAi 'core/ai/cognitiveservices.bicep' = {
  name: 'openai'
  scope: openAiResourceGroup
  params: {
    name: !empty(openAiResourceName) ? openAiResourceName : '${resourceToken}-cog'
    location: !empty(openAiResourceGroupLocation) ? openAiResourceGroupLocation : location
    tags: tags
    sku: {
      name: !empty(openAiSkuName) ? openAiSkuName : 'S0'
    }
    deployments: [
      {
        name: openAiDeploymentName
        model: {
          format: 'OpenAI'
          name: 'gpt-35-turbo'
          version: '0613'
        }
        sku: {
          name: 'Standard'
          capacity: 30
        }
      }
      {
        name: openAiEmbeddingsDeploymentName
        model: {
          format: 'OpenAI'
          name: 'text-embedding-ada-002'
          version: '2'
        }
        sku: {
          name: 'Standard'
          capacity: 20
        }
      }
    ]
  }
}

module search 'core/search/search-services.bicep' = {
  name: 'search'
  scope: resourceGroup
  params: {
    name: !empty(searchServiceName) ? searchServiceName : '${prefix}-search-contoso'
    location: location
    semanticSearch: 'free'
    authOptions: {
      aadOrApiKey: {
        aadAuthFailureMode: 'http401WithBearerChallenge'
      }
    }
  }
}

module cosmos 'core/database/cosmos/sql/cosmos-sql-db.bicep' = {
  name: 'cosmos'
  scope: resourceGroup
  params: {
    accountName: !empty(cosmosAccountName) ? cosmosAccountName : 'cosmos-contoso-${resourceToken}'
    databaseName: 'contoso-outdoor'
    location: location
    tags: union(tags, {
        defaultExperience: 'Core (SQL)'
        'hidden-cosmos-mmspecial': ''
      })
    containers: [
      {
        name: 'customers'
        id: 'customers'
        partitionKey: '/id'
      }
    ]
  }
}

module logAnalyticsWorkspace 'core/monitor/loganalytics.bicep' = {
  name: 'loganalytics'
  scope: resourceGroup
  params: {
    name: '${prefix}-loganalytics'
    location: location
    tags: tags
  }
}

// Container apps host (including container registry)
module containerApps 'core/host/container-apps.bicep' = {
  name: 'container-apps'
  scope: resourceGroup
  params: {
    name: 'app'
    location: location
    tags: tags
    containerAppsEnvironmentName: '${prefix}-containerapps-env'
    containerRegistryName: '${replace(prefix, '-', '')}registry'
    logAnalyticsWorkspaceName: logAnalyticsWorkspace.outputs.name
  }
}

module aca 'app/aca.bicep' = {
  name: 'aca'
  scope: resourceGroup
  params: {
    name: replace('${take(prefix, 19)}-ca', '--', '-')
    location: location
    tags: tags
    identityName: managedIdentity.outputs.managedIdentityName
    identityId: managedIdentity.outputs.managedIdentityClientId
    containerAppsEnvironmentName: containerApps.outputs.environmentName
    containerRegistryName: containerApps.outputs.registryName
    openAiDeploymentName: !empty(openAiDeploymentName) ? openAiDeploymentName : 'gpt-35-turbo'
    openAiEmbeddingDeploymentName: !empty(openAiEmbeddingDeploymentName) ? openAiEmbeddingDeploymentName : 'text-embedding-ada-002'
    openAiEndpoint: openAi.outputs.endpoint
    openAiType: openAiType
    openAiApiVersion: !empty(openAiApiVersion) ? openAiApiVersion : '2023-07-01-preview'
    aiSearchEndpoint: search.outputs.endpoint
    aiSearchIndexName: !empty(aiSearchIndexName) ? aiSearchIndexName : 'contoso-products'
    cosmosEndpoint: cosmos.outputs.endpoint
    cosmosDatabaseName: !empty(cosmosDatabaseName) ? cosmosDatabaseName : 'contoso-outdoor'
    cosmosContainerName: !empty(cosmosContainerName) ? cosmosContainerName : 'customers'
  }
}

module aiSearchRole 'core/security/role.bicep' = {
  scope: openAiResourceGroup
  name: 'ai-search-role'
  params: {
    principalId: managedIdentity.outputs.managedIdentityPrincipalId
    roleDefinitionId: '8ebe5a00-799e-43f5-93ac-243d3dce84a7' //Search Index Data Contributor
    principalType: 'ServicePrincipal'
  }
}

module cosmosRole 'core/security/role.bicep' = {
  scope: openAiResourceGroup
  name: 'cosmos-role'
  params: {
    principalId: managedIdentity.outputs.managedIdentityPrincipalId
    roleDefinitionId: 'fbdf93bf-df7d-467e-a4d2-9458aa1360c8' //Cosmos DB Account
    principalType: 'ServicePrincipal'
  }
}

module cosmosRoleContributor 'core/security/role.bicep' = {
  scope: openAiResourceGroup
  name: 'cosmos-role-Contributor'
  params: {
    principalId: managedIdentity.outputs.managedIdentityPrincipalId
    roleDefinitionId: '7ca78c08-252a-4471-8644-bb5ff32d4ba0' //Search Service Contributor
    principalType: 'ServicePrincipal'
  }
}

module cosmosAccountRole 'core/security/role-cosmos.bicep' = {
  scope: openAiResourceGroup
  name: 'cosmos-account-role'
  params: {
    principalId: managedIdentity.outputs.managedIdentityPrincipalId
    databaseAccountId: cosmos.outputs.accountId
    databaseAccountName: cosmos.outputs.accountName
  }
}

output AZURE_LOCATION string = location
output RESOURCE_GROUP_NAME string = resourceGroup.name

output AZURE_OPENAI_CHATGPT_DEPLOYMENT string = openAiDeploymentName
output AZURE_OPENAI_API_VERSION string = openAiApiVersion
output AZURE_OPENAI_ENDPOINT string = openAi.outputs.endpoint
output AZURE_OPENAI_RESOURCE string = openAi.outputs.name
output AZURE_OPENAI_RESOURCE_GROUP string = openAiResourceGroup.name
output AZURE_OPENAI_SKU_NAME string = openAi.outputs.skuName
output AZURE_OPENAI_RESOURCE_GROUP_LOCATION string = openAiResourceGroup.location

output SERVICE_ACA_NAME string = aca.outputs.SERVICE_ACA_NAME
output SERVICE_ACA_URI string = aca.outputs.SERVICE_ACA_URI
output SERVICE_ACA_IMAGE_NAME string = aca.outputs.SERVICE_ACA_IMAGE_NAME

output AZURE_CONTAINER_ENVIRONMENT_NAME string = containerApps.outputs.environmentName
output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerApps.outputs.registryLoginServer
output AZURE_CONTAINER_REGISTRY_NAME string = containerApps.outputs.registryName