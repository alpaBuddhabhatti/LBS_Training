# Blob Storage exercise

Follow the [complete Blob handout](../../Day%201/Basic_Lab_02_Blob_Storage_Portal_And_VS_Code.docx).
It includes networking, scoped data access, a private attendee container,
portal/VS Code transfers, download hash comparisons and safe cleanup.

Use the fictional files in [Samples/Storage](Samples/Storage).
Use your assigned container and Entra identity; never share account keys,
tokens or SAS URLs. A private container does not by itself configure the
storage account's network access.


---

## Day 1 tested walkthrough - student support

Updated 28 October 2026.

### 3. Blob Storage: transfer and verify actual bytes

![blob roundtrip](<../../Day 1/Diagrams/blob-roundtrip.png>)

Open your assigned storage account > Data storage > Containers > your private lab container. Select Microsoft Entra user account authentication, not Access key. Network access and Blob data permission are separate gates: a private container is not the same as a private endpoint.

The authorised instructor provides management Reader for discovery and Storage Blob Data Contributor at your container scope for transfers. An account-level assignment is broader; report it for owner review rather than removing shared access yourself. Role propagation can take up to 30 minutes. Do not keep adding roles, request Owner, copy account keys or use SAS to work around an error.

Standard route: portal-upload Samples\Storage\portal-note.txt, then download that blob from the VS Code terminal with --auth-mode login. Upload Samples\Storage\vscode-note.txt from the terminal, then Refresh the same container in the portal and Download it. Keep overwrite off. If a name already exists, confirm ownership and use an approved fresh name.

If the instructor assigns different blob names, compare each download against the exact original file uploaded under that name. A matching name alone does not establish matching content.

### Safe terminal download with an explicit failure check

```powershell
$account = "YOUR-APPROVED-ACCOUNT"
$container = "YOUR-ASSIGNED-CONTAINER"
$blob = "portal-note.txt"
$download = Join-Path $env:TEMP ("lbs-download-" + [guid]::NewGuid() + ".txt")
az storage blob download --account-name $account --container-name $container --name $blob --file "$download" --auth-mode login --overwrite false -o none
if ($LASTEXITCODE -ne 0) { throw "Download failed; inspect the Azure CLI error above." }
Get-Content -LiteralPath $download
Get-FileHash -LiteralPath $download -Algorithm SHA256
```

Replace all resource placeholders and use the actual approved blob name. A unique temporary file avoids overwriting existing work. Do not continue to read/hash a stale file after a failed transfer. Compare this download hash with the exact original file's Get-FileHash result.

### Portal download: find its real local path

In the browser's Downloads list, use Show in folder and copy the downloaded file's path. The browser may add (1) on repeated downloads. A quoted path pasted by itself does not calculate a hash. Pass it to Get-FileHash as shown below, replacing the example path.

```powershell
Get-FileHash '.\Samples\Storage\vscode-note.txt' -Algorithm SHA256
Get-FileHash 'C:\YOUR-DOWNLOAD-FOLDER\vscode-upload-check.txt' -Algorithm SHA256
```

Compare all characters of the Hash values, not the file names or paths. Matching SHA256 values confirm a byte-for-byte match. Today's reverse transfer matched; that hash is not an expected constant for attendees who edit their fictional source. Check both directions separately. A downloaded file opening successfully or having the right size is weaker evidence than matching hashes.

### Blob troubleshooting and cleanup

403: capture the non-secret error code and check the intended identity, container scope, role propagation and approved network. An extension asking for listKeys or a connection string is not a reason to grant more privilege; use the explicit Entra CLI route. A private blob URL refusing anonymous browser access is expected. Delete only your assigned test blobs/container with owner approval; never delete Function host containers or the shared storage account.
