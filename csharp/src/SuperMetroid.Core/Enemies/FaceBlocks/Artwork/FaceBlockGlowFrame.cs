namespace SuperMetroid.Core.Enemies.FaceBlocks.Artwork;

/// <summary>
/// One frame of the room-wide glow the Blue Brinstar face blocks' eyes cycle through (#2369): each shows in turn, the
/// first following the last.
/// </summary>
internal readonly record struct FaceBlockGlowFrame
{
    /// <summary>The frame at <paramref name="frame"/> of the glow.</summary>
    private FaceBlockGlowFrame(int frame) => Frame = frame;

    /// <summary>The frame's place in the glow, from zero: the step of its installed shade.</summary>
    internal int Frame { get; }

    /// <summary>Every frame of the glow, in cycle order.</summary>
    internal static IReadOnlyList<FaceBlockGlowFrame> All { get; } =
        [.. Enumerable.Range(0, FaceBlockGlowDefinitions.GlowFrames).Select(frame => new FaceBlockGlowFrame(frame))];

    /// <summary>The glow's last frame, which a room's glow starts at so that its first step shows the first.</summary>
    internal static FaceBlockGlowFrame Last => All[^1];

    /// <summary>The frame the glow shows after this one, the first following the last.</summary>
    internal FaceBlockGlowFrame Next => All[(Frame + 1) % All.Count];
}
