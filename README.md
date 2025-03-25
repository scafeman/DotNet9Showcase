# .NET 9 Razor Pages Showcase App

## 📖 Overview

This is a modern **.NET 9 Razor Pages Web Application** showcasing clean architecture, CI/CD automation, Azure integration, and DevOps practices. It includes:

- Azure Blob Storage for centralized image hosting  
- SQL Server for relational data  
- GitHub webhooks triggering automatic deployment via PowerShell  
- Multi-server support with IIS and load balancing

---

## 🏗️ Key Components

| Component                          | Purpose                                                       |
|-----------------------------------|---------------------------------------------------------------|
| **.NET 9 Razor Pages App**        | Main application UI and backend logic                         |
| **SQL Server (Standalone VM)**    | Production-grade relational database                          |
| **Azure Blob Storage**            | Centralized image storage (public product images)             |
| **IIS on Windows Server**         | Web host serving the published app                            |
| **GitHub**                        | Source control and webhook trigger for deployments            |
| **GitWebhookListenerService**     | Minimal API service running as a Windows Service              |
| **deploy.ps1**                    | PowerShell script to pull, build, and publish the app         |
| **EF Core Migrations**            | Code-first DB schema management and automation                |

---

## 🌐 Server Requirements

Each IIS/Web Server must have:

- [.NET SDK 9](https://dotnet.microsoft.com/en-us/download)
- Git for Windows
- PowerShell 5.1+
- **IIS** with:
  - Web Server role
  - ASP.NET Core Module
  - Web Management Tools
- GitWebhookListenerService (Windows Service)
- Outbound access to GitHub and SQL Server (port 1433)
- Inbound webhook access (port 5000 or reverse-proxied)

---

## 🔐 Required Machine Environment Variables

These must be added **permanently** using PowerShell:

```powershell
[System.Environment]::SetEnvironmentVariable("AZURE_BLOB_CONNECTION_STRING", "<your-blob-conn-string>", "Machine")
[System.Environment]::SetEnvironmentVariable("SQL_SA_PASSWORD", "<your-sa-password>", "Machine")
[System.Environment]::SetEnvironmentVariable("DOTNET9_SQL_CONN", "Server=172.16.21.70;Database=DotNet9Showcase;User Id=sa;Password={SQL_PASSWORD_PLACEHOLDER};", "Machine")
[System.Environment]::SetEnvironmentVariable("UseSqlServer", "true", "Machine")
```

Note: `DOTNET9_SQL_CONN` is templated to insert the real password at runtime using `SQL_SA_PASSWORD`.

---

## 🔐 GitHub Repository Secrets & Protections

| Secret / Protection               | Description                                                  |
|----------------------------------|--------------------------------------------------------------|
| `AZURE_BLOB_CONNECTION_STRING`   | Must be excluded from commits, stored as env var             |
| GitHub Push Protection           | Prevents accidental secret pushes                            |
| `.env` or `appsettings.Production.json` (optional) | Can store non-sensitive overrides             |

---

## 🔒 IIS Site & Deployment Permissions

- App Pool Identity (`IIS AppPool\DotNet9Showcase`) needs:
  - Read/Write to `C:\inetpub\wwwroot\DotNet9Showcase`
- `GitWebhookListenerService` must be able to:
  - Pull GitHub repo
  - Execute PowerShell scripts
  - Stop/start IIS app pool

---

## 🚀 `deploy.ps1` – Deployment Script

**Location:**

```powershell
C:\Users\rackadmin\DotNet9Showcase\deploy.ps1
```

### What It Does

1. Stops IIS App Pool
2. Clones or pulls latest GitHub repo
3. Publishes app to `_publish` directory
4. Copies published files to `C:\inetpub\wwwroot\DotNet9Showcase`
5. Starts IIS App Pool again

### Important Paths

| Path/Value                      | Description                             |
|--------------------------------|-----------------------------------------|
| `$deployPath`                  | `C:\deploy\DotNet9Showcase`             |
| `$sitePath`                    | `C:\inetpub\wwwroot\DotNet9Showcase`    |
| `$appPool`                     | `DotNet9Showcase`                       |

---

## 🔁 GitHub Webhook Deployment Flow

1. Developer pushes to `main` on GitHub
2. Webhook triggers a POST to `http://<server>:5000/github-webhook`
3. Listener service calls `deploy.ps1`
4. App is pulled, published, and deployed
5. IIS serves the updated app immediately

---

## 🧱 SQL Server Integration

- Database hosted at: `172.16.21.70`
- Authenticated using `sa` and `SQL_SA_PASSWORD` environment variable
- Uses EF Core 9 and Migrations for schema control
- Automatically creates DB if not present

### Migration & Setup Commands

```bash
dotnet ef migrations add InitSqlServerMigration --context AppDbContext
dotnet ef database update --context AppDbContext
```

### Decimal Precision Fix Example

```csharp
modelBuilder.Entity<Product>()
    .Property(p => p.Price)
    .HasPrecision(18, 2);
```

---

## ☁️ Azure Blob Storage

- Product images are uploaded to `productimages` container
- Public URLs look like:
  ```
  https://<youraccount>.blob.core.windows.net/productimages/image.jpg
  ```
- Upload and delete logic is in:
  - `Create.cshtml.cs`
  - `Edit.cshtml.cs`
  - `Delete.cshtml.cs`

Old blobs are deleted when products are edited or removed.

---

## 🔄 DevOps & Automation Summary

| Practice                  | Implementation                                               |
|---------------------------|---------------------------------------------------------------|
| CI/CD                     | GitHub → Webhook → Listener → PowerShell                     |
| Infrastructure as Code    | PowerShell for publishing, IIS App Pool control              |
| Centralized Image Storage | Azure Blob Storage                                           |
| Schema Management         | EF Core + Migrations                                         |
| Multi-server Ready        | Webhook listener & environment vars per node                 |
| Secrets Management        | GitHub Push Protection + local env vars                      |

---

## 📝 Final Notes

- Ensure all web servers have correct environment variables set
- Make sure App Pool identity has required permissions
- For public GitHub webhook access, use HTTPS + firewall or reverse proxy
- Azure Blob Storage access is public-read for images, secure-write for app

---

🧠 Built for reliability, automation, and modern DevOps workflows.