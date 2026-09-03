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
    private const int PhantoonDeathExplosionTable = 0xa7da1d;
    private const int WreckedShipPowerPalette = 0xa7ca61;
    private const ushort PhantoonDeathWaveDelta = 0x0100;
    private const ushort PhantoonDeathWaveMaximum = 0xf000;

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
        int entry = PhantoonDeathExplosionTable + tentacles.VariableF * 4;
        sbyte xOffset = unchecked((sbyte)_bus!.ReadByte(entry));
        sbyte yOffset = unchecked((sbyte)_bus.ReadByte(entry + 1));
        ushort explosionType = _bus.ReadByte(entry + 2);
        body.VariableE = _bus.ReadByte(entry + 3);
        ushort x = unchecked((ushort)(body.XPosition + xOffset));
        ushort y = unchecked((ushort)(body.YPosition + yOffset));

        int before = _enemyProjectiles.Count(projectile => projectile.IsActive);
        state.DeathExplosionRequests++;
        SpawnRoomGraphicsDustExplosion(x, y, explosionType);
        if (_enemyProjectiles.Count(projectile => projectile.IsActive) > before)
            state.DeathExplosionsSpawned++;
        state.LastCombatSoundEffect = explosionType == 0x001d ? (ushort)0x0024 : (ushort)0x002b;

        tentacles.VariableF = unchecked((ushort)(tentacles.VariableF + 1));
        if (tentacles.VariableF < 13)
            return;

        tentacles.VariableF = 5;
        body.VariableA = unchecked((ushort)(body.VariableA + 1));
        if (body.VariableA >= 3)
            body.VariableF = (ushort)PhantoonAiFunction.BeginFinalWavyDeath;
    }

    private static void BeginPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth!;
        mouth.VariableD = 0;
        mouth.VariableE = 0;
        mouth.VariableF = PhantoonWavyPhaseDelta;
        body.VariableF = (ushort)PhantoonAiFunction.DyingFadeOut;
        state.Eye!.VariableC = 2;
        state.MosaicRegister = 2;

        // The body remains the BG2 producer. Eye/tentacle/mouth become invisible and ignore
        // Samus while preserving every unrelated raw property bit exactly as $DA6C does.
        ushort hiddenPartProperties = unchecked((ushort)((body.Properties & 0xdfff) | 0x0500));
        state.Eye.Properties = hiddenPartProperties;
        state.Tentacles!.Properties = hiddenPartProperties;
        mouth.Properties = hiddenPartProperties;
        state.LastCombatSoundEffect = 0x007e;
    }

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
            if ((nmiFrameCounter8 & 0x0f) != 0)
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
        if ((nmiFrameCounter8 & 3) != 0)
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
            ushort target = ReadWord(_bus!, WreckedShipPowerPalette + color * 2);
            _cgram.SetColor(
                color,
                CalculatePhantoonTransitionColor(numerator, denominator, current, target));
        }
        eye.VariableE = unchecked((ushort)(numerator + 1));
        return false;
    }
}
