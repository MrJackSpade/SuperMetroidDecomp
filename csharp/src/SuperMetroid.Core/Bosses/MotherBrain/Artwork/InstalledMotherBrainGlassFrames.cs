using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed pictures of the eleven frames of Mother Brain's glass (#1557), by frame. The glass's geometry,
/// collision, hits, shards and timing stay with its typed object and draws.
/// </summary>
public sealed class InstalledMotherBrainGlassFrames
{
    /// <summary>Holds the pictures the import made of every glass frame.</summary>
    /// <param name="images">Every glass frame's picture.</param>
    internal InstalledMotherBrainGlassFrames(RoomObjectFrameImages<MotherBrainGlassDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every glass frame's picture.</summary>
    internal RoomObjectFrameImages<MotherBrainGlassDraw> Images { get; }
}
