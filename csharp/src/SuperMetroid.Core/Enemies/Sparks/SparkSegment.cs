namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="SparkAnimations"/>.
/// </summary>
internal enum SparkSegment
{
    /// <summary>The flicker on animation.</summary>
    FlickerOn,
    /// <summary>The active animation.</summary>
    Active,
    /// <summary>The flicker out animation.</summary>
    FlickerOut,
    /// <summary>The emitter animation.</summary>
    Emitter,
}
