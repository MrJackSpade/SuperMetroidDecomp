using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored wall-cannon PLMs used by Draygon's room.</summary>
public sealed partial class RoomPlmSystem
{
    private Action<ushort>? _disableDraygonCannon;

    /// <summary>Debugger-visible projections of every live Draygon cannon PLM.</summary>
    public IReadOnlyList<DraygonCannonPlmSnapshot> DraygonCannons => _slots
        .Where(slot => slot.Active && slot.DraygonCannon is not null)
        .Select(slot => new DraygonCannonPlmSnapshot(
            slot.HeaderPointer,
            slot.BlockIndex,
            slot.RoomArgument,
            slot.DraygonCannon!.VariablePointer,
            slot.DraygonCannon.Orientation,
            slot.DraygonCannon.Destroyed,
            slot.InstructionPointer,
            slot.PreInstruction))
        .ToArray();

    private static bool IsDraygonCannonHeader(ushort header) => header is
        RoomPlmHeaders.DraygonCannonFacingRight or
        RoomPlmHeaders.DraygonCannonFacingRightDestroyed or
        RoomPlmHeaders.DraygonCannonFacingLeft;

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

    private bool TryNotifyDraygonCannonHit(int blockIndex, SamusProjectileTypeWord projectileType)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.DraygonCannon is null)
                continue;

            slot.LoopTimer = projectileType.Raw;
            slot.DraygonCannon.HasPendingHit = true;
            return true;
        }
        return false;
    }

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
                slot.LinkInstruction = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.IncrementArgumentAndGotoIfGreaterOrEqual:
            {
                byte threshold = bus.ReadByte(Bank84(unchecked((ushort)(cursor + 2))));
                ushort destination = ReadBank84Word(bus, unchecked((ushort)(cursor + 3)));
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

    private void ResetDraygonCannonState() => _disableDraygonCannon = null;

    private sealed class DraygonCannonPlmState(
        ushort variablePointer,
        DraygonCannonOrientation orientation)
    {
        public ushort VariablePointer { get; } = variablePointer;
        public DraygonCannonOrientation Orientation { get; } = orientation;
        public bool Destroyed { get; set; }
        public bool HasPendingHit { get; set; }
    }
}

/// <summary>Stable debugger view over one retail Draygon cannon PLM.</summary>
public readonly record struct DraygonCannonPlmSnapshot(
    ushort Header,
    int BlockIndex,
    ushort RoomArgument,
    ushort VariablePointer,
    DraygonCannonOrientation Orientation,
    bool Destroyed,
    ushort InstructionPointer,
    ushort PreInstruction);
