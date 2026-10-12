using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents wrap shot rooms verification.</summary>
internal static class WrapShotRoomsVerification
{
    /// <summary>Verifies retail wrap shot doors.</summary>
    internal static void VerifyRetailWrapShotDoors()
    {
        foreach (var setup in new[] {
            (Room: RoomHeaderPointers.LandingSite, X: 2260, Y: 546, Target: 0x1561, Beams: 9, Hit: 9),
            (Room: RoomHeaderPointers.Crocomire, X: 2004, Y: 34, Target: 0x0301, Beams: 9, Hit: 9),
            (Room: RoomHeaderPointers.GreenBrinstarMainShaft, X: 38, Y: 1606, Target: 0x2981, Beams: 5, Hit: 5),
        })
        foreach (int beams in new[] { setup.Beams, 1, 0 })
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(setup.Room);
            var level = runtime.Room.LevelData!;
            bool left = setup.Room == RoomHeaderPointers.GreenBrinstarMainShaft;
            var samus = runtime.Player.Samus!;
            samus.Position = samus.Position.WithWhole(new((ushort)setup.X, (ushort)setup.Y));
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
            samus.PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose;
            samus.EquippedBeams = (ushort)beams;
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlockByIndex(setup.Target).CollisionType, "Retail remote cap begins closed");
            int firstHit = -1, firstClear = -1;
            bool openingSound = false;
            ushort cameraX = (ushort)(left ? 0 : level.Size.X * 16 - 256);
            ushort cameraY = (ushort)Math.Max(0, setup.Y - 128);
            for (int frame = 0; frame < 40; frame++)
            {
                ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                runtime.BombProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, input, input);
                runtime.Projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = level, Plms = runtime.Plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(cameraX, cameraY), runtime.BombProjectiles);
                if (firstHit < 0 && runtime.Plms.PopulationSlots.Any(slot => slot.BlockIndex == setup.Target && slot.HeaderPointer == PlmHeaderId.BlueDoorFacingRight))
                    firstHit = frame;
                runtime.Plms.Step(bus, level, runtime.Room.BackgroundStreamer!, new PlmView { Layer1Position = new(cameraX, cameraY), Bg1XOffset = 0 });
                openingSound |= runtime.Plms.Sounds.SoundRequests.Any(request => request == new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 7), 6));
                if (firstClear < 0 && Enumerable.Range(0, 4).All(row => level.GetCollisionBlockByIndex(setup.Target + row * level.Size.X).CollisionType == RoomCollisionType.Air))
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

    /// <summary>Wrap shot rooms command.</summary>
    [VerificationCommand("--wrap-shot-rooms")]
    internal static int WrapShotRoomsCommand()
    {
        Suite(nameof(VerifyRetailWrapShotDoors), () => VerifyRetailWrapShotDoors());
        return 0;
    }
}
