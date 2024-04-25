param name string
param location string = resourceGroup().location
param tags object = {}

param identityName string
param identityId string
param containerAppsEnvironmentName string
param containerRegistryName string
param serviceName string = 'aca'
param exists bool
param openAiDeploymentName string
param openAiEndpoint string
param openAiApiVersion string
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
    exists: exists
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
        name: 'COSMOS_ENDPOINT'
        value: cosmosEndpoint
      }
      {
        name: 'AISEARCH_ENDPOINT'
        value: aiSearchEndpoint
      }
    ]
    targetPort: 50505
  }
}

output SERVICE_ACA_NAME string = app.outputs.name
output SERVICE_ACA_URI string = app.outputs.uri
output SERVICE_ACA_IMAGE_NAME string = app.outputs.imageName