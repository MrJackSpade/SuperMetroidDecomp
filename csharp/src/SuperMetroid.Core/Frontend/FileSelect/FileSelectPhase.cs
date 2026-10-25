namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Debugger-facing file-select phases, including combined data-management steps and explicit frame-wait continuations.</summary>
public enum FileSelectPhase
{
    /// <summary>Brighten the initialized main screen until its brightness reaches 15.</summary>
    FadeIn,
    /// <summary>Accept slot, Copy, Clear and Exit navigation; data-management items are skipped when all slots are empty.</summary>
    Main,
    /// <summary>Fade the main screen to forced blank, then initialize the selected Copy or Clear page.</summary>
    FadeOutToDataManagement,
    /// <summary>Brighten the newly built Copy, Clear or main page; covers both the submenu and the return-to-main fade-ins.</summary>
    FadeInFromDataManagement,
    /// <summary>Choose a checksummed, occupied source slot for copying, or leave through Exit/B.</summary>
    CopySelectSource,
    /// <summary>Choose any destination slot other than the source, including an occupied slot that will be overwritten.</summary>
    CopySelectDestination,
    /// <summary>Animate the copy arrow and accept Yes/No; accepting Yes also performs the copy in this phase.</summary>
    CopyConfirm,
    /// <summary>Display the completed copy without a selection missile and wait for a newly pressed button before returning to main.</summary>
    CopyCompleted,
    /// <summary>Choose a checksummed, occupied slot to clear, or leave through Exit/B.</summary>
    ClearSelectSlot,
    /// <summary>Accept Yes/No for the chosen slot; accepting Yes also clears the slot in this phase.</summary>
    ClearConfirm,
    /// <summary>Display the completed clear without a selection missile and wait for a newly pressed button before returning to main.</summary>
    ClearCompleted,
    /// <summary>Fade the data-management page to forced blank, then rebuild main from the updated slot contents.</summary>
    FadeOutToMain,
    /// <summary>Turn only the selected slot's helmet; a new Start/A press can end the animation early.</summary>
    TurnSelectedHelmet,
    /// <summary>Fade the selected helmet and main page to forced blank, then signal the game-options handoff.</summary>
    FadeOutToOptions,
    /// <summary>Fade to forced blank after Exit/B, then request the host's reset path corresponding to native SoftReset.</summary>
    FadeOutToTitle,
    /// <summary>Finish entry fade-out and establish forced blank before the dispatch's internal NMI wait.</summary>
    EnterBlankScreen,
    /// <summary>Continuation of initialization after its frame wait; the next update advances to background loading without another main-loop RNG call.</summary>
    EnterBlankScreenAfterFrameWait,
    /// <summary>Represent the Zebes/starfield BG2 transfer stage; installed graphics were loaded atomically by construction.</summary>
    LoadBackground,
    /// <summary>Set both fade timing words to one and release forced blank before its internal NMI wait.</summary>
    InitializeMain,
    /// <summary>Continuation of the screen setup after its frame wait; advance to main-screen fade-in without another main-loop RNG call.</summary>
    InitializeMainAfterFrameWait,
}
