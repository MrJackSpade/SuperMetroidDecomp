namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Presentation geometry of Zebetite palette records.</summary>
public static class ZebetiteColorFormat
{
    /// <summary>Eight pulse rows selected by the shared counter's low three bits; these are palette steps rather than independent video-refresh timing data.</summary>
    public const int FrameCount = 8;
    /// <summary>Two adjacent barrier-core colors copied per pulse step to OBJ palette 2 colors 12 and 13.</summary>
    public const int ColorsPerFrame = 2;
}
