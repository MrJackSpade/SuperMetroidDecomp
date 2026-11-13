namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Mutually exclusive attack distributions selected by the Ridley AI.</summary>
public enum RidleyAttackSituation
{
    /// <summary>Choices 0..3 select the ground-attack side phase and 4..7 the swoop; used below 14400 health and by the default branch.</summary>
    BelowHalfHealth,
    /// <summary>Choices 0..3 select the swoop and 4..7 the ground-attack side phase; the reversed distribution is unreachable through the ordinary selector.</summary>
    AboveHalfHealth,
    /// <summary>Grab-eligible Samus movement outside the pogo zone selects grab approach for choices 0..5 and the ground-attack side phase for 6..7.</summary>
    DamageBoosting,
    /// <summary>Samus at room Y >= 352 pixels with Ridley at least 14400 health selects the ground-attack side phase for every choice.</summary>
    PogoZone,
    /// <summary>Samus's spin-jump movement takes priority and selects the pogo-setup entry for every choice.</summary>
    SpinJumping,
    /// <summary>Zero health with Samus not spin jumping selects grab approach for every choice; the caller counts these death lunges.</summary>
    ZeroHealth,
}
