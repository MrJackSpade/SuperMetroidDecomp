using SuperMetroid.Core.Animation;
using E = SuperMetroid.Core.Samus.Artwork.GrappleEndpointFrame;
using R = SuperMetroid.Core.Samus.Artwork.GrappleRopePicture;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// The connected grapple beam's animations (#2388), played one beam draw at a time: the endpoint swells from a point to an
/// orb, opens into a ring and sparkles, over and over; the rope's travelling pattern steps every
/// <see cref="GrappleBeamDefinitions.Rendering.SegmentAnimationDelay"/> rope draws, the muzzle's segment showing the picture the animation stands on and each segment further along the rope
/// the one before its neighbour's.
/// </summary>
internal static partial class GrappleAnimations
{
    /// <summary>The endpoint's pictures, each with the beam draws it shows for.</summary>
    internal static AnimationSegments<GrappleEndpointSegment, E, NoAnimationCue> EndpointSegments { get; } = new(
        new Dictionary<GrappleEndpointSegment, AnimationBeat<E, NoAnimationCue>[]>
        {
            [GrappleEndpointSegment.Cycling] = [Show(E.Point, 6), Show(E.Orb, 6), Show(E.Ring, 6), Show(E.SparklingRing, 6)],
        });

    /// <summary>The endpoint's animation, its four pictures over and over.</summary>
    internal static Animation<GrappleEndpointSegment> Endpoint { get; } =
        Animation<GrappleEndpointSegment>.Looping([EndpointSegments.Whole(GrappleEndpointSegment.Cycling)]);

    /// <summary>The picture the rope's muzzle segment shows at each step of the pattern, each with the rope draws it shows for.</summary>
    internal static AnimationSegments<GrappleRopeSegment, R, NoAnimationCue> RopeSegments { get; } = new(
        new Dictionary<GrappleRopeSegment, AnimationBeat<R, NoAnimationCue>[]>
        {
            [GrappleRopeSegment.Travelling] =
            [
                Show(R.ThreeQuarters, GrappleBeamDefinitions.Rendering.SegmentAnimationDelay),
                Show(R.Start, GrappleBeamDefinitions.Rendering.SegmentAnimationDelay),
                Show(R.Quarter, GrappleBeamDefinitions.Rendering.SegmentAnimationDelay),
                Show(R.Half, GrappleBeamDefinitions.Rendering.SegmentAnimationDelay),
            ],
        });

    /// <summary>The rope's animation, its pattern travelling over and over.</summary>
    internal static Animation<GrappleRopeSegment> Rope { get; } =
        Animation<GrappleRopeSegment>.Looping([RopeSegments.Whole(GrappleRopeSegment.Travelling)]);

    /// <summary>
    /// The picture a rope segment shows after the segment nearer the muzzle shows each: the one before it in the pattern,
    /// the last before the first, so the pattern runs out along the rope as it steps on.
    /// </summary>
    internal static IReadOnlyDictionary<R, R> RopePictureBefore { get; } = new Dictionary<R, R>
    {
        [R.Start] = R.ThreeQuarters,
        [R.Quarter] = R.Start,
        [R.Half] = R.Quarter,
        [R.ThreeQuarters] = R.Half,
    };

    /// <summary>Shows <paramref name="picture"/> for <paramref name="draws"/> draws.</summary>
    private static AnimationBeat<T, NoAnimationCue> Show<T>(T picture, int draws)
        where T : struct => new AnimationBeat<T, NoAnimationCue>.Frame(picture, draws);
}
