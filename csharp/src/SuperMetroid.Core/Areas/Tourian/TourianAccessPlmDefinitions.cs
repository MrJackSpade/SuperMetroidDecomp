using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Fixed PLM identities used to clear or crumble the Tourian access floor.</summary>
internal static class TourianAccessPlmDefinitions
{
    /// <summary>The complete native spawn identity of each floor state; <c>$84:AAE5</c> starts the crumble sequence.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessFloor, TourianAccessPlmDefinition> Floors =
        new Dictionary<TourianAccessFloor, TourianAccessPlmDefinition>
        {
            [TourianAccessFloor.Clear] = new(PlmHeaderId.ClearAccessToTourianElevator, TourianAccessProgram.Clear),
            [TourianAccessFloor.Crumble] = new(PlmHeaderId.CrumbleAccessToTourianElevator, TourianAccessProgram.Crumble),
        };
}
