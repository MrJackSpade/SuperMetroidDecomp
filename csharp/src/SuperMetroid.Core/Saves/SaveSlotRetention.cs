using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// What a saved slot keeps of a snapshot (#1421): every value is checked against the range its save field keeps, so a
/// slot never holds what a save could not, and the explored map keeps only the Zebes areas' runs of cells the stock map
/// occupies (<see cref="ExploredMapStorage"/>, <see cref="SavedMapExplorationRuns"/>).
/// </summary>
internal static class SaveSlotRetention
{
    /// <summary>The slot <paramref name="snapshot"/> saves as in <paramref name="slot"/>.</summary>
    internal static SuperMetroidSaveSlot Retain(SuperMetroidSaveSnapshot snapshot, int slot, IAreaMapSource? maps)
    {
        Ensure.NotNull(snapshot);
        ControllerBindings bindings = snapshot.ControllerBindings.RequireRetailPermutation();
        ReserveTankMode reserveMode = Defined(snapshot.ReserveMode, nameof(snapshot.ReserveMode));
        int health = Kept(snapshot.Health), maxHealth = Kept(snapshot.MaxHealth);
        int missiles = Kept(snapshot.Missiles), maxMissiles = Kept(snapshot.MaxMissiles);
        int superMissiles = Kept(snapshot.SuperMissiles), maxSuperMissiles = Kept(snapshot.MaxSuperMissiles);
        int powerBombs = Kept(snapshot.PowerBombs), maxPowerBombs = Kept(snapshot.MaxPowerBombs);
        HudItem hudItem = Defined(snapshot.HudItem, nameof(snapshot.HudItem));
        int maxReserveEnergy = Kept(snapshot.MaxReserveEnergy), reserveEnergy = Kept(snapshot.ReserveEnergy);
        int reserveMissiles = Kept(snapshot.ReserveMissiles);
        int loadedItemCount = SaveFieldValue.Require(snapshot.LoadedItemCount, nameof(snapshot.LoadedItemCount));
        int gameTimeFrames = SaveFieldValue.Require(snapshot.GameTimeFrames, nameof(snapshot.GameTimeFrames));
        int gameTimeSeconds = SaveFieldValue.Require(snapshot.GameTimeSeconds, nameof(snapshot.GameTimeSeconds));
        int gameTimeMinutes = SaveFieldValue.Require(snapshot.GameTimeMinutes, nameof(snapshot.GameTimeMinutes));
        int gameTimeHours = SaveFieldValue.Require(snapshot.GameTimeHours, nameof(snapshot.GameTimeHours));

        LoadStation checkpoint = Ensure.NotNull(snapshot.Checkpoint);
        PersistentProgress progression = ExploredMapStorage.Retained(snapshot.Progress, maps);

        return new SuperMetroidSaveSlot(
            Slot: slot,
            EquippedItems: snapshot.EquippedItems,
            CollectedItems: snapshot.CollectedItems,
            EquippedBeams: snapshot.EquippedBeams,
            CollectedBeams: snapshot.CollectedBeams,
            ReserveMode: reserveMode,
            Health: health,
            MaxHealth: maxHealth,
            Missiles: missiles,
            MaxMissiles: maxMissiles,
            SuperMissiles: superMissiles,
            MaxSuperMissiles: maxSuperMissiles,
            PowerBombs: powerBombs,
            MaxPowerBombs: maxPowerBombs,
            HudItem: hudItem,
            MaxReserveEnergy: maxReserveEnergy,
            ReserveEnergy: reserveEnergy,
            GameTimeFrames: gameTimeFrames,
            GameTimeSeconds: gameTimeSeconds,
            GameTimeMinutes: gameTimeMinutes,
            GameTimeHours: gameTimeHours,
            Progression: progression,
            StartupRoute: snapshot.StartupRoute,
            Checkpoint: checkpoint)
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = snapshot.MoonwalkEnabled,
            IconCancelEnabled = snapshot.IconCancelEnabled,
            ReserveMissiles = reserveMissiles,
            JapaneseText = snapshot.JapaneseText,
            LoadedItemCount = loadedItemCount,
        };
    }

    /// <summary>A quantity as the save keeps it: checked against the range a count holds.</summary>
    private static int Kept(int quantity) => SamusQuantity.Require(quantity);

    /// <summary>A selection the save keeps, rejected when it names no member of its kind.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is undefined.</exception>
    private static T Defined<T>(T value, string field) where T : struct, Enum =>
        Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(field, value, $"Undefined {typeof(T).Name}.");
}
