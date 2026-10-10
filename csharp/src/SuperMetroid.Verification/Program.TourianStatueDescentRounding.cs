using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: the statues' BG2 descent ($88:DC69) adds $FFFF:C000 to HDMAObject_Var1:Var0.
    // As a signed 16.16 value its whole word floors, so the first quarter-pixel step already
    // reads -1, and $88:DC90's $FF10 test unlocks Tourian three quarter-steps before 240.0.
    // In the 100% movie the port's truncating offset lowered every statue a frame late.
    /// <summary>Verifies that signed 16.16 statue descent floors fractional steps and that Tourian unlocks when the native whole-word threshold is reached.</summary>
    private static void VerifyTourianStatueDescentRounding()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        foreach (EventNumber grey in new[] { EventNumber.PhantoonStatueGrey, EventNumber.RidleyStatueGrey,
                     EventNumber.DraygonStatueGrey, EventNumber.KraidStatueGrey })
            runtime.System.SetEvent(grey);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        runtime.Samus!.InputLocked = true;

        StepUntil(() => runtime.TourianStatues.VerticalOffset != 0, _ => runtime.StepFrame(0),
            maximumFrames: 400, context: "the statue descent begins");
        AssertEqual((short)-1, runtime.TourianStatues.VerticalOffset, "the first quarter step floors to -1");
        for (int frame = 0; frame < 3; frame++)
            runtime.StepFrame(0);
        AssertEqual((short)-1, runtime.TourianStatues.VerticalOffset, "steps two to four stay at -1");
        runtime.StepFrame(0);
        AssertEqual((short)-2, runtime.TourianStatues.VerticalOffset, "the fifth quarter step reaches -2");

        StepUntil(() => runtime.System.HasEvent(EventNumber.TourianUnlocked), _ => runtime.StepFrame(0),
            maximumFrames: 1000, context: "the statues finish descending");
        AssertEqual((short)-240, runtime.TourianStatues.VerticalOffset,
            "Tourian unlocks the frame the whole word first reads $FF10");
        Console.WriteLine("Tourian statue descent rounding: the signed 16.16 descent floors and unlocks at $FF10.");
    }
}
