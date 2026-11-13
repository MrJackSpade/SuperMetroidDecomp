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
    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and function timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _presentation;
    /// <summary>The bosses' results of the pass, where the opening asks for the fight's door.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonOpening(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, PhantoonPresentation presentation, BossFrameResultBuilder frameResults)
    {
        _projectiles = projectiles;
        _audio = audio;
        _presentation = presentation;
        _frameResults = Ensure.NotNull(frameResults);
    }

    /// <summary>Emits the intro flames on the authored schedule before Phantoon materializes.</summary>
    internal void RunPhantoonStartingFlameSpawner(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        _ = SpawnPhantoonStartingFlame(body, PhantoonFlameSpawns.StartingFlameStart(state.BodyState.StartingFlameIndex));
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlamePriority);
        state.BodyState.FunctionTimer = 30;
        state.BodyState.StartingFlameIndex = WrappingCounter.IncrementCount(state.BodyState.StartingFlameIndex);
        if (state.BodyState.StartingFlameIndex < 8)
            return;

        state.BodyState.StartingFlameIndex = 0;
        state.TentaclesState.RoundDamage = 0;
        state.BodyState.Phase = PhantoonPhase.WaitBeforeActivatingStartingFlames;
        state.BodyState.FunctionTimer = 30;
        _frameResults.PhantoonDoor = PhantoonDoorChange.FightDoor;
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    internal static void RunPhantoonStartingFlamePause(PhantoonBodyState body)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(body))
            return;
        body.FunctionTimer = 240;
        body.StartingFlamesActive = true;
        body.Phase = PhantoonPhase.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    internal void RunPhantoonStartingFlameOrbitWait(
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.BodyState.StartingFlamesActive = false;
        state.Translucent = true;
        state.BodyState.Phase = PhantoonPhase.WavyFadeIn;
        state.MouthState.BodyLayer = PhantoonBodyLayer.Shown;
        state.BodyState.FunctionTimer = 120;
        state.MouthState.WaveAmplitude = PhantoonBehavior.PhantoonIntroMaximumAmplitude;
        state.MouthState.WaveAmplitudeShrinking = false;
        state.EyeState.FadeComplete = false;
        _audio.RequestPhantoonMusic(MusicRequest.SelectTrack(MusicTrack.FirstSetSong));
        state.MouthState.WavyPhaseDelta = PhantoonBehavior.PhantoonWavyPhaseDelta;
        state.BodyWave.Begin(PhantoonWaveDefinitions.IntroLength);
    }

    /// <summary>Raises intro wave amplitude and reveals the body palette.</summary>
    internal void RunPhantoonWavyFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _presentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (PhantoonPresentation.AdvancePhantoonWaveAmplitude(
                state.MouthState,
                PhantoonBehavior.PhantoonIntroAmplitudeDelta,
                PhantoonBehavior.PhantoonIntroMaximumAmplitude))
        {
            state.BodyState.Phase = PhantoonPhase.PickFirstRoundPattern;
            state.MouthState.BodyLayer = PhantoonBodyLayer.Shown;
            state.BodyState.FunctionTimer = 30;
            return;
        }

        if (PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            state.MouthState.WaveAmplitudeShrinking = true;
    }

    /// <summary>Allocates one intro flame at its place on the circle round the body.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="start">The flame's starting angle round the body.</param>
    internal bool SpawnPhantoonStartingFlame(RoomEnemySlot body, TrigonometryAngle start)
    {
        EnemyProjectile? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            SpriteGraphics.SpawnedBy(body));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.StartingFlame));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = EightEightVector<RoomPixels>.Zero;
        var orbit = new PhantoonOrbitingFlameState(start);
        flame.State = orbit;
        PhantoonCombat.PositionPhantoonFlameAroundBody(flame, body, orbit.Angle, radius: 48);
        flame.FrameBehavior = PhantoonBehavior.PhantoonStartingFlameWaitingFrameBehavior;
        return true;
    }
}
