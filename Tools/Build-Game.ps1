param([string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.16f1\Editor\Unity.exe', [switch]$EditorOpen)
$ErrorActionPreference = 'Stop'
$projectPath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($EditorOpen) {
    Set-Content -LiteralPath (Join-Path $projectPath 'QA\build.request') -Value 'build-windows'
    Write-Output 'Build requested. The open Unity Editor will pick it up when compilation finishes. See QA/build-result.txt.'
} else {
    $arguments = '-batchmode -nographics -quit -projectPath "' + $projectPath + '" -executeMethod Moonlit.Editor.LevelOneBuilder.Build -logFile "' + (Join-Path $projectPath 'QA\unity-build.log') + '"'
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { throw "Unity exited with code $($process.ExitCode). See QA/unity-build.log. If this project is open, use -EditorOpen." }
    Get-Content -LiteralPath (Join-Path $projectPath 'QA\build-result.txt')
}
