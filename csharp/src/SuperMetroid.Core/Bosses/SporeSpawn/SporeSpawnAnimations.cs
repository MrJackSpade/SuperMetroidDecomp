using SuperMetroid.Core.Animation;
using F = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId;
using SuperMetroid.Core.Enemies.Artwork;
using K = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCueKind;
using P = SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnBodySegment;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body animations (#641), each beat naming the installed frame it shows (#1885).</summary>
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
    /// <summary>Each death-colour fade step's dwell.</summary>
    private const int FadeStepDuration = 16;
    /// <summary>The death colours' fade steps; the husk's colours are the last.</summary>
    internal const int DeathShadeSteps = 7;
    /// <summary>The sway's maximum X radius as the fight starts.</summary>
    private const int FirstMaximumXRadius = 64;

    /// <summary>The opening poses, closed through fully open; the death shows the second-to-last.</summary>
    private static readonly EnemyFrameId[] OpeningFrames =
    [
        F.SporeSpawnParted8, F.SporeSpawnParted16, F.SporeSpawnParted24, F.SporeSpawnParted32,
        F.SporeSpawnParted40, F.SporeSpawnParted44, F.SporeSpawnParted48,
    ];

    /// <summary>The opening poses' dwells, closed through fully open.</summary>
    private static readonly int[] OpeningDurations = [8, 8, 8, 7, 7, 6, 1];

    /// <summary>Every body animation.</summary>
    internal static AnimationSegments<SporeSpawnBodySegment, EnemyFrameId, SporeSpawnCue> Segments { get; } = new(
        new Dictionary<SporeSpawnBodySegment, AnimationBeat<EnemyFrameId, SporeSpawnCue>[]>
    {
        [P.InitialDead] =
        [
            Cue(new SporeSpawnCue.ShowDeathShade(DeathShadeStep: DeathShadeSteps - 1)),
            SelectMovement(SporeSpawnMovement.Idle),
            Show(F.SporeSpawnShutTwin, 1),
        ],
        [P.InitialAlive] =
        [
            Show(F.SporeSpawnShut, DescentWait),
            SelectMovement(SporeSpawnMovement.Descending),
            Show(F.SporeSpawnShut, 1),
        ],
        [P.FightStarted] =
        [
            Cue(new SporeSpawnCue.SetMaxXRadiusAndAngleDelta(MaximumXRadius: FirstMaximumXRadius, AngleDelta: 1)),
            SelectMovement(SporeSpawnMovement.Moving),
            Show(F.SporeSpawnShut, FirstSway),
        ],
        [P.OpenAndStop] =
        [
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Cue(new SporeSpawnCue.QueueSound(Sound: SporeSpawnSoundDefinitions.Opening)),
            Show(F.SporeSpawnShut, 1),
            .. Enumerable.Range(0, OpeningDurations.Length).Select(pose => Show(OpeningFrames[pose], OpeningDurations[pose])),
            Cue(new SporeSpawnCue.Signal(K.ClearDamagedFlag)),
            SelectMovement(SporeSpawnMovement.Idle),
        ],
        [P.OpenPulse] =
        [
            Show(F.SporeSpawnParted48CoreGlowing, PoseDuration),
            Show(F.SporeSpawnParted48CoreBright, PoseDuration),
            Show(F.SporeSpawnParted48Twin, PoseDuration),
            Show(F.SporeSpawnParted48CoreBright, PoseDuration),
        ],
        [P.CloseAndMove] =
        [
            .. Closing(),
            Show(F.SporeSpawnShut, 1),
            SelectMovement(SporeSpawnMovement.Moving),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: false)),
            Cue(new SporeSpawnCue.Signal(K.IncreaseMaxXRadius)),
            Show(F.SporeSpawnShut, ClosedSway),
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Show(F.SporeSpawnShut, SporingSway),
        ],
        [P.Death] =
        [
            SelectMovement(SporeSpawnMovement.SetupDeath),
            Show(F.SporeSpawnParted44, 1),
            SelectMovement(SporeSpawnMovement.Dying),
        ],
        [P.DyingExplosions] =
        [
            Show(F.SporeSpawnParted44, 1),
            Cue(new SporeSpawnCue.Signal(K.SpawnDyingExplosion)),
            Wait(DeathBurstWait),
            .. Closing(),
            Show(F.SporeSpawnShutTwin, 1),
            Cue(new SporeSpawnCue.Signal(K.Harden)),
        ],
        [P.Hardening] =
        [
            Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
            Wait(DeathBurstWait),
            .. Enumerable.Range(0, DeathShadeSteps).SelectMany(step => new[]
            {
                Cue(new SporeSpawnCue.ShowDeathShade(DeathShadeStep: step)),
                Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
                Show(F.SporeSpawnShutTwin, FadeStepDuration),
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
    private static IEnumerable<AnimationBeat<EnemyFrameId, SporeSpawnCue>> Closing() =>
        Enumerable.Range(0, OpeningDurations.Length - 1).Reverse().Select(pose => Show(OpeningFrames[pose], PoseDuration));

    /// <summary>Selects <paramref name="movement"/> as the main-AI movement.</summary>
    private static AnimationBeat<EnemyFrameId, SporeSpawnCue> SelectMovement(SporeSpawnMovement movement) =>
        Cue(new SporeSpawnCue.SetMovement(Movement: movement));

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SporeSpawnCue> Cue(SporeSpawnCue cue) =>
        new AnimationBeat<EnemyFrameId, SporeSpawnCue>.Event(cue);

    /// <summary>Pauses on the shown frame for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SporeSpawnCue> Wait(int duration) =>
        new AnimationBeat<EnemyFrameId, SporeSpawnCue>.Pause(duration);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SporeSpawnCue> Show(EnemyFrameId frame, int duration) =>
        new AnimationBeat<EnemyFrameId, SporeSpawnCue>.Frame(frame, duration);

}
