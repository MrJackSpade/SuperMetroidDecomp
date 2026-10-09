using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Retail Dead Torizo actor and its bank-$A9 corpse-rotting graphics path.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Bank-$A0 enemy definition $ED3F for the Tourian Torizo corpse, initialized by $A9:D308 and required to own native enemy slot zero.</summary>
    public const ushort DeadTorizoDefinition = 0xed3f;

    /// <summary>Bank-$A9 dispatcher word for the intact corpse waiting for contact.</summary>
    private const ushort DeadTorizoWaitFunction = 0xd3ad;
    /// <summary>Bank-$A9 dispatcher word for the sixteen-call delay before rot begins.</summary>
    private const ushort DeadTorizoPreRotFunction = 0xd3c8;
    /// <summary>Bank-$A9 dispatcher word for active corpse and sand progression.</summary>
    private const ushort DeadTorizoRottingFunction = 0xd3e6;
    /// <summary>Bank-$A9 dispatcher word selected after the final corpse row finishes.</summary>
    private const ushort DeadTorizoNoOperationFunction = 0xd3c7;
    /// <summary>WRAM base of the 4 KiB mutable corpse pixel-row workspace.</summary>
    private const int DeadTorizoWorkBufferAddress = 0x7e2000;
    /// <summary>Byte size cleared when the corpse initializes its mutable graphics workspace.</summary>
    private const int DeadTorizoWorkBufferSize = 0x1000;
    /// <summary>WRAM base of the live sand-heap tile rows updated by the rotting effect.</summary>
    private const int DeadTorizoSandBufferAddress = 0x7e9500;

    /// <summary>Frame-local VRAM writes accumulated from the corpse and sand transfer schedule.</summary>
    private readonly List<VramWriteEntry> _deadTorizoFrameVramTransfers = [];
    /// <summary>Extended state for the single initialized Dead Torizo slot, absent outside its room lifecycle.</summary>
    private DeadTorizoEnemyState? _deadTorizo;

    /// <summary>Last library-two sound emitted by a completed corpse row.</summary>
    public ushort? LastDeadTorizoSoundEffect { get; private set; }

    /// <summary>Discards corpse state and queued graphics writes when the room-owned enemy system is reset.</summary>
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

    /// <summary>Advances sand cadence, enemy movement, and one scheduled corpse-row operation until the rot table completes.</summary>
    /// <param name="slot">Native enemy slot zero whose velocity and function word track the corpse lifecycle.</param>
    /// <param name="state">Rot table progress, sand counters, and diagnostic state for this corpse.</param>
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

    /// <summary>Tests Samus against the corpse's asymmetric touch shapes and publishes the native minimum horizontal push on overlap.</summary>
    /// <param name="slot">The corpse slot providing the hitbox origin.</param>
    /// <param name="samus">Samus kinematics used for overlap and displacement output.</param>
    /// <returns><see langword="true"/> after a hitbox overlaps and collision displacement is written; otherwise <see langword="false"/>.</returns>
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

    /// <summary>Seeds the mutable corpse workspace from installed planar artwork using the native row-copy layout.</summary>
    /// <exception cref="InvalidDataException">Installed Dead Torizo artwork is missing or has an invalid size.</exception>
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

    /// <summary>Copies eligible corpse bitplane words two pixels downward, optionally clearing each source row.</summary>
    /// <param name="state">Rotation and wrap offsets controlling the row's source and destination addresses.</param>
    /// <param name="yOffset">Pixel-row offset within the corpse graphics surface.</param>
    /// <param name="move"><see langword="true"/> clears the source words after transfer; otherwise the source remains intact.</param>
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

    /// <summary>Runs the completion side effects for one rot-table entry: dust, counters, and periodic sound.</summary>
    /// <param name="state">Diagnostic counters and completion metadata to update.</param>
    /// <param name="entryIndex">Zero-based table entry that has just reached its terminal row position.</param>
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

    /// <summary>Copies the selected first bitplane word from each tile row into the live sand heap in WRAM.</summary>
    /// <param name="lineIndex">Descending nonzero heap-row selector used to resolve native source and destination offsets.</param>
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

    /// <summary>Appends the transfer descriptors scheduled for the next frame's current corpse-animation phase.</summary>
    /// <param name="state">The wrapping phase counter advanced before selecting descriptors.</param>
    private void BuildDeadTorizoVramTransfers(DeadTorizoEnemyState state)
    {
        state.VramTransferPhase = unchecked((ushort)(state.VramTransferPhase + 1));
        // Immutable descriptors point at live WRAM corpse/sand staging surfaces.
        foreach (DeadTorizoVramTransferDefinition record in
                 DeadTorizoVramTransferDefinitions.ForPhase(state.VramTransferPhase))
            _deadTorizoFrameVramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.EncodedVramDestination));
    }

    /// <summary>Submits all accumulated corpse and sand writes to the frame queue when one is available.</summary>
    /// <param name="queue">Destination queue; <see langword="null"/> leaves the pending list untouched.</param>
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

    /// <summary>Returns the initialized corpse extension state after verifying that the dispatcher received its owning slot.</summary>
    /// <param name="slot">Slot passed by the current enemy dispatch.</param>
    /// <returns>The extended state bound to that slot.</returns>
    /// <exception cref="InvalidDataException">The corpse is uninitialized or another enemy slot entered its dispatcher.</exception>
    private DeadTorizoEnemyState RequireDeadTorizoState(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = _deadTorizo ??
            throw new InvalidDataException("Dead Torizo has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Dead-Torizo slot entered its dispatcher.");
        return state;
    }

    /// <summary>Reads one little-endian word from the corpse's WRAM workspace.</summary>
    /// <param name="wordOffset">Zero-based word offset from the workspace base.</param>
    /// <returns>The 16-bit workspace value.</returns>
    private ushort ReadWorkWord(int wordOffset) =>
        SnesWorkRam.ReadWord(EnemyWorkMemory, DeadTorizoWorkBufferAddress + wordOffset * 2);

    /// <summary>Writes one word to the corpse's WRAM workspace.</summary>
    /// <param name="wordOffset">Zero-based word offset from the workspace base.</param>
    /// <param name="value">The 16-bit value to store.</param>
    private void WriteWorkWord(int wordOffset, ushort value) =>
        WriteWord(_bus!, DeadTorizoWorkBufferAddress + wordOffset * 2, value);

    /// <summary>Computes the 16-bit two's-complement magnitude used by the native collision routine.</summary>
    /// <param name="value">Signed value represented in an unsigned word.</param>
    /// <returns>The wrapped absolute magnitude as a 16-bit word.</returns>
    private static ushort NativeAbsolute(ushort value) =>
        (value & 0x8000) == 0 ? value : unchecked((ushort)(~value + 1));

    /// <summary>Stores a word in little-endian order through the bus's byte-write interface.</summary>
    /// <param name="bus">Address space receiving both bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <param name="value">Word to split into low and high bytes.</param>
    private static void WriteWord(ISnesAddressSpace bus, int address, ushort value)
    {
        bus.WriteByte(address, unchecked((byte)value));
        bus.WriteByte(address + 1, unchecked((byte)(value >> 8)));
    }

}

