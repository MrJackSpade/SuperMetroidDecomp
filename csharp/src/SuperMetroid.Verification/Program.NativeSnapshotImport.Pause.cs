using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Imports a snapshot taken on the pause menu's stable map page (game state $0F). The
    /// menu is built as state $0D builds it, then takes the words the native menu has changed
    /// since: map scroll, animation phases and the delayed-held input filter. Gameplay colors
    /// come from <c>BackupOfPalettesDuringMenu</c>, where pausing moved them.
    /// </summary>
    private static void ImportNativePauseMenu(SuperMetroidGame game, SuperMetroidRuntime runtime,
        ISnesAddressSpace bus, Func<int, ushort> W, byte[] memory)
    {
        AssertEqual((ushort)SuperMetroidGameState.PausedB, W(NativeSnapshotMemory.GameState),
            "pause import covers the stable pause menu");
        AssertEqual((ushort)0, W(PauseMemory.MenuIndex), "imported pause menu is on its map page");
        AssertEqual((ushort)0, W(PauseMemory.ButtonLabelMode), "imported pause menu shows map labels");
        runtime.Cgram.LoadBytes(memory.AsSpan(PauseMemory.GameplayPaletteBackup, SnesCgram.ByteCount));

        CartridgeRoomHeader room = runtime.ActiveRoom ?? throw new InvalidDataException("Paused snapshot has no room.");
        var menu = new PauseMenuState(
            bus,
            runtime.Samus ?? throw new InvalidDataException("Paused snapshot has no Samus."),
            runtime.System,
            room.AreaIndex,
            room.MapX,
            room.MapY,
            PrivateState.Field<CartridgeAudioState>(game, "audio"),
            runtime.Vram,
            game.ConfiguredOptions.MapReveal,
            PrivateState.Field<AreaMapPresentationCatalog>(game, "mapPresentation"));
        PrivateState.SetField(menu, "mapHorizontalScroll", W(PauseMemory.MapHorizontalScroll));
        PrivateState.SetField(menu, "mapVerticalScroll", W(PauseMemory.MapVerticalScroll));
        object palette = PrivateState.Field<object>(menu, "paletteAnimation");
        PrivateState.SetField(palette, "timer", memory[PauseMemory.PaletteAnimationTimer]);
        PrivateState.SetField(palette, "frame", memory[PauseMemory.PaletteAnimationFrame]);
        PrivateState.SetField(menu, "mapIndicatorAnimationFrame", (int)W(PauseMemory.IndicatorAnimationFrame));
        PrivateState.SetField(menu, "mapIndicatorAnimationTimer", (int)W(PauseMemory.IndicatorAnimationTimer));
        PrivateState.SetField(game, "pauseMenu", menu);
        PrivateState.SetField(game, "pauseFadeDelay", (int)W(PauseMemory.ScreenFadeDelay));
        PrivateState.SetField(game, "pauseFadeCounter", (int)W(PauseMemory.ScreenFadeCounter));

        foreach (var (property, address) in new[]
        {
            (nameof(Bank80SystemState.TimedHeldInputTimer), PauseMemory.TimedHeldTimer),
            (nameof(Bank80SystemState.TimedHeldInputTimerReset), PauseMemory.TimedHeldReset),
            (nameof(Bank80SystemState.TimedHeldInput), PauseMemory.TimedHeldInput),
            (nameof(Bank80SystemState.NewlyTimedHeldInput), PauseMemory.TimedHeldNew),
            (nameof(Bank80SystemState.TimedHeldInputPrevious), PauseMemory.TimedHeldPrevious),
        }) PrivateState.SetProperty(runtime.System, property, W(address));
    }
}

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
    /// <summary>$0776/$0778: Samus position indicator animation frame and timer.</summary>
    public const int IndicatorAnimationFrame = 0x0776, IndicatorAnimationTimer = 0x0778;
    /// <summary>$B1/$B3: BG1 scroll, which the pause map page uses as its map scroll.</summary>
    public const int MapHorizontalScroll = 0x00b1, MapVerticalScroll = 0x00b3;
    /// <summary>$05DB-$05E3: delayed-held input timer, reset, held, new and previous words.</summary>
    public const int TimedHeldTimer = 0x05db, TimedHeldReset = 0x05dd, TimedHeldInput = 0x05df,
        TimedHeldNew = 0x05e1, TimedHeldPrevious = 0x05e3;
    /// <summary>$7E:3300: gameplay palettes backed up by <c>$82:8FD4</c> while the menu is open.</summary>
    public const int GameplayPaletteBackup = 0x3300;
}
