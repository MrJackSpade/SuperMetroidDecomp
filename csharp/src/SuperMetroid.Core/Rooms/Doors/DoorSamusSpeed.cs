namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How far a door transition pushes Samus on each step of the door-opening scroll, in 1/256-pixel units: a door's own
/// speed, or the game's default for the door's axis.
/// </summary>
public readonly record struct DoorSamusSpeed
{
    /// <summary>The default push through a vertical door: one and a half pixels per step.</summary>
    private const int VerticalDefaultSubpixels = 384;

    /// <summary>The default push through a horizontal door: 200/256 pixel per step.</summary>
    private const int HorizontalDefaultSubpixels = 200;

    /// <summary>The door's own speed, or <see langword="null"/> for the axis default.</summary>
    private readonly int? _subpixelsPerStep;

    /// <summary>Keeps <paramref name="subpixelsPerStep"/>.</summary>
    private DoorSamusSpeed(int? subpixelsPerStep) => _subpixelsPerStep = subpixelsPerStep;

    /// <summary>The game's default push for the door's axis; most doors use it.</summary>
    public static DoorSamusSpeed AxisDefault { get; } = new(null);

    /// <summary>No push: Samus keeps her position through the scroll, as elevator doors leave her.</summary>
    public static DoorSamusSpeed None { get; } = new(0);

    /// <summary>A door's own push of <paramref name="subpixels"/> 1/256-pixel units per step, 0 through 32767.</summary>
    public static DoorSamusSpeed SubpixelsPerStep(int subpixels) =>
        new(Ensure.BetweenInclusive(0, short.MaxValue, subpixels));

    /// <summary>The push in 1/256-pixel units per step through a door travelled toward <paramref name="direction"/>.</summary>
    public int StepSubpixels(DoorDirection direction) =>
        _subpixelsPerStep ?? (direction.IsVertical() ? VerticalDefaultSubpixels : HorizontalDefaultSubpixels);
}
