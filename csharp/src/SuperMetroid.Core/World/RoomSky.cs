using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// A sky room's scrolling sky (#1392, #2513): the sky's drifting bands and vertical scroll, which its room main moves, and the
/// installed image its rows draw from. A room either has both or neither.
/// </summary>
/// <param name="State">The sky's drifting bands and vertical scroll.</param>
/// <param name="Image">Resolves what the sky's rows show on the background.</param>
[RoomLifetime]
internal sealed partial record RoomSky(ScrollingSkyState State, RoomBackgroundImageResolver Image);
