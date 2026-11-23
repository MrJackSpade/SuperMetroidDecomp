using SuperMetroid.Core.Animation;
using L = SuperMetroid.Core.Rooms.Doors.DoorCapLook;
using S = SuperMetroid.Core.Rooms.Doors.DoorCapSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The door caps' sounds and authored animations (#1554, #2480): every run of pictures and sounds a blue, grey or coloured
/// cap plays, the closing every coloured and grey cap plays behind Samus, and the blue cap's opening, closing and closed
/// image. A hit's flash is one shared run played over (<see cref="FlashesPerHit"/>); the red cap sounds before it.
/// </summary>
internal static partial class DoorCapSequenceDefinitions
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

    /// <summary>The cleared opening every cap leaves once it has opened, which the grey caps draw.</summary>
    private static readonly DoorCapShown ClearedOpening = new(DoorCapLook.Cleared, DoorCapShownPaint.Grey);

    /// <summary>The blue cap a hit flashes: the blue paint's closed look, whatever the cap's own paint.</summary>
    private static readonly DoorCapShown HitFlash = new(DoorCapLook.Closed, DoorCapShownPaint.Blue);

    /// <summary>Every run's pictures, each for its updates, and the sounds requested as the picture after each draws.</summary>
    internal static AnimationSegments<DoorCapSegment, DoorCapShown, SoundEffectId> Segments { get; } = new(
        new Dictionary<DoorCapSegment, AnimationBeat<DoorCapShown, SoundEffectId>[]>
        {
            [S.Closed] = [Show(L.Closed, 1)],
            [S.YellowDownClosed] = [Show(L.Closed, 3)],
            [S.Flash] = [Show(HitFlash, 3), Show(L.Closed, 4)],
            [S.RedHitSounding] = [Sound(RedHitSound)],
            [S.Opening] = Opening(imageUpdates: 6, clearedUpdates: 1),
            [S.YellowLeftOpening] = Opening(imageUpdates: 4, clearedUpdates: 92),
            [S.GreyOpening] = Opening(imageUpdates: 4, clearedUpdates: 1),
            [S.BlueOpening] = Opening(imageUpdates: 6, clearedUpdates: 94),
            [S.Closing] = Closing(clearedUpdates: 2, imagesBeforeSound: 2),
            [S.BombTorizoHeldOpen] = [Show(ClearedOpening, 2)],
            [S.BombTorizoClosing] = Closing(clearedUpdates: 40, imagesBeforeSound: 1),
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

    /// <summary>
    /// A cap opening: its sound, then its ajar, half-open and nearly open images for <paramref name="imageUpdates"/> each and
    /// its cleared image for <paramref name="clearedUpdates"/>.
    /// </summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId>[] Opening(int imageUpdates, int clearedUpdates) =>
        [Sound(OpenSound), Show(L.Ajar, imageUpdates), Show(L.HalfOpen, imageUpdates), Show(L.NearlyOpen, imageUpdates), Show(ClearedOpening, clearedUpdates)];

    /// <summary>
    /// A cap closing: its cleared image for <paramref name="clearedUpdates"/>, then its nearly open, half-open and ajar images
    /// for two updates each, its sound after the first <paramref name="imagesBeforeSound"/> of them.
    /// </summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId>[] Closing(int clearedUpdates, int imagesBeforeSound) =>
        [.. ClosingImages(clearedUpdates)[..imagesBeforeSound], Sound(CloseSound), .. ClosingImages(clearedUpdates)[imagesBeforeSound..]];

    /// <summary>A cap's closing images: its cleared image for <paramref name="clearedUpdates"/>, then its nearly open, half-open and ajar images for two updates each.</summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId>[] ClosingImages(int clearedUpdates) =>
        [Show(ClearedOpening, clearedUpdates), Show(L.NearlyOpen, 2), Show(L.HalfOpen, 2), Show(L.Ajar, 2)];

    /// <summary>Draws <paramref name="frame"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId> Show(DoorCapShown shown, int updates) =>
        new AnimationBeat<DoorCapShown, SoundEffectId>.Frame(shown, updates);

    /// <summary>A beat showing <paramref name="look"/> in the cap's own paint for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId> Show(DoorCapLook look, int updates) =>
        Show(new DoorCapShown(look, DoorCapShownPaint.Own), updates);

    /// <summary>Requests <paramref name="sound"/> as the picture after it draws.</summary>
    private static AnimationBeat<DoorCapShown, SoundEffectId> Sound(SoundEffectId sound) =>
        new AnimationBeat<DoorCapShown, SoundEffectId>.Event(sound);
}
