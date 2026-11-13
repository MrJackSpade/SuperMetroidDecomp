using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The cursor offsets the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The columns of an offset two across and three down.</summary>
    internal static int Columns => new CursorOffset(2, 3).Columns;
}
