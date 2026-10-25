using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Saves;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms the boot-time corrupt-SRAM marker write and demo-set count latch (#1339).</summary>
internal static class BootDemoSetCountVerification
{
    /// <summary>
    /// #1339: boot's <c>DetermineNumberOfDemoSets</c> (<c>$80:8261</c>, called once from <c>$80:8564</c>) overwrites
    /// the completion marker with <c>madadameyohn</c> when all three slots fail <c>LoadFromSRAM</c>
    /// (<c>$80:8283-8290</c>), and fixes <c>NumberOfDemoSets</c> (<c>$1F59</c>) until reset. Bank <c>$82</c> only reads it.
    /// </summary>
    internal static void VerifyBootDemoSetCount()
    {
        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        saveRam.SetGameCompleted(true);
        AssertTrue(saveRam.ReadSlot(0) is null && saveRam.ReadSlot(1) is null && saveRam.ReadSlot(2) is null,
            "fixture has no valid slot");

        var game = new SuperMetroidGame(saveRam);
        AssertEqual("madadameyohn", ReadMarker(saveRam),
            "boot with no valid slot overwrites the completion marker with 'madadameyohn'");
        AssertTrue(!saveRam.HasCompletedGame, "the overwritten marker no longer reads as a completed game");
        AssertEqual(DemoSetUnlockDefinitions.SetsBeforeCompletion, game.Session.AvailableDemoSetCount(),
            "boot with no valid slot enables three demo sets");

        // Saving creates a valid slot. The marker stays overwritten and the boot count stays latched.
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        AssertEqual("madadameyohn", ReadMarker(saveRam),
            "a later save does not restore the completion marker");
        AssertEqual(DemoSetUnlockDefinitions.SetsBeforeCompletion, game.Session.AvailableDemoSetCount(),
            "a save after boot does not unlock the fourth demo set mid-session");

        // The ending writes the marker mid-session; NumberOfDemoSets still waits for reset.
        saveRam.SetGameCompleted(true);
        AssertEqual(DemoSetUnlockDefinitions.SetsBeforeCompletion, game.Session.AvailableDemoSetCount(),
            "a mid-session completion marker does not change the boot demo-set count");
        var completedBoot = new SuperMetroidGame(saveRam);
        AssertEqual("supermetroid", ReadMarker(saveRam),
            "boot with a valid slot leaves the completion marker intact");
        AssertEqual(DemoSetUnlockDefinitions.SetCount, completedBoot.Session.AvailableDemoSetCount(),
            "reset with a valid completed save enables the fourth demo set");
        saveRam.SetGameCompleted(false);
        AssertEqual(DemoSetUnlockDefinitions.SetCount, completedBoot.Session.AvailableDemoSetCount(),
            "clearing the marker mid-session does not change the boot demo-set count");
        Console.WriteLine("Boot demo sets: no valid slot writes 'madadameyohn'; the count is fixed until reset.");
    }

    /// <summary>Reads the completion marker of the saves' SRAM image as ASCII.</summary>
    private static string ReadMarker(SaveSlotStore saves) => System.Text.Encoding.ASCII.GetString(
        SaveRamCodec.Encode(saves, RetailPresentationFixture()), SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length);

    /// <summary>Runs the boot demo-set count suite.</summary>
    [VerificationCommand("--boot-demo-set-count")]
    internal static int BootDemoSetCountCommand()
    {
        VerifyBootDemoSetCount();
        return 0;
    }
}
