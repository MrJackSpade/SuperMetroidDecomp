using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// The game's automatic saves: the gameplay events that write the game's ordinary save without a save station's
/// prompt.
/// </summary>
internal static class AutomaticCheckpointSaver
{
    /// <summary>
    /// The Ceres opening publishes its checkpoint game state before saving the initial Ceres checkpoint.
    /// </summary>
    public static void SaveCeresArrival(SaveSlotStore saves, SuperMetroidRuntime runtime, SaveSlot slot) =>
        SaveCeresCheckpoint(saves, runtime, slot, SaveLoadingGameState.CeresElevatorArrival);

    /// <summary>
    /// The escape ending publishes its checkpoint game state before saving at the completed escape blackout.
    /// This checkpoint resumes the destruction cinematic, never the escaping room.
    /// </summary>
    public static void SaveCeresDeparture(SaveSlotStore saves, SuperMetroidRuntime runtime, SaveSlot slot) =>
        SaveCeresCheckpoint(saves, runtime, slot, SaveLoadingGameState.CeresDestruction);

    /// <summary>Publishes the requested Ceres loading state, then saves Samus, system state, game time, and player options through the ordinary save-file encoder.</summary>
    private static void SaveCeresCheckpoint(
        SaveSlotStore saves, SuperMetroidRuntime runtime, SaveSlot slot, SaveLoadingGameState route)
    {
        Ensure.NotNull(saves);
        Ensure.NotNull(runtime);
        runtime.System.LoadSavedStartupRoute(route);
        saves.SaveSlot(slot, runtime.SaveSnapshotAt(LoadStationDefinitions.CeresElevator));
    }

    /// <summary>
    /// Saves at the Landing Site gunship on the single frame where the gunship restores
    /// control after the arrival cutscene. Returns false on every other gameplay frame.
    /// </summary>
    public static bool TrySaveGunshipLanding(
        SaveSlotStore saves,
        SuperMetroidRuntime runtime,
        SaveSlot selectedSaveSlot)
    {
        Ensure.NotNull(saves);
        Ensure.NotNull(runtime);
        if (runtime.EnemyFrameResults.LastEnemyFrame.Gunship != GunshipFrameEvent.LandingCompleted)
            return false;

        // The gunship sets the ship-landed marker before moving the checkpoint from the Zebes arrival to
        // the gunship and saving the game. The marker controls ship/save-point display
        // after reload and therefore belongs inside the atomic checkpoint operation.
        runtime.System.LoadSavedStartupRoute(SaveLoadingGameState.MainGame);
        runtime.System.MarkSaveStationUsed(LoadStationDefinitions.Gunship);
        saves.SaveSlot(selectedSaveSlot, runtime.SaveSnapshotAt(LoadStationDefinitions.Gunship));
        return true;
    }
}
