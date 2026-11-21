namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// How a sample's last block ends (#2563): the end of the sample, and whether it loops back. The two are composable, as the
/// BRR block header's END and LOOP marks are, and a block with neither mark is not the sample's last.
/// </summary>
[Flags]
internal enum BrrBlockEnd
{
    /// <summary>The block is not the sample's last.</summary>
    None = 0,

    /// <summary>The block ends the sample.</summary>
    End = 1 << 0,

    /// <summary>The sample loops back to its loop point after the block.</summary>
    Loop = 1 << 1,
}
