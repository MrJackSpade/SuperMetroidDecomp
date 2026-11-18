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

    /// <summary>The fall speed per update, increased by the tube's gravity each update.</summary>
    public SignedEightEight FallSpeed { get; internal set; }

    /// <summary>The wait before the next smoke puff (<see cref="BossCountdown"/>).</summary>
    public BossCountdown SmokeDelay { get; } = new();

    /// <summary>
    /// The last smoke puff's offset among <see cref="MotherBrainTubeCollapseDefinitions.SmokeXOffsets"/>: each puff shows
    /// at the next, the first again after the last, so the first puff shows at the second.
    /// </summary>
    public ListStep<int> SmokePuff { get; internal set; } = ListStep<int>.FirstOf(MotherBrainTubeCollapseDefinitions.SmokeXOffsets);

    /// <summary>The main tube's wait before it falls (<see cref="BossCountdown"/>).</summary>
    public BossCountdown FallDelay { get; } = new();
}
