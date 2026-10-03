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
    RoomPlmDynamicCollectibleGraphic? DynamicGraphic = null)
{
    /// <summary>Internal retail identity; constructed placements must still supply validated decoded pairs.</summary>
    internal ushort? CompiledScrollSource { get; init; }
}

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
        var records = new List<RoomPlmPlacement>();
        RoomPlmPopulationDefinitions.Place(pointer, (header, x, y, argument) =>
        {
            records.Add(new RoomPlmPlacement(
                RoomPlmHeaderDefinitions.Get(header), x, y, argument)
                { CompiledScrollSource = header == RoomPlmHeaders.ScrollTrigger ? argument : null });
        });
        return new RoomPlmPopulationDefinition(pointer, records);
    }

    private static RoomPlmPlacement CopyPlacement(RoomPlmPlacement placement)
    {
        ArgumentNullException.ThrowIfNull(placement);
        if (placement.Header.Header == 0)
            throw new InvalidDataException("A decoded PLM placement cannot contain the native zero terminator.");
        byte[] program = placement.ScrollProgram.ToArray();
        ushort? compiledSource = program.Length == 0 ? placement.CompiledScrollSource : null;
        if (compiledSource is { } sourcePointer)
        {
            if (placement.Header.Header != RoomPlmHeaders.ScrollTrigger || program.Length != 0 ||
                !RoomPlmScrollProgramDefinitions.TryApply(sourcePointer, null))
                throw new InvalidDataException("Retail scroll placement has an invalid program identity.");
        }
        else if (placement.Header.Header == RoomPlmHeaders.ScrollTrigger)
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
        return placement with { ScrollProgram = program, DynamicGraphic = graphic, CompiledScrollSource = compiledSource };
    }
}
