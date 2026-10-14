namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>
/// The Wrecked Ship Spark's flicker, active and emitter animations as typed programs (#641), one per bank-$A8 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="SparkAnimations"/>.
/// </summary>
internal enum SparkProgram
{
    /// <summary>The native list at $A8:E5A7.</summary>
    FlickerOn,
    /// <summary>The native list at $A8:E5D1.</summary>
    Active,
    /// <summary>The native list at $A8:E5E5.</summary>
    FlickerOut,
    /// <summary>The native list at $A8:E609.</summary>
    Emitter,
}
