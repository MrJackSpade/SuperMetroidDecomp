using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Sparse CPU-bus fixture. Unwritten addresses read as zero, mirroring cleared memory and
/// making every byte relevant to a transfer visible in the setup directly above it.
/// </summary>
internal sealed class TestAddressSpace : ISnesMemoryBus,
    IImportCartridgeSource,
    IRoomEnemyDefinitionSource
{
    /// <summary>Only CPU addresses explicitly seeded by a focused fixture; all others read as zero.</summary>
    private readonly Dictionary<int, byte> _bytes = [];

    /// <summary>Distinguishes an explicitly seeded zero from an unwritten fixture address.</summary>
    internal bool HasSeededByte(int address) => _bytes.ContainsKey(address);

    /// <summary>Reads a seeded CPU byte, returning zero for an unwritten 24-bit address.</summary>
    public byte ReadByte(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        return _bytes.GetValueOrDefault(address);
    }

    /// <summary>Requires an upper LoROM cartridge window before reading the sparse bus.</summary>
    public byte ReadCartridgeByte(int address)
    {
        SnesAddress source = SnesAddress.FromBusAddress(address);
        if (!source.IsUpperLoRomWindow || source.Bank is 0x7e or 0x7f)
            throw new ArgumentOutOfRangeException(nameof(address), address,
                "Synthetic cartridge bytes require an upper LoROM address.");
        return ReadByte(address);
    }

    /// <summary>Reads the exact seeded WRAM alias without synthesizing mirrored banks.</summary>
    public byte ReadWorkRamByte(int address) => ReadByte(address);

    /// <summary>Seeds one validated 24-bit CPU address for a focused fixture.</summary>
    public void WriteByte(int address, byte value)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        _bytes[address] = value;
    }

    /// <summary>Seeds consecutive bytes at their exact CPU addresses.</summary>
    public void WriteBytes(int startAddress, ReadOnlySpan<byte> values)
    {
        for (int index = 0; index < values.Length; index++)
            WriteByte(startAddress + index, values[index]);
    }

    /// <summary>Imports a ROM-shaped enemy header from the fixture's seeded bytes.</summary>
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.LoadAt(this, FixtureHeaderPointer(pointer));

    /// <summary>
    /// The bank-$A0 header pointer a fixture seeds <paramref name="definition"/>'s header at: a member's retail pointer, as
    /// the import translates it, or for a fixture-only identity (a value no member has) the value itself (#1412).
    /// </summary>
    internal static ushort FixtureHeaderPointer(EnemyDefinitionId definition) => Enum.IsDefined(definition)
        ? SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(definition)
        : (ushort)definition;

    /// <summary>The identity a fixture's seeded header pointer names: the inverse of <see cref="FixtureHeaderPointer"/>.</summary>
    internal static EnemyDefinitionId FixtureDefinitionId(ushort pointer) =>
        SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.TryFromHeaderPointer(pointer, out EnemyDefinitionId id)
            ? id
            : (EnemyDefinitionId)pointer;

    /// <summary>Decodes the bank-$A9 falling-tube population record of <paramref name="piece"/> from seeded words.</summary>
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(MotherBrainTubePiece piece)
    {
        int address = MotherBrainNativeListAddresses.TubePopulationRecord(piece);
        ushort Word(int offset) => LittleEndianBytes.Word(ReadByte(address + offset), ReadByte(address + offset + 1));
        EnemyDefinitionId enemy = FixtureDefinitionId(Word(0));
        return SuperMetroid.AssetExtraction.Enemies.RoomEnemyPopulationRecordCodec.Decode(
            enemy, Word(2), Word(4), Word(6), Word(8), Word(10), Word(12), Word(14));
    }

    /// <summary>Reads up to 32 native-sized enemy population records through the terminating sentinel.</summary>
    public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer)
    {
        int cursor = RoomEnemyRomLayout.PopulationBank | pointer;
        var records = new List<RoomEnemyPopulationRecord>();
        for (int slot = 0; slot <= EnemySlotOrder.Capacity; slot++)
        {
            ushort definitionPointerWord = ReadFixtureWord(cursor);
            if (definitionPointerWord == 0xffff)
                return new RoomEnemyPopulationDefinition(
                    $"fixture population ${pointer:X4}", records.ToArray(), ReadByte(cursor + 2));
            EnemyDefinitionId definitionPointer = FixtureDefinitionId(definitionPointerWord);
            if (slot == EnemySlotOrder.Capacity)
                throw new InvalidDataException($"Fixture population ${pointer:X4} exceeds 32 entries.");
            records.Add(SuperMetroid.AssetExtraction.Enemies.RoomEnemyPopulationRecordCodec.Decode(definitionPointer,
                ReadFixtureWord(cursor + 2), ReadFixtureWord(cursor + 4), ReadFixtureWord(cursor + 6), ReadFixtureWord(cursor + 8),
                ReadFixtureWord(cursor + 10), ReadFixtureWord(cursor + 12), ReadFixtureWord(cursor + 14)));
            cursor += 16;
        }
        throw new InvalidDataException($"Fixture population ${pointer:X4} is unterminated.");
    }

    /// <summary>An empty graphics set for a fixture whose empty population never loads one.</summary>
    public static RoomEnemyGraphicsSetDefinition UnusedGraphicsSet { get; } = new("unused fixture graphics set", []);

    /// <summary>Reads up to four enemy graphics headers through the native terminator.</summary>
    public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer)
    {
        int cursor = RoomEnemyRomLayout.TilesetBank | pointer;
        var records = new List<RoomEnemyGraphicsSetHeader>();
        for (int slot = 0; slot <= 4; slot++)
        {
            ushort definitionPointerWord = ReadFixtureWord(cursor);
            if (definitionPointerWord == 0xffff)
                return new RoomEnemyGraphicsSetDefinition($"fixture graphics set ${pointer:X4}", records.ToArray());
            EnemyDefinitionId definitionPointer = FixtureDefinitionId(definitionPointerWord);
            if (slot == 4)
                throw new InvalidDataException($"Fixture graphics set ${pointer:X4} exceeds four entries.");
            records.Add(RoomEnemyGraphicsSetHeader.FromNative(
                definitionPointer, ReadFixtureWord(cursor + 2)));
            cursor += 4;
        }
        throw new InvalidDataException($"Fixture graphics set ${pointer:X4} is unterminated.");
    }

    /// <summary>Combines two seeded bus bytes in native little-endian order.</summary>
    private ushort ReadFixtureWord(int address) =>
        LittleEndianBytes.Word(ReadByte(address), ReadByte(address + 1));
}
