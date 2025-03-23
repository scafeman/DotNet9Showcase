# deploy.ps1

$repoUrl    = "https://github.com/scafeman/DotNet9Showcase.git"
$branch     = "main"
$deployPath = "C:\deploy\DotNet9Showcase"
$sitePath   = "C:\inetpub\wwwroot\DotNet9Showcase"
$appPool    = "DotNet9Showcase"

# Stop IIS App Pool to unlock files
Import-Module WebAdministration
if (Test-Path IIS:\AppPools\$appPool) {
    Write-Host "Stopping App Pool: $appPool"
    Stop-WebAppPool $appPool
}

# Clone or pull latest from GitHub
if (-Not (Test-Path $deployPath)) {
    git clone -b $branch $repoUrl $deployPath
} else {
    Set-Location $deployPath
    git pull origin $branch
}

# Publish the app
dotnet publish "$deployPath\DotNet9Showcase.csproj" -c Release -o "$deployPath\publish"

# Copy to IIS folder
Copy-Item "$deployPath\publish\*" $sitePath -Recurse -Force

# Start IIS App Pool
if (Test-Path IIS:\AppPools\$appPool) {
    Write-Host "Starting App Pool: $appPool"
    Start-WebAppPool $appPool
}
