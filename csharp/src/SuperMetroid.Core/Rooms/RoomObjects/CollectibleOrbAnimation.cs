using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The Chozo orb's animation (#1825), played by the shared animation playback until a shot breaks the orb: its shell
/// shows dim for twenty updates, glowing for ten, bright for twenty and glowing again for ten, then starts again.
/// </summary>
internal static class CollectibleOrbAnimation
{
    /// <summary>The orb's shell pictures and how long each shows.</summary>
    internal static AnimationSegments<CollectibleOrbSegment, CollectibleDraw, NoAnimationCue> Segments { get; } = new(
        new Dictionary<CollectibleOrbSegment, AnimationBeat<CollectibleDraw, NoAnimationCue>[]>
        {
            [CollectibleOrbSegment.Pulsing] =
            [
                Show(CollectibleDraw.ChozoOrbDim, 20),
                Show(CollectibleDraw.ChozoOrbGlowing, 10),
                Show(CollectibleDraw.ChozoOrbBright, 20),
                Show(CollectibleDraw.ChozoOrbGlowing, 10),
            ],
        });

    /// <summary>The orb's pulse, again and again.</summary>
    internal static Animation<CollectibleOrbSegment> Pulsing { get; } =
        new([new(CollectibleOrbSegment.Pulsing, 0, Segments.Beats(CollectibleOrbSegment.Pulsing).Count, 1)], AnimationEnd.LoopsFrom(0));

    /// <summary>Shows <paramref name="draw"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<CollectibleDraw, NoAnimationCue> Show(CollectibleDraw draw, int duration) =>
        new AnimationBeat<CollectibleDraw, NoAnimationCue>.Frame(draw, duration);
}
