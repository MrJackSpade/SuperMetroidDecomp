namespace SuperMetroid.Core.Assets;

/// <summary>
/// The 34 bank-$A5 Draygon extended frames whose sole zero-offset component
/// points to a $FFFE BG2 tilemap stream. The remaining 60 selected frames are
/// ordinary OAM components in <see cref="EnemyExtendedFrameDefinitions"/>.
/// Hitbox pointers and instruction timing are not part of this visual catalog.
/// </summary>
internal static class DraygonBg2FrameDefinitions
{
    internal const int Version = 1;
    internal const string FileName = "draygon-bg2-frames.json";
    internal const byte Bank = 0xa5;
    /// <summary>Each selected Draygon BG2 frame has one zero-offset stream.</summary>
    internal const int MaximumComponents = 1;

    private static readonly EnemyBg2FrameDefinition[] FrameDefinitions =
    [
        new(0xa31b, "draygon_bg2_A31B"),
        new(0xa325, "draygon_bg2_A325"),
        new(0xa32f, "draygon_bg2_A32F"),
        new(0xa339, "draygon_bg2_A339"),
        new(0xa343, "draygon_bg2_A343"),
        new(0xa34d, "draygon_bg2_A34D"),
        new(0xa357, "draygon_bg2_A357"),
        new(0xa361, "draygon_bg2_A361"),
        new(0xa36b, "draygon_bg2_A36B"),
        new(0xa375, "draygon_bg2_A375"),
        new(0xa37f, "draygon_bg2_A37F"),
        new(0xa389, "draygon_bg2_A389"),
        new(0xa393, "draygon_bg2_A393"),
        new(0xa39d, "draygon_bg2_A39D"),
        new(0xa3a7, "draygon_bg2_A3A7"),
        new(0xa3b1, "draygon_bg2_A3B1"),
        new(0xa3bb, "draygon_bg2_A3BB"),
        new(0xa643, "draygon_bg2_A643"),
        new(0xa64d, "draygon_bg2_A64D"),
        new(0xa657, "draygon_bg2_A657"),
        new(0xa661, "draygon_bg2_A661"),
        new(0xa66b, "draygon_bg2_A66B"),
        new(0xa675, "draygon_bg2_A675"),
        new(0xa67f, "draygon_bg2_A67F"),
        new(0xa689, "draygon_bg2_A689"),
        new(0xa693, "draygon_bg2_A693"),
        new(0xa69d, "draygon_bg2_A69D"),
        new(0xa6a7, "draygon_bg2_A6A7"),
        new(0xa6b1, "draygon_bg2_A6B1"),
        new(0xa6bb, "draygon_bg2_A6BB"),
        new(0xa6c5, "draygon_bg2_A6C5"),
        new(0xa6cf, "draygon_bg2_A6CF"),
        new(0xa6d9, "draygon_bg2_A6D9"),
        new(0xa6e3, "draygon_bg2_A6E3"),
    ];

    internal static ReadOnlySpan<EnemyBg2FrameDefinition> Frames => FrameDefinitions;
}
