using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks signed 16.16 statue descent and the exact Tourian unlock threshold.</summary>
internal static class TourianStatueDescentRoundingVerification
{
    /// <summary>Confirms $88:DC69 floors each negative quarter-pixel step, so the first step reads -1 and $88:DC90 unlocks at whole word $FF10.</summary>
    internal static void VerifyTourianStatueDescentRounding()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        foreach (EventNumber grey in new[] { EventNumber.PhantoonStatueGrey, EventNumber.RidleyStatueGrey,
                     EventNumber.DraygonStatueGrey, EventNumber.KraidStatueGrey })
            runtime.System.SetEvent(grey);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Statues);
        runtime.Player.IntroducedSamus.Control.LockHandlers();

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

    /// <summary>Runs the focused 100% movie statue descent rounding reproduction.</summary>
    [VerificationCommand("--tourian-statue-descent-rounding")]
    internal static int TourianStatueDescentRoundingCommand()
    {
        VerifyTourianStatueDescentRounding();
        return 0;
    }
}
