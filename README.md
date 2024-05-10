---
name: Contoso Chat Retail with .NET and Semantic Kernel
description: A front store with AI integration and chat features in the Cloud
languages:
- DotNet
- bicep
- azdeveloper
- Prompty
products:
- azure-openai
- azure-cognitive-search
- azure-app-service
- azure
page_type: sample
urlFragment: contoso-chat-csharp-prompty
---

# Contoso Chat Retail with .NET and Semantic Kernel

Samples in JavaScript, Python, and Java. Learn more at https://aka.ms/azai.

---

# Table of Contents

- [What is this sample?](#what-is-this-sample)
- [Features](#features)
- [Architecture Diagram](#architecture-diagram)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Quickstart](#quickstart)
- [Security Guidelines](#security-guidelines)
- [Resources](#resources)

[![Open in GitHub Codespaces](https://img.shields.io/static/v1?style=for-the-badge&label=GitHub+Codespaces&message=Open&color=brightgreen&logo=github)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=599293758&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=WestUs2)
[![Open in Dev Containers](https://img.shields.io/static/v1?style=for-the-badge&label=Dev%20Containers&message=Open&color=blue&logo=visualstudiocode)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/Azure-Samples/chat-rag-openai-csharp-prompty) 

# What is this sample?

In this sample, we present **Contoso Outdoors**, a conceptual store specializing in outdoor gear for hiking and camping enthusiasts. This virtual store enhances customer engagement and sales support through an intelligent chat agent. This agent is powered by the **Retrieval Augmented Generation (RAG)** pattern within the **Microsoft Azure AI Stack**, enriched with **Semantic Kernel** and **Prompty** support.

Artificial Intelligence integrates into the customer service experience, offering responses that are not only relevant but also personalized, drawing from the extensive product catalog and individual customer purchase histories.

For our web application, we are using **.NET Stack**, **Blazor**, and **AZD**, for an easy and quick deploy.

This sample uses the [Azure AI](https://azure.microsoft.com/solutions/ai/). It leverages **Azure OpenAI** to our chat features and **Semantic Kernel** to manage and insert the prompt into our code, and to evaluate prompt/LLM performance.

**Contoso Chat .NET** shows you how to:

1. Build a retail copilot application using the **RAG pattern**.
2. Ideate & iterate on application using **Semantic Kernel** and **Prompty**.
3. Build & manage the solution using the **Azure AI platform & tools**.
4. Provision & deploy the solution using the **Azure Developer CLI**.
5. Support **Responsible AI** practices with evaluation & content safety.

![Contoso Chat Application UI](./data/images/00-app-scenario-ai.png)

# Features

The project comes with:

- Sample **model configurations** and **evaluation prompts** for a RAG-based copilot application
- Sample **product and customer data** for retail application scenario
- Sample **application code** for copilot chat and evaluation functions
- Sample **azd-template configuration** for managing application on Azure

The sample is also a signature application for demonstrating new the capabilities of the Azure AI platform. Expect regular updates to showcase cutting-edge features and best practices for generative AI development. 

Planned updates include support for:

- New **Prompty assets** (to simplify prompt creation & iteration)
- New **azd ai.endpoint host type** (to configure AI deployments in Azure)

## Prompty


## Architecture Diagram
![Architecture Digram](data\images\architecture-diagram-contoso-dotnet.png)

# Getting Started



## Prerequisites

- **Azure Subscription** - [Signup for a free account.](https://azure.microsoft.com/free/)
- **Ability to provision Azure AI Search (Paid)** - Required for Semantic Ranker
- **Install [azd](https://aka.ms/install-azd)**
    - Windows: `winget install microsoft.azd`
    - Linux: `curl -fsSL https://aka.ms/install-azd.sh | bash`
    - MacOS: `brew tap azure/azd && brew install azd`
- **Azure OpenAi** -  Check for [up-to-date region availability](https://learn.microsoft.com/azure/ai-services/openai/concepts/models#standard-deployment-model-availability) and select a region during deployment accordingly
    - We recommend using Sweden Central or East US 2
- **Ability to provision Azure AI Search (Paid)** - Required for Semantic Ranker

## Quickstart


1. Fork this repo and open on [**Codespaces**](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=599293758&machine=standardLinux32gb&devcontainer_path=.devcontainer%2Fdevcontainer.json&location=WestUs)
    - Or Clone the repsitory or intialize the project: `azd init [name-of-repo]`
1. Login to Azure using: `az login --use-device-code`
1. Add credentials to AZD: `azd auth login`
1. Provision and deploy the project to Azure: `azd up`

## Costs
You can estimate the cost of this project's architecture with [Azure's pricing calculator](https://azure.microsoft.com/pricing/calculator/)

- Azure OpenAI - Standard tier, GPT-4, GPT-35-turbo and Ada models.  [See Pricing](https://azure.microsoft.com/pricing/details/cognitive-services/openai-service/)
- Azure AI Search - Basic tier, Semantic Ranker enabled [See Pricing](https://azure.microsoft.com/en-us/pricing/details/search/)
- Azure Cosmos DB for NoSQL - Serverless, Free Tier [See Pricing](https://azure.microsoft.com/en-us/pricing/details/cosmos-db/autoscale-provisioned/#pricing)

# Security Guidelines

Each template has either [Managed Identity](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview) or Key Vault built in to eliminate the need for developers to manage these credentials. Applications can use managed identities to obtain Microsoft Entra tokens without having to manage any credentials. 

Additionally, we have added a [GitHub Action tool](https://github.com/microsoft/security-devops-action) that scans the infrastructure-as-code files and generates a report containing any detected issues. 

To ensure best practices in your repo we recommend anyone creating solutions based on our templates ensure that the [Github secret scanning](https://docs.github.com/en/code-security/secret-scanning/about-secret-scanning) setting is enabled in your repos.

# Resources

- [Take a look on more .NET AI Samples.](https://github.com/dotnet/ai-samples/)
- [Learn more .NET AI with Microsoft Learn](https://learn.microsoft.com/pt-pt/dotnet/azure/)
- [Learn Azure, deploying in GitHub!](https://github.com/Azure-Samples)

## Troubleshooting

Have questions or issues to report? Please [open a new issue](https://github.com/Azure-Samples/contoso-chat-csharp-prompty/issues) after first verifying that the same question or issue has not already been reported. In the latter case, please add any additional comments you may have, to the existing issue.


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.