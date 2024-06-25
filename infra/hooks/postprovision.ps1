#!/usr/bin/env pwsh

Write-Output  "#### Building contosochatapi:latest...####"

az acr build --subscription $env:AZURE_SUBSCRIPTION_ID --registry $env:AZURE_CONTAINER_REGISTRY_NAME --image contosochatapi:latest ./src/ContosoChatAPI/ContosoChatAPI/