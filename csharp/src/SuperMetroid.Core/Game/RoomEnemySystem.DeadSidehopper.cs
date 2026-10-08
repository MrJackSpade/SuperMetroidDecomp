using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail dead-sidehopper actors used by the Shitroid encounter and Tourian corpse rooms.
/// </summary>
public sealed partial class RoomEnemySystem
{
    public const ushort DeadSidehopperDefinition = 0xed7f;

    private const int DeadMonsterWorkBufferAddress = 0x7e2000;
    private const ushort DeadMonsterSolidProperty = 0x8000;
    private const ushort DeadMonsterInteractionRejectedProperty = 0x0400;

    private readonly DeadSidehopperEnemyState?[] _deadSidehopperStates =
        new DeadSidehopperEnemyState?[MaximumEnemyCount];
    private readonly List<VramWriteEntry> _deadSidehopperFrameVramTransfers = [];
    private RoomLevelData? _deadSidehopperLevel;
    private ushort _deadSidehopperCameraX;

    /// <summary>Last library-two dust sound requested by a completed sidehopper row.</summary>
    public ushort? LastDeadSidehopperSoundEffect { get; private set; }

    private void ResetDeadSidehopperRoomState()
    {
        Array.Clear(_deadSidehopperStates);
        _deadSidehopperFrameVramTransfers.Clear();
        LastDeadSidehopperSoundEffect = null;
        _deadSidehopperLevel = null;
        _deadSidehopperCameraX = 0;
    }

    private void BeginDeadSidehopperFrame()
    {
        _deadSidehopperFrameVramTransfers.Clear();
        LastDeadSidehopperSoundEffect = null;
    }

