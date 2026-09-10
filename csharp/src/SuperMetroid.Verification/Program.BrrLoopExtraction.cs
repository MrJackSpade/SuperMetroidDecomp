using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    private static void VerifyBrrLoopExtractionRetainsPredictorHistory()
    {
        var ram = new byte[65536];
        var written = new bool[65536];
        Array.Fill(written, true, 0, 9);
        // One looping BRR block: shift one, predictor one, constant positive nibble.
        // The independent recurrence is y[n] = 7 + floor(15*y[n-1]/16), y[-1]=0.
        // Repeating only the first sixteen decoded samples restarts the rising edge
        // on every loop instead of converging to the native stable value of 97.
        ram[0] = 0x17;
        Array.Fill(ram, (byte)0x77, 1, 8);
        AssertTrue(SpcAudioAssetExtractor.TryDecodeBrr(ram, written, 0, 0,
            out short[] pcm, out int blocks, out int? loop), "filtered BRR loop extracts");
        AssertEqual(1, blocks, "manifest counts original BRR blocks, not unfolded passes");
        AssertTrue(pcm.Length > 16 && loop is > 0, "filtered transient remains before recurring PCM loop");
        int previous = 0;
        for (int index = 0; index < 1024; index++)
        {
            int expected = 7 + 15 * previous / 16;
            int source = index < pcm.Length ? index : loop!.Value + (index - loop.Value) % (pcm.Length - loop.Value);
            AssertEqual(expected, pcm[source], $"filtered loop sample {index} retains predictor history");
            previous = expected;
        }
        AssertEqual(97, previous, "synthetic predictor settles to exact integer fixed point");

        ram[0] = 0x13; // Filter zero ignores incoming history, so no transient extension.
        AssertTrue(SpcAudioAssetExtractor.TryDecodeBrr(ram, written, 0, 0,
            out pcm, out blocks, out loop), "history-independent BRR loop extracts");
        AssertEqual(16, pcm.Length, "filter-zero loop does not need duplicate passes");
        AssertEqual(0, loop!.Value, "filter-zero loop keeps original loop start");

        ram[0] = 0x11; // Non-looping end marker.
        AssertTrue(SpcAudioAssetExtractor.TryDecodeBrr(ram, written, 0, 0,
            out pcm, out blocks, out loop), "one-shot BRR extracts");
        AssertEqual(16, pcm.Length, "one-shot length unchanged");
        AssertTrue(loop is null, "one-shot remains non-looping");

        ram[0] = 0x17;
        AssertTrue(!SpcAudioAssetExtractor.TryDecodeBrr(ram, written, 0, 1,
            out _, out _, out _), "loop address must identify a decoded BRR block boundary");
    }
}
