param(
    [switch]$Deploy
)

$ErrorActionPreference = 'Stop'
$subscription = 'fbd069b7-a20d-4619-9fbe-a7d8b2ab3367'
$project = Join-Path $PSScriptRoot 'Functions\LbsDocumentTracker.Azure.Functions.csproj'
$publish = Join-Path $PSScriptRoot 'Functions\bin\Release\net10.0\linux-x64\publish'
$zip = Join-Path $PSScriptRoot 'Functions\bin\document-functions-linux.zip'

Push-Location $PSScriptRoot
try {
    dotnet publish $project -c Release -r linux-x64 --self-contained false
    if ($LASTEXITCODE -ne 0) {
        throw 'Functions publish failed.'
    }
}
finally {
    Pop-Location
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (Test-Path -LiteralPath $zip) {
    Remove-Item -LiteralPath $zip
}

$archive = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in Get-ChildItem -LiteralPath $publish -Recurse -File -Force) {
        # ZIP entry names must use portable separators even on Windows PowerShell.
        $entry = $file.FullName.Substring($publish.Length + 1).Replace('\', '/')
        if ($entry -match '(^|/)local\.settings') {
            throw 'Local settings must not be deployed.'
        }
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $file.FullName, $entry, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $archive.Dispose()
}

$archive = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $names = @($archive.Entries | ForEach-Object { $_.FullName })
    if ('host.json' -notin $names -or 'worker.config.json' -notin $names -or
        -not ($names | Where-Object { $_.StartsWith('.azurefunctions/') })) {
        throw 'Functions package is missing required root configuration or extension payload.'
    }
}
finally {
    $archive.Dispose()
}

Write-Output "Validated Linux deployment package: $zip"
if ($Deploy) {
    Write-Warning 'This replaces the existing functions in fristAzureFunctionApp. Owner approval and a recovery package are required.'
    az functionapp deployment source config-zip --resource-group Training --name fristAzureFunctionApp `
        --src $zip --build-remote false --subscription $subscription `
        --output none
    if ($LASTEXITCODE -ne 0) {
        throw 'Function deployment failed. Inspect deployment logs before retrying.'
    }

    $functions = @(az functionapp function list --resource-group Training --name fristAzureFunctionApp `
        --subscription $subscription --query '[].name' -o tsv)
    if ($LASTEXITCODE -ne 0 -or
        'fristAzureFunctionApp/DispatchTrainingDocuments' -notin $functions -or
        'fristAzureFunctionApp/ProcessTrainingDocument' -notin $functions) {
        throw 'Deployment returned, but both expected Function triggers were not indexed. Inspect host logs.'
    }
    Write-Output 'Verified both hosted document-processing Functions. Run Test-Live.ps1 to verify data delivery.'
}
