namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="SparkAnimations"/>.
/// </summary>
internal enum SparkSegment
{
    /// <summary>
    /// Turning solid and flickering on, large crosses and slants blinking and scattering, then sparking small.
    /// </summary>
    FlickerOn,
    /// <summary>Sparking small, crossed and slanted with trails, repeating.</summary>
    Active,
    /// <summary>Flickering large and scattering until it blinks out and is no longer solid.</summary>
    FlickerOut,
    /// <summary>The emitter sparking large, crossed and slanted, scattering, repeating.</summary>
    Emitter,
}
