using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Translation of the resident <c>$84:DB44</c> PLM that marks Tourian's four Metroid rooms
/// cleared after their native enemy death quota is satisfied.
/// </summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Applies setup <c>$84:DB1E</c>: select one bank-$84 pre-instruction through the exact
    /// room-argument table and leave the actor on its permanent sleeping instruction list.
    /// </summary>
    private static void SetupMetroidsClearedSlot(PlmSlot slot)
    {
        slot.PreInstruction = MetroidsClearedPlmRomData.ResolvePreInstruction(slot.RoomArgument);
        slot.InstructionPointer =
            RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired;
    }

    /// <summary>
    /// Runs the selected pre-instruction before the ordinary timer/list pass, matching
    /// <c>PLM_Handler</c>. Arguments below $12 are deliberate RTS handlers. The four active
    /// handlers compare the complete enemy-death word with the room's required-kill byte and
    /// repeatedly mark their idempotent event once the quota is reached.
    /// </summary>
    private void RunMetroidsClearedPreInstruction(
        PlmSlot slot,
        ushort enemyDeaths,
        byte enemyDeathQuota)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired)
            return;

        ushort expectedPreInstruction =
            MetroidsClearedPlmRomData.ResolvePreInstruction(slot.RoomArgument);
        if (slot.PreInstruction != expectedPreInstruction)
        {
            throw new InvalidDataException(
                $"Metroids-cleared PLM argument ${slot.RoomArgument:X4} selected " +
                $"pre-instruction $84:{slot.PreInstruction:X4}, expected " +
                $"$84:{expectedPreInstruction:X4}.");
        }

        EventNumber? eventNumber = MetroidsClearedPlmRomData.ResolveEvent(slot.RoomArgument);
        if (eventNumber is null || enemyDeaths < enemyDeathQuota)
            return;

        (_setEvent ?? throw new InvalidOperationException(
            "Metroids-cleared PLM reached its kill quota without a room event writer."))(
                eventNumber.Value);
    }
}
