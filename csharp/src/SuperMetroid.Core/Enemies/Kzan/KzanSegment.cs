namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed animation (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="KzanAnimations"/>.
/// </summary>
internal enum KzanSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
}
