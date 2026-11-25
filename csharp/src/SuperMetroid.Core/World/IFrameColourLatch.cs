using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.World;

/// <summary>
/// The frame's colours as the frame boundary latches them (#3036): the room's shading under the display's fades and Samus's
/// own colours on the sprite draws the boundary makes visible. The cartridge uploads the colours an update leaves with the
/// sprites and scrolls it leaves, in the same NMI, so a frame shows one update's colours with that update's scene.
/// </summary>
internal interface IFrameColourLatch
{
    /// <summary>The colours of the frame the boundary is latching in <paramref name="room"/>, over <paramref name="shownSprites"/>.</summary>
    /// <param name="room">The loaded room whose frame the boundary latches.</param>
    /// <param name="shownSprites">The sprite frame the boundary has just made visible.</param>
    FramePaint LatchColours(RoomScope room, SpriteFrameBuffer shownSprites);
}
