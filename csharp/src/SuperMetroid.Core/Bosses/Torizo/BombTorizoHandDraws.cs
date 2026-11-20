using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Bomb Torizo's hand block draws (#2034): the block shape each draws, from <see cref="BombTorizoHandObjectDrawDefinitions"/>,
/// and the installed picture (#1557) each shows.
/// </summary>
internal static class BombTorizoHandDraws
{
    /// <summary>The intact and cleared hand, pictured by the installed hand frames.</summary>
    internal static RoomObjectDrawFamily<BombTorizoHandDraw> Family { get; } =
        new RoomObjectDrawFamily<BombTorizoHandDraw>("Bomb Torizo hand",
            frame => RoomObjectDrawFamily<BombTorizoHandDraw>.Defined(BombTorizoHandObjectDrawDefinitions.Shapes, frame));
}
