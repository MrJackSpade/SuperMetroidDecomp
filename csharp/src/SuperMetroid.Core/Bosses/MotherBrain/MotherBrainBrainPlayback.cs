using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's live brain animation (#2317, #2554): where it stands and the picture it shows, in one of her brain's
/// catalogs, each carrying what its cues act on. Her head's own animations act on her encounter; her late fight's volley at
/// Samus carries that late fight; her volley at the Baby carries the arrived Baby. An animation that opens with a cue shows
/// the picture after that cue from its start, before its first draw plays the cue.
/// </summary>
internal abstract record MotherBrainBrainPlayback
{
    /// <summary>Closes the hierarchy to the catalogs declared here.</summary>
    /// <param name="shown">The picture the brain shows.</param>
    private MotherBrainBrainPlayback(EnemyFrameId shown)
    {
        Shown = shown;
    }

    /// <summary>The picture the brain shows.</summary>
    internal EnemyFrameId Shown { get; init; }

    /// <summary>
    /// Plays the animation on to its next picture, carrying out each cue it passes in <paramref name="cues"/>, shows that
    /// picture on <paramref name="clock"/> for the updates it states or for good, and returns where the animation then
    /// stands with the picture it shows.
    /// </summary>
    internal abstract MotherBrainBrainPlayback Step(EnemyAnimationClock clock, IMotherBrainBrainCues cues);

    /// <summary>Her head's own animation <paramref name="segment"/> starts, shown on <paramref name="clock"/>.</summary>
    internal static MotherBrainBrainPlayback StartingHead(MotherBrainHeadSegment segment, EnemyAnimationClock clock) =>
        Starting(MotherBrainAnimations.HeadSegments, MotherBrainAnimations.HeadStartingWith(segment), clock,
            (animation, shown) => new Head(animation, shown));

    /// <summary><paramref name="lateFight"/>'s volley at Samus starts at <paramref name="segment"/>, shown on <paramref name="clock"/>.</summary>
    internal static MotherBrainBrainPlayback StartingLateFight(MotherBrainLateFightState lateFight, MotherBrainLateFightHeadSegment segment,
        EnemyAnimationClock clock) =>
        Starting(MotherBrainAnimations.LateFightSegments, MotherBrainAnimations.LateFightStartingWith(segment), clock,
            (animation, shown) => new LateFight(lateFight, animation, shown));

    /// <summary>Her volley at the arrived <paramref name="baby"/> starts at <paramref name="segment"/>, shown on <paramref name="clock"/>.</summary>
    internal static MotherBrainBrainPlayback StartingBabyAttack(BabyMetroidCutsceneState baby, MotherBrainBabyAttackSegment segment,
        EnemyAnimationClock clock) =>
        Starting(MotherBrainAnimations.BabyAttackSegments, MotherBrainAnimations.BabyAttackStartingWith(segment), clock,
            (animation, shown) => new BabyAttack(baby, animation, shown));

