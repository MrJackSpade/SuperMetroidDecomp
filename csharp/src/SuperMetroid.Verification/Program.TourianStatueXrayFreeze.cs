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
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents tourian statue xray freeze verification.</summary>
internal static class TourianStatueXrayFreezeVerification
{
    // #1269: the Tourian entrance statues are bank-$87 animated-tile objects. XraySetup calls
    // Disable_AnimatedTilesObjects ($91:E239) and Set_NonXray_SamusPose re-enables them
    // ($91:E34B), so the statues' timers stand still while the scope is up. In the 100% movie
    // an X-ray in the statue room delays the first unlock by the scope's 175 frames.
    /// <summary>Verifies tourian statue xray freeze.</summary>
    internal static void VerifyTourianStatueXrayFreeze()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertTrue(runtime.TourianStatues.Enabled, "the statue room runs the statue sequence");
        runtime.Player.Samus!.InputLocked = true;

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var objects = (System.Collections.IList)typeof(TourianStatueSequence)
            .GetField("objects", flags)!.GetValue(runtime.TourianStatues)!;
        string Snapshot() => string.Join(" ", objects.Cast<object>().Select(o =>
        {
            Type t = o.GetType();
            return $"{t.GetField("Pointer")!.GetValue(o)}/{t.GetField("Timer")!.GetValue(o)}";
        }));
        var suspended = typeof(SamusXrayState).GetProperty(nameof(SamusXrayState.SuspendedSubsystems))!;

        runtime.StepFrame(0);
        suspended.SetValue(runtime.Player.Samus.Xray, XraySuspendedSubsystems.All);
        string frozen = Snapshot();
        for (int frame = 0; frame < 20; frame++)
            runtime.StepFrame(0);
        AssertEqual(frozen, Snapshot(), "the X-ray scope freezes every statue program");

        suspended.SetValue(runtime.Player.Samus.Xray, XraySuspendedSubsystems.None);
        runtime.StepFrame(0);
        AssertTrue(frozen != Snapshot(), "the statues resume once the scope re-enables animated tiles");
        Console.WriteLine("Tourian statue X-ray freeze: the scope suspends the statues' animated-tile programs.");
    }

    /// <summary>Tourian statue xray freeze command.</summary>
    [VerificationCommand("--tourian-statue-xray-freeze")]
    internal static int TourianStatueXrayFreezeCommand()
    {
        VerifyTourianStatueXrayFreeze();
        return 0;
    }
}
