namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-authored door-cap reactions owned by bank $84's PLM pool.</summary>
public sealed partial class RoomPlmSystem
{
    private const byte BlueDoorFacingLeftBts = 0x40;
    private const byte BlueDoorFacingRightBts = 0x41;
    private const byte BlueDoorFacingUpBts = 0x42;
    private const byte BlueDoorFacingDownBts = 0x43;

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
