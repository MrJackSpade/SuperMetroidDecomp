namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Where the Tourian entrance statue's decorations stand: the room points the scene's first placement spawns them at, each
/// tied there as the statue descends.
/// </summary>
internal static class TourianStatueDecorationDefinitions
{
    /// <summary>The room point, in whole pixels, of the decoration on the statue's base.</summary>
    internal static readonly Position16<RoomPixels> BaseDecoration = new(120, 184);

    /// <summary>The room point, in whole pixels, of Ridley's statue.</summary>
    internal static readonly Position16<RoomPixels> RidleyStatue = new(142, 85);

    /// <summary>The room point, in whole pixels, of Phantoon's statue.</summary>
    internal static readonly Position16<RoomPixels> PhantoonStatue = new(132, 136);
}
