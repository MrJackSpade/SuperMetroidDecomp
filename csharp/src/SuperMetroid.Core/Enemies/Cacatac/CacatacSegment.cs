namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// Cacatac's upright and inverted idle and spike-attack animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="CacatacAnimations"/>.
/// </summary>
internal enum CacatacSegment
{
    /// <summary>
    /// Standing upright on its patrol, its spikes splaying, drawing in, gathering tall and stretching out, repeating.
    /// </summary>
    UpsideUpIdle,
    /// <summary>
    /// Standing upright, puffing up and firing five spikes in a fan above it with their sound, then idling again.
    /// </summary>
    UpsideUpAttack,
    /// <summary>The upside-down idle's start: its restore-patrol cue.</summary>
    UpsideDownIdle,
    /// <summary>Hanging upside down, its spikes splaying, drawing in, gathering tall and stretching out, eight updates each.</summary>
    UpsideDownIdlePoses,
    /// <summary>
    /// Hanging upside down, puffing up and firing five spikes in a fan below it with their sound, then idling again.
    /// </summary>
    UpsideDownAttack,
}
