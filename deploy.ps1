# deploy.ps1

$repoUrl     = "https://github.com/scafeman/DotNet9Showcase.git"
$branch      = "main"
$deployPath  = "C:\deploy\DotNet9Showcase"
$sitePath    = "C:\inetpub\wwwroot\DotNet9Showcase"
$publishPath = "$deployPath\_publish"
$appPool     = "DotNet9Showcase"

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

# Clean previous publish output
if (Test-Path $publishPath) {
    Write-Host "Cleaning old publish directory: $publishPath"
    Remove-Item $publishPath -Recurse -Force -ErrorAction SilentlyContinue
}

# Publish the app to a clean directory
dotnet publish "$deployPath\DotNet9Showcase.csproj" -c Release -o $publishPath

# Copy published content to IIS web root
Write-Host "Deploying to IIS site path: $sitePath"
Copy-Item "$publishPath\*" $sitePath -Recurse -Force

# Start IIS App Pool
if (Test-Path IIS:\AppPools\$appPool) {
    Write-Host "Starting App Pool: $appPool"
    Start-WebAppPool $appPool
}
