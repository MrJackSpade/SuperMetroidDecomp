namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-select phases, including combined data-management steps and explicit frame-wait continuations.</summary>
public abstract partial record FileSelectPhase
{
    /// <summary>Allows only the cases below, each named <paramref name="name"/>.</summary>
    private FileSelectPhase(string name) => Name = name;

    /// <summary>The phase's name, which the frontend's frames report.</summary>
    public string Name { get; }

    /// <summary>Brighten the initialized screen until it is fully lit.</summary>
    public sealed partial record FadeIn() : FileSelectPhase(nameof(FadeIn));

    /// <summary>Accept slot, Copy, Clear and Exit navigation; data-management items are skipped when all slots are empty.</summary>
    public sealed partial record Main() : FileSelectPhase(nameof(Main));

    /// <summary>Fade the screen to black, then build the Copy page on the first occupied slot.</summary>
    public sealed partial record FadeOutToCopy() : FileSelectPhase(nameof(FadeOutToCopy));

    /// <summary>Fade the screen to black, then build the Clear page on the first occupied slot.</summary>
    public sealed partial record FadeOutToClear() : FileSelectPhase(nameof(FadeOutToClear));

    /// <summary>Brighten the newly built Copy, Clear or main page; covers both the submenu and the return-to-main fade-ins.</summary>
    public sealed partial record FadeInFromDataManagement() : FileSelectPhase(nameof(FadeInFromDataManagement));

    /// <summary>Fade the data-management page to black, then rebuild main from the updated slot contents.</summary>
    public sealed partial record FadeOutToMain() : FileSelectPhase(nameof(FadeOutToMain));

    /// <summary>Turn only the selected slot's helmet; a new Start/A press can end the animation early.</summary>
    public sealed partial record TurnSelectedHelmet() : FileSelectPhase(nameof(TurnSelectedHelmet));

    /// <summary>Fade the selected helmet and main page to black, then signal the game-options handoff.</summary>
    public sealed partial record FadeOutToOptions() : FileSelectPhase(nameof(FadeOutToOptions));

    /// <summary>Fade to black after Exit/B, then request the host's soft-reset path.</summary>
    public sealed partial record FadeOutToTitle() : FileSelectPhase(nameof(FadeOutToTitle));

    /// <summary>Finish the entry fade-out and blank the display, then wait out one update.</summary>
    public sealed partial record EnterBlankScreen() : FileSelectPhase(nameof(EnterBlankScreen));

    /// <summary>Continuation of initialization after its frame wait; the next update advances to the black hold without another per-update random advance.</summary>
    public sealed partial record EnterBlankScreenAfterFrameWait() : FileSelectPhase(nameof(EnterBlankScreenAfterFrameWait));

    /// <summary>Holds the blanked screen black (<see cref="FileSelectLayout.BlackHoldUpdates"/>) before the screen's setup.</summary>
    public sealed partial record BlackHold() : FileSelectPhase(nameof(BlackHold));

    /// <summary>Sets the fades to step at the menu's fade pace (<see cref="FileSelectLayout.FadeUpdatesPerStep"/>), then waits out one update.</summary>
    public sealed partial record InitializeMain() : FileSelectPhase(nameof(InitializeMain));

    /// <summary>Continuation of the screen setup after its frame wait; advance to the screen's fade-in without another per-update random advance.</summary>
    public sealed partial record InitializeMainAfterFrameWait() : FileSelectPhase(nameof(InitializeMainAfterFrameWait));

    /// <summary>The phases of the copy and clear pages, which data management steps.</summary>
    public abstract partial record DataManagement : FileSelectPhase
    {
        /// <summary>Allows only the cases below, each named <paramref name="name"/>.</summary>
        private DataManagement(string name)
            : base(name)
        {
        }

        /// <summary>Choose an occupied source slot for copying, or leave through Exit/B.</summary>
        public sealed partial record CopySelectSource() : DataManagement(nameof(CopySelectSource));

        /// <summary>Choose any destination slot other than the source, including an occupied slot that will be overwritten.</summary>
        public sealed partial record CopySelectDestination() : DataManagement(nameof(CopySelectDestination));

        /// <summary>Animate the copy arrow and accept Yes/No; accepting Yes also performs the copy in this phase.</summary>
        public sealed partial record CopyConfirm : DataManagement
        {
            /// <summary>Confirms the copy <paramref name="route"/>.</summary>
            internal CopyConfirm(FileCopyRoute route)
                : base(nameof(CopyConfirm)) => Route = route;

            /// <summary>The copy being confirmed, from its source onto another slot.</summary>
            internal FileCopyRoute Route { get; }
        }

        /// <summary>Display the completed copy without a selection missile and wait for a newly pressed button before returning to main.</summary>
        public sealed partial record CopyCompleted() : DataManagement(nameof(CopyCompleted));

        /// <summary>Choose an occupied slot to clear, or leave through Exit/B.</summary>
        public sealed partial record ClearSelectSlot() : DataManagement(nameof(ClearSelectSlot));

        /// <summary>Accept Yes/No for the chosen slot; accepting Yes also clears the slot in this phase.</summary>
        public sealed partial record ClearConfirm() : DataManagement(nameof(ClearConfirm));

        /// <summary>Display the completed clear without a selection missile and wait for a newly pressed button before returning to main.</summary>
        public sealed partial record ClearCompleted() : DataManagement(nameof(ClearCompleted));
    }
}
