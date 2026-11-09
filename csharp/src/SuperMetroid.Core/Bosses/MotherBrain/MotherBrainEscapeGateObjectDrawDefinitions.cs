using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The shape of the three four-block gate frames chosen by the Mother Brain escape gate. What each frame shows is its
/// installed picture (#1557).
/// </summary>
internal static class MotherBrainEscapeGateObjectDrawDefinitions
{
    /// <summary>The cells each gate frame covers, down its column from the origin.</summary>
    internal const int Height = 4;

    /// <summary>How every cell of every gate frame collides, the visually open frame included.</summary>
    internal const RoomCollisionType Collision = RoomCollisionType.SolidBlock;
}
