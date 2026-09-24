namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed first-instruction pointers in the two bank-$84 downward-gate PLM headers.
/// The resident gate and its shot block still take their setup behavior from their
/// separately compiled header identities and room argument.
/// </summary>
internal static class DownwardGatePlmHeaderDefinitions
{
    /// <summary>Bank-$84 resident header's first-instruction word at $84:C82C.</summary>
    internal const int ResidentInitialInstructionAddress =
        0x840000 | (RoomPlmHeaders.DownwardGate + 2);

    /// <summary>Bank-$84 trigger header's first-instruction word at $84:C838.</summary>
    internal const int ShotBlockInitialInstructionAddress =
        0x840000 | (RoomPlmHeaders.DownwardGateShotBlock + 2);

    /// <summary>$84:C82C begins the resident gate in its open-and-wait list.</summary>
    internal const ushort ResidentInitialInstruction =
        RoomPlmInstructionLists.DownwardGateOpening;

    /// <summary>$84:C838 begins the blue-left trigger; setup selects the other seven variants.</summary>
    internal const ushort ShotBlockInitialInstruction =
        RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft;

    internal static bool TryGetInitialInstruction(ushort header, out ushort instruction)
    {
        instruction = header switch
        {
            RoomPlmHeaders.DownwardGate => ResidentInitialInstruction,
            RoomPlmHeaders.DownwardGateShotBlock => ShotBlockInitialInstruction,
            _ => 0,
        };
        return instruction != 0;
    }
}
