namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one of Mother Brain's falling tubes. Its function, floor, fall speed and smoke cadence are values the
/// cartridge keeps in the enemy's common record, so they are emptied with the record when the tube is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
internal sealed class MotherBrainFallingTubeState
{
    /// <summary>The bank-$A9 tube function; none once a kill has emptied the record.</summary>
    public MotherBrainTubeFunction? Function { get; internal set; }

    /// <summary>The room Y, in pixels, at which a non-main tube explodes.</summary>
    public int FloorY
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The signed 8.8 fall speed per update, increased by six 256ths each update.</summary>
    public SnesSignedEightEight FallSpeed { get; internal set; }

    /// <summary>The updates before the next smoke puff, counted down past zero.</summary>
    public NativeCountdown SmokeDelay { get; internal set; }

    /// <summary>The smoke puff's horizontal position, cycling through the four authored offsets.</summary>
    public int SmokePositionIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 3, value);
    }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        FloorY = 0;
        FallSpeed = default;
        SmokeDelay = NativeCountdown.Start(0);
        SmokePositionIndex = 0;
    }
}
