using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge file-copy and file-clear substates from bank $81. Keeping them beside, but
/// outside, the main screen implementation prevents the already dense renderer from turning
/// back into a thousand-line frontend monolith.
/// </summary>
public sealed partial class FileSelectMenuState
{
    /// <summary>Copy or clear workflow chosen from the main file-select menu.</summary>
    private FileSelectDataMode pendingDataMode;
    /// <summary>Phase restored after the data-management page finishes fading in.</summary>
    private FileSelectPhase phaseAfterFadeIn;
    /// <summary>Whether the copy/clear page currently replaces the main file-select screen.</summary>
    private bool showDataManagementScreen;
    /// <summary>Selected save slot or Exit row within the active submenu.</summary>
    private int submenuSelection;
    /// <summary>Save slot read or cleared by the pending operation.</summary>
    private int operationSourceSlot;
    /// <summary>Destination save slot chosen by the copy workflow.</summary>
    private int operationDestinationSlot;
    /// <summary>Selected Yes/No row on an operation confirmation page.</summary>
    private int confirmationSelection;
    /// <summary>Remaining updates before the copy-arrow palette advances to its next color.</summary>
    private int copyArrowPaletteTimer;

    /// <summary>Whether at least one save slot can be selected for data management.</summary>
    private bool HasAnySave => saveSlots.Any(slot => slot is not null);

    /// <summary>Whether rendering and input should use the main menu.</summary>
    private bool IsMainScreenPhase => !showDataManagementScreen;

    /// <summary>Whether the active data-management workflow is copying save data.</summary>
    private bool IsCopyPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Copy;

    /// <summary>Whether the active data-management workflow is clearing a save slot.</summary>
    private bool IsClearPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Clear;

    /// <summary>Whether file-select is still in setup phases before the menu becomes interactive.</summary>
    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.LoadBackground or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterNmi;

    /// <summary>Whether the current phase should display the selection cursor.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    /// <summary>Handles one update of main-menu navigation and dispatches the selected action.</summary>
    /// <param name="pressed">Newly pressed buttons consumed by the file-select menu.</param>
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

