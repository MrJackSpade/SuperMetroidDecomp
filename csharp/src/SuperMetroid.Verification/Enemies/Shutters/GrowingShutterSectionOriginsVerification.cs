using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks where a growing shutter places its sections.</summary>
internal static class GrowingShutterSectionOriginsVerification
{
    /// <summary>Confirms an upward-growing shutter places its fourth section 24 pixels above its spawn in its column.</summary>
    internal static void VerifyGrowingShutterSectionOrigins()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Position = slot.Position.WithWhole(new(0x0100, 0x0200));
        var state = new GrowingShutterEnemyState(slot);
        slot.SetSpeciesState(state);
        state.PlaceSectionOrigins(growsUp: true);
        AssertEqual(new Position16<RoomPixels>(0x0100, 0x0200 - 24), state.SectionOrigin(3),
            "the fourth section sits 24 pixels above the spawn in the shutter's column");
        Console.WriteLine("Growing shutter section origins: an upward shutter stacks its sections above its spawn.");
    }

    /// <summary>Runs the focused growing shutter section-origin check.</summary>
    [VerificationCommand("--growing-shutter-section-origins")]
    internal static int GrowingShutterSectionOriginsCommand()
    {
        VerifyGrowingShutterSectionOrigins();
        return 0;
    }
}
