using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;
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
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.StartingFlameRequests++;
        if (SpawnPhantoonStartingFlame(body, unchecked((byte)state.Parts[body.SlotIndex].StartingFlameIndex)))
            state.StartingFlamesSpawned++;
        _audio.QueueEnemySound(PhantoonSoundDefinitions.StartingFlame, PhantoonSoundDefinitions.StartingFlameQueueCapacity);
        state.Parts[body.SlotIndex].FunctionTimer = 30;
        state.Parts[body.SlotIndex].StartingFlameIndex = WrappingCounter.IncrementWord(state.Parts[body.SlotIndex].StartingFlameIndex);
        if (unchecked((short)(state.Parts[body.SlotIndex].StartingFlameIndex - 8)) < 0)
            return;

        state.Parts[body.SlotIndex].StartingFlameIndex = 0;
        state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WaitBeforeActivatingStartingFlames;
        state.Parts[body.SlotIndex].FunctionTimer = 30;
        state.BossDoorPlmRequest = PlmHeaderId.DrawPhantoonDoorDuringBossFight;
    }

    /// <summary>Holds the body between intro flame spawns.</summary>
    internal static void RunPhantoonStartingFlamePause(PhantoonPartState body)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(body))
            return;
        body.FunctionTimer = 240;
        body.StartingFlamesActive = true;
        body.Function = PhantoonAiFunction.WaitForStartingFlamesToDisappear;
    }

    /// <summary>Waits for the initial flames to finish orbiting before the next phase.</summary>
    internal static void RunPhantoonStartingFlameOrbitWait(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        state.Parts[body.SlotIndex].StartingFlamesActive = false;
        state.SemiTransparent = true;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.WavyFadeIn;
        state.Parts[state.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.RoomDefault;
        state.Parts[body.SlotIndex].FunctionTimer = 120;
        state.Parts[state.Mouth.SlotIndex].WaveAmplitude = PhantoonBehavior.PhantoonIntroMaximumAmplitude;
        state.Parts[state.Mouth.SlotIndex].WaveAmplitudeShrinking = false;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.MusicRequest = MusicRequest.SelectTrack(5);
        state.Parts[state.Mouth.SlotIndex].WavyPhaseDelta = PhantoonBehavior.PhantoonWavyPhaseDelta;
        state.Parts[state.Tentacles.SlotIndex].RequestedWaveMode = PhantoonWaveDefinitions.IntroMode;
        state.Wave.Begin(PhantoonWaveDefinitions.IntroMode);
    }

    /// <summary>Raises intro wave amplitude and reveals the body palette.</summary>
    internal void RunPhantoonWavyFadeIn(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        _presentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        if (PhantoonPresentation.AdvancePhantoonWaveAmplitude(
                state.Parts[state.Mouth.SlotIndex],
                PhantoonBehavior.PhantoonIntroAmplitudeDelta,
                PhantoonBehavior.PhantoonIntroMaximumAmplitude))
        {
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.PickFirstRoundPattern;
            state.Parts[state.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.RoomDefault;
            state.Parts[body.SlotIndex].FunctionTimer = 30;
            return;
        }

        if (PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            state.Parts[state.Mouth.SlotIndex].WaveAmplitudeShrinking = true;
    }

    /// <summary>Allocates one intro flame with its chosen orbit direction.</summary>
    /// <param name="body">Phantoon's body.</param>
    /// <param name="directionIndex">The flame's orbit direction, 0 through 255; its eighth turns wrap within the orbit.</param>
    internal bool SpawnPhantoonStartingFlame(RoomEnemySlot body, int directionIndex)
    {
        RoomEnemyProjectileSlot? flame = _projectiles.AllocateEnemyProjectile();
        if (flame is null)
            return false;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            flame,
            RoomEnemyProjectileKind.PhantoonStartingFlame,
            unchecked((ushort)(body.PaletteIndex | body.TileIndexBase)));
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.StartingFlame));
        flame.Position = flame.Position.WithFractionX(0).WithFractionY(0);
        flame.Velocity = new(new SignedEightEight(0), new SignedEightEight(0));
        flame.Variable0 = unchecked((byte)(directionIndex * 32));
        PhantoonCombat.PositionPhantoonFlameAroundBody(flame, body, checked(new ByteAngle(unchecked((byte)((ushort)flame.Variable0)))), radius: 48);
        flame.PreInstruction = PhantoonBehavior.PhantoonStartingFlameWaitingPreInstruction;
        return true;
    }
}
