# .NET 9 Razor Pages Showcase App

## 📖 Overview

This is a modern **.NET 9 Razor Pages Web Application** that demonstrates clean architecture, CI/CD automation with GitHub and PowerShell, and centralized image storage using **Azure Blob Storage**. The app is load-balanced across multiple IIS servers and supports webhook-driven deployments triggered by GitHub pushes.

---

## 🏗️ Key Components

| Component                          | Purpose                                                       |
|-----------------------------------|---------------------------------------------------------------|
| **.NET 9 Razor Pages App**        | Main application UI and backend logic                         |
| **IIS on Windows Server**         | Web host serving the published app                            |
| **Azure Blob Storage**            | Centralized image storage (public product images)             |
| **GitHub**                        | Source control and webhook trigger for deployments            |
| **GitWebhookListenerService**     | Minimal API service listening for GitHub webhook POSTs        |
| **deploy.ps1**                    | PowerShell deployment script to pull, publish, and deploy app |

---

## 🌐 Server Requirements

Install these on each **IIS Web Server**:

- [.NET SDK 9](https://dotnet.microsoft.com/en-us/download)
- IIS with:
  - Web Server role
  - ASP.NET Core Module
  - Web Management Tools
- PowerShell 5.1+
- Git for Windows
- GitWebhookListenerService (custom Minimal API run as Windows Service)
- Inbound access for GitHub Webhooks (port 5000 or reverse proxy via IIS)
- Outbound internet access for pulling from GitHub

---

## 🔐 Required Machine Environment Variables

| Name                          | Purpose                                       |
|-------------------------------|-----------------------------------------------|
| `AZURE_BLOB_CONNECTION_STRING`| Full connection string to Azure Blob Storage |

💡 You can set this permanently using:
```powershell
[System.Environment]::SetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING", "<your-connection-string>", "Machine")
```

---

## 🔐 GitHub Repository Secrets

To protect sensitive values from being committed:

| Secret Name                    | Description                                |
|--------------------------------|--------------------------------------------|
| `AZURE_BLOB_CONNECTION_STRING` | Ensure it's **never** committed to GitHub. |
| `.env` or `appsettings.Production.json` | (optional) store production-only configs |

Also ensure **GitHub Push Protection** is enabled to block secrets from being accidentally pushed.

---

## 🔒 IIS Site and File Permissions

- The IIS App Pool user (`IIS AppPool\DotNet9Showcase`) must have:
  - Read/Write access to `C:\inetpub\wwwroot\DotNet9Showcase`
- The GitWebhookListenerService should run with permissions to:
  - Execute PowerShell scripts
  - Pull from GitHub repo
  - Publish the app
  - Restart IIS App Pool (if required)

---

## 🚀 `deploy.ps1` - Deployment Script

Location:
```powershell
C:\Users\rackadmin\DotNet9Showcase\deploy.ps1
```

### What it does:
1. Stops the IIS App Pool (`DotNet9Showcase`)
2. Clones or pulls latest code from GitHub
3. Runs `dotnet publish` to compile and prepare assets
4. Copies published output to IIS site directory
5. Starts the IIS App Pool again

### Important Paths:
- **$deployPath**: `C:\deploy\DotNet9Showcase`
- **$sitePath**: `C:\inetpub\wwwroot\DotNet9Showcase`
- **App Pool Name**: `DotNet9Showcase`

---

## 🔁 GitHub Webhook Integration

Each IIS server runs an instance of `GitWebhookListenerService` on port `5000`. It listens for GitHub push events and triggers `deploy.ps1`.

### Flow:
1. Push to `main` branch on GitHub
2. Webhook sends POST to `http://<web-server>:5000/github-webhook`
3. Listener service executes `deploy.ps1`
4. App is pulled, built, and deployed automatically

---

## ☁️ Azure Blob Storage for Images

- All product images are uploaded directly to Azure Blob Storage (`productimages` container)
- Public URLs are stored in the database (e.g. https://<storageaccount>.blob.core.windows.net/productimages/image.jpg)
- Uses `AZURE_BLOB_CONNECTION_STRING` for secure read/write access

Image upload logic is built into:
- `Create.cshtml.cs`
- `Edit.cshtml.cs`
- `Delete.cshtml.cs`

Old images are **deleted** from Blob Storage when replaced or removed.

---

## 🔄 DevOps & Automation Summary

This architecture follows DevOps best practices:

| Practice                  | Implementation                                               |
|---------------------------|---------------------------------------------------------------|
| **CI/CD**                | GitHub → Webhook → Listener → PowerShell Deploy              |
| **Infrastructure as Code**| Deployment script manages web app lifecycle                 |
| **Centralized Storage**  | Azure Blob Storage handles all media assets                  |
| **Load Balanced Ready**  | Hostname shown in footer for server identification           |
| **Secrets Management**   | GitHub Push Protection + Env Variables                       |
| **Observable & Auditable**| IIS logs + optional deployment logs                          |

---

## 📌 Final Notes

- Ensure all web servers have the correct environment variable set.
- Make sure app pool identity has required permissions.
- For external webhook exposure, consider HTTPS with IIS reverse proxy and firewall rules.
- Azure Blob Storage access is public for image URLs but write access is secured via the connection string.

---

🛠 Built for speed, scalability, and simplicity.

