using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored wall-cannon PLMs used by Draygon's room.</summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>Writer installed during PLM population to disable the enemy control word when a cannon is destroyed.</summary>
    private Action<ushort>? _disableDraygonCannon;

    /// <summary>Recognizes the three retail header words that create right-facing, destroyed, or left-facing Draygon cannons.</summary>
    /// <param name="header">The PLM header word to classify.</param>
    /// <returns><see langword="true"/> when the header belongs to a Draygon wall cannon.</returns>
    private static bool IsDraygonCannonHeader(ushort header) => header is
        RoomPlmHeaders.DraygonCannonFacingRight or
        RoomPlmHeaders.DraygonCannonFacingRightDestroyed or
        RoomPlmHeaders.DraygonCannonFacingLeft;

    /// <summary>Creates per-PLM cannon state and initializes the two collision blocks beneath an intact cannon.</summary>
    /// <param name="level">Room data whose cannon collision and extension blocks are initialized.</param>
    /// <param name="slot">The PLM slot carrying a supported cannon header and its native control word.</param>
    private static void SetupDraygonCannonSlot(RoomLevelData level, PlmSlot slot)
    {
        DraygonCannonOrientation orientation = slot.HeaderPointer switch
        {
            RoomPlmHeaders.DraygonCannonFacingRight or
                RoomPlmHeaders.DraygonCannonFacingRightDestroyed =>
                    DraygonCannonOrientation.Right,
            RoomPlmHeaders.DraygonCannonFacingLeft => DraygonCannonOrientation.Left,
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not a retail Draygon cannon header."),
        };

        ushort variablePointer = slot.RoomArgument;
        if (!DraygonCannonData.IsControlWord(variablePointer))
        {
            throw new InvalidDataException(
                $"Draygon cannon header $84:{slot.HeaderPointer:X4} has invalid control " +
                $"word ${variablePointer:X4}.");
        }

        slot.DraygonCannon = new DraygonCannonPlmState(variablePointer, orientation);
        if (slot.HeaderPointer == RoomPlmHeaders.DraygonCannonFacingRightDestroyed)
        {
            // Setup $DF4C only moves the original argument to PLM_Variable and seeds three.
            // Its list begins at the damage instruction, which owns the terrain mutation.
            slot.RoomArgument = 3;
            return;
        }

        slot.RoomArgument = 0;
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonRomData.CannonCollisionWord);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.WidthInBlocks),
            DraygonCannonRomData.CannonExtensionWord);
    }

    /// <summary>Consumes a pending missile hit and redirects the PLM to the native damage instruction sequence.</summary>
    /// <param name="slot">The cannon PLM whose pre-instruction and hit state are processed.</param>
    private static void RunDraygonCannonPreInstruction(PlmSlot slot)
    {
        DraygonCannonPlmState? state = slot.DraygonCannon;
        if (state is null || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != DraygonCannonRomData.MissileHitPreInstruction)
        {
            throw new InvalidDataException(
                $"Draygon cannon reached untranslated pre-instruction $84:{slot.PreInstruction:X4}.");
        }
        if (!state.HasPendingHit)
            return;

        SamusProjectileFamily family = new SamusProjectileTypeWord(slot.LoopTimer).Family;
        if (family == SamusProjectileFamily.SuperMissile)
            slot.RoomArgument = DraygonCannonRomData.SuperMissileHitCounterSeed;
        else if (family != SamusProjectileFamily.Missile)
            return;

        slot.LoopTimer = 0;
        state.HasPendingHit = false;
        slot.InstructionPointer = slot.LinkInstruction;
        slot.InstructionTimer = 1;
    }

    /// <summary>Executes the translated link, counter, and orientation-specific damage opcodes used by cannon PLMs.</summary>
    /// <param name="bus">Address space used to read instruction operands from the room PLM program.</param>
    /// <param name="level">Room collision data that damage instructions may update.</param>
    /// <param name="slot">The PLM slot whose instruction cursor and runtime state are advanced.</param>
    /// <param name="instruction">The opcode currently dispatched by the room PLM interpreter.</param>
    /// <returns><see langword="true"/> when the opcode was handled for an initialized cannon; otherwise <see langword="false"/>.</returns>
    private bool TryExecuteDraygonCannonInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        PlmSlot slot,
        ushort instruction)
    {
        DraygonCannonPlmState? state = slot.DraygonCannon;
        if (state is null)
            return false;

        ushort cursor = slot.InstructionPointer;
        switch (instruction)
        {
            case RoomPlmInstructionCodes.LinkInstruction:
                slot.LinkInstruction = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.IncrementArgumentAndGotoIfGreaterOrEqual:
            {
                byte threshold = ReadProgramByte(bus, unchecked((ushort)(cursor + 2)));
                ushort destination = ReadProgramWord(bus, unchecked((ushort)(cursor + 3)));
                byte next = unchecked((byte)(slot.RoomArgument + 1));
                if (next >= threshold)
                {
                    slot.RoomArgument = ushort.MaxValue;
                    slot.PreInstruction = 0;
                    slot.InstructionPointer = destination;
                }
                else
                {
                    slot.RoomArgument = next;
                    slot.InstructionPointer = unchecked((ushort)(cursor + 5));
                }
                return true;
            }

            case RoomPlmInstructionCodes.DamageDraygonCannonFacingRight:
                if (state.Orientation != DraygonCannonOrientation.Right)
                    throw new InvalidDataException("Right-facing cannon damage opcode reached a left-facing PLM.");
                DamageDraygonCannon(level, slot, state);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.DamageDraygonCannonFacingLeft:
                if (state.Orientation != DraygonCannonOrientation.Left)
                    throw new InvalidDataException("Left-facing cannon damage opcode reached a right-facing PLM.");
                DamageDraygonCannon(level, slot, state);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    /// <summary>Disables the cannon's controlling enemy, replaces its two collision blocks, and marks its PLM destroyed.</summary>
    /// <param name="level">Room collision data receiving the destroyed-cannon block words.</param>
    /// <param name="slot">The cannon PLM whose foreground block pair is changed.</param>
    /// <param name="state">Runtime data containing the native enemy control-word address and orientation.</param>
    private void DamageDraygonCannon(
        RoomLevelData level,
        PlmSlot slot,
        DraygonCannonPlmState state)
    {
        (_disableDraygonCannon ?? throw new InvalidOperationException(
            "Draygon cannon bytecode has no enemy-control-word writer."))(state.VariablePointer);
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonRomData.DestroyedCannonWord);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.WidthInBlocks),
            DraygonCannonRomData.DestroyedCannonWord);
        state.Destroyed = true;
    }

    /// <summary>Changes a foreground entry's type and BTS byte while preserving its low twelve level-word bits.</summary>
    /// <param name="level">Room data containing the foreground entry to update.</param>
    /// <param name="blockIndex">Linear index of the foreground block.</param>
    /// <param name="typeAndBts">Word whose high nibble supplies the block type and low byte supplies BTS.</param>
    private static void WriteDraygonCannonTypeAndBts(
        RoomLevelData level,
        int blockIndex,
        ushort typeAndBts)
    {
        ushort original = level.GetPlmCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetPlmForegroundEntry(
            blockIndex,
            unchecked((ushort)((original & 0x0fff) | (typeAndBts & 0xf000))));
        level.SetPlmBehavior(blockIndex, unchecked((byte)typeAndBts));
    }

    /// <summary>Clears the room-lifetime callback that writes the Draygon cannon's enemy control word.</summary>
    private void ResetDraygonCannonState() => _disableDraygonCannon = null;

    /// <summary>Tracks the native arguments and hit/destruction progress for one populated Draygon cannon PLM.</summary>
    /// <param name="variablePointer">Enemy control-word address retained from the PLM room argument.</param>
    /// <param name="orientation">Facing direction encoded by the cannon's native PLM header.</param>
    private sealed class DraygonCannonPlmState(
        ushort variablePointer,
        DraygonCannonOrientation orientation)
    {
        /// <summary>Enemy control-word address used to disable the cannon when its damage instruction executes.</summary>
        public ushort VariablePointer { get; } = variablePointer;
        /// <summary>Direction used to validate and select the cannon's orientation-specific damage opcode.</summary>
        public DraygonCannonOrientation Orientation { get; } = orientation;
        /// <summary>Whether the cannon's enemy and room collision blocks have already been disabled.</summary>
        public bool Destroyed { get; set; }
        /// <summary>Whether projectile collision queued a hit for the next PLM pre-instruction update.</summary>
        public bool HasPendingHit { get; set; }
    }
}