    /// <summary>Moves the selected main-menu row using save-slot-aware navigation rules.</summary>
    /// <param name="down">True to move toward the following row; false to move upward.</param>
    private void MoveMainSelection(bool down) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, down);

    /// <summary>Runs the black-screen handoff and fade between the main and data-management pages.</summary>
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

    /// <summary>Chooses the first occupied slot and builds the initial copy or clear page.</summary>
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

    /// <summary>Dispatches input according to the active copy/clear subphase.</summary>
    /// <param name="pressed">Newly pressed buttons for this update.</param>
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

    /// <summary>Moves among occupied slots and opens the selected workflow step.</summary>
    /// <param name="pressed">Newly pressed navigation or confirmation buttons.</param>
    /// <param name="sourceSelection">True for copy-source selection; false for choosing a clear target.</param>
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

    /// <summary>Chooses a copy destination or returns to source selection.</summary>
    /// <param name="pressed">Newly pressed navigation or confirmation buttons.</param>
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

    /// <summary>Rotates the copy confirmation arrow palette at its authored cadence.</summary>
    private void StepCopyArrowPalette()
    {
        if (copyArrowPaletteTimer == 0 || --copyArrowPaletteTimer != 0)
            return;
        copyArrowPaletteTimer = FileCopyArrowDefinitions.PaletteDelay;
        int first = FileCopyArrowDefinitions.FirstColor;
        int last = first + FileCopyArrowDefinitions.ColorCount - 1;
        ushort color = ppu.Cgram.Colors[first];
        for (int index = first; index < last; index++)
            ppu.Cgram.SetColor(index, ppu.Cgram.Colors[index + 1]);
        ppu.Cgram.SetColor(last, color);
    }

    /// <summary>Handles Yes/No input and performs a confirmed copy or clear operation.</summary>
    /// <param name="pressed">Newly pressed confirmation buttons.</param>
    /// <param name="copy">True to copy between slots; false to clear the selected slot.</param>
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

    /// <summary>Returns to the relevant selection page after declining or cancelling confirmation.</summary>
    /// <param name="copy">True to return to copy destination selection; false to clear-slot selection.</param>
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

    /// <summary>Updates the submenu cursor and plays its movement sound only when the row changes.</summary>
    /// <param name="next">Next slot or Exit row index.</param>
    private void MoveSubmenuSelection(int next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

    /// <summary>Builds the page asking which occupied slot supplies the copied data.</summary>
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

    /// <summary>Builds the page asking which slot receives the selected source data.</summary>
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

    /// <summary>Builds the confirmation page showing both source and destination slots.</summary>
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

    /// <summary>Builds the completion page shown after save data has been copied.</summary>
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

    /// <summary>Builds the page for choosing a slot to clear.</summary>
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

    /// <summary>Builds the confirmation page identifying the slot selected for clearing.</summary>
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

    /// <summary>Builds the completion page shown after the selected slot is cleared.</summary>
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

    /// <summary>Creates the shared mode heading, prompt, Exit row, and save-slot area.</summary>
    /// <param name="modeLabel">Tilemap source for the Copy or Clear heading.</param>
    /// <param name="modeDestination">BG1 destination for the heading.</param>
    /// <param name="promptLabel">Tilemap source for the current workflow prompt.</param>
    /// <param name="promptDestination">BG1 destination for the prompt.</param>
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

    /// <summary>Draws the three save-slot labels, energy values, and play-time labels.</summary>
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

    /// <summary>Adds the shared question and Yes/No labels to a confirmation tilemap.</summary>
    private static void AddConfirmationText()
    {
        LoadMenuTilemap(FileSelectLayout.ConfirmationQuestionDestination, FileSelectTilemaps.IsThisOkay);
        LoadMenuTilemap(FileSelectLayout.ConfirmationYesDestination, FileSelectTilemaps.Yes);
        LoadMenuTilemap(FileSelectLayout.ConfirmationNoDestination, FileSelectTilemaps.No);
    }

    /// <summary>Finds the first occupied slot used to initialize submenu selection.</summary>
    /// <returns>The zero-based occupied slot index.</returns>
    /// <exception cref="InvalidOperationException">No save slot is occupied.</exception>
    private int FindFirstNonemptySlot()
    {
        int slot = Array.FindIndex(saveSlots, save => save is not null);
        return slot >= 0 ? slot : throw new InvalidOperationException(
            "Data-management screen requires at least one nonempty save slot.");
    }

    /// <summary>Resolves the current cursor's screen-space origin from the active presentation.</summary>
    /// <returns>Horizontal and vertical pixel coordinates for the selection marker.</returns>
    private (ushort X, ushort Y) GetSelectionMissilePosition()
    {
        if (mapPresentation is not null)
        {
            bool confirmation = Phase is FileSelectPhase.CopyConfirm or
                FileSelectPhase.ClearConfirm;
            int selected = IsMainScreenPhase
                ? SelectedItem
                : confirmation ? confirmationSelection : submenuSelection;
            MapLabelPoint point = mapPresentation.FileSelect.CursorPosition(
                IsMainScreenPhase, confirmation, selected);
            return (checked((ushort)point.X), checked((ushort)point.Y));
        }
        if (!showDataManagementScreen)
            return (14, FileSelectLayout.MainSelectionY(SelectedItem));
        if (Phase is FileSelectPhase.CopyConfirm or FileSelectPhase.ClearConfirm)
            return (94, confirmationSelection == 0 ? (ushort)184 : (ushort)208);
        return (22, FileSelectLayout.DataSelectionY(submenuSelection));
    }

    /// <summary>Transfers the current file-select tilemap to its BG1 VRAM region.</summary>
    private void UploadBg1Tilemap() =>
        ppu.Vram.ExecuteWordTransfer(bg1Tilemap, MenuPpuState.Bg1TilemapWord, 1);

    /// <summary>Queues the standard file-select cursor sound within the audio queue limit.</summary>
    private void QueueCursorSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
}

/// <summary>Data operation selected from the file-select menu.</summary>
internal enum FileSelectDataMode
{
    /// <summary>Copy one occupied save slot into another slot.</summary>
    Copy,
    /// <summary>Clear an occupied save slot after confirmation.</summary>
    Clear,
}
