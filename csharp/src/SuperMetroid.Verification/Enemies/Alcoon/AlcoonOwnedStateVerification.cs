using SuperMetroid.Core.Enemies.Alcoon;
namespace SuperMetroid.Verification.Enemies.Alcoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Alcoon state the species owns: its whole-pixel horizontal velocity.</summary>
internal static class AlcoonOwnedStateVerification
{
    /// <summary>Confirms a horizontal velocity with a fraction is rejected (Alcoon has none).</summary>
    internal static void VerifyAlcoonOwnedState()
    {
        var state = new AlcoonEnemyState
        {
            Action = AlcoonAction.WalkingAndFiring,
            Velocity = new(SignedSixteenSixteen.FromParts(-2, 0), SignedSixteenSixteen.FromParts(-4, 0x8000)),
            SpawnPosition = new(0x0300, 0x0180),
            LandingPosition = new(0x0300, 0x01a0),
            YAcceleration = SignedSixteenSixteen.FromParts(0, 0x8000),
            StepCounter = 2,
        };
        AssertThrows<InvalidOperationException>(
            () => state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(1, 1)),
            "Alcoon rejects a horizontal velocity fraction");
        Console.WriteLine("Alcoon owned state: the horizontal velocity rule matches.");
    }

    /// <summary>Runs the focused Alcoon owned-state check.</summary>
    [VerificationCommand("--alcoon-owned-state")]
    internal static int AlcoonOwnedStateCommand()
    {
        VerifyAlcoonOwnedState();
        return 0;
    }
}
