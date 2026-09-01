using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Proves the replay seed and every raw controller bit survive serialization.</summary>
    private static void VerifyControllerInputRecording()
    {
        byte[] digest = Enumerable.Range(0, 32).Select(index => (byte)index).ToArray();
        byte[] saveRam = new byte[SuperMetroidAddressSpace.SaveRamByteCount];
        for (int index = 0; index < saveRam.Length; index++)
            saveRam[index] = unchecked((byte)(index * 37));
        ushort[] inputs = [0, 0x0080, 0x0280, 0xffff, 0x1000];
        var expected = new ControllerInputRecording
        {
            StartedUtc = new DateTimeOffset(2026, 9, 1, 12, 34, 56, TimeSpan.Zero),
            RomSha256 = digest,
            InitialSaveRam = saveRam,
            GameOptions = new SuperMetroidGameOptions { SkipOpeningCinematic = true },
            ControllerInputs = inputs,
        };

        using var bytes = new MemoryStream();
        expected.Write(bytes);
        bytes.Position = 0;
        ControllerInputRecording actual = ControllerInputRecording.Read(bytes);

        AssertEqual(expected.StartedUtc, actual.StartedUtc, "input recording UTC seed");
        AssertTrue(actual.GameOptions.SkipOpeningCinematic, "input recording host option");
        AssertTrue(digest.SequenceEqual(actual.RomSha256), "input recording ROM digest");
        AssertTrue(saveRam.SequenceEqual(actual.InitialSaveRam), "input recording SRAM seed");
        AssertTrue(inputs.SequenceEqual(actual.ControllerInputs), "input recording frame words");

        // The frame count is an integrity boundary, not merely a preallocation hint. A
        // partially flushed file must fail loudly instead of replaying an incomplete tail.
        byte[] truncated = bytes.ToArray()[..^1];
        try
        {
            ControllerInputRecording.Read(new MemoryStream(truncated));
            throw new InvalidOperationException("Truncated input recording was accepted.");
        }
        catch (InvalidDataException)
        {
            // Expected strict-length rejection.
        }

        Console.WriteLine("  Controller input recording: deterministic seed, words, and truncation guard pass.");
    }
}
