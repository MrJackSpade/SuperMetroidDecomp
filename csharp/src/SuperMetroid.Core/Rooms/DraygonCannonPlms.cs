using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Draygon's wall-turret PLMs: header identification and the cannon state Draygon's grab fight drives.</summary>
internal sealed class DraygonCannonPlms
{
    /// <summary>Enemy control-word writer notified when a cannon is destroyed.</summary>
    private readonly PlmRoomBindings _bindings;

    /// <summary>Connects cannon PLM instruction handling to the active room's enemy services.</summary>
    internal DraygonCannonPlms(PlmRoomBindings bindings)
    {
        _bindings = bindings;
    }


    /// <summary>Recognizes the active right, pre-destroyed right, and left retail cannon headers.</summary>
    internal static bool IsDraygonCannonHeader(PlmHeaderId header) => header is
        PlmHeaderId.DraygonCannonFacingRight or
        PlmHeaderId.DraygonCannonFacingRightDestroyed or
        PlmHeaderId.DraygonCannonFacingLeft;

    /// <summary>Captures cannon orientation and its enemy control word, installing shootable terrain unless the cannon starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, PlmSlot slot)
    {
        DraygonCannonOrientation orientation = slot.HeaderPointer switch
        {
            PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed =>
                    DraygonCannonOrientation.Right,
            PlmHeaderId.DraygonCannonFacingLeft => DraygonCannonOrientation.Left,
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not a retail Draygon cannon header."),
        };

        ushort variablePointer = slot.RoomArgument;
        if (!DraygonCannonWorkRamAddresses.TryDecode(variablePointer, out _))
        {
            throw new InvalidDataException(
                $"Draygon cannon header $84:{(int)slot.HeaderPointer:X4} has invalid control " +
                $"word ${variablePointer:X4}.");
        }

        slot.DraygonCannon = new DraygonCannonPlmState(variablePointer, orientation);
        if (slot.HeaderPointer == PlmHeaderId.DraygonCannonFacingRightDestroyed)
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
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonRomData.CannonExtensionWord);
    }

    /// <summary>Accepts pending missile hits, treating a Super Missile as enough damage to reach the destruction threshold.</summary>
    internal static void RunDraygonCannonPreInstruction(PlmSlot slot)
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
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Handles cannon link, hit-count branch, and facing-specific destruction opcodes while advancing the PLM cursor.</summary>
    internal bool TryExecuteDraygonCannonInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        PlmSlot slot,
        RoomPlmInstruction instruction)
    {
        DraygonCannonPlmState? state = slot.DraygonCannon;
        if (state is null)
            return false;

        ushort cursor = slot.InstructionPointer;
        if (instruction is not (
            RoomPlmInstruction.LinkInstruction or
            RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual or
            RoomPlmInstruction.DamageDraygonCannonFacingRight or
            RoomPlmInstruction.DamageDraygonCannonFacingLeft))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.LinkInstruction:
                slot.LinkInstruction = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            {
                byte threshold = RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(cursor + 2)));
                ushort destination = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 3)));
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

            case RoomPlmInstruction.DamageDraygonCannonFacingRight:
                if (state.Orientation != DraygonCannonOrientation.Right)
                    throw new InvalidDataException("Right-facing cannon damage opcode reached a left-facing PLM.");
                DamageDraygonCannon(level, slot, state);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.DamageDraygonCannonFacingLeft:
                if (state.Orientation != DraygonCannonOrientation.Left)
                    throw new InvalidDataException("Left-facing cannon damage opcode reached a right-facing PLM.");
                DamageDraygonCannon(level, slot, state);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
        }
    }

    /// <summary>Disables the matching bank-$A5 cannon firing word and changes both room blocks to destroyed terrain.</summary>
    private void DamageDraygonCannon(
        RoomLevelData level,
        PlmSlot slot,
        DraygonCannonPlmState state)
    {
        (_bindings.Enemies ?? throw new InvalidOperationException(
            "Draygon cannon bytecode has no enemy-control-word writer.")).DisableDraygonCannon(
                DraygonCannonWorkRamAddresses.Decode(state.VariablePointer));
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonRomData.DestroyedCannonWord);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonRomData.DestroyedCannonWord);
        state.Destroyed = true;
    }

    /// <summary>Changes a cannon block's collision type and behavior while preserving its visual tile bits.</summary>
    private static void WriteDraygonCannonTypeAndBts(
        RoomLevelData level,
        int blockIndex,
        ushort typeAndBts)
    {
        ushort original = level.GetPlmCollisionBlockByIndex(blockIndex).LevelWord;
        level.SetPlmForegroundEntry(
            blockIndex,
            new RoomLevelWord(original).WithCollisionType(new RoomLevelWord(typeAndBts).CollisionType).Raw);
        level.SetPlmBehavior(blockIndex, unchecked((byte)typeAndBts));
    }

    /// <summary>Per-slot cannon orientation, enemy control-word identity, destruction state, and queued hit flag.</summary>
    internal sealed class DraygonCannonPlmState(
        ushort variablePointer,
        DraygonCannonOrientation orientation)
    {
        /// <summary>Bank-$A5 word written when this physical cannon stops firing.</summary>
        public ushort VariablePointer { get; } = variablePointer;
        /// <summary>Selects the right or left damage opcode accepted by this PLM.</summary>
        public DraygonCannonOrientation Orientation { get; } = orientation;
        /// <summary>Marks that this cannon's terrain and firing control have been disabled.</summary>
        public bool Destroyed { get; set; }
        /// <summary>Indicates a projectile collision awaiting missile-family filtering.</summary>
        public bool HasPendingHit { get; set; }
    }
}
