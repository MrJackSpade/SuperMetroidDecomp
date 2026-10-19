using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

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
    /// <summary>Receives menu sound requests when audio is available.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Reads and mutates the cartridge's checksummed three-slot SRAM layout.</summary>
    private readonly SaveSlotStore saves;
    /// <summary>The menu screen file select draws: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Tracks button edges used by native menu navigation and confirmation.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Host-owned file-select artwork rebound after debugger-state restoration.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Staging buffer for the complete 32-by-32 BG1 menu tilemap.</summary>
    internal readonly ushort[] bg1Tilemap = new ushort[32 * 32];
    /// <summary>Decoded slots whose null entries failed or lacked native SRAM checksums.</summary>
    private readonly SuperMetroidSaveSlot?[] saveSlots = new SuperMetroidSaveSlot?[3];
    /// <summary>Updates remaining before the selection missile advances its sprite frame.</summary>
    private int missileAnimationTimer = 1;
    /// <summary>Current frame of the four-frame selection missile cycle.</summary>
    private int missileAnimationFrame;
    /// <summary>Updates remaining in the selected helmet's current turn frame.</summary>
    private int helmetAnimationTimer;
    /// <summary>Selected helmet turn frame retained through the options fade.</summary>
    internal int helmetAnimationFrame;
    /// <summary>$51 INIDISP byte: forced-blank bit plus brightness nibble.</summary>
    private int brightness;
    /// <summary>Advances the native brightness timing for entry and submenu transitions.</summary>
    private readonly ScreenFade screenFade = new();
    /// <summary>Installed page identifier rebuilt after save data or debugger bindings change.</summary>
    private string currentPresentationPage = FileSelectPresentationDefinitions.MainEmptyPage;

    /// <summary>Performs menu indices zero through two, including every native ROM transfer.</summary>
    public FileSelectMenuState(SaveSlotStore saves, CartridgeAudioState? audio = null,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.audio = audio;
        this.mapPresentation = mapPresentation;
        if (mapPresentation is null)
            throw new InvalidOperationException("File select requires installed menu presentation assets.");
        screen = new MenuScreen(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites);
        mapPresentation.FileSelect.LoadBackground(screen.Tilemaps);

        this.saves = Ensure.NotNull(saves);
        for (int slot = 0; slot < saveSlots.Length; slot++)
            saveSlots[slot] = saves.ReadSlot(slot);
        SelectedItem = saves.ReadSelectedSlot();

        BuildSaveTilemap();
        bg1Tilemap.CopyTo(screen.Tilemaps.Bg1);

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

    /// <summary>
    /// True when the next update resumes a native dispatch after its internal NMI wait
    /// rather than entering a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesAfterNmiWait => Phase is
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.InitializeMainAfterNmi;
    /// <summary>Advances one native menu frame from a raw SNES controller word.</summary>
    public void Step(SnesButton controllerInput)
    {
        SaveRamChangedThisFrame = false;
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressed;

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

    /// <summary>Captures file-menu display state after simulation-owned OAM preparation.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return MenuRenderSnapshotCapture.Capture(screen, 0, ScreenFade.Displayed(brightness));
    }

    /// <summary>Builds the current border, missile, helmet, and optional copy-arrow sprite entries from menu phase.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        bool mainScreen = IsMainScreenPhase;
        FileSelectPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "File-select sprites require installed menu presentation assets.")).FileSelect;
        string borderName = IsCopyPhase
            ? FileSelectPresentationDefinitions.CopyBorder
            : IsClearPhase
                ? FileSelectPresentationDefinitions.ClearBorder
                : FileSelectPresentationDefinitions.MainBorder;
        presentation.DrawBorder(sprites, borderName);
        if (ShouldDrawSelectionMissile)
            presentation.DrawCursor(sprites, missileAnimationFrame, GetSelectionMissilePosition());
        for (int slot = 0; mainScreen && slot < 3; slot++)
        {
            // Menu index 32 fades with the completed (or Start-shortened) helmet turn
            // still resident in OAM. Resetting to the first frame during the fade produces
            // a one-frame-visible head snap immediately before loading.
            int frame = slot == SelectedItem && Phase is
                    FileSelectPhase.TurnSelectedHelmet or FileSelectPhase.FadeOutToOptions
                ? helmetAnimationFrame
                : 0;
            presentation.DrawHelmet(sprites, Math.Min(frame, FileSelectHelmetAnimation.FrameCount - 1), slot);
        }
        if (Phase == FileSelectPhase.CopyConfirm)
        {
            (int shape, ushort y) = FileCopyArrowDefinitions.Select(operationSourceSlot, operationDestinationSlot);
            foreach (CompiledSpritePart part in FileCopyArrowDefinitions.Parts(shape))
                sprites.AddPart(part.X, part.Y, part.Attributes, new Position16<OamPixels>(FileCopyArrowDefinitions.OriginX, y));
        }
        sprites.FinishFrame();
    }

    /// <summary>Builds the main page from decoded slots, hiding Copy and Clear when every slot is empty.</summary>
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
        LoadMenuTilemap(FileSelectLayout.SamusDataDestination, FileSelectTilemapDefinitions.SamusData);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(0));
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
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(0, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(1));
        DrawFileSlot(
            saveSlots[1],
            FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.Energy),
            FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeValue));
        // Slot B repeats the same native split between conditional digits and an
        // unconditional ROM-authored caption (`$81:9F70-$81:9F79`).
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(1, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(2));
        DrawFileSlot(
            saveSlots[2],
            FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.Energy),
            FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeValue));
        // Slot C's caption is likewise loaded unconditionally at `$81:9FA9-$81:9FAF`.
        LoadMenuTilemap(FileSelectLayout.MainSlotDestination(2, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
        if (saveSlots.Any(slot => slot is not null))
        {
            // Native index 16 exposes both data-management entries only when at least one
            // checksummed slot exists; an all-empty SRAM image skips directly from C to Exit.
            LoadMenuTilemap(FileSelectLayout.DataCopyDestination, FileSelectTilemapDefinitions.DataCopy);
            LoadMenuTilemap(FileSelectLayout.DataClearDestination, FileSelectTilemapDefinitions.DataClear);
        }
        LoadMenuTilemap(FileSelectLayout.ExitDestination, FileSelectTilemapDefinitions.Exit);
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
                FileSelectTilemapDefinitions.NoData);
            return;
        }

        LoadMenuTilemap(energyOrigin, FileSelectTilemapDefinitions.Energy);
        int healthRemainder = slot.Health % 100;
        WriteMenuDigit(energyOrigin + 66, healthRemainder / 10);
        WriteMenuDigit(energyOrigin + 68, healthRemainder % 10);

        int hours = Math.Min(slot.GameTimeHours, (ushort)99);
        int minutes = Math.Min(slot.GameTimeMinutes, (ushort)99);
        WriteMenuDigit(timeOrigin, hours / 10);
        WriteMenuDigit(timeOrigin + 2, hours % 10);
        LoadMenuTilemap(timeOrigin + 4, FileSelectTilemapDefinitions.TimeColon);
        WriteMenuDigit(timeOrigin + 6, minutes / 10);
        WriteMenuDigit(timeOrigin + 8, minutes % 10);
    }

    /// <summary>Writes one decimal glyph to BG1 and rejects an address outside its 32-by-32 buffer.</summary>
    private void WriteMenuDigit(int destinationByteOffset, int digit)
    {
        int wordIndex = destinationByteOffset >> 1;
        if ((uint)wordIndex >= bg1Tilemap.Length)
            throw new InvalidDataException("A file-select digit escaped the 32x32 BG1 buffer.");
        bg1Tilemap[wordIndex] = unchecked((ushort)(FileSelectLayout.DigitTileBase + digit));
    }

    /// <summary>Rejects the obsolete ROM tilemap path; file-select pages require installed presentation assets.</summary>
    private static void LoadMenuTilemap(int destinationByteOffset, ushort sourcePointer)
    {
        throw new InvalidOperationException(
            "File-select tilemaps require installed menu presentation assets.");
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

    /// <summary>Advances the four-frame missile cursor when its presentation-defined delay expires.</summary>
    private void StepMissileAnimation()
    {
        if (--missileAnimationTimer != 0)
            return;
        missileAnimationFrame = (missileAnimationFrame + 1) & 3;
        missileAnimationTimer = mapPresentation?.FileSelect.CursorFrameDuration ?? MenuMissileAnimationDefinitions.FrameDuration;
    }

    /// <summary>Copy or Clear mode selected on the main page before its fade finishes.</summary>
    private FileSelectDataMode pendingDataMode;
    /// <summary>Submenu phase entered once the newly built page reaches full brightness.</summary>
    private FileSelectPhase phaseAfterFadeIn;
    /// <summary>Chooses submenu border and cursor layout during cross-page fade phases.</summary>
    private bool showDataManagementScreen;
    /// <summary>Selected slot or Exit row in source, destination, and clear menus.</summary>
    private int submenuSelection;
    /// <summary>Source slot retained while choosing a copy destination or confirming a clear.</summary>
    private int operationSourceSlot;
    /// <summary>Copy target retained through confirmation and completion pages.</summary>
    private int operationDestinationSlot;
    /// <summary>Yes or No row selected on the copy or clear confirmation page.</summary>
    private int confirmationSelection;
    /// <summary>Updates remaining before the copy arrow rotates its CGRAM colors.</summary>
    private int copyArrowPaletteTimer;

    /// <summary>Controls whether the main page exposes Copy and Clear navigation.</summary>
    private bool HasAnySave => saveSlots.Any(slot => slot is not null);

    /// <summary>Chooses main-page border, helmets, and missile coordinates, including fades.</summary>
    private bool IsMainScreenPhase => !showDataManagementScreen;

    /// <summary>Chooses the Copy border while its submenu remains visible.</summary>
    private bool IsCopyPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Copy;

    /// <summary>Chooses the Clear border while its submenu remains visible.</summary>
    private bool IsClearPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Clear;

    /// <summary>Suppresses cursor animation before the initialized main page begins fading in.</summary>
    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.LoadBackground or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterNmi;

    /// <summary>Hides the selection missile during entry and on completed data-operation pages.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    /// <summary>Handles slot selection, hidden data-management rows, and options or title handoffs.</summary>
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

    /// <summary>Moves through main-page entries while skipping Copy and Clear for empty SRAM.</summary>
    private void MoveMainSelection(bool down) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, down);

    /// <summary>Swaps main and data pages only at forced blank, then enters the saved phase after fade-in.</summary>
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

    /// <summary>Starts Copy or Clear on the first occupied slot and builds the corresponding page.</summary>
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

    /// <summary>Dispatches Copy and Clear selection, confirmation, and completed-page input.</summary>
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
                    SelectedItem = saves.ReadSelectedSlot();
                    Phase = FileSelectPhase.FadeOutToMain;
                }
                break;
            default:
                throw new InvalidDataException($"Invalid data-management phase {Phase}.");
        }
    }

    /// <summary>Chooses an occupied source or clear slot, then opens destination or confirmation.</summary>
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

    /// <summary>Chooses any slot other than the copy source, preserving B navigation back to source selection.</summary>
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

    /// <summary>Rotates the copy arrow palette colors when its native CGRAM animation timer expires.</summary>
    private void StepCopyArrowPalette()
    {
        if (copyArrowPaletteTimer == 0 || --copyArrowPaletteTimer != 0)
            return;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.PaletteDelay;
        CgramPalette arrow = new FrontendPalettes(screen.Palette).FileCopyArrow;
        int last = arrow.Count - 1;
        Bgr555 color = arrow[0];
        for (int index = 0; index < last; index++)
            arrow.Write(index, arrow[index + 1]);
        arrow.Write(last, color);
    }

    /// <summary>Accepts Yes or No and mutates checksummed SRAM only after explicit Yes confirmation.</summary>
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
            saves.CopySlot(operationSourceSlot, operationDestinationSlot);
            saveSlots[operationDestinationSlot] = saves.ReadSlot(operationDestinationSlot)
                ?? throw new InvalidDataException("Copied SRAM slot failed its copied checksums.");
            BuildCopyCompletedTilemap();
            Phase = FileSelectPhase.CopyCompleted;
        }
        else
        {
            saves.ClearSlot(operationSourceSlot);
            saveSlots[operationSourceSlot] = null;
            BuildClearCompletedTilemap();
            Phase = FileSelectPhase.ClearCompleted;
        }
        UploadBg1Tilemap();
        SaveRamChangedThisFrame = true;
    }

    /// <summary>Restores the prior source or destination selection page after B or No.</summary>
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

    /// <summary>Updates a changed submenu row and queues one cursor sound.</summary>
    private void MoveSubmenuSelection(int next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

    /// <summary>Builds the Copy page that asks which occupied save to use as source.</summary>
    private void BuildCopySourceTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopySourcePage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemapDefinitions.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemapDefinitions.CopyWhichData,
            FileSelectLayout.CopySourcePromptDestination);
    }

    /// <summary>Builds the destination prompt and inserts the chosen source slot letter.</summary>
    private void BuildCopyDestinationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopyDestinationPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemapDefinitions.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemapDefinitions.CopySamusToWhere,
            FileSelectLayout.CopyDestinationPromptDestination);
        bg1Tilemap[FileSelectLayout.CopyDestinationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
    }

    /// <summary>Builds the Yes/No prompt with both source and destination slot letters.</summary>
    private void BuildCopyConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.CopyConfirmPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemapDefinitions.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemapDefinitions.CopySamusToSamus,
            FileSelectLayout.CopyConfirmationPromptDestination);
        bg1Tilemap[FileSelectLayout.CopyConfirmationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
        bg1Tilemap[FileSelectLayout.CopyConfirmationDestinationLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationDestinationSlot));
        AddConfirmationText();
    }

    /// <summary>Displays the completed copy page after SRAM and decoded slots have been updated.</summary>
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
        LoadMenuTilemap(FileSelectLayout.CopyCompletedDestination, FileSelectTilemapDefinitions.CopyCompleted);
    }

    /// <summary>Builds the Clear page that asks which occupied slot to erase.</summary>
    private void BuildClearSelectionTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.ClearSelectionPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemapDefinitions.DataClearMode,
            FileSelectLayout.DataModeClearDestination,
            FileSelectTilemapDefinitions.ClearWhichData,
            FileSelectLayout.ClearPromptDestination);
    }

    /// <summary>Builds the erase Yes/No prompt with the selected source slot letter.</summary>
    private void BuildClearConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPresentationDefinitions.ClearConfirmPage;
        if (mapPresentation is not null)
        {
            RebuildInstalledPresentationPage();
            return;
        }
        BuildDataManagementBase(
            FileSelectTilemapDefinitions.DataClearMode,
            FileSelectLayout.DataModeClearDestination,
            FileSelectTilemapDefinitions.ClearSamus,
            FileSelectLayout.ClearPromptDestination);
        bg1Tilemap[FileSelectLayout.ClearConfirmationSourceLetterDestination / 2] =
            unchecked((ushort)(FileSelectLayout.SamusLetterTileBase + operationSourceSlot));
        AddConfirmationText();
    }

    /// <summary>Displays the cleared page with refreshed slot data after SRAM is erased.</summary>
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
        LoadMenuTilemap(FileSelectLayout.DataClearedDestination, FileSelectTilemapDefinitions.DataCleared);
        DrawDataManagementSlots();
    }

    /// <summary>Clears BG1 and draws the submenu mode, prompt, Exit row, and current save-slot summaries.</summary>
    private void BuildDataManagementBase(
        ushort modeLabel,
        int modeDestination,
        ushort promptLabel,
        int promptDestination)
    {
        Array.Fill(bg1Tilemap, FileSelectLayout.BlankTile);
        LoadMenuTilemap(modeDestination, modeLabel);
        LoadMenuTilemap(promptDestination, promptLabel);
        LoadMenuTilemap(FileSelectLayout.ExitDestination, FileSelectTilemapDefinitions.Exit);
        DrawDataManagementSlots();
    }

    /// <summary>Draws all three slot labels, energy values, and time captions on a submenu page.</summary>
    private void DrawDataManagementSlots()
    {
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(0));
        DrawFileSlot(saveSlots[0], FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(0, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(1));
        DrawFileSlot(saveSlots[1], FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(1, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Label), FileSelectTilemapDefinitions.SlotLabel(2));
        DrawFileSlot(saveSlots[2], FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.Energy), FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeValue));
        LoadMenuTilemap(FileSelectLayout.DataSlotDestination(2, FileSelectSlotField.TimeLabel), FileSelectTilemapDefinitions.Time);
    }

    /// <summary>Adds the native question and Yes/No labels to a confirmation page.</summary>
    private static void AddConfirmationText()
    {
        LoadMenuTilemap(FileSelectLayout.ConfirmationQuestionDestination, FileSelectTilemapDefinitions.IsThisOkay);
        LoadMenuTilemap(FileSelectLayout.ConfirmationYesDestination, FileSelectTilemapDefinitions.Yes);
        LoadMenuTilemap(FileSelectLayout.ConfirmationNoDestination, FileSelectTilemapDefinitions.No);
    }

    /// <summary>Finds the initial data-management selection and rejects entry with no occupied slot.</summary>
    private int FindFirstNonemptySlot()
    {
        int slot = Array.FindIndex(saveSlots, save => save is not null);
        return slot >= 0 ? slot : throw new InvalidOperationException(
            "Data-management screen requires at least one nonempty save slot.");
    }

    /// <summary>Places the cursor on the active main, slot, or confirmation row for installed or ROM layouts.</summary>
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

    /// <summary>Copies the staged BG1 menu page into menu VRAM.</summary>
    private void UploadBg1Tilemap() =>
        bg1Tilemap.CopyTo(screen.Tilemaps.Bg1);

    /// <summary>Queues the menu cursor sound with the native six-request limit.</summary>
    private void QueueCursorSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);

    /// <summary>Rebinds host-owned file-select visuals after debugger-state restoration.</summary>
    internal void BindMapPresentation([NotNull] AreaMapPresentationCatalog? catalog)
    {
        if (catalog is null)
            throw new InvalidOperationException("File-select requires installed map presentation assets.");
        mapPresentation = catalog;
        screen.BindWorldArtwork(catalog.WorldArtwork);
        screen.BindMapTiles(catalog.Tiles);
        screen.BindMapSprites(catalog.Sprites);
        screen.BindMapPalettes(catalog.Palettes);
        catalog.FileSelect.LoadBackground(screen.Tilemaps);
        RebuildInstalledPresentationPage();
        UploadBg1Tilemap();
    }

    /// <summary>Copies the installed page and overlays live save data and dynamic source/destination letters.</summary>
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

    /// <summary>Overlays NO DATA or the slot's health, energy tanks, and clamped time onto an installed page.</summary>
    private void DrawInstalledSlot(
        SuperMetroid.Core.Saves.SuperMetroidSaveSlot? slot,
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
