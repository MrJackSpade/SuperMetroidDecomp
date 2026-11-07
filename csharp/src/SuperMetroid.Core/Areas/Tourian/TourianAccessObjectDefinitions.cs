using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Fixed PLM identities used to clear or crumble the Tourian access floor.</summary>
internal static class TourianAccessObjectDefinitions
{
    /// <summary>The complete spawn identity of each floor state; the crumbling floor starts the crumble sequence.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessFloor, TourianAccessObjectDefinition> Floors =
        new Dictionary<TourianAccessFloor, TourianAccessObjectDefinition>
        {
            [TourianAccessFloor.Clear] = new(RoomObjectHeaderId.ClearAccessToTourianElevator, TourianAccessProgram.Clear),
            [TourianAccessFloor.Crumble] = new(RoomObjectHeaderId.CrumbleAccessToTourianElevator, TourianAccessProgram.Crumble),
        };
}
