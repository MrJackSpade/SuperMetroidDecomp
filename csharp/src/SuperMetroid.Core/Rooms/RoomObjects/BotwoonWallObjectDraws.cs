namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Botwoon's wall block draw (#2034): the block shape its clear draws, from <see cref="BotwoonWallObjectDrawDefinitions"/>,
/// and the clear's installed picture (#1557).
/// </summary>
internal static class BotwoonWallObjectDraws
{
    /// <summary>The wall's clear, pictured by the installed Botwoon wall frame.</summary>
    internal static RoomObjectDrawFamily<BotwoonWallDraw> Family { get; } =
        new RoomObjectDrawFamily<BotwoonWallDraw>("Botwoon wall",
            frame => RoomObjectDrawFamily<BotwoonWallDraw>.Defined(BotwoonWallObjectDrawDefinitions.Shapes, frame));
}
