using SuperMetroid.Core.Animation;
using L = SuperMetroid.Core.Frontend.Ending.EndingShootingStarLook;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-credits shooting stars' pictures through each flight (#1983, #2359), played by the shared animation player: the
/// hidden lead-in at the origin for <see cref="EndingShootingStarDefinitions.LeadInDuration"/> updates, then a still star's
/// faint speck for the rest of its flight, or an animated star's faint speck, bright speck and dim star, each for its
/// period, and then the full star for the rest of its flight.
/// </summary>
internal static class EndingShootingStarAnimations
{
    /// <summary>Each pace's flight, for every pace a star flies at.</summary>
    internal static AnimationSegments<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue> Segments { get; } = Create();

    /// <summary>The flight of a star that flies at <paramref name="pace"/>: its pictures from its lead-in, once.</summary>
    internal static Animation<EndingShootingStarPace> Flight(EndingShootingStarPace pace) => new([Segments.Whole(pace)], AnimationEnd.Holds);

    /// <summary>Writes each pace's flight.</summary>
    private static AnimationSegments<EndingShootingStarPace, EndingShootingStarLook, NoAnimationCue> Create()
    {
        EndingShootingStarPace[] paces = [.. EndingShootingStarDefinitions.Stars.ToArray().Select(star => new EndingShootingStarPace(star.Period)).Distinct()];
        return new(paces.ToDictionary(pace => pace, Pictures), paces);
    }

    /// <summary>The pictures of a flight at <paramref name="pace"/>.</summary>
    private static AnimationBeat<EndingShootingStarLook, NoAnimationCue>[] Pictures(EndingShootingStarPace pace) => pace.Period is { } period
        ? [Show(L.LeadIn, EndingShootingStarDefinitions.LeadInDuration), Show(L.FaintSpeck, period), Show(L.BrightSpeck, period),
            Show(L.DimStar, period), Hold(L.FullStar)]
        : [Show(L.LeadIn, EndingShootingStarDefinitions.LeadInDuration), Hold(L.FaintSpeck)];

    /// <summary>Shows <paramref name="look"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EndingShootingStarLook, NoAnimationCue> Show(L look, int duration) =>
        new AnimationBeat<EndingShootingStarLook, NoAnimationCue>.Frame(look, duration);

    /// <summary>Shows <paramref name="look"/> for the rest of the flight.</summary>
    private static AnimationBeat<EndingShootingStarLook, NoAnimationCue> Hold(L look) =>
        new AnimationBeat<EndingShootingStarLook, NoAnimationCue>.HeldFrame(look);
}
