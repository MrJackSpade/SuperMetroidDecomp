using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using System.Buffers.Binary;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Torizo species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DeadTorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.CorpseTorizo];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DeadTorizoEntity(this, slot, common);

    /// <summary>Supplies corpse scratch WRAM, installed tiles, and the shared random seed.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy-frame counter used to cadence rotting sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns dust when individual corpse rows finish rotting.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Draws the fixed corpse hook using installed spritemap parts.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds corpse rotting to room memory, frame timing, projectiles, and presentation.</summary>
    internal DeadTorizoBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyPresentation presentation)
    {
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _presentation = presentation;
    }

    /// <summary>Native corpse and sand DMA records produced during the current enemy update.</summary>
    internal readonly List<VramWriteEntry> _deadTorizoFrameVramTransfers = [];
    /// <summary>Single corpse state owned by enemy slot zero in the loaded room.</summary>
    internal DeadTorizoEnemyState? _deadTorizo;
    /// <summary>Slot zero's typed corpse pose (#641), started before the corpse state exists.</summary>
    internal AnimationProgramCursor<DeadTorizoProgram> _deadTorizoProgram;

    /// <summary>Last library-two sound emitted by a completed corpse row.</summary>
    public ushort? LastDeadTorizoSoundEffect { get; private set; }

    /// <summary>Clears corpse state, pending VRAM writes, and the last sound event on room load.</summary>
    internal void ResetDeadTorizoRoomState()
    {
        _deadTorizo = null;
        _deadTorizoProgram = default;
        _deadTorizoFrameVramTransfers.Clear();
        LastDeadTorizoSoundEffect = null;
    }

    /// <summary>Ports <c>DeadTorizo_Init</c> at <c>$A9:D308</c>.</summary>
    internal void InitializeDeadTorizo(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Dead Torizo requires native slot zero, not slot {slot.SlotIndex}.");
        }

        _room.Scratch.CorpseTiles.Fill(0);

        slot.VariableA = (ushort)DeadTorizoFunction.Wait;
        slot.Properties = slot.Properties.With(
            EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions);
        _deadTorizoProgram = AnimationProgramCursor<DeadTorizoProgram>.Start(DeadTorizoProgram.Stationary);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.PaletteIndex = EnemyPaletteBits.Palette1;
        slot.VariableB = 0;
        slot.VariableC = 8;

        DeadTorizoCorpseDefinition definition = DeadTorizoCorpseDefinitions.Corpse;
        ushort yLimit = unchecked((ushort)(definition.EntryCount - 1));
        ushort lateMoveEntryIndex = unchecked((ushort)(yLimit - 1));
        _deadTorizo = new DeadTorizoEnemyState(slot, new CorpseRottingProgram { TablePointer = definition.RottingTablePointer, VramTablePointer = definition.VramTransferPointer, CopyFunction = definition.CopyFunction, MoveFunction = definition.MoveFunction, RotationTablePointer = definition.RotationTablePointer, FinishFunction = definition.FinishFunction, EntryCount = definition.EntryCount, YLimit = yLimit, LateMoveEntryIndex = lateMoveEntryIndex, WrapOffset = definition.WrapOffset })
        {
            SandLineCounter = 15,
        };

        CorpseRottingTableProcessor.Initialize(
            _room.Scratch.RotTable(definition.RottingTablePointer, definition.EntryCount));
        InitializeDeadTorizoGraphics();
    }

    /// <summary>Ports <c>DeadTorizo_Main</c> at <c>$A9:D368</c>.</summary>
    internal void RunDeadTorizoMain(RoomEnemySlot slot, SamusState? samus)
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

        switch (ClosedNativeWords.Decode<DeadTorizoFunction>(slot.VariableA, "Dead Torizo corpse function"))
        {
            case DeadTorizoFunction.Wait:
                if (samus?.Kinematics.DidCollideWithSolidEnemy(slot.NativeIndex) == true)
                    slot.VariableA = (ushort)DeadTorizoFunction.PreRot;
                break;

            case DeadTorizoFunction.PreRot:
                state.PreRotDelayCounter = unchecked((ushort)(state.PreRotDelayCounter + 1));
                if (state.PreRotDelayCounter >= 16)
                {
                    slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
                    slot.VariableA = (ushort)DeadTorizoFunction.Rotting;
                    RunDeadTorizoRotting(slot, state);
                }
                break;

            case DeadTorizoFunction.Rotting:
                RunDeadTorizoRotting(slot, state);
                break;

            case DeadTorizoFunction.NoOperation:
                break;

            default:
                throw new InvalidDataException(
                    $"Dead Torizo function $A9:{slot.VariableA:X4} is not translated.");
        }

        BuildDeadTorizoVramTransfers(state);
    }

    /// <summary>Advances sand rows, corpse motion, and one step of the native rotting table.</summary>
    private void RunDeadTorizoRotting(RoomEnemySlot slot, DeadTorizoEnemyState state)
    {
        state.SandFrameCounter = unchecked((ushort)(state.SandFrameCounter + 1));
        if (state.SandFrameCounter >= 15)
        {
            state.SandFrameCounter = 0;
            if (state.SandLineCounter != 0)
            {
                CopyDeadTorizoSandLine(state.SandLineCounter);
                state.SandLineCounter = NativeWordCounter.Decrement(state.SandLineCounter).Value;
                state.SandLineCopyCount++;
            }
        }

        slot.VariableC = unchecked((ushort)(slot.VariableC + 1));
        MoveBankA9EnemyWithVelocity(slot);
        state.ProcessCallCount++;
        bool stillRotting = CorpseRottingTableProcessor.Step(
            _room.Scratch.RotTable(state.TablePointer, state.EntryCount),
            state.YLimit,
            state.LateMoveEntryIndex,
            (yOffset, move) => CopyOrMoveDeadTorizoPixelRow(state, yOffset, move),
            _ => FinishDeadTorizoCorpseRow(state));
        if (!stillRotting)
            slot.VariableA = (ushort)DeadTorizoFunction.NoOperation;
    }

    /// <summary>Shot/touch tail at <c>$A9:D433</c>.</summary>
    internal void TriggerDeadTorizoRotting(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.VariableA = (ushort)DeadTorizoFunction.Rotting;
    }

    /// <summary>Power-bomb entry at <c>$A9:D42A</c>.</summary>
    internal void TriggerDeadTorizoPowerBomb(RoomEnemySlot slot)
    {
        RequireDeadTorizoState(slot);
        if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
            TriggerDeadTorizoRotting(slot);
    }

    /// <summary>Checks asymmetric corpse touch boxes and publishes Samus's minimum separation displacement.</summary>
    private static bool DeadTorizoCustomHitboxOverlaps(RoomEnemySlot slot, SamusState samus)
    {
        SamusKinematicsState kinematics = samus.Kinematics;
        foreach (DeadCorpseTouchHitbox hitbox in DeadMonsterRottingDefinitions.TorizoTouchHitboxes)
        {
            ushort verticalDistance;
            ushort verticalRadius;
            if (unchecked((short)(kinematics.Position.Y.Whole - slot.Position.Y.Whole)) >= 0)
            {
                verticalDistance = unchecked((ushort)(kinematics.Position.Y.Whole - slot.Position.Y.Whole));
                verticalRadius = hitbox.Bottom;
            }
            else
            {
                verticalDistance = unchecked((ushort)(slot.Position.Y.Whole - kinematics.Position.Y.Whole));
                verticalRadius = hitbox.Top;
            }

            ushort verticalOverlap = unchecked((ushort)(
                kinematics.Radius.Y + NativeAbsolute(verticalRadius) - verticalDistance));
            if (unchecked((short)verticalOverlap) < 0)
                continue;

            ushort horizontalDistance;
            ushort horizontalRadius;
            if (unchecked((short)(kinematics.Position.X.Whole - slot.Position.X.Whole)) >= 0)
            {
                horizontalDistance = unchecked((ushort)(kinematics.Position.X.Whole - slot.Position.X.Whole));
                horizontalRadius = hitbox.Right;
            }
            else
            {
                horizontalDistance = unchecked((ushort)(slot.Position.X.Whole - kinematics.Position.X.Whole));
                horizontalRadius = hitbox.Left;
            }

            short overlap = unchecked((short)(
                kinematics.Radius.X + NativeAbsolute(horizontalRadius) - horizontalDistance));
            if (overlap < 0)
                continue;
            if (overlap < 4)
                overlap = 4;

            kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(unchecked((short)overlap), 4));
            return true;
        }

        return false;
    }

    /// <summary>Copies discontinuous regions of the installed corpse sheet into scratch WRAM.</summary>
    internal void InitializeDeadTorizoGraphics()
    {
        ReadOnlySpan<byte> installedTiles = DeadTorizoInstalledTiles();
        if (installedTiles.IsEmpty)
            throw new InvalidDataException("Dead Torizo requires installed corpse artwork.");
        WorkRamByteTable tiles = _room.Scratch.CorpseTiles;
        for (int row = 0; row < DeadTorizoGeometryDefinitions.Rows; row++)
        {
            DeadTorizoGraphicsCopy copy = DeadTorizoGeometryDefinitions.InitialCopy(row);
            for (int byteIndex = 0; byteIndex < copy.Length; byteIndex++)
            {
                int sourceOffset = copy.SourceOffset + byteIndex;
                tiles[copy.DestinationOffset + byteIndex] = installedTiles[sourceOffset];
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
        if (_room.Artwork is null)
            return [];
        if (!_room.Artwork.TryResolve(DeadTorizoArtworkDefinitions.SourceAddress,
                DeadTorizoArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles))
            throw new InvalidDataException(
                "Installed dead-Torizo sheet enemy-ed3f-tiles.png is missing or has the wrong size.");
        return tiles.Span;
    }

    /// <summary>Shifts selected corpse tile words through rotation-table offsets, clearing sources on move.</summary>
    internal void CopyOrMoveDeadTorizoPixelRow(
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

    /// <summary>Records a finished rot row, spawns dust from shared RNG, and optionally publishes sound.</summary>
    private void FinishDeadTorizoCorpseRow(DeadTorizoEnemyState state)
    {
        state.FinishedEntryCount++;
        ushort random = _room.Random.Read();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)((random & CorpseDustDefinitions.TorizoSpreadBits) + 272)), 188),
            MiscDustAnimation.CorpseDustCloud);
        state.DustSpawnCount++;
        if ((_frameContext._randomEnemyCounter & 7) == 0)
            LastDeadTorizoSoundEffect = EnemySoundEffectDefinitions.DeadTorizoRotSound;
    }

    /// <summary>Copies the first bitplane word of eighteen spaced source rows into sand scratch WRAM.</summary>
    internal void CopyDeadTorizoSandLine(ushort lineIndex)
    {
        ReadOnlySpan<byte> installedTiles = DeadTorizoInstalledTiles();
        ushort destinationOffset = DeadMonsterRottingDefinitions.SandDestination(lineIndex);
        ushort sourceOffset = DeadMonsterRottingDefinitions.SandSource(lineIndex);

        // Eighteen tile rows are sixteen bytes apart in both the source sheet and the WRAM
        // heap surface. Only the first 16-bit bitplane word of each row is replaced.
        WorkRamByteTable sand = _room.Scratch.DeadTorizoSand;
        for (int row = 0; row < 18; row++)
        {
            int tileOffset = sourceOffset + row * 16;
            ushort value = BinaryPrimitives.ReadUInt16LittleEndian(installedTiles.Slice(tileOffset, 2));
            sand.WriteWord(destinationOffset + row * 16, value);
        }
    }

    /// <summary>
    /// Applies bank-$A9's shared signed 8.8 enemy velocity pair. Dead Torizo and Shitroid
    /// both call the same cartridge helper; retaining one implementation also makes its
    /// unusual use of the high byte of the subposition explicit.
    /// </summary>
    internal static void MoveBankA9EnemyWithVelocity(RoomEnemySlot slot)
    {
        SnesFixedPosition x = new SnesFixedPosition(slot.Position.X.Whole, slot.Position.X.Fraction)
            .AddEightEightToHighFraction(slot.VariableB);
        slot.Position = slot.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        SnesFixedPosition y = new SnesFixedPosition(slot.Position.Y.Whole, slot.Position.Y.Fraction)
            .AddEightEightToHighFraction(slot.VariableC);
        slot.Position = slot.Position.WithFractionY(y.Fraction).WithWholeY(y.Whole);
    }

    /// <summary>Advances the corpse transfer phase and appends its authored WRAM-to-VRAM descriptors.</summary>
    internal void BuildDeadTorizoVramTransfers(DeadTorizoEnemyState state)
    {
        state.VramTransferPhase = unchecked((ushort)(state.VramTransferPhase + 1));
        // Immutable descriptors point at live WRAM corpse/sand staging surfaces.
        foreach (DeadTorizoVramTransferDefinition record in
                 DeadTorizoVramTransferDefinitions.ForPhase(state.VramTransferPhase))
            _deadTorizoFrameVramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.EncodedVramDestination));
    }

    /// <summary>Queues this frame's corpse DMA descriptors at their native VRAM destinations.</summary>
    [PpuLayoutBoundary("codec: staged Dead Torizo corpse DMA records' native destinations")]
    internal void QueueDeadTorizoFrameVramTransfers(VramWriteQueue? queue)
    {
        if (queue is null)
            return;
        foreach (VramWriteEntry transfer in _deadTorizoFrameVramTransfers)
        {
            queue.Enqueue(
                transfer.SizeInBytes,
                transfer.SourceAddress,
                VramDmaDestination.FromNative(transfer.EncodedVramDestination));
        }
    }

    /// <summary>Runs the post-enemy ordinary-spritemap hook at <c>$A9:D39A</c>.</summary>
    internal void DrawDeadTorizoHook(OamBuffer oam, Position16<RoomPixels> camera)
    {
        if (_deadTorizo is null)
            return;

        ushort screenY = unchecked((ushort)(187 - camera.Y));
        if (unchecked((short)screenY) < 0)
            return;
        _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Bank = DeadTorizoArtworkDefinitions.SpritemapBank, Pointer = DeadTorizoArtworkDefinitions.HookSpritemap, Origin = new(unchecked((ushort)(296 - camera.X)), screenY), PaletteBits = 0, BaseTileIndex = 0 });
    }

    /// <summary>Requires the initialized slot-zero corpse state and rejects unrelated enemy slots.</summary>
    private DeadTorizoEnemyState RequireDeadTorizoState(RoomEnemySlot slot)
    {
        DeadTorizoEnemyState state = _deadTorizo ??
            throw new InvalidDataException("Dead Torizo has no initialized extended state.");
        if (!ReferenceEquals(state.Slot, slot))
            throw new InvalidDataException("A non-Dead-Torizo slot entered its dispatcher.");
        return state;
    }

    /// <summary>Reads a little-endian corpse scratch word at a table-relative word offset.</summary>
    private ushort ReadWorkWord(int wordOffset) =>
        _room.Scratch.CorpseTiles.ReadWord(wordOffset * 2);

    /// <summary>Writes one corpse scratch word at a table-relative word offset.</summary>
    private void WriteWorkWord(int wordOffset, ushort value) =>
        _room.Scratch.CorpseTiles.WriteWord(wordOffset * 2, value);

    /// <summary>Applies the cartridge's unsigned-word two's-complement absolute operation.</summary>
    private static ushort NativeAbsolute(ushort value) =>
        !EnemyMath.IsNegative16(value) ? value : unchecked((ushort)(~value + 1));

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetDeadTorizoRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastDeadTorizoSoundEffect = null;
        _deadTorizoFrameVramTransfers.Clear();
    }

    /// <summary>Routes corpse bomb contact to rotting after the common no-op gate.</summary>
    internal void RegisterDeadTorizoBombReactions(EnemySharedReactions table)
    {
        // Specials consulted after the selected literal no-op gate.
        // Corpse shot callbacks run directly after the bank-$A0 collision mark. They do not consult bomb
        // vulnerability or health; they only hand the actor to its authored rotting state machine.
        table.AddBombSpecialAfterNoOp(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
                                           c.SelectedShotAi == EnemyInteractionCallback.DeadTorizoTouchAndShot,
            c => TriggerDeadTorizoRotting(c.Enemy));
    }

    /// <summary>Advances DeadTorizo's typed animation.</summary>
    internal void AdvanceDeadTorizoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        EnemyProgramAnimation.Run(slot, DeadTorizoAnimations.Programs, ref _deadTorizoProgram, DeadTorizoFrameDefinitions.Spritemap);
    }

    /// <summary>Ports DeadTorizo's initialization AI.</summary>
    internal void RunDeadTorizoInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeDeadTorizo(slot);
    }

    /// <summary>Ports DeadTorizo's main AI.</summary>
    internal void RunDeadTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte _)
    {
        RunDeadTorizoMain(slot, frame.Samus);
    }

    /// <summary>Starts corpse rotting on a power bomb without common damage handling.</summary>
    internal void RegisterDeadTorizoPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DeadTorizoPowerBomb)
        {
            ReplacesCommon = (enemy, _) => TriggerDeadTorizoPowerBomb(enemy),
        });
    }

    /// <summary>Marks projectile collision and starts rotting without impact animation or health loss.</summary>
    internal void RegisterDeadTorizoShotReactions(EnemySharedReactions table)
    {
        // The dead corpses and Shitroid receive the bank-$A0 projectile prelude but never an impact or vulnerability.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DeadTorizoTouchAndShot)
        {
            // Its private callback runs after the ordinary collision prelude but before common impact/vulnerability
            // code. The shot is marked, not converted into an explosion, and the corpse never loses health.
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                TriggerDeadTorizoRotting(c.Enemy);
                return true;
            },
        });
    }

    /// <summary>Starts the same rotting tail on direct Samus contact.</summary>
    internal void RegisterDeadTorizoTouch(EnemySharedReactions table)
    {
        // `$A9:D433` is shared by touch and shot. Contact does not enter common damage; it makes the corpse solid
        // and starts the exact rotting table immediately.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DeadTorizoTouchAndShot,
            context => TriggerDeadTorizoRotting(context.Slot));
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastDeadTorizoSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
