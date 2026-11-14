namespace SuperMetroid.Core.Lists;

/// <summary>A list's shape on screen.</summary>
public abstract class ListShape
{
    /// <summary>The columns the shape takes, which production reads only through a row: the row's override needs it.</summary>
    public abstract int Width { get; }
}
