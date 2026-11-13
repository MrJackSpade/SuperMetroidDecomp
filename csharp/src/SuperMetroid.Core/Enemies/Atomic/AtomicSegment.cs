namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// Atomic's four spinning animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="AtomicAnimations"/>.
/// </summary>
internal enum AtomicSegment
{
    /// <summary>The up right animation.</summary>
    UpRight,
    /// <summary>The up left animation.</summary>
    UpLeft,
    /// <summary>The down left animation.</summary>
    DownLeft,
    /// <summary>The down right animation.</summary>
    DownRight,
}
