namespace SuperMetroid.Core.Frontend;

/// <summary>What the random generator does before a sequence's current step runs each update (#1420).</summary>
internal enum PhaseRandomStep
{
    /// <summary>The generator is reseeded at power-on, then advances once: the reset state runs after the first advance.</summary>
    Reseed,

    /// <summary>
    /// The room liquid's wave and the Tourian statues' descent advance first (a lava or acid wave swaps the halves of the
    /// random number), then the generator advances.
    /// </summary>
    LiquidWaveAndStatueDescentThenAdvance,

    /// <summary>
    /// The generator advances once before the phase runs, except on an update that finishes a step waiting out a frame of
    /// the display (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    Advance,

    /// <summary>The gameplay runtime advances it itself, or the state does not advance it.</summary>
    RuntimeOwned,
}
