using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
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
    /// <summary>How many blue-and-cap pairs one pass of the flash shows.</summary>
    private const int FlashPairs = 3;
    /// <summary>How long each opening image shows.</summary>
    private const int OpeningFrameDuration = 4;

    /// <summary>Each grey cap's program set.</summary>
    private static readonly Dictionary<GreyDoorVariant, AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue>> Sets =
        Enum.GetValues<ColoredDoorOrientation>().Select(orientation => new GreyDoorVariant(orientation, BombTorizo: false))
            .Append(new GreyDoorVariant(ColoredDoorOrientation.Right, BombTorizo: true))
            .ToDictionary(variant => variant, variant => new AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue>(Build(variant)));

    /// <summary>The programs of grey cap <paramref name="variant"/>.</summary>
    internal static AnimationProgramSet<GreyDoorProgram, DoorCapFrame, GreyDoorCue> ProgramsFor(GreyDoorVariant variant) => Sets[variant];

    /// <summary>The unlock-check pre-instruction a door with <paramref name="condition"/> installs.</summary>
    internal static GreyDoorPreInstruction UnlockCheckOf(GreyDoorCondition condition) => condition switch
    {
        GreyDoorCondition.AreaBossDefeated => GreyDoorPreInstruction.WakeIfAreaBossDead,
        GreyDoorCondition.AreaMiniBossDefeated => GreyDoorPreInstruction.WakeIfAreaMiniBossDead,
        GreyDoorCondition.AreaTorizoDefeated => GreyDoorPreInstruction.WakeIfAreaTorizoDead,
        GreyDoorCondition.EnemyDeathQuota => GreyDoorPreInstruction.WakeIfEnemiesDead,
        GreyDoorCondition.Never => GreyDoorPreInstruction.PlayDudSound,
        GreyDoorCondition.TourianStatueFinished => GreyDoorPreInstruction.WakeIfTourianStatueFinished,
        GreyDoorCondition.CrittersEscaped => GreyDoorPreInstruction.WakeIfCrittersEscaped,
        _ => throw new InvalidOperationException($"Undefined {nameof(GreyDoorCondition)} {condition}."),
    };

    /// <summary>One grey cap's seven programs.</summary>
    private static Dictionary<GreyDoorProgram, Step[]> Build(GreyDoorVariant variant)
    {
        List<Step> flash = [];
        for (int pair = 0; pair < FlashPairs; pair++)
        {
            flash.Add(Step.Show(DoorCapFrame.BlueFlash, FlashBlueDuration));
            flash.Add(Step.Show(DoorCapFrame.Closed0, FlashCapDuration));
        }
        flash.Add(Step.GoTo(GreyDoorProgram.Flash));
        return new Dictionary<GreyDoorProgram, Step[]>
        {
            [GreyDoorProgram.Closing] = variant.BombTorizo ? BombTorizoClosing() : Closing(),
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
            [GreyDoorProgram.Flash] = [.. flash],
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
    }

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
