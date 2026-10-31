namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-copy arrow placement, palette rotation and sheet frames; the arrows' compositions belong to the import pipeline (#1463).</summary>
public static class FileCopyArrowDefinitions
{
    /// <summary>Initial delay before the first arrow palette rotation.</summary>
    public const int InitialPaletteDelay = 8;
    /// <summary>Subsequent arrow palette rotation delay.</summary>
    internal const int PaletteDelay = 4;
    /// <summary>All file-copy arrows use screen X = 20.</summary>
    internal const int OriginX = 20;

    /// <summary>The file-copy arrow shapes: short and long, downward and upward.</summary>
    internal const int ShapeCount = 4;

    /// <summary>The sheet frame showing arrow shape <paramref name="shape"/>.</summary>
    public static string FrameName(int shape) => $"CopyArrow.{shape}";
}
