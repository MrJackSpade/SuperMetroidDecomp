namespace SuperMetroid.Core.Rooms;

/// <summary>One reachable Draygon cannon frame's blocks in native run order.</summary>
public sealed record RoomPlmDraygonCannonVisualEntry(string Id, ushort[] Blocks);

/// <summary>
/// Editable cannon block appearances. Native draw geometry, collision,
/// projectile thresholds, damaged-state transitions and control-word writes
/// remain compiled and cannot be modified through this resource.
/// </summary>
public sealed class RoomPlmDraygonCannonVisualCatalog
{
    private readonly IReadOnlyDictionary<ushort, ushort[]> blocks;

    public RoomPlmDraygonCannonVisualCatalog(
        IEnumerable<RoomPlmDraygonCannonVisualEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<ushort, ushort[]>();
        foreach (RoomPlmDraygonCannonVisualEntry entry in entries)
        {
            if (entry is null || entry.Blocks is null ||
                !DraygonCannonPlmDrawDefinitions.TryGetByVisualId(entry.Id,
                    out var draw) ||
                entry.Blocks.Length != draw.Runs.Span.ToArray().Sum(run =>
                    run.LevelWords.Length) ||
                entry.Blocks.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
                throw new InvalidDataException(
                    "Draygon cannon visuals changed a frame identity, draw shape, or visual word.");
            if (!selected.TryAdd(draw.Pointer, entry.Blocks.ToArray()))
                throw new InvalidDataException(
                    $"Draygon cannon visuals repeat frame {entry.Id}.");
        }
        if (selected.Count != DraygonCannonPlmDrawDefinitions.All.Count())
            throw new InvalidDataException(
                "Draygon cannon visuals do not cover all reachable compiled frames.");
        blocks = selected;
    }

    public static RoomPlmDraygonCannonVisualCatalog Stock() => new(
        DraygonCannonPlmDrawDefinitions.All.Select(draw =>
            new RoomPlmDraygonCannonVisualEntry(
                DraygonCannonPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span.ToArray().SelectMany(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord)).ToArray())));

    public ushort GetWord(ushort drawPointer, int runIndex, int blockIndex)
    {
        if (!blocks.TryGetValue(drawPointer, out ushort[]? words) ||
            !DraygonCannonPlmDrawDefinitions.TryGet(drawPointer, out var draw))
            throw new InvalidDataException(
                $"Draygon cannon visuals lack frame ${drawPointer:X4}.");
        if ((uint)runIndex >= (uint)draw.Runs.Length ||
            (uint)blockIndex >= (uint)draw.Runs.Span[runIndex].LevelWords.Length)
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        int flatIndex = blockIndex;
        for (int run = 0; run < runIndex; run++)
            flatIndex += draw.Runs.Span[run].LevelWords.Length;
        return words[flatIndex];
    }
}
