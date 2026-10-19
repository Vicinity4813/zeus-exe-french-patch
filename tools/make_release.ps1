# Builds the exe and packs the release zip: dist\Zeus-EXE-French-Patch-<version>.zip
# (Zeus EXE French Patch.exe + LISEZMOI.txt + source\).
# Compile l'exe et prepare le zip de la release.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
& cmd /c "`"$root\build.cmd`""
if ($LASTEXITCODE -ne 0) { throw 'build failed' }
$exe = Join-Path $root 'dist\Zeus EXE French Patch.exe'
$version = (Get-Item -LiteralPath $exe).VersionInfo.ProductVersion
$stage = Join-Path $root 'dist\stage'
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory "$stage\source" | Out-Null
Copy-Item -LiteralPath $exe -Destination $stage
Copy-Item -LiteralPath "$root\docs\LISEZMOI.txt" -Destination $stage
Copy-Item -Path "$root\src\*" -Destination "$stage\source"
Copy-Item -LiteralPath "$root\LICENSE" -Destination $stage
$zip = Join-Path $root "dist\Zeus-EXE-French-Patch-$version.zip"
Compress-Archive -Path "$stage\*" -DestinationPath $zip -CompressionLevel Optimal -Force
Remove-Item $stage -Recurse -Force
"Release: $zip"
