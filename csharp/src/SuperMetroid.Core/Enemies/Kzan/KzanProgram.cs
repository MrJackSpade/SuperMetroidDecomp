namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Kzan's idle pose as a typed program (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="KzanAnimations"/>.
/// </summary>
internal enum KzanProgram
{
    /// <summary>The idle animation.</summary>
    Idle,
}
