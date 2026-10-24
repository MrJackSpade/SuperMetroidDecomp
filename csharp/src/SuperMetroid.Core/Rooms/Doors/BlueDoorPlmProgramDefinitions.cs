using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.BlueDoorProgram, SuperMetroid.Core.Rooms.Doors.BlueDoorFrame, SuperMetroid.Core.Rooms.Doors.BlueDoorCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue door caps' PLM programs (#1296), in place of their bank-$84 instruction lists: a shot cap opens and
/// deletes; a cap closing behind Samus closes, restores its BTS and deletes.
/// </summary>
internal static class BlueDoorPlmProgramDefinitions
{
    /// <summary>The library-3 door-opening sound.</summary>
    internal static readonly SoundEffectId OpenSound = Library3Sound.DoorOpened;
    /// <summary>The library-3 door-closing sound.</summary>
    internal static readonly SoundEffectId CloseSound = Library3Sound.DoorClosed;
    /// <summary>The door sounds' native queue cap.</summary>
    internal const byte SoundMaximumQueued = 6;
    /// <summary>How long each opening image shows before the last.</summary>
    private const int OpeningFrameDuration = 6;
    /// <summary>How long the opened cap's cleared image shows before the PLM deletes.</summary>
    private const int OpenedDuration = 94;
    /// <summary>How long each closing image shows.</summary>
    private const int ClosingFrameDuration = 2;

    /// <summary>Every blue cap program.</summary>
    internal static AnimationProgramSet<BlueDoorProgram, BlueDoorFrame, BlueDoorCue> Programs { get; } = new(Build());

    /// <summary>The program a shot cap facing <paramref name="orientation"/> opens with.</summary>
    internal static BlueDoorProgram Opening(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left => BlueDoorProgram.LeftOpening,
        ColoredDoorOrientation.Right => BlueDoorProgram.RightOpening,
        ColoredDoorOrientation.Up => BlueDoorProgram.UpOpening,
        ColoredDoorOrientation.Down => BlueDoorProgram.DownOpening,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
    };

    /// <summary>The program a cap facing <paramref name="orientation"/> closes behind Samus with.</summary>
    internal static BlueDoorProgram Closing(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left => BlueDoorProgram.LeftClosing,
        ColoredDoorOrientation.Right => BlueDoorProgram.RightClosing,
        ColoredDoorOrientation.Up => BlueDoorProgram.UpClosing,
        ColoredDoorOrientation.Down => BlueDoorProgram.DownClosing,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
    };

    /// <summary>The program that restores a closed cap facing <paramref name="orientation"/>.</summary>
    internal static BlueDoorProgram Closed(ColoredDoorOrientation orientation) => orientation switch
    {
        ColoredDoorOrientation.Left => BlueDoorProgram.LeftClosed,
        ColoredDoorOrientation.Right => BlueDoorProgram.RightClosed,
        ColoredDoorOrientation.Up => BlueDoorProgram.UpClosed,
        ColoredDoorOrientation.Down => BlueDoorProgram.DownClosed,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {orientation}."),
    };

    /// <summary>The facing a blue program draws for.</summary>
    internal static ColoredDoorOrientation Orientation(BlueDoorProgram program) => program switch
    {
        BlueDoorProgram.LeftOpening or BlueDoorProgram.LeftClosing or BlueDoorProgram.LeftClosed => ColoredDoorOrientation.Left,
        BlueDoorProgram.RightOpening or BlueDoorProgram.RightClosing or BlueDoorProgram.RightClosed => ColoredDoorOrientation.Right,
        BlueDoorProgram.UpOpening or BlueDoorProgram.UpClosing or BlueDoorProgram.UpClosed => ColoredDoorOrientation.Up,
        BlueDoorProgram.DownOpening or BlueDoorProgram.DownClosing or BlueDoorProgram.DownClosed => ColoredDoorOrientation.Down,
        _ => throw new InvalidOperationException($"Undefined {nameof(BlueDoorProgram)} {program}."),
    };

    /// <summary>Builds the three programs of each facing.</summary>
    private static Dictionary<BlueDoorProgram, Step[]> Build()
    {
        var programs = new Dictionary<BlueDoorProgram, Step[]>();
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            programs[Opening(orientation)] =
            [
                Step.Publish(BlueDoorCue.QueueOpenSound),
                Step.Show(BlueDoorFrame.Blue1, OpeningFrameDuration),
                Step.Show(BlueDoorFrame.Blue2, OpeningFrameDuration),
                Step.Show(BlueDoorFrame.Blue3, OpeningFrameDuration),
                Step.Show(BlueDoorFrame.Clear, OpenedDuration),
                Step.Stop,
            ];
            programs[Closing(orientation)] =
            [
                Step.Show(BlueDoorFrame.Clear, ClosingFrameDuration),
                Step.Show(BlueDoorFrame.Blue3, ClosingFrameDuration),
                Step.Publish(BlueDoorCue.QueueCloseSound),
                Step.Show(BlueDoorFrame.Blue2, ClosingFrameDuration),
                Step.Show(BlueDoorFrame.Blue1, ClosingFrameDuration),
                Step.FallInto(Closed(orientation)),
            ];
            programs[Closed(orientation)] =
            [
                Step.Publish(BlueDoorCue.RestoreBlueBts),
                Step.Show(BlueDoorFrame.Blue0, 1),
                Step.Stop,
            ];
        }
        return programs;
    }
}
