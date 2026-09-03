using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed implementation of the file-select path at
/// <c>$81:944E-$81:A32A</c>.
/// </summary>
/// <remarks>
/// This class deliberately models the native menu index and brightness transitions instead
/// of replacing the screen with desktop widgets. Its tile graphics, starfield, text, palette,
/// border, helmets, and selection missile all come from the supplied retail ROM. Slot
/// validity and displayed energy/time are decoded from the cartridge's redundant SRAM
/// checksum layout rather than from host-authored metadata.
/// </remarks>
public sealed partial class FileSelectMenuState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SuperMetroidSaveRam saveRam;
    private readonly MenuPpuState ppu;
    private readonly OamBuffer oam = new();
    private readonly ControllerInputState controller = new();
    private readonly ushort[] bg1Tilemap = new ushort[32 * 32];
    private readonly SuperMetroidSaveSlot?[] saveSlots = new SuperMetroidSaveSlot?[3];
    private int missileAnimationTimer = 1;
    private int missileAnimationFrame;
    private int helmetAnimationTimer;
    private int helmetAnimationFrame;
    private int brightness;

    /// <summary>Performs menu indices zero through two, including every native ROM transfer.</summary>
    public FileSelectMenuState(ISnesAddressSpace bus, CartridgeAudioState? audio = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        ppu = new MenuPpuState(bus);

        saveRam = new SuperMetroidSaveRam(bus);
        for (int slot = 0; slot < saveSlots.Length; slot++)
            saveSlots[slot] = saveRam.ReadSlot(slot);
        SelectedItem = saveRam.ReadSelectedSlot();

        BuildSaveTilemap();
        ppu.Vram.ExecuteWordTransfer(bg1Tilemap, MenuPpuState.Bg1TilemapWord, 1);

        // Menu index two turns the screen on and index three fades to brightness fifteen.
        // Begin at zero so the first frames remain observable instead of appearing instantly.
        Phase = FileSelectPhase.FadeIn;
        brightness = 0;
    }

    /// <summary>Native main-menu selection: slots A-C, Copy, Clear, or Exit.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>Current translated native menu phase.</summary>
    public FileSelectPhase Phase { get; private set; }

    /// <summary>True once a fresh slot finishes its helmet turn and fade-out.</summary>
    public bool NewGameRequested { get; private set; }

    /// <summary>Selected physical SRAM slot, restricted to A-C once a game is requested.</summary>
    public int SelectedSaveSlot => Math.Clamp(SelectedItem, 0, 2);

    /// <summary>Whether the selected A-C slot passed either native checksum directory.</summary>
    public bool SelectedSlotContainsSave =>
        SelectedItem < saveSlots.Length && saveSlots[SelectedItem] is not null;

    /// <summary>True when Exit/B completes the native fade back toward reset.</summary>
    public bool TitleRequested { get; private set; }

    /// <summary>True only on the frame in which Copy or Clear mutated cartridge SRAM.</summary>
    public bool SaveRamChangedThisFrame { get; private set; }

    /// <summary>Current INIDISP brightness nibble.</summary>
    public byte Brightness => (byte)brightness;

    /// <summary>
    /// Read-only access for cartridge-layout verification. Gameplay code uploads this same
    /// buffer to BG1; exposing a span avoids adding a second, test-only tilemap builder.
    /// </summary>
    internal ReadOnlySpan<ushort> BackgroundTilemap => bg1Tilemap;

    /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
    internal int SelectedHelmetFrame => helmetAnimationFrame;

    /// <summary>Advances one native menu frame from a raw SNES controller word.</summary>
    public void Step(ushort controllerInput)
    {
        SaveRamChangedThisFrame = false;
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressedButtons;

        StepMissileAnimation();
        switch (Phase)
        {
            case FileSelectPhase.FadeIn:
                brightness = Math.Min(15, brightness + 1);
                if (brightness == 15)
                    Phase = FileSelectPhase.Main;
                break;

            case FileSelectPhase.Main:
                StepMainMenu(pressed);
                break;

            case FileSelectPhase.FadeOutToDataManagement:
            case FileSelectPhase.FadeOutToMain:
            case FileSelectPhase.FadeInFromDataManagement:
                StepDataManagementFade();
                break;

            case FileSelectPhase.CopySelectSource:
            case FileSelectPhase.CopySelectDestination:
            case FileSelectPhase.CopyConfirm:
            case FileSelectPhase.CopyCompleted:
            case FileSelectPhase.ClearSelectSlot:
            case FileSelectPhase.ClearConfirm:
            case FileSelectPhase.ClearCompleted:
                StepDataManagement(pressed);
                break;

            case FileSelectPhase.TurnSelectedHelmet:
                if (--helmetAnimationTimer <= 0)
                {
                    helmetAnimationTimer = 8;
                    helmetAnimationFrame++;
                    if (helmetAnimationFrame >= 7)
                        Phase = FileSelectPhase.FadeOutToOptions;
                }
                // The native routine also permits Start/A to end the turn early.
                if ((pressed & (SnesButton.Start | SnesButton.A)) != 0)
                    Phase = FileSelectPhase.FadeOutToOptions;
                break;

            case FileSelectPhase.FadeOutToOptions:
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    NewGameRequested = true;
                break;

            case FileSelectPhase.FadeOutToTitle:
                brightness = Math.Max(0, brightness - 1);
                if (brightness == 0)
                    TitleRequested = true;
                break;
        }
    }

    /// <summary>Composes the menu's BG2, BG1, and OBJ main-screen layers.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224);
        Rgba32[] backgroundLayer = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg2TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        Rgba32[] foreground = SnesBgTilemapRenderer.Render4BppViewport(
            ppu.Vram, ppu.Cgram, MenuPpuState.Bg1TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        SnesLayerCompositor.Composite(background, backgroundLayer);
        SnesLayerCompositor.Composite(background, foreground);

        oam.BeginFrame();
        bool mainScreen = IsMainScreenPhase;
        ushort border = IsCopyPhase
            ? FileSelectLayout.CopyBorderSpritemap
            : IsClearPhase
                ? FileSelectLayout.ClearBorderSpritemap
                : FileSelectLayout.NormalBorderSpritemap;
        ushort borderX = IsClearPhase ? (ushort)124 : (ushort)128;
        DrawMenuSpritemap(border, borderX, 16);
        if (ShouldDrawSelectionMissile)
        {
            (ushort missileX, ushort missileY) = GetSelectionMissilePosition();
            DrawMenuSpritemap(
                FileSelectLayout.MissileSpritemapIds[missileAnimationFrame],
                missileX,
                missileY);
        }
        for (int slot = 0; mainScreen && slot < 3; slot++)
        {
            // Menu index 32 fades with the completed (or Start-shortened) helmet turn
            // still resident in OAM. Resetting to spritemap $2C during the fade produces
            // a one-frame-visible head snap immediately before loading.
            int frame = slot == SelectedItem && Phase is
                    FileSelectPhase.TurnSelectedHelmet or FileSelectPhase.FadeOutToOptions
                ? helmetAnimationFrame
                : 0;
            DrawMenuSpritemap(
                (ushort)(0x2c + Math.Min(frame, 7)),
                100,
                FileSelectLayout.HelmetY[slot]);
        }
        oam.FinalizeFrame();

        Rgba32[] objects = SnesObjRenderer.Render(oam, ppu.Vram, ppu.Cgram, obsel: 0x03);
        SnesLayerCompositor.Composite(background, objects);
        ApplyBrightness(background);
        return background;
    }

    private void BuildSaveTilemap()
    {
        // Native `ClearMenuTilemap` fills every word with character $00F (blank).
        Array.Fill(bg1Tilemap, FileSelectLayout.BlankTile);
        LoadMenuTilemap(FileSelectLayout.SamusDataDestination, FileSelectTilemaps.SamusData);
        LoadMenuTilemap(FileSelectLayout.SlotALabelDestination, FileSelectTilemaps.SamusA);
        // `$81:A08E-$81:A096` adds one complete $40-byte tilemap row to the slot's
        // energy-field origin before loading NO DATA. The leading blank word in the ROM
        // string then places N at column 15. Using $01BC here would instead begin at column
        // 30, putting N in column 31 and wrapping O DATA onto the following scanline.
        DrawFileSlot(
            saveSlots[0],
            FileSelectLayout.SlotAEnergyDestination,
            FileSelectLayout.SlotATimeValueDestination);
        // `$81:9F3A` conditionally draws only the numeric HH:MM value. The following
        // `$81:9F3D-$81:9F43` tilemap load is unconditional, so the static TIME caption
        // remains visible even when slot A is empty and its numeric fields are omitted.
        LoadMenuTilemap(FileSelectLayout.SlotATimeLabelDestination, FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.SlotBLabelDestination, FileSelectTilemaps.SamusB);
        DrawFileSlot(
            saveSlots[1],
            FileSelectLayout.SlotBEnergyDestination,
            FileSelectLayout.SlotBTimeValueDestination);
        // Slot B repeats the same native split between conditional digits and an
        // unconditional ROM-authored caption (`$81:9F70-$81:9F79`).
        LoadMenuTilemap(FileSelectLayout.SlotBTimeLabelDestination, FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.SlotCLabelDestination, FileSelectTilemaps.SamusC);
        DrawFileSlot(
            saveSlots[2],
            FileSelectLayout.SlotCEnergyDestination,
            FileSelectLayout.SlotCTimeValueDestination);
        // Slot C's caption is likewise loaded unconditionally at `$81:9FA9-$81:9FAF`.
        LoadMenuTilemap(FileSelectLayout.SlotCTimeLabelDestination, FileSelectTilemaps.Time);
        if (saveSlots.Any(slot => slot is not null))
        {
            // Native index 16 exposes both data-management entries only when at least one
            // checksummed slot exists; an all-empty SRAM image skips directly from C to Exit.
            LoadMenuTilemap(FileSelectLayout.DataCopyDestination, FileSelectTilemaps.DataCopy);
            LoadMenuTilemap(FileSelectLayout.DataClearDestination, FileSelectTilemaps.DataClear);
        }
        LoadMenuTilemap(FileSelectLayout.ExitDestination, FileSelectTilemaps.Exit);
    }

    /// <summary>Ports <c>Draw_FileSelection_Energy/Time</c> at $81:A087/$A14E.</summary>
    private void DrawFileSlot(
        SuperMetroidSaveSlot? slot,
        int energyOrigin,
        int timeOrigin)
    {
        if (slot is null)
        {
            // The native empty path advances one complete tilemap row before writing the
            // leading blank and NO DATA text. The tilemap is already blank everywhere else.
            LoadMenuTilemap(
                energyOrigin + FileSelectLayout.NextTilemapRowByteOffset,
                FileSelectTilemaps.NoData);
            return;
        }

        LoadMenuTilemap(energyOrigin, FileSelectTilemaps.Energy);
        int healthRemainder = slot.Health % 100;
        WriteMenuDigit(energyOrigin + 0x42, healthRemainder / 10);
        WriteMenuDigit(energyOrigin + 0x44, healthRemainder % 10);

        int hours = Math.Min(slot.GameTimeHours, (ushort)99);
        int minutes = Math.Min(slot.GameTimeMinutes, (ushort)99);
        WriteMenuDigit(timeOrigin, hours / 10);
        WriteMenuDigit(timeOrigin + 2, hours % 10);
        LoadMenuTilemap(timeOrigin + 4, FileSelectTilemaps.TimeColon);
        WriteMenuDigit(timeOrigin + 6, minutes / 10);
        WriteMenuDigit(timeOrigin + 8, minutes % 10);
    }

    private void WriteMenuDigit(int destinationByteOffset, int digit)
    {
        int wordIndex = destinationByteOffset >> 1;
        if ((uint)wordIndex >= bg1Tilemap.Length)
            throw new InvalidDataException("A file-select digit escaped the 32x32 BG1 buffer.");
        bg1Tilemap[wordIndex] = unchecked((ushort)(FileSelectLayout.DigitTileBase + digit));
    }

    private void LoadMenuTilemap(int destinationByteOffset, ushort sourcePointer)
    {
        int initialColumn = destinationByteOffset;
        int sourceAddress = FileSelectTilemapFormat.Bank | sourcePointer;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
            sourceAddress = FileSelectTilemapFormat.Bank | ((sourceAddress + 2) & 0xffff);
            if (word == FileSelectTilemapFormat.End)
                return;
            if (word == FileSelectTilemapFormat.NextRow)
            {
                initialColumn += FileSelectTilemapFormat.RowByteCount;
                destinationByteOffset = initialColumn;
                continue;
            }

            int wordIndex = destinationByteOffset >> 1;
            if ((uint)wordIndex >= bg1Tilemap.Length)
                throw new InvalidDataException("A bank-$81 menu tilemap escaped the 32x32 BG1 buffer.");
            bg1Tilemap[wordIndex] = word;
            destinationByteOffset += 2;
        }
    }

    private void DrawMenuSpritemap(ushort spritemapId, ushort x, ushort y)
    {
        // `DrawMenuSpritemap` indexes a word-pointer array, so the displayed spritemap ID
        // is doubled before following bank $82. AddOnScreenSpritemap then applies the same
        // `$F1FF | $0E00` attribute replacement as the native r3 argument.
        int pointerAddress = MenuPpuState.SpritemapPointerTableAddress + spritemapId * 2;
        ushort pointer = RomDataReader.ReadWordFixedBank(bus, pointerAddress);
        oam.AddOnScreenSpritemap(bus, 0x820000 | pointer, x, y, MenuPpuState.ObjectPaletteBits);
    }

    private void StepMissileAnimation()
    {
        if (--missileAnimationTimer != 0)
            return;
        missileAnimationFrame = (missileAnimationFrame + 1) & 3;
        missileAnimationTimer = 8;
    }

    private void ApplyBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            pixels[pixel] = brightness switch
            {
                >= 15 => color,
                <= 0 => new Rgba32(0, 0, 0, color.A),
                _ => new Rgba32(
                    (byte)(color.R * brightness / 15),
                    (byte)(color.G * brightness / 15),
                    (byte)(color.B * brightness / 15),
                    color.A),
            };
        }
    }
}

/// <summary>Debugger-facing names for native file-select menu indices 3, 4, 31-33.</summary>
public enum FileSelectPhase
{
    FadeIn,
    Main,
    FadeOutToDataManagement,
    FadeInFromDataManagement,
    CopySelectSource,
    CopySelectDestination,
    CopyConfirm,
    CopyCompleted,
    ClearSelectSlot,
    ClearConfirm,
    ClearCompleted,
    FadeOutToMain,
    TurnSelectedHelmet,
    FadeOutToOptions,
    FadeOutToTitle,
}
