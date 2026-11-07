using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The one-block reveal used by bomb-special Speed Booster terrain. Its special-block collision stays even when its
/// appearance changes.
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

    /// <summary>The special-block parent showing the Speed Booster block.</summary>
    internal static RoomTerrainBlock BombRevealTerrain { get; } = new(RoomObjectVisualBlockDefinitions.SpeedBoosterParent, RoomCollisionType.SpecialBlock);
}