/// <summary>Typed projection of Dead Torizo's bank-$A9 extended WRAM fields.</summary>
public sealed class DeadTorizoEnemyState
{
    /// <summary>Creates the typed projection of Dead Torizo's slot and native rot-table configuration.</summary>
    /// <param name="slot">The physical enemy slot that owns this extended state.</param>
    /// <param name="tablePointer">WRAM-relative address of the mutable corpse rot table.</param>
    /// <param name="vramTablePointer">Native generic transfer-table pointer retained for state inspection.</param>
    /// <param name="copyFunction">Native row-copy callback word from the corpse definition.</param>
    /// <param name="moveFunction">Native destructive row-move callback word from the corpse definition.</param>
    /// <param name="rotationTablePointer">Bank-$A9 row-offset table used to address corpse tiles.</param>
    /// <param name="finishFunction">Native completion callback word for each finished rot-table entry.</param>
    /// <param name="entryCount">Number of staggered pixel-row entries in the corpse animation.</param>
    /// <param name="yLimit">Native terminal Y boundary for a row entry.</param>
    /// <param name="lateMoveEntryIndex">Entry index at which the final destructive-move rules begin.</param>
    /// <param name="wrapOffset">Byte adjustment applied when downward movement crosses a tile-row boundary.</param>
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

