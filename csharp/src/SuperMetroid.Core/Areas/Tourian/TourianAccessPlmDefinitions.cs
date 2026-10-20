using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Fixed PLM identities used to clear or crumble the Tourian access floor.</summary>
internal static class TourianAccessPlmDefinitions
{

    /// <summary>
    /// <c>$84:AAE5</c>, initial list for the Tourian access-floor crumble sequence.
    /// </summary>


    /// <summary>Returns the complete native spawn identity for the requested floor state.</summary>
    public static TourianAccessPlmDefinition ForState(TourianAccessFloor floor) => floor switch
    {
        TourianAccessFloor.Clear => new(PlmHeaderId.ClearAccessToTourianElevator, TourianAccessProgram.Clear),
        TourianAccessFloor.Crumble => new(PlmHeaderId.CrumbleAccessToTourianElevator, TourianAccessProgram.Crumble),
        _ => throw new InvalidOperationException($"Undefined {nameof(TourianAccessFloor)} {floor}."),
    };
}
