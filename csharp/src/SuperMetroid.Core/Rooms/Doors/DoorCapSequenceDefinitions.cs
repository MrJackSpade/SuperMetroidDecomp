using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Rooms.Doors.DoorCapFrame;
using S = SuperMetroid.Core.Rooms.Doors.DoorCapSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The door caps' sounds and authored animations (#1554, #2480): every run of pictures and sounds a blue, grey or coloured
/// cap plays, the closing every coloured and grey cap plays behind Samus, and the blue cap's opening, closing and closed
/// image. A hit's flash is one shared run played over (<see cref="FlashesPerHit"/>); the red cap sounds before it.
/// </summary>
internal static class DoorCapSequenceDefinitions
{
    /// <summary>The sound a cap makes as it starts to open.</summary>
    internal static readonly SoundEffectId OpenSound = RoomSound.DoorOpened;

    /// <summary>The sound a cap makes as it closes behind Samus.</summary>
    internal static readonly SoundEffectId CloseSound = RoomSound.DoorClosed;

    /// <summary>The sound each hit on a red cap makes as its flash starts.</summary>
    internal static readonly SoundEffectId RedHitSound = RoomSound.MissileDoorHit;

    /// <summary>How crowded the sound queue may be for a door cap's sound to join it.</summary>
    internal static readonly SoundRequestPriority SoundPriority = SoundRequestPriority.Normal;

    /// <summary>How many times a hit short of opening a cap flashes it.</summary>
    internal const int FlashesPerHit = 3;

    /// <summary>Every run's pictures, each for its updates, and the sounds requested as the picture after each draws.</summary>
    internal static AnimationSegments<DoorCapSegment, DoorCapFrame, SoundEffectId> Segments { get; } = new(
        new Dictionary<DoorCapSegment, AnimationBeat<DoorCapFrame, SoundEffectId>[]>
        {
            [S.Closed] = [Show(F.Closed, 1)],
            [S.YellowDownClosed] = [Show(F.Closed, 3)],
            [S.Flash] = [Show(F.BlueFlash, 3), Show(F.Closed, 4)],
            [S.RedHitSounding] = [Sound(RedHitSound)],
            [S.Opening] = [Sound(OpenSound), Show(F.Ajar, 6), Show(F.HalfOpen, 6), Show(F.NearlyOpen, 6), Show(F.Clear, 1)],
            [S.YellowLeftOpening] = [Sound(OpenSound), Show(F.Ajar, 4), Show(F.HalfOpen, 4), Show(F.NearlyOpen, 4), Show(F.Clear, 92)],
            [S.GreyOpening] = [Sound(OpenSound), Show(F.Ajar, 4), Show(F.HalfOpen, 4), Show(F.NearlyOpen, 4), Show(F.Clear, 1)],
            [S.BlueOpening] = [Sound(OpenSound), Show(F.Ajar, 6), Show(F.HalfOpen, 6), Show(F.NearlyOpen, 6), Show(F.Clear, 94)],
            [S.Closing] = [Show(F.Clear, 2), Show(F.NearlyOpen, 2), Sound(CloseSound), Show(F.HalfOpen, 2), Show(F.Ajar, 2)],
            [S.BombTorizoHeldOpen] = [Show(F.Clear, 2)],
            [S.BombTorizoClosing] = [Show(F.Clear, 40), Sound(CloseSound), Show(F.NearlyOpen, 2), Show(F.HalfOpen, 2), Show(F.Ajar, 2)],
        });

    /// <summary>A cap's closed image, for one update: a coloured cap waiting, a grey cap locked or a blue cap closed again.</summary>
    internal static Animation<DoorCapSegment> Closed { get; } = Once(S.Closed);

    /// <summary>A hit short of opening a cap: the shared flash, played over.</summary>
    internal static Animation<DoorCapSegment> Flash { get; } = new([Segments.Repeated(S.Flash, FlashesPerHit)], AnimationEnd.Stops);

    /// <summary>
    /// A coloured or grey cap closing behind Samus: the blue cap's closing (<see cref="BlueClosing"/>), then its closed image
    /// for one update.
    /// </summary>
    internal static Animation<DoorCapSegment> CapClosing { get; } = new([Segments.Whole(S.Closing), Segments.Whole(S.Closed)], AnimationEnd.Stops);

    /// <summary>A shot blue cap opening.</summary>
    internal static Animation<DoorCapSegment> BlueOpening { get; } = Once(S.BlueOpening);

    /// <summary>A blue cap closing behind Samus.</summary>
    internal static Animation<DoorCapSegment> BlueClosing { get; } = Once(S.Closing);

    /// <summary>A blue cap closed again: its closed image for one update.</summary>
    internal static Animation<DoorCapSegment> BlueClosed => Closed;

    /// <summary>The whole of <paramref name="segment"/>, once.</summary>
    internal static Animation<DoorCapSegment> Once(DoorCapSegment segment) => new([Segments.Whole(segment)], AnimationEnd.Stops);

    /// <summary>Draws <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<DoorCapFrame, SoundEffectId> Show(DoorCapFrame frame, int updates) =>
        new AnimationBeat<DoorCapFrame, SoundEffectId>.Frame(frame, updates);

    /// <summary>Requests <paramref name="sound"/> as the picture after it draws.</summary>
    private static AnimationBeat<DoorCapFrame, SoundEffectId> Sound(SoundEffectId sound) =>
        new AnimationBeat<DoorCapFrame, SoundEffectId>.Event(sound);
}
