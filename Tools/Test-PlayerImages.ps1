# Scan frames for Unity's flat error-magenta, including where the night mask darkens it.
[CmdletBinding()]
param([string]$Directory = (Join-Path $PSScriptRoot '..\QA\Campaign'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (!('MoonlitShaderCheck' -as [type])) {
    Add-Type @'
public static class MoonlitShaderCheck {
    public static int BadPixels(byte[] bgra) {
        int count = 0;
        for (int i = 0; i < bgra.Length; i += 4) {
            int blue = bgra[i], green = bgra[i + 1], red = bgra[i + 2];
            if (red > 100 && blue > 100 && green < 20 && System.Math.Abs(red - blue) < 10) count++;
        }
        return count;
    }
}
'@
}
$files = @(Get-ChildItem -LiteralPath $Directory -Filter '*.png' -File)
if (!$files.Count) { throw "No native QA captures under $Directory" }
foreach ($file in $files) {
    $bitmap = [Drawing.Bitmap]::new($file.FullName)
    try {
        $size = [Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height)
        $data = $bitmap.LockBits($size, [Drawing.Imaging.ImageLockMode]::ReadOnly, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
        try {
            $buffer = [byte[]]::new($bitmap.Height * $data.Stride)
            [Runtime.InteropServices.Marshal]::Copy($data.Scan0,$buffer,0,$buffer.Length)
            $suspect = [MoonlitShaderCheck]::BadPixels($buffer)
            [pscustomobject]@{ Frame=$file.Name; Width=$bitmap.Width; Height=$bitmap.Height; MagentaPixels=$suspect }
        } finally { $bitmap.UnlockBits($data) }
    } finally { $bitmap.Dispose() }
}
