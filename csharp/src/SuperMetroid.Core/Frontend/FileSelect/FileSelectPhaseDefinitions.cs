namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What each of the file-select menu's phases shows and leads to (<see cref="FileSelectMenuState"/>).</summary>
internal static class FileSelectPhaseDefinitions
{
    /// <summary>
    /// The phases that show the selected helmet's turn: the turn itself and the fade out to the options it leads to, through
    /// which the helmet keeps its last turned picture.
    /// </summary>
    internal static readonly FileSelectPhase[] SelectedHelmetTurnPhases =
        [FileSelectPhase.TurnSelectedHelmet, FileSelectPhase.FadeOutToOptions];

    /// <summary>The phases that draw the copy arrow from the chosen source to the chosen target: the copy's confirmation.</summary>
    internal static readonly FileSelectPhase[] CopyArrowPhases = [FileSelectPhase.CopyConfirm];

    /// <summary>
    /// The phases of the menu's opening, before the initialized main page begins fading in, in which the selection missile
    /// is neither drawn nor animated.
    /// </summary>
    internal static readonly FileSelectPhase[] EntryPhases =
    [
        FileSelectPhase.EnterBlankScreen,
        FileSelectPhase.EnterBlankScreenAfterFrameWait,
        FileSelectPhase.BlackHold,
        FileSelectPhase.InitializeMain,
        FileSelectPhase.InitializeMainAfterFrameWait,
    ];

    /// <summary>The completed data-operation pages, which show no selection missile.</summary>
    internal static readonly FileSelectPhase[] CompletedOperationPhases =
        [FileSelectPhase.CopyCompleted, FileSelectPhase.ClearCompleted];

    /// <summary>The confirmation pages, whose cursor stands on the Yes and No rows.</summary>
    internal static readonly FileSelectPhase[] ConfirmationPhases =
        [FileSelectPhase.CopyConfirm, FileSelectPhase.ClearConfirm];

    /// <summary>The fade outs to a data-management page, each with the mode of the page it leads to.</summary>
    internal static readonly Dictionary<FileSelectPhase, FileSelectDataMode> DataManagementFadeOuts = new()
    {
        [FileSelectPhase.FadeOutToCopy] = FileSelectDataMode.Copy,
        [FileSelectPhase.FadeOutToClear] = FileSelectDataMode.Clear,
    };

    /// <summary>The fade outs back to the main page.</summary>
    internal static readonly FileSelectPhase[] MainPageFadeOuts = [FileSelectPhase.FadeOutToMain];
}
