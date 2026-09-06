using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Initial palette preparation and centered area-map reveal from $81:A32A-$81:A725.</summary>
public sealed class FileSelectMapEntry
{
    private readonly CartridgePaletteTransition palette;
    private int revealSteps;
    public SnesCgram Cgram { get; } = new();
    public FileSelectMapEntryPhase Phase { get; private set; } = FileSelectMapEntryPhase.PaletteFade;
    public bool IsComplete => Phase == FileSelectMapEntryPhase.Complete;
    public int Left => FileSelectMapRomData.EntryWindowLeft - revealSteps * FileSelectMapRomData.EntryWindowSpeed;
    public int Right => FileSelectMapRomData.EntryWindowRight + revealSteps * FileSelectMapRomData.EntryWindowSpeed;
    public int Top => FileSelectMapRomData.EntryWindowTop - revealSteps * FileSelectMapRomData.EntryWindowSpeed;
    public int Bottom => FrontendFrame.Height - Top;

    public FileSelectMapEntry(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Cgram.LoadFromBus(bus, FileSelectMapRomData.EntryPalette);
        ushort[] target = Cgram.Colors.ToArray();
        target[14] = target[30] = 0;
        palette = new CartridgePaletteTransition(target, FileSelectMapRomData.EntryPaletteDenominator);
        // The gradual first-two-palettes routine starts at transition number one,
        // unlike the global door/death fade. Consume only the shared helper's no-op.
        palette.Step(Cgram);
    }

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

    /// <summary>BG1/OBJ/subscreen are visible inside window one; the cleared BG2 covers its exterior.</summary>
    public Rgba32[] Render(ReadOnlySpan<Rgba32> areaScene)
    {
        if (areaScene.Length != FrontendFrame.Width * FrontendFrame.Height)
            throw new ArgumentException("Area reveal requires a complete frontend frame.", nameof(areaScene));
        if (IsComplete) return areaScene.ToArray();
        var pixels = new Rgba32[areaScene.Length];
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
public enum FileSelectMapEntryPhase { PaletteFade, LoadForeground, LoadBackground, SetupWindow, Revealing, Complete }
