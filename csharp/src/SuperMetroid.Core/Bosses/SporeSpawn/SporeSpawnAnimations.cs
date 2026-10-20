using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnFrameKind;
using K = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCueKind;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnBodyProgram;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body programs (#641), in place of its bank-$A5 lists.</summary>
internal static class SporeSpawnAnimations
{
    /// <summary>How long the living room waits before the descent ($A5:E6C7).</summary>
    private const int DescentWait = 256;
    /// <summary>The fight's first sway before the head opens ($A5:E6DF).</summary>
    private const int FirstSway = 768;
    /// <summary>The closed sway with spores stopped ($A5:E74F).</summary>
    private const int ClosedSway = 512;
    /// <summary>The closed sway with spores falling, before the head opens again ($A5:E757).</summary>
    private const int SporingSway = 208;
    /// <summary>Each closing and open-pulse pose's dwell.</summary>
    private const int PoseDuration = 8;
    /// <summary>The open head's pulse repeats ($A5:E711).</summary>
    private const int OpenPulseRepeats = 5;
    /// <summary>The dying explosions and hardening dust clouds ($A5:E789/E7B9).</summary>
    private const int DeathBurstRepeats = 10;
    /// <summary>The wait between death bursts ($A5:E793/E7BF).</summary>
    private const int DeathBurstWait = 8;
    /// <summary>Each death-palette fade step's dwell ($A5:E7CD..E809).</summary>
    private const int FadeStepDuration = 16;
    /// <summary>The death palette's fade steps; the husk's palette is the last.</summary>
    internal const int DeathPaletteSteps = 7;
    /// <summary>The sway's maximum X radius as the fight starts ($A5:E6D7).</summary>
    private const int FirstMaximumXRadius = 64;

    /// <summary>The opening poses' dwells, closed through fully open ($A5:E6EB..E707).</summary>
    private static readonly int[] OpeningDurations = [8, 8, 8, 7, 7, 6, 1];

    /// <summary>Every body program.</summary>
    internal static AnimationProgramSet<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> Programs { get; } = new(
        new Dictionary<SporeSpawnBodyProgram, AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>[]>
    {
        [P.InitialDead] =
        [
            Cue(new SporeSpawnCue.LoadDeathTargetPalette(DeathPaletteStep: DeathPaletteSteps - 1)),
            Function(SporeSpawnFunction.Idle),
            Show(new(F.Dead, 0), 1),
            Hold,
        ],
        [P.InitialAlive] =
        [
            Show(new(F.Closed, 0), DescentWait),
            Function(SporeSpawnFunction.Descending),
            Show(new(F.Closed, 0), 1),
            Hold,
        ],
        [P.FightStarted] =
        [
            Cue(new SporeSpawnCue.SetMaxXRadiusAndAngleDelta(MaximumXRadius: FirstMaximumXRadius, AngleDelta: 1)),
            Function(SporeSpawnFunction.Moving),
            Show(new(F.Closed, 0), FirstSway),
            FallInto(P.OpenAndStop),
        ],
        [P.OpenAndStop] =
        [
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Cue(new SporeSpawnCue.QueueSound(Sound: SporeSpawnSoundDefinitions.Opening)),
            Show(new(F.Closed, 0), 1),
            .. Enumerable.Range(0, OpeningDurations.Length).Select(pose => Show(new(F.Opening, pose), OpeningDurations[pose])),
            Cue(new SporeSpawnCue.Command(K.ClearDamagedFlag)),
            Function(SporeSpawnFunction.Idle),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.SetCounter(OpenPulseRepeats),
            FallInto(P.OpenPulse),
        ],
        [P.OpenPulse] =
        [
            Show(new(F.Open, 0), PoseDuration),
            Show(new(F.Open, 1), PoseDuration),
            Show(new(F.Open, 2), PoseDuration),
            Show(new(F.Open, 1), PoseDuration),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.RepeatWhileCounting(P.OpenPulse),
            FallInto(P.CloseAndMove),
        ],
        [P.CloseAndMove] =
        [
            .. Closing(),
            Show(new(F.Closed, 0), 1),
            Function(SporeSpawnFunction.Moving),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: false)),
            Cue(new SporeSpawnCue.Command(K.IncreaseMaxXRadius)),
            Show(new(F.Closed, 0), ClosedSway),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Show(new(F.Closed, 0), SporingSway),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.GoTo(P.OpenAndStop),
        ],
        [P.Death] =
        [
            Function(SporeSpawnFunction.SetupDeath),
            Show(new(F.Opening, 5), 1),
            Function(SporeSpawnFunction.Dying),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.SetCounter(DeathBurstRepeats),
            FallInto(P.DyingExplosions),
        ],
        [P.DyingExplosions] =
        [
            Show(new(F.Opening, 5), 1),
            Cue(new SporeSpawnCue.Command(K.SpawnDyingExplosion)),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.Wait(DeathBurstWait),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.RepeatWhileCounting(P.DyingExplosions),
            .. Closing(),
            Show(new(F.Dead, 0), 1),
            Cue(new SporeSpawnCue.Command(K.Harden)),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.SetCounter(DeathBurstRepeats),
            FallInto(P.Hardening),
        ],
        [P.Hardening] =
        [
            Cue(new SporeSpawnCue.Command(K.SpawnHardeningDust)),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.Wait(DeathBurstWait),
            AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.RepeatWhileCounting(P.Hardening),
            .. Enumerable.Range(0, DeathPaletteSteps).SelectMany(step => new[]
            {
                Cue(new SporeSpawnCue.LoadDeathPalette(DeathPaletteStep: step)),
                Cue(new SporeSpawnCue.Command(K.SpawnHardeningDust)),
                Show(new(F.Dead, 0), FadeStepDuration),
            }),
            Cue(new SporeSpawnCue.Command(K.RequestDeathDrops)),
            Hold,
        ],
    });

    /// <summary>The six closing poses, from the second-to-last opening pose back to the first.</summary>
    private static IEnumerable<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>> Closing() =>
        Enumerable.Range(0, OpeningDurations.Length - 1).Reverse().Select(pose => Show(new(F.Opening, pose), PoseDuration));

    /// <summary>Selects <paramref name="function"/> as the main-AI function.</summary>
    private static AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> Function(SporeSpawnFunction function) =>
        Cue(new SporeSpawnCue.SetFunction(Function: function));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> Cue(SporeSpawnCue cue) =>
        AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.Publish(cue);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> Show(SporeSpawnFrame frame, int duration) =>
        AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.Show(frame, duration);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> FallInto(SporeSpawnBodyProgram program) =>
        AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue> Hold =>
        AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.Hold;
}
