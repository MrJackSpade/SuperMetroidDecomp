using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>
/// Checks the ending's wavy Samus wave (#1627): it starts one step short of a whole turn, an angle on the turn rather than a
/// minus-one marker, and each frame from its first turns it on by the frame's step round the turn.
/// </summary>
internal static class EndingWavySamusWaveVerification
{
    /// <summary>Runs the wave phase checks over a whole turn and beyond.</summary>
    internal static void VerifyEndingWavySamusWave()
    {
        AssertEqual(new TrigonometryAngle(255), EndingWavySamusDefinitions.InitialAngle, "the wave starts one step short of a whole turn");
        AssertTrue(EndingWavySamus.WaveAtAge(EndingWavySamusDefinitions.FirstWaveAge - 1) is null, "the setup frame shows no wave");
        for (int age = EndingWavySamusDefinitions.FirstWaveAge; age < EndingWavySamusDefinitions.FirstWaveAge + 70; age++)
        {
            ScrollWave wave = EndingWavySamus.WaveAtAge(age) ?? throw new InvalidOperationException($"Verification failed: age {age} shows no wave.");
            int expected = (255 + EndingWavySamusDefinitions.AngleStepPerFrame * (age - 1)) % 256;
            AssertEqual(new TrigonometryAngle(expected), wave.Phase, $"age {age}: the wave's phase turns on one frame's step round the turn");
        }
        Console.WriteLine("Ending wavy Samus: the wave's phase starts short of a whole turn and turns round it frame by frame.");
    }
}
