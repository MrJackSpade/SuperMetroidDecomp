using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.BlueDoorProgram, SuperMetroid.Core.Rooms.Doors.BlueDoorFrame, SuperMetroid.Core.Rooms.Doors.BlueDoorCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue door caps' PLM programs (#1296), in place of their bank-$84 instruction lists: a shot cap opens and
/// deletes; a cap closing behind Samus closes, restores its behaviour and deletes.
/// </summary>
internal static class BlueDoorPlmProgramDefinitions
{
    /// <summary>The library-3 door-opening sound.</summary>
    internal static readonly SoundEffectId OpenSound = Library3Sound.DoorOpened;
    /// <summary>The library-3 door-closing sound.</summary>
    internal static readonly SoundEffectId CloseSound = Library3Sound.DoorClosed;
    /// <summary>The door sounds' native queue cap.</summary>
    internal static readonly SoundQueueLimit SoundMaximumQueued = SoundQueueLimit.Max6;
    /// <summary>How long each opening image shows before the last.</summary>
    private const int OpeningFrameDuration = 6;
    /// <summary>How long the opened cap's cleared image shows before the PLM deletes.</summary>
    private const int OpenedDuration = 94;
    /// <summary>How long each closing image shows.</summary>
    private const int ClosingFrameDuration = 2;

    /// <summary>Every blue cap program: each facing opens, closes and restores its closed cap the same way.</summary>
    internal static AnimationProgramSet<BlueDoorProgram, BlueDoorFrame, BlueDoorCue> Programs { get; } =
        new(new Dictionary<BlueDoorProgram, Step[]>
    {
        [BlueDoorProgram.LeftOpening] = OpeningSteps(),
        [BlueDoorProgram.LeftClosing] = ClosingSteps(BlueDoorProgram.LeftClosed),
        [BlueDoorProgram.LeftClosed] = ClosedSteps(),
        [BlueDoorProgram.RightOpening] = OpeningSteps(),
        [BlueDoorProgram.RightClosing] = ClosingSteps(BlueDoorProgram.RightClosed),
        [BlueDoorProgram.RightClosed] = ClosedSteps(),
        [BlueDoorProgram.UpOpening] = OpeningSteps(),
        [BlueDoorProgram.UpClosing] = ClosingSteps(BlueDoorProgram.UpClosed),
        [BlueDoorProgram.UpClosed] = ClosedSteps(),
        [BlueDoorProgram.DownOpening] = OpeningSteps(),
        [BlueDoorProgram.DownClosing] = ClosingSteps(BlueDoorProgram.DownClosed),
        [BlueDoorProgram.DownClosed] = ClosedSteps(),
    });

    /// <summary>The program a shot cap of each facing opens with.</summary>
    internal static IReadOnlyDictionary<ColoredDoorOrientation, BlueDoorProgram> OpeningPrograms { get; } =
        new Dictionary<ColoredDoorOrientation, BlueDoorProgram>
    {
        [ColoredDoorOrientation.Left] = BlueDoorProgram.LeftOpening,
        [ColoredDoorOrientation.Right] = BlueDoorProgram.RightOpening,
        [ColoredDoorOrientation.Up] = BlueDoorProgram.UpOpening,
        [ColoredDoorOrientation.Down] = BlueDoorProgram.DownOpening,
    };

    /// <summary>The program a cap of each facing closes behind Samus with.</summary>
    internal static IReadOnlyDictionary<ColoredDoorOrientation, BlueDoorProgram> ClosingPrograms { get; } =
        new Dictionary<ColoredDoorOrientation, BlueDoorProgram>
    {
        [ColoredDoorOrientation.Left] = BlueDoorProgram.LeftClosing,
        [ColoredDoorOrientation.Right] = BlueDoorProgram.RightClosing,
        [ColoredDoorOrientation.Up] = BlueDoorProgram.UpClosing,
        [ColoredDoorOrientation.Down] = BlueDoorProgram.DownClosing,
    };

    /// <summary>The program that restores a closed cap of each facing.</summary>
    internal static IReadOnlyDictionary<ColoredDoorOrientation, BlueDoorProgram> ClosedPrograms { get; } =
        new Dictionary<ColoredDoorOrientation, BlueDoorProgram>
    {
        [ColoredDoorOrientation.Left] = BlueDoorProgram.LeftClosed,
        [ColoredDoorOrientation.Right] = BlueDoorProgram.RightClosed,
        [ColoredDoorOrientation.Up] = BlueDoorProgram.UpClosed,
        [ColoredDoorOrientation.Down] = BlueDoorProgram.DownClosed,
    };

    /// <summary>The facing each blue program draws for.</summary>
    internal static IReadOnlyDictionary<BlueDoorProgram, ColoredDoorOrientation> Orientations { get; } =
        new Dictionary<BlueDoorProgram, ColoredDoorOrientation>
    {
        [BlueDoorProgram.LeftOpening] = ColoredDoorOrientation.Left,
        [BlueDoorProgram.LeftClosing] = ColoredDoorOrientation.Left,
        [BlueDoorProgram.LeftClosed] = ColoredDoorOrientation.Left,
        [BlueDoorProgram.RightOpening] = ColoredDoorOrientation.Right,
        [BlueDoorProgram.RightClosing] = ColoredDoorOrientation.Right,
        [BlueDoorProgram.RightClosed] = ColoredDoorOrientation.Right,
        [BlueDoorProgram.UpOpening] = ColoredDoorOrientation.Up,
        [BlueDoorProgram.UpClosing] = ColoredDoorOrientation.Up,
        [BlueDoorProgram.UpClosed] = ColoredDoorOrientation.Up,
        [BlueDoorProgram.DownOpening] = ColoredDoorOrientation.Down,
        [BlueDoorProgram.DownClosing] = ColoredDoorOrientation.Down,
        [BlueDoorProgram.DownClosed] = ColoredDoorOrientation.Down,
    };

    /// <summary>A shot cap opens: the sound, three opening images, then the cleared cap until the PLM deletes.</summary>
    private static Step[] OpeningSteps() =>
    [
        Step.Publish(BlueDoorCue.QueueOpenSound),
        Step.Show(BlueDoorFrame.Blue1, OpeningFrameDuration),
        Step.Show(BlueDoorFrame.Blue2, OpeningFrameDuration),
        Step.Show(BlueDoorFrame.Blue3, OpeningFrameDuration),
        Step.Show(BlueDoorFrame.Clear, OpenedDuration),
        Step.Stop,
    ];

    /// <summary>A cap closes behind Samus and runs into <paramref name="closed"/>, its facing's closed program.</summary>
    private static Step[] ClosingSteps(BlueDoorProgram closed) =>
    [
        Step.Show(BlueDoorFrame.Clear, ClosingFrameDuration),
        Step.Show(BlueDoorFrame.Blue3, ClosingFrameDuration),
        Step.Publish(BlueDoorCue.QueueCloseSound),
        Step.Show(BlueDoorFrame.Blue2, ClosingFrameDuration),
        Step.Show(BlueDoorFrame.Blue1, ClosingFrameDuration),
        Step.FallInto(closed),
    ];

    /// <summary>A closed cap restores its blue-door behaviour and shows its closed image.</summary>
    private static Step[] ClosedSteps() =>
    [
        Step.Publish(BlueDoorCue.RestoreBlueBehavior),
        Step.Show(BlueDoorFrame.Blue0, 1),
        Step.Stop,
    ];
}
