namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Bits in Shaktool's motion state controlling reversal, straightening, and orbital direction.</summary>
[Flags]
internal enum ShaktoolMotionFlags : ushort
{
    /// <summary>The seven-piece chain has already reversed at a wall once.</summary>
    ReversedOnce = 0x2000,
    /// <summary>The chain is returning its angular offsets toward the straight configuration.</summary>
    Straightening = 0x4000,
    /// <summary>Orbital angle increments turn clockwise while this bit is set.</summary>
    Clockwise = 0x8000,
}
