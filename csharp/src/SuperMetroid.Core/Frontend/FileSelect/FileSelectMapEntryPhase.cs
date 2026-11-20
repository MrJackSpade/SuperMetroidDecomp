namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The file-select map's entry stages before normal area-map input becomes active.</summary>
public enum FileSelectMapEntryPhase
{
    /// <summary>Holds the screen black before the reveal (<see cref="FileSelectMapDefinitions.EntryBlackHoldUpdates"/>).</summary>
    BlackHold,
    /// <summary>Expands the reveal window by <see cref="FileSelectMapDefinitions.EntryWindowSpeed"/> each update.</summary>
    Revealing,
    /// <summary>Displays the complete area scene and permits normal map input.</summary>
    Complete
}
