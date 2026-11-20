namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed pictures of the breakable Grapple block's five frames (#1557). Their Grapple and air collision and the
/// respawn timing stay the game's.
/// </summary>
public sealed class InstalledRoomObjectGrappleBlockVisuals : IRoomObjectFamilyPictures
{
    /// <summary>Holds the pictures the import made of every Grapple-block frame.</summary>
    /// <param name="images">Every Grapple-block frame's picture, keyed by its draw.</param>
    internal InstalledRoomObjectGrappleBlockVisuals(RoomObjectFrameImages<RoomObjectDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every Grapple-block frame's picture, keyed by its draw.</summary>
    internal RoomObjectFrameImages<RoomObjectDraw> Images { get; }

    /// <inheritdoc/>
    IReadOnlyDictionary<RoomObjectDraw, RoomObjectFrameImage> IRoomObjectFamilyPictures.Pictures =>
        Images.All;
}
