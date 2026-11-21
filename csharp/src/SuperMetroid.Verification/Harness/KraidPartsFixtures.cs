using SuperMetroid.Core.Bosses.Kraid;

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
}
