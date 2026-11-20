using SuperMetroid.Core.Frontend.Artwork;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select menu: slot choice, copy, clear and the
/// helmet turn before loading.
/// </summary>
/// <remarks>
/// This class models the game's menu steps and brightness transitions. Its pictures, starfield, text, colours,
/// border, helmets, and selection missile all come from the installed artwork. Slot
/// validity and displayed energy/time are read from the saves themselves
/// rather than from separately authored metadata.
/// </remarks>
public sealed class FileSelectMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the borders, cursor, helmets and copy arrows draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>Receives the menu's sound requests.</summary>
    private readonly AudioQueues audio;
    /// <summary>Reads and changes the three save slots.</summary>
    private readonly SaveSlotStore saves;
    /// <summary>The menu screen file select draws: its artwork, pictures, colours and sprite entries.</summary>
    private readonly MenuScreen screen;
    /// <summary>Tracks button edges used by menu navigation and confirmation.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Installed file-select artwork, bound when the game is bound to its installed content.</summary>
    private InstalledScreenContent screenContent;
    /// <summary>Each slot's save; a null entry is an empty or invalid slot.</summary>
    private readonly Dictionary<SaveSlot, SuperMetroidSaveSlot?> saveSlots = [];
    /// <summary>The selection missile's roll.</summary>
    private readonly MenuMissileAnimation missile;
    /// <summary>The selected slot's helmet turn, whose picture the helmet keeps through the options fade.</summary>
    private readonly FileSelectHelmetAnimation selectedHelmet;
    /// <summary>The selected slot's helmet picture, retained through the options fade.</summary>
    private FileSelectHelmetFrame HelmetAnimationFrame => selectedHelmet.Frame;
    /// <summary>The screen's brightness and the cadence of its entry and submenu fades; the menu opens black.</summary>
    private readonly ScreenFade screenFade = new(0);
    /// <summary>The opening's black hold (<see cref="FileSelectPhase.BlackHold"/>), counted once each update.</summary>
    private readonly UpdateCountdown blackHold = UpdateCountdown.Lasting(FileSelectLayout.BlackHoldUpdates);
    /// <summary>The page file select has selected, which it shows with its slots as they are now.</summary>
    private FileSelectPage selectedPage = FileSelectPage.MainEmpty;
    /// <summary>The page shown, with its slots' fields and the slots its prompt names (#1467).</summary>
    private ShownFileSelectPage ShownPage { get; set; } = new(FileSelectPage.MainEmpty,
        SaveSlots.All.ToImmutableDictionary(slot => slot, _ => (ShownFileSelectSave?)null), SaveSlot.A, SaveSlot.A);

    /// <summary>Reads the saves and readies the menu to open from black.</summary>
    public FileSelectMenuState(SaveSlotStore saves, AudioQueues audio,
        InstalledScreenContent screenContent, InstalledSpriteSheets spriteSheets)
    {
        this.audio = Ensure.NotNull(audio);
        this.screenContent = Ensure.NotNull(screenContent);
        missile = new MenuMissileAnimation(screenContent.FileSelect.CursorFrameDuration);
        selectedHelmet = new FileSelectHelmetAnimation(screenContent.FileSelect.HelmetFrameDuration);
        screen = new MenuScreen(screenContent.Backdrops.Menus, Ensure.NotNull(spriteSheets));
        BindSpriteSheets(spriteSheets);

        this.saves = Ensure.NotNull(saves);
        foreach (SaveSlot slot in SaveSlots.All)
            saveSlots[slot] = saves.ReadSlot(slot);
        SelectedItem = FileSelectItemRows.ForSaveSlot(saves.ReadSelectedSlot());

        ShowMainPage();

        // The slots are read at once, but the menu opens over updates: the blanking and unblanking steps each wait out one
        // more update.
        Phase = FileSelectPhase.EnterBlankScreen;
    }

    /// <summary>Main-menu selection: slots A-C, Copy, Clear, or Exit.</summary>
    private FileSelectItem SelectedItem { get; set; }

    /// <summary>Current menu phase.</summary>
    public FileSelectPhase Phase { get; private set; }

    /// <summary>How the menu hands off to its host (#2562): the game options or the title, kept once it reaches one.</summary>
    public FileSelectMenuOutcome Outcome { get; private set; }

    /// <summary>The selected save slot, which a requested game always names (#1976).</summary>
    /// <exception cref="InvalidDataException">A command row is selected, which names no save slot.</exception>
    public SaveSlot SelectedSaveSlot => FileSelectItemRows.SaveSlotOf(SelectedItem);

    /// <summary>Whether the selected A-C slot holds a valid save.</summary>
    public bool SelectedSlotContainsSave =>
        FileSelectItemRows.TryGetSaveSlot(SelectedItem, out SaveSlot slot) && saveSlots[slot] is not null;


    /// <summary>
    /// True while the menu's current step waits out a frame of the display: the next update finishes that step instead
    /// of starting a new update of the menu (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool IsWaitingOutDisplayFrame => Phase is
        FileSelectPhase.EnterBlankScreenAfterFrameWait or FileSelectPhase.InitializeMainAfterFrameWait;
    /// <summary>Advances one menu frame from the held controller buttons.</summary>
    /// <returns>What the update did to the saves: a copy, a clear, or nothing.</returns>
    public FileSelectSaveChange Step(ControllerButton controllerInput)
    {
        controller.Latch(controllerInput);
        ControllerButton pressed = controller.NewlyPressed;
        FileSelectSaveChange saveChange = FileSelectSaveChange.None;

        if (!IsEntryPhase)
            missile.Step();
        switch (Phase)
        {
            case FileSelectPhase.EnterBlankScreen:
                // Finish any fade-out, then blank the display and wait out one update.
                if (!screenFade.Darken())
                    break;
                Phase = FileSelectPhase.EnterBlankScreenAfterFrameWait;
                break;

            case FileSelectPhase.EnterBlankScreenAfterFrameWait:
                Phase = FileSelectPhase.BlackHold;
                break;

            case FileSelectPhase.BlackHold:
                if (blackHold.StepEnds())
                    Phase = FileSelectPhase.InitializeMain;
                break;

            case FileSelectPhase.InitializeMain:
                // The fades step at the menu's fade pace from here; the display then waits out one update.
                screenFade.StepEvery(FileSelectLayout.FadeUpdatesPerStep);
                Phase = FileSelectPhase.InitializeMainAfterFrameWait;
                break;

            case FileSelectPhase.InitializeMainAfterFrameWait:
                Phase = FileSelectPhase.FadeIn;
                break;

            case FileSelectPhase.FadeIn:
                if (screenFade.Brighten())
                    Phase = FileSelectPhase.Main;
                break;

            case FileSelectPhase.Main:
                StepMainMenu(pressed);
                break;

            case FileSelectPhase.FadeOutToCopy:
            case FileSelectPhase.FadeOutToClear:
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
                saveChange = StepDataManagement(pressed);
                break;

            case FileSelectPhase.TurnSelectedHelmet:
                selectedHelmet.Step();
                // Start/A ends the turn early; otherwise it ends once its last picture has shown for its whole hold.
                if ((pressed & (ControllerButton.Start | ControllerButton.A)) != ControllerButton.None || selectedHelmet.HasFinished)
                    Phase = FileSelectPhase.FadeOutToOptions;
                break;

            case FileSelectPhase.FadeOutToOptions:
                if (screenFade.Darken())
                    Outcome = FileSelectMenuOutcome.NewGame;
                break;

            case FileSelectPhase.FadeOutToTitle:
                if (screenFade.Darken())
                    Outcome = FileSelectMenuOutcome.Title;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(FileSelectPhase)} {Phase}.");
        }
        return saveChange;
    }

    /// <summary>Captures file-menu display state after simulation-owned sprite preparation.</summary>
    public LayeredScene ComposeScene()
    {
        PrepareRenderSprites();
        FileSelectPresentation presentation = screenContent.FileSelect;
        // The backdrop, the page, its slots' fields and its slot letters are the menus' pictures (#1467).
        return MenuSceneComposer.Capture(screen, screen.PagePictures(FileSelectParts.BackgroundFrame),
            screen.PlacedPictures(ShownPage.Pictures(presentation)), 0, screenFade.Level);
    }

    /// <summary>Builds the current border, missile, helmet, and optional copy-arrow sprite entries from menu phase.</summary>
    private void PrepareRenderSprites()
    {
        ScreenSprites sprites = screen.Sprites;
        sprites.BeginFrame();
        bool mainPage = IsMainPagePhase;
        FileSelectPresentation presentation = screenContent.FileSelect;
        FileSelectBorder borderName = ShownBorder;
        SpriteSheet sheet = spriteSheets.Get(SpriteSheetId.FileSelect);
        presentation.DrawBorder(sprites, sheet, borderName);
        if (ShouldDrawSelectionMissile)
            FileSelectPresentation.DrawCursor(sprites, sheet, missile.Frame, GetSelectionMissilePosition(presentation));
        // The helmets stand beside the slots on the main page only.
        IReadOnlyList<SaveSlot> helmetSlots = mainPage ? SaveSlots.All : [];
        foreach (SaveSlot slot in helmetSlots)
        {
            // The selected helmet keeps its last turned picture, the completed (or Start-shortened) turn, through the
            // fade out.
            FileSelectHelmetFrame frame = FileSelectItemRows.ForSaveSlot(slot) == SelectedItem && Phase is
                    FileSelectPhase.TurnSelectedHelmet or FileSelectPhase.FadeOutToOptions
                ? HelmetAnimationFrame
                : FileSelectHelmetFrame.TurnedAway;
            presentation.DrawHelmet(sprites, sheet, frame, slot);
        }
        if (Phase == FileSelectPhase.CopyConfirm)
        {
            (FileCopyArrowShape shape, int y) = FileCopyArrows.Select(operationSourceSlot, copyTargetSlot);
            sprites.AddImage(sheet.Frame(FileCopyArrowDefinitions.FrameName(shape)),
                new Offset16<ScreenPixels>(FileCopyArrowDefinitions.OriginX, y),
                shading: PictureShading.Shaded(FileCopyArrowShades.Rotation(CopyArrowRotation)));
        }
        sprites.FinishFrame();
    }

    /// <summary>Shows the main page, without its Copy and Clear entries when every slot is empty.</summary>
    private void ShowMainPage() => ShowPage(HasAnySave ? FileSelectPage.MainWithData : FileSelectPage.MainEmpty);

    /// <summary>
    /// Shows <paramref name="page"/>: the main menu, a copy page naming its source and destination slots, or a clear page
    /// naming its slot, with each slot's field as it is now.
    /// </summary>
    private void ShowPage(FileSelectPage page)
    {
        selectedPage = page;
        RefreshShownPage();
    }

    /// <summary>Submenu phase entered once the newly built page reaches full brightness.</summary>
    private FileSelectPhase phaseAfterFadeIn;
    /// <summary>Selected slot or Exit row in source, destination, and clear menus.</summary>
    private FileSelectItem submenuSelection;
    /// <summary>Source slot retained while choosing a copy destination or confirming a clear.</summary>
    private SaveSlot operationSourceSlot;
    /// <summary>Copy target retained through confirmation and completion pages.</summary>
    private SaveSlot copyTargetSlot;
    /// <summary>Yes or No row selected on the copy or clear confirmation page.</summary>
    private YesNoChoice confirmationSelection;
    /// <summary>The wait before the copy arrow rotates its colors; over while the glow is stopped.</summary>
    private readonly UpdateCountdown copyArrowGlow = UpdateCountdown.Ended();
    /// <summary>
    /// The step the copy arrows' colour rotation shows
    /// (#1512). The rotation runs on from where it stopped each time a copy is confirmed, as the game's colours keep it.
    /// </summary>
    private FileCopyArrowRotationStep CopyArrowRotation { get; set; } = FileCopyArrowRotationStep.Unrotated;

    /// <summary>Controls whether the main page exposes Copy and Clear navigation.</summary>
    private bool HasAnySave => saveSlots.Values.Any(slot => slot is not null);

    /// <summary>
    /// The border the shown page draws in: the main page's, or the Copy or Clear border of a data-management page, which the
    /// pages keep through their fades, the swap between them coming at black.
    /// </summary>
    private FileSelectBorder ShownBorder => selectedPage switch
    {
        FileSelectPage.MainWithData or FileSelectPage.MainEmpty => FileSelectBorder.Main,
        FileSelectPage.CopySource or FileSelectPage.CopyDestination or FileSelectPage.CopyConfirm or
            FileSelectPage.CopyCompleted => FileSelectBorder.Copy,
        FileSelectPage.ClearSelection or FileSelectPage.ClearConfirm or FileSelectPage.ClearCompleted => FileSelectBorder.Clear,
        _ => throw new InvalidOperationException($"Undefined {nameof(FileSelectPage)} {selectedPage}."),
    };

    /// <summary>Chooses main-page border, helmets, and missile coordinates, including fades.</summary>
    private bool IsMainPagePhase => ShownBorder == FileSelectBorder.Main;

    /// <summary>Suppresses cursor animation before the initialized main page begins fading in.</summary>
    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterFrameWait or FileSelectPhase.BlackHold or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterFrameWait;

    /// <summary>Hides the selection missile during entry and on completed data-operation pages.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.CopyCompleted and not FileSelectPhase.ClearCompleted;

    /// <summary>Handles slot selection, hidden data-management rows, and options or title handoffs.</summary>
    private void StepMainMenu(ControllerButton pressed)
    {
        if ((pressed & ControllerButton.Up) != ControllerButton.None)
        {
            MoveMainSelection(direction: VerticalDirection.Up);
            QueueCursorSound();
        }
        else if ((pressed & ControllerButton.Down) != ControllerButton.None)
        {
            MoveMainSelection(direction: VerticalDirection.Down);
            QueueCursorSound();
        }

        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueCursorSound();
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToTitle;
            return;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;

        if (FileSelectItemRows.TryGetSaveSlot(SelectedItem, out _))
        {
            audio.QueueSound(SamusSound.FileSelected, priority: SoundRequestPriority.Normal);
            // The menu moves on to the helmet turn, which turns only the selected helmet.
            selectedHelmet.Restart();
            Phase = FileSelectPhase.TurnSelectedHelmet;
            return;
        }
        if (SelectedItem is FileSelectItem.Copy or FileSelectItem.Clear)
        {
            if (!HasAnySave)
                throw new InvalidOperationException("Hidden data-management item became selectable.");
            QueueCursorSound();
            Phase = SelectedItem == FileSelectItem.Copy ? FileSelectPhase.FadeOutToCopy : FileSelectPhase.FadeOutToClear;
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

    /// <summary>Moves through main-page entries while skipping Copy and Clear when no save exists.</summary>
    private void MoveMainSelection(VerticalDirection direction) =>
        SelectedItem = FileSelectMainNavigation.Move(SelectedItem, HasAnySave, direction);

    /// <summary>Swaps main and data pages only once the screen is black, then enters the saved phase after fade-in.</summary>
    private void StepDataManagementFade()
    {
        if (Phase is FileSelectPhase.FadeOutToCopy or FileSelectPhase.FadeOutToClear or FileSelectPhase.FadeOutToMain)
        {
            if (!screenFade.Darken())
                return;

            if (Phase == FileSelectPhase.FadeOutToMain)
            {
                ShowMainPage();
                phaseAfterFadeIn = FileSelectPhase.Main;
            }
            else
            {
                InitializeDataManagementScreen(Phase == FileSelectPhase.FadeOutToCopy ? FileSelectDataMode.Copy : FileSelectDataMode.Clear);
            }
            Phase = FileSelectPhase.FadeInFromDataManagement;
            return;
        }

        if (screenFade.Brighten())
            Phase = phaseAfterFadeIn;
    }

    /// <summary>Starts <paramref name="mode"/> on the first occupied slot and builds its page.</summary>
    private void InitializeDataManagementScreen(FileSelectDataMode mode)
    {
        submenuSelection = FileSelectItemRows.ForSaveSlot(FindFirstNonemptySlot());
        operationSourceSlot = SaveSlot.A;
        copyTargetSlot = SaveSlot.A;
        confirmationSelection = YesNoChoice.Yes;
        if (mode == FileSelectDataMode.Copy)
        {
            ShowPage(FileSelectPage.CopySource);
            phaseAfterFadeIn = FileSelectPhase.CopySelectSource;
        }
        else
        {
            ShowPage(FileSelectPage.ClearSelection);
            phaseAfterFadeIn = FileSelectPhase.ClearSelectSlot;
        }
    }

    /// <summary>Runs Copy and Clear selection, confirmation, and completed-page input.</summary>
    /// <returns>What the update did to the saves.</returns>
    private FileSelectSaveChange StepDataManagement(ControllerButton pressed)
    {
        switch (Phase)
        {
            case FileSelectPhase.CopySelectSource:
                StepSlotSelection(pressed, FileSelectDataMode.Copy);
                return FileSelectSaveChange.None;
            case FileSelectPhase.CopySelectDestination:
                StepCopyDestination(pressed);
                return FileSelectSaveChange.None;
            case FileSelectPhase.CopyConfirm:
                StepCopyArrowGlow();
                return StepConfirmation(pressed, FileSelectDataMode.Copy);
            case FileSelectPhase.ClearSelectSlot:
                StepSlotSelection(pressed, FileSelectDataMode.Clear);
                return FileSelectSaveChange.None;
            case FileSelectPhase.ClearConfirm:
                return StepConfirmation(pressed, FileSelectDataMode.Clear);
            case FileSelectPhase.CopyCompleted:
            case FileSelectPhase.ClearCompleted:
                if (pressed != ControllerButton.None)
                {
                    QueueCursorSound();
                    SelectedItem = FileSelectItemRows.ForSaveSlot(saves.ReadSelectedSlot());
                    Phase = FileSelectPhase.FadeOutToMain;
                }
                return FileSelectSaveChange.None;
            case FileSelectPhase.FadeIn:
            case FileSelectPhase.Main:
            case FileSelectPhase.FadeOutToCopy:
            case FileSelectPhase.FadeOutToClear:
            case FileSelectPhase.FadeInFromDataManagement:
            case FileSelectPhase.FadeOutToMain:
            case FileSelectPhase.TurnSelectedHelmet:
            case FileSelectPhase.FadeOutToOptions:
            case FileSelectPhase.FadeOutToTitle:
            case FileSelectPhase.EnterBlankScreen:
            case FileSelectPhase.EnterBlankScreenAfterFrameWait:
            case FileSelectPhase.BlackHold:
            case FileSelectPhase.InitializeMain:
            case FileSelectPhase.InitializeMainAfterFrameWait:
            default:
                throw new InvalidDataException($"Invalid data-management phase {Phase}.");
        }
    }

    /// <summary>Chooses an occupied source or clear slot, then opens destination or confirmation.</summary>
    private void StepSlotSelection(ControllerButton pressed, FileSelectDataMode mode)
    {
        MoveSubmenuSelection(FileSelectDataNavigation.MoveSourceOrClear(
            submenuSelection, SaveSlots.All.Where(slot => saveSlots[slot] is not null).ToHashSet(), pressed));

        if ((pressed & ControllerButton.B) != ControllerButton.None ||
            ((pressed & (ControllerButton.Start | ControllerButton.A)) != ControllerButton.None && submenuSelection == FileSelectItem.Exit))
        {
            QueueCursorSound();
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;

        QueueCursorSound();
        operationSourceSlot = FileSelectItemRows.SaveSlotOf(submenuSelection);
        if (mode == FileSelectDataMode.Copy)
        {
            submenuSelection = FileSelectItemRows.ForSaveSlot(SaveSlots.All.First(slot => slot != operationSourceSlot));
            ShowPage(FileSelectPage.CopyDestination);
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            confirmationSelection = YesNoChoice.Yes;
            ShowPage(FileSelectPage.ClearConfirm);
            Phase = FileSelectPhase.ClearConfirm;
        }
    }

    /// <summary>Chooses any slot other than the copy source, preserving B navigation back to source selection.</summary>
    private void StepCopyDestination(ControllerButton pressed)
    {
        MoveSubmenuSelection(FileSelectDataNavigation.MoveCopyDestination(
            submenuSelection, operationSourceSlot, pressed));
        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueCursorSound();
            submenuSelection = FileSelectItemRows.ForSaveSlot(operationSourceSlot);
            ShowPage(FileSelectPage.CopySource);
            Phase = FileSelectPhase.CopySelectSource;
            return;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return;
        QueueCursorSound();
        if (submenuSelection == FileSelectItem.Exit)
        {
            Phase = FileSelectPhase.FadeOutToMain;
            return;
        }
        copyTargetSlot = FileSelectItemRows.SaveSlotOf(submenuSelection);
        confirmationSelection = YesNoChoice.Yes;
        copyArrowGlow.Restart(FileCopyArrowDefinitions.InitialGlowDelay);
        ShowPage(FileSelectPage.CopyConfirm);
        Phase = FileSelectPhase.CopyConfirm;
    }

    /// <summary>Moves the copy arrows' glow on one colour when its timer expires.</summary>
    private void StepCopyArrowGlow()
    {
        if (!copyArrowGlow.StepEnds())
            return;
        copyArrowGlow.Restart(FileCopyArrowDefinitions.GlowDelay);
        CopyArrowRotation = CopyArrowRotation.Next;
    }

    /// <summary>Accepts Yes or No and changes the saves only after explicit Yes confirmation.</summary>
    /// <returns>What the confirmation did to the saves.</returns>
    private FileSelectSaveChange StepConfirmation(ControllerButton pressed, FileSelectDataMode mode)
    {
        if ((pressed & (ControllerButton.Up | ControllerButton.Down)) != ControllerButton.None)
        {
            confirmationSelection = confirmationSelection == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
            QueueCursorSound();
            return FileSelectSaveChange.None;
        }
        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueCursorSound();
            ReturnFromConfirmation(mode);
            return FileSelectSaveChange.None;
        }
        if ((pressed & (ControllerButton.Start | ControllerButton.A)) == ControllerButton.None)
            return FileSelectSaveChange.None;

        audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
        if (confirmationSelection != YesNoChoice.Yes)
        {
            ReturnFromConfirmation(mode);
            return FileSelectSaveChange.None;
        }

        if (mode == FileSelectDataMode.Copy)
        {
            saves.CopySlot(operationSourceSlot, copyTargetSlot);
            saveSlots[copyTargetSlot] = saves.ReadSlot(copyTargetSlot)
                ?? throw new InvalidDataException("The copied save slot did not read back as a valid save.");
            ShowPage(FileSelectPage.CopyCompleted);
            Phase = FileSelectPhase.CopyCompleted;
            return FileSelectSaveChange.Copied;
        }
        saves.ClearSlot(operationSourceSlot);
        saveSlots[operationSourceSlot] = null;
        ShowPage(FileSelectPage.ClearCompleted);
        Phase = FileSelectPhase.ClearCompleted;
        return FileSelectSaveChange.Cleared;
    }

    /// <summary>Restores the prior source or destination selection page after B or No.</summary>
    private void ReturnFromConfirmation(FileSelectDataMode mode)
    {
        submenuSelection = FileSelectItemRows.ForSaveSlot(mode == FileSelectDataMode.Copy ? copyTargetSlot : operationSourceSlot);
        if (mode == FileSelectDataMode.Copy)
        {
            ShowPage(FileSelectPage.CopyDestination);
            Phase = FileSelectPhase.CopySelectDestination;
        }
        else
        {
            ShowPage(FileSelectPage.ClearSelection);
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

    /// <summary>Finds the initial data-management selection and rejects entry with no occupied slot.</summary>
    private SaveSlot FindFirstNonemptySlot()
    {
        foreach (SaveSlot slot in SaveSlots.All)
        {
            if (saveSlots[slot] is not null)
                return slot;
        }
        throw new InvalidOperationException("Data-management screen requires at least one nonempty save slot.");
    }

    /// <summary>Places the cursor on the active main, slot, or confirmation row at the installed presentation's anchor.</summary>
    private Offset16<ScreenPixels> GetSelectionMissilePosition(FileSelectPresentation presentation)
    {
        if (IsMainPagePhase)
            return presentation.CursorPosition(FileSelectCursorPage.Main, SelectedItem);
        return Phase is FileSelectPhase.CopyConfirm or FileSelectPhase.ClearConfirm
            ? presentation.CursorPosition(confirmationSelection)
            : presentation.CursorPosition(FileSelectCursorPage.DataManagement, submenuSelection);
    }

    /// <summary>Queues the menu cursor sound at normal priority.</summary>
    private void QueueCursorSound() =>
        audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);

    /// <summary>Binds the installed file-select artwork when the game is bound to its installed content.</summary>
    public void BindScreenContent(InstalledScreenContent catalog)
    {
        screenContent = Ensure.NotNull(catalog);
        missile.Rebind(catalog.FileSelect.CursorFrameDuration);
        selectedHelmet.Rebind(catalog.FileSelect.HelmetFrameDuration);
        screen.Backdrop = catalog.Backdrops.Menus;
        RefreshShownPage();
    }

    /// <summary>
    /// Shows the selected page with each slot's field and the slots its copy or clear prompt names, as they are now: the
    /// game redraws the page only where file select selects it or its slots change (#1467).
    /// </summary>
    private void RefreshShownPage() =>
        ShownPage = new ShownFileSelectPage(selectedPage,
            SaveSlots.All.ToImmutableDictionary(slot => slot, slot => saveSlots[slot] is { } save ? ShownFileSelectSave.Of(save) : null),
            operationSourceSlot, copyTargetSlot);
}
