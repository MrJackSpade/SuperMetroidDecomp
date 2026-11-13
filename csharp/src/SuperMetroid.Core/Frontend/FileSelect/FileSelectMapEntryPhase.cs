namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Entry coroutine stages before normal area-map input becomes active.</summary>
public enum FileSelectMapEntryPhase
{
    /// <summary>Fades the first two palettes to their installed file-select colors.</summary>
    PaletteFade,
    /// <summary>Represents the foreground-picture loading stage.</summary>
    LoadForeground,
    /// <summary>Represents the background-picture loading stage.</summary>
    LoadBackground,
    /// <summary>Configures the centered initial reveal window.</summary>
    SetupWindow,
    /// <summary>Expands the reveal window by eight pixels per update.</summary>
    Revealing,
    /// <summary>Displays the complete area scene and permits normal map input.</summary>
    Complete
}