    /// <summary>
    /// Starts <paramref name="animation"/>: one that opens with a picture shows it at once, the start counting as its first
    /// update; one that opens with a cue shows the picture after the cue, and plays the cue and that picture at her brain's
    /// next draw.
    /// </summary>
    private static MotherBrainBrainPlayback Starting<TSegment, TCue>(AnimationSegments<TSegment, EnemyFrameId, TCue> segments,
        Animation<TSegment> animation, EnemyAnimationClock clock, Func<AnimationPlayback<TSegment>, EnemyFrameId, MotherBrainBrainPlayback> playing)
        where TSegment : struct
        where TCue : notnull
    {
        var start = new AnimationPlayback<TSegment>(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        EnemyFrameId first = segments.FirstFrameShown(animation);
        if (!MotherBrainBrainCadence.ShowsOpeningPictureAtStart(segments.OpensWithFrame(animation)))
        {
            clock.StepOnNextUpdate();
            return playing(start, first);
        }
        MotherBrainBrainPlayed<TSegment> opened = PlayOn(segments, start, first, clock,
            cue => throw new InvalidOperationException($"Mother Brain's brain animation raised {cue} before its opening picture."));
        return playing(opened.Playback, opened.Shown);
    }

    /// <summary>
    /// Plays <paramref name="playback"/> on to its next picture, carrying out each cue it passes in <paramref name="react"/>,
    /// and shows that picture on <paramref name="clock"/> for the updates it states or for good; an animation resting where
    /// it is keeps showing <paramref name="shown"/>.
    /// </summary>
    /// <returns>Where the animation then stands, and the picture it shows.</returns>
    private static MotherBrainBrainPlayed<TSegment> PlayOn<TSegment, TCue>(
        AnimationSegments<TSegment, EnemyFrameId, TCue> segments, AnimationPlayback<TSegment> playback, EnemyFrameId shown,
        EnemyAnimationClock clock, Func<TCue, AnimationContinuation<TSegment>?> react)
        where TSegment : struct
        where TCue : notnull
    {
        Animation<TSegment> animation = playback.Animation;
        AnimationPlace place = playback.Place;
        AnimationShown<TSegment, EnemyFrameId> next = segments.PlayToNextFrame(ref animation, ref place, react);
        var advanced = new AnimationPlayback<TSegment>(animation, place);
        switch (next)
        {
            case AnimationShown<TSegment, EnemyFrameId>.Frame frame:
                clock.Show(frame.Duration);
                return new(advanced, frame.Shown);
            case AnimationShown<TSegment, EnemyFrameId>.HeldFrame held:
                clock.ShowForGood();
                return new(advanced, held.Shown);
            case AnimationShown<TSegment, EnemyFrameId>.Held:
                clock.Stop();
                return new(advanced, shown);
            default:
                throw new InvalidOperationException($"Mother Brain's brain animations have no {next} beat.");
        }
    }

    /// <summary>The play of an animation's continuation at <paramref name="segment"/>'s animation, from its start; none when there is none.</summary>
    private static AnimationContinuation<TSegment>? ContinuingAt<TSegment>(TSegment? segment, Func<TSegment, Animation<TSegment>> startingWith)
        where TSegment : struct =>
        segment is { } continued ? new AnimationContinuation<TSegment>(startingWith(continued), AnimationPlace.StartOfAnimation) : null;

    /// <summary>Her head's own animation, whose cues act on her encounter.</summary>
    /// <param name="Animation">The playing animation and where it stands.</param>
    /// <param name="Shown">The picture the brain shows.</param>
    internal sealed record Head(AnimationPlayback<MotherBrainHeadSegment> Animation, EnemyFrameId Shown) : MotherBrainBrainPlayback(Shown)
    {
        /// <inheritdoc/>
        internal override MotherBrainBrainPlayback Step(EnemyAnimationClock clock, IMotherBrainBrainCues cues)
        {
            MotherBrainBrainPlayed<MotherBrainHeadSegment> played = PlayOn(MotherBrainAnimations.HeadSegments, Animation, Shown,
                clock, cue => ContinuingAt(cues.RunHeadCue(cue), MotherBrainAnimations.HeadStartingWith));
            return this with { Animation = played.Playback, Shown = played.Shown };
        }
    }

    /// <summary>Her late fight's volley at Samus, whose own cues act on <paramref name="Fight"/>.</summary>
    /// <param name="Fight">The late fight the volley belongs to.</param>
    /// <param name="Animation">The playing animation and where it stands.</param>
    /// <param name="Shown">The picture the brain shows.</param>
    internal sealed record LateFight(MotherBrainLateFightState Fight, AnimationPlayback<MotherBrainLateFightHeadSegment> Animation, EnemyFrameId Shown)
        : MotherBrainBrainPlayback(Shown)
    {
        /// <inheritdoc/>
        internal override MotherBrainBrainPlayback Step(EnemyAnimationClock clock, IMotherBrainBrainCues cues)
        {
            MotherBrainBrainPlayed<MotherBrainLateFightHeadSegment> played = PlayOn(MotherBrainAnimations.LateFightSegments,
                Animation, Shown, clock, cue => ContinuingAt(cues.RunLateFightCue(Fight, cue), MotherBrainAnimations.LateFightStartingWith));
            return this with { Animation = played.Playback, Shown = played.Shown };
        }
    }

    /// <summary>Her volley at the arrived Baby, whose own cues act on <paramref name="Baby"/>.</summary>
    /// <param name="Baby">The Baby the volley is fired at.</param>
    /// <param name="Animation">The playing animation and where it stands.</param>
    /// <param name="Shown">The picture the brain shows.</param>
    internal sealed record BabyAttack(BabyMetroidCutsceneState Baby, AnimationPlayback<MotherBrainBabyAttackSegment> Animation, EnemyFrameId Shown)
        : MotherBrainBrainPlayback(Shown)
    {
        /// <inheritdoc/>
        internal override MotherBrainBrainPlayback Step(EnemyAnimationClock clock, IMotherBrainBrainCues cues)
        {
            MotherBrainBrainPlayed<MotherBrainBabyAttackSegment> played = PlayOn(MotherBrainAnimations.BabyAttackSegments,
                Animation, Shown, clock, cue => ContinuingAt(cues.RunBabyAttackCue(Baby, cue), MotherBrainAnimations.BabyAttackStartingWith));
            return this with { Animation = played.Playback, Shown = played.Shown };
        }
    }
}
