param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,
    
    [Parameter(Mandatory = $true)]
    [string]$BranchName,
    
    [Parameter(Mandatory = $true)]
    [string]$BuildNumber,
    
    [Parameter(Mandatory = $true)]
    [string]$NexusUrl
)

Write-Host "====================================================="
Write-Host "Starting NuGet Packaging and Publish Process"
Write-Host "Project: $ProjectName"
Write-Host "Branch: $BranchName"
Write-Host "Build Number: $BuildNumber"
Write-Host "====================================================="

# Paths
$workspace = $env:WORKSPACE
$projectPath = "$workspace\$ProjectName\$ProjectName.csproj"
$publishDir = "$workspace\Publish\$ProjectName"
$packageDir = "$workspace\Packages"

# Create directories
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# 🧱 Smart versioning
$version = switch -Wildcard ($BranchName) {
    "master" { "1.0.$BuildNumber-beta" }
    "main" { "1.0.$BuildNumber-beta" }
    "release/*" { 
        $releaseVersion = $BranchName -replace 'release/', ''
        "$releaseVersion.$BuildNumber"
    }
    "hotfix/*" { 
        $hotfixVersion = $BranchName -replace 'hotfix/', ''
        "$hotfixVersion.$BuildNumber-hotfix"
    }
    default { 
        $safeBranch = $BranchName -replace '[^a-zA-Z0-9\-]', '-'
        "1.0.$BuildNumber-$safeBranch-alpha"
    }
}

Write-Host "Building NuGet Package Version: $version" -ForegroundColor Yellow

# STEP 1: PUBLISH THE APPLICATION
Write-Host "=== STEP 1: Publishing Application ===" -ForegroundColor Cyan
Write-Host "Publishing to: $publishDir" -ForegroundColor Yellow

dotnet publish $projectPath -c Release -o $publishDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet publish failed. Aborting." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Application published successfully" -ForegroundColor Green
Write-Host "Published files:" -ForegroundColor Yellow
Get-ChildItem $publishDir -Recurse | ForEach-Object { Write-Host "  - $($_.Name)" }

# STEP 2: CREATE NUGET PACKAGE FROM PUBLISHED FILES
Write-Host "=== STEP 2: Creating NuGet Package ===" -ForegroundColor Cyan

# Create a temporary .nuspec file for the package
$nuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata>
    <id>$ProjectName</id>
    <version>$version</version>
    <title>$ProjectName</title>
    <authors>YourCompany</authors>
    <description>Deployable package for $ProjectName</description>
  </metadata>
  <files>
    <file src="**\*" target="content" />
  </files>
</package>
"@

$nuspecPath = "$workspace\$ProjectName.nuspec"
$nuspecContent | Out-File -FilePath $nuspecPath -Encoding UTF8

Write-Host "Created .nuspec file: $nuspecPath" -ForegroundColor Yellow

# 🔧 Use persistent location for nuget.exe
$toolsDir = "C:\Tools"
$nugetExePath = "$toolsDir\nuget.exe"

# Check if nuget.exe exists in persistent location
if (Test-Path $nugetExePath) {
    Write-Host "✅ Using existing nuget.exe from: $nugetExePath" -ForegroundColor Green
} else {
    Write-Host "Downloading nuget.exe v5.11.0 to persistent location..." -ForegroundColor Yellow
    try {
        # Create tools directory if it doesn't exist
        if (-not (Test-Path $toolsDir)) {
            New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
        }
        
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/v5.11.0/nuget.exe" -OutFile $nugetExePath
        Write-Host "✅ Downloaded nuget.exe to persistent location" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to download nuget.exe: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Create package using nuget.exe with published files as base
Write-Host "Creating package from published files..." -ForegroundColor Yellow

& $nugetExePath pack $nuspecPath -BasePath $publishDir -OutputDirectory $packageDir -NoPackageAnalysis
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ nuget pack failed. Aborting." -ForegroundColor Red
    exit 1
}

# Locate generated package
$packageFile = Get-ChildItem $packageDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $packageFile) {
    Write-Host "❌ No .nupkg file found in $packageDir" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Found Package: $($packageFile.Name)" -ForegroundColor Green
Write-Host "Package size: $([math]::Round($packageFile.Length/1KB, 2)) KB" -ForegroundColor Green

# STEP 3: PUSH TO NEXUS
Write-Host "=== STEP 3: Pushing to Nexus ===" -ForegroundColor Cyan

$nexusApiKey = "f565566d-3dce-398c-9822-394334535e82"

# Verify nuget version
Write-Host "NuGet Version:" -ForegroundColor Yellow
& $nugetExePath | Out-String

# Push package to Nexus
Write-Host "Pushing package to Nexus..." -ForegroundColor Cyan
& $nugetExePath push $packageFile.FullName -Source $NexusUrl -ApiKey $nexusApiKey -SkipDuplicate

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Package pushed successfully to Nexus!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to push package to Nexus." -ForegroundColor Red
    exit 1
}

# Cleanup
Remove-Item $nuspecPath -Force -ErrorAction SilentlyContinue

Write-Host "=== NuGet Package Publish Completed ===" -ForegroundColor Green
Write-Host "Package: $ProjectName v$version" -ForegroundColor Green
Write-Host "Location: Nexus Repository" -ForegroundColor Green
