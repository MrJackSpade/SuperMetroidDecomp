using SuperMetroid.Core.Input;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// What a saved slot keeps of a snapshot (#1421): every value is checked against the range its save field keeps, so a
/// slot never holds what a save could not (the clock is a reading, checked when it was made), and the explored map keeps only the Zebes areas' runs of cells the stock map
/// occupies (<see cref="ExploredMapStorage"/>, <see cref="SavedMapExplorationRuns"/>).
/// </summary>
internal static class SaveSlotRetention
{
    /// <summary>The slot <paramref name="snapshot"/> saves as in <paramref name="slot"/>.</summary>
    internal static SuperMetroidSaveSlot Retain(SuperMetroidSaveSnapshot snapshot, SaveSlot slot, IAreaMapSource maps)
    {
        Ensure.NotNull(snapshot);
        ControllerBindings bindings = snapshot.ControllerBindings.RequireUniquePermutation();
        SamusInventory inventory = Kept(snapshot.Inventory);
        HudItem hudItem = Defined(snapshot.HudItem, nameof(snapshot.HudItem));

        LoadStation checkpoint = Ensure.NotNull(snapshot.Checkpoint);
        PersistentProgress progression = ExploredMapStorage.Retained(snapshot.Progress, maps);

        return new SuperMetroidSaveSlot(
            Slot: slot,
            Inventory: inventory,
            HudItem: hudItem,
            GameTime: snapshot.GameTime,
            Progression: progression,
            StartupRoute: snapshot.StartupRoute,
            Checkpoint: checkpoint)
        {
            ControllerBindings = bindings,
            MoonwalkEnabled = snapshot.MoonwalkEnabled,
            IconCancelEnabled = snapshot.IconCancelEnabled,
            Language = snapshot.Language,
        };
    }

    /// <summary>
    /// What Samus carries as the save keeps it (#2557): the reserve mode a defined one, and every count and capacity within
    /// the range a count holds; the items and beams are kept as they are.
    /// </summary>
    private static SamusInventory Kept(SamusInventory inventory) => inventory with
    {
        ReserveMode = Defined(inventory.ReserveMode, nameof(inventory.ReserveMode)),
        Health = Kept(inventory.Health),
        MaxHealth = Kept(inventory.MaxHealth),
        Missiles = Kept(inventory.Missiles),
        MaxMissiles = Kept(inventory.MaxMissiles),
        SuperMissiles = Kept(inventory.SuperMissiles),
        MaxSuperMissiles = Kept(inventory.MaxSuperMissiles),
        PowerBombs = Kept(inventory.PowerBombs),
        MaxPowerBombs = Kept(inventory.MaxPowerBombs),
        MaxReserveEnergy = Kept(inventory.MaxReserveEnergy),
        ReserveEnergy = Kept(inventory.ReserveEnergy),
    };

    /// <summary>A quantity as the save keeps it: checked against the range a count holds.</summary>
    private static int Kept(int quantity) => CountsRunBelowZero.Require(quantity);

    /// <summary>A selection the save keeps, rejected when it names no member of its kind.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is undefined.</exception>
    private static T Defined<T>(T value, string field) where T : struct, Enum =>
        Enum.IsDefined(value) ? value : throw new ArgumentOutOfRangeException(field, value, $"Undefined {typeof(T).Name}.");
}
