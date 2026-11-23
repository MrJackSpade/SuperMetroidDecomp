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
/// The menu's steps and brightness transitions are the game's; its pictures, starfield, text, colours,
/// border, helmets, and selection missile all come from the installed artwork. Slot
/// validity and displayed energy/time are read from the saves themselves
/// rather than from separately authored metadata.
/// </remarks>
public sealed partial class FileSelectMenuState : IMenuFrontendScreen, IComposedFrontendScene<LayeredScene>
{
    /// <summary>The buttons that confirm a choice throughout the menu: Start or A.</summary>
    private const ControllerButton ConfirmButtons = ControllerButton.Start | ControllerButton.A;

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
    /// <summary>The menu screen file select draws: its artwork, pictures, colours and sprites.</summary>
    internal readonly MenuScreen screen;
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
    internal FileSelectHelmetFrame HelmetAnimationFrame => selectedHelmet.Frame;
    /// <summary>The screen's brightness and the cadence of its entry and submenu fades; the menu opens black.</summary>
    private readonly ScreenFade screenFade = new(DisplayBrightness.Black);
    /// <summary>The opening's black hold (<see cref="FileSelectPhase.BlackHold"/>), counted once each update.</summary>
    private readonly UpdateCountdown blackHold = UpdateCountdown.Lasting(FileSelectLayout.BlackHoldUpdates);
    /// <summary>The page file select has selected, which it shows with its slots as they are now.</summary>
    private SelectedFileSelectPage selectedPage = SelectedFileSelectPage.MainEmpty;
    /// <summary>The page shown, with its slots' fields and the slots its prompt names (#1467).</summary>
    internal ShownFileSelectPage ShownPage { get; set; } = new(SelectedFileSelectPage.MainEmpty,
        SaveSlots.All.ToImmutableDictionary(slot => slot, _ => (ShownFileSelectSave?)null));

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
        Phase = new FileSelectPhase.EnterBlankScreen();
    }

    /// <summary>Main-menu selection: slots A-C, Copy, Clear, or Exit.</summary>
    internal FileSelectItem SelectedItem { get; set; }

    /// <summary>Current menu phase.</summary>
    public FileSelectPhase Phase { get; private set; } = new FileSelectPhase.FadeIn();

    /// <summary>
    /// How the menu hands off to its host (#2562): the game options or the title, kept once it reaches one; absent while
    /// the menu runs, before it has reached either (#2587).
    /// </summary>
    public FileSelectMenuOutcome? Outcome { get; private set; }

    /// <summary>
    /// The save slot whose row was confirmed, which a requested game names (#1976): the slot row's own slot, kept as the
    /// menu moves on to the helmet turn.
    /// </summary>
    public SaveSlot SelectedSaveSlot { get; private set; }

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
        StepPhase();

        void StepPhase()
        {
            if (Phase is FileSelectPhase.EnterBlankScreen)
            {
                // Finish any fade-out, then blank the display and wait out one update.
                if (!screenFade.Darken())
                    return;
                Phase = new FileSelectPhase.EnterBlankScreenAfterFrameWait();
            }
            else if (Phase is FileSelectPhase.EnterBlankScreenAfterFrameWait)
            {
                Phase = new FileSelectPhase.BlackHold();
            }
            else if (Phase is FileSelectPhase.BlackHold)
            {
                if (blackHold.StepEnds())
                    Phase = new FileSelectPhase.InitializeMain();
            }
            else if (Phase is FileSelectPhase.InitializeMain)
            {
                // The fades step at the menu's fade pace from here; the display then waits out one update.
                screenFade.StepEvery(FileSelectLayout.FadeUpdatesPerStep);
                Phase = new FileSelectPhase.InitializeMainAfterFrameWait();
            }
            else if (Phase is FileSelectPhase.InitializeMainAfterFrameWait)
            {
                Phase = new FileSelectPhase.FadeIn();
            }
            else if (Phase is FileSelectPhase.FadeIn)
            {
                if (screenFade.Brighten())
                    Phase = new FileSelectPhase.Main();
            }
            else if (Phase is FileSelectPhase.Main)
            {
                StepMainMenu(pressed);
            }
            else if (Phase is FileSelectPhase.FadeOutToCopy or FileSelectPhase.FadeOutToClear or FileSelectPhase.FadeOutToMain or FileSelectPhase.FadeInFromDataManagement)
            {
                StepDataManagementFade();
            }
            else if (Phase is FileSelectPhase.DataManagement managing)
            {
                saveChange = StepDataManagement(managing, pressed);
            }
            else if (Phase is FileSelectPhase.TurnSelectedHelmet)
            {
                selectedHelmet.Step();
                // Start/A ends the turn early; otherwise it ends once its last picture has shown for its whole hold.
                if ((pressed & ConfirmButtons) != ControllerButton.None || selectedHelmet.HasFinished)
                    Phase = new FileSelectPhase.FadeOutToOptions();
            }
            else if (Phase is FileSelectPhase.FadeOutToOptions)
            {
                if (screenFade.Darken())
                    Outcome = FileSelectMenuOutcome.NewGame;
            }
            else
            {
                // The remaining member, FileSelectPhase.FadeOutToTitle.
                if (screenFade.Darken())
                    Outcome = FileSelectMenuOutcome.Title;
            }
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

    /// <summary>Draws the border, the missile, the helmets and the copy arrow, as the menu's step shows them, among the
    /// screen's sprites.</summary>
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
        if (Phase is FileSelectPhase.DataManagement.CopyConfirm confirming)
        {
            (FileCopyArrowShape shape, int y) = FileCopyRoutes.ArrowOf(confirming.Route);
            sprites.AddImage(sheet.Frame(FileCopyArrowDefinitions.FrameName(shape)),
                new Offset16<ScreenPixels>(FileCopyArrowDefinitions.OriginX, y),
                shading: PictureShading.Shaded(FileCopyArrowShades.Rotation(CopyArrowRotation)));
        }
        sprites.FinishFrame();
    }

    /// <summary>Shows the main page, without its Copy and Clear entries when every slot is empty.</summary>
    private void ShowMainPage() => ShowPage(HasAnySave ? SelectedFileSelectPage.MainWithData : SelectedFileSelectPage.MainEmpty);

    /// <summary>
    /// Shows <paramref name="page"/>: the main menu, a copy page naming its source and destination slots, or a clear page
    /// naming its slot, with each slot's field as it is now.
    /// </summary>
    private void ShowPage(SelectedFileSelectPage page)
    {
        selectedPage = page;
        RefreshShownPage();
    }

    /// <summary>Submenu phase entered once the newly built page reaches full brightness.</summary>
    private FileSelectPhase phaseAfterFadeIn = new FileSelectPhase.FadeIn();
    /// <summary>Selected slot or Exit row in source, destination, and clear menus.</summary>
    private FileSelectItem submenuSelection;
    /// <summary>Source slot retained while choosing a copy destination or confirming a clear.</summary>
    private SaveSlot operationSourceSlot;
    /// <summary>The row chosen on the copy-destination menu.</summary>
    private FileCopyDestinationRow copyDestination = FileCopyDestinationRow.FirstFrom(SaveSlot.A);
    /// <summary>Yes or No row selected on the copy or clear confirmation page.</summary>
    private YesNoChoice confirmationSelection;
    /// <summary>The wait before the copy arrow rotates its colors; over while the glow is stopped.</summary>
    private readonly UpdateCountdown copyArrowGlow = UpdateCountdown.Ended();
    /// <summary>
    /// The step the copy arrows' colour rotation shows
    /// (#1512). The rotation runs on from where it stopped each time a copy is confirmed, as the game's colours keep it.
    /// </summary>
    internal FileCopyArrowRotationStep CopyArrowRotation { get; set; } = FileCopyArrowRotationStep.Unrotated;

    /// <summary>Controls whether the main page exposes Copy and Clear navigation.</summary>
    private bool HasAnySave => saveSlots.Values.Any(slot => slot is not null);

    /// <summary>The border each page shows.</summary>
    private static IdentityTable<FileSelectPage, FileSelectBorder> Borders => field ??= new(new Dictionary<FileSelectPage, FileSelectBorder>
    {
        [FileSelectPage.MainWithData] = FileSelectBorder.Main,
        [FileSelectPage.MainEmpty] = FileSelectBorder.Main,
        [FileSelectPage.CopySource] = FileSelectBorder.Copy,
        [FileSelectPage.CopyDestination] = FileSelectBorder.Copy,
        [FileSelectPage.CopyConfirm] = FileSelectBorder.Copy,
        [FileSelectPage.CopyCompleted] = FileSelectBorder.Copy,
        [FileSelectPage.ClearSelection] = FileSelectBorder.Clear,
        [FileSelectPage.ClearConfirm] = FileSelectBorder.Clear,
        [FileSelectPage.ClearCompleted] = FileSelectBorder.Clear,
    });

    /// <summary>
    /// The border the shown page draws in: the main page's, or the Copy or Clear border of a data-management page, which the
    /// pages keep through their fades, the swap between them coming at black.
    /// </summary>
    private FileSelectBorder ShownBorder => Borders[selectedPage.Page];

    /// <summary>Chooses main-page border, helmets, and missile coordinates, including fades.</summary>
    private bool IsMainPagePhase => ShownBorder == FileSelectBorder.Main;

    /// <summary>Suppresses cursor animation before the initialized main page begins fading in.</summary>
    private bool IsEntryPhase => Phase is FileSelectPhase.EnterBlankScreen or
        FileSelectPhase.EnterBlankScreenAfterFrameWait or FileSelectPhase.BlackHold or
        FileSelectPhase.InitializeMain or FileSelectPhase.InitializeMainAfterFrameWait;

    /// <summary>Hides the selection missile during entry and on completed data-operation pages.</summary>
    private bool ShouldDrawSelectionMissile => !IsEntryPhase && Phase is not
        FileSelectPhase.DataManagement.CopyCompleted and not FileSelectPhase.DataManagement.ClearCompleted;

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
            Phase = new FileSelectPhase.FadeOutToTitle();
            return;
        }
        if ((pressed & ConfirmButtons) == ControllerButton.None)
            return;

        if (FileSelectItemRows.TryGetSaveSlot(SelectedItem, out SaveSlot chosen))
        {
            SelectedSaveSlot = chosen;
            audio.QueueSound(SamusSound.FileSelected, priority: SoundRequestPriority.Normal);
            // The menu moves on to the helmet turn, which turns only the selected helmet.
            selectedHelmet.Restart();
            Phase = new FileSelectPhase.TurnSelectedHelmet();
            return;
        }
        if (SelectedItem is FileSelectItem.Copy or FileSelectItem.Clear)
        {
            // The game opens the page whether or not a save exists.
            QueueCursorSound();
            Phase = SelectedItem == FileSelectItem.Copy ? new FileSelectPhase.FadeOutToCopy() : new FileSelectPhase.FadeOutToClear();
            return;
        }
        // Exit is the last of the main page's items.
        QueueCursorSound();
        Phase = new FileSelectPhase.FadeOutToTitle();
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

            if (Phase is FileSelectPhase.FadeOutToMain)
            {
                ShowMainPage();
                phaseAfterFadeIn = new FileSelectPhase.Main();
            }
            else
            {
                InitializeDataManagementScreen((Phase is FileSelectPhase.FadeOutToCopy) ? FileSelectDataMode.Copy : FileSelectDataMode.Clear);
            }
            Phase = new FileSelectPhase.FadeInFromDataManagement();
            return;
        }

        if (screenFade.Brighten())
            Phase = phaseAfterFadeIn;
    }

    /// <summary>Starts <paramref name="mode"/> on the first occupied slot and builds its page.</summary>
    private void InitializeDataManagementScreen(FileSelectDataMode mode)
    {
        submenuSelection = FirstDataManagementSelection();
        operationSourceSlot = SaveSlot.A;
        confirmationSelection = YesNoChoice.Yes;
        if (mode == FileSelectDataMode.Copy)
        {
            ShowPage(SelectedFileSelectPage.CopySource);
            phaseAfterFadeIn = new FileSelectPhase.DataManagement.CopySelectSource();
        }
        else
        {
            ShowPage(SelectedFileSelectPage.ClearSelection);
            phaseAfterFadeIn = new FileSelectPhase.DataManagement.ClearSelectSlot();
        }
    }

    /// <summary>Runs Copy and Clear selection, confirmation, and completed-page input.</summary>
    /// <returns>What the update did to the saves.</returns>
    private FileSelectSaveChange StepDataManagement(FileSelectPhase.DataManagement managing, ControllerButton pressed)
    {
        if (managing is FileSelectPhase.DataManagement.CopySelectSource)
        {
            StepSlotSelection(pressed, FileSelectDataMode.Copy);
            return FileSelectSaveChange.None;
        }
        else if (managing is FileSelectPhase.DataManagement.CopySelectDestination)
        {
            StepCopyDestination(pressed);
            return FileSelectSaveChange.None;
        }
        else if (managing is FileSelectPhase.DataManagement.CopyConfirm confirming)
        {
            StepCopyArrowGlow();
            return StepCopyConfirmation(pressed, confirming.Route);
        }
        else if (managing is FileSelectPhase.DataManagement.ClearSelectSlot)
        {
            StepSlotSelection(pressed, FileSelectDataMode.Clear);
            return FileSelectSaveChange.None;
        }
        else if (managing is FileSelectPhase.DataManagement.ClearConfirm)
        {
            return StepClearConfirmation(pressed);
        }
        else
        {
            // The remaining member, FileSelectPhase.DataManagement.CopyCompleted / FileSelectPhase.DataManagement.ClearCompleted.
            if (pressed != ControllerButton.None)
            {
                QueueCursorSound();
                SelectedItem = FileSelectItemRows.ForSaveSlot(saves.ReadSelectedSlot());
                Phase = new FileSelectPhase.FadeOutToMain();
            }
            return FileSelectSaveChange.None;
        }
    }

    /// <summary>Chooses an occupied source or clear slot, then opens destination or confirmation.</summary>
    private void StepSlotSelection(ControllerButton pressed, FileSelectDataMode mode)
    {
        MoveSubmenuSelection(FileSelectDataNavigation.MoveSourceOrClear(
            submenuSelection, SaveSlots.All.Where(slot => saveSlots[slot] is not null).ToHashSet(), pressed));

        // The page's rows are its slots and Exit: confirming the row that names no slot leaves the page.
        bool confirmed = (pressed & ConfirmButtons) != ControllerButton.None;
        bool selectsSlot = FileSelectItemRows.TryGetSaveSlot(submenuSelection, out SaveSlot source);
        if ((pressed & ControllerButton.B) != ControllerButton.None || (confirmed && !selectsSlot))
        {
            QueueCursorSound();
            Phase = new FileSelectPhase.FadeOutToMain();
            return;
        }
        if (!confirmed)
            return;

        QueueCursorSound();
        operationSourceSlot = source;
        if (mode == FileSelectDataMode.Copy)
        {
            copyDestination = FileCopyDestinationRow.FirstFrom(operationSourceSlot);
            submenuSelection = copyDestination.Item;
            ShowPage(SelectedFileSelectPage.CopyDestination(operationSourceSlot));
            Phase = new FileSelectPhase.DataManagement.CopySelectDestination();
        }
        else
        {
            confirmationSelection = YesNoChoice.Yes;
            ShowPage(SelectedFileSelectPage.ClearConfirm(operationSourceSlot));
            Phase = new FileSelectPhase.DataManagement.ClearConfirm();
        }
    }

    /// <summary>Chooses any slot other than the copy source, preserving B navigation back to source selection.</summary>
    private void StepCopyDestination(ControllerButton pressed)
    {
        copyDestination = FileSelectDataNavigation.MoveCopyDestination(copyDestination, pressed);
        MoveSubmenuSelection(copyDestination.Item);
        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueCursorSound();
            submenuSelection = FileSelectItemRows.ForSaveSlot(operationSourceSlot);
            ShowPage(SelectedFileSelectPage.CopySource);
            Phase = new FileSelectPhase.DataManagement.CopySelectSource();
            return;
        }
        if ((pressed & ConfirmButtons) == ControllerButton.None)
            return;
        QueueCursorSound();
        if (copyDestination is not FileCopyDestinationRow.Destination chosen)
        {
            Phase = new FileSelectPhase.FadeOutToMain();
            return;
        }
        confirmationSelection = YesNoChoice.Yes;
        copyArrowGlow.Restart(FileCopyArrowDefinitions.InitialGlowDelay);
        ShowPage(SelectedFileSelectPage.CopyConfirm(chosen.Route));
        Phase = new FileSelectPhase.DataManagement.CopyConfirm(chosen.Route);
    }

    /// <summary>Moves the copy arrows' glow on one colour when its timer expires.</summary>
    private void StepCopyArrowGlow()
    {
        if (!copyArrowGlow.StepEnds())
            return;
        copyArrowGlow.Restart(FileCopyArrowDefinitions.GlowDelay);
        CopyArrowRotation = CopyArrowRotation.Next;
    }

    /// <summary>
    /// Accepts Yes or No on the copy <paramref name="route"/>'s confirmation: Yes copies it, B or No goes back to choosing
    /// its destination.
    /// </summary>
    /// <returns>What the confirmation did to the saves.</returns>
    private FileSelectSaveChange StepCopyConfirmation(ControllerButton pressed, FileCopyRoute route)
    {
        FileSelectConfirmationAnswer answer = StepConfirmation(pressed);
        if (answer == FileSelectConfirmationAnswer.Declined)
        {
            submenuSelection = FileSelectItemRows.ForSaveSlot(FileCopyRoutes.DestinationOf(route));
            copyDestination = FileCopyDestinationRow.Choosing(route);
            ShowPage(SelectedFileSelectPage.CopyDestination(FileCopyRoutes.SourceOf(route)));
            Phase = new FileSelectPhase.DataManagement.CopySelectDestination();
            return FileSelectSaveChange.None;
        }
        if (answer == FileSelectConfirmationAnswer.Undecided)
            return FileSelectSaveChange.None;
        SaveSlot copyTargetSlot = FileCopyRoutes.DestinationOf(route);
        saves.CopySlot(FileCopyRoutes.SourceOf(route), copyTargetSlot);
        saveSlots[copyTargetSlot] = saves.ReadSavedSlot(copyTargetSlot);
        ShowPage(SelectedFileSelectPage.CopyCompleted(route));
        Phase = new FileSelectPhase.DataManagement.CopyCompleted();
        return FileSelectSaveChange.Copied;
    }

    /// <summary>
    /// Accepts Yes or No on the chosen slot's clear: Yes clears it, B or No goes back to choosing the slot to clear.
    /// </summary>
    /// <returns>What the confirmation did to the saves.</returns>
    private FileSelectSaveChange StepClearConfirmation(ControllerButton pressed)
    {
        FileSelectConfirmationAnswer answer = StepConfirmation(pressed);
        if (answer == FileSelectConfirmationAnswer.Declined)
        {
            submenuSelection = FileSelectItemRows.ForSaveSlot(operationSourceSlot);
            ShowPage(SelectedFileSelectPage.ClearSelection);
            Phase = new FileSelectPhase.DataManagement.ClearSelectSlot();
            return FileSelectSaveChange.None;
        }
        if (answer == FileSelectConfirmationAnswer.Undecided)
            return FileSelectSaveChange.None;
        saves.ClearSlot(operationSourceSlot);
        saveSlots[operationSourceSlot] = null;
        ShowPage(SelectedFileSelectPage.ClearCompleted(operationSourceSlot));
        Phase = new FileSelectPhase.DataManagement.ClearCompleted();
        return FileSelectSaveChange.Cleared;
    }

    /// <summary>
    /// Moves the Yes or No selection on Up or Down, and answers the confirmation on B (declined) or on A or Start with the
    /// row selected: the saves change only after an explicit Yes.
    /// </summary>
    private FileSelectConfirmationAnswer StepConfirmation(ControllerButton pressed)
    {
        if ((pressed & (ControllerButton.Up | ControllerButton.Down)) != ControllerButton.None)
        {
            confirmationSelection = confirmationSelection == YesNoChoice.Yes ? YesNoChoice.No : YesNoChoice.Yes;
            QueueCursorSound();
            return FileSelectConfirmationAnswer.Undecided;
        }
        if ((pressed & ControllerButton.B) != ControllerButton.None)
        {
            QueueCursorSound();
            return FileSelectConfirmationAnswer.Declined;
        }
        if ((pressed & ConfirmButtons) == ControllerButton.None)
            return FileSelectConfirmationAnswer.Undecided;

        audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
        return confirmationSelection == YesNoChoice.Yes ? FileSelectConfirmationAnswer.Accepted : FileSelectConfirmationAnswer.Declined;
    }

    /// <summary>Updates a changed submenu row and queues one cursor sound.</summary>
    private void MoveSubmenuSelection(FileSelectItem next)
    {
        if (next == submenuSelection)
            return;
        submenuSelection = next;
        QueueCursorSound();
    }

    /// <summary>
    /// The data-management page's first selection: the first occupied slot's row, or Exit, the row after the slots, when
    /// every slot is empty, as the game's search runs on past the slots.
    /// </summary>
    private FileSelectItem FirstDataManagementSelection()
    {
        foreach (SaveSlot slot in SaveSlots.All)
        {
            if (saveSlots[slot] is not null)
                return FileSelectItemRows.ForSaveSlot(slot);
        }
        return FileSelectItem.Exit;
    }

    /// <summary>Places the cursor on the active main, slot, or confirmation row at the installed presentation's anchor.</summary>
    private Offset16<ScreenPixels> GetSelectionMissilePosition(FileSelectPresentation presentation)
    {
        if (IsMainPagePhase)
            return presentation.CursorPosition(FileSelectCursorPage.Main, SelectedItem);
        return Phase is FileSelectPhase.DataManagement.CopyConfirm or FileSelectPhase.DataManagement.ClearConfirm
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
            SaveSlots.All.ToImmutableDictionary(slot => slot, slot => saveSlots[slot] is { } save ? ShownFileSelectSave.Of(save) : null));
}
