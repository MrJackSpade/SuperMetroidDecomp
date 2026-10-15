namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Selects which file-management flow receives the current file-selection input.</summary>
internal enum FileSelectDataMode
{
    /// <summary>Copies an occupied save slot into a chosen destination slot.</summary>
    Copy,
    /// <summary>Clears the chosen save slot after its confirmation flow.</summary>
    Clear,
}
