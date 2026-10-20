using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Immutable, ordered input to the room PLM allocator. Native pointers survive only
/// as identities and diagnostic context; construction never reads a CPU bus.
/// </summary>
public sealed class RoomPlmPopulationDefinition
{
    /// <summary>Owned placement records kept in the order the PLM allocator consumes them.</summary>
    private readonly RoomPlmPlacement[] placements;

    /// <summary>The population's name, the disassembly's label for it without its prefix, or a fixture's description.</summary>
    public string Name { get; }
    /// <summary>Validated placement records in native allocation order, excluding the zero header terminator; scroll and collectible payloads are owned copies.</summary>
    public ReadOnlyMemory<RoomPlmPlacement> Placements => placements;

    /// <summary>Copies an ordered, bounded decoded population and validates setup-specific scroll and collectible payloads before any PLM slots are allocated.</summary>
    /// <param name="name">The population's name, used in diagnostics.</param>
    /// <param name="records">Placement sequence without a native zero terminator; at most 255 records may be supplied.</param>
    /// <exception cref="ArgumentNullException">The sequence or one of its placements is null.</exception>
    /// <exception cref="InvalidDataException">The record limit is exceeded, a header is the zero terminator, or a setup-specific payload is invalid.</exception>
    public RoomPlmPopulationDefinition(string name, IEnumerable<RoomPlmPlacement> records)
    {
        Ensure.NotNull(records);
        Name = Ensure.NotNull(name);
        // A native population is bounded by its 256-record parser. Never enumerate
        // an unbounded caller sequence before enforcing that same upper limit.
        placements = records.Take(RoomPlmPopulationFormat.MaximumParserIterations).Select(CopyPlacement).ToArray();
        if (placements.Length > RoomPlmPopulationFormat.MaximumRecordCount)
            throw new InvalidDataException($"PLM population {name} exceeds the bounded native record count.");
    }

    /// <summary>Builds a retail population from its compiled records, binding scroll triggers to their compiled program identities.</summary>
    /// <param name="name">The population's name.</param>
    /// <param name="records">Each record's PLM header, block coordinates and room argument, in native order.</param>
    internal static RoomPlmPopulationDefinition FromRecords(string name, RoomPlmPlacementDefinition[] records) =>
        new(name, Ensure.NotNull(records).Select(record => new RoomPlmPlacement(
            RoomPlmHeaderDefinitions.Get(record.Header), record.Block, record.Argument)
            { CompiledScrollSource = record.Header == PlmHeaderId.ScrollTrigger ? record.Argument : null }));

    /// <summary>Copies one placement's payloads and rejects malformed scroll programs or collectible graphics.</summary>
    private static RoomPlmPlacement CopyPlacement(RoomPlmPlacement placement)
    {
        Ensure.NotNull(placement);
        if (placement.Header.Header == 0)
            throw new InvalidDataException("A decoded PLM placement cannot contain the native zero terminator.");
        byte[] program = placement.ScrollProgram.ToArray();
        int? compiledSource = program.Length == 0 ? placement.CompiledScrollSource : null;
        if (compiledSource is { } sourcePointer)
        {
            if (placement.Header.Header != PlmHeaderId.ScrollTrigger || program.Length != 0 ||
                !RoomPlmScrollProgramDefinitions.TryApply(checked((ushort)sourcePointer), null))
                throw new InvalidDataException("Retail scroll placement has an invalid program identity.");
        }
        else if (placement.Header.Header == PlmHeaderId.ScrollTrigger)
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
            if (!CollectiblePlms.TryIdentifyPermanentCollectible(placement.Header.Header, out var kind, out _) ||
                kind != graphic.Kind || kind < InWorldCollectibleKind.Bombs ||
                graphic.Tiles.Length != 0x100 || graphic.PaletteOffsets.Length != 8 ||
                graphic.PaletteOffsets.Span.ContainsAnyExceptInRange((byte)0, (byte)7))
                throw new InvalidDataException("Decoded collectible graphics do not match their placement or payload shape.");
            graphic = graphic with { Tiles = graphic.Tiles.ToArray(), PaletteOffsets = graphic.PaletteOffsets.ToArray() };
        }
        return placement with { ScrollProgram = program, DynamicGraphic = graphic, CompiledScrollSource = compiledSource };
    }
}
