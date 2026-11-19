namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>How the file-select area map hands off to its host (#2562), which it keeps once it reaches it.</summary>
public enum FileSelectMapOutcome
{
    /// <summary>The map runs: it has handed off to nothing yet.</summary>
    Browsing,
    /// <summary>After the confirmation, two prelude updates, fifteen brightness steps and thirty-two black updates, the host loads gameplay; the navigation phase alone does not authorize loading it.</summary>
    Load,
    /// <summary>Once its cancel fade reaches zero brightness, the host replaces the map with the game options; the map creates no options state.</summary>
    Options,
}
