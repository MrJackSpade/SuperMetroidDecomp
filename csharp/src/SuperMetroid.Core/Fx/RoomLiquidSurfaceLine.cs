using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The live surface of a room's liquid while the liquid has one (#1546, #2554): its room line after rising and tide
/// processing, which each liquid motion pass and each viewport priming republishes. The room's effect creates it once the
/// liquid's surface is first published and moves it from then on; a room whose liquid has no surface has none, so a reader
/// that holds one always has a surface to read.
/// </summary>
[RoomLifetime]
public sealed class RoomLiquidSurfaceLine
{
    /// <summary>Publishes a liquid's first surface on room line <paramref name="line"/>.</summary>
    /// <param name="line">The surface's room line, negative once a tide carries it above the room's top.</param>
    internal RoomLiquidSurfaceLine(int line) => Line = line;

    /// <summary>
    /// The surface's room line, negative once a tide carries it above the room's top, where the room shows no liquid.
    /// </summary>
    public int Line { get; internal set; }
}
