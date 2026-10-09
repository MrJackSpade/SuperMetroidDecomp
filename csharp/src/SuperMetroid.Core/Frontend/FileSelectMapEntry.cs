using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Initial palette preparation and centered area-map reveal from $81:A32A-$81:A725.</summary>
public sealed class FileSelectMapEntry
{
    /// <summary>Transition tracking progress toward the installed file-select colors without restarting the fade.</summary>
    private readonly CartridgePaletteTransition palette;
    /// <summary>Number of eight-pixel expansion updates applied to the centered area-map reveal.</summary>
    private int revealSteps;

    /// <summary>Gets the live 256-color memory used by the entry palette fade.</summary>
    public SnesCgram Cgram { get; } = new();

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
        ArgumentNullException.ThrowIfNull(mapPalettes);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            Cgram.SetColor(color, mapPalettes.FileSelect[color]);
        ushort[] target = Cgram.Colors.ToArray();
        target[14] = target[30] = 0;
        // File select runs outside gameplay; no other fade advances the counter meanwhile.
        palette = new CartridgePaletteTransition(
            target, FileSelectMapRomData.EntryPaletteDenominator, new GradualColorChangeCounter());
        // The gradual first-two-palettes routine starts at transition number one,
        // unlike the global door/death fade. Consume only the shared helper's no-op.
        palette.Step(Cgram);
    }

    /// <summary>Rebinds the fade destination to current installed palettes while retaining its current interpolation state.</summary>
    /// <param name="content">Current static palettes, or null when no replacement palette data is available.</param>
    internal void BindPalettes(MapStaticPalettes? content)
    {
        if (content is null) return;
        // Update the destination without restarting the ongoing fade or replacing
        // current interpolated colors. The native two cleared entries stay black.
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            palette.SetTargetColor(color, color is 14 or 30 ? (ushort)0 : content.FileSelect[color]);
    }

    /// <summary>Advances the palette fade, setup coroutine, or centered reveal by one frontend update.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case FileSelectMapEntryPhase.PaletteFade:
                if (palette.Step(Cgram)) Phase = FileSelectMapEntryPhase.LoadForeground;
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

    /// <summary>As above, writing into <paramref name="pixels"/>, which must not alias the area scene.</summary>
    public Rgba32[] Render(ReadOnlySpan<Rgba32> areaScene, Rgba32[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        if (areaScene.Length != FrontendFrame.Width * FrontendFrame.Height || pixels.Length != areaScene.Length)
            throw new ArgumentException("Area reveal requires a complete frontend frame and output.", nameof(areaScene));
        if (IsComplete)
        {
            areaScene.CopyTo(pixels);
            return pixels;
        }
        Array.Fill(pixels, new Rgba32(0, 0, 0));
        if (Phase == FileSelectMapEntryPhase.Revealing)
            for (int y = Top; y < Bottom; y++)
            {
                int offset = y * FrontendFrame.Width + Left;
                areaScene.Slice(offset, Right - Left + 1).CopyTo(pixels.AsSpan(offset));
            }
        return pixels;
    }
}

/// <summary>Entry coroutine stages before normal area-map input becomes active.</summary>
public enum FileSelectMapEntryPhase
{
    /// <summary>Fades the first two palettes to their installed file-select colors.</summary>
    PaletteFade,
    /// <summary>Represents the native foreground-tilemap loading stage.</summary>
    LoadForeground,
    /// <summary>Represents the native background-tilemap loading stage.</summary>
    LoadBackground,
    /// <summary>Configures the centered initial reveal window.</summary>
    SetupWindow,
    /// <summary>Expands the reveal window by eight pixels per update.</summary>
    Revealing,
    /// <summary>Displays the complete area scene and permits normal map input.</summary>
    Complete
}
