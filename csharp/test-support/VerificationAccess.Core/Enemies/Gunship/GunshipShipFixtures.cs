using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// Samus's gunship as a room's population places it (#2554): its top hull in the system's first place, then its bottom
/// hull and entrance pad, each set up as the population load sets it up, so the top hull finds the pieces listed after it
/// and offers the pad the animation it opens and closes.
/// </summary>
internal static class GunshipShipFixtures
{
    /// <summary>Where the fixture's ship is placed: inside the room, clear of its top, as the Landing Site places its ship.</summary>
    private static readonly Position16<RoomPixels> ShipPlace = new(0x0480, 0x0480);

    /// <summary>Places the gunship in <paramref name="enemies"/>' first three places beside <paramref name="samus"/>.</summary>
    /// <returns>The top hull, which moves the ship.</returns>
    internal static GunshipTopEntity PlaceShip(RoomEnemySystem enemies, SamusState samus)
    {
        // The room's population load places the three pieces in order, the top first (#2554).
        IReadOnlyList<RoomEnemyPlace> places = Ensure.NotNull(enemies).LoadPopulation(samus,
            PopulationPlacementAccess.Entry(EnemyDefinitionId.ShipTop, EnemySpawnSettings.None, ShipPlace),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.ShipBottomEntrance, new GunshipSpawnSettings(GunshipBottomPiece.Hull), ShipPlace),
            PopulationPlacementAccess.Entry(EnemyDefinitionId.ShipBottomEntrance, new GunshipSpawnSettings(GunshipBottomPiece.EntrancePad), ShipPlace));
        _ = places[1].EntityOf<GunshipBottomEntity>();
        _ = places[2].EntityOf<GunshipBottomEntity>();
        return places[0].EntityOf<GunshipTopEntity>();
    }
}
