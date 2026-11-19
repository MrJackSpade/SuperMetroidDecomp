namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the enemies' terrain movers move (#2572): a body with a room position, which a move changes, and the half-extents
/// it collides with. An enemy's place is one; a probe of the terrain away from an enemy is another
/// (<see cref="EnemyTerrainProbe"/>), so a probe never moves the enemy it probes for.
/// </summary>
internal interface IEnemyTerrainBody
{
    /// <summary>The body's room position with its fraction of a pixel.</summary>
    FixedPosition<RoomPixels> Position { get; set; }

    /// <summary>The body's collision half-extents in pixels.</summary>
    Extent16<RoomPixels> Radius { get; }
}
