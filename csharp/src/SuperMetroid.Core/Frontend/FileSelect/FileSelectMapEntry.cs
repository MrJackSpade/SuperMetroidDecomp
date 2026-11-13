namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select map's entry (#1512): the wait the game spends fading two colours of the still-black screen, its setup
/// coroutine, and the centered area-map reveal.
/// </summary>
/// <remarks>
/// The game fades two background colours toward black with its gradual colour change while the screen shows only its
/// black backdrop; the faded colours are never shown, and the area map's own colours replace them before the reveal. What
/// the entry keeps is the fade's length: the number of colour-change steps it takes before the setup runs.
/// </remarks>
public sealed class FileSelectMapEntry
{
    /// <summary>
    /// The gradual colour change's step number: one after the setup call the game makes, then one more per fade update,
    /// until it passes the entry fade's last step.
    /// </summary>
    private int fadeStep = 1;
    /// <summary>Number of eight-pixel expansion updates applied to the centered reveal.</summary>
    private int revealSteps;

    /// <summary>Gets the current entry-coroutine stage.</summary>
    public FileSelectMapEntryPhase Phase { get; private set; } = FileSelectMapEntryPhase.PaletteFade;

    /// <summary>Gets whether the reveal has finished and ordinary area-map input may begin.</summary>
    public bool IsComplete => Phase == FileSelectMapEntryPhase.Complete;

    /// <summary>Gets the inclusive left edge of the expanding reveal window in pixels.</summary>
    public int Left => FileSelectMapDefinitions.EntryWindowLeft - revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the inclusive right edge of the expanding reveal window in pixels.</summary>
    public int Right => FileSelectMapDefinitions.EntryWindowRight + revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the upper edge of the expanding reveal window in pixels.</summary>
    public int Top => FileSelectMapDefinitions.EntryWindowTop - revealSteps * FileSelectMapDefinitions.EntryWindowSpeed;

    /// <summary>Gets the lower edge of the expanding reveal window in pixels.</summary>
    public int Bottom => FrontendFrame.Height - Top;

    /// <summary>Advances the entry fade, setup coroutine, or centered reveal by one frontend update.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case FileSelectMapEntryPhase.PaletteFade:
                // The gradual colour change reaches its target at step denominator plus one and reports completion on
                // the call after it.
                if (fadeStep > FileSelectMapDefinitions.EntryPaletteDenominator + 1) Phase = FileSelectMapEntryPhase.LoadForeground;
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
                // The game returns before updating edges on the call that takes the edge past zero,
                // disables the mask, and displays the whole area scene on that frame.
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
