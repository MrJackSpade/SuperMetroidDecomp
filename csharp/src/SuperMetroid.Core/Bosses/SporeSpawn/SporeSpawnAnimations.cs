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

    /// <summary>How wide he sways as the fight starts.</summary>
    private static readonly SporeSpawnSwayRadius FightSwayRadius = new(64);

    /// <summary>The head opening (#2331): its poses from cracked to fully open, each for its own dwell.</summary>
    private static readonly AnimationBeat<EnemyFrameId, SporeSpawnCue>[] Opening =
    [
        Show(F.SporeSpawnCracked, 8),
        Show(F.SporeSpawnAjar, 8),
        Show(F.SporeSpawnParting, 8),
        Show(F.SporeSpawnHalfOpen, 7),
        Show(F.SporeSpawnWideOpening, 7),
        Show(F.SporeSpawnNearlyOpen, 6),
        Show(F.SporeSpawnOpen, 1),
    ];

    /// <summary>
    /// The head closing (#2331): the opening's poses back from nearly open to cracked, each for the closing's dwell. The
    /// fight's closing and his death's both show it.
    /// </summary>
    private static readonly AnimationBeat<EnemyFrameId, SporeSpawnCue>[] Closing =
    [
        Show(F.SporeSpawnNearlyOpen, PoseDuration),
        Show(F.SporeSpawnWideOpening, PoseDuration),
        Show(F.SporeSpawnHalfOpen, PoseDuration),
        Show(F.SporeSpawnParting, PoseDuration),
        Show(F.SporeSpawnAjar, PoseDuration),
        Show(F.SporeSpawnCracked, PoseDuration),
    ];

    /// <summary>Every body animation.</summary>
    internal static AnimationSegments<SporeSpawnBodySegment, EnemyFrameId, SporeSpawnCue> Segments { get; } = new(
        new Dictionary<SporeSpawnBodySegment, AnimationBeat<EnemyFrameId, SporeSpawnCue>[]>
    {
        [P.InitialDead] =
        [
            Cue(new SporeSpawnCue.ShowDeathShade(DeathShadeStep: SporeSpawnDeathFadeStep.Husk)),
            SelectMovement(SporeSpawnMovement.Idle),
            Show(F.SporeSpawnHusk, 1),
        ],
        [P.InitialAlive] =
        [
            Show(F.SporeSpawnShut, DescentWait),
            SelectMovement(SporeSpawnMovement.Descending),
            Show(F.SporeSpawnShut, 1),
        ],
        [P.FightStarted] =
        [
            Cue(new SporeSpawnCue.SetSway(Radius: FightSwayRadius, Step: SporeSpawnArenaDefinitions.SwayStep)),
            SelectMovement(SporeSpawnMovement.Moving),
            Show(F.SporeSpawnShut, FirstSway),
        ],
        [P.OpenAndStop] =
        [
            Cue(new SporeSpawnCue.SetSporeGeneration(SporeGenerationSuppressed: true)),
            Cue(new SporeSpawnCue.QueueSound(Sound: SporeSpawnSoundDefinitions.Opening)),
            Show(F.SporeSpawnShut, 1),
            .. Opening,
            Cue(new SporeSpawnCue.Signal(K.ClearDamagedFlag)),
            SelectMovement(SporeSpawnMovement.Idle),
        ],
        [P.OpenPulse] =
        [
            Show(F.SporeSpawnOpenCoreGlowing, PoseDuration),
            Show(F.SporeSpawnOpenCoreBright, PoseDuration),
            Show(F.SporeSpawnOpen, PoseDuration),
            Show(F.SporeSpawnOpenCoreBright, PoseDuration),
        ],
        [P.CloseAndMove] =
        [
            .. Closing,
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
            Show(F.SporeSpawnNearlyOpen, 1),
            SelectMovement(SporeSpawnMovement.Dying),
        ],
        [P.DyingExplosions] =
        [
            Show(F.SporeSpawnNearlyOpen, 1),
            Cue(new SporeSpawnCue.Signal(K.SpawnDyingExplosion)),
            Wait(DeathBurstWait),
        ],
        [P.DeathClosing] =
        [
            .. Closing,
            Show(F.SporeSpawnHusk, 1),
            Cue(new SporeSpawnCue.Signal(K.Harden)),
        ],
        [P.Hardening] =
        [
            Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
            Wait(DeathBurstWait),
        ],
        [P.DeathFade] =
        [
            .. FadeStep(SporeSpawnDeathFadeStep.All[0]),
            .. FadeStep(SporeSpawnDeathFadeStep.All[1]),
            .. FadeStep(SporeSpawnDeathFadeStep.All[2]),
            .. FadeStep(SporeSpawnDeathFadeStep.All[3]),
            .. FadeStep(SporeSpawnDeathFadeStep.All[4]),
            .. FadeStep(SporeSpawnDeathFadeStep.All[5]),
            .. FadeStep(SporeSpawnDeathFadeStep.Husk),
            Cue(new SporeSpawnCue.Signal(K.RequestDeathDrops)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SporeSpawnBodySegment, Animation<SporeSpawnBodySegment>> StartingAt { get; } = new Dictionary<SporeSpawnBodySegment, Animation<SporeSpawnBodySegment>>
    {
        [P.InitialDead] = new([Whole(P.InitialDead)], AnimationEnd.Holds),
        [P.InitialAlive] = new([Whole(P.InitialAlive)], AnimationEnd.Holds),
        [P.FightStarted] = new([Whole(P.FightStarted), Whole(P.OpenAndStop), Repeat(P.OpenPulse, OpenPulseRepeats), Whole(P.CloseAndMove)], AnimationEnd.LoopsFrom(1)),
        [P.OpenAndStop] = new([Whole(P.OpenAndStop), Repeat(P.OpenPulse, OpenPulseRepeats), Whole(P.CloseAndMove)], AnimationEnd.LoopsFrom(0)),
        [P.CloseAndMove] = new([Whole(P.CloseAndMove), Whole(P.OpenAndStop), Repeat(P.OpenPulse, OpenPulseRepeats)], AnimationEnd.LoopsFrom(0)),
        [P.Death] = new([Whole(P.Death), Repeat(P.DyingExplosions, DeathBurstRepeats), Whole(P.DeathClosing), Repeat(P.Hardening, DeathBurstRepeats), Whole(P.DeathFade)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SporeSpawnBodySegment> StartingWith(SporeSpawnBodySegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<SporeSpawnBodySegment> Whole(SporeSpawnBodySegment segment) => Repeat(segment, 1);

    /// <summary><paramref name="plays"/> plays of every beat of <paramref name="segment"/> (#2235).</summary>
    private static AnimationPhase<SporeSpawnBodySegment> Repeat(SporeSpawnBodySegment segment, int plays) =>
        new(segment, 0, Segments.Beats(segment).Count, plays);

    /// <summary>
    /// One step of the death fade (#2331): the step's shade, a hardening dust cloud, and the husk held for the step's
    /// dwell.
    /// </summary>
    private static AnimationBeat<EnemyFrameId, SporeSpawnCue>[] FadeStep(SporeSpawnDeathFadeStep step) =>
    [
        Cue(new SporeSpawnCue.ShowDeathShade(DeathShadeStep: step)),
        Cue(new SporeSpawnCue.Signal(K.SpawnHardeningDust)),
        Show(F.SporeSpawnHusk, FadeStepDuration),
    ];

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
