#!/bin/bash

echo  "#### Building contosochatapi:latest... ####"

az acr build --subscription ${AZURE_SUBSCRIPTION_ID} --registry ${AZURE_CONTAINER_REGISTRY_NAME} --image contosochatapi:latest ./src/ContosoChatAPI/ContosoChatAPI
image_name="${AZURE_CONTAINER_REGISTRY_NAME}.azurecr.io/contosochatapi:latest"