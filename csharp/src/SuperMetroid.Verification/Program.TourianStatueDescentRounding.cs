using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Represents tourian statue descent rounding verification.</summary>
internal static class TourianStatueDescentRoundingVerification
{
    // #1269: the statues' BG2 descent ($88:DC69) adds $FFFF:C000 to HDMAObject_Var1:Var0.
    // As a signed 16.16 value its whole word floors, so the first quarter-pixel step already
    // reads -1, and $88:DC90's $FF10 test unlocks Tourian three quarter-steps before 240.0.
    // In the 100% movie the port's truncating offset lowered every statue a frame late.
    /// <summary>verify tourian statue descent rounding.</summary>
    internal static void VerifyTourianStatueDescentRounding()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        foreach (EventNumber grey in new[] { EventNumber.PhantoonStatueGrey, EventNumber.RidleyStatueGrey,
                     EventNumber.DraygonStatueGrey, EventNumber.KraidStatueGrey })
            runtime.System.SetEvent(grey);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        runtime.Player.Samus!.InputLocked = true;

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

    /// <summary>tourian statue descent rounding command.</summary>
    [VerificationCommand("--tourian-statue-descent-rounding")]
    internal static int TourianStatueDescentRoundingCommand()
    {
        VerifyTourianStatueDescentRounding();
        return 0;
    }
}
