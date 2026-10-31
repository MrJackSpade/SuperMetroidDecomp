using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one of Mother Brain's falling tubes. Its function, floor, fall speed and smoke cadence are values the
/// cartridge keeps in the enemy's common record, so they are emptied with the record when the tube is killed
/// (<see cref="ClearWithRecord"/>). Its setup starts its animation every time, so nothing it holds is inherited from an
/// earlier occupant of its slot (#1476).
/// </summary>
internal sealed class MotherBrainFallingTubeState
{
    /// <summary>Where the falling tube's program resumes (#641); a kill leaves it where it was.</summary>
    public AnimationProgramCursor<MotherBrainTubePiece> Cursor { get; internal set; }

    /// <summary>The bank-$A9 tube function; none once a kill has emptied the record.</summary>
    public MotherBrainTubeFunction? Function { get; internal set; }

    /// <summary>The room Y, in pixels, at which a non-main tube explodes.</summary>
    public int FloorY
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The signed 8.8 fall speed per update, increased by six 256ths each update.</summary>
    public SignedEightEight FallSpeed { get; internal set; }

    /// <summary>The updates before the next smoke puff, counted down past zero.</summary>
    public WrappingCountdown SmokeDelay { get; internal set; }

    /// <summary>The smoke puff's horizontal position, cycling through the four authored offsets.</summary>
    public int SmokePositionIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 3, value);
    }

    /// <summary>The main tube's wait before it falls (parameter two), counted down past zero.</summary>
    public WrappingCountdown FallDelay { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        FallDelay = WrappingCountdown.Start(0);
        Function = null;
        FloorY = 0;
        FallSpeed = default;
        SmokeDelay = WrappingCountdown.Start(0);
        SmokePositionIndex = 0;
    }
}
