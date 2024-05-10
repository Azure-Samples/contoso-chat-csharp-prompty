#!/usr/bin/env pwsh

Write-Output  "Building contosochatapi:latest..."
az acr build --subscription $env:AZURE_SUBSCRIPTION_ID --registry $env:AZURE_CONTAINER_REGISTRY_NAME --image contosochatapi:latest ./src/ContosoChatAPI/ContosoChatAPI/
$image_name = $env:AZURE_CONTAINER_REGISTRY_NAME + '.azurecr.io/contosochatapi:latest'
az containerapp update --subscription $env:AZURE_SUBSCRIPTION_ID --name $env:SERVICE_ACA_NAME --resource-group $env:RESOURCE_GROUP_NAME --image $image_name
az containerapp ingress update --subscription $env:AZURE_SUBSCRIPTION_ID --name $env:SERVICE_ACA_NAME --resource-group $env:RESOURCE_GROUP_NAME --target-port 8080