using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge file-copy and file-clear substates from bank $81. Keeping them beside, but
/// outside, the main screen implementation prevents the already dense renderer from turning
/// back into a thousand-line frontend monolith.
/// </summary>
public sealed partial class FileSelectMenuState
{
    private static readonly ushort[] DataManagementSelectionY = [72, 104, 136, 211];

    private FileSelectDataMode pendingDataMode;
    private FileSelectPhase phaseAfterFadeIn;
    private bool showDataManagementScreen;
    private int submenuSelection;
    private int operationSourceSlot;
    private int operationDestinationSlot;
    private int confirmationSelection;

    private bool HasAnySave => saveSlots.Any(slot => slot is not null);

    private bool IsMainScreenPhase => !showDataManagementScreen;

    private bool IsCopyPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Copy;

    private bool IsClearPhase =>
        showDataManagementScreen && pendingDataMode == FileSelectDataMode.Clear;

    private bool ShouldDrawSelectionMissile => Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    private void StepMainMenu(SnesButton pressed)
    {
        if ((pressed & SnesButton.Up) != 0)
        {
            MoveMainSelection(-1);
            QueueCursorSound();
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            MoveMainSelection(1);
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
            audio?.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2a), maximumQueued: 6);
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

    private void MoveMainSelection(int direction)
    {
        int[] selectable = HasAnySave ? [0, 1, 2, 3, 4, 5] : [0, 1, 2, 5];
        int current = Array.IndexOf(selectable, SelectedItem);
        if (current < 0)
            throw new InvalidDataException($"File-select item {SelectedItem} is not currently visible.");
        SelectedItem = selectable[(current + direction + selectable.Length) % selectable.Length];
    }

    private void StepDataManagementFade()
    {
        if (Phase is FileSelectPhase.FadeOutToDataManagement or FileSelectPhase.FadeOutToMain)
        {
            brightness = Math.Max(0, brightness - 1);
            if (brightness != 0)
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

        brightness = Math.Min(15, brightness + 1);
        if (brightness == 15)
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
        int[] selectable = saveSlots
            .Select((slot, index) => (slot, index))
            .Where(entry => entry.slot is not null)
            .Select(entry => entry.index)
            .Append(3)
            .ToArray();
        MoveSubmenuSelection(pressed, selectable);

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
        int[] selectable = Enumerable.Range(0, 3)
            .Where(slot => slot != operationSourceSlot)
            .Append(3)
            .ToArray();
        MoveSubmenuSelection(pressed, selectable);
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
        BuildCopyConfirmationTilemap();
        UploadBg1Tilemap();
        Phase = FileSelectPhase.CopyConfirm;
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

    private void MoveSubmenuSelection(SnesButton pressed, int[] selectable)
    {
        int current = Array.IndexOf(selectable, submenuSelection);
        if (current < 0)
            throw new InvalidDataException($"Submenu item {submenuSelection} is not selectable.");
        int next = current;
        if ((pressed & SnesButton.Up) != 0)
            next = Math.Max(0, current - 1);
        else if ((pressed & SnesButton.Down) != 0)
            next = Math.Min(selectable.Length - 1, current + 1);
        if (next == current)
            return;
        submenuSelection = selectable[next];
        QueueCursorSound();
    }

    private void BuildCopySourceTilemap()
    {
        BuildDataManagementBase(
            FileSelectTilemaps.DataCopyMode,
            FileSelectLayout.DataModeCopyDestination,
            FileSelectTilemaps.CopyWhichData,
            FileSelectLayout.CopySourcePromptDestination);
    }

    private void BuildCopyDestinationTilemap()
    {
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
        BuildCopyConfirmationTilemap();
        LoadMenuTilemap(FileSelectLayout.CopyCompletedDestination, FileSelectTilemaps.CopyCompleted);
    }

    private void BuildClearSelectionTilemap()
    {
        BuildDataManagementBase(
            FileSelectTilemaps.DataClearMode,
            FileSelectLayout.DataModeClearDestination,
            FileSelectTilemaps.ClearWhichData,
            FileSelectLayout.ClearPromptDestination);
    }

    private void BuildClearConfirmationTilemap()
    {
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
        BuildClearConfirmationTilemap();
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
        LoadMenuTilemap(FileSelectLayout.DataSlotALabelDestination, FileSelectTilemaps.SamusA);
        DrawFileSlot(saveSlots[0], FileSelectLayout.DataSlotAEnergyDestination, FileSelectLayout.DataSlotATimeValueDestination);
        LoadMenuTilemap(FileSelectLayout.DataSlotATimeLabelDestination, FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotBLabelDestination, FileSelectTilemaps.SamusB);
        DrawFileSlot(saveSlots[1], FileSelectLayout.DataSlotBEnergyDestination, FileSelectLayout.DataSlotBTimeValueDestination);
        LoadMenuTilemap(FileSelectLayout.DataSlotBTimeLabelDestination, FileSelectTilemaps.Time);
        LoadMenuTilemap(FileSelectLayout.DataSlotCLabelDestination, FileSelectTilemaps.SamusC);
        DrawFileSlot(saveSlots[2], FileSelectLayout.DataSlotCEnergyDestination, FileSelectLayout.DataSlotCTimeValueDestination);
        LoadMenuTilemap(FileSelectLayout.DataSlotCTimeLabelDestination, FileSelectTilemaps.Time);
    }

    private void AddConfirmationText()
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
        if (!showDataManagementScreen)
            return (14, FileSelectLayout.MainSelectionY[SelectedItem]);
        if (Phase is FileSelectPhase.CopyConfirm or FileSelectPhase.ClearConfirm)
            return (94, confirmationSelection == 0 ? (ushort)184 : (ushort)208);
        return (22, DataManagementSelectionY[submenuSelection]);
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
