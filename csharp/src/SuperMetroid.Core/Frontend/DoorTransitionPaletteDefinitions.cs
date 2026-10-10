using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Named color-copy operations of GameState_A_LoadingNextRoom, $82:E1F1-$E264.</summary>
public static class DoorTransitionPaletteDefinitions
{
    /// <summary>$90:ACCD writes the sixteen beam colors directly to live sprite palette six.</summary>
    private const int BeamPaletteStart = 224, BeamPaletteCount = 16;
    /// <summary>$82:E52E writes Palettes_SpriteP4C4 when door IRQ scrolling completes.</summary>
    public const int VisorColorIndex = 196;
    /// <summary>$82:E52B loads the ordinary green visor color before the final loading PLM call.</summary>
    private static readonly Bgr555 DoorCompletionVisorGreen = Bgr555.FromWord(0x3be0);

    /// <summary>Reproduces the live sprite-palette-six write of $90:ACCD after restoring the faded source colors; the newly loaded beam colors do not wait for the destination palette fade.</summary>
    /// <param name="current">Live CGRAM palette to receive colors 224..239; all other colors are retained.</param>
    /// <param name="loaded">Destination's captured palette, indexed by CGRAM color number, containing at least 240 colors.</param>
    public static void RestoreLoadedBeamPalette(SnesCgram current, ReadOnlySpan<Bgr555> loaded)
    {
        for (int index = BeamPaletteStart; index < BeamPaletteStart + BeamPaletteCount; index++)
            current.SetColor(index, loaded[index]);
    }

    /// <summary>Performs $82:E52B-$E52E's live visor reset to $3BE0 when door scrolling completes, before the final loading PLM handler.</summary>
    /// <param name="current">Live CGRAM palette whose sprite-palette-four color four (index 196) is replaced; no fade target is modified.</param>
    public static void PublishCompletedScrollVisor(SnesCgram current) =>
        current.SetColor(VisorColorIndex, DoorCompletionVisorGreen);

    /// <summary>$82:E1F1: Palettes_BG3P2MinimapExplored; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapExplored = 9;

    /// <summary>$82:E1F7: Palettes_BG3P2MinimapExploredFeature; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapExploredFeature = 10;

    /// <summary>$82:E1FD: Palettes_BG3P3MinimapUnexplored; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapUnexplored = 13;

    /// <summary>$82:E203: Palettes_BG3P3MinimapUnexploredFeature; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapUnexploredFeature = 14;

    /// <summary>$82:E209: Palettes_BG3P4HighlightedHUDItemBackgroundOutline; CGRAM slot copied unchanged during the source fade.</summary>
    private const int HighlightedItemBackgroundOutline = 17;

    /// <summary>$82:E20F: Palettes_BG3P4HighlightedHUDItemBackground; CGRAM slot copied unchanged during the source fade.</summary>
    private const int HighlightedItemBackground = 18;

    /// <summary>$82:E215: Palettes_BG3P4HighlightedHUDItemOutline; CGRAM slot copied unchanged during the source fade.</summary>
    private const int HighlightedItemOutline = 19;

    /// <summary>$82:E21B: Palettes_BG3P7MinimapRoomHighlight; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapRoomHighlight = 29;

    /// <summary>$82:E22C: Palettes_BG3P5; CGRAM slot copied unchanged during the source fade.</summary>
    private const int HudPaletteBackground = 20;

    /// <summary>$82:E232: Palettes_BG3P5HUDItemBackgroundOutline; CGRAM slot copied unchanged during the source fade.</summary>
    private const int ItemBackgroundOutline = 21;

    /// <summary>$82:E238: Palettes_BG3P5HUDItemBackground; CGRAM slot copied unchanged during the source fade.</summary>
    private const int ItemBackground = 22;

    /// <summary>$82:E23E: Palettes_BG3P5HUDItemOutline; CGRAM slot copied unchanged during the source fade.</summary>
    private const int ItemOutline = 23;

