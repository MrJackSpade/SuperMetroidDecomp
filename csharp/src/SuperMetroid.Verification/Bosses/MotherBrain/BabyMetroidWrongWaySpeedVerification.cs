using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduces the Baby Metroid's off-screen reversal from issue #1269.</summary>
internal static class BabyMetroidWrongWaySpeedVerification
{
    /// <summary>
    /// Confirms the $A9:F466 movement legs apply a $10 off-screen wrong-way X extra
    /// before acceleration. At the recorded 100% movie position, native velocity
    /// changes from $015F to $0145 rather than slowing by only $0A.
    /// </summary>
    internal static void VerifyBabyMetroidWrongWaySpeed()
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)) };
        var motherBrain = MotherBrainLateFightFixtures.NewSequence();
        var baby = motherBrain.Baby;
        MotherBrainLateFightFixtures.Arrive(baby, FixedPosition<RoomPixels>.FromStoredParts(0, 0x1400, 0, 0x3100));
        global::SuperMetroid.Core.PrivateState.Set(baby, "Phase", (global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidCutscenePhase)(BabyMetroidCutscenePhase.MoveToFinalChargeStart));
        global::SuperMetroid.Core.PrivateState.Set(baby, "Position", (global::SuperMetroid.Core.Spatial.FixedPosition<global::SuperMetroid.Core.Spatial.RoomPixels>)(FixedPosition<RoomPixels>.FromWhole(new(0x0133, 0x004d)).WithFractionsOf(baby.Position)));
        baby.Velocity = new PixelVelocity<RoomPixels>(new SignedEightEight(0x015f), new SignedEightEight(0x012b));

        // Layer-1 X zero puts X $133 beyond the $120-wide vaguely-on-screen band.
        baby.Step(samus, HeadOf(motherBrain), Position16<RoomPixels>.Zero, enemyFrameCounter: 0, randomNumber: 0);
        AssertEqual((ushort)0x0145, baby.Velocity.X.RawValue,
            "the wrong-way off-screen reversal subtracts the $10 extra, $8 and two $1 steps");
        Console.WriteLine("Baby Metroid wrong-way speed: $A9:F466 legs apply the $10 off-screen extra.");
    }

    /// <summary>Runs the targeted off-screen reversal regression from the verification host.</summary>
    [VerificationCommand("--baby-metroid-wrong-way-speed")]
    internal static int BabyMetroidWrongWaySpeedCommand()
    {
        VerifyBabyMetroidWrongWaySpeed();
        return 0;
    }
}
