namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// Literal bank-$A8 function word stored in Wrecked Ship Spark variable B. The values are
/// deliberately the cartridge addresses: a debugger can compare the selected state with
/// <c>$0FAA,x</c> directly, and an accidental table overread remains visible as an unknown
/// native address instead of being coerced into a plausible host state.
/// </summary>
public enum SparkEnemyFunction : ushort
{
    /// <summary><c>$A8:E694</c>: returns immediately, leaving the continuously active instruction list in control.</summary>
    AlwaysActive = 0xe694,

    /// <summary><c>$A8:E695</c>: counts down the intangible interval before installing the flicker-on list.</summary>
    IntermittentInactive = 0xe695,

    /// <summary><c>$A8:E6B7</c>: counts down the tangible interval before installing the flicker-out list and eight-update extension.</summary>
    IntermittentActive = 0xe6b7,

    /// <summary><c>$A8:E6DC</c>: periodically spawns a falling Spark projectile and reloads the countdown.</summary>
    EmitFallingSparks = 0xe6dc,
}
