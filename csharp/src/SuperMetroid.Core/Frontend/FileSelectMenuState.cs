using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

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
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    /// <summary>Stores object layer scratch.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Stores frame buffer.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Stores bus.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Stores audio.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Stores save ram.</summary>
    private readonly SuperMetroidSaveRam saveRam;
    /// <summary>Stores ppu.</summary>
    private readonly MenuPpuState ppu;
    /// <summary>Stores oam.</summary>
    private readonly OamBuffer oam = new();
    /// <summary>Stores controller.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Stores map presentation.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Stores bg1 tilemap.</summary>
    private readonly ushort[] bg1Tilemap = new ushort[32 * 32];
    /// <summary>Stores save slots.</summary>
    private readonly SuperMetroidSaveSlot?[] saveSlots = new SuperMetroidSaveSlot?[3];
    /// <summary>Stores missile animation timer.</summary>
    private int missileAnimationTimer = 1;
    /// <summary>Stores missile animation frame.</summary>
    private int missileAnimationFrame;
    /// <summary>Stores helmet animation timer.</summary>
    private int helmetAnimationTimer;
    /// <summary>Stores helmet animation frame.</summary>
    private int helmetAnimationFrame;
    /// <summary>$51 INIDISP byte: forced-blank bit plus brightness nibble.</summary>
    private int brightness;
    /// <summary>Stores screen fade.</summary>
    private readonly ScreenFade screenFade = new();
    /// <summary>Stores current presentation page.</summary>
    private string currentPresentationPage = FileSelectPresentationDefinitions.MainEmptyPage;

    /// <summary>Performs menu indices zero through two, including every native ROM transfer.</summary>
    public FileSelectMenuState(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = Ensure.NotNull(bus);
        this.audio = audio;
        this.mapPresentation = mapPresentation;
        ppu = new MenuPpuState(bus, mapPresentation?.Tiles, mapPresentation?.Palettes,
            mapPresentation?.WorldArtwork, mapPresentation?.Sprites,
            loadInitialBackground: mapPresentation is null);
        if (mapPresentation is not null)
            mapPresentation.FileSelect.LoadBackground(ppu.Vram);

        saveRam = new SuperMetroidSaveRam(bus, mapPresentation);
        for (int slot = 0; slot < saveSlots.Length; slot++)
            saveSlots[slot] = saveRam.ReadSlot(slot);
        SelectedItem = saveRam.ReadSelectedSlot();

        BuildSaveTilemap();
        new MenuVram(ppu.Vram).Bg1Tilemap.Write(bg1Tilemap);

        // Host-side loading above is atomic, but the native dispatches are not: indices
        // zero and two each wait for an NMI inside their dispatch before returning.
        Phase = FileSelectPhase.EnterBlankScreen;
        brightness = ScreenFade.ForcedBlank;
    }

    /// <summary>Native main-menu selection: slots A-C, Copy, Clear, or Exit.</summary>
    public int SelectedItem { get; private set; }

    /// <summary>Current translated native menu phase.</summary>
    public FileSelectPhase Phase { get; private set; }

    /// <summary>True once a selected slot, empty or occupied, finishes its helmet turn and fade-out, requesting the game-options screen.</summary>
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
    public byte Brightness => ScreenFade.Displayed(brightness);

    /// <summary>
    /// True when the next update resumes a native dispatch after its internal NMI wait
    /// rather than entering a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesAfterNmiWait => Phase is
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.InitializeMainAfterNmi;
    /// <summary>Advances one native menu frame from a raw SNES controller word.</summary>
    public void Step(ushort controllerInput)
    {
        SaveRamChangedThisFrame = false;
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressedButtons;

        if (!IsEntryPhase)
            StepMissileAnimation();
        switch (Phase)
        {
            case FileSelectPhase.EnterBlankScreen:
                // $81:944E: finish any fade-out, then SetForceBlankAndWaitForNMI.
                screenFade.FadeOut(ref brightness);
                if ((brightness & ScreenFade.FullyLit) != 0)
                    break;
                brightness = ScreenFade.ForcedBlank;
                Phase = FileSelectPhase.EnterBlankScreenAfterNmi;
                break;

            case FileSelectPhase.EnterBlankScreenAfterNmi:
                Phase = FileSelectPhase.LoadBackground;
                break;

            case FileSelectPhase.LoadBackground:
                // $81:9E93 queues the Zebes/stars BG2 tilemap.
                Phase = FileSelectPhase.InitializeMain;
                break;

            case FileSelectPhase.InitializeMain:
                // $81:9ED6 sets both fade words to one, then ClearForceBlankAndWaitForNMI.
                screenFade.SetTiming(1, 1);
                brightness &= ~ScreenFade.ForcedBlank;
                Phase = FileSelectPhase.InitializeMainAfterNmi;
                break;

            case FileSelectPhase.InitializeMainAfterNmi:
                Phase = FileSelectPhase.FadeIn;
                break;

            case FileSelectPhase.FadeIn:
                // $81:A058 index three.
                screenFade.FadeIn(ref brightness);
                if (brightness == ScreenFade.FullyLit)
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
                StepSelectedHelmet();
                // $81:9D91: Start/A ends the turn early; otherwise it ends once the
                // final frame has also used up its own timer, which then reads zero.
                if ((pressed & (SnesButton.Start | SnesButton.A)) != 0 ||
                    (helmetAnimationFrame == FileSelectHelmetAnimation.FrameCount - 1 && helmetAnimationTimer == 0))
                    Phase = FileSelectPhase.FadeOutToOptions;
                break;

            case FileSelectPhase.FadeOutToOptions:
                screenFade.FadeOut(ref brightness);
                if (ScreenFade.IsForcedBlank(brightness))
                    NewGameRequested = true;
                break;

            case FileSelectPhase.FadeOutToTitle:
                screenFade.FadeOut(ref brightness);
                if (ScreenFade.IsForcedBlank(brightness))
                    TitleRequested = true;
                break;
        }
    }

    /// <summary>Composes the menu's BG2, BG1, and OBJ main-screen layers.</summary>
    [PpuLayoutBoundary("backend: file-select software PPU composition")]
    public Rgba32[] Render()
    {
        Rgba32[] background = SnesLayerCompositor.CreateBackdrop(ppu.Cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);
        SnesBgTilemapRenderer.Composite4BppViewport(background, ppu.Vram, ppu.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg2TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) });
        SnesBgTilemapRenderer.Composite4BppViewport(background, ppu.Vram, ppu.Cgram, new BgVramAddresses { TilemapBaseWord = MenuVramLayout.Bg1TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) });

        PrepareRenderOam();
        Rgba32[] objects = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(objects, oam, ppu.Vram, ppu.Cgram, obsel: MenuRenderDefinitions.ObjectSelection);
        SnesLayerCompositor.Composite(background, objects);
        ApplyBrightness(background);
        return background;
    }

    /// <summary>Captures file-menu display state after simulation-owned OAM preparation.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        return MenuRenderSnapshotCapture.Capture(ppu, oam, 0, Brightness);
    }

    /// <summary>Prepares render oam.</summary>
    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        bool mainScreen = IsMainScreenPhase;
        if (mapPresentation is not null)
        {
            string borderName = IsCopyPhase
                ? FileSelectPresentationDefinitions.CopyBorder
                : IsClearPhase
                    ? FileSelectPresentationDefinitions.ClearBorder
                    : FileSelectPresentationDefinitions.MainBorder;
            mapPresentation.FileSelect.DrawBorder(oam, borderName);
            if (ShouldDrawSelectionMissile)
            {
                mapPresentation.FileSelect.DrawCursor(oam, missileAnimationFrame,
                    GetSelectionMissilePosition());
            }
            for (int slot = 0; mainScreen && slot < 3; slot++)
            {
                int frame = slot == SelectedItem && Phase is
                        FileSelectPhase.TurnSelectedHelmet or FileSelectPhase.FadeOutToOptions
                    ? helmetAnimationFrame
                    : 0;
                mapPresentation.FileSelect.DrawHelmet(oam, Math.Min(frame, FileSelectHelmetAnimation.FrameCount - 1), slot);
            }
            if (Phase == FileSelectPhase.CopyConfirm)
            {
                (int shape, ushort y) = FileCopyArrowDefinitions.Select(operationSourceSlot, operationDestinationSlot);
                foreach (CompiledSpritePart part in FileCopyArrowDefinitions.Parts(shape))
                    oam.AddOnScreenSpritePart(part.X, part.Y, part.Attributes, new Position16<OamPixels>(FileCopyArrowDefinitions.OriginX, y));
            }
            oam.FinalizeFrame();
            return;
        }
        ushort border = IsCopyPhase
            ? FileSelectLayout.CopyBorderSpritemap
            : IsClearPhase
                ? FileSelectLayout.ClearBorderSpritemap
                : FileSelectLayout.NormalBorderSpritemap;
        ushort borderX = IsClearPhase ? (ushort)124 : (ushort)128;
        DrawMenuSpritemap(border, new(borderX, 16));
        if (ShouldDrawSelectionMissile)
        {
            DrawMenuSpritemap(
                MenuMissileAnimationDefinitions.SpritemapId(missileAnimationFrame),
                GetSelectionMissilePosition());
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
                FileSelectHelmetAnimation.SpritemapId(Math.Min(frame, FileSelectHelmetAnimation.FrameCount - 1)),
                new(100, FileSelectLayout.HelmetY(slot)));
        }
        oam.FinalizeFrame();

    }

    /// <summary>Builds save tilemap.</summary>
    private void BuildSaveTilemap()
    {
        currentPresentationPage = HasAnySave
            ? FileSelectPresentationDefinitions.MainWithDataPage
            : FileSelectPresentationDefinitions.MainEmptyPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        // Native `ClearMenuTilemap` fills every word with character $00F (blank).
        Array.Fill(bg1Tilemap, FileSelectLayout.BlankTile);
        LoadMenuTilemap(FileSelectLayout.SamusDataDestination, FileSelectTilemaps.SamusData);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(0));
        // `$81:A08E-$81:A096` adds one complete $40-byte tilemap row to the slot's
        // energy-field origin before loading NO DATA. The leading blank word in the ROM
        // string then places N at column 15. Using $01BC here would instead begin at column
        // 30, putting N in column 31 and wrapping O DATA onto the following scanline.
        DrawFileSlot(
            saveSlots[0],
            FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.Energy),
            FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.TimeValue));
        // `$81:9F3A` conditionally draws only the numeric HH:MM value. The following
        // `$81:9F3D-$81:9F43` tilemap load is unconditional, so the static TIME caption
        // remains visible even when slot A is empty and its numeric fields are omitted.
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(1));
        DrawFileSlot(
            saveSlots[1],
            FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Energy),
            FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeValue));
        // Slot B repeats the same native split between conditional digits and an
        // unconditional ROM-authored caption (`$81:9F70-$81:9F79`).
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(2));
        DrawFileSlot(
            saveSlots[2],
            FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Energy),
            FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeValue));
        // Slot C's caption is likewise loaded unconditionally at `$81:9FA9-$81:9FAF`.
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
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

    /// <summary>Writes menu digit.</summary>
    private void WriteMenuDigit(int destinationByteOffset, int digit)
    {
        int wordIndex = destinationByteOffset >> 1;
        if ((uint)wordIndex >= bg1Tilemap.Length)
            throw new InvalidDataException("A file-select digit escaped the 32x32 BG1 buffer.");
        bg1Tilemap[wordIndex] = unchecked((ushort)(FileSelectLayout.DigitTileBase + digit));
    }

    /// <summary>Loads menu tilemap.</summary>
    private static void LoadMenuTilemap(int destinationByteOffset, ushort sourcePointer)
    {
        throw new InvalidOperationException(
            "File-select tilemaps require installed menu presentation assets.");
    }

    /// <summary>Draws menu spritemap.</summary>
    private static void DrawMenuSpritemap(ushort spritemapId, Position16<OamPixels> position)
    {
        // `DrawMenuSpritemap` indexes a word-pointer array, so the displayed spritemap ID
        // is doubled before following bank $82. AddOnScreenSpritemap then applies the same
        // `$F1FF | $0E00` attribute replacement as the native r3 argument.
        throw new InvalidOperationException(
            "File-select sprites require installed menu presentation assets.");
    }

    /// <summary>
    /// $81:9DE4 timer step for the selected helmet: each frame lasts the reload duration,
    /// and advancing past the last frame stops the timer at zero on that frame instead.
    /// </summary>
    private void StepSelectedHelmet()
    {
        if (helmetAnimationTimer == 0 || --helmetAnimationTimer != 0)
            return;
        helmetAnimationTimer = mapPresentation?.FileSelect.HelmetFrameDuration ?? FileSelectHelmetAnimation.FrameDuration;
        int next = helmetAnimationFrame + 1;
        if (next >= FileSelectHelmetAnimation.FrameCount)
        {
            helmetAnimationTimer = 0;
            next = FileSelectHelmetAnimation.FrameCount - 1;
        }
        helmetAnimationFrame = next;
    }

    /// <summary>Steps missile animation.</summary>
    private void StepMissileAnimation()
    {
        if (--missileAnimationTimer != 0)
            return;
        missileAnimationFrame = (missileAnimationFrame + 1) & 3;
        missileAnimationTimer = mapPresentation?.FileSelect.CursorFrameDuration ?? MenuMissileAnimationDefinitions.FrameDuration;
    }

    /// <summary>Applies brightness.</summary>
    private void ApplyBrightness(Span<Rgba32> pixels)
    {
        int displayed = Brightness;
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            pixels[pixel] = displayed switch
            {
                >= 15 => color,
                <= 0 => new Rgba32(0, 0, 0, color.A),
                _ => new Rgba32(
                    (byte)(color.R * displayed / 15),
                    (byte)(color.G * displayed / 15),
                    (byte)(color.B * displayed / 15),
                    color.A),
            };
        }
    }

    /// <summary>Stores pending data mode.</summary>
    private FileSelectDataMode pendingDataMode;
    /// <summary>Stores phase after fade in.</summary>
    private FileSelectPhase phaseAfterFadeIn;
    /// <summary>Stores show data management screen.</summary>
    private bool showDataManagementScreen;
    /// <summary>Stores submenu selection.</summary>
    private int submenuSelection;
    /// <summary>Stores operation source slot.</summary>
    private int operationSourceSlot;
    /// <summary>Stores operation destination slot.</summary>
    private int operationDestinationSlot;
    /// <summary>Stores confirmation selection.</summary>
    private int confirmationSelection;
    /// <summary>Stores copy arrow palette timer.</summary>
    private int copyArrowPaletteTimer;

    /// <summary>Reports whether any save slot contains data.</summary>
    private bool HasAnySave => saveSlots.Any(slot => slot is not null);

    /// <summary>Reports whether the main file-selection screen is active.</summary>
    private bool IsMainScreenPhase => !showDataManagementScreen;

    /// <summary>Reports whether the data-management screen is copying a save.</summary>
    private bool IsCopyPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Copy;

    /// <summary>Reports whether the data-management screen is clearing a save.</summary>
    private bool IsClearPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Clear;

    /// <summary>Reports whether file selection is still entering or initializing.</summary>
    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.LoadBackground or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterNmi;

    /// <summary>Reports whether the selection missile is visible in the current phase.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    /// <summary>Steps main menu.</summary>
    private void StepMainMenu(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            MoveMainSelection(down: false);
            QueueCursorSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            MoveMainSelection(down: true);
            QueueCursorSound();
        }

        if ((pressed & SnesButton.B) != 0)
        {
            QueueCursorSound();
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToTitle;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        if (SelectedItem < 3)
        {
            audio?.QueueSound(SoundEffectLibrary1Sounds.FileSelectSwoosh, maximumQueued: 6);
            // `menu_index += 27` enters index 31 and enables only the selected helmet.
            helmetAnimationFrame = 0;
            helmetAnimationTimer = 1;
            Phase = FileSelectPhase.TurnSelectedHelmet;
            return;
        }
        if (SelectedItem is 3 or 4)
        {
            if (!HasAnySave)
                throw new InvalidOperationException("Hidden data-management item became selectable.");
            QueueCursorSound();
            pendingDataMode = SelectedItem == 3 ? FileSelectDataMode.Copy : FileSelectDataMode.Clear;
            Phase = FileSelectPhase.FadeOutToDataManagement;
            return;
        }
        if (SelectedItem == 5)
        {
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToTitle;
            return;
        }
        throw new InvalidDataException($"Invalid file-select main item {SelectedItem}.");
    }

    /// <summary>Moves main selection.</summary>
    private void MoveMainSelection(bool down) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, down);

    /// <summary>Steps data management fade.</summary>
    private void StepDataManagementFade()
    {
        if (Phase is FileSelectPhase.FadeOutToDataManagement or FileSelectPhase.FadeOutToMain)
        {
            screenFade.FadeOut(ref brightness);
            if (!ScreenFade.IsForcedBlank(brightness))
                return;

            if (Phase == FileSelectPhase.FadeOutToDataManagement)
            {
                showDataManagementScreen = true;
                InitializeDataManagementScreen();
            }
            else
            {
                showDataManagementScreen = false;
                BuildSaveTilemap();
                UploadBg1Tilemap();
                phaseAfterFadeIn = FileSelectPhase.Main;
            }
            Phase = FileSelectPhase.FadeInFromDataManagement;
            return;
        }

        screenFade.FadeIn(ref brightness);
        if (brightness == ScreenFade.FullyLit)
            Phase = phaseAfterFadeIn;
    }

    /// <summary>Initializes data management screen.</summary>
    private void InitializeDataManagementScreen()
    {
        submenuSelection = FindFirstNonemptySlot();
        operationSourceSlot = 0;
        operationDestinationSlot = 0;
        confirmationSelection = 0;
        if (pendingDataMode == FileSelectDataMode.Copy)
        {
            BuildCopySourceTilemap();
            phaseAfterFadeIn = FileSelectPhase.CopySelectSource;
        }
        else
        {
            BuildClearSelectionTilemap();
            phaseAfterFadeIn = FileSelectPhase.ClearSelectSlot;
        }
        UploadBg1Tilemap();
    }

    /// <summary>Steps data management.</summary>
    private void StepDataManagement(SnesButton pressed)
    {
        switch (Phase)
        {
            case FileSelectPhase.CopySelectSource:
                StepSlotSelection(pressed, sourceSelection: true);
                break;
            case FileSelectPhase.CopySelectDestination:
                StepCopyDestination(pressed);
                break;
            case FileSelectPhase.CopyConfirm:
                StepCopyArrowPalette();
                StepConfirmation(pressed, copy: true);
                break;
            case FileSelectPhase.ClearSelectSlot:
                StepSlotSelection(pressed, sourceSelection: false);
                break;
            case FileSelectPhase.ClearConfirm:
                StepConfirmation(pressed, copy: false);
                break;
            case FileSelectPhase.CopyCompleted:
            case FileSelectPhase.ClearCompleted:
                if (pressed != SnesButton.None)
                {
                    QueueCursorSound();
                    SelectedItem = saveRam.ReadSelectedSlot();
                    Phase = FileSelectPhase.FadeOutToMain;
                }
                break;
            default:
                throw new InvalidDataException($"Invalid data-management phase {Phase}.");
        }
    }

    /// <summary>Steps slot selection.</summary>
    private void StepSlotSelection(SnesButton pressed, bool sourceSelection)
    {
        int occupiedSlots = 0;
        for (int slot = 0; slot < FileSelectLayout.SaveSlotCount; slot++)
            if (saveSlots[slot] is not null) occupiedSlots |= 1 << slot;
        MoveSubmenuSelection(FileSelectDataNavigation.MoveSourceOrClear(
            submenuSelection, occupiedSlots, pressed));

        if ((pressed & SnesButton.B) != 0 ||
            ((pressed & (SnesButton.Start | SnesButton.A)) != 0 && submenuSelection == 3))
        {
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        QueueCursorSound();
        operationSourceSlot = submenuSelection;
        if (sourceSelection)
        {
            submenuSelection = Enumerable.Range(0, 3)
                .First(slot => slot != operationSourceSlot);
            BuildCopyDestinationTilemap();
            UploadBg1Tilemap();
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            confirmationSelection = 0;
            BuildClearConfirmationTilemap();
            UploadBg1Tilemap();
            Phase = FileSelectPhase.ClearConfirm;
        }
    }

    /// <summary>Steps copy destination.</summary>
    private void StepCopyDestination(SnesButton pressed)
    {
        MoveSubmenuSelection(FileSelectDataNavigation.MoveCopyDestination(
            submenuSelection, operationSourceSlot, pressed));
        if ((pressed & SnesButton.B) != 0)
        {
            QueueCursorSound();
            submenuSelection = operationSourceSlot;
            BuildCopySourceTilemap();
            UploadBg1Tilemap();
            Phase = FileSelectPhase.CopySelectSource;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;
        QueueCursorSound();
        if (submenuSelection == 3)
        {
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        operationDestinationSlot = submenuSelection;
        confirmationSelection = 0;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.InitialPaletteDelay;
        BuildCopyConfirmationTilemap();
        UploadBg1Tilemap();
        Phase = FileSelectPhase.CopyConfirm;
    }

    /// <summary>Steps copy arrow palette.</summary>
    private void StepCopyArrowPalette()
    {
        if (copyArrowPaletteTimer == 0 || --copyArrowPaletteTimer != 0)
            return;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.PaletteDelay;
        CgramPalette arrow = new FrontendPalettes(ppu.Cgram).FileCopyArrow;
        int last = arrow.Count - 1;
        Bgr555 color = arrow[0];
        for (int index = 0; index < last; index++)
            arrow.Write(index, arrow[index + 1]);
        arrow.Write(last, color);
    }

    /// <summary>Steps confirmation.</summary>
    private void StepConfirmation(SnesButton pressed, bool copy)
    {
        if ((pressed & (SnesButton.Up | SnesButton.Down)) != 0)
        {
            confirmationSelection ^= 1;
            QueueCursorSound();
            return;
        }
        if ((pressed & SnesButton.B) != 0)
        {
            QueueCursorSound();
            ReturnFromConfirmation(copy);
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        if (confirmationSelection != 0)
        {
            ReturnFromConfirmation(copy);
            return;
        }

        if (copy)
        {
            saveRam.CopySlot(operationSourceSlot, operationDestinationSlot);
            saveSlots[operationDestinationSlot] = saveRam.ReadSlot(operationDestinationSlot)
                ?? throw new InvalidDataException("Copied SRAM slot failed its copied checksums.");
            BuildCopyCompletedTilemap();
            Phase = FileSelectPhase.CopyCompleted;
        }
        else
        {
            saveRam.ClearSlot(operationSourceSlot);
            saveSlots[operationSourceSlot] = null;
            BuildClearCompletedTilemap();
            Phase = FileSelectPhase.ClearCompleted;
        }
        UploadBg1Tilemap();
        SaveRamChangedThisFrame = true;
    }

    /// <summary>Return from confirmation.</summary>
    private void ReturnFromConfirmation(bool copy)
    {
        submenuSelection = copy ? operationDestinationSlot : operationSourceSlot;
        if (copy)
        {
            BuildCopyDestinationTilemap();
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            BuildClearSelectionTilemap();
            Phase = FileSelectPhase.ClearSelectSlot;
        }
        UploadBg1Tilemap();
    }

    /// <summary>Moves submenu selection.</summary>
    private void MoveSubmenuSelection(int next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

    /// <summary>Builds copy source tilemap.</summary>
    private void BuildCopySourceTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopySourcePage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemaps.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemaps.CopyWhichData,
            FileSelectLayout.CopySourcePromptDestination);
    }

    /// <summary>Builds copy destination tilemap.</summary>
    private void BuildCopyDestinationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopyDestinationPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemaps.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemaps.CopySamusToWhere,
            FileSelectLayout.CopyDestinationPromptDestination);
        bg1Tilemap[FileSelectLayout.CopyDestinationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
    }

    /// <summary>Builds copy confirmation tilemap.</summary>
    private void BuildCopyConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopyConfirmPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemaps.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemaps.CopySamusToSamus,
            FileSelectLayout.CopyConfirmationPromptDestination);
        bg1Tilemap[FileSelectLayout.CopyConfirmationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
        bg1Tilemap[FileSelectLayout.CopyConfirmationDestinationLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationDestinationSlot));
        AddConfirmationText();
    }

    /// <summary>Builds copy completed tilemap.</summary>
    private void BuildCopyCompletedTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopyCompletedPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildCopyConfirmationTilemap();
        currentPresentationPage = FileSelectPresentationDefinitions.CopyCompletedPage;
        LoadMenuTilemap(FileSelectLayout.CopyCompletedDestination, FileSelectTilemaps.CopyCompleted);
    }

    /// <summary>Builds clear selection tilemap.</summary>
    private void BuildClearSelectionTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.ClearSelectionPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemaps.DataClearMode,
            FileSelectLayout.DataModeClearDestination,
            FileSelectTilemaps.ClearWhichData,
            FileSelectLayout.ClearPromptDestination);
    }

    /// <summary>Builds clear confirmation tilemap.</summary>
    private void BuildClearConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.ClearConfirmPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemaps.DataClearMode,
            FileSelectLayout.DataModeClearDestination,
            FileSelectTilemaps.ClearSamus,
            FileSelectLayout.ClearPromptDestination);
        bg1Tilemap[FileSelectLayout.ClearConfirmationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
        AddConfirmationText();
    }

    /// <summary>Builds clear completed tilemap.</summary>
    private void BuildClearCompletedTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.ClearCompletedPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildClearConfirmationTilemap();
        currentPresentationPage = FileSelectPresentationDefinitions.ClearCompletedPage;
        LoadMenuTilemap(FileSelectLayout.DataClearedDestination, FileSelectTilemaps.DataCleared);
        DrawDataManagementSlots();
    }

    /// <summary>Builds data management base.</summary>
    private void BuildDataManagementBase(
        ushort modeLabel,
        int modeDestination,
        ushort promptLabel,
        int promptDestination)
    {
        Array.Fill(bg1Tilemap, FileSelectLayout.BlankTile);
        LoadMenuTilemap(modeDestination, modeLabel);
        LoadMenuTilemap(promptDestination, promptLabel);
        LoadMenuTilemap(FileSelectLayout.ExitDestination, FileSelectTilemaps.Exit);
        DrawDataManagementSlots();
    }

    /// <summary>Draws data management slots.</summary>
    private void DrawDataManagementSlots()
    {
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(0));
        DrawFileSlot(saveSlots[0], FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(1));
        DrawFileSlot(saveSlots[1], FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Label), FileSelectTilemaps.SlotLabel(2));
        DrawFileSlot(saveSlots[2], FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeLabel), FileSelectTilemaps.Time);
    }

    /// <summary>Adds confirmation text.</summary>
    private static void AddConfirmationText()
    {
        LoadMenuTilemap(FileSelectLayout.ConfirmationQuestionDestination, FileSelectTilemaps.IsThisOkay);
        LoadMenuTilemap(FileSelectLayout.ConfirmationYesDestination, FileSelectTilemaps.Yes);
        LoadMenuTilemap(FileSelectLayout.ConfirmationNoDestination, FileSelectTilemaps.No);
    }

    /// <summary>Finds first nonempty slot.</summary>
    private int FindFirstNonemptySlot()
    {
        int slot = Array.FindIndex(saveSlots, save => save is not null);
        return slot >= 0 ? slot : throw new InvalidOperationException(
            "Data-management screen requires at least one nonempty save slot.");
    }

    /// <summary>Gets selection missile position.</summary>
    private Position16<OamPixels> GetSelectionMissilePosition()
    {
        if (mapPresentation is not null)
        {
            bool confirmation = Phase is FileSelectPhase.CopyConfirm or
                FileSelectPhase.ClearConfirm;
            int selected = IsMainScreenPhase
                ? SelectedItem
                : confirmation ? confirmationSelection : submenuSelection;
            Position16<OamPixels> point = mapPresentation.FileSelect.CursorPosition(
                IsMainScreenPhase, confirmation, selected);
            return point;
        }
        if (!showDataManagementScreen)
            return new(14, FileSelectLayout.MainSelectionY(SelectedItem));
        if (Phase is FileSelectPhase.CopyConfirm or FileSelectPhase.ClearConfirm)
            return new(94, confirmationSelection == 0 ? (ushort)184 : (ushort)208);
        return new(22, FileSelectLayout.DataSelectionY(submenuSelection));
    }

    /// <summary>Upload bg1 tilemap.</summary>
    private void UploadBg1Tilemap() =>
        new MenuVram(ppu.Vram).Bg1Tilemap.Write(bg1Tilemap);

    /// <summary>Queues cursor sound.</summary>
    private void QueueCursorSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);

    /// <summary>Rebinds host-owned file-select visuals after debugger-state restoration.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        if (catalog is null)
            throw new InvalidOperationException("File-select requires installed map presentation assets.");
        mapPresentation = catalog;
        saveRam.BindMapPresentation(catalog);
        ppu.BindWorldArtwork(catalog.WorldArtwork);
        ppu.BindMapTiles(catalog.Tiles);
        ppu.BindMapSprites(catalog.Sprites);
        ppu.BindMapPalettes(catalog.Palettes);
        catalog.FileSelect.LoadBackground(ppu.Vram);
        RebuildInstalledPresentationPage();
        UploadBg1Tilemap();
    }

    /// <summary>Rebuild installed presentation page.</summary>
    private void RebuildInstalledPresentationPage()
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        presentation.CopyPage(currentPresentationPage, bg1Tilemap);
        bool main = currentPresentationPage is
            FileSelectPresentationDefinitions.MainWithDataPage or
            FileSelectPresentationDefinitions.MainEmptyPage;
        for (int slot = 0; slot < saveSlots.Length; slot++)
            DrawInstalledSlot(saveSlots[slot], presentation.Slot(!main, slot));

        switch (currentPresentationPage)
        {
            case FileSelectPresentationDefinitions.CopyDestinationPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyDestinationSourceAnchor),
                    operationSourceSlot);
                break;
            case FileSelectPresentationDefinitions.CopyConfirmPage:
            case FileSelectPresentationDefinitions.CopyCompletedPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyConfirmSourceAnchor),
                    operationSourceSlot);
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.CopyConfirmDestinationAnchor),
                    operationDestinationSlot);
                break;
            case FileSelectPresentationDefinitions.ClearConfirmPage:
            case FileSelectPresentationDefinitions.ClearCompletedPage:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectPresentationDefinitions.ClearConfirmSourceAnchor),
                    operationSourceSlot);
                break;
        }
    }

    /// <summary>Draws installed slot.</summary>
    private void DrawInstalledSlot(
        SuperMetroid.Core.Game.SuperMetroidSaveSlot? slot,
        FileSelectSlotFieldDocument layout)
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        if (slot is null)
        {
            presentation.ApplyPatch(bg1Tilemap,
                FileSelectPresentationDefinitions.NoDataPatch, layout.NoDataAnchor);
            return;
        }

        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPresentationDefinitions.EnergyPatch, layout.EnergyAnchor);
        int health = slot.Health % 100;
        presentation.WriteDigit(bg1Tilemap, layout.HealthAnchor, 0, health / 10);
        presentation.WriteDigit(bg1Tilemap, layout.HealthAnchor, 1, health % 10);
        int tankCell = (layout.EnergyAnchor.Y + FileSelectEnergyTankDefinitions.RowOffset) *
            FileSelectPresentationDefinitions.Width + layout.EnergyAnchor.X + FileSelectEnergyTankDefinitions.ColumnOffset;
        int rowRemaining = FileSelectEnergyTankDefinitions.FirstRowCount;
        int filledTanks = slot.Health / FileSelectEnergyTankDefinitions.EnergyPerTank;
        ushort palette = (ushort)(bg1Tilemap[layout.HealthAnchor.Y * FileSelectPresentationDefinitions.Width +
            layout.HealthAnchor.X] & FileSelectEnergyTankDefinitions.PaletteMask);
        for (int tank = 0; tank < slot.MaxHealth / FileSelectEnergyTankDefinitions.EnergyPerTank; tank++)
        {
            if ((uint)tankCell >= bg1Tilemap.Length)
                throw new InvalidDataException("File-select energy tanks escaped the menu tilemap.");
            bg1Tilemap[tankCell++] = (ushort)(palette | (tank < filledTanks
                ? FileSelectEnergyTankDefinitions.FilledTile : FileSelectEnergyTankDefinitions.EmptyTile));
            if (--rowRemaining == 0)
            {
                tankCell -= FileSelectEnergyTankDefinitions.RowRewindCells;
                rowRemaining = FileSelectEnergyTankDefinitions.WrappedRowCount;
            }
        }
        int hours = Math.Min(slot.GameTimeHours, (ushort)99);
        int minutes = Math.Min(slot.GameTimeMinutes, (ushort)99);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 0, hours / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 1, hours % 10);
        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPresentationDefinitions.TimeColonPatch,
            new(checked((ushort)(layout.TimeValueAnchor.X + 2)), layout.TimeValueAnchor.Y));
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 3, minutes / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 4, minutes % 10);
    }
}

