namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The routine Dragon's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum DragonEnemyFunction : ushort
{
    /// <summary><c>$A2:E654</c>: waits below the lava, then selects facing and begins the 48-update rise.</summary>
    WaitToRise = 1,

    /// <summary><c>$A2:E6AD</c>: moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising,

    /// <summary><c>$A2:E6F1</c>: waits for each attack animation and emits the three-fireball volley.</summary>
    Attacking,

    /// <summary><c>$A2:E734</c>: holds the exposed idle pose for 96 updates before sinking.</summary>
    WaitToSink,

    /// <summary><c>$A2:E749</c>: moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking,

    /// <summary><c>$A2:E781</c>: no-op function used by the cosmetic wing's separate physical enemy record.</summary>
    WingNoOp,
}
