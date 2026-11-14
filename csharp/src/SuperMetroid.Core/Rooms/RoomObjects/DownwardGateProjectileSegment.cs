using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The downward shot gate's animations (#641, #1826): a moving gate moves down a block at a time and stops closed; a closed
/// gate, woken, rises a block at a time and vanishes. Each block the gate travels plays the animation of its next place,
/// which shows the gate's picture there; see <see cref="DownwardGateProjectileAnimations"/>.
/// </summary>
internal enum DownwardGateProjectileSegment
{
    /// <summary>A moving gate as it spawns: it starts moving down and shows its first picture.</summary>
    Moving,
    /// <summary>The moving gate one block down.</summary>
    MovedOneBlock,
    /// <summary>The moving gate two blocks down.</summary>
    MovedTwoBlocks,
    /// <summary>The moving gate three blocks down.</summary>
    MovedThreeBlocks,
    /// <summary>The moving gate four blocks down: it stops and shows itself closed, as <see cref="Closed"/> does.</summary>
    Stopping,
    /// <summary>A closed gate: it shows itself closed, ready to rise once woken.</summary>
    Closed,
    /// <summary>The woken closed gate: it starts rising and shows its first rising picture.</summary>
    Rising,
    /// <summary>The rising gate one block up.</summary>
    RisenOneBlock,
    /// <summary>The rising gate two blocks up.</summary>
    RisenTwoBlocks,
    /// <summary>The rising gate three blocks up.</summary>
    RisenThreeBlocks,
    /// <summary>The rising gate four blocks up: it vanishes.</summary>
    Vanishing,
}
