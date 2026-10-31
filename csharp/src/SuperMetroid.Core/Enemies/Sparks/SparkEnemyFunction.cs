namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The routine Wrecked Ship Spark's main AI runs. Its values are arbitrary; verification's native view names each
/// member's cartridge routine (#1412).
/// </summary>
public enum SparkEnemyFunction
{
    /// <summary><c>$A8:E694</c>: returns immediately, leaving the continuously active instruction list in control.</summary>
    AlwaysActive = 1,

    /// <summary><c>$A8:E695</c>: counts down the intangible interval before installing the flicker-on list.</summary>
    IntermittentInactive,

    /// <summary><c>$A8:E6B7</c>: counts down the tangible interval before installing the flicker-out list and eight-update extension.</summary>
    IntermittentActive,

    /// <summary><c>$A8:E6DC</c>: periodically spawns a falling Spark projectile and reloads the countdown.</summary>
    EmitFallingSparks,
}
