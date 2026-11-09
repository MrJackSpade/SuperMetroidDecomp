using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The one-block reveal used by bomb-special Speed Booster terrain: a special block. What it shows is its installed
/// picture (#1557).
/// </summary>
internal static class SpeedBoosterBlockObjectDrawDefinitions
{
    /// <summary>The Speed Booster block's own draw; its crumble frames are the single shot-block breakup draws.</summary>
    internal enum Draw
    {
        /// <summary>Reveals the Speed Booster block when it is bombed.</summary>
        BombReveal,
    }

    /// <summary>The draw that reveals a bombed Speed Booster block.</summary>
    internal static RoomObjectFrameDraw<Draw> BombRevealDraw { get; } = RoomObjectDraw.Of(Draw.BombReveal);

    /// <summary>How the revealed Speed Booster block collides: the special-block parent.</summary>
    internal const RoomCollisionType BombRevealCollision = RoomCollisionType.SpecialBlock;
}
