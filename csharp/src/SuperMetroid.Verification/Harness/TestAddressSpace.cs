using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Harness;


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

    /// <summary>
    /// Advances with every seeded write, so a fixture built from the seeded bytes is known to be current while it is
    /// unchanged.
    /// </summary>
    internal int Version { get; private set; }

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
        Version++;
    }

    /// <summary>Seeds consecutive bytes at their exact CPU addresses.</summary>
    public void WriteBytes(int startAddress, ReadOnlySpan<byte> values)
    {
        for (int index = 0; index < values.Length; index++)
            WriteByte(startAddress + index, values[index]);
    }

    /// <summary>Imports a ROM-shaped enemy header from the fixture's seeded bytes.</summary>
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.LoadAt(this, FixtureHeaderPointer(pointer), pointer);

    /// <summary>
    /// The bank-$A0 header pointer a fixture seeds <paramref name="definition"/>'s header at: a member's retail pointer, as
    /// the import translates it, or for a fixture-only identity (a value no member has) the value itself (#1412).
    /// </summary>
    internal static ushort FixtureHeaderPointer(EnemyDefinitionId definition) => Enum.IsDefined(definition)
        ? SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(definition)
        : (ushort)definition;

    /// <summary>
    /// The bank-$A0 header pointer a fixture seeds graphics-set record <paramref name="record"/> at: its enemy's (see
    /// <see cref="FixtureHeaderPointer(EnemyDefinitionId)"/>), or the header of the artwork it loads for no enemy (#1686).
    /// </summary>
    internal static ushort FixtureHeaderPointer(RoomEnemyGraphicsSetMember record) => record.Source switch
    {
        GraphicsSetEnemy enemy => FixtureHeaderPointer(enemy.DefinitionId),
        GraphicsSetUnplacedArtwork unplaced =>
            SuperMetroid.AssetExtraction.CartridgeTables.EnemyHeaderWords.UnplacedArtworkHeaders.WordsByIdentity[unplaced.Artwork].Single(),
        _ => throw new InvalidOperationException($"Graphics-set record {record.Name} loads an unknown source."),
    };

    /// <summary>The identity a fixture's seeded header pointer names: the inverse of <see cref="FixtureHeaderPointer(EnemyDefinitionId)"/>.</summary>
    internal static EnemyDefinitionId FixtureDefinitionId(ushort pointer) =>
        SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIdsConstants2.TryFromHeaderPointer(pointer, out EnemyDefinitionId id)
            ? id
            : (EnemyDefinitionId)pointer;

    /// <summary>
    /// The actor a fixture's seeded placement of the header at <paramref name="pointer"/> places: the header's identity
    /// (<see cref="FixtureDefinitionId"/>), or, for the header the Ceres doors and walls share, the actor its first
    /// parameter <paramref name="parameter1"/> selects (#1989).
    /// </summary>
    internal static EnemyDefinitionId FixturePlacedActor(ushort pointer, int parameter1) =>
        pointer == SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.CeresDoor)
            ? SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.PlacedActor(pointer, parameter1)
            : FixtureDefinitionId(pointer);

    /// <summary>Decodes the bank-$A9 falling-tube population record of <paramref name="piece"/> from seeded words.</summary>
    public RoomEnemyPlacement ReadFallingTubePopulation(MotherBrainTubePiece piece)
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
        var records = new List<RoomEnemyPlacement>();
        for (int slot = 0; slot <= EnemyPlaceOrder.Capacity; slot++)
        {
            ushort definitionPointerWord = ReadFixtureWord(cursor);
            // The room load reads the quota after a terminator only when entries precede it (#2584).
            if (definitionPointerWord == 0xffff)
                return records.Count == 0
                    ? new EmptyRoomEnemyPopulationDefinition($"fixture population ${pointer:X4}")
                    : new PlacedRoomEnemyPopulationDefinition($"fixture population ${pointer:X4}", records.ToArray(), ReadByte(cursor + 2));
            EnemyDefinitionId definitionPointer = FixturePlacedActor(definitionPointerWord, ReadFixtureWord(cursor + 12));
            if (slot == EnemyPlaceOrder.Capacity)
                throw new InvalidDataException($"Fixture population ${pointer:X4} exceeds 32 entries.");
            records.Add(SuperMetroid.AssetExtraction.Enemies.RoomEnemyPopulationRecordCodec.Decode(definitionPointer,
                ReadFixtureWord(cursor + 2), ReadFixtureWord(cursor + 4), ReadFixtureWord(cursor + 6), ReadFixtureWord(cursor + 8),
                ReadFixtureWord(cursor + 10), ReadFixtureWord(cursor + 12), ReadFixtureWord(cursor + 14)));
            cursor += 16;
        }
        throw new InvalidDataException($"Fixture population ${pointer:X4} is unterminated.");
    }

    /// <summary>
    /// Reads the population at <paramref name="pointer"/>, as <see cref="ReadEnemyPopulation(ushort)"/> does, and records that
    /// it draws under the graphics set at <paramref name="graphicsSetPointer"/> (#1599), as a room header naming both would:
    /// the import's knowledge of the set, which the population's artwork is composed from.
    /// </summary>
    public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer, ushort graphicsSetPointer)
    {
        RoomEnemyPopulationDefinition population = ReadEnemyPopulation(pointer);
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.FixturePopulationSets.Record(population, ReadEnemyGraphicsSet(graphicsSetPointer));
        return population;
    }

    /// <summary>Reads up to four enemy graphics headers through the native terminator.</summary>
    private RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer)
    {
        int cursor = RoomEnemyRomLayout.TilesetBank | pointer;
        var records = new List<RoomEnemyGraphicsSetMember>();
        for (int slot = 0; slot <= 4; slot++)
        {
            ushort definitionPointerWord = ReadFixtureWord(cursor);
            if (definitionPointerWord == 0xffff)
            {
                if (RetailSetOf(records) is { } retail)
                    return retail;
                // A set no retail set holds is coloured in the rows its records' placement words name (#1512).
                var fixture = new RoomEnemyGraphicsSetDefinition($"fixture graphics set ${pointer:X4}");
                SuperMetroid.TestSupport.RepositoryInstallation.Verification.FixtureGraphicsSetMembers.Record(fixture, [.. records]);
                return fixture;
            }
            if (slot == 4)
                throw new InvalidDataException($"Fixture graphics set ${pointer:X4} exceeds four entries.");
            // A seeded header of artwork a set loads for no enemy (#1686) is that artwork's record.
            int row = RoomEnemyGraphicsSetMember.NativePaletteRow(ReadFixtureWord(cursor + 2));
            records.Add(SuperMetroid.AssetExtraction.CartridgeTables.EnemyHeaderWords.UnplacedArtworkHeaders.TryDecode(definitionPointerWord, out UnplacedEnemyArtwork unplaced)
                ? new RoomEnemyGraphicsSetMember(unplaced, row)
                : new RoomEnemyGraphicsSetMember(FixtureDefinitionId(definitionPointerWord), row));
            cursor += 4;
        }
        throw new InvalidDataException($"Fixture graphics set ${pointer:X4} is unterminated.");
    }

    /// <summary>
    /// The retail graphics set holding exactly <paramref name="records"/>, each in the palette row the import colours its
    /// member's sheets in (#1512), or null when no retail set holds them: a fixture seeding a retail set's records loads as
    /// that set, as the game's room loader would.
    /// </summary>
    private static RoomEnemyGraphicsSetDefinition? RetailSetOf(List<RoomEnemyGraphicsSetMember> records)
    {
        // Only the cartridge's own sets: the session's start-up set is no retail set and loads no records.
        return RoomEnemyGraphicsSetLists.All
            .FirstOrDefault(set => RoomEnemyGraphicsSetLists.MembersOf(set).Records.Span.SequenceEqual(records.ToArray()));
    }

    /// <summary>Combines two seeded bus bytes in native little-endian order.</summary>
    private ushort ReadFixtureWord(int address) =>
        LittleEndianBytes.Word(ReadByte(address), ReadByte(address + 1));
}
