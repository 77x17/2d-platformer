param([string]$Project = (Join-Path $PSScriptRoot '..'))
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
if (-not ('MoonlitLandAudit' -as [type])) {
    Add-Type -TypeDefinition @"
using System;
public sealed class MoonlitLandAudit
{
    public int opaque, touchingBase, missingBaseColumns, largestDetached;
    public static MoonlitLandAudit Inspect(byte[] bgra, int width, int stride, int top, int bottom)
    {
        int rows = bottom - top + 1;
        var result = new MoonlitLandAudit();
        byte[] mask = new byte[width * rows];
        int[] queue = new int[mask.Length];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (bgra[(top + y) * stride + x * 4 + 3] >= 96)
                {
                    mask[y * width + x] = 1;
                    result.opaque++;
                }
                else if (y == rows - 1) result.missingBaseColumns++;
            }
        }
        // Eight-connected components allow diagonal pixel-art stems and roots.
        for (int start = 0; start < mask.Length; start++)
        {
            if (mask[start] != 1) continue;
            int head = 0, tail = 0;
            bool touches = false;
            mask[start] = 2;
            queue[tail++] = start;
            while (head != tail)
            {
                int p = queue[head++], x = p % width, y = p / width;
                if (y == rows - 1) touches = true;
                for (int yy = Math.Max(0, y - 1); yy <= Math.Min(y + 1, rows - 1); yy++)
                    for (int step = -1; step <= 1; step++)
                    {
                        // The runtime repeats these strips, so a branch may cross the image seam.
                        int xx = (x + step + width) % width;
                        int neighbor = yy * width + xx;
                        if (mask[neighbor] != 1) continue;
                        mask[neighbor] = 2;
                        queue[tail++] = neighbor;
                    }
            }
            if (touches) result.touchingBase += tail;
            else result.largestDetached = Math.Max(result.largestDetached, tail);
        }
        return result;
    }
}
"@
}
$assets = Join-Path $Project 'Assets/Moonlit/Resources'
$cameraSource = Get-Content -Raw -LiteralPath (Join-Path $Project 'Assets/Moonlit/Scripts/MoonlitCamera.cs')
function Get-Rows([string]$name) {
    $expression = [regex]::Escape($name) + '\s*=\s*\{([^}]+)\}'
    $matchArray = [regex]::Match($cameraSource, $expression)
    if (!$matchArray.Success) { throw "Missing camera slice array $name" }
    [int[]]($matchArray.Groups[1].Value.Split(',') | ForEach-Object { $_.Trim() })
}
$middleBottom = Get-Rows 'MiddleBottom'
$bankTop = Get-Rows 'BankTop'
$edgeTop = Get-Rows 'EdgeTop'
$edgeBottom = Get-Rows 'EdgeBottom'
for ($n = 1; $n -le 4; $n++) {
    $bands = @(
        @{Name='Middle'; Asset="Embankment$n"; Top=0; Bottom=$middleBottom[$n-1]}
        @{Name='Near'; Asset="Embankment$n"; Top=$bankTop[$n-1]; Bottom=940}
        @{Name='Edge'; Asset='EdgeHabitat'; Top=$edgeTop[$n-1]; Bottom=$edgeBottom[$n-1]}
    )
    foreach ($band in $bands) {
        $file = Join-Path $assets ($band.Asset + '.png')
        $bitmap = [Drawing.Bitmap]::new((Resolve-Path $file).Path)
        try {
            $rect = [Drawing.Rectangle]::new(0,0,$bitmap.Width,$bitmap.Height)
            $data = $bitmap.LockBits($rect, [Drawing.Imaging.ImageLockMode]::ReadOnly, [Drawing.Imaging.PixelFormat]::Format32bppArgb)
            try {
                $pixels = [byte[]]::new($data.Stride * $bitmap.Height)
                [Runtime.InteropServices.Marshal]::Copy($data.Scan0,$pixels,0,$pixels.Length)
                $top = [int][Math]::Round($band.Top * $bitmap.Height / 941)
                $bottom = [int][Math]::Round(($band.Bottom + 1) * $bitmap.Height / 941) - 1
                $r = [MoonlitLandAudit]::Inspect($pixels,$bitmap.Width,$data.Stride,$top,$bottom)
                [pscustomobject]@{
                    Chapter=$n; Plane=$band.Name; Width=$bitmap.Width; MissingBaseColumns=$r.missingBaseColumns
                    OpaquePixels=$r.opaque; ConnectedPercent=[math]::Round(100 * $r.touchingBase / $r.opaque,4)
                    LargestDetachedPixels=$r.largestDetached
                }
            } finally { $bitmap.UnlockBits($data) }
        } finally { $bitmap.Dispose() }
    }
}
