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
            if (room == 0x9804)
            {
                var level = runtime.LevelData!;
                int origin = level.GetBlockIndex(15, 10);
                AssertEqual((byte)0x4f, level.GetCollisionBlock(15, 10).Behavior, "retail room loader installs rescue wall");
                runtime.Plms.TrySpawnProjectileShotBlock(level, origin, (byte)0x4f, 1, true);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertTrue(runtime.System.HasEvent(EventNumber.CrittersEscaped), "live rescue room publishes escaped event");
                var door = runtime.Plms.GreyDoors.Single();
                AssertEqual(SuperMetroid.Core.Rooms.GreyDoorPhase.Flashing, door.Phase, "rescue unlocks exit door");
                runtime.Plms.TryNotifyResidentProjectileHit(door.BlockIndex, 1);
                for (int frame = 0; frame < 32; frame++) runtime.StepFrame(0);
                AssertEqual(0, runtime.Plms.GreyDoors.Count, "exit door finishes opening");
                var samus = runtime.Samus!;
                samus.Kinematics.XPosition = 48;
                samus.Kinematics.YPosition = 112;
                samus.Kinematics.XRadius = 5;
                samus.Kinematics.YRadius = 16;
                for (int frame = 0; frame < 32 && !runtime.HasPendingDoorTransition; frame++)
                    SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -(1 << 16), plms: runtime.Plms);
                AssertTrue(runtime.HasPendingDoorTransition, "Samus can cross opened rescue exit and trigger room transition");
                AssertEqual((ushort)184, runtime.RoomLayer3Fx.TargetYPosition, "rescue liquid target matches retail FX header");
            }
            Console.WriteLine($"Escape {room:X4}: {explosionFrames} explosion frames, {offsets.Count} shake offsets.");
        }
    }
}
