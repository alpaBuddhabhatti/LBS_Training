param(
    [switch]$SubmitTrainingData
)

$ErrorActionPreference = 'Stop'
if (-not $SubmitTrainingData) {
    throw 'This test writes ten fictional records to REAL Azure SQL and triggers hosted Functions. Run with -SubmitTrainingData after approval.'
}

Add-Type -AssemblyName System.Net.Http
$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.AllowAutoRedirect = $false
$handler.UseCookies = $true
$client = [System.Net.Http.HttpClient]::new($handler)
$client.BaseAddress = [Uri]'http://127.0.0.1:5293'
$client.Timeout = [TimeSpan]::FromSeconds(40)
$cases = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'SampleData\cases.json') -Raw | ConvertFrom-Json
$submitted = @()

try {
    foreach ($case in $cases) {
        $get = $client.GetAsync('/Upload').GetAwaiter().GetResult()
        try {
            $get.EnsureSuccessStatusCode() | Out-Null
            $html = $get.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        }
        finally {
            $get.Dispose()
        }

        $match = [regex]::Match($html, 'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
        if (-not $match.Success) {
            throw 'The upload form is missing its antiforgery token.'
        }

        $form = [System.Net.Http.MultipartFormDataContent]::new()
        try {
            $form.Add([System.Net.Http.StringContent]::new(
                [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)), '__RequestVerificationToken')
            $form.Add([System.Net.Http.StringContent]::new($case.reference), 'Reference')
            $bytes = [System.IO.File]::ReadAllBytes((Join-Path $PSScriptRoot ('SampleData\' + $case.file)))
            $form.Add([System.Net.Http.ByteArrayContent]::new($bytes), 'Upload', $case.file)
            $post = $client.PostAsync('/Upload', $form).GetAwaiter().GetResult()
            try {
                if ([int]$post.StatusCode -ne 302) {
                    throw "Submission failed for $($case.file): HTTP $([int]$post.StatusCode). Check the web terminal."
                }
                $location = $post.Headers.Location.ToString()
                if ($location -notmatch '^/Documents/Details/([0-9a-fA-F-]{36})$') {
                    throw 'Unexpected document redirect.'
                }
                $submitted += [PSCustomObject]@{
                    File = $case.file
                    Reference = $case.reference
                    Id = $Matches[1]
                    Path = $location
                    Expected = $case.expectedStatus
                }
            }
            finally {
                $post.Dispose()
            }
        }
        finally {
            $form.Dispose()
        }
    }

    $deadline = [DateTimeOffset]::UtcNow.AddMinutes(3)
    foreach ($document in $submitted) {
        do {
            $get = $client.GetAsync($document.Path).GetAwaiter().GetResult()
            try {
                $get.EnsureSuccessStatusCode() | Out-Null
                $html = $get.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            }
            finally {
                $get.Dispose()
            }

            $status = [regex]::Match($html, '<dd>(Queued|Completed|Needs review)</dd>')
            if (-not $status.Success) {
                throw "Missing status for $($document.Id)."
            }
            if ($status.Groups[1].Value -eq 'Queued') {
                if ([DateTimeOffset]::UtcNow -ge $deadline) {
                    throw "Timed out waiting for hosted Functions. Inspect outbox, Function logs and dead-letter queue. Document: $($document.Id)"
                }
                Start-Sleep -Seconds 3
            }
        } while ($status.Groups[1].Value -eq 'Queued')

        if ($status.Groups[1].Value -ne $document.Expected) {
            throw "$($document.File): expected $($document.Expected), got $($status.Groups[1].Value)."
        }
        Write-Host "PASS LIVE: $($document.File) -> $($document.Expected) [$($document.Id)]"
    }

    $submitted
}
finally {
    $client.Dispose()
    $handler.Dispose()
}
