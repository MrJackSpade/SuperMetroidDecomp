namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The behaviour Wrecked Ship Spark's main AI runs. Its values are arbitrary.
/// </summary>
public enum SparkEnemyFunction
{
    /// <summary>Returns immediately, leaving the continuously active program in control.</summary>
    AlwaysActive = 1,

    /// <summary>Counts down the intangible interval before installing the flicker-on list.</summary>
    IntermittentInactive,

    /// <summary>Counts down the tangible interval before installing the flicker-out list and eight-update extension.</summary>
    IntermittentActive,

    /// <summary>Periodically spawns a falling Spark projectile and reloads the countdown.</summary>
    EmitFallingSparks,
}
