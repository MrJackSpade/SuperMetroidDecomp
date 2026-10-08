using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Retail Dead Torizo actor and its bank-$A9 corpse-rotting graphics path.</summary>
public sealed partial class RoomEnemySystem
{
    public const ushort DeadTorizoDefinition = 0xed3f;

    private const ushort DeadTorizoWaitFunction = 0xd3ad;
    private const ushort DeadTorizoPreRotFunction = 0xd3c8;
    private const ushort DeadTorizoRottingFunction = 0xd3e6;
    private const ushort DeadTorizoNoOperationFunction = 0xd3c7;
    private const int DeadTorizoWorkBufferAddress = 0x7e2000;
    private const int DeadTorizoWorkBufferSize = 0x1000;
    private const int DeadTorizoSandBufferAddress = 0x7e9500;

    private readonly List<VramWriteEntry> _deadTorizoFrameVramTransfers = [];
    private DeadTorizoEnemyState? _deadTorizo;

    /// <summary>Last library-two sound emitted by a completed corpse row.</summary>
    public ushort? LastDeadTorizoSoundEffect { get; private set; }

    private void ResetDeadTorizoRoomState()
    {
        _deadTorizo = null;
        _deadTorizoFrameVramTransfers.Clear();
        LastDeadTorizoSoundEffect = null;
    }

