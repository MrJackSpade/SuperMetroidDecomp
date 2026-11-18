using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's four parts (#2298), his body, eye, tail or arms: the encounter it is part of, the place it occupies and
/// the animation it plays. His body's setup makes all four with the encounter, as his room lists them together, each
/// playing the animation its setup starts; the eye, tail and arms install their own as their places' species state as
/// they set up, so each part's entity holds the encounter and its playback from its setup.
/// </summary>
[ArchitectureReview("back-reference", "A part of Draygon's encounter (#2298): the encounter makes and owns its four parts for the room, and each part's entity reads the encounter it joined at setup through its part, so the two share one lifetime.")]
public sealed class DraygonPart
{
    /// <summary>Makes <paramref name="draygon"/>'s part occupying <paramref name="place"/>, playing the animation from <paramref name="starting"/>.</summary>
    /// <param name="draygon">The encounter the part is part of.</param>
    /// <param name="place">The place the part occupies.</param>
    /// <param name="starting">The segment the animation its setup starts begins with.</param>
    internal DraygonPart(DraygonEnemyState draygon, RoomEnemyPlace place, DraygonSegment starting)
    {
        Draygon = Ensure.NotNull(draygon);
        Place = Ensure.NotNull(place);
        Playback = Starting(starting);
    }

    /// <summary>The encounter the part is part of.</summary>
    internal DraygonEnemyState Draygon { get; }

    /// <summary>The place the part occupies.</summary>
    public RoomEnemyPlace Place { get; }

    /// <summary>The part's playing animation and where it stands.</summary>
    internal AnimationPlayback<DraygonSegment> Playback { get; set; }

    /// <summary>The animation beginning at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<DraygonSegment> Starting(DraygonSegment segment) =>
        new(DraygonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Starts the animation beginning at <paramref name="segment"/>, before its first play; its timer is set separately.</summary>
    internal void SetAnimation(DraygonSegment segment) => Playback = Starting(segment);

    /// <summary>Restarts the part's animation at <paramref name="segment"/>, stepping it on its next update.</summary>
    internal void StartAnimation(DraygonSegment segment) => StartAnimation(DraygonAnimations.StartingWith(segment));

    /// <summary>Restarts the part's animation with <paramref name="animation"/>, before its first play, stepping it on its next update.</summary>
    internal void StartAnimation(Animation<DraygonSegment> animation)
    {
        Playback = new(Ensure.NotNull(animation), AnimationPlace.StartOfAnimation);
        Place.AnimationClock.StepOnNextUpdate();
    }
}
