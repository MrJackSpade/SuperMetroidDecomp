using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Doors.GreyDoorProgram, SuperMetroid.Core.Rooms.Doors.DoorCapFrame, SuperMetroid.Core.Rooms.Doors.GreyDoorCue>;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps' PLM programs (#1296), in place of their bank-$84 instruction lists at $84:BE59..BFFC and Bomb
/// Torizo's at $84:BA4C..BAD0. A grey cap sleeps until its unlock condition holds, then flashes blue until shot.
/// </summary>
internal static class GreyDoorPlmProgramDefinitions
{
    /// <summary>The hits a grey cap takes before it opens.</summary>
    internal const byte HitThreshold = 1;
    /// <summary>How long each closing image shows.</summary>
    private const int ClosingFrameDuration = 2;
    /// <summary>How long Bomb Torizo's door holds open once Samus has Bombs, before it closes.</summary>
    private const int BombTorizoCloseDelay = 40;
    /// <summary>How long the blue image of the unlocked flash shows.</summary>
    private const int FlashBlueDuration = 3;
    /// <summary>How long the cap image of the unlocked flash shows.</summary>
    private const int FlashCapDuration = 4;
    /// <summary>How long each opening image shows.</summary>
    private const int OpeningFrameDuration = 4;

    /// <summary>Each grey cap's program set: Bomb Torizo's door closes only once Samus has Bombs.</summary>
    internal static IReadOnlyDictionary<GreyDoorVariant, AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue>> Sets { get; } =
        new Dictionary<GreyDoorVariant, AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue>>
    {
        [new(ColoredDoorOrientation.Left, BombTorizo: false)] = new(Programs(Closing())),
        [new(ColoredDoorOrientation.Right, BombTorizo: false)] = new(Programs(Closing())),
        [new(ColoredDoorOrientation.Up, BombTorizo: false)] = new(Programs(Closing())),
        [new(ColoredDoorOrientation.Down, BombTorizo: false)] = new(Programs(Closing())),
        [new(ColoredDoorOrientation.Right, BombTorizo: true)] = new(Programs(BombTorizoClosing())),
    };

    /// <summary>The unlock-check pre-instruction a door with each condition installs.</summary>
    internal static IReadOnlyDictionary<GreyDoorCondition, GreyDoorPreInstruction> UnlockChecks { get; } =
        new Dictionary<GreyDoorCondition, GreyDoorPreInstruction>
    {
        [GreyDoorCondition.AreaBossDefeated] = GreyDoorPreInstruction.WakeIfAreaBossDead,
        [GreyDoorCondition.AreaMiniBossDefeated] = GreyDoorPreInstruction.WakeIfAreaMiniBossDead,
        [GreyDoorCondition.AreaTorizoDefeated] = GreyDoorPreInstruction.WakeIfAreaTorizoDead,
        [GreyDoorCondition.EnemyDeathQuota] = GreyDoorPreInstruction.WakeIfEnemiesDead,
        [GreyDoorCondition.Never] = GreyDoorPreInstruction.PlayDudSound,
        [GreyDoorCondition.TourianStatueFinished] = GreyDoorPreInstruction.WakeIfTourianStatueFinished,
        [GreyDoorCondition.CrittersEscaped] = GreyDoorPreInstruction.WakeIfCrittersEscaped,
    };

    /// <summary>One grey cap's seven programs, closing with <paramref name="closing"/>; the unlocked flash shows three blue-and-cap pairs per pass.</summary>
    private static Dictionary<GreyDoorProgram, Step[]> Programs(Step[] closing) =>
        new()
        {
            [GreyDoorProgram.Closing] = closing,
            [GreyDoorProgram.Waiting] =
            [
                Step.Publish(GreyDoorCue.BranchIfOpened),
                Step.Publish(GreyDoorCue.LinkActivate),
                Step.Publish(GreyDoorCue.ArmCondition),
                Step.Show(DoorCapFrame.Closed0, 1),
                Step.FallInto(GreyDoorProgram.Sleep),
            ],
            [GreyDoorProgram.Sleep] = [Step.Hold],
            [GreyDoorProgram.Activate] =
            [
                Step.Publish(GreyDoorCue.LinkHit),
                Step.Publish(GreyDoorCue.ArmShotWake),
                Step.FallInto(GreyDoorProgram.Flash),
            ],
            [GreyDoorProgram.Flash] =
            [
                Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
                Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
                Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
                Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
                Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration),
                Step.Show(DoorCapFrame.Closed0, FlashCapDuration),
                Step.GoTo(GreyDoorProgram.Flash),
            ],
            [GreyDoorProgram.Hit] = [Step.Publish(GreyDoorCue.CountHit), Step.FallInto(GreyDoorProgram.Open)],
            [GreyDoorProgram.Open] =
            [
                Step.Publish(GreyDoorCue.QueueOpenSound),
                Step.Show(DoorCapFrame.Closed1, OpeningFrameDuration),
                Step.Show(DoorCapFrame.Closed2, OpeningFrameDuration),
                Step.Show(DoorCapFrame.Closed3, OpeningFrameDuration),
                Step.Show(DoorCapFrame.Clear, 1),
                Step.Stop,
            ],
        };

    /// <summary>An ordinary grey cap closes behind Samus and runs into its waiting program.</summary>
    private static Step[] Closing() =>
    [
        Step.Show(DoorCapFrame.Clear, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed3, ClosingFrameDuration),
        Step.Publish(GreyDoorCue.QueueCloseSound),
        Step.Show(DoorCapFrame.Closed2, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed1, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed0, 1),
        Step.FallInto(GreyDoorProgram.Waiting),
    ];

    /// <summary>Bomb Torizo's door stays open until Samus has Bombs, holds, then closes and goes to its waiting program.</summary>
    private static Step[] BombTorizoClosing() =>
    [
        Step.Show(DoorCapFrame.Clear, ClosingFrameDuration),
        Step.Publish(GreyDoorCue.RepeatUntilSamusHasBombs),
        Step.Show(DoorCapFrame.Clear, BombTorizoCloseDelay),
        Step.Publish(GreyDoorCue.QueueCloseSound),
        Step.Show(DoorCapFrame.Closed3, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed2, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed1, ClosingFrameDuration),
        Step.Show(DoorCapFrame.Closed0, 1),
        Step.GoTo(GreyDoorProgram.Waiting),
    ];
}
