using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyDraygonDefeatedRoom()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetBossBits(AreaId.Maridia, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xda60);
        AssertTrue(runtime.Enemies.Draygon is null, "defeated retail state has no Draygon actor");
        runtime.StepFrame(0);
        AssertEqual((byte)1, bus.ReadByte(0x7e0000 | DraygonCannonData.UpperLeftDisabledWord),
            "pre-destroyed cannon publishes WRAM control word without boss actor");
        for (int frame = 0; frame < 120; frame++) runtime.StepFrame(0);
        AssertTrue(runtime.Enemies.Draygon is null, "cannon processing must not recreate defeated boss");
        Console.WriteLine("Defeated Draygon room: native cannon WRAM write and 121 gameplay frames pass without boss actor.");
        runtime.LoadCartridgeRoomForDebug(0xd9aa);
        runtime.LevelData!.ResolveDoorCollision(bus, 0, runtime.Samus!.Pose);
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
}
