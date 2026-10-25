namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The Kzan words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class KzanExtensionWords
{
    /// <summary>Countdown reset copied from init1's low byte, in actor updates.</summary>
    internal int FallWaitTimerResetValue { get; set; }

    /// <summary>Position before each rise/fall step, whose Y carries Samus by the accepted delta.</summary>
    internal Position16<RoomPixels> PreviousPosition { get; set; }

    /// <summary>Byte offset into the linear enemy speed table, capped at its final NTSC entry.</summary>
    internal int FallingYSpeedTableIndex { get; set; }

    /// <summary>hold between landing and rising.</summary>
    internal WrappingCountdown RiseWaitTimer { get; set; }
}
