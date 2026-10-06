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
    private const ushort DoorCompletionVisorGreen = 0x3be0;

    public static void RestoreLoadedBeamPalette(SnesCgram current, ReadOnlySpan<ushort> loaded)
    {
        for (int index = BeamPaletteStart; index < BeamPaletteStart + BeamPaletteCount; index++)
            current.SetColor(index, loaded[index]);
    }

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

    public static void PreserveHud(ReadOnlySpan<ushort> current, Span<ushort> target)
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

    public static void PreserveCommonCre(ReadOnlySpan<ushort> current, Span<ushort> target)
    {
        target[HudPaletteBackground] = current[HudPaletteBackground];
        target[ItemBackgroundOutline] = current[ItemBackgroundOutline];
        target[ItemBackground] = current[ItemBackground];
        target[ItemOutline] = current[ItemOutline];
        target[MinimapPaletteBackground] = current[MinimapPaletteBackground];
    }

    public static void PreserveEscapeTimer(ReadOnlySpan<ushort> current, Span<ushort> target)
    {
        target[TimerFirstColor] = current[TimerFirstColor];
        target[TimerSecondColor] = current[TimerSecondColor];
        target[TimerFourthColor] = current[TimerFourthColor];
        target[TimerThirteenthColor] = current[TimerThirteenthColor];
    }
}

