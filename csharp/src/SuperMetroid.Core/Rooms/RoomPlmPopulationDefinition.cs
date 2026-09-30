using System.Buffers.Binary;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One decoded placement and its setup metadata. Program/graphics payloads are
/// bounded domain records, not an address space that can supply arbitrary bytes.
/// </summary>
public sealed record RoomPlmPlacement(
    RoomPlmHeaderDefinition Header,
    byte BlockX,
    byte BlockY,
    ushort RoomArgument,
    ReadOnlyMemory<byte> ScrollProgram = default,
    RoomPlmDynamicCollectibleGraphic? DynamicGraphic = null);

/// <summary>
/// Immutable, ordered input to the room PLM allocator. Native pointers survive only
/// as identities and diagnostic context; construction never reads a CPU bus.
/// </summary>
public sealed class RoomPlmPopulationDefinition
{
    private readonly RoomPlmPlacement[] placements;

    public ushort Pointer { get; }
    public ReadOnlyMemory<RoomPlmPlacement> Placements => placements;

    public RoomPlmPopulationDefinition(ushort pointer, IEnumerable<RoomPlmPlacement> records)
    {
        ArgumentNullException.ThrowIfNull(records);
        Pointer = pointer;
        // A native population is bounded by its 256-record parser. Never enumerate
        // an unbounded caller sequence before enforcing that same upper limit.
        placements = records.Take(RoomPlmPopulationFormat.MaximumParserIterations).Select(CopyPlacement).ToArray();
        if (placements.Length > RoomPlmPopulationFormat.MaximumRecordCount)
            throw new InvalidDataException($"PLM population $8F:{pointer:X4} exceeds the bounded native record count.");
    }

    /// <summary>Resolves the complete fixed placement list before allocation begins.</summary>
    public static RoomPlmPopulationDefinition FromCompiled(ushort pointer)
    {
        ReadOnlySpan<byte> source = RoomPlmPopulationDefinitions.Get(pointer).Span;
        var records = new List<RoomPlmPlacement>((source.Length - 2) / RoomPlmPopulationFormat.RecordByteCount);
        for (int offset = 0; offset < source.Length - 2; offset += RoomPlmPopulationFormat.RecordByteCount)
        {
            ushort header = BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
            ushort argument = BinaryPrimitives.ReadUInt16LittleEndian(source[(offset + 4)..]);
            records.Add(new RoomPlmPlacement(
                RoomPlmHeaderDefinitions.Get(header), source[offset + 2], source[offset + 3], argument,
                header == RoomPlmHeaders.ScrollTrigger
                    ? RoomPlmScrollProgramDefinitions.Get(argument) : default));
        }
        return new RoomPlmPopulationDefinition(pointer, records);
    }

    private static RoomPlmPlacement CopyPlacement(RoomPlmPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        if (placement.Header.Header == 0)
            throw new InvalidDataException("A decoded PLM placement cannot contain the native zero terminator.");
        byte[] program = placement.ScrollProgram.ToArray();
        if (placement.Header.Header == RoomPlmHeaders.ScrollTrigger)
        {
            if (program.Length is < 1 or > 2 * RoomScrollGrid.StorageByteCount - 1 ||
                program.Length % 2 == 0 || (program[^1] & 0x80) == 0)
                throw new InvalidDataException("A decoded scroll PLM needs a bounded, terminated pair program.");
            for (int offset = 0; offset < program.Length - 1; offset += 2)
            {
                if (program[offset] >= RoomScrollGrid.StorageByteCount ||
                    program[offset + 1] > (byte)RoomScrollState.Green)
                    throw new InvalidDataException("A decoded scroll PLM contains an invalid index/state pair.");
            }
        }
        else if (program.Length != 0)
            throw new InvalidDataException("Only a resident scroll PLM may own scroll pairs.");

        RoomPlmDynamicCollectibleGraphic? graphic = placement.DynamicGraphic;
        if (graphic is not null)
        {
            if (!RoomPlmSystem.TryIdentifyPermanentCollectible(placement.Header.Header, out var kind, out _) ||
                kind != graphic.Kind || kind < InWorldCollectibleKind.Bombs ||
                graphic.Tiles.Length != 0x100 || graphic.PaletteOffsets.Length != 8 ||
                graphic.PaletteOffsets.Span.ContainsAnyExceptInRange((byte)0, (byte)7))
                throw new InvalidDataException("Decoded collectible graphics do not match their placement or payload shape.");
            graphic = graphic with { Tiles = graphic.Tiles.ToArray(), PaletteOffsets = graphic.PaletteOffsets.ToArray() };
        }
        return placement with { ScrollProgram = program, DynamicGraphic = graphic };
    }
}
