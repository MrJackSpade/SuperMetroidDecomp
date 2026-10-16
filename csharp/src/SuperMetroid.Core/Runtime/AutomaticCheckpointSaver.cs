using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Owns gameplay events that execute the cartridge's ordinary <c>$81:8000</c> SRAM encoder
/// without presenting a save-station prompt. Keeping this out of the outer dispatcher makes
/// each native trigger independently testable while retaining one production save path.
/// </summary>
internal static class AutomaticCheckpointSaver
{
    /// <summary>
    /// $8B:C100-$C116 publishes $1F before saving the initial Ceres checkpoint.
    /// </summary>
    public static void SaveCeresArrival(ISnesAddressSpace bus, SuperMetroidRuntime runtime, int slot) =>
        SaveCeresCheckpoint(bus, runtime, slot, SaveLoadingGameStates.CeresElevatorArrival);

    /// <summary>
    /// $82:83D6-$83E3 publishes $22 before saving at the completed escape blackout.
    /// This checkpoint resumes the destruction cinematic, never the escaping room.
    /// </summary>
    public static void SaveCeresDeparture(ISnesAddressSpace bus, SuperMetroidRuntime runtime, int slot) =>
        SaveCeresCheckpoint(bus, runtime, slot, SaveLoadingGameStates.CeresDestruction);

    /// <summary>Publishes the requested Ceres loading state, then saves Samus, system state, game time, and player options through the ordinary SRAM encoder.</summary>
    private static void SaveCeresCheckpoint(
        ISnesAddressSpace bus, SuperMetroidRuntime runtime, int slot, ushort loadingGameState)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(runtime);
        SamusState samus = runtime.Player.Samus
            ?? throw new InvalidOperationException("Ceres checkpoint requires a live Samus actor.");
        runtime.System.LoadSavedLoadingGameState(loadingGameState);
        new SuperMetroidSaveRam(bus, runtime.Artwork.MapPresentation).SaveSlot(slot, SuperMetroidSaveSnapshot.Capture(samus, runtime.System, new SaveSnapshotContext { Area = (ushort)AreaId.Ceres, SaveStation = 0, GameTime = runtime.GameTime, ControllerBindings = runtime.Options.ControllerBindings, MoonwalkEnabled = runtime.Options.MoonwalkEnabled, IconCancelEnabled = runtime.Options.IconCancelEnabled, JapaneseText = runtime.JapaneseText }));
    }

    /// <summary>
    /// Saves Crateria station zero on the single frame where <c>GunshipTop_7</c> restores
    /// control after the arrival cutscene. Returns false on every other gameplay frame.
    /// </summary>
    public static bool TrySaveGunshipLanding(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        int selectedSaveSlot)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(runtime);
        if (runtime.EnemyFrameResults.LastEnemyFrame.Gunship != GunshipFrameEvent.LandingCompleted)
            return false;

        SamusState samus = runtime.Player.Samus
            ?? throw new InvalidOperationException(
                "Gunship landing completed without a live Samus actor.");

        // `$A2:A987` executes `ORA #$0001` on $D8F8 before changing load station 18 to
        // station zero and calling SaveToSram. The marker controls ship/save-point display
        // after reload and therefore belongs inside the atomic checkpoint operation.
        runtime.System.LoadSavedLoadingGameState(SaveLoadingGameStates.MainGame);
        runtime.System.MarkSaveStationUsed(areaIndex: AreaId.Crateria, stationBitIndex: 0);
        new SuperMetroidSaveRam(bus, runtime.Artwork.MapPresentation).SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(samus, runtime.System, new SaveSnapshotContext { Area = 0, SaveStation = 0, GameTime = runtime.GameTime, ControllerBindings = runtime.Options.ControllerBindings, MoonwalkEnabled = runtime.Options.MoonwalkEnabled, IconCancelEnabled = runtime.Options.IconCancelEnabled, JapaneseText = runtime.JapaneseText }));
        return true;
    }
}
