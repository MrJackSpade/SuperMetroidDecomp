namespace SuperMetroid.Core.Rooms;

/// <summary>Fixed PLM identities used to clear or crumble the Tourian access floor.</summary>
internal static class TourianAccessPlmDefinitions
{

    /// <summary>
    /// <c>$84:AAE5</c>, initial list for the Tourian access-floor crumble sequence.
    /// </summary>
    public const ushort CrumbleInstructionList = 0xaae5;


    /// <summary><c>$84:AB0C</c>, initial list that immediately clears the access floor.</summary>
    public const ushort ClearInstructionList = 0xab0c;

    /// <summary>Returns the complete native spawn identity for the requested floor state.</summary>
    public static TourianAccessPlmDefinition ForState(bool clear) => clear
        ? new(PlmHeaderId.ClearAccessToTourianElevator, ClearInstructionList)
        : new(PlmHeaderId.CrumbleAccessToTourianElevator, CrumbleInstructionList);
}

/// <summary>One Tourian access-floor PLM header and its initial instruction list.</summary>
internal readonly record struct TourianAccessPlmDefinition(
    PlmHeaderId HeaderPointer,
    ushort InstructionListPointer);
