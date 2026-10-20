using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Baby Metroid's fatal-blow shake velocity and position against native $A9:CEDB behavior.</summary>
internal static class BabyMetroidFatalBlowShakeVerification
{
    /// <summary>
    /// Confirms each $A9:CEDB call subtracts one from Baby Y velocity and applies undoubled
    /// shake offsets around its saved origin. Issue #1269 exposed doubled offsets and a
    /// stationary Y fraction in the 100% movie.
    /// </summary>
    internal static void VerifyBabyMetroidFatalBlowShake()
    {
        var samus = new SamusState { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)) };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.Zero);
        baby.Phase = BabyMetroidCutscenePhase.FinalCharge;
        baby.Position = FixedPosition<RoomPixels>.FromWhole(new(0x00c8, 0x0057)).WithFractionsOf(baby.Position);
        baby.Health = (ushort)0;
        var head = new Position16<RoomPixels>(0x00c8, 0x0077);

        // Frame-counter index 0: zero offsets around the origin; velocity zeroed then decremented.
        baby.Step(samus, motherBrain, head, enemyFrameCounter: 0);
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, baby.Phase, "zero health starts the fatal blow");
        AssertEqual((ushort)0xffff, baby.Velocity.Y.RawValue, "the first shake call decrements the cleared Y velocity");

        // Index 1: offsets (-1,+1), undoubled.
        baby.Step(samus, motherBrain, head, enemyFrameCounter: 2);
        AssertEqual((ushort)0xfffe, baby.Velocity.Y.RawValue, "every shake call decrements the Y velocity");
        AssertEqual((ushort)0x00c7, baby.Position.X.Whole, "the shake adds the undoubled X offset");
        Console.WriteLine("Baby Metroid fatal-blow shake: $A9:CEDB decrements Y velocity and adds undoubled offsets.");
    }

    /// <summary>Runs the focused fatal-blow shake check from the verification command line.</summary>
    [VerificationCommand("--baby-metroid-fatal-blow-shake")]
    internal static int BabyMetroidFatalBlowShakeCommand()
    {
        VerifyBabyMetroidFatalBlowShake();
        return 0;
    }
}
