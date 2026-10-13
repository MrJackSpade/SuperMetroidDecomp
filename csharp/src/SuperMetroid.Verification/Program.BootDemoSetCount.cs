using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using static AssertionsVerification;
using static RetailPresentationFixtureVerification;

internal static class BootDemoSetCountVerification
{
    // #1339: boot's DetermineNumberOfDemoSets ($80:8261, called once from $80:8564) overwrites
    // the completion marker with 'madadameyohn' when all three slots fail LoadFromSRAM
    // ($80:8283-8290), and fixes NumberOfDemoSets ($1F59) until reset. Bank $82 only reads it.
    internal static void VerifyBootDemoSetCount()
    {
        var bus = new TestAddressSpace();
        var saveRam = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        saveRam.SetGameCompleted(true);
        AssertTrue(saveRam.ReadSlot(0) is null && saveRam.ReadSlot(1) is null && saveRam.ReadSlot(2) is null,
            "fixture has no valid slot");

        var game = new SuperMetroidGame(bus);
        AssertEqual("madadameyohn", ReadMarker(bus),
            "boot with no valid slot overwrites the completion marker with 'madadameyohn'");
        AssertTrue(!saveRam.HasCompletedGame, "the overwritten marker no longer reads as a completed game");
        AssertEqual(AttractDemoRomData.DefaultSetCount, game.AvailableDemoSetCount(),
            "boot with no valid slot enables three demo sets");

        // Saving creates a valid slot. The marker stays overwritten and the boot count stays latched.
        saveRam.SaveSlot(0, new SuperMetroidSaveSnapshot());
        AssertEqual("madadameyohn", ReadMarker(bus),
            "a later save does not restore the completion marker");
        AssertEqual(AttractDemoRomData.DefaultSetCount, game.AvailableDemoSetCount(),
            "a save after boot does not unlock the fourth demo set mid-session");

        // The ending writes the marker mid-session; NumberOfDemoSets still waits for reset.
        saveRam.SetGameCompleted(true);
        AssertEqual(AttractDemoRomData.DefaultSetCount, game.AvailableDemoSetCount(),
            "a mid-session completion marker does not change the boot demo-set count");
        var completedBoot = new SuperMetroidGame(bus);
        AssertEqual("supermetroid", ReadMarker(bus),
            "boot with a valid slot leaves the completion marker intact");
        AssertEqual(AttractDemoRomData.SetCount, completedBoot.AvailableDemoSetCount(),
            "reset with a valid completed save enables the fourth demo set");
        saveRam.SetGameCompleted(false);
        AssertEqual(AttractDemoRomData.SetCount, completedBoot.AvailableDemoSetCount(),
            "clearing the marker mid-session does not change the boot demo-set count");
        Console.WriteLine("Boot demo sets: no valid slot writes 'madadameyohn'; the count is fixed until reset.");
    }

    private static string ReadMarker(TestAddressSpace bus) => System.Text.Encoding.ASCII.GetString(
        SaveRamImage.Capture(bus), SaveRamLayout.CompletionMarkerOffset, SaveRamLayout.CompletionMarker.Length);

    [VerificationCommand("--boot-demo-set-count")]
    internal static int BootDemoSetCountCommand()
    {
        VerifyBootDemoSetCount();
        return 0;
    }
}
