param([Parameter(Mandatory=$true)][string]$Image, [Parameter(Mandatory=$true)][string]$Output)
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $Output) { throw 'Refusing to overwrite an existing decoded fixture' }
Add-Type -AssemblyName System.Drawing
$bitmap = [Drawing.Bitmap]::new((Resolve-Path -LiteralPath $Image).Path)
$hasher = [Security.Cryptography.SHA256]::Create()
try {
    if ($bitmap.Width -ne 256 -or $bitmap.Height -ne 224) { throw 'Expected a native 256x224 reference' }
    $bytes = [byte[]]::new($bitmap.Width * $bitmap.Height * 4)
    $offset = 0
    for ($y = 0; $y -lt $bitmap.Height; $y++) {
        for ($x = 0; $x -lt $bitmap.Width; $x++) {
            $pixel = $bitmap.GetPixel($x, $y)
            $bytes[$offset++] = $pixel.R
            $bytes[$offset++] = $pixel.G
            $bytes[$offset++] = $pixel.B
            $bytes[$offset++] = $pixel.A
        }
    }
    # Generated fixture data, not a new golden derived from the managed renderer.
    [IO.File]::WriteAllBytes([IO.Path]::GetFullPath($Output), $bytes)
    [BitConverter]::ToString($hasher.ComputeHash($bytes)).Replace('-', '')
} finally { $bitmap.Dispose(); $hasher.Dispose() }
