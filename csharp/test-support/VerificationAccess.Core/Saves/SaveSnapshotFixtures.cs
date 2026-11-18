using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Complete save snapshots and snapshot contexts for verification fixtures, which state only the members a check is about
/// and take the rest from a fresh game.
/// </summary>
internal static class SaveSnapshotFixtures
{
    /// <summary>
    /// A fresh game's save at the gunship: default controls and options, 99 energy of 99 and nothing else collected, a zero
    /// clock, the main game's startup route and no progress. Each read is a new snapshot with its own progress.
    /// </summary>
    internal static SuperMetroidSaveSnapshot NewGame => new()
    {
        ControllerBindings = ControllerBindings.Default,
        MoonwalkEnabled = false,
        IconCancelEnabled = false,
        JapaneseText = false,
        EquippedItems = SamusEquipmentFlags.None,
        CollectedItems = SamusEquipmentFlags.None,
        EquippedBeams = SamusBeamFlags.None,
        CollectedBeams = SamusBeamFlags.None,
        ReserveMode = ReserveTankMode.None,
        Health = 99,
        MaxHealth = 99,
        Missiles = 0,
        MaxMissiles = 0,
        SuperMissiles = 0,
        MaxSuperMissiles = 0,
        PowerBombs = 0,
        MaxPowerBombs = 0,
        HudItem = HudItem.Beam,
        MaxReserveEnergy = 0,
        ReserveEnergy = 0,
        GameTime = default,
        StartupRoute = SaveLoadingGameState.MainGame,
        Checkpoint = LoadStationDefinitions.Gunship,
        Progress = new(),
    };

    /// <summary>A save made at <paramref name="checkpoint"/> with a zero clock, default controls and every option off.</summary>
    internal static SaveSnapshotContext At(LoadStation checkpoint) => new()
    {
        Checkpoint = checkpoint,
        GameTime = default,
        ControllerBindings = ControllerBindings.Default,
        MoonwalkEnabled = false,
        IconCancelEnabled = false,
        JapaneseText = false,
    };
}
