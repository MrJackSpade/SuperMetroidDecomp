using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyEscapeRoomEffects(string romPath)
    {
        foreach (ushort room in new ushort[] { 0xde4d, 0xde7a, 0xdea7, 0xdede, 0x92fd, 0x9804 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
            runtime.LoadCartridgeRoomForDebug(room);
            runtime.Samus!.InputLocked = true;
            int explosionFrames = 0;
            var offsets = new HashSet<(short, short)>();
            for (int frame = 0; frame < 80; frame++)
            {
                runtime.StepFrame(0);
                if (runtime.Enemies.RoomSpriteObjects.Any(s => s.IsActive && s.SpritemapPointer != 0)) explosionFrames++;
                var shake = runtime.Enemies.LastRoomShake;
                offsets.Add((shake.Bg1X, shake.Bg1Y));
            }
            AssertTrue(explosionFrames > 10, $"escape {room:X4} produces animated explosion sprites");
            AssertTrue(offsets.Count > 1, $"escape {room:X4} alternates actual background scroll offsets");
            Console.WriteLine($"Escape {room:X4}: {explosionFrames} explosion frames, {offsets.Count} shake offsets.");
        }
    }
}
