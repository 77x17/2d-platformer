# Compile the two wildlife encounters and four chapter samples captured with -parallaxClip.
[CmdletBinding()]
param([string]$CaptureRoot, [string]$ReportPath, [string]$OutputDirectory, [string]$Ffmpeg='ffmpeg')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if (!$CaptureRoot) { $CaptureRoot = Join-Path $root 'Temp/ParallaxFrames' }
if (!$ReportPath) { $ReportPath = Join-Path $root 'QA/Campaign/results.json' }
$preview = if ($OutputDirectory) { $OutputDirectory } else { Join-Path $root 'QA/Campaign' }
[IO.Directory]::CreateDirectory($preview) | Out-Null
$sections = @('chapter-1','chapter-2','crow','chapter-3','fish','chapter-4')
$themes = @('village','bamboo','river','hill')
$durations = @(4,9,9,4)
$audioDir = Join-Path $root 'Assets/Moonlit/Resources/MoonAudio'
$movie = Join-Path $preview 'CuoiTimTrang-gameplay.mp4'
$arguments = @('-hide_banner','-loglevel','error','-y')
foreach ($section in $sections) {
    $dir = Join-Path $CaptureRoot $section
    $expected = if ($section.StartsWith('chapter')) { 120 } else { 150 }
    if (@(Get-ChildItem -LiteralPath $dir -Filter '*.png').Count -ne $expected) { throw "Missing frames for $section." }
    $arguments += @('-framerate','30','-i',(Join-Path $dir '%04d.png'))
}
foreach ($theme in $themes) { $arguments += @('-i',(Join-Path $audioDir "mus_$theme.ogg")) }
$cues = @(
    @{animal='crow';field='warningFrame';sound='crowCall';start=8},
    @{animal='crow';field='attackFrame';sound='crowFlight';start=8},
    @{animal='fish';field='warningFrame';sound='carpBubbles';start=17},
    @{animal='fish';field='attackFrame';sound='carpLeap';start=17},
    @{animal='fish';field='returnFrame';sound='carpSplash';start=17}
)
foreach ($cue in $cues) { $arguments += @('-i',(Join-Path $audioDir "sfx_$($cue.sound).wav")) }
$filters = @()
foreach ($n in 0..5) { $filters += "[$($n):v]setpts=PTS-STARTPTS[v$n]" }
foreach ($n in 0..3) {
    $audio = $sections.Count + $n
    $duration = $durations[$n]
    $end = ($duration-.18).ToString([Globalization.CultureInfo]::InvariantCulture)
    $filters += "[$($audio):a]atrim=duration=$duration,asetpts=PTS-STARTPTS,volume=0.78,afade=t=in:d=0.09,afade=t=out:st=$($end):d=0.18[a$n]"
}
$filters += '[v0][v1][v2][v3][v4][v5]concat=n=6:v=1:a=0[v]'
$filters += '[a0][a1][a2][a3]concat=n=4:v=0:a=1[score]'
foreach ($n in 0..4) {
    $cue = $cues[$n]
    if ($cue.field -eq 'returnFrame') {
        $report = Get-Content $ReportPath -Raw | ConvertFrom-Json
        $escape = $report.checks | Where-Object name -eq 'escape_film_fish'
        if ($escape.detail -notmatch 'return=(\d+)') { throw 'Missing fish splash frame.' }
        $frame = [int]$Matches[1]
    } else {
        $timing = Import-Csv (Join-Path $CaptureRoot "$($cue.animal)/cues.csv") | Select-Object -First 1
        $value = $timing.($cue.field)
        if ($null -eq $value) { throw "Missing $($cue.sound) cue." }
        $frame = [int]$value
    }
    if ($frame -lt 0 -or $frame -ge 150) { throw "Invalid $($cue.sound) cue." }
    $ms = [int][Math]::Round(1000 * ($cue.start + $frame/30.0))
    $filters += "[$($n+10):a]aformat=sample_rates=24000:channel_layouts=stereo,volume=0.65,adelay=$($ms):all=1[s$n]"
}
$filters += '[score][s0][s1][s2][s3][s4]amix=inputs=6:duration=first:normalize=0,alimiter=limit=0.89:level=false[a]'
$arguments += @('-filter_complex',($filters -join ';'),'-map','[v]','-map','[a]','-c:v','libx264','-crf','18','-preset','fast','-pix_fmt','yuv420p','-c:a','aac','-b:a','144k','-movflags','+faststart',$movie)
& $Ffmpeg @arguments
if ($LASTEXITCODE -ne 0) { throw "ffmpeg exited $LASTEXITCODE" }
& $Ffmpeg -hide_banner -loglevel error -y -i $movie -t 4 -c copy (Join-Path $preview 'Parallax-BoTre.mp4')
if ($LASTEXITCODE -ne 0) { throw "ffmpeg exited $LASTEXITCODE" }

$motion = @('-hide_banner','-loglevel','error','-y')
foreach ($kind in 'run','climb') {
    $dir = Join-Path $CaptureRoot $kind
    if (@(Get-ChildItem -LiteralPath $dir -Filter '*.png').Count -ne 150) { throw "Missing frames for $kind." }
    $motion += @('-framerate','30','-i',(Join-Path $dir '%04d.png'))
}
$motion += @('-i',(Join-Path $audioDir 'mus_village.ogg'),'-i',(Join-Path $audioDir 'mus_river.ogg'))
$motionFilters = @(
    '[0:v]crop=624:352:120:118,scale=960:540:flags=neighbor,setsar=1[v0]',
    '[1:v]crop=624:352:120:102,scale=960:540:flags=neighbor,setsar=1[v1]',
    '[2:a]atrim=duration=5,volume=0.8,afade=t=out:st=4.82:d=0.18[a0]',
    '[3:a]atrim=duration=5,volume=0.8,afade=t=in:d=0.12,afade=t=out:st=4.82:d=0.18[a1]',
    '[v0][a0][v1][a1]concat=n=2:v=1:a=1[v][a]'
)
$motion += @('-filter_complex',($motionFilters -join ';'),'-map','[v]','-map','[a]','-c:v','libx264','-crf','17','-pix_fmt','yuv420p','-c:a','aac','-b:a','144k','-movflags','+faststart',(Join-Path $preview 'Cuoi-chay-va-leo.mp4'))
& $Ffmpeg @motion
if ($LASTEXITCODE -ne 0) { throw "ffmpeg exited $LASTEXITCODE" }
