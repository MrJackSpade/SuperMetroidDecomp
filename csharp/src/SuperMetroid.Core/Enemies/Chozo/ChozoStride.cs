namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>The Chozo statue's stride: where its support foot is planted in each phase.</summary>
internal static class ChozoStride
{
    /// <summary>Gets the support foot's left edge, relative to the statue's origin, for one of the four stride phases.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside zero through three.</exception>
    internal static int SupportFootX(int phase) =>
        (uint)phase < (uint)ChozoStrideDefinitions.SupportFootXs.Length
            ? ChozoStrideDefinitions.SupportFootXs[phase]
            : throw new ArgumentOutOfRangeException(nameof(phase));
}
