using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Initial palette preparation and centered area-map reveal from $81:A32A-$81:A725.</summary>
public sealed class FileSelectMapEntry
{
    /// <summary>Native gradual palette fade toward installed file-select colors.</summary>
    private readonly CartridgePaletteTransition palette;
    /// <summary>Number of eight-pixel expansion updates applied to the centered reveal.</summary>
    private int revealSteps;

    /// <summary>Gets the live 256 colors the entry palette fade writes (#1358).</summary>
    public ScreenPalette Palette { get; } = new();

    /// <summary>Gets the current native entry-coroutine stage.</summary>
    public FileSelectMapEntryPhase Phase { get; private set; } = FileSelectMapEntryPhase.PaletteFade;

    /// <summary>Gets whether the reveal has finished and ordinary area-map input may begin.</summary>
    public bool IsComplete => Phase == FileSelectMapEntryPhase.Complete;

    /// <summary>Gets the inclusive left edge of the expanding reveal window in pixels.</summary>
    public int Left => FileSelectMapRomData.EntryWindowLeft - revealSteps * FileSelectMapRomData.EntryWindowSpeed;

    /// <summary>Gets the inclusive right edge of the expanding reveal window in pixels.</summary>
    public int Right => FileSelectMapRomData.EntryWindowRight + revealSteps * FileSelectMapRomData.EntryWindowSpeed;

    /// <summary>Gets the upper edge of the expanding reveal window in pixels.</summary>
    public int Top => FileSelectMapRomData.EntryWindowTop - revealSteps * FileSelectMapRomData.EntryWindowSpeed;

    /// <summary>Gets the lower edge of the expanding reveal window in pixels.</summary>
    public int Bottom => FrontendFrame.Height - Top;

    /// <summary>Creates the entry sequence from the installed file-select palette.</summary>
    /// <param name="mapPalettes">Static frontend palettes supplying the fade destination.</param>
    public FileSelectMapEntry(MapStaticPalettes mapPalettes)
    {
        Ensure.NotNull(mapPalettes);
        new CgramPalettes(Palette).LoadImage(mapPalettes.FileSelect);
        Bgr555[] target = new CgramPalettes(Palette).Image.ToArray();
        target[14] = target[30] = Bgr555.Black;
        // File select runs outside gameplay; no other fade advances the counter meanwhile.
        palette = new CartridgePaletteTransition(
            target, FileSelectMapRomData.EntryPaletteDenominator, new GradualColorChangeCounter());
        // The gradual first-two-palettes routine starts at transition number one,
        // unlike the global door/death fade. Consume only the shared helper's no-op.
        palette.Step(Palette);
    }

    /// <summary>Changes the fade destination while retaining its current colors and counter.</summary>
    internal void BindPalettes(MapStaticPalettes? content)
    {
        if (content is null) return;
        // Update the destination without restarting the ongoing fade or replacing
        // current interpolated colors. The native two cleared entries stay black.
        for (int color = 0; color < CgramPalettes.ColorCount; color++)
            palette.SetTargetColor(color, color is 14 or 30 ? Bgr555.Black : content.FileSelect[color]);
    }

    /// <summary>Advances the palette fade, setup coroutine, or centered reveal by one frontend update.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case FileSelectMapEntryPhase.PaletteFade:
                if (palette.Step(Palette)) Phase = FileSelectMapEntryPhase.LoadForeground;
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
                // Native returns before updating edges on the signed-underflow call,
                // disables the mask, and displays the whole area scene on that frame.
                if (Top - FileSelectMapRomData.EntryWindowSpeed < 0)
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
