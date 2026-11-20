using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The stations' animations (#1554, #1994), played by the shared animation playback: each animation's images in order,
/// with the updates each shows. The map and resource displays glow dim, lit and bright, more quickly once the map is
/// held, again and again; the save station's idle image shows again and again, and the accepted save flashes its pod's lights on
/// and off for its authored loops.
/// </summary>
internal static class StationAnimationDefinitions
{
    /// <summary>Each animation's images and how long each shows.</summary>
    internal static AnimationSegments<StationAnimation, StationDraw, NoAnimationCue> Segments { get; } = new(
        new Dictionary<StationAnimation, AnimationBeat<StationDraw, NoAnimationCue>[]>
        {
            [StationAnimation.MapIdle] = [Show(StationDraw.MapDim, 6), Show(StationDraw.MapLit, 6), Show(StationDraw.MapBright, 6)],
            [StationAnimation.MapAcquired] = [Show(StationDraw.MapDim, 2), Show(StationDraw.MapLit, 2), Show(StationDraw.MapBright, 2)],
            [StationAnimation.Energy] = [Show(StationDraw.EnergyDim, 6), Show(StationDraw.EnergyLit, 6), Show(StationDraw.EnergyBright, 6)],
            [StationAnimation.Missile] = [Show(StationDraw.MissileDim, 6), Show(StationDraw.MissileLit, 6), Show(StationDraw.MissileBright, 6)],
            [StationAnimation.SaveIdle] = [Show(StationDraw.SaveIdle, 1)],
            [StationAnimation.SaveFlash] = [Show(StationDraw.SaveLit, 4), Show(StationDraw.SaveDark, 4)],
        });

    /// <summary>
    /// How each animation plays: a display's glow and the idle pod again and again; the save's flash its authored loops
    /// (<see cref="SaveStationAnimationDefinitions.SaveAnimationLoops"/>), after which it is over.
    /// </summary>
    internal static IReadOnlyDictionary<StationAnimation, Animation<StationAnimation>> Animations { get; } =
        new Dictionary<StationAnimation, Animation<StationAnimation>>
        {
            [StationAnimation.MapIdle] = Looping(StationAnimation.MapIdle),
            [StationAnimation.MapAcquired] = Looping(StationAnimation.MapAcquired),
            [StationAnimation.Energy] = Looping(StationAnimation.Energy),
            [StationAnimation.Missile] = Looping(StationAnimation.Missile),
            [StationAnimation.SaveIdle] = Looping(StationAnimation.SaveIdle),
            [StationAnimation.SaveFlash] = new(
                [Segments.Repeated(StationAnimation.SaveFlash, SaveStationAnimationDefinitions.SaveAnimationLoops)],
                AnimationEnd.Stops),
        };

    /// <summary>The whole of <paramref name="segment"/>, again and again.</summary>
    private static Animation<StationAnimation> Looping(StationAnimation segment) =>
        Animation<StationAnimation>.Looping([Segments.Whole(segment)]);

    /// <summary>Shows <paramref name="draw"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<StationDraw, NoAnimationCue> Show(StationDraw draw, int duration) =>
        new AnimationBeat<StationDraw, NoAnimationCue>.Frame(draw, duration);
}
