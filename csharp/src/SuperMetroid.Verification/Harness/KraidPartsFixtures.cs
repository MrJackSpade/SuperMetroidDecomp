using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>Kraid's parts in a fixture room, for checks that build his encounter's state without running his body.</summary>
internal static class KraidPartsFixtures
{
    /// <summary>Kraid's parts in a fresh fixture room that lists them in his room's order (#2554).</summary>
    internal static KraidParts Standalone()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        enemies.Places.BindKraidPartsInRoomOrder();
        return new KraidParts(enemies.Places, enemies.Places.InUpdateOrder[0]);
    }

    /// <summary>
    /// Places Kraid in <paramref name="enemies"/> as his room's population places him (#2554): his eight parts bound in his
    /// room's order, his body's setup beginning his encounter and his other parts set up after it. His setup sets his
    /// room's screens' scrolls, which are the room's own; the check keeps no room around him, unless it watches them itself.
    /// </summary>
    /// <returns>The encounter his body's setup began.</returns>
    internal static KraidEnemyState PlaceKraid(RoomEnemySystem enemies)
    {
        VerificationRoomEnemyServices services = enemies.VerificationServices();
        services.SetRoomScrollState ??= (_, _) => { };
        enemies.Places.BindKraidPartsInRoomOrder();
        return enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
    }
}
