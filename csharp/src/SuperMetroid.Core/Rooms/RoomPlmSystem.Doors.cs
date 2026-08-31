using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored door-cap reactions owned by bank $84's PLM pool.</summary>
public sealed partial class RoomPlmSystem
{
    private const ushort YellowDoorFacingLeftHeader = 0xc85a;
    private const ushort YellowDoorFacingRightHeader = 0xc860;
    private const ushort YellowDoorFacingUpHeader = 0xc866;
    private const ushort YellowDoorFacingDownHeader = 0xc86c;
    private const ushort GreenDoorFacingLeftHeader = 0xc872;
    private const ushort GreenDoorFacingRightHeader = 0xc878;
    private const ushort GreenDoorFacingUpHeader = 0xc87e;
    private const ushort GreenDoorFacingDownHeader = 0xc884;
    private const ushort RedDoorFacingLeftHeader = 0xc88a;
    private const ushort RedDoorFacingRightHeader = 0xc890;
    private const ushort RedDoorFacingUpHeader = 0xc896;
    private const ushort RedDoorFacingDownHeader = 0xc89c;

    private const byte BlueDoorFacingLeftBts = 0x40;
    private const byte BlueDoorFacingRightBts = 0x41;
    private const byte BlueDoorFacingUpBts = 0x42;
    private const byte BlueDoorFacingDownBts = 0x43;

    /// <summary>
    /// Runs setup <c>$84:C7B1</c> for every cartridge-authored yellow, green, and red
    /// door in a room population.
    /// </summary>
    /// <remarks>
    /// Colored-door art is drawn by a resident PLM, but collision gating begins
    /// synchronously during room load: the cap origin becomes shootable-solid type $C
    /// with BTS $44. Leaving the decompressed blue BTS $40..$43 in place would let a
    /// power-beam collision allocate a blue-door opener before the colored-door actor had
    /// any chance to check missile family. This method deliberately ports that common
    /// setup seam first; the resident hit counter and opening animation remain owned by
    /// the colored-door translation rather than the generic blue-door reaction.
    /// </remarks>
    public int ApplyColoredDoorSetups(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort populationPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        int applied = 0;
        ushort cursor = populationPointer;
        for (int recordIndex = 0; recordIndex < 256; recordIndex++)
        {
            ushort header = ReadBank8fWord(bus, cursor);
            if (header == 0)
                return applied;

            byte blockX = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 2)));
            byte blockY = bus.ReadByte(0x8f0000 | unchecked((ushort)(cursor + 3)));
            cursor = unchecked((ushort)(cursor + 6));

            if (!IsColoredDoorHeader(header))
                continue;

            int blockIndex = level.GetBlockIndex(blockX, blockY);
            ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, (ushort)((originalWord & 0x0fff) | 0xc000));
            level.SetBehavior(blockIndex, 0x44);
            applied++;
        }

        throw new InvalidDataException(
            $"Room PLM population $8F:{populationPointer:X4} has no zero terminator.");
    }

    private static bool IsColoredDoorHeader(ushort header) => header is
        YellowDoorFacingLeftHeader or YellowDoorFacingRightHeader or
        YellowDoorFacingUpHeader or YellowDoorFacingDownHeader or
        GreenDoorFacingLeftHeader or GreenDoorFacingRightHeader or
        GreenDoorFacingUpHeader or GreenDoorFacingDownHeader or
        RedDoorFacingLeftHeader or RedDoorFacingRightHeader or
        RedDoorFacingUpHeader or RedDoorFacingDownHeader;

    /// <summary>
    /// Spawns the blue-door entry selected by shootable BTS <c>$40..$43</c> at
    /// <c>$94:9F26-$9F2C</c> and applies setup <c>$84:C7BB</c>.
    /// </summary>
    /// <remarks>
    /// The cap's top/left origin begins as a type-$C shootable solid. Setup changes only
    /// that origin to type $8, preserving its twelve-bit tile index; the ROM instruction
    /// list then animates all four cap blocks and ends on the ordinary air-door frame.
    /// A power bomb is the one rejected projectile family. Exhausting all forty PLM slots
    /// silently loses the request, matching <c>Spawn_PLM_to_CurrentBlockIndex</c>.
    /// </remarks>
    public bool TrySpawnBlueDoorOpening(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior is < BlueDoorFacingLeftBts or > BlueDoorFacingDownBts)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                behavior,
                "Blue-door shootable BTS must be $40 through $43.");
        }

        // Setup_BlueDoor masks the native projectile word with $0F00. A power-bomb
        // collision therefore deletes the just-allocated PLM without touching the cap.
        if ((projectileType & 0x0f00) == 0x0300)
            return false;

        ushort instructionPointer = behavior switch
        {
            BlueDoorFacingLeftBts => 0xc489,
            BlueDoorFacingRightBts => 0xc4ba,
            BlueDoorFacingUpBts => 0xc4eb,
            BlueDoorFacingDownBts => 0xc51c,
            _ => throw new InvalidOperationException(
                "Validated blue-door BTS escaped its four-way instruction table."),
        };
        ushort headerPointer = unchecked((ushort)(0xc8a2 +
            ((behavior - BlueDoorFacingLeftBts) * 6)));

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            slot.Active = true;
            slot.HeaderPointer = headerPointer;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = instructionPointer;
            slot.InstructionTimer = 1;
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.Item = null;

            // `$84:C7D3-$C7DD` is a direct LevelData write, not a PLM draw. The first
            // animated draw occurs on this actor's next handler pass and owns the VRAM
            // update, while collision observes the type-$8 origin immediately.
            ushort originalWord = level.GetCollisionBlockByIndex(blockIndex).LevelWord;
            level.SetForegroundEntry(blockIndex, (ushort)((originalWord & 0x0fff) | 0x8000));
            return true;
        }

        return false;
    }
}
