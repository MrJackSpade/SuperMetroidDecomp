using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyRetailWrapShotDoors()
    {
        foreach (var setup in new[] {
            (Room: RoomHeaderPointers.LandingSite, X: 2260, Y: 546, Target: 0x1561, Beams: 9, Hit: 9),
            (Room: RoomHeaderPointers.Crocomire, X: 2004, Y: 34, Target: 0x0301, Beams: 9, Hit: 9),
            (Room: RoomHeaderPointers.GreenBrinstarMainShaft, X: 38, Y: 1606, Target: 0x2981, Beams: 5, Hit: 5),
        })
        foreach (int beams in new[] { setup.Beams, 1, 0 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(setup.Room);
            var level = runtime.LevelData!;
            bool left = setup.Room == RoomHeaderPointers.GreenBrinstarMainShaft;
            var samus = runtime.Samus!;
            samus.XPosition = (ushort)setup.X; samus.YPosition = (ushort)setup.Y;
            samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
            samus.PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose;
            samus.EquippedBeams = (ushort)beams;
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlockByIndex(setup.Target).CollisionType, "Retail remote cap begins closed");
            int firstHit = -1, firstClear = -1;
            bool openingSound = false;
            ushort cameraX = (ushort)(left ? 0 : level.WidthInBlocks * 16 - 256);
            ushort cameraY = (ushort)Math.Max(0, setup.Y - 128);
            for (int frame = 0; frame < 40; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                runtime.BombProjectiles.StepFrame(bus, level, samus, input, input);
                runtime.Projectiles.StepFrame(bus, level, samus, input, input, cameraX, cameraY, runtime.BombProjectiles, roomPlms: runtime.Plms);
                if (firstHit < 0 && runtime.Plms.PopulationSlots.Any(slot => slot.BlockIndex == setup.Target && slot.HeaderPointer == RoomPlmHeaders.BlueDoorFacingRight))
                    firstHit = frame;
                runtime.Plms.Step(bus, level, runtime.BackgroundStreamer!, cameraX, cameraY, 0);
                openingSound |= runtime.Plms.SoundRequests.Any(request => request == new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 7), 6));
                if (firstClear < 0 && Enumerable.Range(0, 4).All(row => level.GetCollisionBlockByIndex(setup.Target + row * level.WidthInBlocks).CollisionType == RoomCollisionType.Air))
                    firstClear = frame;
            }
            Console.WriteLine($"Retail wrap room={setup.Room:X4} beams={beams:X}: hit={firstHit}, clear={firstClear}, sound={openingSound}");
            bool succeeds = beams == setup.Beams || left && beams == 1;
            AssertEqual(succeeds ? setup.Hit : -1, firstHit, "Real remote-door activation and room-specific narrower/no-Wave cases");
            AssertEqual(succeeds ? setup.Hit + 18 : -1, firstClear, "Opening list clears all four cap cells after eighteen frames");
            AssertTrue(succeeds ? openingSound : !openingSound,
                "Remote opening completes all four cap cells and queues the cartridge sound only for a successful wrap shot");
        }
    }
}
