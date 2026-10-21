using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Shared checks that a corpse's rot table is the one its configuration record names.</summary>
internal static class CorpseRotTableChecks
{
    /// <summary>
    /// Requires <paramref name="actual"/> to hold the configured entry count and to be the configured rows of the
    /// rotting heap: a delay written through it is read back at the configured location.
    /// </summary>
    internal static void AssertConfiguredRotTable(
        RoomEnemySystem enemies, int configuredOffset, int configuredEntryCount, CorpseRotTable actual, string context)
    {
        AssertEqual(configuredEntryCount, actual.EntryCount, $"{context} rot entry count");
        CorpseRotTable configured = enemies.Room.Staging.RotTable(configuredOffset, configuredEntryCount);
        int last = configuredEntryCount - 1;
        ushort probe = unchecked((ushort)(configured.Delay(last) + 1));
        actual.SetDelay(last, probe);
        AssertEqual(probe, configured.Delay(last), $"{context} rot table occupies the configured heap rows");
    }
}
