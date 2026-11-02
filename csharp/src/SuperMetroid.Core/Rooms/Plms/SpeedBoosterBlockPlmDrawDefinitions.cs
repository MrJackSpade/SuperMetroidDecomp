using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The one-block reveal used by bomb-special Speed Booster terrain. Its special-block collision stays even when its
/// appearance changes.
/// </summary>
internal static class SpeedBoosterBlockPlmDrawDefinitions
{
    /// <summary>The Speed Booster block's own draw; its crumble frames are the single shot-block breakup draws.</summary>
    internal enum Draw
    {
        /// <summary>Reveals the Speed Booster block when it is bombed.</summary>
        BombReveal,
    }

    /// <summary>The draw that reveals a bombed Speed Booster block.</summary>
    internal static PlmFrameDraw<Draw> BombRevealDraw { get; } = PlmDraw.Of(Draw.BombReveal);

    /// <summary>The special-block parent showing the Speed Booster block.</summary>
    internal static RoomTerrainBlock BombRevealTerrain { get; } = new(RoomPlmVisualBlockDefinitions.SpeedBoosterParent, RoomCollisionType.SpecialBlock);
}
