namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>
/// Selects one of Boyon's two authored movement functions. The values are
/// table indexes, not invented states: zero dispatches falling and one dispatches
/// rising.
/// </summary>
public enum BoyonBounceMovement
{
    /// <summary>Table index zero: advance down the speed curve until the stored spawn-height baseline is reached.</summary>
    Falling = 0,
    /// <summary>Table index one: retreat up the speed curve until the wrapped index selects falling.</summary>
    Rising = 1,
}
