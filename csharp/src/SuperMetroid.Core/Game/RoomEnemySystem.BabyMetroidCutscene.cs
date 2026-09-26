using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Physical room-enemy ownership for the Baby Metroid spawned during Mother Brain's final
/// phase-two rainbow beam. The long cutscene state machine is shared with the exhaustive
/// standalone verifier; this adapter preserves the cartridge's fixed-slot allocation,
/// ordinary bank-$A9 instruction interpreter, OAM layer, palette writes, projectile pool,
/// and increasing-slot cross-enemy scheduling.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports <c>$A9:BE1B-$BE27</c> through the generic one-part allocator.</summary>
    private void SpawnMotherBrainBabyMetroid(MotherBrainEnemyState state)
    {
        if (state.BabyMetroidSlot is { EnemyDefinitionPointer: not 0 })
        {
            throw new InvalidOperationException(
                "Mother Brain attempted to allocate the cutscene Baby more than once.");
        }

        // `$A0:9275` searches from native slot zero for the first consecutive free run.
        // The Baby header declares one part, so the first zero ID is the exact result.
        int slotIndex = Array.FindIndex(_slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            return; // Generic SpawnEnemy returns $FFFF and X=$0800 when the pool is full.

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        if (population.DefinitionPointer != MotherBrainBabyMetroidDefinitions.EnemyDefinition)
        {
            throw new InvalidDataException(
                "Mother Brain Baby spawn record " +
                $"names enemy ${population.DefinitionPointer:X4}, not $ECBF.");
        }

        RoomEnemySlot babySlot = _slots[slotIndex];
        RoomEnemyDefinition definition = ResolveRoomEnemyDefinition(_bus!, population.DefinitionPointer);
        InitializeSlotFromDefinition(babySlot, population, definition);
        RunInitializationAi(babySlot);

        // SpawnEnemy installs the canonical empty map after init when property $2000 is set.
        // The actor's list is still live and produces its first visible map on its first
        // scheduled turn; the active-index array was frozen before this allocation.
        babySlot.SpritemapPointer = babySlot.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? (ushort)0x804d
            : (ushort)0;
        EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
        FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * NativeSlotSize));
    }

    /// <summary>Ports Baby initialization <c>$A9:C710-$C776</c>.</summary>
    private void InitializeMotherBrainBabyMetroid(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidDataException(
            "The cutscene Baby was initialized without Mother Brain's physical records.");
        if (state.RainbowBeamSequence is null)
        {
            throw new InvalidDataException(
                "The cutscene Baby was initialized before the final rainbow sequence.");
        }

        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(slot.Properties);
        state.BabyMetroidSlot = slot;
        state.BabyMetroid = baby;
        state.BabyAppliedInstructionList = baby.InstructionList;

        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        slot.CurrentInstruction = baby.InstructionList;
        slot.InstructionTimer = baby.InstructionTimer;
        slot.Timer = 0;

        // The writer receives byte index $01E2 and count $000F. Color zero is deliberately
        // skipped at source `$94D2+2`, preserving the room's transparent backdrop entry.
        LoadBabyMetroidCutsceneInitialPalette();
    }

    private void LoadBabyMetroidCutsceneInitialPalette()
    {
        int destination = BabyMetroidCutsceneColorRomData.DestinationByteIndex / 2;
        if (TileArtwork?.BabyMetroidCutsceneColors is { } colors)
        {
            for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount;
                 color++)
                _cgram!.SetColor(destination + color, colors.InitialColor(color));
            return;
        }
        _cgram!.LoadFromBus(_bus!, BabyMetroidCutsceneColorRomData.InitialSource,
            BabyMetroidCutsceneColorRomData.InitialColorCount, destination);
    }

    /// <summary>Runs one physical Baby main-AI call at <c>$A9:C779</c>.</summary>
    private void RunMotherBrainBabyMetroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        MotherBrainEnemyState state = _motherBrain ?? throw new InvalidOperationException(
            "The cutscene Baby lost Mother Brain's encounter state.");
        BabyMetroidCutsceneState baby = state.BabyMetroid ??
            throw new InvalidOperationException("The physical Baby has no cutscene state.");
        if (!ReferenceEquals(slot, state.BabyMetroidSlot))
        {
            throw new InvalidDataException(
                $"Baby state names slot {state.BabyMetroidSlot?.SlotIndex}, but dispatcher " +
                $"ran slot {slot.SlotIndex}.");
        }

        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException("The cutscene Baby lost the rainbow sequence.");
        SamusState target = samus ?? throw new InvalidOperationException(
            "The cutscene Baby requires the live Samus actor.");

        slot.ShakeTimer = 0;
        if (state.PendingBabyCryCount != 0 &&
            baby.Phase is BabyMetroidCutscenePhase.HealSamusToFullHealth or
                BabyMetroidCutscenePhase.IdleUntilNoHealth)
        {
            // `$A9:C7B7` clears the complete shared word and queues only one `$72`, even
            // when several rings incremented it before this later enemy slot ran.
            state.PendingBabyCryCount = 0;
            state.LastSoundEffect = 0x0072;
        }
        BabyMetroidCutsceneStepResult step = baby.Step(
            _bus!,
            target,
            sequence,
            layer1X: cameraX,
            layer1Y: cameraY,
            enemyFrameCounter: slot.FrameCounter,
            randomNumber: RequireRandomNumber());
        state.LastBabyMetroidStep = step;
        // Both outcomes of the native heal helper call the sound helper after the
        // energy write. Use the phase that ran, including its final full-health call.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            unchecked((short)(target.Health - MotherBrainHealthSounds.MinimumEnergy)) >= 0 &&
            (_randomEnemyCounter & MotherBrainHealthSounds.EnemyClockMask) == 0)
            QueueEnemySound(MotherBrainHealthSounds.IncrementalEnergy, maximumQueued: 3);

        // `$A9:C879` writes the body enemy's instruction words from this later Baby slot.
        // The reusable sequence retains the requested list, but the physical body already
        // completed its AI/instruction pass earlier this frame; install the list here so it
        // begins on the next increasing-slot enemy pass exactly like the cross-WRAM write.
        if (step.BodyStumbleRequested)
        {
            ushort bodyInstruction = sequence.RequestedBodyInstructionList;
            if (bodyInstruction == 0)
            {
                throw new InvalidDataException(
                    "Baby requested Mother Brain body movement without an instruction list.");
            }
            SetMotherBrainInstructionList(state.Body, bodyInstruction);
        }

        // Cross-enemy writes happen during the later Baby slot and become visible in the
        // body record immediately, though body AI cannot execute them until the next frame.
        state.Function = MapLiveMotherBrainRainbowFunction(sequence.Phase);

        if (baby.InstructionList != state.BabyAppliedInstructionList)
        {
            SetMotherBrainInstructionList(slot, baby.InstructionList);
            state.BabyAppliedInstructionList = baby.InstructionList;
        }
        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        ApplyBabyMetroidFrameEffects(state, step);
    }

    private static void CopyBabyMetroidActorToPhysicalSlot(
        RoomEnemySlot slot,
        BabyMetroidCutsceneState baby)
    {
        slot.XPosition = baby.XPosition;
        slot.XSubposition = baby.XSubposition;
        slot.YPosition = baby.YPosition;
        slot.YSubposition = baby.YSubposition;
        slot.Properties = baby.Properties;
        slot.PaletteIndex = baby.Palette;
        slot.VramTilesIndex = baby.GraphicsOffset;
        slot.Health = baby.Health;
    }

    private void ApplyBabyMetroidFrameEffects(
        MotherBrainEnemyState state,
        BabyMetroidCutsceneStepResult step)
    {
        if (step.LatchSoundQueued)
            state.LastSoundEffectLibrary1 = 0x0040;
        if (step.AmbientCrySoundQueued)
            state.LastSoundEffect = 0x0052;

        foreach (BabyMetroidReleaseDustRequest dust in step.ReleaseDustClouds)
            SpawnRoomGraphicsDustExplosion(dust.XPosition, dust.YPosition, dust.ProjectileParameter);

        if (step.DeathExplosion is { } explosion)
        {
            SpawnRoomGraphicsDustExplosion(
                explosion.XPosition,
                explosion.YPosition,
                explosion.ProjectileParameter);
            state.LastSoundEffect = explosion.SoundEffect;
        }

        if (step.BabyPaletteTransfer is { } palette)
            LoadBabyMetroidCutsceneFadePalette(palette);

        if (step.AttackTileTransfer is { } attackTiles)
            ApplyMotherBrainRainbowTileTransfer(attackTiles);

        if (step.BackgroundPaletteTransfer is { } roomPalette)
            LoadMotherBrainRecoveryLights(roomPalette);
    }

    private void LoadMotherBrainRecoveryLights(
        MotherBrainBackgroundPaletteTransferRequest request)
    {
        if (request.PaletteIndex >= MotherBrainRoomColorRomData.RecoveryLightsFrames ||
            request.SourceAddress != (uint)MotherBrainRoomColorRomData.RecoveryLightsSource(
                request.PaletteIndex) ||
            request.FirstDestinationColorIndex !=
                MotherBrainRoomColorRomData.RecoveryLightsFirstColor * sizeof(ushort) ||
            request.SecondDestinationColorIndex !=
                MotherBrainRoomColorRomData.RecoveryLightsSecondColor * sizeof(ushort) ||
            request.ColorsPerDestination !=
                MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination)
            throw new InvalidDataException(
                $"Mother Brain recovery-light request {request.PaletteIndex} has a non-native transfer layout.");

        if (MotherBrainRoomColors is { } colors)
        {
            colors.ApplyRecoveryLights(_cgram!, request.PaletteIndex);
            return;
        }
        int source = MotherBrainRoomColorRomData.RecoveryLightsSource(request.PaletteIndex);
        _cgram!.LoadFromBus(_bus!, source,
            MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination,
            MotherBrainRoomColorRomData.RecoveryLightsFirstColor);
        _cgram.LoadFromBus(_bus!, source +
            MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * sizeof(ushort),
            MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination,
            MotherBrainRoomColorRomData.RecoveryLightsSecondColor);
    }

    private void LoadBabyMetroidCutsceneFadePalette(
        BabyMetroidPaletteTransferRequest palette)
    {
        int expectedSource = BabyMetroidCutsceneColorRomData.FadeSource(
            palette.PaletteIndex);
        if (palette.SourceAddress != expectedSource ||
            palette.DestinationColorIndex !=
                BabyMetroidCutsceneColorRomData.DestinationByteIndex ||
            palette.ColorCount != BabyMetroidCutsceneColorRomData.FadeColorCount)
            throw new InvalidDataException(
                $"Cutscene Baby fade {palette.PaletteIndex} has a non-native transfer layout.");

        int destination = palette.DestinationColorIndex / 2;
        if (TileArtwork?.BabyMetroidCutsceneColors is { } colors)
        {
            for (int color = 0; color < palette.ColorCount; color++)
                _cgram!.SetColor(destination + color,
                    colors.FadeColor(palette.PaletteIndex, color));
            return;
        }
        _cgram!.LoadFromBus(_bus!, expectedSource, palette.ColorCount, destination);
    }

    /// <summary>Baby private opcodes `$CFB4/$CFCA` are same-bank direct gotos.</summary>
    private static bool TryRunMotherBrainBabyInstruction(
        ushort opcode,
        ref ushort cursor)
    {
        switch (opcode)
        {
            case MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoInitial:
                cursor = BabyMetroidCutsceneState.InitialInstructionList;
                return true;
            case MotherBrainInstructionCodes.Instruction_BabyMetroid_GotoDrainingMotherBrain:
                cursor = BabyMetroidCutsceneState.DrainingMotherBrainInstructionList;
                return true;
            default:
                return false;
        }
    }
}
