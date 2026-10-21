namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The scanlines Draygon's body occupies on the main screen.</summary>
/// <param name="First">The first scanline shown.</param>
/// <param name="End">The scanline after the last one shown.</param>
public readonly record struct DraygonBodyScanlines(int First, int End);
