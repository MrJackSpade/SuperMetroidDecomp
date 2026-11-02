using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete visual selection for the five Grapple-block frames. Collision, timing,
/// draw shape, and the original terrain remain in compiled definitions.
/// </summary>
public sealed class InstalledRoomPlmGrappleBlockVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
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
    private readonly IReadOnlyDictionary<GrappleBlockDraw, RoomBlockAppearance> customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' appearances, keyed by frame.</param>
    internal InstalledRoomPlmGrappleBlockVisuals(IReadOnlyDictionary<GrappleBlockDraw, RoomBlockAppearance> customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves the selected appearance for one breakable-Grapple frame while preserving its compiled Grapple or air collision.</summary>
    /// <param name="draw">The draw whose appearance is resolved.</param>
    /// <returns>The authored appearance, or the compiled stock appearance when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported Grapple-block frame.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw)
    {
        if (!GrappleBlockPlmDraws.TryGet(draw, out var definition))
            throw new InvalidDataException($"Grapple-block visuals lack draw {draw}.");
        return customBlocks.TryGetValue(definition.Frame, out RoomBlockAppearance selected)
            ? selected : definition.Terrain.Appearance;
    }
}
