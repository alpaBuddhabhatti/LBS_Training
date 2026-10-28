param(
    [Parameter(Mandatory = $true)]
    [guid]$WorkspaceId
)

$ErrorActionPreference = 'Stop'
# PSScriptRoot resolves samples beside this script, independent of the terminal's current folder.
$query = Get-Content -LiteralPath (Join-Path $PSScriptRoot '01-fictional-status.kql') -Raw
$requestFile = Join-Path $PSScriptRoot '01-fictional-status.request.json'
$request = Get-Content -LiteralPath $requestFile -Raw | ConvertFrom-Json
# Keep both copies identical, including KQL comments; tolerate Windows/Unix line endings.
if ($query.Trim().Replace("`r`n", "`n") -cne $request.query.Trim().Replace("`r`n", "`n")) {
    throw 'The KQL file and request JSON differ. Keep both supplied query copies in sync.'
}
$uri = 'https://api.loganalytics.azure.com/v1/workspaces/{0}/query' -f $WorkspaceId
# CLI supplies the signed-in identity's token. The JSON file avoids shell-quoting the query.
$output = & az rest --method post --url $uri --resource 'https://api.loganalytics.io' --body "@$requestFile" --output json
if ($LASTEXITCODE -ne 0) {
    throw 'Azure Monitor query failed. Check CLI sign-in, workspace ID, network and query permissions.'
}
$result = $output | ConvertFrom-Json
if ($null -ne $result.error) {
    throw "Azure Monitor returned a query error: $($result.error.code)"
}
$table = @($result.tables)[0]
if ($null -eq $table -or @($table.rows).Count -ne 3) {
    throw 'Expected a query table with exactly three fictional records.'
}
if (@($table.columns).Count -ne 2 -or $table.columns[0].name -ne 'Reference' -or $table.columns[1].name -ne 'Status') {
    throw 'Unexpected result columns.'
}
# Synthetic rows prove query access, not real telemetry ingestion from the Function.
$expected = @(
    @{ Reference = 'LBS-DEMO-1001'; Status = 'Completed' },
    @{ Reference = 'LBS-DEMO-1002'; Status = 'Needs review' },
    @{ Reference = 'LBS-DEMO-1003'; Status = 'Received' }
)
for ($i = 0; $i -lt 3; $i++) {
    if ($table.rows[$i][0] -cne $expected[$i].Reference -or $table.rows[$i][1] -cne $expected[$i].Status) {
        throw "Unexpected fictional query values at row $i."
    }
}
$expected | ForEach-Object { [pscustomobject]$_ } | Format-Table Reference, Status
Write-Output 'PASS Azure Monitor query: exactly three expected fictional rows.'
