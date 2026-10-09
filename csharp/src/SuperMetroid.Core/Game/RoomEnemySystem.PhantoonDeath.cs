using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Phantoon's complete death program from <c>$A7:D92E-$DB99</c>. The sequence deliberately
/// remains frame-driven: palette fades, explosion allocation pressure, mosaic cadence, and
/// the delayed Wrecked Ship power transition are observable gameplay, not a single host
/// "boss defeated" event.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Increase applied to the death-wave amplitude on each update.</summary>
    private const ushort PhantoonDeathWaveDelta = 0x0100;
    /// <summary>Maximum amplitude reached by the death-wave palette effect.</summary>
    private const ushort PhantoonDeathWaveMaximum = 0xf000;

    /// <summary>Moves Phantoon through the fatal swoop until his horizontal position starts the death fade sequence.</summary>
    /// <param name="body">The body slot whose swoop position and AI function are updated.</param>
    /// <param name="state">Phantoon's per-instance state, including the eye used for aim direction.</param>
    /// <param name="samus">The player position used to aim the eye and steer the swoop.</param>
    private void RunPhantoonFatalSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PointPhantoonEyeAtSamus(body, state.Eye!, samus);
        MovePhantoonInSwoop(body, state, samus, fatal: true);
        if (body.XPosition >= 96 && body.XPosition < 160)
            body.VariableF = (ushort)PhantoonAiFunction.DyingFadeInOut;
    }

    /// <summary>Alternates five fade-outs and five fade-ins at $A7:D948.</summary>
    private void RunPhantoonDyingFadeCycles(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        RoomEnemySlot eye = state.Eye!;
        if ((eye.VariableC & 1) != 0)
            AdvancePhantoonFadeIn(body, state, denominator: 12, nmiFrameCounter8);
        else
            AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);

        if (eye.VariableF == 0)
            return;
        eye.VariableF = 0;
        eye.VariableC = unchecked((ushort)(eye.VariableC + 1));
        if (unchecked((short)(eye.VariableC - 10)) < 0)
            return;

        body.VariableF = (ushort)PhantoonAiFunction.DyingExplosions;
        body.VariableE = 15;
        state.Tentacles!.VariableF = 0;
        body.VariableA = 0;
    }

    /// <summary>Runs the literal 13-entry-then-5..12-twice explosion schedule.</summary>
    private void RunPhantoonDyingExplosions(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!TickPhantoonFunctionTimer(body))
            return;

        RoomEnemySlot tentacles = state.Tentacles!;
        var (xOffset, yOffset, explosionType, delay) = PhantoonDeathExplosionDefinitions.Read(tentacles.VariableF);
        body.VariableE = delay;
        ushort x = unchecked((ushort)(body.XPosition + xOffset));
        ushort y = unchecked((ushort)(body.YPosition + yOffset));

        int before = _enemyProjectiles.Count(projectile => projectile.IsActive);
        state.DeathExplosionRequests++;
        SpawnRoomGraphicsDustExplosion(x, y, explosionType);
        if (_enemyProjectiles.Count(projectile => projectile.IsActive) > before)
            state.DeathExplosionsSpawned++;
        state.LastCombatSoundEffect = explosionType == 0x001d ? (ushort)0x0024 : (ushort)0x002b;

        tentacles.VariableF = unchecked((ushort)(tentacles.VariableF + 1));
        if (tentacles.VariableF < PhantoonDeathExplosionDefinitions.Count)
            return;

        tentacles.VariableF = PhantoonDeathExplosionDefinitions.RepeatStart;
        body.VariableA = unchecked((ushort)(body.VariableA + 1));
        if (body.VariableA >= PhantoonDeathExplosionDefinitions.PassCount)
            body.VariableF = (ushort)PhantoonAiFunction.BeginFinalWavyDeath;
    }

    /// <summary>Initializes the wave, hides the auxiliary parts, and enters Phantoon's final mosaic death phase.</summary>
    /// <param name="body">The body slot that continues producing the BG2 wave.</param>
    /// <param name="state">The boss state receiving wave, mosaic, visibility, and sound initialization.</param>
    private static void BeginPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth!;
        mouth.VariableD = 0;
        mouth.VariableE = 0;
        mouth.VariableF = PhantoonWavyPhaseDelta;
        state.Tentacles!.Parameter1 = PhantoonWaveRomData.DeathMode;
        state.Wave.Begin(PhantoonWaveRomData.DeathMode);
        body.VariableF = (ushort)PhantoonAiFunction.DyingFadeOut;
        state.Eye!.VariableC = 2;
        state.MosaicRegister = 2;

        // The body remains the BG2 producer. Eye/tentacle/mouth become invisible and ignore
        // Samus while preserving every unrelated raw property bit exactly as $DA6C does.
        ushort hiddenPartProperties = body.Properties.Replace(
            EnemyProperties.ProcessInstructions,
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        state.Eye.Properties = hiddenPartProperties;
        state.Tentacles!.Properties = hiddenPartProperties;
        mouth.Properties = hiddenPartProperties;
        state.LastCombatSoundEffect = 0x007e;
    }

    /// <summary>Advances the death-wave amplitude, expands the mosaic effect, and then completes the final fade.</summary>
    /// <param name="body">The body slot whose AI function changes when the fade reaches its endpoint.</param>
    /// <param name="state">Phantoon's wave and mosaic state updated during the death sequence.</param>
    /// <param name="nmiFrameCounter8">The frame counter used to preserve the native palette-fade cadence.</param>
    private void RunPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        AdvancePhantoonWaveAmplitude(
            state.Mouth!,
            PhantoonDeathWaveDelta,
            PhantoonDeathWaveMaximum);

        RoomEnemySlot eye = state.Eye!;
        if (eye.VariableC != 0xffff)
        {
            if ((_enemyFrameNmiFrameCounter & 0x0f) != 0)
                return;

            byte mosaic = unchecked((byte)eye.VariableC);
            if (mosaic == 0xf2)
            {
                eye.VariableC = 0xffff;
                eye.VariableF = 0;
                return;
            }

            mosaic = unchecked((byte)(mosaic + 0x10));
            eye.VariableC = unchecked((ushort)((eye.VariableC & 0xff00) | mosaic));
            state.MosaicRegister = mosaic;
            return;
        }

        AdvancePhantoonFadeOut(state, denominator: 12, nmiFrameCounter8);
        if (eye.VariableF != 0)
            body.VariableF = (ushort)PhantoonAiFunction.AlmostDead;
    }

    /// <summary>Clears the visible enemy BG2 page and starts the 60-frame power delay.</summary>
    private void ClearPhantoonDeathGraphics(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.MosaicRegister = 0;
        state.Eye!.Parameter1 = 0;
        state.SemiTransparencyLayerFlags &= 0xbfff;
        state.Mouth!.Parameter1 = 0xffff;
        body.VariableF = (ushort)PhantoonAiFunction.Dead;
        body.VariableE = 60;
        state.Eye.VariableF = 0;
        body.XPosition = 384;
        body.YPosition = 128;

        ushort[] clearedTilemap = new ushort[0x0200];
        Array.Fill(clearedTilemap, PhantoonBlankBg2Tile);
        _vram!.ExecuteWordTransfer(clearedTilemap, PhantoonBg2VramBase, wordIncrement: 1);
    }

    /// <summary>Waits through the death delay, powers on the Wrecked Ship, and applies the boss defeat and reward effects.</summary>
    /// <param name="body">The body slot used for the final arena drop and then marked deleted.</param>
    /// <param name="state">The boss state holding the power palette, persistence, door, and music requests.</param>
    /// <param name="nmiFrameCounter8">The NMI frame counter supplied by the enemy update loop.</param>
    private void ActivateWreckedShipAfterPhantoon(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        byte nmiFrameCounter8)
    {
        if (body.VariableE != 0)
        {
            body.VariableE = unchecked((ushort)(body.VariableE - 1));
            return;
        }
        if ((_enemyFrameNmiFrameCounter & 3) != 0)
            return;

        state.Eye!.VariableD = 12;
        if (!AdvanceWreckedShipPowerPalette(state.Eye))
            return;

        state.MainScreenBg2Enabled = true;

        // $A0:B95D scatters sixteen room-graphics pickups over the visible boss arena.
        // The coordinates are absolute room coordinates, not offsets from Phantoon's body.
        SpawnEnemyDropScatter(
            PhantoonBodyDefinition,
            count: 16,
            xBase: 64,
            xMask: 0x007f,
            yBase: 96,
            yMask: 0x3f00);
        state.ItemDropRequested = true;
        ushort deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye.Properties = deletedProperties;
        state.Tentacles!.Properties = deletedProperties;
        state.Mouth!.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            RequireSetAreaBossDefeated();
            state.BossDefeatPersisted = true;
        }
        state.BossDoorPlmRequest = RoomPlmHeaders.RestorePhantoonDoorAfterBossFight;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.WreckedShipPowerPaletteComplete = true;
    }

    /// <summary>Ports $A7:DC5A over CGRAM colors 0..111.</summary>
    private bool AdvanceWreckedShipPowerPalette(RoomEnemySlot eye)
    {
        ushort denominator = eye.VariableD;
        ushort numerator = eye.VariableE;
        if (unchecked((ushort)(denominator + 1)) < numerator)
        {
            eye.VariableE = 0;
            return true;
        }

        for (int color = 0; color < 112; color++)
        {
            ushort current = _cgram!.Colors[color];
            ushort target = (TileArtwork?.PhantoonColors ?? throw new InvalidDataException(
                "Wrecked Ship power-on palette requires installed artwork.")).ResolvePowerOn(color);
            _cgram.SetColor(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.VariableE = unchecked((ushort)(numerator + 1));
        return false;
    }
}
