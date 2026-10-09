using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Drained-Samus controller zero ($91:E4F8) changes only the pose; the movement-handler word
    /// keeps whatever owns it. A spark crash ($90:D3F3) therefore keeps counting and finishes
    /// ($90:D40D), standing Samus up. In the 13% movie Baby Metroid finishes draining Samus
    /// during her crash and native releases her when the crash timer runs out, sending its echoes
    /// at angles read past the end of the crash-pose table.
    /// </summary>
    private static void VerifyDrainedSparkCrash()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // Two calls left in the echo hold after the echoes finish circling Samus.
        PrivateState.SetProperty(samus.Shinespark, nameof(SamusShinesparkState.Phase), ShinesparkPhase.CrashEchoCircle);
        PrivateState.SetProperty(samus.Shinespark, nameof(SamusShinesparkState.StartStopTimer), (ushort)2);
        samus.Drained.LetFall(bus, samus);
        AssertEqual(SamusPoseIds.DrainedCrouchingLeftPose, samus.Pose, "controller zero installs the drained pose");

        for (int frame = 0; frame < 3; frame++)
            runtime.StepFrame(0);

        AssertEqual(ShinesparkPhase.Inactive, samus.Shinespark.Phase, "the crash runs out and finishes");
        AssertEqual(SamusPoseIds.FacingLeftNormalPose, samus.Pose, "the finished crash stands Samus up");
        AssertEqual(DrainedSamusPhase.Inactive, samus.Drained.Phase, "normal movement replaces the drained pose's handler");
        // $90:D482 indexes its angle table with the drained pose, reading $90:D506/$D507.
        AssertEqual((ushort)0xb6, runtime.Projectiles!.Slots[3].Variable, "the first echo takes the overread angle $B6");
        AssertEqual((ushort)0x0a, runtime.Projectiles.Slots[4].Variable, "the second echo takes the overread angle $0A");
        Console.WriteLine("  Drained spark crash: the crash keeps its handler through the drain and stands Samus up.");
    }
}
