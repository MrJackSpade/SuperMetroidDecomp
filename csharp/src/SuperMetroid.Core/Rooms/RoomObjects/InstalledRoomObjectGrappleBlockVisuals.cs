using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the five Grapple-block frames (#1557). Their terrain, timing and draw shape stay the
/// game's.
/// </summary>
public sealed class InstalledRoomObjectGrappleBlockVisuals
{
    /// <summary>Each Grapple-block frame's installed picture, owned by these visuals.</summary>
    private readonly Dictionary<GrappleBlockDraw, RoomBlockAppearance> pictures;

    /// <summary>Holds the installed picture of every Grapple-block frame.</summary>
    /// <param name="pictures">Every frame's picture, keyed by frame; copied.</param>
    internal InstalledRoomObjectGrappleBlockVisuals(IReadOnlyDictionary<GrappleBlockDraw, RoomBlockAppearance> pictures) =>
        this.pictures = new Dictionary<GrappleBlockDraw, RoomBlockAppearance>(Ensure.NotNull(pictures));

    /// <summary>The installed picture of one breakable-Grapple frame, whose terrain keeps its Grapple or air collision.</summary>
    /// <param name="draw">The Grapple-block frame.</param>
    /// <returns>The block picture and its mirroring.</returns>
    /// <exception cref="InvalidDataException">The draw is not a Grapple-block frame, or the installation lacks it.</exception>
    internal RoomBlockAppearance GetAppearance(RoomObjectDraw draw)
    {
        if (!GrappleBlockObjectDraws.TryGet(draw, out var definition))
            throw new InvalidDataException($"Grapple-block visuals lack draw {draw}.");
        return pictures.TryGetValue(definition.Frame, out RoomBlockAppearance picture)
            ? picture : throw new InvalidDataException($"The installed Grapple-block visuals hold no frame {definition.Frame}.");
    }
}
