using SuperMetroid.Core.Bosses;

namespace SuperMetroid.Verification.Bosses;

/// <summary>Checks the fall restart of Kraid's ceiling rocks and Crocomire's bridge fragments.</summary>
internal static class RestartingFallSpeedVerification
{
    /// <summary>
    /// A fall gains its gravity each update, and a speed that reaches 64 pixels an update starts again from rest, keeping
    /// what lies past it, exactly as keeping the speed's lower fourteen bits does; an upward speed or gravity is rejected.
    /// </summary>
    internal static void VerifyRestartingFallSpeed()
    {
        SignedEightEight gravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);
        foreach (int start in new[] { 64, 127, 16000, 16359, 16360, 16383 })
        {
            SignedEightEight next = RestartingFallSpeed.Accelerate(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(start), gravity);
            AssertEqual((start + 24) & 16383, next.InTwoHundredFiftySixthsOfPixel, $"fall speed {start} plus gravity 24");
        }
        AssertEqual(0, RestartingFallSpeed.Accelerate(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16360), gravity).InTwoHundredFiftySixthsOfPixel,
            "a fall reaching 64 pixels an update starts again from rest");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RestartingFallSpeed.Accelerate(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1), gravity), "an upward fall speed is rejected");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RestartingFallSpeed.Accelerate(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-24)),
            "an upward gravity is rejected");
    }
}
