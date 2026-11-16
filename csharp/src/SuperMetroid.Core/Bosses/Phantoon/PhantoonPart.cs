using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of Phantoon's four parts (#2252), his body, eye, tentacles or mouth: the encounter it is part of, the place it
/// occupies and the animation it plays. His body's setup makes all four with the encounter, as his room lists them
/// together; each part's setup installs its own as its place's species state, so the part's entity holds the encounter and
/// its playback from its setup.
/// </summary>
public sealed class PhantoonPart
{
    /// <summary>Makes <paramref name="phantoon"/>'s part occupying <paramref name="place"/>.</summary>
    /// <param name="phantoon">The encounter the part is part of.</param>
    /// <param name="place">The place the part occupies.</param>
    internal PhantoonPart(PhantoonEnemyState phantoon, RoomEnemyPlace place)
    {
        Phantoon = Ensure.NotNull(phantoon);
        Place = Ensure.NotNull(place);
    }

    /// <summary>The encounter the part is part of.</summary>
    internal PhantoonEnemyState Phantoon { get; }

    /// <summary>The place the part occupies.</summary>
    public RoomEnemyPlace Place { get; }

    /// <summary>
    /// The part's animation playback (#641, #1600): the invulnerable body's until the part's setup starts its own.
    /// </summary>
    internal AnimationPlayback<PhantoonPartSegment> Playback { get; set; } = Starting(PhantoonPartSegment.InvulnerableBody);

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
