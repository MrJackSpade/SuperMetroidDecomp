namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File-select phases, including combined data-management steps and explicit frame-wait continuations.</summary>
public abstract record FileSelectPhase
{
    /// <summary>Allows only the cases below.</summary>
    private FileSelectPhase()
    {
    }

    /// <summary>Brighten the initialized screen until it is fully lit.</summary>
    public sealed record FadeIn : FileSelectPhase;

    /// <summary>Accept slot, Copy, Clear and Exit navigation; data-management items are skipped when all slots are empty.</summary>
    public sealed record Main : FileSelectPhase;

    /// <summary>Fade the screen to black, then build the Copy page on the first occupied slot.</summary>
    public sealed record FadeOutToCopy : FileSelectPhase;

    /// <summary>Fade the screen to black, then build the Clear page on the first occupied slot.</summary>
    public sealed record FadeOutToClear : FileSelectPhase;

    /// <summary>Brighten the newly built Copy, Clear or main page; covers both the submenu and the return-to-main fade-ins.</summary>
    public sealed record FadeInFromDataManagement : FileSelectPhase;

    /// <summary>Fade the data-management page to black, then rebuild main from the updated slot contents.</summary>
    public sealed record FadeOutToMain : FileSelectPhase;

    /// <summary>Turn only the selected slot's helmet; a new Start/A press can end the animation early.</summary>
    public sealed record TurnSelectedHelmet : FileSelectPhase;

    /// <summary>Fade the selected helmet and main page to black, then signal the game-options handoff.</summary>
    public sealed record FadeOutToOptions : FileSelectPhase;

    /// <summary>Fade to black after Exit/B, then request the host's soft-reset path.</summary>
    public sealed record FadeOutToTitle : FileSelectPhase;

    /// <summary>Finish the entry fade-out and blank the display, then wait out one update.</summary>
    public sealed record EnterBlankScreen : FileSelectPhase;

    /// <summary>Continuation of initialization after its frame wait; the next update advances to the black hold without another per-update random advance.</summary>
    public sealed record EnterBlankScreenAfterFrameWait : FileSelectPhase;

    /// <summary>Holds the blanked screen black (<see cref="FileSelectLayout.BlackHoldUpdates"/>) before the screen's setup.</summary>
    public sealed record BlackHold : FileSelectPhase;

    /// <summary>Sets the fades to step at the menu's fade pace (<see cref="FileSelectLayout.FadeUpdatesPerStep"/>), then waits out one update.</summary>
    public sealed record InitializeMain : FileSelectPhase;

    /// <summary>Continuation of the screen setup after its frame wait; advance to the screen's fade-in without another per-update random advance.</summary>
    public sealed record InitializeMainAfterFrameWait : FileSelectPhase;

    /// <summary>The phases of the copy and clear pages, which data management steps.</summary>
    public abstract record DataManagement : FileSelectPhase
    {
        /// <summary>Allows only the cases below.</summary>
        private DataManagement()
        {
        }

        /// <summary>Choose an occupied source slot for copying, or leave through Exit/B.</summary>
        public sealed record CopySelectSource : DataManagement;

        /// <summary>Choose any destination slot other than the source, including an occupied slot that will be overwritten.</summary>
        public sealed record CopySelectDestination : DataManagement;

        /// <summary>Animate the copy arrow and accept Yes/No; accepting Yes also performs the copy in this phase.</summary>
        public sealed record CopyConfirm : DataManagement;

        /// <summary>Display the completed copy without a selection missile and wait for a newly pressed button before returning to main.</summary>
        public sealed record CopyCompleted : DataManagement;

        /// <summary>Choose an occupied slot to clear, or leave through Exit/B.</summary>
        public sealed record ClearSelectSlot : DataManagement;

        /// <summary>Accept Yes/No for the chosen slot; accepting Yes also clears the slot in this phase.</summary>
        public sealed record ClearConfirm : DataManagement;

        /// <summary>Display the completed clear without a selection missile and wait for a newly pressed button before returning to main.</summary>
        public sealed record ClearCompleted : DataManagement;
    }
}
