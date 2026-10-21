using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
namespace SuperMetroid.Verification.Bosses.Draygon;

using SuperMetroid.Verification.Harness;

/// <summary>Checks cannon processing and door exit after Draygon has already been defeated.</summary>
internal static class DraygonDefeatedRoomVerification
{
    /// <summary>Verifies cannon disabling without a boss actor and a single scroll finalization before destination fade.</summary>
    internal static void VerifyDraygonDefeatedRoom()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetBossBits(AreaId.Maridia, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
        AssertTrue(runtime.Enemies.DraygonBehavior.Parts.Draygon is null, "defeated retail state has no Draygon actor");
        for (int frame = 0; frame < 121; frame++) runtime.StepFrame(0);
        AssertTrue(runtime.Enemies.DraygonBehavior.Parts.Draygon is null, "cannon processing must not recreate defeated boss");
        Console.WriteLine("Defeated Draygon room: pre-destroyed cannon and 121 gameplay frames pass without boss actor.");
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.SpaceJump);
        runtime.Room.LoadedLevelData.ResolveDoorCollision(0, runtime.Player.IntroducedSamus.Pose.ForRoomCollision());
        var transition = new DoorTransitionState();
        var audio = new CartridgeAudioState();
        transition.Begin(runtime);
        for (int frame = 0; transition.Phase != DoorTransitionPhase.HandleTransition && frame < 600; frame++)
            transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
        AssertEqual(DoorTransitionPhase.HandleTransition, transition.Phase, "Space Jump exit reaches finalization");
        // $82:E6A2 finalizes the scroll and returns before the E737 actor/fade dispatch, so a
        // destination actor frame can never re-enter scroll finalization (#387).
        transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
        AssertEqual(DoorTransitionPhase.BuildDestinationOam, transition.Phase,
            "scroll finalization returns before the destination actor frame");
        for (int frame = 0; transition.IsActive && frame < 120; frame++) transition.Step(runtime, audio, 0, SuperMetroid.Core.Compatibility.InstantRoomLoad.Instance);
        AssertEqual(DoorTransitionPhase.Complete, transition.Phase, "destination OAM and fade complete after one scroll finalization");
        Console.WriteLine("Space Jump to defeated Draygon: scroll finalization precedes destination OAM and fade.");
    }

    /// <summary>Runs the defeated Draygon room and Space Jump door transition regression.</summary>
    [VerificationCommand("--draygon-defeated-room")]
    internal static int DraygonDefeatedRoomCommand()
    {
        Suite(nameof(VerifyDraygonDefeatedRoom), () => VerifyDraygonDefeatedRoom());
        return 0;
    }
}
