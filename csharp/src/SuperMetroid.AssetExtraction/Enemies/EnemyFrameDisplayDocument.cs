namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed enemy display bindings (#1638): for each named frame an enemy shows, the frame displayed for it.</summary>
internal sealed record EnemyFrameDisplayDocument
{
    /// <summary>Document revision; loading requires <see cref="EnemyFrameDisplayFiles.Revision"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Each named ordinary frame's displayed frame, by name; never AI timing.</summary>
    public required Dictionary<string, string> DisplayFrames { get; init; }

    /// <summary>
    /// Each named multipart frame's displayed frame of the same enemy family, by name; never AI timing. A multipart frame
    /// places several parts, as a Space Pirate's or a boss's body does (#1712).
    /// </summary>
    public required Dictionary<string, string> MultipartDisplayFrames { get; init; }
}
