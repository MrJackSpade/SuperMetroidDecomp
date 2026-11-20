namespace SuperMetroid.Core.Frontend;

/// <summary>Which screen of choosing a game (<see cref="Progression.SuperMetroidGameState.FileSelect"/>) runs.</summary>
internal enum FileSelectStep
{
    /// <summary>The save slots, with copying and clearing.</summary>
    SaveSlots,
    /// <summary>The selected game's options, before a new game, its introduction or a saved game's map.</summary>
    Options,
    /// <summary>A saved game's area map, where the player chooses the save station to continue from.</summary>
    AreaMap,
}