    /// <summary>Physical enemy slot zero, owning the corpse's position, collision/instruction words, bank-$A9 dispatcher in VariableA, and signed 8.8 velocities in Variables B and C.</summary>
    public RoomEnemySlot Slot { get; }
    /// <summary>Bank-$7E offset $9000 of the mutable four-byte (signed pixel Y, delay) entries initialized for the 96-pixel corpse.</summary>
    public ushort TablePointer { get; }
    /// <summary>Native generic transfer-pointer field, zero in configuration $A9:DD58; Torizo instead uses the dedicated phase-selected transfer queues at $A9:D4CF.</summary>
    public ushort VramTablePointer { get; }
    /// <summary>Native non-destructive row-copy callback $A9:E38B, copying the eligible corpse pixel-row bitplanes downward while retaining their source.</summary>
    public ushort CopyFunction { get; }
    /// <summary>Native destructive row-move callback $A9:E272, copying the eligible corpse pixel-row bitplanes downward and clearing their source.</summary>
    public ushort MoveFunction { get; }
    /// <summary>Bank-$A9 tile-row byte-offset table $E226; the legacy Rotation name describes addressing of 8-pixel tile rows, not an angular transform.</summary>
    public ushort RotationTablePointer { get; }
    /// <summary>Native completed-entry hook $A9:D5BD, requesting dust at whole room Y 188 and X 272..303 from the current RNG seed, plus a periodic library-two $10 sound.</summary>
    public ushort FinishFunction { get; }
    /// <summary>Ninety-six staggered rotting entries, matching the native corpse height $0060 in pixel rows.</summary>
    public ushort EntryCount { get; }
    /// <summary>Native height-minus-one value, 95: a moved row completes when its next pixel Y reaches this boundary, which also identifies the final entry.</summary>
    public ushort YLimit { get; }
    /// <summary>Native height-minus-two value, 94: this entry and the final entry use destructive moves even during their last three delay ticks.</summary>
    public ushort LateMoveEntryIndex { get; }
    /// <summary>Native inter-tile byte adjustment $0134, applied when source pixel Y modulo eight is 6 or 7 so the two-pixel downward destination wraps into the next tile row.</summary>
    public ushort WrapOffset { get; }
    /// <summary>Wrapping phase word incremented on every main-AI transfer-building call, even while intact or finished; its low bit alternates six corpse tile rows and one sand strip per queue.</summary>
    public ushort VramTransferPhase { get; internal set; }
    /// <summary>Native $7E:7808 contact-delay counter, incremented in $A9:D3C8 until sixteen calls have elapsed before falling through into rotting; direct shot/touch triggering bypasses this delay.</summary>
    public ushort PreRotDelayCounter { get; internal set; }
    /// <summary>Native $7E:7804 sand-heap pixel-row selector, initialized to 15 and copied in descending order through 1; zero means no more heap lines remain.</summary>
    public ushort SandLineCounter { get; internal set; }
    /// <summary>Native $7E:7806 divider incremented only by rotting calls; reaching fifteen resets it to zero and copies the next nonzero sand line.</summary>
    public ushort SandFrameCounter { get; internal set; }
    /// <summary>Host diagnostic count of shared corpse-rotting scheduler calls since initialization, including the call that completes its final entry.</summary>
    public uint ProcessCallCount { get; internal set; }
    /// <summary>Host diagnostic count of completed row entries, incremented before their dust callback, including the final entry.</summary>
    public uint FinishedEntryCount { get; internal set; }
    /// <summary>Host diagnostic count of dust effects requested by completed entries, one request per completed-entry hook invocation.</summary>
    public uint DustSpawnCount { get; internal set; }
    /// <summary>Host diagnostic count of sand lines copied into the live $7E:9500 heap surface, incremented once for each descending nonzero line selector.</summary>
    public uint SandLineCopyCount { get; internal set; }
    /// <summary>Most recently completed zero-based row-entry index, or $FFFF before any completion; retained as host diagnostic state.</summary>
    public ushort LastFinishedEntryIndex { get; internal set; } = ushort.MaxValue;
}
