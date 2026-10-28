using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's death: the fatal swoop, the fades, explosions and wavy mosaic, and Wrecked Ship's power returning.</summary>
[RoomLifetime]
internal sealed class PhantoonDeath
{
    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies body palettes and BG2 tilemap changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Phantoon's fades, health colors, wave amplitude, programs and function timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;
    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    private readonly PhantoonPatterns _patterns;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonDeath(PhantoonDeathDependencies dependencies)
    {
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _phantoonPresentation = dependencies.PhantoonPresentation;
        _patterns = dependencies.Patterns;
        _bossArtwork = dependencies.BossArtwork;
    }

    /// <summary>Tracks Samus during the fatal swoop and advances once the body reaches the center band.</summary>
    internal static void RunPhantoonFatalSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PhantoonPatterns.PointPhantoonEyeAtSamus(body, state, samus);
        PhantoonPatterns.MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.DeathHeight);
        if (body.Position.X.Whole is >= 96 and < 160)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingFadeInOut;
    }

    /// <summary>Alternates five fade-outs and five fade-ins at $A7:D948.</summary>
    internal void RunPhantoonDyingFadeCycles(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot eye = state.Eye;
        if ((state.Parts[eye.SlotIndex].DyingFadeCycles & 1) != 0)
            _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        else
            _patterns.AdvancePhantoonFadeOut(state, denominator: 12);

        if (!state.Parts[eye.SlotIndex].FadeComplete)
            return;
        state.Parts[eye.SlotIndex].FadeComplete = false;
        state.Parts[eye.SlotIndex].DyingFadeCycles = WrappingCounter.IncrementWord(state.Parts[eye.SlotIndex].DyingFadeCycles);
        if (unchecked((short)(state.Parts[eye.SlotIndex].DyingFadeCycles - 10)) < 0)
            return;

        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingExplosions;
        state.Parts[body.SlotIndex].FunctionTimer = 15;
        state.Parts[state.Tentacles.SlotIndex].ExplosionIndex = 0;
        state.Parts[body.SlotIndex].ExplosionPass = 0;
    }

    /// <summary>Runs the literal 13-entry-then-5..12-twice explosion schedule.</summary>
    internal void RunPhantoonDyingExplosions(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonFunctionTimer(state.Parts[body.SlotIndex]))
            return;

        RoomEnemySlot tentacles = state.Tentacles;
        PhantoonDeathExplosionDefinition explosion = PhantoonDeathExplosionDefinitions.Read(state.Parts[tentacles.SlotIndex].ExplosionIndex);
        state.Parts[body.SlotIndex].FunctionTimer = explosion.Delay;
        Position16<RoomPixels> position = body.Position.Whole + explosion.Offset;

        int before = _projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
        state.DeathExplosionRequests++;
        _projectiles.SpawnRoomGraphicsDustExplosion(position, explosion.Type);
        if (_projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive) > before)
            state.DeathExplosionsSpawned++;
        state.LastCombatSoundEffect = explosion.Type == MiscDustAnimation.BigExplosion
            ? PhantoonSoundDefinitions.BigDeathExplosionSound
            : PhantoonSoundDefinitions.DeathExplosionSound;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = WrappingCounter.IncrementWord(state.Parts[tentacles.SlotIndex].ExplosionIndex);
        if (state.Parts[tentacles.SlotIndex].ExplosionIndex < PhantoonDeathExplosionDefinitions.Count)
            return;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = PhantoonDeathExplosionDefinitions.RepeatStart;
        state.Parts[body.SlotIndex].ExplosionPass = WrappingCounter.IncrementWord(state.Parts[body.SlotIndex].ExplosionPass);
        if (state.Parts[body.SlotIndex].ExplosionPass >= PhantoonDeathExplosionDefinitions.PassCount)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.BeginFinalWavyDeath;
    }

    /// <summary>Starts the distorted BG2 and mosaic stages after the fatal swoop.</summary>
    internal static void BeginPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        state.Parts[mouth.SlotIndex].WaveAmplitude = 0;
        state.Parts[mouth.SlotIndex].WaveAmplitudeShrinking = false;
        state.Parts[mouth.SlotIndex].WavyPhaseDelta = PhantoonBehavior.PhantoonWavyPhaseDelta;
        state.Parts[state.Tentacles.SlotIndex].RequestedWaveMode = PhantoonWaveDefinitions.DeathMode;
        state.Wave.Begin(PhantoonWaveDefinitions.DeathMode);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingFadeOut;
        state.Parts[state.Eye.SlotIndex].DeathMosaicSize = PhantoonDeathDefinitions.InitialMosaicSize;
        state.Parts[state.Eye.SlotIndex].DeathMosaicFinished = false;
        state.MosaicBlockSize = PhantoonDeathDefinitions.InitialMosaicSize;

        // The body remains the BG2 producer. Eye/tentacle/mouth become invisible and ignore
        // Samus while preserving every unrelated raw property bit exactly as $DA6C does.
        EnemyPropertyWord hiddenPartProperties = body.Properties.Replace(
            EnemyProperties.ProcessInstructions,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.Eye.Properties = hiddenPartProperties;
        state.Tentacles.Properties = hiddenPartProperties;
        mouth.Properties = hiddenPartProperties;
        state.LastCombatSoundEffect = PhantoonSoundDefinitions.DyingCry;
    }

    /// <summary>Advances death-wave and mosaic counters before room activation.</summary>
    internal void RunPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        PhantoonPresentation.AdvancePhantoonWaveAmplitude(
            state.Parts[state.Mouth.SlotIndex],
            PhantoonBehavior.PhantoonDeathWaveDelta,
            PhantoonBehavior.PhantoonDeathWaveMaximum);

        RoomEnemySlot eye = state.Eye;
        PhantoonPartState eyeWords = state.Parts[eye.SlotIndex];
        if (!eyeWords.DeathMosaicFinished)
        {
            if (_frameContext.FrameCounter % PhantoonDeathDefinitions.MosaicGrowthInterval != 0)
                return;

            if (eyeWords.DeathMosaicSize == PhantoonDeathDefinitions.FinalMosaicSize)
            {
                eyeWords.DeathMosaicFinished = true;
                eyeWords.FadeComplete = false;
                return;
            }

            eyeWords.DeathMosaicSize++;
            state.MosaicBlockSize = eyeWords.DeathMosaicSize;
            return;
        }

        _patterns.AdvancePhantoonFadeOut(state, denominator: 12);
        if (state.Parts[eye.SlotIndex].FadeComplete)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.AlmostDead;
    }


    /// <summary>Clears the visible enemy BG2 page and starts the 60-frame power delay.</summary>
    internal void ClearPhantoonDeathGraphics(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.MosaicBlockSize = 1;
        state.Parts[state.Eye.SlotIndex].WaveMode = 0;
        state.SemiTransparent = false;
        state.Parts[state.Mouth.SlotIndex].LayerControl = PhantoonLayerControl.Removed;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Dead;
        state.Parts[body.SlotIndex].FunctionTimer = 60;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        body.Position = body.Position.WithWhole(new(384, 128));

        var clearedTilemap = new BackgroundTile[512];
        Array.Fill(clearedTilemap, PhantoonBg2FrameDefinitions.BlankTile);
        _presentation.RequireBg2Image().Write(0, clearedTilemap);
    }

    /// <summary>Enables the cleared Wrecked Ship room and its post-boss state.</summary>
    internal void ActivateWreckedShipAfterPhantoon(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (state.Parts[body.SlotIndex].FunctionTimer != 0)
        {
            state.Parts[body.SlotIndex].FunctionTimer = WrappingCounter.Decrement(checked((ushort)state.Parts[body.SlotIndex].FunctionTimer)).Value;
            return;
        }
        if ((_frameContext.FrameCounter & 3) != 0)
            return;

        state.Parts[state.Eye.SlotIndex].FadeDenominator = 12;
        if (!AdvanceWreckedShipPowerPalette(state.Parts[state.Eye.SlotIndex]))
            return;

        state.MainScreenBg2Enabled = true;

        // $A0:B95D scatters sixteen room-graphics pickups over the visible boss arena.
        // The coordinates are absolute room coordinates, not offsets from Phantoon's body.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.PhantoonBody,
            count: 16,
            xBase: 64,
            xMask: 127,
            yBase: 96,
            yMask: 63);
        state.ItemDropRequested = true;
        EnemyPropertyWord deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye.Properties = deletedProperties;
        state.Tentacles.Properties = deletedProperties;
        state.Mouth.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        state.BossDoorPlmRequest = PlmHeaderId.RestorePhantoonDoorAfterBossFight;
        state.MusicRequest = MusicRequest.SelectTrack(3);
        state.WreckedShipPowerPaletteComplete = true;
    }

    /// <summary>Ports $A7:DC5A over CGRAM colors 0..111.</summary>
    internal bool AdvanceWreckedShipPowerPalette(PhantoonPartState eye)
    {
        int denominator = eye.FadeDenominator;
        int numerator = eye.FadeNumerator;
        if (denominator + 1 < numerator)
        {
            eye.FadeNumerator = 0;
            return true;
        }

        ColourRun room = PhantoonRoomColourDefinitions.PowerOn.In(_presentation.BackgroundSets);
        for (int color = 0; color < room.Count; color++)
        {
            RgbColor current = room[color];
            RgbColor target = (_bossArtwork.Artwork?.PhantoonColors ?? throw new InvalidDataException(
                "Wrecked Ship power-on palette requires installed artwork.")).ResolvePowerOn(color);
            room.Write(
                color,
                PhantoonPresentation.CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.FadeNumerator = numerator + 1;
        return false;
    }
}
