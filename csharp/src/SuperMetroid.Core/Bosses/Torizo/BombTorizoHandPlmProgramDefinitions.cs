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
    /// <summary>How many fragments the hand drops.</summary>
    internal const int FragmentCount = 8;
    /// <summary>How long the woken hand holds before it crumbles.</summary>
    private const int CrumbleDelay = 120;

    /// <summary>Both hand programs.</summary>
    internal static AnimationProgramSet<BombTorizoHandProgram, BombTorizoHandDraw, BombTorizoHandCue> Programs { get; } = new(
        new Dictionary<BombTorizoHandProgram, Step[]>
        {
            [BombTorizoHandProgram.Sleeping] =
            [
                Step.Show(BombTorizoHandDraw.Intact, 1),
                Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.ArmBombWake)),
                Step.Hold,
            ],
            [BombTorizoHandProgram.Crumble] = Crumble(),
        });

    /// <summary>How long the hand shows before fragment <paramref name="fragment"/> drops.</summary>
    internal static int FragmentDelay(int fragment) => fragment switch
    {
        0 => 96,
        1 => 48,
        _ => 17 - fragment,
    };

    /// <summary>The hold, the eight fragments, the cleared hand and the music.</summary>
    private static Step[] Crumble()
    {
        List<Step> steps =
        [
            Step.Show(BombTorizoHandDraw.Intact, CrumbleDelay),
        ];
        for (int fragment = 0; fragment < FragmentCount; fragment++)
        {
            steps.Add(Step.Show(BombTorizoHandDraw.Intact, FragmentDelay(fragment)));
            steps.Add(Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, fragment)));
        }
        steps.Add(Step.Show(BombTorizoHandDraw.Cleared, 1));
        steps.Add(Step.Publish(new BombTorizoHandCue(BombTorizoHandCueKind.QueueMusic)));
        steps.Add(Step.Stop);
        return [.. steps];
    }

    /// <summary>The program a wake past <paramref name="program"/>'s sleep continues with.</summary>
    internal static BombTorizoHandProgram? ProgramAfterSleep(BombTorizoHandProgram program) => program switch
    {
        BombTorizoHandProgram.Sleeping => BombTorizoHandProgram.Crumble,
        BombTorizoHandProgram.Crumble => null,
        _ => throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandProgram)} {program}."),
    };
}
