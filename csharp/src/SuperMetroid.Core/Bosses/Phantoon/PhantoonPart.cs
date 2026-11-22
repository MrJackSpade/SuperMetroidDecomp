using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's four parts (#2252), his body, eye, tentacles or mouth: the encounter it is part of, the place it
/// occupies and the animation it plays. His body's setup makes all four with the encounter, as his room lists them
/// together; each part's setup installs its own as its place's species state, so the part's entity holds the encounter and
/// its playback from its setup.
/// </summary>
[ArchitectureReview("back-reference", "A part of Phantoon's encounter (#2252): the encounter makes and owns its four parts for the room, and each part's entity reads the encounter it joined at setup through its part, so the two share one lifetime.")]
public sealed partial class PhantoonPart
{
    /// <summary>
    /// Makes <paramref name="phantoon"/>'s part occupying <paramref name="place"/>, playing the animation its setup starts
    /// (#2319).
    /// </summary>
    /// <param name="phantoon">The encounter the part is part of.</param>
    /// <param name="place">The place the part occupies.</param>
    /// <param name="setupAnimation">The segment the animation the part's setup starts begins with.</param>
    internal PhantoonPart(PhantoonEnemyState phantoon, RoomEnemyPlace place, PhantoonPartSegment setupAnimation)
    {
        Phantoon = Ensure.NotNull(phantoon);
        Place = Ensure.NotNull(place);
        Playback = Starting(setupAnimation);
    }

    /// <summary>The encounter the part is part of.</summary>
    internal PhantoonEnemyState Phantoon { get; }

    /// <summary>The place the part occupies.</summary>
    public RoomEnemyPlace Place { get; }

    /// <summary>The part's animation playback (#641, #1600), begun with the animation its setup starts.</summary>
    internal AnimationPlayback<PhantoonPartSegment> Playback { get; set; }

    /// <summary>The animation starting at <paramref name="segment"/>, before its first play.</summary>
    internal static AnimationPlayback<PhantoonPartSegment> Starting(PhantoonPartSegment segment) =>
        new(PhantoonAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Restarts the part's animation at <paramref name="segment"/> with a one-update timer.</summary>
    internal void StartAnimation(PhantoonPartSegment segment)
    {
        Place.AnimationClock.StepOnNextUpdate();
        Playback = Starting(segment);
    }
}
