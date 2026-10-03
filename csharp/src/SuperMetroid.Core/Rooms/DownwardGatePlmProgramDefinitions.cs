using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bounded decoder of $84:BC13..BC60 and BCAF..BCDE. Resident control actions
/// are named cases; four timed closing/opening frames traverse the draw records
/// in opposite directions, with sixteen-frame holds and a twenty-four-frame final
/// hold. Eight trigger programs each draw for one frame then delete. Packed sound
/// bytes change word alignment; only actual instruction/operand boundaries exist.
/// No word, byte or generated lookup cache is stored.
/// </summary>
internal static class DownwardGatePlmProgramDefinitions
{
    /// <summary>$84:BC13: open-and-wait list, entered again after opening completes.</summary>
    private const ushort OpenStart = 0xbc13;
    /// <summary>$84:BC2A: first timed closing frame.</summary>
    private const ushort ClosingFrames = 0xbc2a;
    /// <summary>$84:BC4D: first timed opening frame.</summary>
    private const ushort OpeningFrames = 0xbc4d;
    /// <summary>$84:BC29: packed movement sound after the closing spawn command.</summary>
    private const ushort ClosingSoundAddress = 0xbc29;
    /// <summary>$84:BC4C: packed movement sound after the opening wake command.</summary>
    private const ushort OpeningSoundAddress = 0xbc4c;
    /// <summary>$84:BC61: adjacent upward-gate program, outside this decoder.</summary>
    private const ushort ResidentEnd = 0xbc61;
    /// <summary>$84:BCDF: adjacent upward-trigger program, outside this decoder.</summary>
    private const ushort TriggerEnd = 0xbcdf;

    internal static IEnumerable<(ushort Address, ushort Value)> MechanicsWords
    {
        get
        {
            for (int address = OpenStart; address < ResidentEnd; address++)
                if (TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
            for (int address = RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft; address < TriggerEnd; address += 2)
                if (TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
        }
    }

    internal static IEnumerable<(ushort Address, byte Value)> MechanicsBytes
    {
        get
        {
            yield return (ClosingSoundAddress, DownwardGatePlmRomData.MovementSound);
            yield return (OpeningSoundAddress, DownwardGatePlmRomData.MovementSound);
        }
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        bool owned = address is ClosingSoundAddress or OpeningSoundAddress;
        value = owned ? DownwardGatePlmRomData.MovementSound : (byte)0;
        return owned;
    }

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int triggerOffset = address - RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft;
        if (triggerOffset >= 0 && address < TriggerEnd && (triggerOffset & 1) == 0)
        {
            int trigger = triggerOffset / 6;
            value = (triggerOffset % 6) switch
            {
                0 => 1,
                2 => (ushort)(0xa5d7 + trigger / 2 * 20 + (trigger % 2) * 12),
                _ => RoomPlmInstructionCodes.Delete,
            };
            return true;
        }
        int closingOffset = address - ClosingFrames, openingOffset = address - OpeningFrames;
        bool closing = closingOffset is >= 0 and < 16 && (closingOffset & 1) == 0;
        bool opening = openingOffset is >= 0 and < 16 && (openingOffset & 1) == 0;
        if (closing || opening)
        {
            int offset = closing ? closingOffset : openingOffset;
            int frame = offset / 4;
            value = offset % 4 == 0 ? (ushort)(frame == 3 ? 24 : 16) :
                (ushort)(closing ? 0xa525 + frame * 14 : 0xa54f - frame * 14);
            return true;
        }
        value = address switch
        {
            0xbc13 or 0xbc3a => 1,
            0xbc15 or 0xbc21 => 0xa517,
            0xbc1f => 16,
            0xbc3c => 0xa55d,
            0xbc17 or 0xbc3e => RoomPlmInstructionCodes.ClearDownwardGateTrigger,
            0xbc19 or 0xbc40 => RoomPlmInstructionCodes.InstallPreInstruction,
            0xbc1b => DownwardGatePreInstructionCodes.WakeIfTriggered,
            0xbc42 => DownwardGatePreInstructionCodes.WakeIfTriggeredOrSamusBelow,
            0xbc1d or 0xbc44 => RoomPlmInstructionCodes.Sleep,
            0xbc23 => RoomPlmInstructionCodes.SpawnDownwardGateProjectile,
            0xbc25 => (ushort)RoomEnemyProjectileKind.DownwardGateMoving,
            0xbc27 or 0xbc4a => RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6,
            0xbc46 => RoomPlmInstructionCodes.WakeDownwardGateProjectile,
            // Native BBF0 advances over this operand without using its value.
            0xbc48 => DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep,
            0xbc5d => RoomPlmInstructionCodes.Goto,
            0xbc5f => OpenStart,
            _ => 0,
        };
        return value != 0;
    }
}
