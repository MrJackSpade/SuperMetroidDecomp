using SuperMetroid.Core.Fx;
using Step = SuperMetroid.Core.Fx.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.ColoredDoorProgram, SuperMetroid.Core.Rooms.Doors.DoorCapFrame, SuperMetroid.Core.Rooms.Doors.ColoredDoorCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The coloured door caps' PLM programs (#1296), in place of their bank-$84 instruction lists at $84:BFFD..C488. Every
/// colour and facing runs the same shape; the yellow caps keep their own timings, and the red cap sounds each hit.
/// </summary>
internal static class ColoredDoorPlmProgramDefinitions
{
    /// <summary>The library-3 sound each hit on a red cap queues.</summary>
    internal const byte RedHitSound = 9;
    /// <summary>The hits a red cap takes before it opens.</summary>
    internal const byte RedHitThreshold = 5;
    /// <summary>The hits a yellow or green cap takes before it opens.</summary>
    internal const byte HitThreshold = 1;
    /// <summary>The hit counter a Super Missile seeds on a red cap: the next count passes any threshold.</summary>
    internal const byte RedSuperMissileHitCounter = 119;
    /// <summary>How long each closing image shows.</summary>
    private const int ClosingFrameDuration = 2;
    /// <summary>How long the blue image of a hit flash shows.</summary>
    private const int FlashBlueDuration = 3;
    /// <summary>How long the cap image of a hit flash shows.</summary>
    private const int FlashCapDuration = 4;
    /// <summary>How many blue-and-cap pairs a hit flashes.</summary>
    private const int FlashPairs = 3;

    /// <summary>Each colour and facing's program set.</summary>
    private static readonly Dictionary<(ColoredDoorColor, ColoredDoorOrientation), AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue>> Sets =
        Enum.GetValues<ColoredDoorColor>()
            .SelectMany(color => Enum.GetValues<ColoredDoorOrientation>().Select(orientation => (color, orientation)))
            .ToDictionary(variant => variant, variant => new AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue>(
                Build(variant.color, variant.orientation)));

    /// <summary>The programs of the <paramref name="color"/> cap facing <paramref name="orientation"/>.</summary>
    internal static AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue> ProgramsFor(
        ColoredDoorColor color, ColoredDoorOrientation orientation) => Sets[(color, orientation)];

    /// <summary>The hits the <paramref name="color"/> cap takes before it opens.</summary>
    internal static byte HitThresholdOf(ColoredDoorColor color) => color switch
    {
        ColoredDoorColor.Red => RedHitThreshold,
        ColoredDoorColor.Yellow or ColoredDoorColor.Green => HitThreshold,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {color}."),
    };

    /// <summary>The shot-wake pre-instruction the <paramref name="color"/> cap installs.</summary>
    internal static ColoredDoorPreInstruction WakeOf(ColoredDoorColor color) => color switch
    {
        ColoredDoorColor.Yellow => ColoredDoorPreInstruction.WakeOnPowerBomb,
        ColoredDoorColor.Green => ColoredDoorPreInstruction.WakeOnSuperMissile,
        ColoredDoorColor.Red => ColoredDoorPreInstruction.WakeOnMissile,
        _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {color}."),
    };

    /// <summary>One colour and facing's five programs.</summary>
    private static Dictionary<ColoredDoorProgram, Step[]> Build(ColoredDoorColor color, ColoredDoorOrientation orientation)
    {
        bool yellowDown = color == ColoredDoorColor.Yellow && orientation == ColoredDoorOrientation.Down;
        bool yellowLeft = color == ColoredDoorColor.Yellow && orientation == ColoredDoorOrientation.Left;
        List<Step> waiting =
        [
            Step.Publish(ColoredDoorCue.BranchIfOpened),
            Step.Publish(ColoredDoorCue.LinkHit),
            Step.Publish(ColoredDoorCue.ArmWake),
            Step.Show(DoorCapFrame.Closed0, yellowDown ? 2 : 1),
        ];
        if (yellowDown)
            waiting.Add(Step.Show(DoorCapFrame.Closed0, 1));
        waiting.Add(Step.FallInto(ColoredDoorProgram.Sleep));

        List<Step> hit = [Step.Publish(ColoredDoorCue.CountHit)];
        if (color == ColoredDoorColor.Red)
            hit.Add(Step.Publish(ColoredDoorCue.QueueHitSound));
        for (int pair = 0; pair < FlashPairs; pair++)
        {
            hit.Add(Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration));
            hit.Add(Step.Show(DoorCapFrame.Closed0, FlashCapDuration));
        }
        hit.Add(Step.GoTo(ColoredDoorProgram.Sleep));

        // The yellow left cap opens slowly and lingers open; every other cap opens quickly and deletes at once.
        int openingFrameDuration = yellowLeft ? 4 : 6;
        int openedDuration = yellowLeft ? 92 : 1;
        return new Dictionary<ColoredDoorProgram, Step[]>
        {
            [ColoredDoorProgram.Closing] =
            [
                Step.Show(DoorCapFrame.Clear, ClosingFrameDuration),
                Step.Show(DoorCapFrame.Closed3, ClosingFrameDuration),
                Step.Publish(ColoredDoorCue.QueueCloseSound),
                Step.Show(DoorCapFrame.Closed2, ClosingFrameDuration),
                Step.Show(DoorCapFrame.Closed1, ClosingFrameDuration),
                Step.Show(DoorCapFrame.Closed0, 1),
                Step.FallInto(ColoredDoorProgram.Waiting),
            ],
            [ColoredDoorProgram.Waiting] = [.. waiting],
            [ColoredDoorProgram.Sleep] = [Step.Hold],
            [ColoredDoorProgram.Hit] = [.. hit],
            [ColoredDoorProgram.Open] =
            [
                Step.Publish(ColoredDoorCue.QueueOpenSound),
                Step.Show(DoorCapFrame.Closed1, openingFrameDuration),
                Step.Show(DoorCapFrame.Closed2, openingFrameDuration),
                Step.Show(DoorCapFrame.Closed3, openingFrameDuration),
                Step.Show(DoorCapFrame.Clear, openedDuration),
                Step.Stop,
            ],
        };
    }
}
