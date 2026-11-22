using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// What the selected game's setup loads (#2560): a new file, the selected slot's save, or the Ceres restart after the
/// Spacetime Beam's intro with the save whose inventory it keeps. One value, chosen as the game is selected.
/// </summary>
internal abstract partial record SelectedGameStart
{
    /// <summary>A new file: a fresh Ceres game.</summary>
    internal sealed partial record NewFile : SelectedGameStart;

    /// <summary>The selected slot's save.</summary>
    internal sealed partial record SavedGame : SelectedGameStart;

    /// <summary>The Ceres restart after the Spacetime Beam's intro, from fresh progression with the save's inventory.</summary>
    /// <param name="Slot">The save whose live inventory the restart keeps.</param>
    internal sealed partial record SpacetimeRestart(SuperMetroidSaveSlot Slot) : SelectedGameStart;
}
