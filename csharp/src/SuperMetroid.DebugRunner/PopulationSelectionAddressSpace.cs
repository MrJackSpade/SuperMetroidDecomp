using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>
/// Read-only audit decorator that republishes selected literal population records as one
/// compact bank-$A1 list and creates a matching bank-$B4 graphics set. Record bytes and all
/// definition/code/palette/tile reads still come from the user's cartridge; only list shape
/// is synthetic so a family can be audited without first translating unrelated neighbors.
/// </summary>
internal sealed class PopulationSelectionAddressSpace : ISnesAddressSpace
{
    public const ushort PopulationPointer = 0xf000;
    public const ushort TilesetPointer = 0xf000;

    private readonly ISnesAddressSpace _inner;
    private readonly Dictionary<int, byte> _overlay = new();

    public PopulationSelectionAddressSpace(
        ISnesAddressSpace inner,
        IReadOnlyList<RoomEnemyPopulationRecord> records,
        byte deathQuota = 0)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(records);
        if (records.Count > RoomEnemySystem.MaximumEnemyCount)
            throw new ArgumentOutOfRangeException(nameof(records));

        _inner = inner;
        int populationAddress = 0xa10000 | PopulationPointer;
        for (int index = 0; index < records.Count; index++)
            WritePopulationRecord(populationAddress + index * 16, records[index]);
        WriteWord(populationAddress + records.Count * 16, 0xffff);
        _overlay[populationAddress + records.Count * 16 + 2] = deathQuota;

        // ProcessEnemyTilesets accepts at most four definitions. Family-focused audits use
        // one in practice, but retaining the actual limit makes misuse fail at construction.
        ushort[] definitions = records
            .Select(record => record.DefinitionPointer)
            .Distinct()
            .ToArray();
        if (definitions.Length > 4)
            throw new ArgumentException("Selected population has more than four graphics definitions.", nameof(records));

        int tilesetAddress = 0xb40000 | TilesetPointer;
        for (int index = 0; index < definitions.Length; index++)
        {
            WriteWord(tilesetAddress + index * 4, definitions[index]);
            // Palette row zero is a legal native graphics-set destination. The definition
            // still supplies its real palette and tile data; this word merely selects where
            // the audit stages them in otherwise-empty CGRAM/VRAM.
            WriteWord(tilesetAddress + index * 4 + 2, unchecked((ushort)index));
        }
        WriteWord(tilesetAddress + definitions.Length * 4, 0xffff);
    }

    public byte ReadByte(int address) =>
        _overlay.TryGetValue(address & 0xffffff, out byte value)
            ? value
            : _inner.ReadByte(address);

    public void WriteByte(int address, byte value) => _inner.WriteByte(address, value);

    private void WritePopulationRecord(int address, RoomEnemyPopulationRecord record)
    {
        WriteWord(address, record.DefinitionPointer);
        WriteWord(address + 2, record.XPosition);
        WriteWord(address + 4, record.YPosition);
        WriteWord(address + 6, record.InitializationParameter);
        WriteWord(address + 8, record.Properties);
        WriteWord(address + 10, record.ExtraProperties);
        WriteWord(address + 12, record.Parameter1);
        WriteWord(address + 14, record.Parameter2);
    }

    private void WriteWord(int address, ushort value)
    {
        _overlay[address & 0xffffff] = unchecked((byte)value);
        _overlay[(address + 1) & 0xffffff] = unchecked((byte)(value >> 8));
    }
}
