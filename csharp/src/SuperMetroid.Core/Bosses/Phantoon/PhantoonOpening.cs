using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's entrance: the starting flames and its wavy fade-in.</summary>
[RoomLifetime]
internal sealed class PhantoonOpening
{
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _presentation;
    /// <summary>The bosses' results of the pass, where the opening asks for the fight's door.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Phantoon's combat, whose flame behaviours the intro flames are bound to.</summary>
    private readonly PhantoonCombat _combat;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonOpening(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, PhantoonPresentation presentation, BossFrameResultBuilder frameResults,
        PhantoonCombat combat)
    {
        _combat = combat;
        _projectiles = projectiles;
        _audio = audio;
        _presentation = presentation;
        _frameResults = Ensure.NotNull(frameResults);
    }

    /// <summary>Emits the intro flames on the authored schedule before Phantoon materializes.</summary>
    internal void RunPhantoonStartingFlameSpawner(PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        _ = SpawnPhantoonStartingFlame(state, PhantoonFlameSpawns.StartingFlameStart(state.StartingFlames.NextFlame));
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlamePriority);
        state.PhaseTimer = PhantoonArenaDefinitions.StartingFlameIntervalUpdates;
        state.StartingFlames.NextFlame++;
        if (state.StartingFlames.NextFlame < PhantoonFlameSpawns.StartingFlameCount)
            return;

        state.StartingFlames.NextFlame = 0;
        state.Round.Damage = 0;
        state.Phase = PhantoonPhase.WaitBeforeActivatingStartingFlames;
        state.PhaseTimer = PhantoonArenaDefinitions.StartingFlamesActivateUpdates;
        _frameResults.PhantoonDoor = PhantoonDoorChange.FightDoor;
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    internal static void RunPhantoonStartingFlamePause(PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;
        state.PhaseTimer = PhantoonArenaDefinitions.StartingFlamesOrbitUpdates;
        state.StartingFlames.Orbiting = true;
        state.Phase = PhantoonPhase.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    internal void RunPhantoonStartingFlameOrbitWait(
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        state.StartingFlames.Orbiting = false;
        state.Translucent = true;
        state.Phase = PhantoonPhase.WavyFadeIn;
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.Show;
        state.PhaseTimer = PhantoonArenaDefinitions.WavyFadeInUpdates;
        state.Wave.Amplitude = PhantoonWaveDefinitions.IntroAmplitude;
        state.Wave.AmplitudeShrinking = false;
        state.BodyFade.Complete = false;
        _audio.RequestPhantoonMusic(MusicRequest.SelectTrack(MusicTrack.KraidAndPhantoonBattle));
        state.Wave.PhaseStep = PhantoonWaveDefinitions.PhaseStep;
        state.BodyWave.Begin(PhantoonWaveDefinitions.IntroLength);
    }

    /// <summary>Raises intro wave amplitude and reveals the body colours.</summary>
    internal void RunPhantoonWavyFadeIn(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        _presentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (PhantoonPresentation.AdvancePhantoonWaveAmplitude(
                state.Wave,
                PhantoonWaveDefinitions.IntroAmplitudeChange,
                PhantoonWaveDefinitions.IntroAmplitude))
        {
            state.Phase = PhantoonPhase.PickFirstRoundPattern;
            state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.Show;
            state.PhaseTimer = PhantoonArenaDefinitions.FirstRoundPickUpdates;
            return;
        }

        if (PhantoonPresentation.TickPhantoonPhaseTimer(state))
            state.Wave.AmplitudeShrinking = true;
    }

    /// <summary>Allocates one intro flame at its place on the circle round the body.</summary>
    /// <param name="phantoon">The encounter, whose body the flame circles.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    internal bool SpawnPhantoonStartingFlame(PhantoonEnemyState phantoon, TrigonometryAngle start)
    {
        RoomEnemyPlace body = phantoon.Body;
        EnemyProjectile? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            _combat.StartingFlame,
            SpriteGraphics.SpawnedBy(body));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.StartingFlame));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = EightEightVector<RoomPixels>.Zero;
        var orbit = new PhantoonOrbitingFlameState(phantoon, start);
        flame.State = orbit;
        PhantoonCombat.PositionPhantoonFlameAroundBody(flame, body, orbit.Angle, radius: PhantoonFlameMotionDefinitions.IntroCircleRadius);
        flame.StartMotion(PhantoonFlameMotion.IntroWaiting);
        return true;
    }
}
