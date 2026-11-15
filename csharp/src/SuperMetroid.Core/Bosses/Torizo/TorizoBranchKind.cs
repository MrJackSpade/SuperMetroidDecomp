namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Which decision a <see cref="TorizoCue.Branch"/> cue is (#2028): a dodge or an attack that leaves the walk for good
/// when its test passes, continuing in its animation from the start.
/// </summary>
internal enum TorizoBranchKind
{
    /// <summary>Bomb Torizo jumps back from Samus close in front of it.</summary>
    BombJumpBackFromSamusInFront,
    /// <summary>Golden Torizo sits down on Samus morphed close behind it.</summary>
    GoldenSitOnSamusMorphedBehind,
    /// <summary>Golden Torizo leaps forward at Samus far in front of it.</summary>
    GoldenLeapAtDistantSamus,
    /// <summary>Golden Torizo jumps back from Samus close in front of it, or after enough steps without a decision.</summary>
    GoldenJumpBackFromSamusInFront,
}