    /// <summary>$82:E244: Palettes_BG3P7; CGRAM slot copied unchanged during the source fade.</summary>
    private const int MinimapPaletteBackground = 28;

    /// <summary>$82:E24F: Palettes_SpriteP5+2; CGRAM slot copied unchanged during the source fade.</summary>
    private const int TimerFirstColor = 209;

    /// <summary>$82:E255: Palettes_SpriteP5+4; CGRAM slot copied unchanged during the source fade.</summary>
    private const int TimerSecondColor = 210;

    /// <summary>$82:E25B: Palettes_SpriteP5+8; CGRAM slot copied unchanged during the source fade.</summary>
    private const int TimerFourthColor = 212;

    /// <summary>$82:E261: Palettes_SpriteP5+$1A; CGRAM slot copied unchanged during the source fade.</summary>
    private const int TimerThirteenthColor = 221;

    /// <summary>Copies the eight always-preserved minimap and highlighted-item colors of $82:E1F1-$E21E into the source-room fade target, keeping their current appearance while room colors fade to black.</summary>
    /// <param name="current">Live packed RGB5 colors, indexed by CGRAM color number, with at least 30 entries.</param>
    /// <param name="target">Caller-owned fade target with at least 30 entries; only indices 9, 10, 13, 14, 17, 18, 19, and 29 are overwritten.</param>
    public static void PreserveHud(ReadOnlySpan<Bgr555> current, Span<Bgr555> target)
    {
        target[MinimapExplored] = current[MinimapExplored];
        target[MinimapExploredFeature] = current[MinimapExploredFeature];
        target[MinimapUnexplored] = current[MinimapUnexplored];
        target[MinimapUnexploredFeature] = current[MinimapUnexploredFeature];
        target[HighlightedItemBackgroundOutline] = current[HighlightedItemBackgroundOutline];
        target[HighlightedItemBackground] = current[HighlightedItemBackground];
        target[HighlightedItemOutline] = current[HighlightedItemOutline];
        target[MinimapRoomHighlight] = current[MinimapRoomHighlight];
    }

    /// <summary>Copies the five additional common-room HUD/minimap colors of $82:E22C-$E247; the caller selects this operation only when bit zero is clear in both source and destination CRE bitsets.</summary>
    /// <param name="current">Live packed RGB5 colors, indexed by CGRAM color number, with at least 29 entries.</param>
    /// <param name="target">Caller-owned source fade target with at least 29 entries; only indices 20..23 and 28 are overwritten.</param>
    public static void PreserveCommonCre(ReadOnlySpan<Bgr555> current, Span<Bgr555> target)
    {
        target[HudPaletteBackground] = current[HudPaletteBackground];
        target[ItemBackgroundOutline] = current[ItemBackgroundOutline];
        target[ItemBackground] = current[ItemBackground];
        target[ItemOutline] = current[ItemOutline];
        target[MinimapPaletteBackground] = current[MinimapPaletteBackground];
    }

    /// <summary>Copies sprite-palette-five timer colors 1, 2, 4, and 13 as at $82:E24F-$E264; the caller requires an active escape timer and the same CRE-bit-zero condition as the common-room colors.</summary>
    /// <param name="current">Live packed RGB5 colors, indexed by CGRAM color number, with at least 222 entries.</param>
    /// <param name="target">Caller-owned source fade target with at least 222 entries; only indices 209, 210, 212, and 221 are overwritten.</param>
    public static void PreserveEscapeTimer(ReadOnlySpan<Bgr555> current, Span<Bgr555> target)
    {
        target[TimerFirstColor] = current[TimerFirstColor];
        target[TimerSecondColor] = current[TimerSecondColor];
        target[TimerFourthColor] = current[TimerFourthColor];
        target[TimerThirteenthColor] = current[TimerThirteenthColor];
    }
}

