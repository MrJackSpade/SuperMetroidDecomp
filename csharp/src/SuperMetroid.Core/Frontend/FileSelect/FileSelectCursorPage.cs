namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The file-select page whose cursor anchors place the selection missile.</summary>
internal enum FileSelectCursorPage
{
    /// <summary>The main page.</summary>
    Main,

    /// <summary>A copy or clear slot page.</summary>
    DataManagement,

    /// <summary>A copy or clear Yes/No confirmation.</summary>
    Confirmation,
}
