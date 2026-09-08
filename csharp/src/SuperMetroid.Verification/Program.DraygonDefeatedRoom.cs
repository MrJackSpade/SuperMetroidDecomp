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
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
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
            transition.Step(runtime, audio, 0);
        AssertEqual(DoorTransitionPhase.HandleTransition, transition.Phase, "Space Jump exit reaches finalization");
        var writer = typeof(RoomPlmSystem).GetField("_disableDraygonCannon", BindingFlags.NonPublic | BindingFlags.Instance)!;
        writer.SetValue(runtime.Plms, (Action<ushort>)(_ => throw new IOException("injected cannon failure")));
        bool failed = false;
        try { transition.Step(runtime, audio, 0); }
        catch (IOException error) when (error.Message == "injected cannon failure") { failed = true; }
        AssertTrue(failed, "destination actor frame reproduces the failure after scroll finalization");
        writer.SetValue(runtime.Plms, (Action<ushort>)runtime.Enemies.DisableDraygonCannon);
        for (int frame = 0; transition.IsActive && frame < 120; frame++) transition.Step(runtime, audio, 0);
        AssertEqual(DoorTransitionPhase.Complete, transition.Phase, "retry finishes destination fade without finalizing scroll twice");
        Console.WriteLine("Space Jump to defeated Draygon: injected post-scroll failure recovers through destination OAM and fade.");
    }
}
