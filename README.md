# Azure E-Voting System
The system is an E-Voting platform designed for the Romanian voting process.

---

## GitHub repository link

[https://github.com/larisajurj/AzureE-VotingSystem](https://github.com/larisajurj/AzureE-VotingSystem)

---
## Solutions Overview

### 1. **PollingStation App**
Handles local polling station logic and interfaces.

Deployed version can be accessed at: https://polling-station-portal.azurewebsites.net/

### 2. **VotingApp App**
Manages the voter-facing app and vote processing.

Deployed version can be accessed at: https://voting-portal-ro.azurewebsites.net/

### 3. **RegistrulElectoralApi**
API for verifying voters.

Deployed version can be accessed at: https://registrul-electoral-api.azurewebsites.net/swagger

---

## Prerequisites

- [.NET 9.0 SDK or newer](https://dotnet.microsoft.com/en-us/download)
- [Visual Studio 2022 (v17.0 or later)](https://visualstudio.microsoft.com/)
---

## How to Run the Applications

### Step 1: Clone the Repository
```bash
git clone https://github.com/larisajurj/AzureE-VotingSystem.git
cd AzureE-VotingSystem
```
---

### Step 2: Run the solutions locally
Each solution needs to be built and run separately
### 1: PollingStation Solution
Open PollingStation.sln in Visual Studio 2022.

You must configure the solution to run both `PollingStationAPI` and `PollingStationApp`, with `PollingStationAPI` launching first.

#### Steps to configure the multiple startup project:
1. Right-click the solution → Properties

2. Go to Common Properties → Configure Startup Projects

3. Choose Multiple startup projects

4. Set `PollingStationAPI` action to Start, and `PollingStationApp` to Start (in that order)

#### Environment Variables

To run this project, you will need to add the following environment variables to your appsettings file

1. PollingStation/PollingStationAPI/appsettings.json

   `AzureAd_TenantId`

   `AzureAd_ClientId`

   `AzureAd_Audience`

   `GeminiApiKey`

   `AzureBlob_AccountKey`
2. PollingStation/PollingStationApp/appsettings.json

   `AzureAd_TenantId`

   `AzureAd_ClientId`

   `AzureAd_ClientSecret`

   `AzureAd_Scopes`

#### Running the projects

After everything is configured, press `F5` to run the solution

Navigate to:
* Polling Station API: http://localhost:5062/swagger/
* Polling Station App: http://localhost:5072/

### 2: Voting Solution
Open Voting.sln in Visual Studio 2022.

You must configure the solution to run both `VotingFn` and `VotingApp`, with `VotingFn` launching first.

#### Steps to configure the multiple startup project:
1. Right-click the solution → Properties

2. Go to Common Properties → Configure Startup Projects

3. Choose Multiple startup projects

4. Set `VotingFn` action to Start, and `VotingApp` to Start (in that order)

#### Environment Variables

To run this project, you will need to add the following environment variables to your appsettings file

1. Voting/VotingApp/appsettings.json

   `AzureAd_TenantId`

   `AzureAd_ClientId`

   `AzureAd_ClientSecret`

   `AzureAd_Scopes`

2. Voting/VotingFn/appsettings.json

   `AzureBlob_AccountKey`
#### Running the projects

After everything is configured, press `F5` to run the solution

Navigate to:
* VotingApp: http://localhost:7207/
* VotingFunc: http://localhost:7154/

### 3: Electoral Register API
Open RegistrulElectoral_API.sln in Visual Studio 2022.

#### Configure startup project
1. Right-click the solution → Properties

2. Go to Common Properties → Configure Startup Projects

3. Choose `RegistrulElectoralAPI` as Single startup project

#### Running the projects
Press `F5` to run the solution

Navigate to:
* ElectoralRegisterApp: https://localhost:7165/swagger
---
## How to Deploy the Applications
The deployment pipeline can be found at https://dev.azure.com/larisajurj/eVotingAzure/_build?definitionId=1

For deploying only the source code of an app, use the following checks:
- [x] Deploy RegistrulElectoralAPI
- [x] Deploy PollingStation Web App
- [x] Deploy PollingStation API
- [x] Deploy Voting Web App
- [x] Deploy Voting Function

For deploying only the infrastructure of the system, use the following check:
- [x] Deploy Infrastructure
