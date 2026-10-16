namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The Kzan words kept per physical slot for the room rather than per occupant, like the native per-slot
/// RAM they model: words an initializer does not write keep the previous occupant's values until room load.
/// </summary>
internal sealed class KzanExtensionWords
{
    /// <summary>countdown reset copied from init1's low byte.</summary>
    internal ushort FallWaitTimerResetValue { get; set; }

    /// <summary>Whole Y before each rise/fall step, used to carry Samus by the accepted delta.</summary>
    internal ushort PreviousYPosition { get; set; }

    /// <summary>index into the linear enemy speed table, capped at its final NTSC entry.</summary>
    internal ushort FallingYSpeedTableIndex { get; set; }

    /// <summary>hold between landing and rising.</summary>
    internal ushort RiseWaitTimer { get; set; }
}
