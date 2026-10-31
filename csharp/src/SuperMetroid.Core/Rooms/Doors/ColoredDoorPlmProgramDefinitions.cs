using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.ColoredDoorProgram, SuperMetroid.Core.Rooms.Doors.DoorCapFrame, SuperMetroid.Core.Rooms.Doors.ColoredDoorCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The coloured door caps' PLM programs (#1296), in place of their bank-$84 instruction lists at $84:BFFD..C488. Every
/// colour and facing runs the same shape; the yellow caps keep their own timings, and the red cap sounds each hit.
/// </summary>
internal static class ColoredDoorPlmProgramDefinitions
{
    /// <summary>The library-3 sound each hit on a red cap queues.</summary>
    internal static readonly SoundEffectId RedHitSound = Library3Sound.MissileDoorHit;
    /// <summary>The hits a red cap takes before it opens.</summary>
    internal const int RedHitThreshold = 5;
    /// <summary>The hits a yellow or green cap takes before it opens.</summary>
    internal const int HitThreshold = 1;
    /// <summary>The hit counter a Super Missile seeds on a red cap: the next count passes any threshold.</summary>
    internal const int RedSuperMissileHitCounter = 119;
    /// <summary>How long each closing image shows.</summary>
    private const int ClosingFrameDuration = 2;
    /// <summary>How long the blue image of a hit flash shows.</summary>
    private const int FlashBlueDuration = 3;
    /// <summary>How long the cap image of a hit flash shows.</summary>
    private const int FlashCapDuration = 4;
    /// <summary>How long each opening image shows.</summary>
    private const int OpeningFrameDuration = 6;
    /// <summary>How long the yellow left cap shows each opening image: it opens slowly.</summary>
    private const int YellowLeftOpeningFrameDuration = 4;
    /// <summary>How long the opened cap's cleared image shows before the PLM deletes.</summary>
    private const int OpenedDuration = 1;
    /// <summary>How long the yellow left cap's cleared image shows: it lingers open.</summary>
    private const int YellowLeftOpenedDuration = 92;
    /// <summary>How long the waiting cap shows its closed image before it sleeps.</summary>
    private const int WaitingDuration = 1;
    /// <summary>How long the yellow downward cap shows its closed image before its second, ordinary one.</summary>
    private const int YellowDownFirstWaitingDuration = 2;

    /// <summary>
    /// Each colour and facing's program set. Every cap runs the same shape; the yellow downward cap waits on its closed
    /// image twice, the yellow left cap opens slowly and lingers open, and the red cap sounds each hit.
    /// </summary>
    internal static IReadOnlyDictionary<ColoredDoorVariant, AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue>> Sets { get; } =
        new Dictionary<ColoredDoorVariant, AnimationProgramSet<ColoredDoorProgram, DoorCapFrame, ColoredDoorCue>>
    {
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = new(Programs(Waiting(), Hit(), YellowLeftOpeningFrameDuration, YellowLeftOpenedDuration)),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = new(Programs(YellowDownWaiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Left)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Right)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Up)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Down)] = new(Programs(Waiting(), Hit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Left)] = new(Programs(Waiting(), RedHit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Right)] = new(Programs(Waiting(), RedHit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Up)] = new(Programs(Waiting(), RedHit(), OpeningFrameDuration, OpenedDuration)),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Down)] = new(Programs(Waiting(), RedHit(), OpeningFrameDuration, OpenedDuration)),
    };

    /// <summary>The hits each colour's cap takes before it opens.</summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, byte> HitThresholds { get; } = new Dictionary<ColoredDoorColor, byte>
    {
        [ColoredDoorColor.Red] = RedHitThreshold,
        [ColoredDoorColor.Yellow] = HitThreshold,
        [ColoredDoorColor.Green] = HitThreshold,
    };

    /// <summary>The shot-wake pre-instruction each colour's cap installs.</summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, ColoredDoorPreInstruction> Wakes { get; } =
        new Dictionary<ColoredDoorColor, ColoredDoorPreInstruction>
    {
        [ColoredDoorColor.Yellow] = ColoredDoorPreInstruction.WakeOnPowerBomb,
        [ColoredDoorColor.Green] = ColoredDoorPreInstruction.WakeOnSuperMissile,
        [ColoredDoorColor.Red] = ColoredDoorPreInstruction.WakeOnMissile,
    };

    /// <summary>A waiting cap links its hit, arms its wake and sleeps.</summary>
    private static Step[] Waiting() =>
    [
        Step.Publish(ColoredDoorCue.BranchIfOpened),
        Step.Publish(ColoredDoorCue.LinkHit),
        Step.Publish(ColoredDoorCue.ArmWake),
        Step.Show(DoorCapFrame.Closed0, WaitingDuration),
        Step.FallInto(ColoredDoorProgram.Sleep),
    ];

    /// <summary>The yellow downward cap waits on its closed image twice before it sleeps.</summary>
    private static Step[] YellowDownWaiting() =>
    [
        Step.Publish(ColoredDoorCue.BranchIfOpened),
        Step.Publish(ColoredDoorCue.LinkHit),
        Step.Publish(ColoredDoorCue.ArmWake),
        Step.Show(DoorCapFrame.Closed0, YellowDownFirstWaitingDuration),
        Step.Show(DoorCapFrame.Closed0, WaitingDuration),
        Step.FallInto(ColoredDoorProgram.Sleep),
    ];

    /// <summary>A hit counts and flashes the cap blue three times.</summary>
    private static Step[] Hit() =>
    [
        Step.Publish(ColoredDoorCue.CountHit),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.GoTo(ColoredDoorProgram.Sleep),
    ];

    /// <summary>A hit on the red cap counts, sounds and flashes the cap blue three times.</summary>
    private static Step[] RedHit() =>
    [
        Step.Publish(ColoredDoorCue.CountHit),
        Step.Publish(ColoredDoorCue.QueueHitSound),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
        Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
        Step.GoTo(ColoredDoorProgram.Sleep),
    ];

    /// <summary>One colour and facing's five programs, from its waiting and hit programs and its opening timings.</summary>
    private static Dictionary<ColoredDoorProgram, Step[]> Programs(Step[] waiting, Step[] hit, int openingFrameDuration, int openedDuration) =>
        new()
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
            [ColoredDoorProgram.Waiting] = waiting,
            [ColoredDoorProgram.Sleep] = [Step.Hold],
            [ColoredDoorProgram.Hit] = hit,
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
