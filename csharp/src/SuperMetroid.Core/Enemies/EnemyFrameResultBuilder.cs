using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy pass's results as its producers write them. Reset at the start of each pass and at room load, so a paused
/// or frozen actor never replays a request, and read back until the next pass. The bosses write theirs to their own builder.
/// </summary>
internal sealed class EnemyFrameResultBuilder
{
    /// <summary>The elevator lifecycle event, if any.</summary>
    internal ElevatorFrameEvent Elevator { get; set; }

    /// <summary>The elevator called <c>ResetProjectileData</c>.</summary>
    internal bool ElevatorClearedProjectileData { get; set; }

    /// <summary>The gunship lifecycle event, if any.</summary>
    internal GunshipFrameEvent Gunship { get; set; }

    /// <summary>Clears every result before a pass or a room load.</summary>
    internal void Begin()
    {
        Elevator = ElevatorFrameEvent.None;
        ElevatorClearedProjectileData = false;
        Gunship = GunshipFrameEvent.None;
    }
}
