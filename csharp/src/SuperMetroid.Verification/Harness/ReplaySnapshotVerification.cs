using System.Text;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Confirms the movie start-state reader accepts the SRAM block sizes Snes9x builds write.</summary>
internal static class ReplaySnapshotVerification
{
    /// <summary>
    /// Snes9x builds freeze the <c>SRA</c> block as 128 KiB or 512 KiB, and the pinned core's
    /// unfreeze reads either. Both must parse with their contents intact; any other size is refused.
    /// </summary>
    internal static void VerifyReplaySnapshotSaveRamSizes()
    {
        foreach (int length in new[] { 128 * 1024, 512 * 1024 })
        {
            byte[] saveRam = new byte[length];
            for (int index = 0; index < length; index++)
                saveRam[index] = (byte)(index * 7 + 3);
            ReplaySnapshot snapshot = ReplaySnapshot.Parse(Snapshot(("NAM", new byte[4]), ("SRA", saveRam), ("JXQ", new byte[8])));
            AssertSequenceEqual(saveRam, snapshot.SaveRam, $"{length}-byte SRA block");
        }
        AssertThrows<InvalidDataException>(() => ReplaySnapshot.Parse(Snapshot(("SRA", new byte[64 * 1024]))),
            "an SRA block of an unwritten size is refused");
        AssertThrows<InvalidDataException>(() => ReplaySnapshot.Parse(Snapshot(("NAM", new byte[4]))),
            "a snapshot without an SRA block is refused");
    }

    /// <summary>Builds a decompressed version-11 snapshot from named blocks.</summary>
    private static byte[] Snapshot(params (string Name, byte[] Data)[] blocks)
    {
        using var image = new MemoryStream();
        image.Write("#!s9xsnp:0011\n"u8);
        foreach (var (name, data) in blocks)
        {
            image.Write(Encoding.ASCII.GetBytes($"{name}:{data.Length:D6}:"));
            image.Write(data);
        }
        return image.ToArray();
    }
}
