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
public sealed class FileSelectMenuState
{
    private static readonly ushort[] SelectionY = [48, 88, 128, 163, 187, 211];
    private static readonly ushort[] HelmetY = [47, 87, 127];
    private static readonly ushort[] MissileSpritemapIds = [0x37, 0x36, 0x35, 0x34];

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
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

        var saveRam = new SuperMetroidSaveRam(bus);
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

    /// <summary>Current INIDISP brightness nibble.</summary>
    public byte Brightness => (byte)brightness;

    /// <summary>
    /// Read-only access for cartridge-layout verification. Gameplay code uploads this same
    /// buffer to BG1; exposing a span avoids adding a second, test-only tilemap builder.
    /// </summary>
    internal ReadOnlySpan<ushort> BackgroundTilemap => bg1Tilemap;

    /// <summary>Advances one native menu frame from a raw SNES controller word.</summary>
    public void Step(ushort controllerInput)
    {
        controller.Latch(controllerInput);
        SnesButton pressed = (SnesButton)controller.NewlyPressed;

        StepMissileAnimation();
        switch (Phase)
        {
            case FileSelectPhase.FadeIn:
                brightness = Math.Min(15, brightness + 1);
                if (brightness == 15)
                    Phase = FileSelectPhase.Main;
                break;

            case FileSelectPhase.Main:
                // Copy/Clear remain outside the current playable slice. Up and Down retain
                // the native no-submenu cycle through A, B, C, and Exit.
                if ((pressed & SnesButton.Up) != 0)
                {
                    SelectedItem = SelectedItem switch { 0 => 5, 5 => 2, _ => SelectedItem - 1 };
                    audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                }
                else if ((pressed & SnesButton.Down) != 0)
                {
                    SelectedItem = SelectedItem switch { 2 => 5, 5 => 0, _ => SelectedItem + 1 };
                    audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                }

                if ((pressed & SnesButton.B) != 0)
                {
                    audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                    audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                    Phase = FileSelectPhase.FadeOutToTitle;
                }
                else if ((pressed & (SnesButton.Start | SnesButton.A)) != 0)
                {
                    if (SelectedItem < 3)
                    {
                        audio?.QueueSound(library: 1, soundId: 0x2a, maximumQueued: 6);
                        // `menu_index += 27` enters index 31 and enables only the selected
                        // helmet timer. A newly created save initializes to 99 energy later.
                        helmetAnimationFrame = 0;
                        helmetAnimationTimer = 1;
                        Phase = FileSelectPhase.TurnSelectedHelmet;
                    }
                    else if (SelectedItem == 5)
                    {
                        audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                        Phase = FileSelectPhase.FadeOutToTitle;
                    }
                }
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
        DrawMenuSpritemap(0x48, 128, 16); // Samus Data border.
        DrawMenuSpritemap(MissileSpritemapIds[missileAnimationFrame], 14, SelectionY[SelectedItem]);
        for (int slot = 0; slot < 3; slot++)
        {
            int frame = slot == SelectedItem && Phase == FileSelectPhase.TurnSelectedHelmet
                ? helmetAnimationFrame
                : 0;
            DrawMenuSpritemap((ushort)(0x2c + Math.Min(frame, 7)), 100, HelmetY[slot]);
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
        Array.Fill(bg1Tilemap, (ushort)0x000f);
        LoadMenuTilemap(destinationByteOffset: 0x056, sourcePointer: 0xb40a); // SAMUS DATA
        LoadMenuTilemap(destinationByteOffset: 0x146, sourcePointer: 0xb436); // SAMUS A
        // `$81:A08E-$81:A096` adds one complete $40-byte tilemap row to the slot's
        // energy-field origin before loading NO DATA. The leading blank word in the ROM
        // string then places N at column 15. Using $01BC here would instead begin at column
        // 30, putting N in column 31 and wrapping O DATA onto the following scanline.
        DrawFileSlot(saveSlots[0], energyOrigin: 0x15c, timeOrigin: 0x1b4);
        // `$81:9F3A` conditionally draws only the numeric HH:MM value. The following
        // `$81:9F3D-$81:9F43` tilemap load is unconditional, so the static TIME caption
        // remains visible even when slot A is empty and its numeric fields are omitted.
        LoadMenuTilemap(destinationByteOffset: 0x176, sourcePointer: 0xb4a0); // TIME
        LoadMenuTilemap(destinationByteOffset: 0x286, sourcePointer: 0xb456); // SAMUS B
        DrawFileSlot(saveSlots[1], energyOrigin: 0x29c, timeOrigin: 0x2f4);
        // Slot B repeats the same native split between conditional digits and an
        // unconditional ROM-authored caption (`$81:9F70-$81:9F79`).
        LoadMenuTilemap(destinationByteOffset: 0x2b6, sourcePointer: 0xb4a0); // TIME
        LoadMenuTilemap(destinationByteOffset: 0x3c6, sourcePointer: 0xb476); // SAMUS C
        DrawFileSlot(saveSlots[2], energyOrigin: 0x3dc, timeOrigin: 0x434);
        // Slot C's caption is likewise loaded unconditionally at `$81:9FA9-$81:9FAF`.
        LoadMenuTilemap(destinationByteOffset: 0x3f6, sourcePointer: 0xb4a0); // TIME
        LoadMenuTilemap(destinationByteOffset: 0x688, sourcePointer: 0xb4ee); // EXIT
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
            LoadMenuTilemap(energyOrigin + 0x40, 0xb4ac);
            return;
        }

        LoadMenuTilemap(energyOrigin, 0xb496); // ENERGY
        int healthRemainder = slot.Health % 100;
        WriteMenuDigit(energyOrigin + 0x42, healthRemainder / 10);
        WriteMenuDigit(energyOrigin + 0x44, healthRemainder % 10);

        int hours = Math.Min(slot.GameTimeHours, (ushort)99);
        int minutes = Math.Min(slot.GameTimeMinutes, (ushort)99);
        WriteMenuDigit(timeOrigin, hours / 10);
        WriteMenuDigit(timeOrigin + 2, hours % 10);
        LoadMenuTilemap(timeOrigin + 4, 0xb4a8); // Colon.
        WriteMenuDigit(timeOrigin + 6, minutes / 10);
        WriteMenuDigit(timeOrigin + 8, minutes % 10);
    }

    private void WriteMenuDigit(int destinationByteOffset, int digit)
    {
        int wordIndex = destinationByteOffset >> 1;
        if ((uint)wordIndex >= bg1Tilemap.Length)
            throw new InvalidDataException("A file-select digit escaped the 32x32 BG1 buffer.");
        bg1Tilemap[wordIndex] = unchecked((ushort)(0x2060 + digit));
    }

    private void LoadMenuTilemap(int destinationByteOffset, ushort sourcePointer)
    {
        int initialColumn = destinationByteOffset;
        int sourceAddress = 0x810000 | sourcePointer;
        while (true)
        {
            ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
            sourceAddress = 0x810000 | ((sourceAddress + 2) & 0xffff);
            if (word == 0xffff)
                return;
            if (word == 0xfffe)
            {
                initialColumn += 64;
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
    TurnSelectedHelmet,
    FadeOutToOptions,
    FadeOutToTitle,
}
