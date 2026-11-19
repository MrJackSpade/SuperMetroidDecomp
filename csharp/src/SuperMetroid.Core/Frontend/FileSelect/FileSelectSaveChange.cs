namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What one update of the file-select menu did to the saves (#2562), which its step returns.</summary>
public enum FileSelectSaveChange
{
    /// <summary>The update changed no save.</summary>
    None,
    /// <summary>Copy copied a save into another slot.</summary>
    Copied,
    /// <summary>Clear cleared a save, which starts a new file and clears every stored explored map.</summary>
    Cleared,
}
