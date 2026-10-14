namespace SuperMetroid.Core.Game;

/// <summary>
/// Selects one of Boyon's two ROM movement functions at <c>$A2:8718</c>. The values are
/// table indexes, not invented host states: zero dispatches falling and one dispatches
/// rising.
/// </summary>
public enum BoyonBounceMovement : ushort
{
    /// <summary>Native table index zero: advance down the speed curve until the stored spawn-height baseline is reached.</summary>
    Falling = 0,
    /// <summary>Native table index one: retreat up the speed curve until the wrapped index selects falling.</summary>
    Rising = 1,
}
