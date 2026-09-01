using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Owns gameplay events that execute the cartridge's ordinary <c>$81:8000</c> SRAM encoder
/// without presenting a save-station prompt. Keeping this out of the outer dispatcher makes
/// each native trigger independently testable while retaining one production save path.
/// </summary>
internal static class AutomaticCheckpointSaver
{
    /// <summary>
    /// Saves Crateria station zero on the single frame where <c>GunshipTop_7</c> restores
    /// control after the arrival cutscene. Returns false on every other gameplay frame.
    /// </summary>
    public static bool TrySaveGunshipLanding(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        int selectedSaveSlot)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(runtime);
        if (runtime.Enemies.LastGunshipEvent != GunshipFrameEvent.LandingCompleted)
            return false;

        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException(
                "Gunship landing completed without a live Samus actor.");

        // `$A2:A987` executes `ORA #$0001` on $D8F8 before changing load station 18 to
        // station zero and calling SaveToSram. The marker controls ship/save-point display
        // after reload and therefore belongs inside the atomic checkpoint operation.
        runtime.System.MarkSaveStationUsed(areaIndex: 0, stationBitIndex: 0);
        new SuperMetroidSaveRam(bus).SaveSlot(
            selectedSaveSlot,
            SuperMetroidSaveSnapshot.Capture(
                samus,
                runtime.System,
                area: 0,
                saveStation: 0,
                gameTime: runtime.GameTime,
                controllerBindings: runtime.ControllerBindings,
                moonwalkEnabled: runtime.MoonwalkEnabled,
                iconCancelEnabled: runtime.IconCancelEnabled));
        return true;
    }
}
