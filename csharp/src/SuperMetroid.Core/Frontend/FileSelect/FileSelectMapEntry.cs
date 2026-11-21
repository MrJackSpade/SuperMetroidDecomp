namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select map's entry (#1512, #2415): the black screen it holds for
/// <see cref="FileSelectMapDefinitions.EntryBlackHoldUpdates"/> updates, then the centered area-map reveal.
/// </summary>
public sealed partial class FileSelectMapEntry
{
    /// <summary>The entry's black hold, counted once each update; the reveal starts on its last.</summary>
    private readonly UpdateCountdown blackHold = UpdateCountdown.Lasting(FileSelectMapDefinitions.EntryBlackHoldUpdates);
    /// <summary>Number of expansion updates applied to the centered reveal, each edge moving at <see cref="FileSelectMapDefinitions.EntryWindowSpeed"/>.</summary>
    private int revealSteps;

    /// <summary>Gets the current entry stage.</summary>
    public FileSelectMapEntryPhase Phase { get; private set; } = FileSelectMapEntryPhase.BlackHold;

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

    /// <summary>Advances the entry's black hold or centered reveal by one frontend update.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case FileSelectMapEntryPhase.BlackHold:
                if (blackHold.StepEnds())
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
