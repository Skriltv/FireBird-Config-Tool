$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'FireBird Config Tool.csproj'
$out = Join-Path $PSScriptRoot 'release'
$exeName = 'FireBird Config Tool.exe'

Write-Host 'Preparing FireBird Config Tool build...' -ForegroundColor Cyan

# A previous build may still be running and holding clrjit.dll (or another DLL) open.
# Try to close only this app before cleaning the output folder.
Get-Process -Name 'FireBird Config Tool' -ErrorAction SilentlyContinue | ForEach-Object {
    try {
        Write-Host "Closing running FireBird Config Tool (PID $($_.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    } catch {}
}

# Clean the existing release folder when possible. If Windows still has a file locked,
# publish to a fresh timestamped folder instead of failing before compilation starts.
$publishOut = $out
if (Test-Path $out) {
    $removed = $false
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        try {
            Remove-Item $out -Recurse -Force -ErrorAction Stop
            $removed = $true
            break
        } catch {
            if ($attempt -lt 3) {
                Write-Host "Release folder is in use; retrying cleanup ($attempt/3)..." -ForegroundColor Yellow
                Start-Sleep -Milliseconds 800
            }
        }
    }

    if (-not $removed) {
        $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
        $publishOut = Join-Path $PSScriptRoot "release-$stamp"
        Write-Host "Could not remove the old release folder because a file is locked." -ForegroundColor Yellow
        Write-Host "Publishing to: $publishOut" -ForegroundColor Yellow
        New-Item -ItemType Directory -Path $publishOut -Force | Out-Null
    }
}

Write-Host 'Publishing FireBird Config Tool (Windows x64, self-contained)...' -ForegroundColor Cyan

dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $publishOut
if ($LASTEXITCODE -ne 0) {
    Write-Host "`nBuild failed (dotnet exit code $LASTEXITCODE)." -ForegroundColor Red
    exit $LASTEXITCODE
}

$exe = Join-Path $publishOut $exeName
if (-not (Test-Path $exe)) {
    throw "Publish completed but the expected executable was not created: $exe"
}

# Single-file release: remove any loose files generated alongside the executable.
Get-ChildItem -Path $publishOut -File | Where-Object { $_.FullName -ne $exe } | Remove-Item -Force -ErrorAction SilentlyContinue

$remaining = @(Get-ChildItem -Path $publishOut -File)
if ($remaining.Count -ne 1 -or $remaining[0].Name -ne $exeName) {
    throw "Single-file publish expected exactly '$exeName' in $publishOut, but found: $($remaining.Name -join ', ')"
}

Write-Host "`nDone: $exe" -ForegroundColor Green
Write-Host 'Single EXE release: no .dll/.json/.pdb files are included beside the application.' -ForegroundColor Green
if ($publishOut -ne $out) {
    Write-Host 'The previous release folder was left untouched because Windows still had a file open.' -ForegroundColor Yellow
}
