param name string
param location string = resourceGroup().location
param tags object = {}

param identityName string
param identityId string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'aca'
param openAiDeploymentName string
param openAiEndpoint string
param openAiApiVersion string
param openAiType string
param cosmosEndpoint string
param aiSearchEndpoint string

module app '../core/host/container-app-upsert.bicep' = {
  name: '${serviceName}-container-app-module'
  params: {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': serviceName })
    identityName: identityName
    identityType: 'UserAssigned'
    containerAppsEnvironmentName: containerAppsEnvironmentName
    containerRegistryName: containerRegistryName
    env: [
      {
        name: 'AZURE_OPENAI_DEPLOYMENT'
        value: openAiDeploymentName
      }
      {
        name: 'AZURE_OPENAI_ENDPOINT'
        value: openAiEndpoint
      }
      {
        name: 'AZURE_OPENAI_API_VERSION'
        value: openAiApiVersion
      }
      {
        name: 'AZURE_USERASSIGNED_ID'
        value: identityId
      }
      {
        name: 'AZURE_CLIENT_ID'
        value: identityId
      }
      {
        name: 'COSMOSDB__ENDPOINT'
        value: cosmosEndpoint
      }
      {
        name: 'AZUREAISEARCH__ENDPOINT'
        value: aiSearchEndpoint
      }
      {
        name: 'PROMPTY__TYPE'
        value: openAiType
      }
      {
        name: 'PROMPTY__API_VERSION'
        value: openAiApiVersion
      }
      {
        name: 'PROMPTY__AZURE_ENDPOINT'
        value: openAiEndpoint
      }
      {
        name: 'PROMPTY__AZURE_DEPLOYMENT'
        value: openAiDeploymentName
      }

    ]
    targetPort: 50505
  }
}

output SERVICE_ACA_NAME string = app.outputs.name
output SERVICE_ACA_URI string = app.outputs.uri
output SERVICE_ACA_IMAGE_NAME string = app.outputs.imageName
