namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The behaviour Dragon's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum DragonAction
{
    /// <summary>Waits below the lava, then selects facing and begins the 48-update rise.</summary>
    WaitToRise,

    /// <summary>Moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising,

    /// <summary>Waits for each attack animation and emits the three-fireball volley.</summary>
    Attacking,

    /// <summary>Holds the exposed idle pose for 96 updates before sinking.</summary>
    WaitToSink,

    /// <summary>Moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking,

    /// <summary>No-op action used by the cosmetic wing, an enemy of its own beside the body.</summary>
    WingNoOp,
}
