namespace SuperMetroid.Core.Game;

/// <summary>
/// Native bank-$A2 function pointer stored in Dragon variable F. Exposing the cartridge
/// addresses keeps debugger state comparable with WRAM while giving callers readable states.
/// </summary>
public enum DragonEnemyFunction : ushort
{
    /// <summary><c>$A2:E654</c>: waits below the lava, then selects facing and begins the 48-update rise.</summary>
    WaitToRise = 0xe654,

    /// <summary><c>$A2:E6AD</c>: moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising = 0xe6ad,

    /// <summary><c>$A2:E6F1</c>: waits for each attack animation and emits the three-fireball volley.</summary>
    Attacking = 0xe6f1,

    /// <summary><c>$A2:E734</c>: holds the exposed idle pose for 96 updates before sinking.</summary>
    WaitToSink = 0xe734,

    /// <summary><c>$A2:E749</c>: moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking = 0xe749,

    /// <summary><c>$A2:E781</c>: no-op function used by the cosmetic wing's separate physical enemy record.</summary>
    WingNoOp = 0xe781,
}
