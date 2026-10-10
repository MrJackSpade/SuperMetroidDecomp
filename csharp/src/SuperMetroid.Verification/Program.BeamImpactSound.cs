using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// A beam that strikes a wall queues library-two $0C ($93:80EF, Max6) as it becomes an
    /// explosion. The 13% movie's door wait depends on it: the busy library holds back the
    /// next door sound for one more $82:E29E dispatch.
    /// </summary>
    private static void VerifyBeamImpactSound()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        RoomLevelData level = runtime.LevelData!;
        // A floored corridor ending in a solid column at block 44, inside the settled view.
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 64; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 || x == 44 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.XPosition = 512;
        samus.YPosition = 490;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(400, 350);
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
                AssertTrue(runtime.Projectiles.ImpactSoundRequests.Contains(
                        new SamusSoundRequest(SoundEffectLibrary2Sounds.BeamImpact, 6)),
                    "the frame a beam hits the wall queues library-two $0C with Max6");
                Console.WriteLine("  Beam impact sound: a wall hit queues library-two $0C (Max6).");
                return;
            }
        }
        throw new InvalidOperationException("The beam never reached the wall at block 44.");
    }
}
