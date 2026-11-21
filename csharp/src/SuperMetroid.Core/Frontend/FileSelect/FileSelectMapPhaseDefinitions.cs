namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>What each of the save-selection map's navigation phases does (<see cref="FileSelectMapMenuState"/>).</summary>
internal static class FileSelectMapPhaseDefinitions
{
    /// <summary>
    /// The phases the room map is navigated in: an update that begins in one steps the map's arrows and scrolls it, and
    /// entering one sets up the room map's scroll, station marker and highlight.
    /// </summary>
    internal static readonly FileSelectMapNavigationPhase[] RoomMapPhases = [FileSelectMapNavigationPhase.Room];

    /// <summary>
    /// The phases whose updates step the room map's highlight colours and station marker before input is read: the room
    /// map itself and the load it requests, through the load's fade.
    /// </summary>
    internal static readonly FileSelectMapNavigationPhase[] IconAnimatingPhases =
        [FileSelectMapNavigationPhase.Room, FileSelectMapNavigationPhase.LoadRequested];

    /// <summary>The sound each phase plays on the update navigation enters it; a phase not listed enters silently.</summary>
    internal static readonly Dictionary<FileSelectMapNavigationPhase, SoundEffectId> EntrySounds = new()
    {
        [FileSelectMapNavigationPhase.PreparingWindow] = SamusSound.MenuConfirm,
        [FileSelectMapNavigationPhase.LoadRequested] = SamusSound.MenuConfirm,
        [FileSelectMapNavigationPhase.ExpandingWindow] = SamusSound.MapExpand,
        [FileSelectMapNavigationPhase.AreaReturnRequested] = SamusSound.MapReturn,
    };
}
