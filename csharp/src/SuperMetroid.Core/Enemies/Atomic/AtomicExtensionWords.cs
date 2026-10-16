namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>
/// The Atomic words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class AtomicExtensionWords
{
    /// <summary>Positive 16.16 speed fraction.</summary>
    internal ushort SpeedFraction { get; set; }

    /// <summary>Positive 16.16 speed whole word.</summary>
    internal ushort SpeedWhole { get; set; }

    /// <summary>Native negative-speed fractions retained separately to preserve subpixel arithmetic.</summary>
    internal ushort NegativeSpeedFraction { get; set; }

    /// <summary>Native negative-speed whole words retained separately from positive speed.</summary>
    internal ushort NegativeSpeedWhole { get; set; }
}
