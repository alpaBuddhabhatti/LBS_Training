param(
    [Parameter(Mandatory = $true)]
    [uri]$WebsiteUrl,
    [uri]$FunctionUrl
)
$ErrorActionPreference = 'Stop'
# These requests check HTTP content, not JavaScript button behaviour or deployment history.
if ($WebsiteUrl.Scheme -notin @('http', 'https')) {
    throw 'WebsiteUrl must use HTTP or HTTPS.'
}
$baseUrl = $WebsiteUrl.AbsoluteUri.TrimEnd('/')
$index = Invoke-WebRequest "$baseUrl/" -UseBasicParsing -TimeoutSec 20
if ($index.StatusCode -ne 200 -or
    $index.Content -notmatch 'Dummy demonstration only' -or
    $index.Content -notmatch 'VS Code demo v[12]') {
    throw 'Website did not return the expected dummy page and build label.'
}
Write-Output 'PASS website index and demo label'
$records = Invoke-RestMethod "$baseUrl/sample-data.json" -TimeoutSec 20
# Compare the fixture in its prescribed order, not merely a successful HTTP response.
$expected = @(
    @{ reference = 'LBS-DEMO-1001'; status = 'Completed' },
    @{ reference = 'LBS-DEMO-1002'; status = 'Needs review' },
    @{ reference = 'LBS-DEMO-1003'; status = 'Received' }
)
if ($records.Count -ne 3) {
    throw "Expected exactly three fictional records, got $($records.Count)."
}
for ($i = 0; $i -lt $expected.Count; $i++) {
    if ($records[$i].reference -ne $expected[$i].reference -or
        $records[$i].status -ne $expected[$i].status -or
        [string]::IsNullOrWhiteSpace($records[$i].document) -or
        [string]::IsNullOrWhiteSpace($records[$i].summary)) {
        throw "Unexpected fictional record at index $i."
    }
}
Write-Output 'PASS all three sample references, statuses and descriptions'
if ($null -ne $FunctionUrl) {
    # Optional independent endpoint check; the website itself does not call the Function.
    # Keep cloud key-bearing URLs out of saved commands and shared evidence.
    if ($FunctionUrl.Scheme -notin @('http', 'https')) {
        throw 'FunctionUrl must use HTTP or HTTPS.'
    }
    $result = Invoke-RestMethod $FunctionUrl.AbsoluteUri -TimeoutSec 20
    if ($result.reference -ne 'LBS-DEMO-1001' -or
        $result.status -ne 'Received' -or
        $result.fictionalData -ne $true) {
        throw 'Function returned unexpected training JSON.'
    }
    Write-Output 'PASS Function response'
}
Write-Output 'Browser detail-button and Azure deployment checks remain separate.'
