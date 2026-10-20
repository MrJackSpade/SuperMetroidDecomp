using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a kill empties the growing shutter values the cartridge keeps in the enemy's common record.</summary>
internal static class GrowingShutterRecordStateVerification
{
    /// <summary>
    /// Installs a growing shutter state with its record-clearing operation, places its section origins and growth
    /// level, and confirms a kill empties the behaviour, every origin and the level.
    /// </summary>
    internal static void VerifyGrowingShutterRecordState()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
        var state = new GrowingShutterEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Function = Enum.GetValues<GrowingShutterFunction>()[^1];
        state.PlaceSectionOrigins(growsUp: true);
        state.GrowthLevel = 3;
        AssertEqual(new Position16<RoomPixels>(0x0100, 0x0200 - 24), state.SectionOrigin(3),
            "the fourth section sits 24 pixels above the spawn in the shutter's column");

        slot.Clear();
        AssertEqual<GrowingShutterFunction?>(null, state.Function, "a kill empties the behaviour");
        AssertEqual(Position16<RoomPixels>.Zero, state.SectionOrigin(0), "a kill empties the first origin");
        AssertEqual(Position16<RoomPixels>.Zero, state.SectionOrigin(3), "a kill empties the last origin");
        AssertEqual(0, state.GrowthLevel, "a kill empties the growth level");
        Console.WriteLine("Growing shutter record state: a kill empties every value the record holds.");
    }

    /// <summary>Runs the focused growing shutter record-state check.</summary>
    [VerificationCommand("--growing-shutter-record-state")]
    internal static int GrowingShutterRecordStateCommand()
    {
        VerifyGrowingShutterRecordState();
        return 0;
    }
}
