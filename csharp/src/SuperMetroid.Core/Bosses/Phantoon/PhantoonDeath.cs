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

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonDeath(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyFrameContext frameContext, EnemyDeathsAndDrops deathsAndDrops, PhantoonPresentation phantoonPresentation, PhantoonPatterns patterns)
    {
        _room = room;
        _projectiles = projectiles;
        _presentation = presentation;
        _frameContext = frameContext;
        _deathsAndDrops = deathsAndDrops;
        _phantoonPresentation = phantoonPresentation;
        _patterns = patterns;
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
        state.Parts[eye.SlotIndex].DyingFadeCycles = unchecked((ushort)(state.Parts[eye.SlotIndex].DyingFadeCycles + 1));
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
        ushort x = unchecked((ushort)(body.Position.X.Whole + explosion.Offset.X));
        ushort y = unchecked((ushort)(body.Position.Y.Whole + explosion.Offset.Y));

        int before = _projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive);
        state.DeathExplosionRequests++;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), explosion.Type);
        if (_projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive) > before)
            state.DeathExplosionsSpawned++;
        state.LastCombatSoundEffect = explosion.Type == MiscDustAnimation.BigExplosion
            ? PhantoonSoundDefinitions.BigDeathExplosionSound
            : PhantoonSoundDefinitions.DeathExplosionSound;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = unchecked((ushort)(state.Parts[tentacles.SlotIndex].ExplosionIndex + 1));
        if (state.Parts[tentacles.SlotIndex].ExplosionIndex < PhantoonDeathExplosionDefinitions.Count)
            return;

        state.Parts[tentacles.SlotIndex].ExplosionIndex = PhantoonDeathExplosionDefinitions.RepeatStart;
        state.Parts[body.SlotIndex].ExplosionPass = unchecked((ushort)(state.Parts[body.SlotIndex].ExplosionPass + 1));
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
        state.Parts[state.Tentacles.SlotIndex].RequestedWaveMode = PhantoonWaveRomData.DeathMode;
        state.Wave.Begin(PhantoonWaveRomData.DeathMode);
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.DyingFadeOut;
        state.Parts[state.Eye.SlotIndex].MosaicRegister = PhantoonDeathDefinitions.InitialMosaicRegister;
        state.MosaicBlockSize = MosaicBlockSizeOf(PhantoonDeathDefinitions.InitialMosaicRegister);

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
        if (state.Parts[eye.SlotIndex].MosaicRegister != PhantoonDeathDefinitions.MosaicFinished)
        {
            // The mosaic grows one size step every sixteenth frame.
            if (_frameContext.NmiFrameCounter % 16 != 0)
                return;

            byte mosaic = unchecked((byte)state.Parts[eye.SlotIndex].MosaicRegister);
            if (mosaic == PhantoonDeathDefinitions.FinalMosaicRegister)
            {
                state.Parts[eye.SlotIndex].MosaicRegister = PhantoonDeathDefinitions.MosaicFinished;
                state.Parts[eye.SlotIndex].FadeComplete = false;
                return;
            }

            mosaic = unchecked((byte)(mosaic + PhantoonDeathDefinitions.MosaicSizeStep));
            state.Parts[eye.SlotIndex].MosaicRegister = unchecked((ushort)((SnesLittleEndian.HighByteInPlace(state.Parts[eye.SlotIndex].MosaicRegister)) | mosaic));
            state.MosaicBlockSize = MosaicBlockSizeOf(mosaic);
            return;
        }

        _patterns.AdvancePhantoonFadeOut(state, denominator: 12);
        if (state.Parts[eye.SlotIndex].FadeComplete)
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.AlmostDead;
    }

    /// <summary>
    /// The BG2 mosaic block size in screen pixels a death-mosaic step on the eye's counter shows: the counter's
    /// high nibble is the size minus one, and every step also enables BG2, so the size alone carries the display.
    /// </summary>
    internal static int MosaicBlockSizeOf(byte mosaic) => (mosaic >> PhantoonDeathDefinitions.MosaicSizeShift) + 1;

    /// <summary>Clears the visible enemy BG2 page and starts the 60-frame power delay.</summary>
    internal void ClearPhantoonDeathGraphics(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.MosaicBlockSize = 1;
        state.Parts[state.Eye.SlotIndex].WaveMode = 0;
        state.SemiTransparent = false;
        state.Parts[state.Mouth.SlotIndex].BlendingControl = PhantoonBlendingRomData.DeleteControlWord;
        state.Parts[body.SlotIndex].Function = PhantoonAiFunction.Dead;
        state.Parts[body.SlotIndex].FunctionTimer = 60;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        body.Position = body.Position.WithWhole(new(384, 128));

        ushort[] clearedTilemap = new ushort[512];
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
            state.Parts[body.SlotIndex].FunctionTimer = NativeWordCounter.Decrement(checked((ushort)state.Parts[body.SlotIndex].FunctionTimer)).Value;
            return;
        }
        if ((_frameContext.NmiFrameCounter & 3) != 0)
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
            yMask: SnesLittleEndian.Word(0, 63));
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
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.WreckedShipPowerPaletteComplete = true;
    }

    /// <summary>Ports $A7:DC5A over CGRAM colors 0..111.</summary>
    internal bool AdvanceWreckedShipPowerPalette(PhantoonPartState eye)
    {
        ushort denominator = checked((ushort)eye.FadeDenominator);
        ushort numerator = checked((ushort)eye.FadeNumerator);
        if (unchecked((ushort)(denominator + 1)) < numerator)
        {
            eye.FadeNumerator = 0;
            return true;
        }

        CgramPalette room = _presentation.BossPalettes.WreckedShipPowerOn;
        for (int color = 0; color < room.Count; color++)
        {
            Bgr555 current = room[color];
            Bgr555 target = (_room.Artwork?.PhantoonColors ?? throw new InvalidDataException(
                "Wrecked Ship power-on palette requires installed artwork.")).ResolvePowerOn(color);
            room.Write(
                color,
                PhantoonPresentation.CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.FadeNumerator = unchecked((ushort)(numerator + 1));
        return false;
    }
}
