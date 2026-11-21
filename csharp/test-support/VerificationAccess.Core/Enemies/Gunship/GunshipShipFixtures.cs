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
    /// <summary>Places the gunship in <paramref name="enemies"/>' first three places beside <paramref name="samus"/>.</summary>
    /// <returns>The top hull, which moves the ship.</returns>
    internal static GunshipTopEntity PlaceShip(RoomEnemySystem enemies, SamusState samus)
    {
        IReadOnlyList<RoomEnemyPlace> places = Ensure.NotNull(enemies).Places.InUpdateOrder;
        GunshipTopEntity top = places[0].PlaceAs<GunshipTopEntity>(EnemyDefinitionId.ShipTop, EnemySpawnSettings.None, samus);
        places[1].PlaceAs<GunshipBottomEntity>(EnemyDefinitionId.ShipBottomEntrance, new GunshipSpawnSettings(GunshipBottomPiece.Hull), samus);
        places[2].PlaceAs<GunshipBottomEntity>(EnemyDefinitionId.ShipBottomEntrance, new GunshipSpawnSettings(GunshipBottomPiece.EntrancePad), samus);
        return top;
    }
}
