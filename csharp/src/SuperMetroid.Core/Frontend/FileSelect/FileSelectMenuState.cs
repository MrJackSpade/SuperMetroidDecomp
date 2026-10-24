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
public sealed class FileSelectMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("File select requires the installed sprite sheets.");
        screen.BindScreenTiles(sheets);
    }

    /// <summary>The installed sprite sheets the borders, cursor, helmets and copy arrows draw from; artwork, never saved.</summary>
    [NonSerialized] private InstalledSpriteSheets spriteSheets;

    /// <summary>Receives menu sound requests when audio is available.</summary>
    private readonly AudioQueues? audio;
    /// <summary>Reads and mutates the cartridge's checksummed three-slot SRAM layout.</summary>
    private readonly SaveSlotStore saves;
    /// <summary>The menu screen file select draws: its artwork, tilemaps, palette and sprite entries.</summary>
    internal readonly MenuScreen screen;
    /// <summary>Tracks button edges used by native menu navigation and confirmation.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Host-owned file-select artwork rebound after debugger-state restoration.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Decoded slots whose null entries failed or lacked native SRAM checksums.</summary>
    private readonly SuperMetroidSaveSlot?[] saveSlots = new SuperMetroidSaveSlot?[3];
    /// <summary>The selection missile's frame cycle.</summary>
    private readonly MenuMissileAnimation missile = new();
    /// <summary>Updates remaining in the selected helmet's current turn frame.</summary>
    private int helmetAnimationTimer;
    /// <summary>Selected helmet turn frame retained through the options fade.</summary>
    internal int HelmetAnimationFrame { get; private set; }
    /// <summary>How bright the file-select screen is; it starts blanked and fades in and out between pages.</summary>
    private ScreenBrightness brightness;
    /// <summary>Advances the native brightness timing for entry and submenu transitions.</summary>
    private readonly ScreenFade screenFade = new();
    /// <summary>Installed page identifier rebuilt after save data or debugger bindings change.</summary>
    private FileSelectPage currentPresentationPage = FileSelectPage.MainEmpty;

    /// <summary>Performs menu indices zero through two, including every native ROM transfer.</summary>
    public FileSelectMenuState(SaveSlotStore saves, AudioQueues? audio = null,
        AreaMapPresentationCatalog? mapPresentation = null, InstalledSpriteSheets? spriteSheets = null)
    {
        this.audio = audio;
        this.mapPresentation = mapPresentation;
        if (mapPresentation is null)
            throw new InvalidOperationException("File select requires installed menu presentation assets.");
        screen = new MenuScreen(mapPresentation.Tiles, mapPresentation.Palettes,
            mapPresentation.WorldArtwork, mapPresentation.Sprites, spriteSheets);
        BindSpriteSheets(spriteSheets);
        mapPresentation.FileSelect.LoadBackground(screen.Tilemaps);

        this.saves = Ensure.NotNull(saves);
        for (int slot = 0; slot < saveSlots.Length; slot++)
            saveSlots[slot] = saves.ReadSlot(slot);
        SelectedItem = FileSelectItemRows.ForSaveSlot(saves.ReadSelectedSlot());

        BuildSaveTilemap();

        // Host-side loading above is atomic, but the native dispatches are not: indices
        // zero and two each wait for an NMI inside their dispatch before returning.
        Phase = FileSelectPhase.EnterBlankScreen;
        brightness = ScreenBrightness.Blanked;
    }

    /// <summary>Native main-menu selection: slots A-C, Copy, Clear, or Exit.</summary>
    public FileSelectItem SelectedItem { get; private set; }

    /// <summary>Current translated native menu phase.</summary>
    public FileSelectPhase Phase { get; private set; }

    /// <summary>True once a selected slot, empty or occupied, finishes its helmet turn and fade-out, requesting the game-options screen.</summary>
    public bool NewGameRequested { get; private set; }

    /// <summary>Selected physical SRAM slot, restricted to A-C once a game is requested; a command row reads as slot C.</summary>
    public int SelectedSaveSlot =>
        FileSelectItemRows.TryGetSaveSlot(SelectedItem, out int slot) ? slot : FileSelectLayout.SaveSlotCount - 1;

    /// <summary>Whether the selected A-C slot passed either native checksum directory.</summary>
    public bool SelectedSlotContainsSave =>
        FileSelectItemRows.TryGetSaveSlot(SelectedItem, out int slot) && saveSlots[slot] is not null;

    /// <summary>True when Exit/B completes the native fade back toward reset.</summary>
    public bool TitleRequested { get; private set; }

    /// <summary>True only on the frame in which Copy or Clear mutated cartridge SRAM.</summary>
    public bool SaveRamChangedThisFrame { get; private set; }

    /// <summary>
    /// True when the next update resumes a native dispatch after its internal NMI wait
    /// rather than entering a new main-loop dispatch, so the main loop's RNG call is absent.
    /// </summary>
    internal bool ResumesAfterFrameWait => Phase is
        FileSelectPhase.EnterBlankScreenAfterFrameWait or FileSelectPhase.InitializeMainAfterFrameWait;
    /// <summary>Advances one native menu frame from a raw SNES controller word.</summary>
    public void Step(SnesButton controllerInput)
    {
        SaveRamChangedThisFrame = false;
        controller.Latch(controllerInput);
        SnesButton pressed = controller.NewlyPressed;

        if (!IsEntryPhase)
            missile.Step(mapPresentation?.FileSelect.CursorFrameDuration);
        switch (Phase)
        {
            case FileSelectPhase.EnterBlankScreen:
                // $81:944E: finish any fade-out, then SetForceBlankAndWaitForNMI.
                screenFade.FadeOut(ref brightness);
                if (brightness.Level != 0)
                    break;
                brightness = ScreenBrightness.Blanked;
                Phase = FileSelectPhase.EnterBlankScreenAfterFrameWait;
                break;

            case FileSelectPhase.EnterBlankScreenAfterFrameWait:
                Phase = FileSelectPhase.LoadBackground;
                break;

            case FileSelectPhase.LoadBackground:
                // $81:9E93 queues the Zebes/stars BG2 tilemap.
                Phase = FileSelectPhase.InitializeMain;
                break;

            case FileSelectPhase.InitializeMain:
                // $81:9ED6 sets both fade words to one, then ClearForceBlankAndWaitForNMI.
                screenFade.SetTiming(1, 1);
                brightness = brightness.Unblanked();
                Phase = FileSelectPhase.InitializeMainAfterFrameWait;
                break;

            case FileSelectPhase.InitializeMainAfterFrameWait:
                Phase = FileSelectPhase.FadeIn;
                break;

            case FileSelectPhase.FadeIn:
                // $81:A058 index three.
                screenFade.FadeIn(ref brightness);
                if (brightness == ScreenBrightness.FullyLit)
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
                    (HelmetAnimationFrame == FileSelectHelmetAnimation.FrameCount - 1 && helmetAnimationTimer == 0))
                    Phase = FileSelectPhase.FadeOutToOptions;
                break;

            case FileSelectPhase.FadeOutToOptions:
                screenFade.FadeOut(ref brightness);
                if (brightness.IsBlanked)
                    NewGameRequested = true;
                break;

            case FileSelectPhase.FadeOutToTitle:
                screenFade.FadeOut(ref brightness);
                if (brightness.IsBlanked)
                    TitleRequested = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FileSelectPhase)} {Phase}.");
        }
    }

    /// <summary>Captures file-menu display state after simulation-owned OAM preparation.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        return MenuRenderSnapshotCapture.Capture(screen, 0, checked((byte)brightness.Level));
    }

    /// <summary>Builds the current border, missile, helmet, and optional copy-arrow sprite entries from menu phase.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Objects;
        sprites.BeginFrame();
        bool mainScreen = IsMainScreenPhase;
        FileSelectPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "File-select sprites require installed menu presentation assets.")).FileSelect;
        FileSelectBorder borderName = IsCopyPhase
            ? FileSelectBorder.Copy
            : IsClearPhase
                ? FileSelectBorder.Clear
                : FileSelectBorder.Main;
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.FileSelect);
        presentation.DrawBorder(sprites, sheet, borderName);
        if (ShouldDrawSelectionMissile)
            FileSelectPresentation.DrawCursor(sprites, sheet, missile.Frame, GetSelectionMissilePosition(presentation));
        for (int slot = 0; mainScreen && slot < 3; slot++)
        {
            // Menu index 32 fades with the completed (or Start-shortened) helmet turn
            // still resident in OAM. Resetting to the first frame during the fade produces
            // a one-frame-visible head snap immediately before loading.
            int frame = FileSelectItemRows.ForSaveSlot(slot) == SelectedItem && Phase is
                    FileSelectPhase.TurnSelectedHelmet or FileSelectPhase.FadeOutToOptions
                ? HelmetAnimationFrame
                : 0;
            presentation.DrawHelmet(sprites, sheet, Math.Min(frame, FileSelectHelmetAnimation.FrameCount - 1), slot);
        }
        if (Phase == FileSelectPhase.CopyConfirm)
        {
            (int shape, int y) = FileCopyArrowDefinitions.Select(operationSourceSlot, copyTargetSlot);
            sprites.AddImage(sheet.Frame(FileCopyArrowDefinitions.FrameName(shape)),
                new Position16<OamPixels>(FileCopyArrowDefinitions.OriginX, checked((ushort)y)));
        }
        sprites.FinishFrame();
    }

    /// <summary>Builds the main page from decoded slots, hiding Copy and Clear when every slot is empty.</summary>
    private void BuildSaveTilemap()
    {
        currentPresentationPage = HasAnySave
            ? FileSelectPage.MainWithData
            : FileSelectPage.MainEmpty;
        RebuildInstalledPresentationPage();
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
        int next = HelmetAnimationFrame + 1;
        if (next >= FileSelectHelmetAnimation.FrameCount)
        {
            helmetAnimationTimer = 0;
            next = FileSelectHelmetAnimation.FrameCount - 1;
        }
        HelmetAnimationFrame = next;
    }

    /// <summary>Copy or Clear mode selected on the main page before its fade finishes.</summary>
    private FileSelectDataMode pendingDataMode;
    /// <summary>Submenu phase entered once the newly built page reaches full brightness.</summary>
    private FileSelectPhase phaseAfterFadeIn;
    /// <summary>Chooses submenu border and cursor layout during cross-page fade phases.</summary>
    private bool showDataManagementScreen;
    /// <summary>Selected slot or Exit row in source, destination, and clear menus.</summary>
    private FileSelectItem submenuSelection;
    /// <summary>Source slot retained while choosing a copy destination or confirming a clear.</summary>
    private int operationSourceSlot;
    /// <summary>Copy target retained through confirmation and completion pages.</summary>
    private int copyTargetSlot;
    /// <summary>Yes or No row selected on the copy or clear confirmation page.</summary>
    private YesNoChoice confirmationSelection;
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
        FileSelectPhase.EnterBlankScreenAfterFrameWait or FileSelectPhase.LoadBackground or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterFrameWait;

    /// <summary>Hides the selection missile during entry and on completed data-operation pages.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    /// <summary>Handles slot selection, hidden data-management rows, and options or title handoffs.</summary>
    private void StepMainMenu(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            MoveMainSelection(direction: VerticalDirection.Up);
            QueueCursorSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            MoveMainSelection(direction: VerticalDirection.Down);
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

        if (FileSelectItemRows.TryGetSaveSlot(SelectedItem, out _))
        {
            audio?.QueueSound(SoundEffectLibrary1Sounds.FileSelectSwoosh, maximumQueued: 6);
            // `menu_index += 27` enters index 31 and enables only the selected helmet.
            HelmetAnimationFrame = 0;
            helmetAnimationTimer = 1;
            Phase = FileSelectPhase.TurnSelectedHelmet;
            return;
        }
        if (SelectedItem is FileSelectItem.Copy or FileSelectItem.Clear)
        {
            if (!HasAnySave)
                throw new InvalidOperationException("Hidden data-management item became selectable.");
            QueueCursorSound();
            pendingDataMode = SelectedItem == FileSelectItem.Copy ? FileSelectDataMode.Copy : FileSelectDataMode.Clear;
            Phase = FileSelectPhase.FadeOutToDataManagement;
            return;
        }
        if (SelectedItem == FileSelectItem.Exit)
        {
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToTitle;
            return;
        }
        throw new InvalidDataException($"Invalid file-select main item {SelectedItem}.");
    }

    /// <summary>Moves through main-page entries while skipping Copy and Clear for empty SRAM.</summary>
    private void MoveMainSelection(VerticalDirection direction) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, direction);

    /// <summary>Swaps main and data pages only at forced blank, then enters the saved phase after fade-in.</summary>
    private void StepDataManagementFade()
    {
        if (Phase is FileSelectPhase.FadeOutToDataManagement or FileSelectPhase.FadeOutToMain)
        {
            screenFade.FadeOut(ref brightness);
            if (!brightness.IsBlanked)
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
                phaseAfterFadeIn = FileSelectPhase.Main;
            }
            Phase = FileSelectPhase.FadeInFromDataManagement;
            return;
        }

        screenFade.FadeIn(ref brightness);
        if (brightness == ScreenBrightness.FullyLit)
            Phase = phaseAfterFadeIn;
    }

    /// <summary>Starts Copy or Clear on the first occupied slot and builds the corresponding page.</summary>
    private void InitializeDataManagementScreen()
    {
        submenuSelection = FileSelectItemRows.ForSaveSlot(FindFirstNonemptySlot());
        operationSourceSlot = 0;
        copyTargetSlot = 0;
        confirmationSelection = YesNoChoice.Yes;
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
    }

    /// <summary>Dispatches Copy and Clear selection, confirmation, and completed-page input.</summary>
    private void StepDataManagement(SnesButton pressed)
    {
        switch (Phase)
        {
            case FileSelectPhase.CopySelectSource:
                StepSlotSelection(pressed, FileSelectDataMode.Copy);
                break;
            case FileSelectPhase.CopySelectDestination:
                StepCopyDestination(pressed);
                break;
            case FileSelectPhase.CopyConfirm:
                StepCopyArrowPalette();
                StepConfirmation(pressed, FileSelectDataMode.Copy);
                break;
            case FileSelectPhase.ClearSelectSlot:
                StepSlotSelection(pressed, FileSelectDataMode.Clear);
                break;
            case FileSelectPhase.ClearConfirm:
                StepConfirmation(pressed, FileSelectDataMode.Clear);
                break;
            case FileSelectPhase.CopyCompleted:
            case FileSelectPhase.ClearCompleted:
                if (pressed != SnesButton.None)
                {
                    QueueCursorSound();
                    SelectedItem = FileSelectItemRows.ForSaveSlot(saves.ReadSelectedSlot());
                    Phase = FileSelectPhase.FadeOutToMain;
                }
                break;
            case FileSelectPhase.FadeIn:
            case FileSelectPhase.Main:
            case FileSelectPhase.FadeOutToDataManagement:
            case FileSelectPhase.FadeInFromDataManagement:
            case FileSelectPhase.FadeOutToMain:
            case FileSelectPhase.TurnSelectedHelmet:
            case FileSelectPhase.FadeOutToOptions:
            case FileSelectPhase.FadeOutToTitle:
            case FileSelectPhase.EnterBlankScreen:
            case FileSelectPhase.EnterBlankScreenAfterFrameWait:
            case FileSelectPhase.LoadBackground:
            case FileSelectPhase.InitializeMain:
            case FileSelectPhase.InitializeMainAfterFrameWait:
            default:
                throw new InvalidDataException($"Invalid data-management phase {Phase}.");
        }
    }

    /// <summary>Chooses an occupied source or clear slot, then opens destination or confirmation.</summary>
    private void StepSlotSelection(SnesButton pressed, FileSelectDataMode mode)
    {
        int occupiedSlots = 0;
        for (int slot = 0; slot < FileSelectLayout.SaveSlotCount; slot++)
            if (saveSlots[slot] is not null) occupiedSlots |= 1 << slot;
        MoveSubmenuSelection(FileSelectDataNavigation.MoveSourceOrClear(
            submenuSelection, occupiedSlots, pressed));

        if ((pressed & SnesButton.B) != 0 ||
            ((pressed & (SnesButton.Start | SnesButton.A)) != 0 && submenuSelection == FileSelectItem.Exit))
        {
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        QueueCursorSound();
        operationSourceSlot = FileSelectItemRows.SaveSlot(submenuSelection);
        if (mode == FileSelectDataMode.Copy)
        {
            submenuSelection = FileSelectItemRows.ForSaveSlot(Enumerable.Range(0, FileSelectLayout.SaveSlotCount)
                .First(slot => slot != operationSourceSlot));
            BuildCopyDestinationTilemap();
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            confirmationSelection = YesNoChoice.Yes;
            BuildClearConfirmationTilemap();
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
            submenuSelection = FileSelectItemRows.ForSaveSlot(operationSourceSlot);
            BuildCopySourceTilemap();
            Phase = FileSelectPhase.CopySelectSource;
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;
        QueueCursorSound();
        if (submenuSelection == FileSelectItem.Exit)
        {
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        copyTargetSlot = FileSelectItemRows.SaveSlot(submenuSelection);
        confirmationSelection = YesNoChoice.Yes;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.InitialPaletteDelay;
        BuildCopyConfirmationTilemap();
        Phase = FileSelectPhase.CopyConfirm;
    }

    /// <summary>Rotates the copy arrow palette colors when its native CGRAM animation timer expires.</summary>
    private void StepCopyArrowPalette()
    {
        if (copyArrowPaletteTimer == 0 || --copyArrowPaletteTimer != 0)
            return;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.PaletteDelay;
        ColourRun arrow = screen.Palette.SpriteRow(FileCopyArrowDefinitions.SpriteRow)
            .Slice(FileCopyArrowDefinitions.FirstColour, FileCopyArrowDefinitions.ColorCount);
        int last = arrow.Count - 1;
        Bgr555 color = arrow[0];
        for (int index = 0; index < last; index++)
            arrow.Write(index, arrow[index + 1]);
        arrow.Write(last, color);
    }

    /// <summary>Accepts Yes or No and mutates checksummed SRAM only after explicit Yes confirmation.</summary>
    private void StepConfirmation(SnesButton pressed, FileSelectDataMode mode)
    {
        if ((pressed & (SnesButton.Up | SnesButton.Down)) != 0)
        {
            confirmationSelection = confirmationSelection == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
            QueueCursorSound();
            return;
        }
        if ((pressed & SnesButton.B) != 0)
        {
            QueueCursorSound();
            ReturnFromConfirmation(mode);
            return;
        }
        if ((pressed & (SnesButton.Start | SnesButton.A)) == 0)
            return;

        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        if (confirmationSelection != YesNoChoice.Yes)
        {
            ReturnFromConfirmation(mode);
            return;
        }

        if (mode == FileSelectDataMode.Copy)
        {
            saves.CopySlot(operationSourceSlot, copyTargetSlot);
            saveSlots[copyTargetSlot] = saves.ReadSlot(copyTargetSlot)
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
        SaveRamChangedThisFrame = true;
    }

    /// <summary>Restores the prior source or destination selection page after B or No.</summary>
    private void ReturnFromConfirmation(FileSelectDataMode mode)
    {
        submenuSelection = FileSelectItemRows.ForSaveSlot(mode == FileSelectDataMode.Copy ? copyTargetSlot : operationSourceSlot);
        if (mode == FileSelectDataMode.Copy)
        {
            BuildCopyDestinationTilemap();
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            BuildClearSelectionTilemap();
            Phase = FileSelectPhase.ClearSelectSlot;
        }
    }

    /// <summary>Updates a changed submenu row and queues one cursor sound.</summary>
    private void MoveSubmenuSelection(FileSelectItem next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

    /// <summary>Builds the Copy page that asks which occupied save to use as source.</summary>
    private void BuildCopySourceTilemap()
    {
        currentPresentationPage = FileSelectPage.CopySource;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Builds the destination prompt and inserts the chosen source slot letter.</summary>
    private void BuildCopyDestinationTilemap()
    {
        currentPresentationPage = FileSelectPage.CopyDestination;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Builds the Yes/No prompt with both source and destination slot letters.</summary>
    private void BuildCopyConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPage.CopyConfirm;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Displays the completed copy page after SRAM and decoded slots have been updated.</summary>
    private void BuildCopyCompletedTilemap()
    {
        currentPresentationPage = FileSelectPage.CopyCompleted;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Builds the Clear page that asks which occupied slot to erase.</summary>
    private void BuildClearSelectionTilemap()
    {
        currentPresentationPage = FileSelectPage.ClearSelection;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Builds the erase Yes/No prompt with the selected source slot letter.</summary>
    private void BuildClearConfirmationTilemap()
    {
        currentPresentationPage = FileSelectPage.ClearConfirm;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Displays the cleared page with refreshed slot data after SRAM is erased.</summary>
    private void BuildClearCompletedTilemap()
    {
        currentPresentationPage = FileSelectPage.ClearCompleted;
        RebuildInstalledPresentationPage();
    }

    /// <summary>Finds the initial data-management selection and rejects entry with no occupied slot.</summary>
    private int FindFirstNonemptySlot()
    {
        int slot = Array.FindIndex(saveSlots, save => save is not null);
        return slot >= 0 ? slot : throw new InvalidOperationException(
            "Data-management screen requires at least one nonempty save slot.");
    }

    /// <summary>Places the cursor on the active main, slot, or confirmation row at the installed presentation's anchor.</summary>
    private Position16<OamPixels> GetSelectionMissilePosition(FileSelectPresentation presentation)
    {
        FileSelectCursorPage page = IsMainScreenPhase ? FileSelectCursorPage.Main
            : Phase is FileSelectPhase.CopyConfirm or FileSelectPhase.ClearConfirm ? FileSelectCursorPage.Confirmation
            : FileSelectCursorPage.DataManagement;
        int row = page switch
        {
            FileSelectCursorPage.Main => FileSelectItemRows.MainRow(SelectedItem),
            FileSelectCursorPage.Confirmation => confirmationSelection == YesNoChoice.Yes ? 0 : 1,
            FileSelectCursorPage.DataManagement => FileSelectItemRows.DataRow(submenuSelection),
            _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectCursorPage)} {page}."),
        };
        return presentation.CursorPosition(page, row);
    }

    /// <summary>Queues the menu cursor sound with the native six-request limit.</summary>
    private void QueueCursorSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);

    /// <summary>Rebinds host-owned file-select visuals after debugger-state restoration.</summary>
    public void BindMapPresentation([NotNull] AreaMapPresentationCatalog? catalog)
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
    }

    /// <summary>Copies the installed page and overlays live save data and dynamic source/destination letters.</summary>
    private void RebuildInstalledPresentationPage()
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        // The page shows from the moment it is selected: file select selects pages only where the cartridge also
        // uploads them in the same dispatch.
        Span<ushort> bg1Tilemap = screen.Tilemaps.Bg1;
        presentation.CopyPage(currentPresentationPage, bg1Tilemap);
        bool main = currentPresentationPage is
            FileSelectPage.MainWithData or
            FileSelectPage.MainEmpty;
        for (int slot = 0; slot < saveSlots.Length; slot++)
            DrawInstalledSlot(saveSlots[slot], presentation.Slot(!main, slot));

        switch (currentPresentationPage)
        {
            case FileSelectPage.CopyDestination:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectAnchor.CopyDestinationSource),
                    operationSourceSlot);
                break;
            case FileSelectPage.CopyConfirm:
            case FileSelectPage.CopyCompleted:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectAnchor.CopyConfirmSource),
                    operationSourceSlot);
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectAnchor.CopyConfirmDestination),
                    copyTargetSlot);
                break;
            case FileSelectPage.ClearConfirm:
            case FileSelectPage.ClearCompleted:
                presentation.WriteSlotLetter(bg1Tilemap,
                    presentation.DynamicAnchor(
                        FileSelectAnchor.ClearConfirmSource),
                    operationSourceSlot);
                break;
            case FileSelectPage.MainWithData:
            case FileSelectPage.MainEmpty:
            case FileSelectPage.CopySource:
            case FileSelectPage.ClearSelection:
                break;
            default:
                throw new InvalidOperationException($"Undefined file-select page {currentPresentationPage}.");
        }
    }

    /// <summary>Overlays NO DATA or the slot's health, energy tanks, and clamped time onto an installed page.</summary>
    private void DrawInstalledSlot(
        SuperMetroid.Core.Saves.SuperMetroidSaveSlot? slot,
        FileSelectSlotFieldDocument layout)
    {
        FileSelectPresentation presentation = mapPresentation?.FileSelect ??
            throw new InvalidOperationException("File-select presentation is not bound.");
        Span<ushort> bg1Tilemap = screen.Tilemaps.Bg1;
        if (slot is null)
        {
            presentation.ApplyPatch(bg1Tilemap,
                FileSelectPatch.NoData, layout.NoDataAnchor);
            return;
        }

        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPatch.Energy, layout.EnergyAnchor);
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
        int hours = Math.Min(slot.GameTimeHours, 99);
        int minutes = Math.Min(slot.GameTimeMinutes, 99);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 0, hours / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 1, hours % 10);
        presentation.ApplyPatch(bg1Tilemap,
            FileSelectPatch.TimeColon,
            new(checked((ushort)(layout.TimeValueAnchor.X + 2)), layout.TimeValueAnchor.Y));
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 3, minutes / 10);
        presentation.WriteDigit(bg1Tilemap, layout.TimeValueAnchor, 4, minutes % 10);
    }
}
