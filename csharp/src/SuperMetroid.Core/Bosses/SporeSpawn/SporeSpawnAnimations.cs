using SuperMetroid.Core.Programs;
using F = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnFrameKind;
using K = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCueKind;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnBodySegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body animations (#641).</summary>
internal static class SporeSpawnAnimations
{
    /// <summary>How long the living room waits before the descent.</summary>
    private const int DescentWait = 256;
    /// <summary>The fight's first sway before the head opens.</summary>
    private const int FirstSway = 768;
    /// <summary>The closed sway with spores stopped.</summary>
    private const int ClosedSway = 512;
    /// <summary>The closed sway with spores falling, before the head opens again.</summary>
    private const int SporingSway = 208;
    /// <summary>Each closing and open-pulse pose's dwell.</summary>
    private const int PoseDuration = 8;
    /// <summary>The open head's pulse repeats.</summary>
    private const int OpenPulseRepeats = 5;
    /// <summary>The dying explosions and hardening dust clouds.</summary>
    private const int DeathBurstRepeats = 10;
    /// <summary>The wait between death bursts.</summary>
    private const int DeathBurstWait = 8;
    /// <summary>Each death-palette fade step's dwell.</summary>
    private const int FadeStepDuration = 16;
    /// <summary>The death palette's fade steps; the husk's palette is the last.</summary>
    internal const int DeathPaletteSteps = 7;
    /// <summary>The sway's maximum X radius as the fight starts.</summary>
    private const int FirstMaximumXRadius = 64;

    /// <summary>The opening poses' dwells, closed through fully open.</summary>
    private static readonly int[] OpeningDurations = [8, 8, 8, 7, 7, 6, 1];

    /// <summary>Every body animation.</summary>
    internal static AnimationSegments<SporeSpawnBodySegment, SporeSpawnFrame, SporeSpawnCue> Segments { get; } = new(
        new Dictionary<SporeSpawnBodySegment, AnimationBeat<SporeSpawnFrame, SporeSpawnCue>[]>
    {
        [P.InitialDead] =
        [
            Cue(new SporeSpawnCue.LoadDeathTargetPalette(DeathPaletteStep: DeathPaletteSteps - 1)),
            SelectMovement(SporeSpawnMovement.Idle),
            Show(new(F.Dead, 0), 1),
        ],
        [P.InitialAlive] =
        [
            Show(new(F.Closed, 0), DescentWait),
            SelectMovement(SporeSpawnMovement.Descending),
            Show(new(F.Closed, 0), 1),
        ],
        [P.FightStarted] =
        [
            Cue(new SporeSpawnCue.SetMaxXRadiusAndAngleDelta(MaximumXRadius: FirstMaximumXRadius, AngleDelta: 1)),
            SelectMovement(SporeSpawnMovement.Moving),
            Show(new(F.Closed, 0), FirstSway),
        ],
        [P.OpenAndStop] =
        [
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Cue(new SporeSpawnCue.QueueSound(Sound: SporeSpawnSoundDefinitions.Opening)),
            Show(new(F.Closed, 0), 1),
            .. Enumerable.Range(0, OpeningDurations.Length).Select(pose => Show(new(F.Opening, pose), OpeningDurations[pose])),
            Cue(new SporeSpawnCue.Signal(K.ClearDamagedFlag)),
            SelectMovement(SporeSpawnMovement.Idle),
        ],
        [P.OpenPulse] =
        [
            Show(new(F.Open, 0), PoseDuration),
            Show(new(F.Open, 1), PoseDuration),
            Show(new(F.Open, 2), PoseDuration),
            Show(new(F.Open, 1), PoseDuration),
        ],
        [P.CloseAndMove] =
        [
            .. Closing(),
            Show(new(F.Closed, 0), 1),
            SelectMovement(SporeSpawnMovement.Moving),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: false)),
            Cue(new SporeSpawnCue.Signal(K.IncreaseMaxXRadius)),
            Show(new(F.Closed, 0), ClosedSway),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Show(new(F.Closed, 0), SporingSway),
        ],
        [P.Death] =
        [
            SelectMovement(SporeSpawnMovement.SetupDeath),
            Show(new(F.Opening, 5), 1),
            SelectMovement(SporeSpawnMovement.Dying),
        ],
        [P.DyingExplosions] =
        [
            Show(new(F.Opening, 5), 1),
            Cue(new SporeSpawnCue.Signal(K.SpawnDyingExplosion)),
            Wait(DeathBurstWait),
            .. Closing(),
            Show(new(F.Dead, 0), 1),
            Cue(new SporeSpawnCue.Signal(K.Harden)),
        ],
        [P.Hardening] =
        [
            Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
            Wait(DeathBurstWait),
            .. Enumerable.Range(0, DeathPaletteSteps).SelectMany(step => new[]
            {
                Cue(new SporeSpawnCue.LoadDeathPalette(DeathPaletteStep: step)),
                Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
                Show(new(F.Dead, 0), FadeStepDuration),
            }),
            Cue(new SporeSpawnCue.Signal(K.RequestDeathDrops)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnBodySegment, Animation<SporeSpawnBodySegment>> StartingAt { get; } = new Dictionary<SporeSpawnBodySegment, Animation<SporeSpawnBodySegment>>
    {
        [P.InitialDead] = new([new(P.InitialDead, 0, 3, 1)], AnimationEnd.Holds),
        [P.InitialAlive] = new([new(P.InitialAlive, 0, 3, 1)], AnimationEnd.Holds),
        [P.FightStarted] = new([new(P.FightStarted, 0, 3, 1), new(P.OpenAndStop, 0, Segments.Beats(P.OpenAndStop).Count, 1), new(P.OpenPulse, 0, 4, OpenPulseRepeats), new(P.CloseAndMove, 0, Segments.Beats(P.CloseAndMove).Count, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OpenAndStop] = new([new(P.OpenAndStop, 0, Segments.Beats(P.OpenAndStop).Count, 1), new(P.OpenPulse, 0, 4, OpenPulseRepeats), new(P.CloseAndMove, 0, Segments.Beats(P.CloseAndMove).Count, 1)], AnimationEnd.LoopsFrom(0)),
        [P.CloseAndMove] = new([new(P.CloseAndMove, 0, Segments.Beats(P.CloseAndMove).Count, 1), new(P.OpenAndStop, 0, Segments.Beats(P.OpenAndStop).Count, 1), new(P.OpenPulse, 0, 4, OpenPulseRepeats)], AnimationEnd.LoopsFrom(0)),
        [P.Death] = new([new(P.Death, 0, 3, 1), new(P.DyingExplosions, 0, 3, DeathBurstRepeats), new(P.DyingExplosions, 3, Segments.Beats(P.DyingExplosions).Count - 3, 1), new(P.Hardening, 0, 2, DeathBurstRepeats), new(P.Hardening, 2, Segments.Beats(P.Hardening).Count - 2, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnBodySegment> StartingWith(SporeSpawnBodySegment segment) => StartingAt[segment];

    /// <summary>The six closing poses, from the second-to-last opening pose back to the first.</summary>
    private static IEnumerable<AnimationBeat<SporeSpawnFrame, SporeSpawnCue>> Closing() =>
        Enumerable.Range(0, OpeningDurations.Length - 1).Reverse().Select(pose => Show(new(F.Opening, pose), PoseDuration));

    /// <summary>Selects <paramref name="movement"/> as the main-AI movement.</summary>
    private static AnimationBeat<SporeSpawnFrame, SporeSpawnCue> SelectMovement(SporeSpawnMovement movement) =>
        Cue(new SporeSpawnCue.SetMovement(Movement: movement));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SporeSpawnFrame, SporeSpawnCue> Cue(SporeSpawnCue cue) =>
        new AnimationBeat<SporeSpawnFrame, SporeSpawnCue>.Event(cue);

    /// <summary>Pauses on the shown frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SporeSpawnFrame, SporeSpawnCue> Wait(int duration) =>
        new AnimationBeat<SporeSpawnFrame, SporeSpawnCue>.Pause(duration);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SporeSpawnFrame, SporeSpawnCue> Show(SporeSpawnFrame frame, int duration) =>
        new AnimationBeat<SporeSpawnFrame, SporeSpawnCue>.Frame(frame, duration);

}
