using SuperMetroid.Core.Enemies.Shutters;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;

/// <summary>Checks where a growing shutter places its sections.</summary>
internal static class GrowingShutterSectionOriginsVerification
{
    /// <summary>
    /// Confirms an upward-growing shutter places its fourth section 24 pixels above its spawn row, and a downward one, as
    /// every placement grows, 24 pixels below it.
    /// </summary>
    internal static void VerifyGrowingShutterSectionOrigins()
    {
        var state = new GrowingShutterEnemyState(GrowingShutterAction.WaitForTimer, spawnY: 0x0200, VerticalDirection.Up);
        AssertEqual(0x0200 - 24, state.SectionOriginY(3), "the fourth section sits 24 pixels above the spawn row");
        state = new GrowingShutterEnemyState(GrowingShutterAction.WaitForTimer, spawnY: 296, VerticalDirection.Down);
        AssertEqual(296 + 24, state.SectionOriginY(3), "a downward shutter's fourth section sits 24 pixels below the spawn row");
        Console.WriteLine("Growing shutter section origins: a shutter stacks its sections from its spawn the way it grows.");
    }

    /// <summary>Runs the focused growing shutter section-origin check.</summary>
    [VerificationCommand("--growing-shutter-section-origins")]
    internal static int GrowingShutterSectionOriginsCommand()
    {
        VerifyGrowingShutterSectionOrigins();
        return 0;
    }
}
