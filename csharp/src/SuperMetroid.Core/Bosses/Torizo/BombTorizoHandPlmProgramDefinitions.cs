using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Torizo.BombTorizoHandProgram, SuperMetroid.Core.Bosses.Torizo.BombTorizoHandDraw, SuperMetroid.Core.Bosses.Torizo.BombTorizoHandCue>;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The Bomb Torizo hand's PLM programs (#1296), in place of its bank-$84 instruction list: the intact hand sleeps until
/// Samus has Bombs, then holds and drops eight fragments at quickening intervals before the cleared hand and the boss
/// music. The cartridge's crumble list also loads the debris characters the fragments are drawn with (#1406); the
/// fragments show their own pictures, so that load has no step here and takes no time.
/// </summary>
internal static class BombTorizoHandPlmProgramDefinitions
{
    /// <summary>How long the woken hand holds before it crumbles.</summary>
    private const int CrumbleDelay = 120;

    /// <summary>How long the hand shows before each fragment drops: a long first wait and a shorter second, then each wait one frame quicker.</summary>
    internal static readonly int[] FragmentDelays = [96, 48, 15, 14, 13, 12, 11, 10];

    /// <summary>
    /// Both hand programs. The crumble program holds, drops the eight fragments, then shows the cleared hand and queues
    /// the music.
    /// </summary>
    internal static AnimationProgramSet<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue> Programs { get; } = new(
        new Dictionary<BombTorizoHandProgram, Step[]>
        {
            [BombTorizoHandProgram.Sleeping] =
            [
                Step.Show(BombTorizoHandDraw.Intact, 1),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.ArmBombWake)),
                Step.Hold,
            ],
            [BombTorizoHandProgram.Crumble] =
            [
                Step.Show(BombTorizoHandDraw.Intact, CrumbleDelay),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[0]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 0)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[1]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 1)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[2]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 2)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[3]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 3)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[4]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 4)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[5]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 5)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[6]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 6)),
                Step.Show(BombTorizoHandDraw.Intact, FragmentDelays[7]),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, 7)),
                Step.Show(BombTorizoHandDraw.Cleared, 1),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.QueueMusic)),
                Step.Stop,
            ],
        });

    /// <summary>The program a wake past each program's sleep continues with; the crumble program has none.</summary>
    internal static readonly Dictionary<BombTorizoHandProgram, BombTorizoHandProgram?> ProgramsAfterSleep = new()
    {
        [BombTorizoHandProgram.Sleeping] = BombTorizoHandProgram.Crumble,
        [BombTorizoHandProgram.Crumble] = null,
    };
}
