using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Audio;

/// <summary>Checks remote blue-door caps hit by wrapped shots in three retail rooms.</summary>
internal static class WrapShotRoomsVerification
{
    /// <summary>Confirms room-specific beam reach, hit frame, four-cell clear, and opening sound.</summary>
    internal static void VerifyRetailWrapShotDoors()
    {
        foreach (var setup in new[] {
            (Room: RoomHeaderDefinitions.LandingSite, X: 2260, Y: 546, Target: 0x1561, Beams: 9, Hit: 9),
            (Room: RoomHeaderDefinitions.Crocomire, X: 2004, Y: 34, Target: 0x0301, Beams: 9, Hit: 9),
            (Room: RoomHeaderDefinitions.GreenBrinstarMainShaft, X: 38, Y: 1606, Target: 0x2981, Beams: 5, Hit: 5),
        })
        foreach (int beams in new[] { setup.Beams, 1, 0 })
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(setup.Room);
            var level = runtime.Room.LoadedLevelData;
            bool left = setup.Room == RoomHeaderDefinitions.GreenBrinstarMainShaft;
            var samus = runtime.Player.IntroducedSamus;
            samus.Position = samus.Position.WithWhole(new((ushort)setup.X, (ushort)setup.Y));
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
            samus.PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose;
            samus.EquippedBeams = (SamusBeamFlags)((ushort)beams);
            AssertEqual(RoomCollisionType.ShootableBlock, level.GetCollisionBlockByIndex(setup.Target).CollisionType, "Retail remote cap begins closed");
            int firstHit = -1, firstClear = -1;
            bool openingSound = false;
            ushort cameraX = (ushort)(left ? 0 : level.Size.X * 16 - 256);
            ushort cameraY = (ushort)Math.Max(0, setup.Y - 128);
            for (int frame = 0; frame < 40; frame++)
            {
                SnesButton input = frame == 0 ? SnesButton.X : 0;
                runtime.BombProjectiles.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
                runtime.Projectiles.StepFrame(new SamusTerrain { Level = level, Plms = runtime.Plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new(cameraX, cameraY), runtime.BombProjectiles);
                if (firstHit < 0 && runtime.Plms.PopulationSlots.Any(slot => slot.BlockIndex == setup.Target && slot.HeaderPointer == PlmHeaderId.BlueDoorFacingRight))
                    firstHit = frame;
                runtime.Plms.Step(level);
                openingSound |= runtime.Plms.Sounds.SoundRequests.Any(request => request == new SoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 7), 6));
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

    /// <summary>Runs the focused retail remote-door shot check.</summary>
    [VerificationCommand("--wrap-shot-rooms")]
    internal static int WrapShotRoomsCommand()
    {
        Suite(nameof(VerifyRetailWrapShotDoors), () => VerifyRetailWrapShotDoors());
        return 0;
    }
}
