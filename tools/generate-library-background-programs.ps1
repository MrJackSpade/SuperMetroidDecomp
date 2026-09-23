param(
    [Parameter(Mandatory = $true)]
    [string]$RomPath
)

$ErrorActionPreference = 'Stop'
try {
    $workspace = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, '..'))
    $target = [IO.Path]::GetFullPath([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.Core', 'Rooms', 'LibraryBackgroundProgramDefinitions.Generated.cs'))
    if (-not $target.StartsWith($workspace + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "Generated catalog path escapes the repository: $target"
    }

    $rom = [IO.File]::ReadAllBytes([IO.Path]::GetFullPath($RomPath))
    if ($rom.Length -ne 3145728) {
        throw "Expected the unheadered 3 MiB pinned ROM, got $($rom.Length) bytes."
    }
    $identityFile = [IO.File]::ReadAllText([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.AssetExtraction', 'SupportedCartridge.cs'))
    $identity = [regex]::Match($identityFile, 'public const string Sha256 = "([0-9a-f]{64})"')
    if (-not $identity.Success -or
        -not [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($rom)).Equals(
            $identity.Groups[1].Value, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'The ROM does not match the pinned Japan/USA NTSC v1.0 cartridge.'
    }

    $states = [IO.File]::ReadAllLines([IO.Path]::Combine($workspace, 'csharp', 'src',
        'SuperMetroid.Core', 'Rooms', 'RoomStateDefinitions.cs'))
    $pointers = @($states | Where-Object { $_ -match '^\s*new\(0x' } | ForEach-Object {
        $fields = ($_.Trim() -replace '^new\(', '' -replace '\),?$', '').Split(',')
        if ($fields.Length -ne 16) { throw "Unexpected room-state definition: $_" }
        [Convert]::ToInt32($fields[14].Trim().Substring(2), 16)
    } | Where-Object { $_ -ge 0x8000 } | Sort-Object -Unique)

    function Read-Word([int]$pointer) {
        if ($pointer -lt 0x8000 -or $pointer -gt 0xFFFE) {
            throw ('Bank-$8F word read out of range at ${0:X4}.' -f $pointer)
        }
        $offset = 0x78000 + ($pointer - 0x8000)
        return [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8)
    }
    function Read-Long([int]$pointer) {
        if ($pointer -lt 0x8000 -or $pointer -gt 0xFFFD) {
            throw ('Bank-$8F long read out of range at ${0:X4}.' -f $pointer)
        }
        $offset = 0x78000 + ($pointer - 0x8000)
        return [int]$rom[$offset] -bor ([int]$rom[$offset + 1] -shl 8) -bor
            ([int]$rom[$offset + 2] -shl 16)
    }

    $lines = [Collections.Generic.List[string]]::new()
    $lines.Add('// Generated from the pinned cartridge by tools/generate-library-background-programs.ps1.')
    $lines.Add('// Fixed command operands only; graphics and tilemaps remain installed presentation assets.')
    $lines.Add('namespace SuperMetroid.Core.Rooms;')
    $lines.Add('')
    $lines.Add('public static partial class LibraryBackgroundProgramDefinitions')
    $lines.Add('{')
    $lines.Add('    private static readonly LibraryBackgroundProgram[] programs =')
    $lines.Add('    [')
    $instructionCount = 0
    foreach ($pointer in $pointers) {
        $cursor = $pointer
        $lines.Add(('        new(0x{0:X4}, [' -f $pointer))
        $terminated = $false
        for ($guard = 0; $guard -lt 128; $guard++) {
            $command = Read-Word $cursor
            $cursor += 2
            if ($command -eq 0) { $terminated = $true; break }
            $source = 0
            $destination = 0
            $byteCount = 0
            $door = 0
            switch ($command) {
                2 { $name = 'TransferToVram'; $source = Read-Long $cursor;
                    $destination = Read-Word ($cursor + 3); $byteCount = Read-Word ($cursor + 5);
                    $cursor += 7 }
                4 { $name = 'DecompressToWorkRam'; $source = Read-Long $cursor;
                    $destination = Read-Word ($cursor + 3); $cursor += 5 }
                6 { $name = 'ClearFxTilemap' }
                8 { $name = 'TransferToVramForKraid'; $source = Read-Long $cursor;
                    $destination = Read-Word ($cursor + 3); $byteCount = Read-Word ($cursor + 5);
                    $cursor += 7 }
                10 { $name = 'ClearBg2' }
                12 { $name = 'ClearBg2ForKraid' }
                14 { $name = 'TransferForDoor'; $door = Read-Word $cursor;
                    $source = Read-Long ($cursor + 2);
                    $destination = Read-Word ($cursor + 5); $byteCount = Read-Word ($cursor + 7);
                    $cursor += 9 }
                default { throw ('Unknown bank-$8F library command ${0:X4} at ${1:X4}.' -f
                        $command, ($cursor - 2)) }
            }
            $lines.Add(('            new(LibraryBackgroundCommand.{0}, 0x{1:X6}, 0x{2:X4}, 0x{3:X4}, 0x{4:X4}),' -f
                $name, $source, $destination, $byteCount, $door))
            $instructionCount++
        }
        if (-not $terminated) { throw ('Unterminated library list ${0:X4}.' -f $pointer) }
        $lines.Add(('        ], 0x{0:X4}),' -f ($cursor - $pointer)))
    }
    $lines.Add('    ];')
    $lines.Add('}')
    [IO.File]::WriteAllLines($target, $lines, [Text.UTF8Encoding]::new($false))
    Write-Output "Generated $($pointers.Count) lists and $instructionCount typed instructions at $target"
}
catch {
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
