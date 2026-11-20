using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Frontend;

/// <summary>How a session turns a gameplay update into the published display.</summary>
internal interface IGameplayFramePublication
{
    /// <summary>Returns the display that follows a gameplay update.</summary>
    /// <param name="runtime">The gameplay runtime that just advanced.</param>
    /// <param name="loaded">The runtime's loaded room, the update's.</param>
    /// <param name="current">The display currently published.</param>
    /// <param name="frameNumber">The number of the update a newly published frame shows.</param>
    DisplayPacket Publish(SuperMetroidRuntime runtime, RoomOccupancy.Loaded loaded, DisplayPacket current, int frameNumber);
}
