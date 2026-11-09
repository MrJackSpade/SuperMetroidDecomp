using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the sixteen collectible frames that show a picture of their own (#1557); a block pair's
/// frame lies under the item's own picture (#1538) and has none. The frames' terrain and pickup effects stay the game's.
/// </summary>
public sealed class InstalledRoomObjectCollectibleVisuals
{
    /// <summary>Each installed collectible frame's picture, owned by these visuals.</summary>
    private readonly Dictionary<RoomObjectCollectibleDrawDefinitions.Draw, RoomBlockAppearance> pictures;

    /// <summary>Holds the installed picture of every collectible frame that shows one.</summary>
    /// <param name="pictures">Every such frame's picture, keyed by frame; copied.</param>
    internal InstalledRoomObjectCollectibleVisuals(IReadOnlyDictionary<RoomObjectCollectibleDrawDefinitions.Draw, RoomBlockAppearance> pictures) =>
        this.pictures = new Dictionary<RoomObjectCollectibleDrawDefinitions.Draw, RoomBlockAppearance>(Ensure.NotNull(pictures));

    /// <summary>The installed picture of one collectible frame.</summary>
    /// <param name="draw">The collectible frame.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a collectible frame, or the installation holds no picture of it.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw)
    {
        if (!CollectibleObjectDraws.TryGetBlock(draw, out _))
            throw new InvalidDataException($"Collectible visuals lack frame {draw}.");
        RoomObjectCollectibleDrawDefinitions.Draw frame = RoomObjectDraw.FrameOf<RoomObjectCollectibleDrawDefinitions.Draw>(draw, "collectible");
        return pictures.TryGetValue(frame, out RoomBlockAppearance picture)
            ? picture : throw new InvalidDataException($"The installed collectible visuals hold no frame {frame}.");
    }
}