    /// <summary>Ports <c>DeadSidehopper_Init</c> at <c>$A9:D7B6</c>.</summary>
    private void InitializeDeadSidehopper(RoomEnemySlot slot)
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
        if (_slots[0].Properties.HasAny(EnemyProperties.Invisible))
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);

        slot.XPosition = 488;
        slot.YPosition = 184;
        slot.PaletteIndex = EnemyPaletteBits.Palette1;
        slot.YRadius = 21;
        SetDeadSidehopperInstruction(
            slot,
            DeadSidehopperInstructionProgramDefinitions.AliveIdle);

        DeadSidehopperEnemyState state = InitializeDeadSidehopperCorpseState(
            slot,
            graphicsVariant: 0,
            DeadSidehopperCorpseDefinitions.InitiallyAlive);
        state.Function = DeadSidehopperAiFunction.AliveWaitForCamera;
        state.PaletteStage = 0;
        state.HorizontalVelocity = 96;
        state.VerticalVelocity = 256;
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

    private DeadSidehopperEnemyState InitializeDeadSidehopperCorpseState(
        RoomEnemySlot slot,
        ushort graphicsVariant,
        DeadSidehopperCorpseDefinition definition)
    {
        if (definition.EntryCount == 0)
            throw new InvalidDataException("Dead sidehopper corpse configuration has zero rows.");

        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        var state = new DeadSidehopperEnemyState(
            slot,
            graphicsVariant,
            definition.ConfigurationPointer,
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.RotationTablePointer,
            definition.FinishFunction,
            definition.EntryCount,
            yLimit,
            lateMoveEntryIndex,
            definition.WrapOffset);
        _deadSidehopperStates[slot.SlotIndex] = state;

        CorpseRottingTableProcessor.Initialize(
            _bus!,
            0x7e0000 | definition.RottingTablePointer,
            definition.EntryCount);
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

    private void DispatchDeadSidehopperState(
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
                state.StateTimer = 64;
                return;

            case DeadSidehopperAiFunction.PostLandingDelay:
                state.StateTimer = unchecked((ushort)(state.StateTimer - 1));
                if ((state.StateTimer & 0x8000) == 0)
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
                state.VerticalVelocity = DeadSidehopperLaunchDefinitions.Vertical[state.JumpPhase];
                state.HorizontalVelocity = DeadSidehopperLaunchDefinitions.Horizontal[state.JumpPhase];
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
        bool beforeTerrainBoundary = unchecked((short)(slot.XPosition - 544)) < 0;
        int horizontalDisplacement = unchecked((short)state.HorizontalVelocity) << 8;
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: true,
                state.HorizontalVelocity);
        }
        else
        {
            MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                RequireDeadSidehopperLevel(level),
                slot,
                horizontalDisplacement);
        }

        // Upward motion accelerates by $20 in signed 8.8; falling motion accelerates by
        // $80. The sign test is performed before the addition in the cartridge.
        ushort acceleration = (state.VerticalVelocity & 0x8000) != 0
            ? (ushort)32
            : (ushort)128;
        state.VerticalVelocity = unchecked((ushort)(state.VerticalVelocity + acceleration));
        if (beforeTerrainBoundary)
        {
            MoveDeadSidehopperAxisDirect(
                slot,
                horizontal: false,
                state.VerticalVelocity);
            return unchecked((short)(slot.YPosition - 184)) >= 0;
        }

        int verticalDisplacement = unchecked((short)state.VerticalVelocity) << 8;
        return MoveEnemyVertically(
            RequireDeadSidehopperLevel(level),
            slot,
            verticalDisplacement);
    }

    private static void MoveDeadSidehopperAxisDirect(
        RoomEnemySlot slot,
        bool horizontal,
        ushort velocity)
    {
        ushort position = horizontal ? slot.XPosition : slot.YPosition;
        ushort subposition = horizontal ? slot.XSubposition : slot.YSubposition;
        int carry = (subposition >> 8) + (byte)velocity;
        subposition = unchecked((ushort)(
            (subposition & 0x00ff) | ((carry & 0xff) << 8)));
        position = unchecked((ushort)(
            position + unchecked((sbyte)(velocity >> 8)) + (carry >> 8)));
        if (horizontal)
        {
            slot.XPosition = position;
            slot.XSubposition = subposition;
        }
        else
        {
            slot.YPosition = position;
            slot.YSubposition = subposition;
        }
    }

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
        var colors = TileArtwork?.AuxiliaryColors ?? throw new InvalidDataException(
            "Dead-sidehopper palette transformation requires installed artwork.");
        for (int color = 0; color < 15; color++)
        {
            _cgram!.SetColor(
                0x91 + color,
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
        slot.YRadius = 12;
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
    private void ActivateDeadSidehopperVictim(RoomEnemySlot shitroid, ShitroidEnemyState state)
    {
        RoomEnemySlot victim = RequireShitroidVictim(shitroid);
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
        SamusState samus,
        ushort controllerInput)
    {
        if (state.PaletteStage < 8)
        {
            ResolveNormalEnemyTouch(slot, samus, controllerInput);
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

    private static void TriggerDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.Function = DeadSidehopperAiFunction.Rotting;
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision);
    }

    private void RunDeadSidehopperRotting(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            EnemyWorkMemory,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadSidehopperPixelRow(state, yOffset, move),
            entryIndex => FinishDeadSidehopperCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            state.Function = DeadSidehopperAiFunction.WaitForSamusCollision;
        BuildDeadSidehopperVramTransfers(state);
    }

    private void InitializeDeadSidehopperGraphics(ushort graphicsVariant)
    {
        ReadOnlySpan<byte> installedTiles = InstalledDeadTourianCorpseTiles();
        if (installedTiles.IsEmpty)
            throw new InvalidDataException("Dead sidehopper requires installed corpse artwork.");

        for (int row = 0; row < 5; row++)
        {
            var copy = DeadMonsterRottingDefinitions.SidehopperInitialCopy(graphicsVariant, row);
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                _bus!.WriteByte(
                    DeadMonsterWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    installedTiles[sourceOffset]);
            }
        }
    }

    private void CopyOrMoveDeadSidehopperPixelRow(
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
                WriteDeadMonsterWorkWord(destinationWord, ReadDeadMonsterWorkWord(sourceWord));
                WriteDeadMonsterWorkWord(
                    destinationWord + 8,
                    ReadDeadMonsterWorkWord(sourceWord + 8));
            }

            if (move)
            {
                WriteDeadMonsterWorkWord(sourceWord, 0);
                WriteDeadMonsterWorkWord(sourceWord + 8, 0);
            }
        }
    }

    private void FinishDeadSidehopperCorpseRow(
        RoomEnemySlot slot,
        DeadSidehopperEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = RequireRandomNumber();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(slot.XPosition + (random & 0x001a) - 14)),
            unchecked((ushort)(slot.YPosition + 16)),
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadSidehopperSoundEffect = 0x0010;
    }

    private void BuildDeadSidehopperVramTransfers(DeadSidehopperEnemyState state) =>
        AppendDeadMonsterVramTransfers(state.VramTablePointer);

    private void QueueDeadSidehopperFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadSidehopperFrameVramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                transfer.EncodedVramDestination);
        }
    }

    private static void SetDeadSidehopperInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private DeadSidehopperEnemyState RequireDeadSidehopperState(RoomEnemySlot slot)
    {
        DeadSidehopperEnemyState state = _deadSidehopperStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no initialized dead-sidehopper state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead-sidehopper state belongs to a different slot.");
        return state;
    }

    private ushort ReadDeadMonsterWorkWord(int wordOffset) =>
        SnesWorkRam.ReadWord(EnemyWorkMemory, DeadMonsterWorkBufferAddress + wordOffset * 2);

    private void WriteDeadMonsterWorkWord(int wordOffset, ushort value) =>
        WriteWord(_bus!, DeadMonsterWorkBufferAddress + wordOffset * 2, value);


}

