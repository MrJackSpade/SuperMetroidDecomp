namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The behaviour Wrecked Ship Spark's main AI runs. Its values are arbitrary.
/// </summary>
public enum SparkAction
{
    /// <summary>Returns immediately, leaving the continuously active animation in control.</summary>
    AlwaysActive = 1,

    /// <summary>Counts down the intangible interval before starting the flicker-on animation.</summary>
    IntermittentInactive,

    /// <summary>Counts down the tangible interval before starting the flicker-out animation and eight-update extension.</summary>
    IntermittentActive,

    /// <summary>Periodically spawns a falling Spark projectile and reloads the countdown.</summary>
    EmitFallingSparks,
}
