using SuperMetroid.Core.Animation;
using S = SuperMetroid.Core.Rooms.RoomObjects.SamusEaterSegment;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One Brinstar plant's animations (#2480), from its authored chew (<see cref="SamusEaterObjectDefinition"/>), each image
/// shown for its own updates: its chew, played its plant's number of times, its fangs out as it releases Samus, and its
/// idle image.
/// </summary>
internal sealed class SamusEaterAnimations
{
    /// <summary>Builds <paramref name="plant"/>'s animations.</summary>
    internal SamusEaterAnimations(SamusEaterObjectDefinition plant)
    {
        Ensure.NotNull(plant);
        Segments = new(new Dictionary<SamusEaterSegment, AnimationBeat<SamusEaterChewFrame, NoAnimationCue>[]>
        {
            [S.Chew] = [.. plant.Chew.Select(Show)],
            [S.Released] = [Show(new(plant.Released, SamusEaterObjectDefinitions.ReleaseDuration, SamusEaterBite.None))],
            [S.Idle] = [Show(new(plant.Idle, 1, SamusEaterBite.None))],
        });
        Chewing = new([Segments.Repeated(S.Chew, plant.Chews)], AnimationEnd.Stops);
        Released = new([Segments.Whole(S.Released)], AnimationEnd.Stops);
        Idle = new([Segments.Whole(S.Idle)], AnimationEnd.Stops);
    }

    /// <summary>The plant's runs of images.</summary>
    internal AnimationSegments<SamusEaterSegment, SamusEaterChewFrame, NoAnimationCue> Segments { get; }

    /// <summary>The plant chewing the held Samus, its chew its number of times.</summary>
    internal Animation<SamusEaterSegment> Chewing { get; }

    /// <summary>The plant's fangs out as it releases Samus.</summary>
    internal Animation<SamusEaterSegment> Released { get; }

    /// <summary>The idle plant for its last update.</summary>
    internal Animation<SamusEaterSegment> Idle { get; }

    /// <summary>Shows <paramref name="frame"/> for its own updates.</summary>
    private static AnimationBeat<SamusEaterChewFrame, NoAnimationCue> Show(SamusEaterChewFrame frame) =>
        new AnimationBeat<SamusEaterChewFrame, NoAnimationCue>.Frame(frame, frame.Updates);
}
