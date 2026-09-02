namespace SuperMetroid.Core.Game;

/// <summary>
/// Spore Spawn's private bank-$A5 instruction callbacks. The boss's authored instruction
/// lists own its open/close cadence, movement-state changes, palette animation, explosions,
/// and drop burst; keeping those operations here makes the ROM list remain the timeline.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const int SporeSpawnHealthPaletteSource = 0xa5e379;
    private const int SporeSpawnDeathSpritePaletteSource = 0xa5e3f9;
    private const int SporeSpawnDeathLevelPaletteSource = 0xa5e4f9;
    private const int SporeSpawnDeathBackgroundPaletteSource = 0xa5e5d9;

    private bool TryProcessSporeSpawnInstruction(
        RoomEnemySlot body,
        ushort opcode,
        ref ushort cursor)
    {
        if (body.EnemyDefinitionPointer != SporeSpawnDefinition)
            return false;

        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        ushort next = unchecked((ushort)(cursor + 2));
        switch (opcode)
        {
            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_IncreaseMaxXRadius:
                if (unchecked((short)(state.MaximumXRadius - 40)) < 0)
                    state.MaximumXRadius = unchecked((ushort)(state.MaximumXRadius + 8));
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_ClearDamagedFlag:
                state.DamagedFlag = 0;
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta:
                state.MaximumXRadius = ReadWord(_bus!, 0xa50000 | next);
                state.AngleDelta = ReadWord(_bus!, 0xa50000 | unchecked((ushort)(next + 2)));
                cursor = unchecked((ushort)(next + 4));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_SporeGenerationFlagInY:
                state.SporeGenerationFlag = ReadWord(_bus!, 0xa50000 | next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_Harden:
                body.XPosition = SporeSpawnDeathCenterX;
                body.YPosition = SporeSpawnDeathCenterY;
                body.Properties = unchecked((ushort)((body.Properties | 0xa000) & 0xfbff));
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6:
                LastSporeSpawnSoundEffectLibrary2 = ReadWord(_bus!, 0xa50000 | next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine:
                RequestSporeSpawnDeathDrops(state);
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_FunctionInY:
                state.Function = (SporeSpawnFunction)ReadWord(_bus!, 0xa50000 | next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_LoadDeathSequencePalette:
                LoadSporeSpawnDeathPalette(
                    ReadWord(_bus!, 0xa50000 | next),
                    targetOnly: false);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_LoadDeathSequenceTargetPalette:
                LoadSporeSpawnDeathPalette(
                    ReadWord(_bus!, 0xa50000 | next),
                    targetOnly: true);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_SpawnHardeningDustCloud:
                SpawnSporeSpawnHardeningDust(state);
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_SpawnDyingExplosion:
                SpawnSporeSpawnDyingExplosion(state);
                cursor = next;
                return true;

            default:
                return false;
        }
    }

    /// <summary>Ports <c>SporeSpawn_Func_7</c> at $A5:EE4A.</summary>
    private void LoadSporeSpawnHealthPalette(ushort sourceByteOffset)
    {
        for (int color = 0; color < 16; color++)
        {
            _cgram!.SetColor(
                144 + color,
                ReadWord(_bus!, SporeSpawnHealthPaletteSource + sourceByteOffset + color * 2));
        }
    }

    private void LoadSporeSpawnDeathPalette(ushort sourceByteOffset, bool targetOnly)
    {
        SporeSpawnEnemyState state = _sporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn palette instruction ran without the boss state.");
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathSpritePaletteSource,
            sourceByteOffset,
            destination: 144,
            state,
            targetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathLevelPaletteSource,
            sourceByteOffset,
            destination: 64,
            state,
            targetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathBackgroundPaletteSource,
            sourceByteOffset,
            destination: 112,
            state,
            targetOnly);
    }

    private void CopySporeSpawnPaletteRow(
        int source,
        ushort sourceByteOffset,
        int destination,
        SporeSpawnEnemyState state,
        bool targetOnly)
    {
        for (int color = 0; color < 16; color++)
        {
            ushort value = ReadWord(_bus!, source + sourceByteOffset + color * 2);
            if (targetOnly)
                state.MutableTargetPalette[destination + color] = value;
            else
                _cgram!.SetColor(destination + color, value);
        }
    }

    private void SpawnSporeSpawnHardeningDust(SporeSpawnEnemyState state)
    {
        ushort random = _nextRandom!();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(state.Body.XPosition + (random & 0x007f) - 64)),
            unchecked((ushort)(state.Body.YPosition + ((random & 0x7f00) >> 8) - 64)),
            animationIndex: 0x0015);
        LastSporeSpawnSoundEffectLibrary2 = 0x0029;
    }

    private void SpawnSporeSpawnDyingExplosion(SporeSpawnEnemyState state)
    {
        ushort random = _nextRandom!();
        _ = SpawnRoomSpriteObject(
            unchecked((ushort)(state.Body.XPosition + (random & 0x007f) - 64)),
            unchecked((ushort)(state.Body.YPosition + ((random & 0x3f00) >> 8) - 32)),
            RoomSpriteObjectKind.SporeSpawnDyingExplosion,
            graphicsIndex: 0);
        LastSporeSpawnSoundEffectLibrary2 = 0x0025;
    }

    private void RequestSporeSpawnDeathDrops(SporeSpawnEnemyState state)
    {
        for (int drop = 0; drop < 16; drop++)
        {
            ushort random = _nextRandom!();
            ushort x = unchecked((ushort)((random & 0x007f) + 64));
            ushort y = unchecked((ushort)(((random & 0x3f00) >> 8) + 528));
            _sporeSpawnDropRequests.Add(new SporeSpawnDropRequest(x, y));
            SpawnEnemyDropFromEnemyHeader(x, y, SporeSpawnDefinition);
        }
        state.DeathDropRequested = true;
    }
}
