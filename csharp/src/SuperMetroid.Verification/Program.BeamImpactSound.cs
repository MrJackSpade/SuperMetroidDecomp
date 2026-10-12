using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class BeamImpactSoundVerification
{
    /// <summary>
    /// A beam that strikes a wall queues library-two $0C ($93:80EF, Max6) as it becomes an
    /// explosion. The 13% movie's door wait depends on it: the busy library holds back the
    /// next door sound for one more $82:E29E dispatch.
    /// </summary>
    internal static void VerifyBeamImpactSound()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        RoomLevelData level = runtime.Room.LevelData!;
        // A floored corridor ending in a solid column at block 44, inside the settled view.
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 64; x++)
        {
            int index = y * level.Size.X + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 || x == 44 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        SamusState samus = runtime.Player.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(512, 490));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        runtime.Room.Camera!.SetPosition(new Position16<RoomPixels>(400, 350));
        for (int frame = 0; frame < 64; frame++)
            runtime.StepFrame(0);

        runtime.StepFrame((ushort)SnesButton.X);
        AssertTrue(runtime.Projectiles!.Slots[0].Type != 0, "the shot is live");
        for (int frame = 0; frame < 120; frame++)
        {
            bool exploding = runtime.Projectiles.Slots[0].PackedType.IsFamily(SamusProjectileFamily.BeamExplosion);
            runtime.StepFrame(0);
            if (!exploding && runtime.Projectiles.Slots[0].PackedType.IsFamily(SamusProjectileFamily.BeamExplosion))
            {
                AssertTrue(runtime.Projectiles.ImpactAudio.ImpactSoundRequests.Contains(
                        new SamusSoundRequest(SoundEffectLibrary2Sounds.BeamImpact, 6)),
                    "the frame a beam hits the wall queues library-two $0C with Max6");
                Console.WriteLine("  Beam impact sound: a wall hit queues library-two $0C (Max6).");
                return;
            }
        }
        throw new InvalidOperationException("The beam never reached the wall at block 44.");
    }

    [VerificationCommand("--beam-impact-sound")]
    internal static int BeamImpactSoundCommand()
    {
        Suite(nameof(VerifyBeamImpactSound), () => VerifyBeamImpactSound());
        return 0;
    }
}
