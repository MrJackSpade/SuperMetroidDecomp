using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
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
    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _presentation;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonOpening(EnemyProjectileEngine projectiles, EnemyAudioQueue audio, PhantoonPresentation presentation)
    {
        _projectiles = projectiles;
        _audio = audio;
        _presentation = presentation;
    }

    /// <summary>Emits the intro flames on the authored schedule before Phantoon materializes.</summary>
    internal void RunPhantoonStartingFlameSpawner(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.StartingFlameRequests++;
        if (SpawnPhantoonStartingFlame(body, state.BodyState.StartingFlameIndex))
            state.StartingFlamesSpawned++;
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlamePriority);
        state.BodyState.FunctionTimer = 30;
        state.BodyState.StartingFlameIndex = WrappingCounter.IncrementCount(state.BodyState.StartingFlameIndex);
        if (state.BodyState.StartingFlameIndex < 8)
            return;

        state.BodyState.StartingFlameIndex = 0;
        state.TentaclesState.RoundDamage = 0;
        state.BodyState.Function = PhantoonAiFunction.WaitBeforeActivatingStartingFlames;
        state.BodyState.FunctionTimer = 30;
        state.BossDoorObjectRequest = RoomObjectHeaderId.DrawPhantoonDoorDuringBossFight;
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    internal static void RunPhantoonStartingFlamePause(PhantoonBodyState body)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(body))
            return;
        body.FunctionTimer = 240;
        body.StartingFlamesActive = true;
        body.Function = PhantoonAiFunction.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    internal void RunPhantoonStartingFlameOrbitWait(
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            return;

        state.BodyState.StartingFlamesActive = false;
        state.Translucent = true;
        state.BodyState.Function = PhantoonAiFunction.WavyFadeIn;
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
            state.BodyState.Function = PhantoonAiFunction.PickFirstRoundPattern;
            state.MouthState.BodyLayer = PhantoonBodyLayer.Shown;
            state.BodyState.FunctionTimer = 30;
            return;
        }

        if (PhantoonPresentation.TickPhantoonFunctionTimer(state.BodyState))
            state.MouthState.WaveAmplitudeShrinking = true;
    }

    /// <summary>Allocates one intro flame with its chosen orbit direction.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="directionIndex">The flame's orbit direction, 0 through 255; its eighth turns wrap within the orbit.</param>
    internal bool SpawnPhantoonStartingFlame(RoomEnemySlot body, int directionIndex)
    {
        EnemyProjectile? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            SpriteGraphics.SpawnedBy(body));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.StartingFlame));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = EightEightVector<RoomPixels>.Zero;
        var orbit = new PhantoonOrbitingFlameState(new TrigonometryAngle(directionIndex * 32));
        flame.State = orbit;
        PhantoonCombat.PositionPhantoonFlameAroundBody(flame, body, orbit.Angle, radius: 48);
        flame.FrameBehavior = PhantoonBehavior.PhantoonStartingFlameWaitingFrameBehavior;
        return true;
    }
}
