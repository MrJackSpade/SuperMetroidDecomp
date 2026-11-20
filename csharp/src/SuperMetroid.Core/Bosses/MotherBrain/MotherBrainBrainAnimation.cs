using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's brain animation (#2317), separate from her head's place, which shows her brain's hit area: the shared
/// animation playback on its own clock, each beat showing for the updates it states, stepped at her brain's draw as her
/// brain's cadence has it (<see cref="MotherBrainBrainCadence"/>).
/// </summary>
public sealed class MotherBrainBrainAnimation
{
    /// <summary>The brain's playing animation, where it stands and the picture it shows, or null while her brain is not live and not drawn.</summary>
    internal MotherBrainBrainPlayback? Playback { get; set; }

    /// <summary>The brain animation's clock: how long the picture shown holds before the animation steps on.</summary>
    internal EnemyAnimationClock Clock { get; } = new();

    /// <summary>The picture the brain shows, or null while her brain is not live and not drawn.</summary>
    public EnemyFrameId? Shown => Playback?.Shown;

    /// <summary>Starts her head's own animation <paramref name="segment"/>, whose cues act on her encounter.</summary>
    internal void Start(MotherBrainHeadSegment segment) => Playback = MotherBrainBrainPlayback.StartingHead(segment, Clock);

    /// <summary>Starts <paramref name="lateFight"/>'s volley at Samus at <paramref name="segment"/>, whose own cues act on that late fight.</summary>
    internal void Start(MotherBrainLateFightState lateFight, MotherBrainLateFightHeadSegment segment) =>
        Playback = MotherBrainBrainPlayback.StartingLateFight(lateFight, segment, Clock);

    /// <summary>Starts her volley at the arrived <paramref name="baby"/> at <paramref name="segment"/>, whose own cues act on the Baby.</summary>
    internal void Start(BabyMetroidCutsceneState baby, MotherBrainBabyAttackSegment segment) =>
        Playback = MotherBrainBrainPlayback.StartingBabyAttack(baby, segment, Clock);

    /// <summary>
    /// Runs the brain animation at her brain's draw and returns the picture it shows, or null while her brain is not live:
    /// the picture shown holds for its updates, then the animation plays on to its next picture, carrying out each cue it
    /// passes in <paramref name="cues"/>. With time frozen it only reads the picture shown.
    /// </summary>
    internal EnemyFrameId? AdvanceAtDraw(bool timeIsFrozen, IMotherBrainBrainCues cues)
    {
        if (Playback is not { } playback)
            return null;
        if (!MotherBrainBrainCadence.StepsAtDraw(timeIsFrozen) || !Clock.Expire())
            return playback.Shown;
        MotherBrainBrainPlayback stepped = playback.Step(Clock, cues);
        Playback = stepped;
        return stepped.Shown;
    }
}
