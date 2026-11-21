namespace SuperMetroid.Core.Curves;

/// <summary>The marks along the speed curve.</summary>
internal static class CurveMarks
{

    /// <summary>The mark past <c>LastMark</c>, where the curve has no speed.</summary>
    internal static string PastLastMark => "past";

    /// <summary>The mark before <c>FirstMark</c>, where the curve starts.</summary>
    internal static string BeforeFirstMark => "before";
}