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

        // The glowing plane's vertical scroll is a displacement in the plane's space, which its offsets hold (#1627).
        var plane = new PicturedPlane([]);
        for (int age = 0; age <= EndingSequenceDefinitions.Timing.WaitingBackdropFrames; age++)
            AssertEqual(EndingWavySamusDefinitions.VerticalStep * age,
                new AddedPicturedPlaneLayer(plane, VerticalScroll: EndingWavySamus.VerticalScrollAtAge(age)).VerticalScroll,
                $"age {age}: the plane keeps the wait's scroll");
        AssertEqual((int)short.MaxValue, new AddedPicturedPlaneLayer(plane, VerticalScroll: short.MaxValue).VerticalScroll,
            "the plane keeps its farthest line below its origin");
        AssertEqual((int)short.MinValue, new AddedPicturedPlaneLayer(plane, VerticalScroll: short.MinValue).VerticalScroll,
            "the plane keeps its farthest line above its origin");
        AssertThrows<OverflowException>(() => _ = new AddedPicturedPlaneLayer(plane, VerticalScroll: short.MaxValue + 1),
            "a scroll past the plane's space names no plane line");
        AssertThrows<OverflowException>(() => _ = new AddedPicturedPlaneLayer(plane, VerticalScroll: 0) with { VerticalScroll = short.MinValue - 1 },
            "a later scroll past the plane's space names no plane line either");
        Console.WriteLine("Ending wavy Samus: the wave's phase starts short of a whole turn and turns round it frame by frame, and the plane keeps every scroll the wait reaches.");
    }
}
