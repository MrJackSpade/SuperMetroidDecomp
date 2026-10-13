using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The dead sidehopper species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DeadSidehopperBehavior : IEnemySpecies
{
    /// <summary>Provides scratch tables, installed artwork, and the room random source for corpse decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Handles level collision after the scripted opening movement reaches the terrain boundary.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Exposes the companion enemy slot whose invisibility suppresses the initial victim.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Receives the victim's staged palette changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies the enemy counter used to pace dust sound effects.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust from each completed corpse row.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies normal touch damage before the corpse finishes transforming.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Owns the mutable corpse tile buffer, DMA records, and dust sound request.</summary>
    private readonly DeadMonsterFrame _deadMonsters;

    /// <summary>Connects the corpse actor to room state, terrain, presentation, and interaction services.</summary>
    internal DeadSidehopperBehavior(DeadSidehopperBehaviorDependencies dependencies)
    {
        _deadMonsters = dependencies.DeadMonsters;
        _room = dependencies.Room;
        _terrain = dependencies.Terrain;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _projectiles = dependencies.Projectiles;
        _collision = dependencies.Collision;
    }

    /// <summary>Executes the dead Sidehopper's private animation instruction.</summary>
    private bool TryProcessDeadSidehopperInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.CorpseSidehopper ||
            !Enum.IsDefined((DeadSidehopperInstruction)word))
            return false;

        switch ((DeadSidehopperInstruction)word)
        {
            case DeadSidehopperInstruction.EndHop:
                // `$A9:ECD0` is embedded at the end of the landing animation. It
                // hands ownership back to main AI without consuming an operand.
                SelectDeadSidehopperPostAnimationState(slot);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Dead Sidehopper does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Native solid-to-Samus bit installed when palette transformation finishes.</summary>
    private const ushort DeadMonsterSolidProperty = 0x8000;
    /// <summary>Native corpse bit that rejects later shot callbacks once decomposition begins.</summary>
    internal const ushort DeadMonsterInteractionRejectedProperty = 0x0400;

    /// <summary>Stores each room enemy slot's private hop, palette, and rotting state.</summary>
    internal readonly DeadSidehopperEnemyState?[] _deadSidehopperStates =
        new DeadSidehopperEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Remembers level data for the private power-bomb callback, which has no level argument.</summary>
    private RoomLevelData? _deadSidehopperLevel;
    /// <summary>Remembers the camera position for the private power-bomb AI dispatch.</summary>
    private ushort _deadSidehopperCameraX;

    /// <summary>Discards prior room corpse state and its queued tile transfers.</summary>
    internal void ResetDeadSidehopperRoomState()
    {
        Array.Clear(_deadSidehopperStates);
        _deadMonsters.Clear();
        _deadSidehopperLevel = null;
        _deadSidehopperCameraX = 0;
    }

    /// <summary>Clears the per-frame corpse DMA and sound records before enemy AI runs.</summary>
    internal void BeginDeadSidehopperFrame()
    {
        _deadMonsters.Clear();
    }

    /// <summary>Ports <c>DeadSidehopper_Init</c> at <c>$A9:D7B6</c>.</summary>
    internal void InitializeDeadSidehopper(RoomEnemySlot slot)
    {
        if (_deadSidehopperStates[slot.SlotIndex] is not null)
            throw new InvalidDataException($"Dead sidehopper slot {slot.SlotIndex} was initialized twice.");

        switch (slot.Parameter1)
        {
            case 0:
                InitializeShitroidVictimSidehopper(slot);
                break;
            case 2:
                InitializeAlternateDeadSidehopper(slot);
                break;
            default:
                throw new InvalidDataException(
                    $"Dead sidehopper parameter 1 ${slot.Parameter1:X4} is not authored by retail AI.");
        }
    }

    /// <summary>Ports the Shitroid victim layout at <c>$A9:D7C4</c>.</summary>
    private void InitializeShitroidVictimSidehopper(RoomEnemySlot slot)
    {
        // The native mask clears the solid bit and any inherited process-offscreen bit,
        // then explicitly reinstalls only process-offscreen. Do not replace this with an
        // enum union: preserving the unrelated population-property bits is observable.
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessOffScreen,
            EnemyProperties.ProcessOffScreen);
        if (_slotTable._slots[0].Properties.HasAny(EnemyProperties.Invisible))
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);

        slot.Position = slot.Position.WithWhole(new(488, 184));
        slot.PaletteIndex = EnemyPaletteBits.Palette1;
        slot.Radius = slot.Radius.WithY(21);
        SetDeadSidehopperInstruction(
            slot,
            DeadSidehopperInstructionProgramDefinitions.AliveIdle);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive);
        state.Function = DeadSidehopperAiFunction.AliveWaitForCamera;
        state.PaletteStage = 0;
        state.Velocity = new EightEightVector<RoomPixels>(96, 256);
    }

    /// <summary>Ports the already-dead Tourian layout at <c>$A9:D825</c>.</summary>
    private void InitializeAlternateDeadSidehopper(RoomEnemySlot slot)
    {
        slot.PaletteIndex = EnemyPaletteBits.Palette7;
        SetDeadSidehopperInstruction(
            slot,
            DeadSidehopperInstructionProgramDefinitions.InitiallyDead);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 2,
            DeadSidehopperCorpseDefinitions.InitiallyDead);
        state.PaletteStage = 0xffff;
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
    }

    /// <summary>Allocates slot state, resets the rotation table, and installs the selected corpse tile variant.</summary>
    private DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(
        RoomEnemySlot slot,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition definition)
    {
        if (definition.EntryCount == 0)
            throw new InvalidDataException("Dead sidehopper corpse configuration has zero rows.");

        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        var state = new DeadSidehopperEnemyState(slot, graphicsVariant, definition.ConfigurationPointer, new CorpseRottingProgram { TablePointer = definition.RottingTablePointer, VramTablePointer = definition.VramTransferPointer, CopyFunction = definition.CopyFunction, MoveFunction = definition.MoveFunction, RotationTablePointer = definition.RotationTablePointer, FinishFunction = definition.FinishFunction, EntryCount = definition.EntryCount, YLimit = yLimit, LateMoveEntryIndex = lateMoveEntryIndex, WrapOffset = definition.WrapOffset });
        _deadSidehopperStates[slot.SlotIndex] = state;

        CorpseRottingTableProcessor.Initialize(
            _room.Scratch.RotTable(definition.RottingTablePointer, definition.EntryCount));
        InitializeDeadSidehopperGraphics(graphicsVariant);
        return state;
    }

    /// <summary>Ports <c>DeadSidehopper_Main</c> at <c>$A9:D8DB</c>.</summary>
    private void RunDeadSidehopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        _deadSidehopperLevel = level;
        _deadSidehopperCameraX = cameraX;
        DispatchDeadSidehopperState(slot, state, samus, level, cameraX);
    }

    /// <summary>Advances the authored hop, post-landing delay, palette change, or collision-triggered decay state.</summary>
    internal void DispatchDeadSidehopperState(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX)
    {
        switch (state.Function)
        {
            case DeadSidehopperAiFunction.AliveWaitForCamera:
                if (unchecked((short)(cameraX - 513)) < 0)
                {
                    state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                    goto case DeadSidehopperAiFunction.ActivatedMovement;
                }
                return;

            case DeadSidehopperAiFunction.ActivatedMovement:
                if (MoveDeadSidehopper(slot, state, level))
                {
                    state.JumpPhase = unchecked((ushort)((state.JumpPhase + 1) & 3));
                    SetDeadSidehopperInstruction(
                        slot,
                        DeadSidehopperInstructionProgramDefinitions.AliveHopping);
                    state.Function = DeadSidehopperAiFunction.NoOperation;
                }
                return;

            case DeadSidehopperAiFunction.NoOperation:
                return;

            case DeadSidehopperAiFunction.BeginPostLandingDelay:
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = NativeCountdown.Start(64);
                return;

            case DeadSidehopperAiFunction.PostLandingDelay:
                state.StateTimer = state.StateTimer.Decrement().Remaining;
                if (!state.StateTimer.IsNegative)
                    return;

                if (state.PaletteStage != 0)
                {
                    state.Function = DeadSidehopperAiFunction.TransformPalette;
                    return;
                }

                state.Function = DeadSidehopperAiFunction.ActivatedMovement;
                SetDeadSidehopperInstruction(
                    slot,
                    DeadSidehopperInstructionProgramDefinitions.AliveIdle);
                state.Velocity = DeadSidehopperLaunchDefinitions.Velocity(state.JumpPhase);
                return;

            case DeadSidehopperAiFunction.TransformPalette:
                StepDeadSidehopperPaletteTransformation(slot, state);
                return;

            case DeadSidehopperAiFunction.WaitForSamusCollision:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    state.Function = DeadSidehopperAiFunction.PreRotDelay;
                return;

            case DeadSidehopperAiFunction.PreRotDelay:
                state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
                if (state.PreRotDelayCounter >= 16)
                {
                    state.Function = DeadSidehopperAiFunction.Rotting;
                    slot.Properties = unchecked((ushort)(
                        slot.Properties | DeadMonsterInteractionRejectedProperty));
                }
                return;

            case DeadSidehopperAiFunction.Rotting:
                RunDeadSidehopperRotting(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Dead sidehopper function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Implements <c>DeadMonsters_Func_3/4</c> at <c>$A9:D961/D9C7</c>. The scripted
    /// Shitroid-room half moves directly through the staged set until X reaches 544; the
    /// general room half then switches to the shared terrain collision primitives.
    /// </summary>
    private bool MoveDeadSidehopper(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        RoomLevelData? level)
    {
        bool beforeTerrainBoundary = unchecked((short)(slot.Position.X.Whole - 544)) < 0;
        int horizontalDisplacement = SnesSixteenSixteen.FromEightEight(state.Velocity.X.SignedRawValue);
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: true,
                state.Velocity.X.RawValue);
        }
        else
        {
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                RequireDeadSidehopperLevel(level),
                slot,
                horizontalDisplacement);
        }

        // Upward motion accelerates by $20 in signed 8.8; falling motion accelerates by
        // $80. The sign test is performed before the addition in the cartridge.
        ushort acceleration = state.Velocity.Y.SignedRawValue < 0
            ? (ushort)32
            : (ushort)128;
        state.Velocity = state.Velocity.WithY(
            new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + acceleration))));
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: false,
                state.Velocity.Y.RawValue);
            return unchecked((short)(slot.Position.Y.Whole - 184)) >= 0;
        }

        int verticalDisplacement = SnesSixteenSixteen.FromEightEight(state.Velocity.Y.SignedRawValue);
        return _terrain.MoveEnemyVertically(
            RequireDeadSidehopperLevel(level),
            slot,
            verticalDisplacement);
    }

    /// <summary>Applies signed 8.8 scripted motion to one 16.16 axis before terrain collision takes over.</summary>
    private static void MoveDeadSidehopperAxisDirect(
        RoomEnemySlot slot,
        bool horizontal,
        ushort velocity)
    {
        ushort position = horizontal ? slot.Position.X.Whole : slot.Position.Y.Whole;
        ushort subposition = horizontal ? slot.Position.X.Fraction : slot.Position.Y.Fraction;
        SnesFixedPosition moved = new SnesFixedPosition(position, subposition).AddEightEightToHighFraction(velocity);
        position = moved.Whole;
        subposition = moved.Fraction;
        if (horizontal)
        {
            slot.Position = slot.Position.WithX(new(position, subposition));
        }
        else
        {
            slot.Position = slot.Position.WithY(new(position, subposition));
        }
    }

    /// <summary>Rejects terrain-driven movement when no room level was supplied.</summary>
    private static RoomLevelData RequireDeadSidehopperLevel(RoomLevelData? level) =>
        level ?? throw new InvalidDataException(
            "Dead sidehopper crossed into terrain-driven movement without room level data.");

    /// <summary>Ports palette transformation <c>$A9:DA08</c>.</summary>
    private void StepDeadSidehopperPaletteTransformation(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.PaletteFrameCounter = unchecked((ushort)(state.PaletteFrameCounter + 1));
        if (state.PaletteFrameCounter < 8)
            return;

        state.PaletteFrameCounter = 0;
        var colors = _room.Artwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Dead-sidehopper palette transformation requires installed artwork.");
        for (int color = 0; color < 15; color++)
        {
            _presentation.BossPalettes.DeadSidehopper.Write(
                color,
                colors.Resolve(SuperMetroid.Core.Assets.EnemyAuxiliaryPalette.DeadSidehopper,
                    state.PaletteStage - 1, color));
        }

        state.PaletteStage = unchecked((ushort)(state.PaletteStage + 1));
        if (state.PaletteStage < 8)
            return;

        SetDeadSidehopperInstruction(
            slot,
            DeadSidehopperInstructionProgramDefinitions.AliveCorpse);
        state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        slot.Properties = unchecked((ushort)(slot.Properties | DeadMonsterSolidProperty));
        slot.Radius = slot.Radius.WithY(12);
    }

    /// <summary>
    /// Instruction <c>$A9:ECD0</c> selects the post-landing state from the companion's
    /// palette-activation word. It is deliberately instruction-driven: replacing it with a
    /// main-AI timer would make animation length and movement cadence diverge.
    /// </summary>
    private void SelectDeadSidehopperPostAnimationState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        state.Function = state.PaletteStage != 0
            ? DeadSidehopperAiFunction.TransformPalette
            : DeadSidehopperAiFunction.BeginPostLandingDelay;
    }

    /// <summary>Writes Shitroid state 11's victim-owned extended word 08.</summary>
    internal void ActivateDeadSidehopperVictim(RoomEnemySlot victim, ShitroidEnemyState state)
    {
        DeadSidehopperEnemyState victimState = RequireDeadSidehopperState(victim);
        victimState.PaletteStage = 1;
        state.VictimActivationFlag = 1;
    }

    /// <summary>Ports shot callback <c>$A9:DD1D</c>.</summary>
    private void ResolveDeadSidehopperShot(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if ((slot.Properties & DeadMonsterInteractionRejectedProperty) != 0 ||
            state.PaletteStage < 8)
        {
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Ports touch callback <c>$A9:DD44</c>.</summary>
    private void ResolveDeadSidehopperTouch(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        SamusState samus)
    {
        if (state.PaletteStage < 8)
        {
            _collision.ResolveNormalEnemyTouch(slot, samus);
            return;
        }
        TriggerDeadSidehopperRotting(slot, state);
    }

    /// <summary>Ports power-bomb callback <c>$A9:D8CC</c>.</summary>
    private void ResolveDeadSidehopperPowerBomb(RoomEnemySlot slot, SamusState? samus)
    {
        DeadSidehopperEnemyState state = RequireDeadSidehopperState(slot);
        if (state.PaletteStage >= 8)
        {
            ResolveDeadSidehopperShot(slot);
            return;
        }

        // Before the corpse is ready, the private power-bomb callback literally invokes
        // the actor's main dispatcher once. Use the last scheduled room context because the
        // outer bank-$A0 power-bomb pass does not carry level/camera arguments.
        DispatchDeadSidehopperState(
            slot,
            state,
            samus,
            _deadSidehopperLevel,
            _deadSidehopperCameraX);
    }

    /// <summary>Starts decay and keeps the corpse updating offscreen without further Samus collision.</summary>
    private static void TriggerDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.Function = DeadSidehopperAiFunction.Rotting;
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Processes one rotation-table step, its dust callback, and the resulting VRAM records.</summary>
    private void RunDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _room.Scratch.RotTable(state.TablePointer, state.EntryCount),
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadSidehopperPixelRow(state, yOffset, move),
            entryIndex => FinishDeadSidehopperCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        BuildDeadSidehopperVramTransfers(state);
    }

    /// <summary>Copies five rows of the selected installed corpse art into the room's mutable tile buffer.</summary>
    internal void InitializeDeadSidehopperGraphics(ushort graphicsVariant)
    {
        ReadOnlySpan<byte> installedTiles = _deadMonsters.InstalledCorpseTiles();
        if (installedTiles.IsEmpty)
            throw new InvalidDataException("Dead sidehopper requires installed corpse artwork.");

        WorkRamByteTable tiles = _room.Scratch.CorpseTiles;
        for (int row = 0; row < 5; row++)
        {
            var copy = DeadMonsterRottingDefinitions.SidehopperInitialCopy(graphicsVariant, row);
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                tiles[copy.DestinationOffset + byteIndex] = installedTiles[sourceOffset];
            }
        }
    }

    /// <summary>Copies eligible corpse tile columns into the next row and optionally erases their source words.</summary>
    internal void CopyOrMoveDeadSidehopperPixelRow(
        DeadSidehopperEnemyState state,
        ushort yOffset,
        bool move)
    {
        ushort sourceOffset = unchecked((ushort)(
            DeadMonsterRottingDefinitions.RotationOffset(state.RotationTablePointer, yOffset) + (yOffset & 7) * 2));
        ushort destinationOffset = (yOffset & 7) >= 6
            ? unchecked((ushort)(state.WrapOffset + sourceOffset))
            : sourceOffset;

        for (int columnIndex = 0; columnIndex < 5; columnIndex++)
        {
            if (yOffset < DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(state.GraphicsVariant, columnIndex))
                continue;

            int columnOffset = DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(state.GraphicsVariant, columnIndex);
            int sourceWord = sourceOffset / 2 + columnOffset;
            int destinationWord = destinationOffset / 2 + columnOffset + 1;
            if (yOffset < 38)
            {
                _deadMonsters.WriteWorkWord(destinationWord, _deadMonsters.ReadWorkWord(sourceWord));
                _deadMonsters.WriteWorkWord(
                    destinationWord + 8,
                    _deadMonsters.ReadWorkWord(sourceWord + 8));
            }

            if (move)
            {
                _deadMonsters.WriteWorkWord(sourceWord, 0);
                _deadMonsters.WriteWorkWord(sourceWord + 8, 0);
            }
        }
    }

    /// <summary>Records a completed row, spawns dust at its randomized X offset, and conditionally requests sound.</summary>
    private void FinishDeadSidehopperCorpseRow(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = _room.Random.Read();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(slot.Position.X.Whole + (random & 0x001a) - 14)), unchecked((ushort)(slot.Position.Y.Whole + 16))),
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_frameContext._randomEnemyCounter & 7) == 0)
            _deadMonsters.LastSoundEffect = SoundEffectLibrary2Sounds.DeadMonsterDust.Value;
    }

    /// <summary>Appends the variant's native corpse tile transfer table after a rotting step.</summary>
    private void BuildDeadSidehopperVramTransfers(DeadSidehopperEnemyState state) =>
        _deadMonsters.AppendVramTransfers(state.VramTablePointer);

    /// <summary>Queues this frame's staged corpse tile uploads at their encoded VRAM destinations.</summary>
    [PpuLayoutBoundary("codec: staged dead-monster corpse DMA records' native destinations")]
    internal void QueueDeadSidehopperFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadMonsters.VramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                VramDmaDestination.FromNative(transfer.EncodedVramDestination));
        }
    }

    /// <summary>Selects a new animation and resets both instruction clocks before its first word runs.</summary>
    internal static void SetDeadSidehopperInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects missing or mismatched private state before dispatching the slot's corpse AI.</summary>
    private DeadSidehopperEnemyState RequireDeadSidehopperState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = _deadSidehopperStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no initialized dead-sidehopper state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead-sidehopper state belongs to a different slot.");
        return state;
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetDeadSidehopperRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginDeadSidehopperFrame();
    }

    /// <summary>Routes bomb-special hits on the corpse shot callback to its decay check.</summary>
    internal void RegisterDeadSidehopperBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecialAfterNoOp(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
                                           c.SelectedShotAi == EnemyInteractionCallback.DeadSidehopperShot,
            c => ResolveDeadSidehopperShot(c.Enemy));
    }

    /// <summary>Registers the corpse's retail initialization routine for sidehopper slots.</summary>
    internal void RegisterDeadSidehopperInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_CorpseSidehopper, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeDeadSidehopper(slot);
        });
    }

    /// <summary>Runs corpse AI with the current Samus, level, and camera context.</summary>
    internal void RegisterDeadSidehopperMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_HurtAI_CorpseEnemies, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper, (slot, frame, nmiFrameCounter8) =>
        {
            RunDeadSidehopperMain(slot, frame.Samus, frame.Level, frame.Camera.X);
        });
    }

    /// <summary>Claims the landing instruction that returns the actor to its post-animation AI state.</summary>
    internal void RegisterDeadSidehopperOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessDeadSidehopperInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Uses the private power-bomb callback in place of common enemy damage handling.</summary>
    internal void RegisterDeadSidehopperPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DeadSidehopperPowerBomb)
        {
            ReplacesCommon = ResolveDeadSidehopperPowerBomb,
        });
    }

    /// <summary>Marks projectile collision while withholding damage until the transformed corpse can decay.</summary>
    internal void RegisterDeadSidehopperShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DeadSidehopperShot)
        {
            // The common projectile prelude always marks the shot, but the private callback ignores it until the
            // corpse palette has fully transformed. Even then it starts rotting without vulnerability damage or impact.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ResolveDeadSidehopperShot(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Uses ordinary touch damage during transformation and starts decomposition afterward.</summary>
    internal void RegisterDeadSidehopperTouch(EnemyAiTable table)
    {
        // Before Shitroid finishes feeding, `$A9:DD44` uses normal contact damage without a death animation. At
        // palette stage eight it instead starts the corpse decomposition and never enters common touch damage.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DeadSidehopperTouch,
            context => ResolveDeadSidehopperTouch(context.Slot, RequireDeadSidehopperState(context.Slot), context.Samus));
    }
}
