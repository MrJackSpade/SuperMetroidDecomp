namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-copy arrow placement, palette rotation and sheet frames; the arrows' compositions belong to the import pipeline (#1463).</summary>
public static class FileCopyArrowDefinitions
{
    /// <summary>Initial delay before the first arrow palette rotation.</summary>
    public const int InitialPaletteDelay = 8;
    /// <summary>Subsequent arrow palette rotation delay.</summary>
    internal const int PaletteDelay = 4;
    /// <summary>
    /// The colours the arrows' rotation cycles through, so the steps it takes before the arrows show their first colouring
    /// again (#1512).
    /// </summary>
    public const int RotationSteps = 7;
    /// <summary>All file-copy arrows use screen X = 20.</summary>
    internal const int OriginX = 20;
    /// <summary>The screen Y of a short arrow between slots A and B.</summary>
    internal const int UpperShortArrowY = 88;
    /// <summary>The screen Y of a short arrow between slots B and C.</summary>
    internal const int LowerShortArrowY = 120;
    /// <summary>The screen Y of a long arrow between slots A and C.</summary>
    internal const int LongArrowY = 104;

    /// <summary>Every arrow shape the file-select sheet pictures.</summary>
    public static IReadOnlyList<FileCopyArrowShape> Shapes { get; } =
        [FileCopyArrowShape.ShortDown, FileCopyArrowShape.ShortUp, FileCopyArrowShape.LongDown, FileCopyArrowShape.LongUp];

    /// <summary>The sheet frame showing arrow shape <paramref name="shape"/>.</summary>
    public static string FrameName(FileCopyArrowShape shape) => $"CopyArrow.{shape}";
}
