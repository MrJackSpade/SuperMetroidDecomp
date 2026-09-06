using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop;

/// <summary>Reproduces a short pad press inside a slow host catch-up batch.</summary>
public static class PlaybackFrameBatchSmokeTest
{
    public static void Run()
    {
        // Each simulated game/audio frame takes 20 ms. A physical tap starts at
        // 10 ms and ends at 30 ms, so the poll at frame two must see it. Sampling
        // only at batch start loses the entire tap despite three game updates.
        int time = 0;
        var observed = new List<ushort>();
        var result = PlaybackFrameBatch.Run(3,
            () => time >= 10 && time < 30 ? (ushort)SnesButton.Left : (ushort)0,
            input =>
            {
                observed.Add(input);
                time += 20;
                return default(FrontendFrame);
            });
        ushort[] expected = [0, (ushort)SnesButton.Left, 0];
        if (!observed.SequenceEqual(expected))
            throw new InvalidDataException(
                $"Catch-up batch lost or stretched a short tap: [{string.Join(',', observed)}], " +
                $"expected [{string.Join(',', expected)}].");
        if (result.CompletedFrames != 3 || result.LastFrame is null)
            throw new InvalidDataException("Batch lost completed-frame accounting or presentation.");

        // Exhausted replay stops the batch; absent frame work must not poll at all.
        int polls = 0, calls = 0;
        result = PlaybackFrameBatch.Run(4, () => { polls++; return 0; },
            _ => ++calls == 2 ? null : default(FrontendFrame));
        if (result.CompletedFrames != 1 || calls != 2 || polls != 2 || result.LastFrame is null)
            throw new InvalidDataException("Batch did not stop at the first exhausted replay frame.");
        result = PlaybackFrameBatch.Run(0,
            () => throw new InvalidDataException("An empty batch polled input."),
            _ => throw new InvalidDataException("An empty batch advanced gameplay."));
        if (result.CompletedFrames != 0 || result.LastFrame is not null)
            throw new InvalidDataException("Empty batch produced a frame.");
    }
}
