namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>How far a list's cursor is drawn from its place.</summary>
internal sealed class CursorOffset
{
    /// <summary>The columns the cursor is drawn across from its place.</summary>
    private readonly int _xOffset;

    /// <summary>The rows the cursor is drawn down from its place.</summary>
    private readonly int yOffset;

    /// <summary>An offset of <paramref name="columns"/> across and <paramref name="rows"/> down.</summary>
    /// <param name="columns">The columns across.</param>
    /// <param name="rows">The rows down.</param>
    internal CursorOffset(int columns, int rows)
    {
        _xOffset = columns;
        yOffset = rows;
    }

    /// <summary>The columns and rows together, which production reads.</summary>
    internal int Total => _xOffset + yOffset;

    /// <summary>The columns alone, which only checks read.</summary>
    internal int Columns => this._xOffset;
}
