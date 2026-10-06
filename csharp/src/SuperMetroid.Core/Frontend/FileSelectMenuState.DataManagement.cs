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
    private FileSelectDataMode pendingDataMode;
    private FileSelectPhase phaseAfterFadeIn;
    private bool showDataManagementScreen;
    private int submenuSelection;
    private int operationSourceSlot;
    private int operationDestinationSlot;
    private int confirmationSelection;
    private int copyArrowPaletteTimer;

    private bool HasAnySave => saveSlots.Any(slot => slot is not null);

    private bool IsMainScreenPhase => !showDataManagementScreen;

    private bool IsCopyPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Copy;

    private bool IsClearPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Clear;

    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterNmi or FileSelectPhase.LoadBackground or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterNmi;

    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

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

    private void MoveMainSelection(bool down) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, down);

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

    private void MoveSubmenuSelection(int next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

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

    private static void AddConfirmationText()
    {
        LoadMenuTilemap(FileSelectLayout.ConfirmationQuestionDestination, FileSelectTilemaps.IsThisOkay);
        LoadMenuTilemap(FileSelectLayout.ConfirmationYesDestination, FileSelectTilemaps.Yes);
        LoadMenuTilemap(FileSelectLayout.ConfirmationNoDestination, FileSelectTilemaps.No);
    }

    private int FindFirstNonemptySlot()
    {
        int slot = Array.FindIndex(saveSlots, save => save is not null);
        return slot >= 0 ? slot : throw new InvalidOperationException(
            "Data-management screen requires at least one nonempty save slot.");
    }

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

    private void UploadBg1Tilemap() =>
        ppu.Vram.ExecuteWordTransfer(bg1Tilemap, MenuPpuState.Bg1TilemapWord, 1);

    private void QueueCursorSound() =>
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
}

internal enum FileSelectDataMode
{
    Copy,
    Clear,
}
