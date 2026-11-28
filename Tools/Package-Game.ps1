# Run after building the Windows player. Includes source notices and excludes Unity's debug-only files.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$playerDir = Join-Path $projectRoot 'Build\PC'
if (!(Test-Path -LiteralPath (Join-Path $playerDir 'Cuoi_Data\Managed\Assembly-CSharp.dll'))) {
    throw 'Build the Windows player before packaging.'
}

$credits = @(
    'CREDITS.md',
    'Documentation\ASSETS.md',
    'Documentation\asset-prompts.json',
    'Documentation\polish-asset-prompt.txt',
    'Documentation\readability-asset-prompts.md',
    'Documentation\campaign-scenery-sources.json',
    'Documentation\campaign-landmark-sources.json',
    'Documentation\landscape-sources.json',
    'Documentation\terrain-sources.json',
    'Documentation\surface-crests-source.json',
    'Documentation\wetland-sources.json',
    'Documentation\campaign-ui-sources.json',
    'Documentation\GUARDIAN_AUDIO_CREDITS.md',
    'Documentation\BANYAN_DUEL.md',
    'Documentation\MOON_CROSSINGS.md',
    'Assets\Moonlit\Art\Cuoi.png',
    'Assets\Moonlit\Resources\CuoiCrouch.png',
    'Assets\Moonlit\Resources\FourLandsAtNight.png',
    'Assets\Moonlit\Art\ForestExtras.png',
    'Assets\Moonlit\Art\Environment.png',
    'Assets\Moonlit\Resources\BanyanGuardian.png',
    'Assets\Moonlit\Resources\BanyanRootArm.png',
    'Documentation\WETLAND_CREATURE_CREDITS.md',
    'Documentation\CUOI_ANIMATION_CREDITS.md',
    'Documentation\FESTIVAL_PIANO.md',
    'Documentation\Unity-UI-LICENSE.md',
    'Assets\TextMesh Pro\Fonts\LiberationSans - OFL.txt',
    'Tools\ScoreMoonlit.py',
    'Tools\FestivalPiano.py'
)
foreach ($relative in $credits) {
    $distributionFile = Join-Path $playerDir $relative
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($distributionFile)) | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot $relative) -Destination $distributionFile -Force
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$settings = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectSettings.asset') -Raw
$version = [regex]::Match($settings, '(?m)^\s*bundleVersion:\s*(\d+\.\d+\.\d+)\s*$').Groups[1].Value
if (!$version) { throw 'Missing release version in Unity player settings.' }
$archiveDir = [IO.Directory]::CreateDirectory((Join-Path $projectRoot 'Builds')).FullName
$zipPath = Join-Path $archiveDir "CuoiTimTrang-$version-Windows-x64.zip"
$zipTemp = "$zipPath.$([guid]::NewGuid().ToString('N')).tmp"
$archive = [IO.Compression.ZipFile]::Open($zipTemp, [IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($item in Get-ChildItem -LiteralPath $playerDir -File -Recurse) {
        $relative = $item.FullName.Substring($playerDir.Length + 1).Replace('\','/')
        if ($relative -match '(^|/)[^/]*_DoNotShip(/|$)') { continue }
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $archive, $item.FullName, $relative, [IO.Compression.CompressionLevel]::Optimal
        ) | Out-Null
    }
} finally {
    $archive.Dispose()
}
Move-Item -LiteralPath $zipTemp -Destination $zipPath -Force
Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