    /// <summary>Ports <c>DeadTorizo_Init</c> at <c>$A9:D308</c>.</summary>
    private void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Dead Torizo requires native slot zero, not slot {slot.SlotIndex}.");
        }

        for (int offset = 0; offset < DeadTorizoWorkBufferSize; offset++)
            _bus!.WriteByte(DeadTorizoWorkBufferAddress + offset, 0);

        slot.VariableA = DeadTorizoWaitFunction;
        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        slot.CurrentInstruction = DeadTorizoInstructionProgramDefinitions.Stationary;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.PaletteIndex = EnemyPaletteBits.Palette1;
        slot.VariableB = 0;
        slot.VariableC = 8;

        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;
        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        _deadTorizo = new DeadTorizoEnemyState(
            slot,
            definition.RottingTablePointer,
            definition.VramTransferPointer,
            definition.CopyFunction,
            definition.MoveFunction,
            definition.RotationTablePointer,
            definition.FinishFunction,
            definition.EntryCount,
            yLimit,
            lateMoveEntryIndex,
            definition.WrapOffset)
        {
            SandLineCounter = 15,
        };

        CorpseRottingTableProcessor.Initialize(
            _bus!,
            0x7e0000 | definition.RottingTablePointer,
            definition.EntryCount);
        InitializeDeadTorizoGraphics();
    }

    /// <summary>Ports <c>DeadTorizo_Main</c> at <c>$A9:D368</c>.</summary>
    private void RunDeadTorizoMain(RoomEnemySlot slot, SamusState? samus)
    {
        DeadTorizoEnemyState state = RequireDeadTorizoState(slot);

        // The custom detector remains enabled while the intact corpse is solid. Rotting
        // disables interaction separately, after publishing Samus's external displacement.
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) &&
            samus is not null &&
            DeadTorizoCustomHitboxOverlaps(slot, samus))
        {
            TriggerDeadTorizoRotting(slot);
        }

        switch (slot.VariableA)
        {
            case DeadTorizoWaitFunction:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    slot.VariableA = DeadTorizoPreRotFunction;
                break;

            case DeadTorizoPreRotFunction:
                state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
                if (state.PreRotDelayCounter >= 0x10)
                {
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                    slot.VariableA = DeadTorizoRottingFunction;
                    RunDeadTorizoRotting(slot, state);
                }
                break;

            case DeadTorizoRottingFunction:
                RunDeadTorizoRotting(slot, state);
                break;

            case DeadTorizoNoOperationFunction:
                break;

            default:
                throw new InvalidDataException(
                    $"Dead Torizo function $A9:{slot.VariableA:X4} is not translated.");
        }

        BuildDeadTorizoVramTransfers(state);
    }

    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.SandFrameCounter = unchecked((ushort)(state.SandFrameCounter + 1));
        if (state.SandFrameCounter >= 0x0f)
        {
            state.SandFrameCounter = 0;
            if (state.SandLineCounter != 0)
            {
                CopyDeadTorizoSandLine(state.SandLineCounter);
                state.SandLineCounter = unchecked((ushort)(state.SandLineCounter - 1));
                state.SandLineCopyCount++;
            }
        }

        slot.VariableC = unchecked((ushort)(slot.VariableC + 1));
        MoveBankA9EnemyWithVelocity(slot);
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _bus!,
            EnemyWorkMemory,
            0x7e0000 | state.TablePointer,
            state.EntryCount,
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTorizoPixelRow(state, yOffset, move),
            entryIndex => FinishDeadTorizoCorpseRow(state, entryIndex));
        if (!stillRotting)
            slot.VariableA = DeadTorizoNoOperationFunction;
    }

    /// <summary>Shot/touch tail at <c>$A9:D433</c>.</summary>
    private void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.VariableA = DeadTorizoRottingFunction;
    }

    /// <summary>Power-bomb entry at <c>$A9:D42A</c>.</summary>
    private void TriggerDeadTorizoPowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
            TriggerDeadTorizoRotting(slot);
    }

    private static bool DeadTorizoCustomHitboxOverlaps(RoomEnemySlot slot, SamusState samus)
    {
        SamusKinematicsState kinematics = samus.Kinematics;
        foreach (DeadCorpseTouchHitbox hitbox in DeadMonsterRottingDefinitions.TorizoTouchHitboxes)
        {
            ushort verticalDistance;
            ushort verticalRadius;
            if (unchecked((short)(kinematics.YPosition - slot.YPosition)) >= 0)
            {
                verticalDistance = unchecked((ushort)(kinematics.YPosition - slot.YPosition));
                verticalRadius = hitbox.Bottom;
            }
            else
            {
                verticalDistance = unchecked((ushort)(slot.YPosition - kinematics.YPosition));
                verticalRadius = hitbox.Top;
            }

            ushort verticalOverlap = unchecked((ushort)(
                kinematics.YRadius + NativeAbsolute(verticalRadius) - verticalDistance));
            if (unchecked((short)verticalOverlap) < 0)
                continue;

            ushort horizontalDistance;
            ushort horizontalRadius;
            if (unchecked((short)(kinematics.XPosition - slot.XPosition)) >= 0)
            {
                horizontalDistance = unchecked((ushort)(kinematics.XPosition - slot.XPosition));
                horizontalRadius = hitbox.Right;
            }
            else
            {
                horizontalDistance = unchecked((ushort)(slot.XPosition - kinematics.XPosition));
                horizontalRadius = hitbox.Left;
            }

            short overlap = unchecked((short)(
                kinematics.XRadius + NativeAbsolute(horizontalRadius) - horizontalDistance));
            if (overlap < 0)
                continue;
            if (overlap < 4)
                overlap = 4;

            kinematics.ExtraXDisplacement = unchecked((ushort)overlap);
            kinematics.ExtraYDisplacement = 4;
            kinematics.ExtraXSubdisplacement = 0;
            kinematics.ExtraYSubdisplacement = 0;
            return true;
        }

        return false;
    }

    private void InitializeDeadTorizoGraphics()
    {
        ReadOnlySpan<byte> installedTiles = DeadTorizoInstalledTiles();
        if (installedTiles.IsEmpty)
            throw new InvalidDataException("Dead Torizo requires installed corpse artwork.");
        for (int row = 0; row < DeadTorizoGeometryDefinitions.Rows; row++)
        {
            DeadTorizoGraphicsCopy copy = DeadTorizoGeometryDefinitions.InitialCopy(row);
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                _bus!.WriteByte(
                    DeadTorizoWorkBufferAddress + copy.DestinationOffset + byteIndex,
                    installedTiles[sourceOffset]);
            }
        }
    }

    /// <summary>
    /// The enemy's ordinary extracted PNG already covers the complete bank-$B7 source
    /// sheet. Reuse that same edited planar image for both the initial corpse copy and
    /// later sand rows; neither path may bypass a bound installation to read the ROM.
    /// Constructed cartridge-only fixtures retain the native source path.
    /// </summary>
    private ReadOnlySpan<byte> DeadTorizoInstalledTiles()
    {
        if (TileArtwork is null)
            return [];
        if (!TileArtwork.TryResolve(DeadTorizoArtworkDefinitions.SourceAddress,
                DeadTorizoArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles))
            throw new InvalidDataException(
                "Installed dead-Torizo sheet enemy-ed3f-tiles.png is missing or has the wrong size.");
        return tiles.Span;
    }

    private void CopyOrMoveDeadTorizoPixelRow(
        DeadTorizoEnemyState state,
        ushort yOffset,
        bool move)
    {
        ushort sourceOffset = unchecked((ushort)(
            DeadMonsterRottingDefinitions.RotationOffset(state.RotationTablePointer, yOffset) + (yOffset & 7) * 2));
        ushort destinationOffset = (yOffset & 7) >= 6
            ? unchecked((ushort)(state.WrapOffset + sourceOffset))
            : sourceOffset;

        for (int columnIndex = 0; columnIndex < DeadTorizoGeometryDefinitions.Columns; columnIndex++)
        {
            if (yOffset < DeadTorizoGeometryDefinitions.ColumnMinimumY(columnIndex))
                continue;

            int sourceWord = sourceOffset / 2 + DeadTorizoGeometryDefinitions.ColumnWordOffset(columnIndex);
            int destinationWord = destinationOffset / 2 + DeadTorizoGeometryDefinitions.ColumnWordOffset(columnIndex) + 1;
            if (yOffset < 94)
            {
                WriteWorkWord(destinationWord, ReadWorkWord(sourceWord));
                WriteWorkWord(destinationWord + 8, ReadWorkWord(sourceWord + 8));
            }

            if (move)
            {
                WriteWorkWord(sourceWord, 0);
                WriteWorkWord(sourceWord + 8, 0);
            }
        }
    }

    private void FinishDeadTorizoCorpseRow(DeadTorizoEnemyState state, ushort entryIndex)
    {
        state.FinishedEntryCount++;
        state.LastFinishedEntryIndex = entryIndex;
        ushort random = RequireRandomNumber();
        SpawnRoomGraphicsDustExplosion(
            unchecked((ushort)((random & 0x001f) + 272)),
            188,
            animationIndex: 10);
        state.DustSpawnCount++;
        if ((_randomEnemyCounter & 7) == 0)
            LastDeadTorizoSoundEffect = 0x0010;
    }

    private void CopyDeadTorizoSandLine(ushort lineIndex)
    {
        ReadOnlySpan<byte> installedTiles = DeadTorizoInstalledTiles();
        ushort destinationOffset = DeadMonsterRottingDefinitions.SandDestination(lineIndex);
        ushort sourceOffset = DeadMonsterRottingDefinitions.SandSource(lineIndex);

        // Eighteen tile rows are sixteen bytes apart in both the source sheet and the WRAM
        // heap surface. Only the first 16-bit bitplane word of each row is replaced.
        for (int row = 0; row < 18; row++)
        {
            int tileOffset = sourceOffset + row * 16;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(installedTiles.Slice(tileOffset, 2));
            WriteWord(
                _bus!,
                DeadTorizoSandBufferAddress + destinationOffset + row * 16,
                value);
        }
    }

    /// <summary>
    /// Applies bank-$A9's shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same cartridge helper; retaining one implementation also makes its
    /// unusual use of the high byte of the subposition explicit.
    /// </summary>
    private static void MoveBankA9EnemyWithVelocity(RoomEnemySlot slot)
    {
        int xCarry = (slot.XSubposition >> 8) + (byte)slot.VariableB;
        slot.XSubposition = unchecked((ushort)(
            (slot.XSubposition & 0x00ff) | ((xCarry & 0xff) << 8)));
        slot.XPosition = unchecked((ushort)(
            slot.XPosition + unchecked((sbyte)(slot.VariableB >> 8)) + (xCarry >> 8)));

        int yCarry = (slot.YSubposition >> 8) + (byte)slot.VariableC;
        slot.YSubposition = unchecked((ushort)(
            (slot.YSubposition & 0x00ff) | ((yCarry & 0xff) << 8)));
        slot.YPosition = unchecked((ushort)(
            slot.YPosition + unchecked((sbyte)(slot.VariableC >> 8)) + (yCarry >> 8)));
    }

    private void BuildDeadTorizoVramTransfers(DeadTorizoEnemyState state)
    {
        state.VramTransferPhase = unchecked((ushort)(state.VramTransferPhase + 1));
        // Immutable descriptors point at live WRAM corpse/sand staging surfaces.
        foreach (DeadTorizoVramTransferDefinition record in
                 DeadTorizoVramTransferDefinitions.ForPhase(state.VramTransferPhase))
            _deadTorizoFrameVramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.EncodedVramDestination));
    }

    private void QueueDeadTorizoFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadTorizoFrameVramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                transfer.EncodedVramDestination);
        }
    }

    /// <summary>Runs the post-enemy ordinary-spritemap hook at <c>$A9:D39A</c>.</summary>
    private void DrawDeadTorizoHook(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        if (_deadTorizo is null)
            return;

        ushort screenY = unchecked((ushort)(187 - cameraY));
        if (unchecked((short)screenY) < 0)
            return;
        DrawEnemySpritemap(
            oam,
            DeadTorizoArtworkDefinitions.SpritemapBank,
            DeadTorizoArtworkDefinitions.HookSpritemap,
            unchecked((ushort)(296 - cameraX)),
            screenY,
            0,
            0);
    }

    private DeadTorizoEnemyState RequireDeadTorizoState(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = _deadTorizo ??
            throw new InvalidDataException("Dead Torizo has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Dead-Torizo slot entered its dispatcher.");
        return state;
    }

    private ushort ReadWorkWord(int wordOffset) =>
        SnesWorkRam.ReadWord(EnemyWorkMemory, DeadTorizoWorkBufferAddress + wordOffset * 2);

    private void WriteWorkWord(int wordOffset, ushort value) =>
        WriteWord(_bus!, DeadTorizoWorkBufferAddress + wordOffset * 2, value);

    private static ushort NativeAbsolute(ushort value) =>
        (value & 0x8000) == 0 ? value : unchecked((ushort)(~value + 1));

    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }

}

/// <summary>Typed projection of Dead Torizo's bank-$A9 extended WRAM fields.</summary>
public sealed class DeadTorizoEnemyState
{
    internal DeadTorizoEnemyState(
        RoomEnemySlot slot,
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
    public ushort VramTransferPhase { get; internal set; }
    public ushort PreRotDelayCounter { get; internal set; }
    public ushort SandLineCounter { get; internal set; }
    public ushort SandFrameCounter { get; internal set; }
    public uint ProcessCallCount { get; internal set; }
    public uint FinishedEntryCount { get; internal set; }
    public uint DustSpawnCount { get; internal set; }
    public uint SandLineCopyCount { get; internal set; }
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
