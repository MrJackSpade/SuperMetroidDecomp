using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using S = SuperMetroid.Core.Rooms.Doors.DoorCapSegment;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The coloured caps' authored data (#1554): each colour's rules (#2546) and each colour and facing's animations
/// (<see cref="DoorCapSequenceDefinitions"/>). A Super Missile opens any cap it may hit at once, whatever hits the cap has
/// taken (#1907). Every cap plays the same shape; the yellow downward cap waits on its closed image longer, the yellow left
/// cap opens fast and lingers open, and the red cap sounds each hit.
/// </summary>
internal static class ColoredDoorDefinitions
{
    /// <summary>
    /// Each colour's rules: a yellow cap counts a Power Bomb's hit and opens on it, a green cap counts a Super Missile's and
    /// opens on it, and a red cap counts a missile's or a Super Missile's and opens on its fifth hit.
    /// </summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, ColoredDoorColorDefinition> Colors { get; } =
        new Dictionary<ColoredDoorColor, ColoredDoorColorDefinition>
    {
        [ColoredDoorColor.Yellow] = new([SamusProjectileFamily.PowerBomb], 1, DoorCapPaint.OfColor(ColoredDoorColor.Yellow)),
        [ColoredDoorColor.Green] = new([SamusProjectileFamily.SuperMissile], 1, DoorCapPaint.OfColor(ColoredDoorColor.Green)),
        [ColoredDoorColor.Red] = new([SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile], 5,
            DoorCapPaint.OfColor(ColoredDoorColor.Red)),
    };

    /// <summary>The yellow downward cap's closed image as it starts to wait, for its segment's beat.</summary>
    private static readonly Animation<DoorCapSegment> YellowDownWaiting = DoorCapSequenceDefinitions.Once(S.YellowDownClosed);

    /// <summary>A hit on a red cap short of opening: its sound, then the shared flash (<see cref="DoorCapSequenceDefinitions.FlashesPerHit"/>).</summary>
    private static readonly Animation<DoorCapSegment> RedFlash = new(
        [
            DoorCapSequenceDefinitions.Segments.Whole(S.RedHitSounding),
            DoorCapSequenceDefinitions.Segments.Repeated(S.Flash, DoorCapSequenceDefinitions.FlashesPerHit),
        ],
        AnimationEnd.Stops);

    /// <summary>A cap opening, after which it leaves the room.</summary>
    private static readonly Animation<DoorCapSegment> Opening = DoorCapSequenceDefinitions.Once(S.Opening);

    /// <summary>The yellow left cap opening, after which it leaves the room.</summary>
    private static readonly Animation<DoorCapSegment> YellowLeftOpening = DoorCapSequenceDefinitions.Once(S.YellowLeftOpening);

    /// <summary>The flash each colour's cap plays when a hit falls short of opening it.</summary>
    private static readonly Dictionary<ColoredDoorColor, Animation<DoorCapSegment>> Flashes = new()
    {
        [ColoredDoorColor.Yellow] = DoorCapSequenceDefinitions.Flash,
        [ColoredDoorColor.Green] = DoorCapSequenceDefinitions.Flash,
        [ColoredDoorColor.Red] = RedFlash,
    };

    /// <summary>
    /// The caps whose animations differ from their colour's shared ones: the yellow left cap opens its own way and the
    /// yellow downward cap waits before it closes.
    /// </summary>
    private static readonly Dictionary<ColoredDoorVariant, ColoredDoorSequences> Exceptions = new()
    {
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = new(DoorCapSequenceDefinitions.Closed, DoorCapSequenceDefinitions.Flash, YellowLeftOpening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = new(YellowDownWaiting, DoorCapSequenceDefinitions.Flash, Opening),
    };

    /// <summary>
    /// Each colour and facing's animations: its colour's closed image, flash and opening, but where <see cref="Exceptions"/>
    /// says otherwise.
    /// </summary>
    internal static IReadOnlyDictionary<ColoredDoorVariant, ColoredDoorSequences> Sequences { get; } = Flashes
        .SelectMany(flash => Enum.GetValues<ColoredDoorOrientation>().Select(orientation => new ColoredDoorVariant(flash.Key, orientation)))
        .ToDictionary(variant => variant, variant => Exceptions.GetValueOrDefault(variant,
            new(DoorCapSequenceDefinitions.Closed, Flashes[variant.Color], Opening)));
}
