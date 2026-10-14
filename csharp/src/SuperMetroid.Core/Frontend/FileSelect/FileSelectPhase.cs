namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Debugger-facing file-select phases, including combined native data-management steps and explicit NMI-wait continuations; enum ordinals are not native menu indices.</summary>
public enum FileSelectPhase
{
    /// <summary>$81:A058, native index $03: brighten the initialized main screen until its brightness reaches 15.</summary>
    FadeIn,
    /// <summary>$81:A1C2, native index $04: accept slot, Copy, Clear and Exit navigation; data-management items are skipped when all slots are empty.</summary>
    Main,
    /// <summary>$81:94EE, native indices $05/$13: fade the main screen to forced blank, then initialize the selected Copy or Clear page.</summary>
    FadeOutToDataManagement,
    /// <summary>Brighten the newly built Copy, Clear or main page; combines native submenu fade-ins at $81:9532 and return-to-main fade-ins at $81:951E.</summary>
    FadeInFromDataManagement,
    /// <summary>$81:96C2, native index $08: choose a checksummed, occupied source slot for copying, or leave through Exit/B.</summary>
    CopySelectSource,
    /// <summary>$81:9813, native index $0A: choose any destination slot other than the source, including an occupied slot that will be overwritten.</summary>
    CopySelectDestination,
    /// <summary>$81:9984, native index $0C: animate the copy arrow and accept Yes/No; accepting Yes also performs the native $0D copy operation in this phase.</summary>
    CopyConfirm,
    /// <summary>$81:9AFA, native index $0E: display the completed copy without a selection missile and wait for a newly pressed button before returning to main.</summary>
    CopyCompleted,
    /// <summary>$81:9B64, native index $16: choose a checksummed, occupied slot to clear, or leave through Exit/B.</summary>
    ClearSelectSlot,
    /// <summary>$81:9C36, native index $18: accept Yes/No for the chosen slot; accepting Yes also performs the native $19 SRAM clear operation in this phase.</summary>
    ClearConfirm,
    /// <summary>$81:9D26, native index $1A: display the completed clear without a selection missile and wait for a newly pressed button before returning to main.</summary>
    ClearCompleted,
    /// <summary>$81:94F4, native indices $0F/$1B: fade the data-management page to forced blank, then rebuild main from the updated slot contents.</summary>
    FadeOutToMain,
    /// <summary>$81:9D77, native index $1F: turn only the selected slot's helmet; a new Start/A press can end the animation early.</summary>
    TurnSelectedHelmet,
    /// <summary>$81:94A3, native index $20: fade the selected helmet and main page to forced blank, then signal the game-options handoff.</summary>
    FadeOutToOptions,
    /// <summary>$81:94D5, native index $21: fade to forced blank after Exit/B, then request the host's reset path corresponding to native SoftReset.</summary>
    FadeOutToTitle,
    /// <summary>$81:944E, native index $00: finish entry fade-out and establish forced blank before the dispatch's internal NMI wait.</summary>
    EnterBlankScreen,
    /// <summary>Continuation of native index $00 after its internal NMI wait; the next update advances to background loading without another main-loop RNG call.</summary>
    EnterBlankScreenAfterNmi,
    /// <summary>$81:9E93, native index $01: represent the Zebes/starfield BG2 transfer stage; installed graphics were loaded atomically by construction.</summary>
    LoadBackground,
    /// <summary>$81:9ED6, native index $02: set both fade timing words to one and release forced blank before its internal NMI wait.</summary>
    InitializeMain,
    /// <summary>Continuation of native index $02 after its internal NMI wait; advance to main-screen fade-in without another main-loop RNG call.</summary>
    InitializeMainAfterNmi,
}
