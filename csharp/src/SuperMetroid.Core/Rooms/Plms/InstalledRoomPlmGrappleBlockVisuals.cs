using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete visual selection for the five Grapple-block frames. Collision, timing,
/// draw shape, and the original terrain remain in compiled definitions.
/// </summary>
public sealed class InstalledRoomPlmGrappleBlockVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding native mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomPlmGrappleBlockVisuals), content =>
    {
        Span<RoomBlockAppearance> block = stackalloc RoomBlockAppearance[1];
        foreach (var draw in GrappleBlockPlmDraws.All.OrderBy(draw => GrappleBlockPlmDraws.VisualId(draw.Frame), StringComparer.Ordinal))
        {
            content.AppendName("frame", GrappleBlockPlmDraws.VisualId(draw.Frame));
            content.Append("runs", 1);
            block[0] = GetAppearance(PlmDraw.Of(draw.Frame));
            content.AppendAppearances("blocks", block);
        }
    });

    /// <summary>Only Grapple-block frame appearances that differ from compiled appearance.</summary>
    private readonly Dictionary<GrappleBlockDraw, RoomBlockAppearance>? customBlocks;

    /// <summary>Validates all five compiled one-block frames and retains authored visual differences without changing Grapple collision, breakup timing, or draw shape.</summary>
    /// <param name="entries">Exactly one visual-only entry for each compiled initial, breakup, and blank draw identity.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry changes an identity/collision bits, repeats a frame, or leaves coverage incomplete.</exception>
    public InstalledRoomPlmGrappleBlockVisuals(IEnumerable<RoomPlmVisualBlockEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<GrappleBlockDraw, RoomBlockAppearance>();
        var seen = new HashSet<GrappleBlockDraw>();
        foreach (RoomPlmVisualBlockEntry entry in entries)
        {
            if (entry is null || !GrappleBlockPlmDraws.TryGetByVisualId(entry.Id, out var draw))
                throw new InvalidDataException(
                    "Grapple-block visuals changed a compiled draw identity.");
            if (!seen.Add(draw.Frame))
                throw new InvalidDataException($"Grapple-block visuals repeat draw {entry.Id}.");
            if (entry.Appearance != draw.Terrain.Appearance)
                selected.Add(draw.Frame, entry.Appearance);
        }

        if (seen.Count != GrappleBlockPlmDraws.All.Count())
            throw new InvalidDataException(
                "Grapple-block visuals do not cover all compiled draw lists.");
        if (selected.Count != 0) customBlocks = selected;
    }

    /// <summary>Resolves the selected appearance for one breakable-Grapple frame while preserving its compiled Grapple or air collision.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <returns>The authored appearance, or the compiled stock appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Grapple-block frame.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw)
    {
        if (!GrappleBlockPlmDraws.TryGet(draw, out var definition))
            throw new InvalidDataException($"Grapple-block visuals lack draw {draw}.");
        return customBlocks is not null && customBlocks.TryGetValue(definition.Frame, out RoomBlockAppearance selected)
            ? selected : definition.Terrain.Appearance;
    }
}
