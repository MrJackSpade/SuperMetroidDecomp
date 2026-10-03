namespace SuperMetroid.Core.Frontend;

/// <summary>Bounded file-select helmet frame identities and native cadence.</summary>
internal static class FileSelectHelmetAnimation
{
    /// <summary>Eight distinct images; native $81:9DFD compares the incremented frame with eight.</summary>
    internal const int FrameCount = 8;
    /// <summary>$81:9DF3 reloads the frame timer with eight ticks.</summary>
    internal const int FrameDuration = 8;
    /// <summary>$81:9E2C contains eight consecutive IDs and one repeated terminal ID.</summary>
    internal const int NativeEntryCount = FrameCount + 1;

    /// <summary>Calculates the complete nine-word mapping at $81:9E2C..9E3D.</summary>
    /// <remarks>
    /// Frame indices 0..7 select consecutive spritemaps $2C..33. Entry 8 repeats
    /// the final image; native $81:9E08 clamps the active frame to 7 before drawing.
    /// The extractor and runtime share this calculation. No generated table is stored.
    /// The ninth native entry remains representable for exact bounded source parity;
    /// other indices reject before arithmetic, without narrowing or wrapping.
    /// </remarks>
    internal static ushort SpritemapId(int frame)
    {
        if ((uint)frame >= NativeEntryCount) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(0x2c + Math.Min(frame, FrameCount - 1));
    }
}
