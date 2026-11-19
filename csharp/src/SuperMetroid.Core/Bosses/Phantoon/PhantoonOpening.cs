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
    /// <summary>Phantoon's fades, health colors, wave amplitude and animations, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _presentation;
    /// <summary>The bosses' results of the pass, where the opening asks for the fight's door.</summary>
    private readonly EnemyPassEvents _passEvents;
    /// <summary>Phantoon's combat, whose flame behaviours the intro flames are bound to.</summary>
    private readonly PhantoonCombat _combat;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonOpening(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, PhantoonPresentation presentation, EnemyPassEvents passEvents,
        PhantoonCombat combat)
    {
        _combat = combat;
        _projectiles = projectiles;
        _audio = audio;
        _presentation = presentation;
        _passEvents = Ensure.NotNull(passEvents);
    }

    /// <summary>Emits the intro flames on the authored schedule before Phantoon materializes.</summary>
    internal void RunPhantoonStartingFlameSpawner(PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        _ = SpawnPhantoonStartingFlame(state, state.StartingFlames.NextFlame.Entry);
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlamePriority);
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.StartingFlameIntervalUpdates);
        if (state.StartingFlames.NextFlame.Following is { } next)
        {
            state.StartingFlames.NextFlame = next;
            return;
        }

        state.StartingFlames.NextFlame = PhantoonStartingFlamesState.First;
        state.Round.Damage = 0;
        state.Phase = PhantoonPhase.WaitBeforeActivatingStartingFlames;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.StartingFlamesActivateUpdates);
        _passEvents.Publish(new PhantoonDoorChanged(PhantoonDoorChange.FightDoor));
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    internal static void RunPhantoonStartingFlamePause(PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.StartingFlamesOrbitUpdates);
        state.StartingFlames.Orbiting = true;
        state.Phase = PhantoonPhase.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    internal void RunPhantoonStartingFlameOrbitWait(
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        state.StartingFlames.Orbiting = false;
        state.Translucent = true;
        state.Phase = PhantoonPhase.WavyFadeIn;
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.Show;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.WavyFadeInUpdates);
        state.Wave.Amplitude = PhantoonWaveDefinitions.IntroAmplitude;
        state.Wave.AmplitudeShrinking = false;
        state.BodyFade.Resume();
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
            state.PhaseWait.LastFor(PhantoonArenaDefinitions.FirstRoundPickUpdates);
            return;
        }

        if (state.PhaseWait.StepRunsOut())
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
        flame.StartAnimation(new PhantoonFlameAnimation(_combat.StartingFlame, PhantoonFlameSegment.StartingFlame));
        flame.Position = flame.Position.WithoutFractions();
        flame.Velocity = PixelVelocity<RoomPixels>.Zero;
        var orbit = new PhantoonOrbitingFlameState(phantoon, start);
        flame.State = orbit;
        PhantoonCombat.PositionPhantoonFlameAroundBody(flame, body, orbit.Angle, radius: PhantoonFlameMotionDefinitions.IntroCircleRadius);
        flame.StartMotion(_combat.StartingFlame, PhantoonFlameMotion.IntroWaiting);
        return true;
    }
}
