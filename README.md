---
name: Contoso Chat Retail with .NET and Semantic Kernel
description: A front store with AI integration and chat features in the Cloud
languages:
- DotNet
- bicep
- azdeveloper
products:
- azure-openai
- azure-cognitive-search
- azure-app-service
- azure
page_type: sample
urlFragment: TBD
---

# Contoso Chat Retail with .NET and Semantic Kernel

Samples in JavaScript Python, and Java. Learn more at https://aka.ms/azai.

---

# Table of Contents

- [What is this sample?](#what-is-this-sample?)
- [Features](#features)
- [Architecture Diagram](#architecture-diagram)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installations](#installations)
  - [Quickstart](#quickstart)
  - [Local Development](#local-development)
- [Costs](#costs) 
- [Security Guidelines](#security-guidelines)
- [Resources](#resources)

[![Open in GitHub Codespaces](https://img.shields.io/static/v1?style=for-the-badge&label=GitHub+Codespaces&message=Open&color=brightgreen&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=599293758&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=WestUs2)
[![Open in Dev Containers](https://img.shields.io/static/v1?style=for-the-badge&label=Dev%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/Azure-Samples/chat-rag-openai-csharp-prompty) 

# What is this sample?

In this sample, we have a Store Front, with a chat application to Support Sales and Costumer Experience, with the Retrieval Augmented Generation pattern in the Microsoft Azure AI Stack, plus Semantic Kernel support.
For our web application, using .NET Stack using Blazor and AZD for a easy and quick deploy.

This sample uses the **[Azure AI](https://azure.microsoft.com/solutions/ai/)**. It leverages **Azure OpenAI** to our chat features and **Semantic Kernel** to manage and insert the prompt into our code, and to evaluate prompt/LLM performance.

By the end of deploying this template you should be able to:

1. Describe what Azure AI Studio and Semantic Kernel provide. 
1. Explain the RAG Architecture for building LLM Apps.
1. Build, run, evaluate, and deploy, a RAG-based LLM App to Azure.

# Features

* A UI Feature (tbd)
* Audio interface to easily transmit the information while in the Field
* Summarization from a Ticket from issues from the Field and Shop Floor workers

**For Developers**
* A Prompty file where the prompt is constructed
* Deployment available via AZD and easily moddable 


# Architecture Diagram
Include a diagram describing the application (DevDiv is working with Designers on this part)

# Getting Started

## Prerequisites

- Install [azd](https://aka.ms/install-azd)
    - Windows: `winget install microsoft.azd`
    - Linux: `curl -fsSL https://aka.ms/install-azd.sh | bash`
    - MacOS: `brew tap azure/azd && brew install azd`
- Run on Codespaces or Visual Studio
- This model uses Azure OpenAi which may not be available in all Azure regions. Check for [up-to-date region availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models#standard-deployment-model-availability) and select a region during deployment accordingly
    - We recommend using Sweden Central or East US 2

## Quickstart

1. Clone the repository or intialize the project: `azd init [name-of-repo]`
1. Login to Azure using: `az login --use-device-code`
1. Add credentials to AZD: `azd auth login`
1. Provision and deploy the project to Azure: `azd up`

# Security Guidelines

Each template has either [Managed Identity](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview) or Key Vault built in to eliminate the need for developers to manage these credentials. Applications can use managed identities to obtain Microsoft Entra tokens without having to manage any credentials. 

Additionally, we have added a [GitHub Action tool](https://github.com/microsoft/security-devops-action) that scans the infrastructure-as-code files and generates a report containing any detected issues. 

To ensure best practices in your repo we recommend anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/en/code-security/secret-scanning/about-secret-scanning) setting is enabled in your repos.

# Resources

- [Take a look on more .NET AI Samples.](https://github.com/dotnet/ai-samples/)
- [Learn more .NET AI with Microsoft Learn](https://learn.microsoft.com/pt-pt/dotnet/azure/)
- [Learn Azure, deploying in GitHub!](https://github.com/Azure-Samples)