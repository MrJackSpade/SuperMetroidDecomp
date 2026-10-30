using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete collectible appearance; compiled draws retain their collision and pickup effects independently of the
/// visual override.
/// </summary>
public sealed class InstalledRoomPlmCollectibleVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmCollectibleVisuals), content =>
    {
        Span<RoomBlockAppearance> buffer = stackalloc RoomBlockAppearance[1];
        foreach (var frame in CollectiblePlmDraws.All.OrderBy(frame => frame.Id, StringComparer.Ordinal))
        {
            content.AppendName("frame", frame.Id);
            content.Append("runs", 1);
            buffer[0] = GetAppearance(PlmDraw.Of(frame.Draw));
            content.AppendAppearances("blocks", buffer);
        }
    });

    /// <summary>Only collectible frame appearances that differ from compiled stock appearance.</summary>
    private readonly Dictionary<RoomPlmCollectibleDrawDefinitions.Draw, RoomBlockAppearance>? customBlocks;

    /// <summary>Validates all twenty-four compiled one-cell frames and retains authored visual differences without changing pickup, collision, or persistence rules.</summary>
    /// <param name="entries">Exactly one appearance for each compiled collectible frame identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, or leaves compiled frame coverage incomplete.</exception>
    public InstalledRoomPlmCollectibleVisuals(
        IEnumerable<RoomPlmVisualBlockEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<RoomPlmCollectibleDrawDefinitions.Draw, RoomBlockAppearance>();
        var seen = new HashSet<RoomPlmCollectibleDrawDefinitions.Draw>();
        foreach (RoomPlmVisualBlockEntry entry in entries)
        {
            if (entry is null ||
                !CollectiblePlmDraws.TryGetById(entry.Id, out var frame))
                throw new InvalidDataException(
                    "Collectible visuals changed a compiled frame identity.");
            if (!seen.Add(frame.Draw))
                throw new InvalidDataException($"Collectible visuals repeat frame {entry.Id}.");
            if (entry.Appearance != frame.Terrain.Appearance)
                selected.Add(frame.Draw, entry.Appearance);
        }
        if (seen.Count != CollectiblePlmDraws.All.Count())
            throw new InvalidDataException("Collectible visuals do not cover all compiled frames.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves the selected appearance of one collectible draw while leaving its terrain compiled.</summary>
    /// <param name="draw">The collectible draw whose appearance is resolved.</param>
    /// <returns>The authored appearance, or the compiled stock frame's when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported collectible frame.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw)
    {
        if (!CollectiblePlmDraws.TryGetBlock(draw, out RoomTerrainBlock physical))
            throw new InvalidDataException($"Collectible visuals lack frame {draw}.");
        return customBlocks is not null &&
            customBlocks.TryGetValue(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw, "collectible"), out RoomBlockAppearance selected)
            ? selected : physical.Appearance;
    }
}
