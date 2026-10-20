namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>One of the file-select menu's foreground pages.</summary>
public enum FileSelectPage
{
    /// <summary>The main menu with at least one save.</summary>
    MainWithData,
    /// <summary>The main menu with no saves.</summary>
    MainEmpty,
    /// <summary>Copy mode, choosing the source slot.</summary>
    CopySource,
    /// <summary>Copy mode, choosing the destination slot.</summary>
    CopyDestination,
    /// <summary>Copy mode, confirming the copy.</summary>
    CopyConfirm,
    /// <summary>Copy mode, after the copy.</summary>
    CopyCompleted,
    /// <summary>Clear mode, choosing the slot.</summary>
    ClearSelection,
    /// <summary>Clear mode, confirming the clear.</summary>
    ClearConfirm,
    /// <summary>Clear mode, after the clear.</summary>
    ClearCompleted,
}
