using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Complete collectible appearance; compiled draws retain their collision and pickup effects independently of the
/// visual override.
/// </summary>
public sealed class InstalledRoomPlmCollectibleVisuals
{
    /// <summary>Canonical identity of the selected visual frames, excluding gameplay mechanics.</summary>
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
    private readonly IReadOnlyDictionary<RoomPlmCollectibleDrawDefinitions.Draw, RoomBlockAppearance> customBlocks;

    /// <summary>Holds the import-validated appearances of the frames that differ from the compiled frames.</summary>
    /// <param name="customBlocks">The changed frames' appearances, keyed by frame.</param>
    internal InstalledRoomPlmCollectibleVisuals(IReadOnlyDictionary<RoomPlmCollectibleDrawDefinitions.Draw, RoomBlockAppearance> customBlocks) =>
        this.customBlocks = Ensure.NotNull(customBlocks);

    /// <summary>Resolves the selected appearance of one collectible draw while leaving its terrain compiled.</summary>
    /// <param name="draw">The collectible draw whose appearance is resolved.</param>
    /// <returns>The authored appearance, or the compiled stock frame's when unchanged.</returns>
    /// <exception cref="InvalidDataException">The draw is not a supported collectible frame.</exception>
    internal RoomBlockAppearance GetAppearance(PlmDraw draw)
    {
        if (!CollectiblePlmDraws.TryGetBlock(draw, out RoomTerrainBlock physical))
            throw new InvalidDataException($"Collectible visuals lack frame {draw}.");
        return customBlocks.TryGetValue(PlmDraw.FrameOf<RoomPlmCollectibleDrawDefinitions.Draw>(draw, "collectible"), out RoomBlockAppearance selected)
            ? selected : physical.Appearance;
    }
}
