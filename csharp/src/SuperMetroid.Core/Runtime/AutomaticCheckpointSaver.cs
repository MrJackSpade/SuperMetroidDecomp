using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Owns gameplay events that write the game's ordinary save
/// without presenting a save-station prompt. Keeping this out of the outer dispatcher makes
/// each automatic trigger independently testable while retaining one production save path.
/// </summary>
internal static class AutomaticCheckpointSaver
{
    /// <summary>
    /// The Ceres opening publishes its checkpoint game state before saving the initial Ceres checkpoint.
    /// </summary>
    public static void SaveCeresArrival(SaveSlotStore saves, SuperMetroidRuntime runtime, int slot) =>
        SaveCeresCheckpoint(saves, runtime, slot, SaveLoadingGameState.CeresElevatorArrival);

    /// <summary>
    /// The escape ending publishes its checkpoint game state before saving at the completed escape blackout.
    /// This checkpoint resumes the destruction cinematic, never the escaping room.
    /// </summary>
    public static void SaveCeresDeparture(SaveSlotStore saves, SuperMetroidRuntime runtime, int slot) =>
        SaveCeresCheckpoint(saves, runtime, slot, SaveLoadingGameState.CeresDestruction);

    /// <summary>Publishes the requested Ceres loading state, then saves Samus, system state, game time, and player options through the ordinary SRAM encoder.</summary>
    private static void SaveCeresCheckpoint(
        SaveSlotStore saves, SuperMetroidRuntime runtime, int slot, SaveLoadingGameState route)
    {
        Ensure.NotNull(saves);
        Ensure.NotNull(runtime);
        SamusState samus = runtime.Player.Samus
            ?? throw new InvalidOperationException("Ceres checkpoint requires a live Samus actor.");
        runtime.System.LoadSavedLoadingGameState(SaveLoadingGameStateCodec.Word(route));
        saves.SaveSlot(slot, SuperMetroidSaveSnapshot.Capture(samus, runtime.System, new SaveSnapshotContext { Area = AreaId.Ceres, SaveStation = 0, GameTime = runtime.GameTime, ControllerBindings = runtime.Options.ControllerBindings, MoonwalkEnabled = runtime.Options.MoonwalkEnabled, IconCancelEnabled = runtime.Options.IconCancelEnabled, JapaneseText = runtime.JapaneseText }));
    }

    /// <summary>
    /// Saves Crateria station zero on the single frame where <c>GunshipTop_7</c> restores
    /// control after the arrival cutscene. Returns false on every other gameplay frame.
    /// </summary>
    public static bool TrySaveGunshipLanding(
        SaveSlotStore saves,
        SuperMetroidRuntime runtime,
        int selectedSaveSlot)
    {
        Ensure.NotNull(saves);
        Ensure.NotNull(runtime);
        if (runtime.EnemyFrameResults.LastEnemyFrame.Gunship != GunshipFrameEvent.LandingCompleted)
            return false;

        SamusState samus = runtime.Player.Samus
            ?? throw new InvalidOperationException(
                "Gunship landing completed without a live Samus actor.");

        // // The gunship sets the ship-landed marker before changing load station 18 to
        // station zero and calling SaveToSram. The marker controls ship/save-point display
        // after reload and therefore belongs inside the atomic checkpoint operation.
        runtime.System.LoadSavedLoadingGameState(SaveLoadingGameStateCodec.Word(SaveLoadingGameState.MainGame));
        runtime.System.MarkSaveStationUsed(area: AreaId.Crateria, stationBitIndex: 0);
        saves.SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, runtime.System, new SaveSnapshotContext { Area = 0, SaveStation = 0, GameTime = runtime.GameTime, ControllerBindings = runtime.Options.ControllerBindings, MoonwalkEnabled = runtime.Options.MoonwalkEnabled, IconCancelEnabled = runtime.Options.IconCancelEnabled, JapaneseText = runtime.JapaneseText }));
        return true;
    }
}
