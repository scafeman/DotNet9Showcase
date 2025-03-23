# deploy.ps1

$repoUrl    = "https://github.com/scafeman/DotNet9Showcase.git"
$branch     = "main"
$deployPath = "C:\deploy\DotNet9Showcase"
$sitePath   = "C:\inetpub\wwwroot\DotNet9Showcase"

# Clone or pull latest from GitHub
if (-Not (Test-Path $deployPath)) {
    git clone -b $branch $repoUrl $deployPath
} else {
    Set-Location $deployPath
    git pull origin $branch
}

# Publish the app
dotnet publish "$deployPath\DotNet9Showcase.csproj" -c Release -o "$deployPath\publish"

# Copy published output to IIS site directory
Copy-Item "$deployPath\publish\*" $sitePath -Recurse -Force
