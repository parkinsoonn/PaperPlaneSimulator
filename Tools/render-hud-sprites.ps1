# Renders the HUD kit's SVG assets (Docs/hud/assets) to transparent PNG sprites (Assets/Resources/UI)
# with headless Microsoft Edge, so no SVG package is needed in Unity.
# Small sprites are rendered at 2x for sharpness; Unity imports them with pixelsPerUnit = 100 * scale.
# Usage (from the project root):  powershell -ExecutionPolicy Bypass -File Tools/render-hud-sprites.ps1 [-Only icon_coin,tape_1]
param(
    [string]$Source = "Docs/hud/assets",
    [string]$Output = "Assets/Resources/UI",
    [string[]]$Only = @()
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false
$edge = "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
if (-not (Test-Path $edge)) { $edge = "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe" }
if (-not (Test-Path $edge)) { throw "Microsoft Edge not found" }

$root = Resolve-Path "$PSScriptRoot\.."
$src = if ([IO.Path]::IsPathRooted($Source)) { $Source } else { Join-Path $root $Source }
$out = if ([IO.Path]::IsPathRooted($Output)) { $Output } else { Join-Path $root $Output }
New-Item -ItemType Directory -Force $out | Out-Null
$profileDir = Join-Path $env:TEMP "hud-render-edge-profile"

Get-ChildItem $src -Filter *.svg | Where-Object { $Only.Count -eq 0 -or $Only -contains $_.BaseName } | ForEach-Object {
    $svg = Get-Content $_.FullName -Raw
    if ($svg -notmatch '<svg[^>]*\swidth="([\d.]+)"[^>]*\sheight="([\d.]+)"') { Write-Warning "$($_.Name): no width/height"; return }
    $w = [math]::Ceiling([double]$Matches[1]); $h = [math]::Ceiling([double]$Matches[2])
    $scale = if ([math]::Max($w, $h) -le 512) { 2 } else { 1 }
    $png = Join-Path $out ($_.BaseName + ".png")
    $url = "file:///" + ($_.FullName -replace '\\', '/')
    # Edge reports progress on stderr; run it through cmd so Windows PowerShell doesn't treat that as an error.
    $edgeArgs = "--headless=new --disable-gpu --hide-scrollbars --no-first-run --user-data-dir=`"$profileDir`" " +
        "--default-background-color=00000000 --force-device-scale-factor=$scale " +
        "--window-size=$w,$h --screenshot=`"$png`" `"$url`""
    cmd /c "`"$edge`" $edgeArgs 2>nul" | Out-Null
    if (Test-Path $png) { "{0,-28} {1}x{2} @{3}x" -f $_.BaseName, $w, $h, $scale } else { Write-Warning "$($_.Name): render failed" }
}
