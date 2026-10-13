using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>The Baby Metroid cutscene species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BabyMetroidCutsceneBehavior
{
    /// <summary>Initializes the dynamically allocated Baby through ordinary enemy dispatch.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Supplies current room bus, RNG, and installed Baby artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Queues healing, death, and cutscene cues with native queue limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Allocates the Baby in the first free native enemy slot.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies Baby and Mother Brain color changes to live CGRAM.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies the enemy-frame counter used to gate incremental healing sound.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns release dust and Baby death explosions.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Shares physical body, head, rainbow sequence, and requested music state with Mother Brain.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Connects the cutscene Baby to its Mother Brain encounter and room services.</summary>
    internal BabyMetroidCutsceneBehavior(BabyMetroidCutsceneBehaviorDependencies dependencies)
    {
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
    }

    /// <summary>Claims the Baby's two same-bank goto opcodes while leaving unrelated enemy instructions to the shared interpreter.</summary>
    private static bool TryProcessBabyMetroidCutsceneInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.BabyMetroidCutscene ||
            !Enum.IsDefined((BabyMetroidInstruction)word))
            return false;

        RunMotherBrainBabyInstruction((BabyMetroidInstruction)word, ref cursor);
        return true;
    }

    /// <summary>Allocates the Baby in the first free native slot, runs its initializer, and preserves the post-init empty map until its first scheduled turn.</summary>
    internal void SpawnMotherBrainBabyMetroid(MotherBrainEnemyState state)
    {
        if (state.BabyMetroidSlot is { EnemyDefinitionPointer: not 0 })
        {
            throw new InvalidOperationException(
                "Mother Brain attempted to allocate the cutscene Baby more than once.");
        }

        // `$A0:9275` searches from native slot zero for the first consecutive free run.
        // The Baby header declares one part, so the first zero ID is the exact result.
        int slotIndex = Array.FindIndex(_slotTable._slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            return; // Generic SpawnEnemy returns $FFFF and X=$0800 when the pool is full.

        RoomEnemyPopulationRecord population =
            MotherBrainBabyMetroidDefinitions.SpawnPopulation;
        if (population.DefinitionPointer != EnemyDefinitionId.BabyMetroidCutscene)
        {
            throw new InvalidDataException(
                "Mother Brain Baby spawn record " +
                $"names enemy ${(int)population.DefinitionPointer:X4}, not $ECBF.");
        }

        RoomEnemySlot babySlot = _slotTable._slots[slotIndex];
        RoomEnemyDefinition definition = EnemyPopulationLoader.ResolveRoomEnemyDefinition(_room.Bus!, population.DefinitionPointer);
        _dispatch.InitializeSlotFromDefinition(babySlot, population, definition);
        _dispatch.RunInitializationAi(babySlot);

        // SpawnEnemy installs the canonical empty map after init when property $2000 is set.
        // The actor's list is still live and produces its first visible map on its first
        // scheduled turn; the active-index array was frozen before this allocation.
        babySlot.SpritemapPointer = babySlot.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? (ushort)0x804d
            : (ushort)0;
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * RoomEnemySystem.NativeSlotSize));
    }

    /// <summary>Creates the physical Baby only after the rainbow sequence exists and installs its first list and fifteen nontransparent palette colors.</summary>
    private void InitializeMotherBrainBabyMetroid(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidDataException(
            "The cutscene Baby was initialized without Mother Brain's physical records.");
        if (state.RainbowBeamSequence is null)
        {
            throw new InvalidDataException(
                "The cutscene Baby was initialized before the final rainbow sequence.");
        }

        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(slot.Position, slot.Properties);
        state.BabyMetroidSlot = slot;
        state.BabyMetroid = baby;
        state.BabyAppliedInstructionList = baby.InstructionList;

        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        slot.CurrentInstruction = baby.InstructionList;
        slot.InstructionTimer = NativeCountdown.FromRaw(baby.InstructionTimer);
        slot.Timer = NativeCountdown.Start(0);

        // The writer receives byte index $01E2 and count $000F. Color zero is deliberately
        // skipped at source `$94D2+2`, preserving the room's transparent backdrop entry.
        LoadBabyMetroidCutsceneInitialPalette();
    }

    /// <summary>Copies installed Baby start colors while leaving CGRAM's transparent entry untouched.</summary>
    internal void LoadBabyMetroidCutsceneInitialPalette()
    {
        var colors = _room.Artwork?.BabyMetroidCutsceneColors ?? throw new InvalidOperationException(
            "Baby Metroid cutscene requires installed colors.");
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
            _presentation.BossPalettes.BabyMetroidCutscene.Write(color, colors.InitialColor(color));
    }

    /// <summary>Advances the later Baby slot, publishes cries/healing, and writes body and Baby list changes for their next ordered enemy turns.</summary>
    internal void RunMotherBrainBabyMetroidMain(
        RoomEnemySlot slot,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
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

        slot.ShakeTimer = NativeCountdown.Start(0);
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
            _room.Bus!,
            target,
            sequence,
            state.Head!.Position.Whole,
            layer1: camera,
            enemyFrameCounter: slot.FrameCounter,
            randomNumber: _room.Random.Read());
        state.LastBabyMetroidStep = step;
        // Both outcomes of the native heal helper call the sound helper after the
        // energy write. Use the phase that ran, including its final full-health call.
        if (step.PhaseBefore == BabyMetroidCutscenePhase.HealSamusToFullHealth &&
            unchecked((short)(target.Health - MotherBrainHealthSounds.MinimumEnergy)) >= 0 &&
            (_frameContext._randomEnemyCounter & MotherBrainHealthSounds.EnemyClockMask) == 0)
            _audio.QueueEnemySound(MotherBrainHealthSounds.IncrementalEnergy, maximumQueued: 3);

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
            MotherBrainBehavior.SetMotherBrainInstructionList(state.Body, bodyInstruction);
        }

        // Cross-enemy writes happen during the later Baby slot and become visible in the
        // body record immediately, though body AI cannot execute them until the next frame.
        state.Function = MotherBrainBehavior.MapLiveMotherBrainRainbowFunction(sequence.Phase);

        if (baby.InstructionList != state.BabyAppliedInstructionList)
        {
            MotherBrainBehavior.SetMotherBrainInstructionList(slot, baby.InstructionList);
            state.BabyAppliedInstructionList = baby.InstructionList;
        }
        CopyBabyMetroidActorToPhysicalSlot(slot, baby);
        ApplyBabyMetroidFrameEffects(state, step);
    }

    /// <summary>Copies cutscene position, property, palette, tile, and health state into the live enemy record.</summary>
    private static void CopyBabyMetroidActorToPhysicalSlot(
        RoomEnemySlot slot,
        BabyMetroidCutsceneState baby)
    {
        slot.Position = baby.Position;
        slot.Properties = baby.Properties;
        slot.PaletteIndex = baby.Palette;
        slot.VramTilesIndex = baby.GraphicsOffset;
        slot.Health = baby.Health;
    }

    /// <summary>Applies one step's music, sounds, dust, palette, attack-tile, and recovery-light requests to room systems.</summary>
    internal void ApplyBabyMetroidFrameEffects(
        MotherBrainEnemyState state,
        BabyMetroidCutsceneStepResult step)
    {
        if (step.FatalBlowStarted)
        {
            _audio.QueueEnemySound(SoundEffectLibrary3Sounds.BabyMetroidDeathCry, maximumQueued: 6);
            SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation.ApplyBabyMetroidDeathBlackout(_presentation._cgram!);
            state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
        }
        if (step.SamusThemeStarted)
        {
            state.RequestMusic(MusicCommand.LoadData(MotherBrainBabyMetroidDefinitions.SamusThemeMusicData),
                MusicCommandDelay.EightFrames);
            state.RequestMusic(MusicCommand.SelectTrack(MotherBrainBabyMetroidDefinitions.SamusThemeMusicTrack),
                MusicCommandDelay.EightFrames);
        }
        if (step.LatchSoundQueued)
            state.LastSoundEffectLibrary1 = 0x0040;
        if (step.AmbientCrySoundQueued)
            state.LastSoundEffect = 0x0052;

        foreach (BabyMetroidReleaseDustRequest dust in step.ReleaseDustClouds)
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.ProjectileParameter);

        if (step.DeathExplosion is { } explosion)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(explosion.Position, explosion.ProjectileParameter);
            // $A9:CDF4 queues the explosion through QueueSound_Lib3_Max3.
            state.LastSoundEffectLibrary3 = explosion.SoundEffect;
        }

        if (step.BabyPaletteTransfer is { } palette)
            LoadBabyMetroidCutsceneFadePalette(palette);

        if (step.AttackTileTransfer is { } attackTiles)
            _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(attackTiles);

        if (step.BackgroundPaletteTransfer is { } roomPalette)
            LoadMotherBrainRecoveryLights(roomPalette);
    }

    /// <summary>Requires the native two-destination recovery-light transfer layout before applying installed room colors.</summary>
    internal void LoadMotherBrainRecoveryLights(
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

        (_presentation.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain recovery requires installed room colors."))
            .ApplyRecoveryLights(_presentation._cgram!, request.PaletteIndex);
    }

    /// <summary>Requires the native fade source, destination, and color count before copying installed Baby shades to CGRAM.</summary>
    internal void LoadBabyMetroidCutsceneFadePalette(
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

        var colors = _room.Artwork?.BabyMetroidCutsceneColors ?? throw new InvalidOperationException(
            "Baby Metroid fade requires installed colors.");
        for (int color = 0; color < palette.ColorCount; color++)
            _presentation.BossPalettes.BabyMetroidCutscene.Write(color,
                colors.FadeColor(palette.PaletteIndex, color));
    }

    /// <summary>Baby private opcodes `$CFB4/$CFCA` are same-bank direct gotos.</summary>
    private static void RunMotherBrainBabyInstruction(
        BabyMetroidInstruction opcode,
        ref ushort cursor)
    {
        cursor = opcode switch
        {
            BabyMetroidInstruction.GotoInitial => BabyMetroidCutsceneState.InitialInstructionList,
            BabyMetroidInstruction.GotoDrainingMotherBrain => BabyMetroidCutsceneState.DrainingMotherBrainInstructionList,
            _ => throw new InvalidOperationException($"Undefined BabyMetroidInstruction {opcode}."),
        };
    }

    /// <summary>Binds Baby cutscene setup only to its physical enemy-definition ID.</summary>
    internal void RegisterBabyMetroidCutsceneInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_BabyMetroidCutscene, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroidCutscene, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMotherBrainBabyMetroid(slot);
        });
    }

    /// <summary>Binds the later-slot Baby update to its native main-AI routine.</summary>
    internal void RegisterBabyMetroidCutsceneMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_BabyMetroidCutscene, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroidCutscene, (slot, frame, nmiFrameCounter8) =>
        {
            RunMotherBrainBabyMetroidMain(slot, frame.Samus, frame.Camera);
        });
    }

    /// <summary>Registers Baby-only goto handling without taking ownership of common enemy opcodes.</summary>
    internal static void RegisterBabyMetroidCutsceneOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessBabyMetroidCutsceneInstruction(slot, word, ref cursor);
        });
    }
}
