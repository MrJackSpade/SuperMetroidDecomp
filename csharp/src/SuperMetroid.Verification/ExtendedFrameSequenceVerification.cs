using static SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Artwork;

/// <summary>Checks the published enemy extended-frame roster and sequence slicing contract.</summary>
internal static class ExtendedFrameSequenceVerification
{
    /// <summary>Checks all 588 frame identities, their order, prefix and range slices, and bounds.</summary>
    internal static void VerifyExtendedFrameSequence()
    {
        var frames = EnemyExtendedFrameDefinitions.Frames.ToArray();
        string identity = string.Join("\n", frames.Select(frame => $"{frame.Bank:X2}:{frame.Pointer:X4}:{frame.Name}"));
        // Captured from the original cached catalog at 1f7c6f72 before replacing
        // its storage. UTF-8, uppercase bank/pointer, colon fields, LF separators,
        // no trailing LF. This oracle covers all published names and their order.
        AssertEqual("3C4FA3A572E604B69A4CE48F276BD1985E930C5EA3DFC239D8AD882D5CBAFB40",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(identity))), "original extended-frame roster");
        AssertEqual(588, frames.Length, "original extended-frame count");
        var sequence = EnemyExtendedFrameDefinitions.Frames;
        AssertEqual(frames.Length, sequence.Length, "sequence length");
        AssertTrue(frames.SequenceEqual(sequence), "ordered extended frames");
        foreach (int prefix in new[] { 0, 37, 55, 131, 142, 202, 230, 260, 269, 319,
            320, 321, 343, 349, 359, 370, 373, 379, 400, 401, 406, 408, 420, 447, 464, 520, 553, 588 })
        {
            AssertTrue(frames[..prefix].SequenceEqual(sequence[..prefix]), "legacy schema prefix");
            AssertTrue(frames[prefix..].SequenceEqual(sequence[prefix..]), "schema suffix");
        }
        AssertTrue(frames[447..464].SequenceEqual(sequence[420..520][27..44]), "nested range offsets");
        bool badRangeRejected = false;
        try { _ = sequence[..589]; }
        catch (ArgumentOutOfRangeException) { badRangeRejected = true; }
        AssertTrue(badRangeRejected, "invalid sequence range rejected");
        Console.WriteLine("Extended-frame sequence: original 588 entries, ordering, legacy prefixes, ranges and bounds pass.");
    }

    /// <summary>Runs the focused extended-frame roster check.</summary>
    [VerificationCommand("--lookup-extended-frame-sequence")]
    internal static int LookupExtendedFrameSequenceCommand()
    {
        Suite(nameof(VerifyExtendedFrameSequence), () => VerifyExtendedFrameSequence());
        return 0;
    }
}
