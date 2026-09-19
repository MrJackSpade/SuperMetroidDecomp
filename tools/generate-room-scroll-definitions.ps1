param(
    [string] $RomPath = "Super Metroid.smc",
    [string] $OutputPath = "csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs"
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$resolvedRom = Resolve-Path (Join-Path $repoRoot $RomPath)
$resolvedStateSource = Resolve-Path (Join-Path $repoRoot 'csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs')
$resolvedOutput = Join-Path $repoRoot $OutputPath

$expectedHash = '12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72'
$actualHash = (Get-FileHash -LiteralPath $resolvedRom -Algorithm SHA256).Hash.ToLowerInvariant()
if ($actualHash -cne $expectedHash) {
    throw "Room-scroll generation requires the unheadered Japan/USA NTSC v1.0 ROM ($expectedHash), got $actualHash."
}

$scrollPointers = [System.Collections.Generic.HashSet[int]]::new()
foreach ($line in [System.IO.File]::ReadLines($resolvedStateSource)) {
    if ($line -notmatch '^\s*new\(') { continue }
    $words = [regex]::Matches($line, '0x([0-9A-F]+)')
    if ($words.Count -ne 16) { continue }
    $pointer = [Convert]::ToInt32($words[10].Groups[1].Value, 16)
    if ($pointer -ge 0x8000) { [void] $scrollPointers.Add($pointer) }
}

$pointers = @($scrollPointers | Sort-Object)
if ($pointers.Count -ne 159) {
    throw "Expected 159 distinct explicit scroll pointers, found $($pointers.Count)."
}

$rom = [System.IO.File]::ReadAllBytes($resolvedRom)
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('using SuperMetroid.Core.Game;')
$lines.Add('using SuperMetroid.Core.Hardware;')
$lines.Add('')
$lines.Add('namespace SuperMetroid.Core.Rooms;')
$lines.Add('')
$lines.Add('/// <summary>An immutable application-owned copy of one native 50-byte room scroll allocation.</summary>')
$lines.Add('public sealed class RoomScrollDefinition')
$lines.Add('{')
$lines.Add('    public RoomScrollDefinition(ushort pointer, byte[] storage)')
$lines.Add('    {')
$lines.Add('        ArgumentNullException.ThrowIfNull(storage);')
$lines.Add('        Pointer = pointer;')
$lines.Add('        Storage = storage.ToArray();')
$lines.Add('    }')
$lines.Add('')
$lines.Add('    public ushort Pointer { get; }')
$lines.Add('')
$lines.Add('    public ReadOnlyMemory<byte> Storage { get; }')
$lines.Add('}')
$lines.Add('')
$lines.Add('/// <summary>Compiled explicit scroll allocations selected by retail room states.</summary>')
$lines.Add('public static class RoomScrollDefinitions')
$lines.Add('{')
$lines.Add('    private static readonly RoomScrollDefinition[] Definitions =')
$lines.Add('    [')
foreach ($pointer in $pointers) {
    $offset = ((0x8F -band 0x7F) * 0x8000) + ($pointer -band 0x7FFF)
    $storage = $rom[$offset..($offset + 0x31)]
    $hex = [Convert]::ToHexString($storage)
    $lines.Add(('        new(0x{0:X4}, Convert.FromHexString("{1}")),' -f $pointer, $hex))
}
$lines.Add('    ];')
$lines.Add('')
$lines.Add('    /// <summary>Number of distinct explicit allocations referenced by retail room states.</summary>')
$lines.Add('    public const int ExplicitDefinitionCount = 159;')
$lines.Add('')
$lines.Add('    static RoomScrollDefinitions()')
$lines.Add('    {')
$lines.Add('        if (Definitions.Length != ExplicitDefinitionCount)')
$lines.Add('            throw new InvalidOperationException("Compiled room-scroll catalog cardinality is inconsistent.");')
$lines.Add('')
$lines.Add('        for (int index = 0; index < Definitions.Length; index++)')
$lines.Add('        {')
$lines.Add('            if (Definitions[index].Storage.Length != RoomScrollGrid.StorageByteCount)')
$lines.Add('                throw new InvalidOperationException("Compiled room-scroll storage is not exactly 50 bytes.");')
$lines.Add('            if (index > 0 && Definitions[index - 1].Pointer >= Definitions[index].Pointer)')
$lines.Add('                throw new InvalidOperationException("Compiled room-scroll pointers must be unique and sorted.");')
$lines.Add('        }')
$lines.Add('    }')
$lines.Add('')
$lines.Add('    /// <summary>Creates the native scroll allocation selected by one compiled room state.</summary>')
$lines.Add('    public static RoomScrollGrid CreateGrid(')
$lines.Add('        ISnesAddressSpace bus,')
$lines.Add('        ushort scrollPointer,')
$lines.Add('        int widthInScreens,')
$lines.Add('        int heightInScreens)')
$lines.Add('    {')
$lines.Add('        short signedPointer = unchecked((short)scrollPointer);')
$lines.Add('        return signedPointer < 0')
$lines.Add('            ? RoomScrollGrid.LoadCompiled(')
$lines.Add('                bus, Get(scrollPointer).Storage.Span, widthInScreens, heightInScreens)')
$lines.Add('            : RoomScrollGrid.CreateImplicit(')
$lines.Add('                bus, widthInScreens, heightInScreens,')
$lines.Add('                RoomScrollStates.FromCartridge(')
$lines.Add('                    unchecked((byte)(scrollPointer + 1)),')
$lines.Add('                    $"implicit scroll word ${scrollPointer:X4}"));')
$lines.Add('    }')
$lines.Add('')
$lines.Add('    /// <summary>Returns one compiled explicit allocation by its bank-$8F pointer.</summary>')
$lines.Add('    public static RoomScrollDefinition Get(ushort pointer)')
$lines.Add('    {')
$lines.Add('        int low = 0;')
$lines.Add('        int high = Definitions.Length - 1;')
$lines.Add('        while (low <= high)')
$lines.Add('        {')
$lines.Add('            int middle = low + ((high - low) >> 1);')
$lines.Add('            int comparison = Definitions[middle].Pointer.CompareTo(pointer);')
$lines.Add('            if (comparison == 0) return Definitions[middle];')
$lines.Add('            if (comparison < 0) low = middle + 1;')
$lines.Add('            else high = middle - 1;')
$lines.Add('        }')
$lines.Add('')
$lines.Add('        throw new ArgumentOutOfRangeException(nameof(pointer), pointer,')
$lines.Add('            "Pointer is not one of the 159 explicit retail room-scroll allocations.");')
$lines.Add('    }')
$lines.Add('}')

[System.IO.File]::WriteAllLines($resolvedOutput, $lines, [System.Text.UTF8Encoding]::new($false))
Write-Host "Wrote $($pointers.Count) room-scroll definitions to $resolvedOutput"
