namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive attack distributions selected by the native Ridley AI.</summary>
public enum RidleyAttackSituation
{
    /// <summary>$A6:B38C table: choices 0..3 select the translated ground-attack side phase and 4..7 the swoop; used below 14400 health and by the default branch.</summary>
    BelowHalfHealth,
    /// <summary>$A6:B39C table: choices 0..3 select the swoop and 4..7 the ground-attack side phase; the reversed distribution is unreachable through the ordinary selector.</summary>
    AboveHalfHealth,
    /// <summary>$A6:B3AC table: grab-eligible Samus movement outside the pogo zone selects grab approach for choices 0..5 and the ground-attack side phase for 6..7.</summary>
    DamageBoosting,
    /// <summary>$A6:B3BC table: Samus at room Y >= 352 pixels with Ridley at least 14400 health selects the ground-attack side phase for every choice.</summary>
    PogoZone,
    /// <summary>$A6:B3CC native hover table: Samus's spin-jump movement takes priority and selects the translated pogo-setup entry for every choice.</summary>
    SpinJumping,
    /// <summary>$A6:B3DC table: zero health with Samus not spin jumping selects grab approach for every choice; the caller counts these death lunges.</summary>
    ZeroHealth,
}
