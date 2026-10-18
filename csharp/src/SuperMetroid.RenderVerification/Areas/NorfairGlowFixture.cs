namespace SuperMetroid.RenderVerification.Areas;

/// <summary>Cathedral room and CGRAM destinations used to isolate Norfair's animated foreground glow.</summary>
internal static class NorfairGlowFixture
{
    /// <summary>CGRAM word destinations written by $8D:F08E/F1D1/F2D9/F3E1, including inline skips.</summary>
    internal static ReadOnlySpan<int> ColorIndices =>
        [53, 54, 55, 60, 61, 65, 66, 67, 76, 77, 81, 82, 83, 92, 93, 97, 98, 99, 108, 109];
}
