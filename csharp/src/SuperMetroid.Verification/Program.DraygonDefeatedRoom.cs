using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;
using System.Reflection;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents draygon defeated room verification.</summary>
internal static class DraygonDefeatedRoomVerification
{
    /// <summary>Verifies draygon defeated room.</summary>
    internal static void VerifyDraygonDefeatedRoom()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetBossBits(AreaId.Maridia, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xda60);
        AssertTrue(runtime.Enemies.DraygonBehavior.Draygon is null, "defeated retail state has no Draygon actor");
        runtime.StepFrame(0);
        AssertEqual((ushort)1, new DraygonCannonVerificationMemory(bus).ControlWord(DraygonCannonControlWord.UpperLeft),
            "pre-destroyed cannon publishes WRAM control word without boss actor");
        for (int frame = 0; frame < 120; frame++) runtime.StepFrame(0);
        AssertTrue(runtime.Enemies.DraygonBehavior.Draygon is null, "cannon processing must not recreate defeated boss");
        Console.WriteLine("Defeated Draygon room: native cannon WRAM write and 121 gameplay frames pass without boss actor.");
        runtime.LoadCartridgeRoomForDebug(0xd9aa);
        runtime.Room.LevelData!.ResolveDoorCollision(bus, 0, runtime.Player.Samus!.Pose);
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        for (int frame = 0; transition.Phase != DoorTransitionPhase.HandleTransition && frame < 600; frame++)
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
        AssertEqual(DoorTransitionPhase.HandleTransition, transition.Phase, "Space Jump exit reaches finalization");
        // $82:E6A2 finalizes the scroll and returns before the E737 actor/fade dispatch, so a
        // destination actor frame can never re-enter scroll finalization (#387).
        transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
        AssertEqual(DoorTransitionPhase.BuildDestinationOam, transition.Phase,
            "scroll finalization returns before the destination actor frame");
        for (int frame = 0; transition.IsActive && frame < 120; frame++) transition.Step(runtime, audio, 0, SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance);
        AssertEqual(DoorTransitionPhase.Complete, transition.Phase, "destination OAM and fade complete after one scroll finalization");
        Console.WriteLine("Space Jump to defeated Draygon: scroll finalization precedes destination OAM and fade.");
    }

    /// <summary>Draygon defeated room command.</summary>
    [VerificationCommand("--draygon-defeated-room")]
    internal static int DraygonDefeatedRoomCommand()
    {
        Suite(nameof(VerifyDraygonDefeatedRoom), () => VerifyDraygonDefeatedRoom());
        return 0;
    }
}
