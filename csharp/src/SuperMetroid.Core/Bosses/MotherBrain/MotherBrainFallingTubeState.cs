using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The state of one of Mother Brain's falling tubes. Its setup starts its animation every time, so nothing it holds is
/// inherited from an earlier occupant of its slot (#1476).
/// </summary>
internal sealed class MotherBrainFallingTubeState
{
    /// <summary>The falling tube's animation and where it stands (#641, #1600).</summary>
    public AnimationPlayback<MotherBrainTubePiece> Playback { get; internal set; } = MotherBrainTubeAnimations.Starting(default);

    /// <summary>The tube function.</summary>
    public MotherBrainTubeFunction? Function { get; internal set; }

    /// <summary>The room Y, in pixels, at which a non-main tube explodes.</summary>
    public int FloorY { get; internal set; }

    /// <summary>The signed 8.8 fall speed per update, increased by six 256ths each update.</summary>
    public SignedEightEight FallSpeed { get; internal set; }

    /// <summary>The updates before the next smoke puff, counted down past zero.</summary>
    public int SmokeDelay { get; internal set; }

    /// <summary>The smoke puff's horizontal position, cycling through the four authored offsets.</summary>
    public int SmokePositionIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, 3, value);
    }

    /// <summary>The main tube's wait before it falls, counted down past zero.</summary>
    public int FallDelay { get; internal set; }
}
