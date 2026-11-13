namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's body's own state beside the phase every part runs: his head-animation timer, his growth's next ceiling
/// rock and the rising-rock timer of his rise through the floor.
/// </summary>
/// <remarks>
/// The game keeps the head timer in the value a lint keeps its X offset in and a nail its horizontal
/// whole pixels, and the next ceiling rock and the rising-rock timer in the value of his phase timer (#1476).
/// The rising-rock phases load their timer before reading it, and the ceiling rocks start from the zero the phase
/// timer leaves when it expires into growth, so each is held on its own.
/// </remarks>
public sealed class KraidBodyState : KraidPartState
{
    /// <summary>
    /// The body's next growth ceiling rock, one of <see cref="KraidCeilingRocks.Count"/> rocks; equal to the count once
    /// every rock has fallen.
    /// </summary>
    public int CeilingRock
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, KraidCeilingRocks.Count, value);
    }

    /// <summary>The updates left in the body's current rising-rock phase; a rock falls at each multiple of its cadence.</summary>
    public int RisingRockTimer { get; internal set; }

    /// <summary>The body's head-animation timer.</summary>
    public int HeadTimer { get; internal set; }
}
