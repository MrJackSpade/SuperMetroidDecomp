namespace SuperMetroid.Core.Curves;

/// <summary>The marks along the speed curve.</summary>
internal static class CurveMarks
{
    /// <summary>The curve's last mark, which nothing reads.</summary>
    internal static string LastMark => Last;

    /// <summary>The curve's first mark, which only checks read.</summary>
    internal static string FirstMark => "first";

    /// <summary>The mark past <see cref="LastMark"/>, where the curve has no speed.</summary>
    internal static string PastLastMark => "past";

    /// <summary>The mark before <see cref="FirstMark"/>, where the curve starts.</summary>
    internal static string BeforeFirstMark => "before";

    /// <summary>The last mark's number, which only <see cref="LastMark"/> reads.</summary>
    private static string Last => "last";
}