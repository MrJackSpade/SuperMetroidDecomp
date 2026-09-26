namespace SuperMetroid.Core.Game;

/// <summary>
/// Spore Spawn's private bank-$A5 instruction callbacks. The boss's authored instruction
/// lists own its open/close cadence, movement-state changes, palette animation, explosions,
/// and drop burst; keeping those operations here makes the ROM list remain the timeline.
/// </summary>
public sealed partial class RoomEnemySystem
{
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
                state.MaximumXRadius = ReadEnemyInstructionMechanicsWord(body, next);
                state.AngleDelta = ReadEnemyInstructionMechanicsWord(
                    body,
                    unchecked((ushort)(next + 2)));
                cursor = unchecked((ushort)(next + 4));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_SporeGenerationFlagInY:
                state.SporeGenerationFlag = ReadEnemyInstructionMechanicsWord(body, next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_Harden:
                body.XPosition = SporeSpawnDeathCenterX;
                body.YPosition = SporeSpawnDeathCenterY;
                body.Properties = body.Properties
                    .With(EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions)
                    .Without(EnemyProperties.IgnoreSamusCollision);
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6:
                LastSporeSpawnSoundEffectLibrary2 =
                    ReadEnemyInstructionMechanicsWord(body, next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine:
                RequestSporeSpawnDeathDrops(state);
                cursor = next;
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_FunctionInY:
                state.Function = (SporeSpawnFunction)ReadEnemyInstructionMechanicsWord(body, next);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_LoadDeathSequencePalette:
                LoadSporeSpawnDeathPalette(
                    ReadEnemyInstructionMechanicsWord(body, next),
                    targetOnly: false);
                cursor = unchecked((ushort)(next + 2));
                return true;

            case SporeSpawnInstructionCodes.Instruction_SporeSpawn_LoadDeathSequenceTargetPalette:
                LoadSporeSpawnDeathPalette(
                    ReadEnemyInstructionMechanicsWord(body, next),
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
        int frame = SporeSpawnColorRomData.FrameFromByteOffset(
            sourceByteOffset, SporeSpawnColorRomData.HealthFrameCount);
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            _cgram!.SetColor(
                SporeSpawnColorRomData.SpriteDestination + color,
                TileArtwork?.SporeSpawnColors is { } colors
                    ? colors.ResolveHealth(frame, color)
                    : ReadWord(_bus!, SporeSpawnColorRomData.HealthSource +
                        sourceByteOffset + color * sizeof(ushort)));
        }
    }

    private void LoadSporeSpawnDeathPalette(ushort sourceByteOffset, bool targetOnly)
    {
        SporeSpawnEnemyState state = _sporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn palette instruction ran without the boss state.");
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Sprite,
            sourceByteOffset,
            state,
            targetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Level,
            sourceByteOffset,
            state,
            targetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Background,
            sourceByteOffset,
            state,
            targetOnly);
    }

    private void CopySporeSpawnPaletteRow(
        SporeSpawnDeathPaletteLayer layer,
        ushort sourceByteOffset,
        SporeSpawnEnemyState state,
        bool targetOnly)
    {
        int frame = SporeSpawnColorRomData.FrameFromByteOffset(sourceByteOffset,
            layer == SporeSpawnDeathPaletteLayer.Sprite
                ? SporeSpawnColorRomData.DeathSpriteFrameCount
                : SporeSpawnColorRomData.DeathSceneFrameCount);
        int destination = SporeSpawnColorRomData.DeathDestination(layer);
        for (int color = 0; color < SporeSpawnColorRomData.ColorsPerFrame; color++)
        {
            ushort value = TileArtwork?.SporeSpawnColors is { } colors
                ? colors.ResolveDeath(layer, frame, color)
                : ReadWord(_bus!, SporeSpawnColorRomData.DeathSource(layer) +
                    sourceByteOffset + color * sizeof(ushort));
            if (targetOnly)
                state.WriteTargetColor(destination + color, value);
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
