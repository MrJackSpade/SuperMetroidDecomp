namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Native WRAM identities of the pause menu and the gameplay state it backs up.</summary>
internal static class PauseMemory
{
    /// <summary>$0723/$0725: screen fade delay and counter.</summary>
    public const int ScreenFadeDelay = 0x0723, ScreenFadeCounter = 0x0725;
    /// <summary>$0727: pause menu index; zero is the stable map page.</summary>
    public const int MenuIndex = 0x0727;
    /// <summary>$073B/$074F: map palette animation timer and frame (byte values).</summary>
    public const int PaletteAnimationTimer = 0x073b, PaletteAnimationFrame = 0x074f;
    /// <summary>$0753: L/R/Start button label mode; zero labels the map page.</summary>
    public const int ButtonLabelMode = 0x0753;
    /// <summary>$0776/$0778: Samus position indicator animation frame (word byte offset) and timer.</summary>
    public const int IndicatorAnimationFrame = 0x0776, IndicatorAnimationTimer = 0x0778;
    /// <summary>$B1/$B3: BG1 scroll, which the pause map page uses as its map scroll.</summary>
    public const int MapHorizontalScroll = 0x00b1, MapVerticalScroll = 0x00b3;
    /// <summary>$05DB-$05E3: delayed-held input timer, reset, held, new and previous words.</summary>
    public const int TimedHeldTimer = 0x05db, TimedHeldReset = 0x05dd, TimedHeldInput = 0x05df,
        TimedHeldNew = 0x05e1, TimedHeldPrevious = 0x05e3;
    /// <summary>$7E:3300: gameplay palettes backed up by <c>$82:8FD4</c> while the menu is open.</summary>
    public const int GameplayPaletteBackup = 0x3300;
}
