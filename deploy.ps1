# deploy.ps1

$repoUrl    = "https://github.com/scafeman/DotNet9Showcase.git"
$branch     = "main"
$deployPath = "C:\deploy\DotNet9Showcase"
$sitePath   = "C:\inetpub\wwwroot\DotNet9Showcase"

# Clone or pull latest
if (-Not (Test-Path $deployPath)) {
    git clone -b $branch $repoUrl $deployPath
} else {
    Set-Location $deployPath
    git pull origin $branch
}

# Clean old publish folder
Remove-Item "$deployPath\publish" -Recurse -Force -ErrorAction SilentlyContinue

# Publish the app
dotnet publish "$deployPath\DotNet9Showcase.csproj" -c Release -o publish

# Deploy to IIS
Copy-Item "$deployPath\publish\*" $sitePath -Recurse -Force
