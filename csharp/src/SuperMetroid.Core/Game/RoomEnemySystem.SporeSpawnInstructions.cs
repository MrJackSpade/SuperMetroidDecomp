namespace SuperMetroid.Core.Game;

/// <summary>
/// Spore Spawn's private bank-$A5 instruction callbacks. The boss's authored instruction
/// lists own its open/close cadence, movement-state changes, palette animation, explosions,
/// and drop burst; keeping those operations here makes the ROM list remain the timeline.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Executes one recognized Spore Spawn instruction callback and advances its operand cursor as the native word layout requires.</summary>
    /// <param name="body">Enemy slot whose definition and instruction state identify the boss.</param>
    /// <param name="opcode">Instruction callback word currently being dispatched.</param>
    /// <param name="cursor">Address of the callback word; handled instructions advance it past any inline operands.</param>
    /// <returns><see langword="true"/> when the slot belongs to Spore Spawn and the opcode is handled; otherwise leaves the cursor unchanged and returns <see langword="false"/>.</returns>
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
                (TileArtwork?.SporeSpawnColors ?? throw new InvalidDataException(
                    "Spore Spawn health palette requires installed artwork.")).ResolveHealth(frame, color));
        }
    }

    /// <summary>Loads the selected death-palette frame into the sprite, level, and background color rows.</summary>
    /// <param name="sourceByteOffset">Native byte offset identifying the death-sequence frame.</param>
    /// <param name="targetOnly">When <see langword="true"/>, updates the target palette snapshot instead of live CGRAM.</param>
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

    /// <summary>Copies one layer of the selected death frame either to live CGRAM or the boss's target colors.</summary>
    /// <param name="layer">Sprite, level, or background palette row being updated.</param>
    /// <param name="sourceByteOffset">Native byte offset used to select a frame in the layer's death-palette table.</param>
    /// <param name="state">Spore Spawn state receiving target colors when <paramref name="targetOnly"/> is enabled.</param>
    /// <param name="targetOnly">Selects snapshot updates instead of visible CGRAM writes.</param>
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
            ushort value = (TileArtwork?.SporeSpawnColors ?? throw new InvalidDataException(
                "Spore Spawn death palette requires installed artwork.")).ResolveDeath(layer, frame, color);
            if (targetOnly)
                state.WriteTargetColor(destination + color, value);
            else
                _cgram!.SetColor(destination + color, value);
        }
    }

    /// <summary>Uses the next RNG value to place the hardening dust cloud around the boss and queues its sound effect.</summary>
    /// <param name="state">Boss state providing the center position for the dust burst.</param>
    private void SpawnSporeSpawnHardeningDust(SporeSpawnEnemyState state)
    {
        ushort random = _nextRandom!();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(state.Body.XPosition + (random & 0x007f) - 64)),
            unchecked((ushort)(state.Body.YPosition + ((random & 0x7f00) >> 8) - 64)),
            animationIndex: 0x0015);
        LastSporeSpawnSoundEffectLibrary2 = 0x0029;
    }

    /// <summary>Uses the next RNG value to place one dying-explosion sprite object and queues its sound effect.</summary>
    /// <param name="state">Boss state providing the center position for the explosion.</param>
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

    /// <summary>Requests the sixteen deterministic drop attempts emitted by the boss's death instruction and records that the burst was issued.</summary>
    /// <param name="state">Boss state whose death-drop request flag is set after all attempts are made.</param>
    private void RequestSporeSpawnDeathDrops(SporeSpawnEnemyState state)
    {
        for (int drop = 0; drop < 16; drop++)
        {
            ushort random = _nextRandom!();
            ushort x = unchecked((ushort)((random & 0x007f) + 64));
            ushort y = unchecked((ushort)(((random & 0x3f00) >> 8) + 528));
            _sporeSpawnDropRequests.Add(new SporeSpawnDropRequest());
            SpawnEnemyDropFromEnemyHeader(x, y, SporeSpawnDefinition);
        }
        state.DeathDropRequested = true;
    }
}
