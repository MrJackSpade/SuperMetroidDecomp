using SuperMetroid.Core.Enemies.Shutters;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;

/// <summary>Checks where a growing shutter places its sections.</summary>
internal static class GrowingShutterSectionOriginsVerification
{
    /// <summary>Confirms an upward-growing shutter places its fourth section 24 pixels above its spawn row.</summary>
    internal static void VerifyGrowingShutterSectionOrigins()
    {
        var state = new GrowingShutterEnemyState(GrowingShutterAction.WaitToGrowUpForTimer);
        state.PlaceSectionOrigins(spawnY: 0x0200, growsUp: true);
        AssertEqual(0x0200 - 24, state.SectionOriginY(3), "the fourth section sits 24 pixels above the spawn row");
        state.PlaceSectionOrigins(spawnY: 8, growsUp: true);
        AssertEqual(65536 - 16, state.SectionOriginY(3), "an origin above the room's top row wraps to its bottom");
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