/// <summary>Debugger-facing file-select phases, including combined native data-management steps and explicit NMI-wait continuations; enum ordinals are not native menu indices.</summary>
public enum FileSelectPhase
{
    /// <summary>$81:A058, native index $03: brighten the initialized main screen until its brightness reaches 15.</summary>
    FadeIn,
    /// <summary>$81:A1C2, native index $04: accept slot, Copy, Clear and Exit navigation; data-management items are skipped when all slots are empty.</summary>
    Main,
    /// <summary>$81:94EE, native indices $05/$13: fade the main screen to forced blank, then initialize the selected Copy or Clear page.</summary>
    FadeOutToDataManagement,
    /// <summary>Brighten the newly built Copy, Clear or main page; combines native submenu fade-ins at $81:9532 and return-to-main fade-ins at $81:951E.</summary>
    FadeInFromDataManagement,
    /// <summary>$81:96C2, native index $08: choose a checksummed, occupied source slot for copying, or leave through Exit/B.</summary>
    CopySelectSource,
    /// <summary>$81:9813, native index $0A: choose any destination slot other than the source, including an occupied slot that will be overwritten.</summary>
    CopySelectDestination,
    /// <summary>$81:9984, native index $0C: animate the copy arrow and accept Yes/No; accepting Yes also performs the native $0D copy operation in this phase.</summary>
    CopyConfirm,
    /// <summary>$81:9AFA, native index $0E: display the completed copy without a selection missile and wait for a newly pressed button before returning to main.</summary>
    CopyCompleted,
    /// <summary>$81:9B64, native index $16: choose a checksummed, occupied slot to clear, or leave through Exit/B.</summary>
    ClearSelectSlot,
    /// <summary>$81:9C36, native index $18: accept Yes/No for the chosen slot; accepting Yes also performs the native $19 SRAM clear operation in this phase.</summary>
    ClearConfirm,
    /// <summary>$81:9D26, native index $1A: display the completed clear without a selection missile and wait for a newly pressed button before returning to main.</summary>
    ClearCompleted,
    /// <summary>$81:94F4, native indices $0F/$1B: fade the data-management page to forced blank, then rebuild main from the updated slot contents.</summary>
    FadeOutToMain,
    /// <summary>$81:9D77, native index $1F: turn only the selected slot's helmet; a new Start/A press can end the animation early.</summary>
    TurnSelectedHelmet,
    /// <summary>$81:94A3, native index $20: fade the selected helmet and main page to forced blank, then signal the game-options handoff.</summary>
    FadeOutToOptions,
    /// <summary>$81:94D5, native index $21: fade to forced blank after Exit/B, then request the host's reset path corresponding to native SoftReset.</summary>
    FadeOutToTitle,
    /// <summary>$81:944E, native index $00: finish entry fade-out and establish forced blank before the dispatch's internal NMI wait.</summary>
    EnterBlankScreen,
    /// <summary>Continuation of native index $00 after its internal NMI wait; the next update advances to background loading without another main-loop RNG call.</summary>
    EnterBlankScreenAfterNmi,
    /// <summary>$81:9E93, native index $01: represent the Zebes/starfield BG2 transfer stage; installed graphics were loaded atomically by construction.</summary>
    LoadBackground,
    /// <summary>$81:9ED6, native index $02: set both fade timing words to one and release forced blank before its internal NMI wait.</summary>
    InitializeMain,
    /// <summary>Continuation of native index $02 after its internal NMI wait; advance to main-screen fade-in without another main-loop RNG call.</summary>
    InitializeMainAfterNmi,
}
