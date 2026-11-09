using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The installed pictures of the open, half-closed and closed frames of the gate in Mother Brain's escape room (#1557),
/// by frame. Every frame's cells stay solid, the visually open one included, and the closing timing stays with the
/// gate's typed object.
/// </summary>
public sealed class InstalledEscapeGateFrames
{
    /// <summary>Holds the pictures the import made of every gate frame.</summary>
    /// <param name="images">Every gate frame's picture.</param>
    internal InstalledEscapeGateFrames(RoomObjectFrameImages<MotherBrainEscapeGateDraw> images) => Images = Ensure.NotNull(images);

    /// <summary>Every gate frame's picture.</summary>
    internal RoomObjectFrameImages<MotherBrainEscapeGateDraw> Images { get; }
}
