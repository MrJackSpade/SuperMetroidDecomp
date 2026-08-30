using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Dead Zoomer, Ripper, and Skree actors sharing bank-$A9's corpse-rotting engine.
/// </summary>
public sealed partial class RoomEnemySystem
{
    public const ushort DeadZoomerDefinition = 0xedff;
    public const ushort DeadRipperDefinition = 0xee3f;
    public const ushort DeadSkreeDefinition = 0xee7f;

    private const ushort DeadTourianCorpseNoOperationFunction = 0xda63;

    private static readonly DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        DeadZoomerDefinition,
        InstructionPointerTable: 0xd86a,
        ConfigurationPointerTable: 0xd870,
        VariantCount: 3,
        WaitFunction: 0xda69,
        PreRotFunction: 0xda94,
        RottingFunction: 0xdad0,
        TouchAndShotFunction: 0xdcf8,
        PowerBombFunction: 0xdced,
        Variants:
        [
            new(0xe6b9, 0xe66a, 0xdf4f, 14, [1184, 1200, 1216],
            [
                new(0x0a60, 0x0940, 0x0060),
                new(0x0c60, 0x09a0, 0x0060),
            ]),
            new(0xe745, 0xe6f6, 0xdf6c, 14, [1280, 1296, 1312],
            [
                new(0x0ac0, 0x0a00, 0x0060),
                new(0x0cc0, 0x0a60, 0x0060),
            ]),
            new(0xe7d1, 0xe782, 0xdf89, 14, [1376, 1392, 1408],
            [
                new(0x0b20, 0x0ac0, 0x0060),
                new(0x0d20, 0x0b20, 0x0060),
            ]),
        ]);

    private static readonly DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        DeadRipperDefinition,
        InstructionPointerTable: 0xd897,
        ConfigurationPointerTable: 0xd89b,
        VariantCount: 2,
        WaitFunction: 0xda73,
        PreRotFunction: 0xda99,
        RottingFunction: 0xdae6,
        TouchAndShotFunction: 0xdd08,
        PowerBombFunction: 0xdcfd,
        Variants:
        [
            new(0xe85d, 0xe80e, 0xdfa6, 14, [1472, 1488, 1504],
            [
                new(0x0a00, 0x0b80, 0x0060),
                new(0x0c00, 0x0be0, 0x0060),
            ]),
            new(0xe8e9, 0xe89a, 0xdfc3, 14, [1568, 1584, 1600],
            [
                new(0x0b80, 0x0c40, 0x0060),
                new(0x0d80, 0x0ca0, 0x0060),
            ]),
        ]);

    private static readonly DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        DeadSkreeDefinition,
        InstructionPointerTable: 0xd8c0,
        ConfigurationPointerTable: 0xd8c6,
        VariantCount: 3,
        WaitFunction: 0xda6e,
        PreRotFunction: 0xda9e,
        RottingFunction: 0xdafc,
        TouchAndShotFunction: 0xdd18,
        PowerBombFunction: 0xdd0d,
        Variants:
        [
            new(0xe95b, 0xe926, 0xdfe0, 30, [800, 816],
            [
                new(0x02a0, 0x0640, 0x0040),
                new(0x04a0, 0x0680, 0x0040),
                new(0x06a0, 0x06c0, 0x0040),
                new(0x08a0, 0x0700, 0x0040),
            ]),
            new(0xe9b9, 0xe984, 0xe019, 30, [928, 944],
            [
                new(0x00e0, 0x0740, 0x0040),
                new(0x02e0, 0x0780, 0x0040),
                new(0x04e0, 0x07c0, 0x0040),
                new(0x06e0, 0x0800, 0x0040),
            ]),
            new(0xea17, 0xe9e2, 0xe052, 30, [1056, 1072],
            [
                new(0x01c0, 0x0840, 0x0040),
                new(0x03c0, 0x0880, 0x0040),
                new(0x05c0, 0x08c0, 0x0040),
                new(0x07c0, 0x0900, 0x0040),
            ]),
        ]);

    private readonly DeadTourianCorpseEnemyState?[] _deadTourianCorpseStates =
        new DeadTourianCorpseEnemyState?[MaximumEnemyCount];

    /// <summary>Dead Zoomer/Ripper/Skree state indexed by physical enemy slot.</summary>
    public IReadOnlyList<DeadTourianCorpseEnemyState?> DeadTourianCorpses =>
        _deadTourianCorpseStates;

    private void ResetDeadTourianCorpseRoomState() =>
        Array.Clear(_deadTourianCorpseStates);

    private static bool IsDeadTourianCorpseDefinition(ushort definitionPointer) =>
        definitionPointer is DeadZoomerDefinition or DeadRipperDefinition or DeadSkreeDefinition;

    private static DeadTourianCorpseProfile ProfileForDeadTourianCorpse(ushort definitionPointer) =>
        definitionPointer switch
        {
            DeadZoomerDefinition => DeadZoomerProfile,
            DeadRipperDefinition => DeadRipperProfile,
            DeadSkreeDefinition => DeadSkreeProfile,
            _ => throw new ArgumentOutOfRangeException(nameof(definitionPointer)),
        };

    private static bool HasDeadTourianCorpseTouchOrShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.TouchAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    private static bool HasDeadTourianCorpseShotCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.ShotAiPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).TouchAndShotFunction;
    }

    private static bool HasDeadTourianCorpsePowerBombCallback(RoomEnemySlot slot)
    {
        if (!IsDeadTourianCorpseDefinition(slot.EnemyDefinitionPointer))
            return false;
        return slot.Definition.PowerBombReactionPointer ==
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer).PowerBombFunction;
    }

    /// <summary>Ports initializers <c>$A9:D849/$D876/$D89F</c>.</summary>
    private void InitializeDeadTourianCorpse(RoomEnemySlot slot)
    {
        DeadTourianCorpseProfile profile =
            ProfileForDeadTourianCorpse(slot.EnemyDefinitionPointer);
        if ((slot.Parameter1 & 1) != 0 || slot.Parameter1 / 2 >= profile.VariantCount)
        {
            throw new NotSupportedException(
                $"Dead {profile.Species} parameter 1 ${slot.Parameter1:X4} has no retail variant.");
        }

        int variantIndex = slot.Parameter1 / 2;
        DeadTourianCorpseVariant variant = profile.Variants[variantIndex];
        ushort instructionPointer = ReadWord(
            _bus!,
            0xa90000 | unchecked((ushort)(profile.InstructionPointerTable + variantIndex * 2)));
        ushort configurationPointer = ReadWord(
            _bus!,
            0xa90000 | unchecked((ushort)(profile.ConfigurationPointerTable + variantIndex * 2)));
        int configurationAddress = 0xa90000 | configurationPointer;
        ushort tablePointer = ReadWord(_bus!, configurationAddress);
        ushort vramTablePointer = ReadWord(_bus!, configurationAddress + 2);
        ushort copyFunction = ReadWord(_bus!, configurationAddress + 4);
        ushort moveFunction = ReadWord(_bus!, configurationAddress + 6);
        ushort entryCount = ReadWord(_bus!, configurationAddress + 8);
        ushort graphicsInitFunction = ReadWord(_bus!, configurationAddress + 10);
        ushort rotationTablePointer = ReadWord(_bus!, configurationAddress + 12);
        ushort finishFunction = ReadWord(_bus!, configurationAddress + 14);

        if (copyFunction != variant.CopyFunction ||
            moveFunction != variant.MoveFunction ||
            graphicsInitFunction != variant.GraphicsInitFunction ||
            finishFunction != DeadMonsterFinishedFunction ||
            entryCount == 0)
        {
            throw new InvalidDataException(
                $"Dead {profile.Species} variant {variantIndex} configuration " +
                $"$A9:{configurationPointer:X4} selected callbacks " +
                $"${copyFunction:X4}/${moveFunction:X4}/${graphicsInitFunction:X4}/" +
                $"${finishFunction:X4} and {entryCount} rows.");
        }

        ushort yLimit = unchecked((ushort)(entryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        ushort wrapOffset = unchecked((ushort)(
            ReadWord(_bus!, 0xa90000 | unchecked((ushort)(rotationTablePointer + 2))) - 12));
        var state = new DeadTourianCorpseEnemyState(
            slot,
            profile.Species,
            variantIndex,
            configurationPointer,
            tablePointer,
            vramTablePointer,
            copyFunction,
            moveFunction,
            rotationTablePointer,
            finishFunction,
            entryCount,
            yLimit,
            lateMoveEntryIndex,
            wrapOffset,
            profile,
            variant);
        _deadTourianCorpseStates[slot.SlotIndex] = state;

        slot.PaletteIndex = 0x0e00;
        slot.VariableA = profile.WaitFunction;
        SetDeadSidehopperInstruction(slot, instructionPointer);
        CorpseRottingTableProcessor.Initialize(_bus!, 0x7e0000 | tablePointer, entryCount);
        InitializeDeadTourianCorpseGraphics(variant);
    }

    /// <summary>All three headers point main AI at shared dispatcher <c>$A9:D8DB</c>.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        DeadTourianCorpseProfile profile = state.Profile;
        if (slot.VariableA == profile.WaitFunction)
        {
            if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                slot.VariableA = profile.PreRotFunction;
            return;
        }

        if (slot.VariableA == profile.PreRotFunction)
        {
            state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
            if (state.PreRotDelayCounter >= 16)
            {
                slot.VariableA = profile.RottingFunction;
                slot.Properties = unchecked((ushort)(
                    slot.Properties | DeadMonsterInteractionRejectedProperty));
            }
            return;
        }

        if (slot.VariableA == profile.RottingFunction)
        {
            RunDeadTourianCorpseRotting(slot, state);
            return;
        }

        if (slot.VariableA != DeadTourianCorpseNoOperationFunction)
        {
            throw new NotSupportedException(
                $"Dead {profile.Species} function $A9:{slot.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Touch and shot callbacks for these three corpse-only actors are identical.</summary>
    private void TriggerDeadTourianCorpseRotting(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = RequireDeadTourianCorpseState(slot);
        slot.VariableA = state.Profile.RottingFunction;
        slot.Properties = unchecked((ushort)(slot.Properties | 0x0c00));
    }

    /// <summary>Power-bomb callbacks reject actors whose decomposition already set $0400.</summary>
    private void ResolveDeadTourianCorpsePowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTourianCorpseState(slot);
        if ((slot.Properties & DeadMonsterInteractionRejectedProperty) == 0)
            TriggerDeadTourianCorpseRotting(slot);
    }

    private void RunDeadTourianCorpseRotting(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state)
    {
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTourianCorpsePixelRow(state, yOffset, move),
            entryIndex => FinishDeadTourianCorpseRow(slot, state, entryIndex));
        if (!stillRotting)
            slot.VariableA = DeadTourianCorpseNoOperationFunction;
        AppendDeadMonsterVramTransfers(state.VramTablePointer);
    }

    private void InitializeDeadTourianCorpseGraphics(DeadTourianCorpseVariant variant)
    {
        foreach (DeadTourianCorpseGraphicsCopy copy in variant.InitialGraphicsCopies)
        {
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                _bus!.WriteByte(
                    DeadMonsterWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    _bus.ReadByte(DeadMonsterTileDataAddress + copy.SourceOffset + byteIndex));
            }
        }
    }

    private void CopyOrMoveDeadTourianCorpsePixelRow(
        DeadTourianCorpseEnemyState state,
        ushort yOffset,
        bool move)
    {
        int rotationEntryAddress = 0xa90000 |
            unchecked((ushort)(state.RotationTablePointer + (yOffset >> 3) * 2));
        ushort sourceOffset = unchecked((ushort)(
            ReadWord(_bus!, rotationEntryAddress) + (yOffset & 7) * 2));
        ushort destinationOffset = (yOffset & 7) >= 6
            ? unchecked((ushort)(state.WrapOffset + sourceOffset))
            : sourceOffset;

        foreach (ushort columnWordOffset in state.Variant.ColumnWordOffsets)
        {
            int sourceWord = sourceOffset / 2 + columnWordOffset;
            int destinationWord = destinationOffset / 2 + columnWordOffset + 1;
            if (yOffset < state.Variant.MaximumY)
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

    private void FinishDeadTourianCorpseRow(
        RoomEnemySlot slot,
        DeadTourianCorpseEnemyState state,
        ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = _readRandomNumber?.Invoke() ?? 0;
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)(slot.XPosition + (random & 0x001a) - 14)),
            unchecked((ushort)(slot.YPosition + 16)),
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadSidehopperSoundEffect = 0x0010;
    }

    private void AppendDeadMonsterVramTransfers(ushort vramTablePointer)
    {
        ushort cursor = vramTablePointer;
        for (int recordIndex = 0; recordIndex < 64; recordIndex++, cursor += 8)
        {
            int address = 0xa90000 | cursor;
            ushort size = ReadWord(_bus!, address);
            if (size == 0)
                return;

            ushort sourceBankWord = ReadWord(_bus!, address + 2);
            ushort sourceOffset = ReadWord(_bus!, address + 4);
            ushort vramDestination = ReadWord(_bus!, address + 6);
            int sourceAddress = ((sourceBankWord & 0xff00) << 8) | sourceOffset;
            _deadSidehopperFrameVramTransfers.Add(new VramWriteEntry(
                size,
                sourceAddress,
                vramDestination));
        }

        throw new InvalidDataException(
            $"Dead-monster VRAM table $A9:{vramTablePointer:X4} has no terminator.");
    }

    private DeadTourianCorpseEnemyState RequireDeadTourianCorpseState(RoomEnemySlot slot)
    {
        DeadTourianCorpseEnemyState state = _deadTourianCorpseStates[slot.SlotIndex] ??
            throw new InvalidDataException(
                $"Enemy slot {slot.SlotIndex} has no dead Tourian corpse state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("Dead Tourian corpse state belongs to another slot.");
        return state;
    }

    internal sealed record DeadTourianCorpseProfile(
        DeadTourianCorpseSpecies Species,
        ushort DefinitionPointer,
        ushort InstructionPointerTable,
        ushort ConfigurationPointerTable,
        int VariantCount,
        ushort WaitFunction,
        ushort PreRotFunction,
        ushort RottingFunction,
        ushort TouchAndShotFunction,
        ushort PowerBombFunction,
        DeadTourianCorpseVariant[] Variants);

    internal sealed record DeadTourianCorpseVariant(
        ushort CopyFunction,
        ushort MoveFunction,
        ushort GraphicsInitFunction,
        ushort MaximumY,
        ushort[] ColumnWordOffsets,
        DeadTourianCorpseGraphicsCopy[] InitialGraphicsCopies);

    internal readonly record struct DeadTourianCorpseGraphicsCopy(
        int SourceOffset,
        int DestinationOffset,
        int Length);
}

/// <summary>The three non-sidehopper dead-monster graphics families.</summary>
public enum DeadTourianCorpseSpecies
{
    Zoomer,
    Ripper,
    Skree,
}

/// <summary>Typed extended state for one dead Zoomer, Ripper, or Skree.</summary>
public sealed class DeadTourianCorpseEnemyState
{
    internal DeadTourianCorpseEnemyState(
        RoomEnemySlot slot,
        DeadTourianCorpseSpecies species,
        int variantIndex,
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
        ushort wrapOffset,
        RoomEnemySystem.DeadTourianCorpseProfile profile,
        RoomEnemySystem.DeadTourianCorpseVariant variant)
    {
        Slot = slot;
        Species = species;
        VariantIndex = variantIndex;
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
        Profile = profile;
        Variant = variant;
    }

    public RoomEnemySlot Slot { get; }
    public DeadTourianCorpseSpecies Species { get; }
    public int VariantIndex { get; }
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
    public ushort PreRotDelayCounter
    {
        get => Slot.VariableB;
        internal set => Slot.VariableB = value;
    }
    public uint ProcessCallCount { get; internal set; }
    public uint FinishedEntryCount { get; internal set; }
    public uint DustSpawnCount { get; internal set; }
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;

    // Callback profiles remain assembly-internal. Public debugger state exposes only the
    // stable ROM-derived words above; actor code keeps typed access without parallel maps.
    internal RoomEnemySystem.DeadTourianCorpseProfile Profile { get; }
    internal RoomEnemySystem.DeadTourianCorpseVariant Variant { get; }
}
