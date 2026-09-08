using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

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
    }
}
