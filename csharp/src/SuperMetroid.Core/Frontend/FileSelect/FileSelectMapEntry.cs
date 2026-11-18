namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select map's entry (#1512): a wait on the black screen, its setup, and the centered area-map reveal.
/// </summary>
/// <remarks>
/// Nothing shows during the entry fade: the screen stays black for as many updates as the fade's steps take, and the area
/// map's own colours show from the reveal.
/// </remarks>
public sealed class FileSelectMapEntry
{
    /// <summary>
    /// The entry fade's step: the first once the map is entered, then one more each update, until it passes the fade's
    /// last step.
    /// </summary>
    private int fadeStep = 1;
    /// <summary>Number of eight-pixel expansion updates applied to the centered reveal.</summary>
    private int revealSteps;

    /// <summary>Gets the current entry stage.</summary>
    public FileSelectMapEntryPhase Phase { get; private set; } = FileSelectMapEntryPhase.EntryFade;

    /// <summary>Gets whether the reveal has finished and ordinary area-map input may begin.</summary>
    public bool IsComplete => Phase == FileSelectMapEntryPhase.Complete;

    /// <summary>Gets the inclusive left edge of the expanding reveal window in pixels.</summary>
    public int Left => FileSelectMapDefinitions.EntryWindowLeft - revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the inclusive right edge of the expanding reveal window in pixels.</summary>
    public int Right => FileSelectMapDefinitions.EntryWindowRight + revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the upper edge of the expanding reveal window in pixels.</summary>
    public int Top => FileSelectMapDefinitions.EntryWindowTop - revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the lower edge of the expanding reveal window in pixels.</summary>
    public int Bottom => ScreenGeometry.ScreenHeightPixels - Top;

    /// <summary>Advances the entry fade, setup, or centered reveal by one frontend update.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case FileSelectMapEntryPhase.EntryFade:
                // The fade reaches its colours one step past its last and finishes on the update after.
                if (fadeStep > FileSelectMapDefinitions.EntryFadeSteps + 1) Phase = FileSelectMapEntryPhase.LoadForeground;
                else fadeStep++;
                break;
            case FileSelectMapEntryPhase.LoadForeground:
                Phase = FileSelectMapEntryPhase.LoadBackground;
                break;
            case FileSelectMapEntryPhase.LoadBackground:
                Phase = FileSelectMapEntryPhase.SetupWindow;
                break;
            case FileSelectMapEntryPhase.SetupWindow:
                Phase = FileSelectMapEntryPhase.Revealing;
                break;
            case FileSelectMapEntryPhase.Revealing:
                // When the window's top would pass the screen's top this update, the window opens fully instead: its
                // edges stay where they are and the whole area shows.
                if (Top - FileSelectMapDefinitions.EntryWindowSpeed < 0)
                    Phase = FileSelectMapEntryPhase.Complete;
                else revealSteps++;
                break;
            case FileSelectMapEntryPhase.Complete:
                break;
            default:
                throw new InvalidOperationException($"Unknown file-select entry phase {Phase}.");
        }
    }
}
