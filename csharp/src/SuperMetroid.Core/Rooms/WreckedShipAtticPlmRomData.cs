namespace SuperMetroid.Core.Rooms;

/// <summary>Named bank-$84 callback pointers owned by Wrecked Ship attic PLM $BB05.</summary>
public static class WreckedShipAtticPlmRomData
{
    /// <summary>
    /// <c>$84:BAFA Setup_WreckedShipAttic</c>. Both the header setup and resident
    /// pre-instruction execute the same intentionally inert REP/SEP/RTS routine.
    /// </summary>
    public const ushort NoOpCallback = 0xbafa;

    /// <summary>
    /// <c>$84:BAFF-BB04 InstList_PLM_WreckedShipAttic</c>: install the inert
    /// callback once, then sleep permanently. This is complete resident
    /// controller logic, not editable visual data.
    /// </summary>
    internal static bool TryReadInstructionWord(ushort address, out ushort value)
    {
        value = address switch
        {
            RoomPlmInstructionLists.WreckedShipAttic =>
                RoomPlmInstructionCodes.InstallPreInstruction,
            RoomPlmInstructionLists.WreckedShipAttic + 2 => NoOpCallback,
            RoomPlmInstructionLists.WreckedShipAttic + 4 => RoomPlmInstructionCodes.Sleep,
            _ => 0,
        };
        return address is RoomPlmInstructionLists.WreckedShipAttic or
            RoomPlmInstructionLists.WreckedShipAttic + 2 or
            RoomPlmInstructionLists.WreckedShipAttic + 4;
    }
}
