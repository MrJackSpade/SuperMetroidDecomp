namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's motion state: reversal, straightening and orbital direction. The values are arbitrary bits; the population
/// entry names the initial state.
/// </summary>
[Flags]
internal enum ShaktoolMotionFlags
{
    /// <summary>No motion state is set.</summary>
    None = 0,
    /// <summary>The seven-piece chain has already reversed at a wall once.</summary>
    ReversedOnce = 1,
    /// <summary>The chain is returning its angular offsets toward the straight configuration.</summary>
    Straightening = 2,
    /// <summary>Orbital angle increments turn clockwise while this bit is set.</summary>
    Clockwise = 4,
}