/// <summary>Native bank-$A9 dead-sidehopper function pointers.</summary>
public enum DeadSidehopperAiFunction : ushort
{
    AliveWaitForCamera = 0xd8e2,
    ActivatedMovement = 0xd8f1,
    NoOperation = 0xd90f,
    BeginPostLandingDelay = 0xd910,
    PostLandingDelay = 0xd91d,
    TransformPalette = 0xda08,
    WaitForSamusCollision = 0xda64,
    PreRotDelay = 0xda8f,
    Rotting = 0xdaba,
}

/// <summary>Typed projection of one dead sidehopper's extended bank-$A9 WRAM.</summary>
public sealed class DeadSidehopperEnemyState
{
    internal DeadSidehopperEnemyState(
        RoomEnemySlot slot,
        ushort graphicsVariant,
        ushort configurationPointer,
        ushort tablePointer,
        ushort vramTablePointer,
        ushort copyFunction,
        ushort moveFunction,
        ushort rotationTablePointer,
        ushort finishFunction,
        ushort entryCount,
        ushort yLimit,
        ushort lateMoveEntryIndex,
        ushort wrapOffset)
    {
        Slot = slot;
        GraphicsVariant = graphicsVariant;
        ConfigurationPointer = configurationPointer;
        TablePointer = tablePointer;
        VramTablePointer = vramTablePointer;
        CopyFunction = copyFunction;
        MoveFunction = moveFunction;
        RotationTablePointer = rotationTablePointer;
        FinishFunction = finishFunction;
        EntryCount = entryCount;
        YLimit = yLimit;
        LateMoveEntryIndex = lateMoveEntryIndex;
        WrapOffset = wrapOffset;
    }

    public RoomEnemySlot Slot { get; }
    public ushort GraphicsVariant { get; }
    public ushort ConfigurationPointer { get; }
    public ushort TablePointer { get; }
    public ushort VramTablePointer { get; }
    public ushort CopyFunction { get; }
    public ushort MoveFunction { get; }
    public ushort RotationTablePointer { get; }
    public ushort FinishFunction { get; }
    public ushort EntryCount { get; }
    public ushort YLimit { get; }
    public ushort LateMoveEntryIndex { get; }
    public ushort WrapOffset { get; }

    public DeadSidehopperAiFunction Function
    {
        get => (DeadSidehopperAiFunction)Slot.VariableA;
        internal set => Slot.VariableA = (ushort)value;
    }

    /// <summary>Native extended word 06, selecting one of four jump velocity pairs.</summary>
    public ushort JumpPhase { get; internal set; }

    /// <summary>Native extended word 07, the eight-frame palette divider.</summary>
    public ushort PaletteFrameCounter { get; internal set; }

    /// <summary>Native extended word 08, written to one by Shitroid after feeding.</summary>
    public ushort PaletteStage { get; internal set; }

    /// <summary>Native signed 8.8 horizontal velocity word 0A.</summary>
    public ushort HorizontalVelocity { get; internal set; }

    /// <summary>Native signed 8.8 vertical velocity word 0B.</summary>
    public ushort VerticalVelocity { get; internal set; }

    /// <summary>Native AI variable B, deliberately retained across repeated corpse contacts.</summary>
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }

    /// <summary>Native AI variable F used by the 64-frame post-landing pause.</summary>
    public ushort StateTimer
    {
        get => Slot.VariableF;
        internal set => Slot.VariableF = value;
    }

    public uint ProcessCallCount { get; internal set; }
    public uint FinishedEntryCount { get; internal set; }
    public uint DustSpawnCount { get; internal set; }
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
