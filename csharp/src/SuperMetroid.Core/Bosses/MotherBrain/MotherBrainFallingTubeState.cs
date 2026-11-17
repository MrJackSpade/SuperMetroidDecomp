using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one of Mother Brain's falling tubes. Its setup makes it and begins its animation every time, so nothing
/// it holds is inherited from an earlier occupant of its place (#1476, #2319).
/// </summary>
internal sealed class MotherBrainFallingTubeState
{
    /// <summary>The falling tube's animation and where it stands (#641, #1600).</summary>
    public AnimationPlayback<MotherBrainTubePiece> Playback { get; internal set; }

    /// <summary>Starts <paramref name="piece"/>'s tube in <paramref name="action"/>, playing its own animation (#2319).</summary>
    /// <param name="piece">The tube piece, whose animation the tube plays.</param>
    /// <param name="action">What the tube does first.</param>
    internal MotherBrainFallingTubeState(MotherBrainTubePiece piece, MotherBrainTubeAction action)
    {
        Playback = MotherBrainTubeAnimations.Starting(piece);
        Action = action;
    }

    /// <summary>The tube action.</summary>
    public MotherBrainTubeAction Action { get; internal set; }

    /// <summary>The room Y, in pixels, at which a non-main tube explodes.</summary>
    public int FloorY { get; internal set; }

    /// <summary>The signed 1/256-pixel fall speed per update, increased by six 256ths each update.</summary>
    public SignedEightEight FallSpeed { get; internal set; }

    /// <summary>The wait before the next smoke puff (<see cref="BossCountdown"/>).</summary>
    public BossCountdown SmokeDelay { get; } = new();

    /// <summary>The smoke puff's horizontal position, cycling through the four authored offsets.</summary>
    public int SmokePositionIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 3, value);
    }

    /// <summary>The main tube's wait before it falls (<see cref="BossCountdown"/>).</summary>
    public BossCountdown FallDelay { get; } = new();
}
