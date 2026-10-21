namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>An item within a pause equipment category.</summary>
/// <param name="Category">The category.</param>
/// <param name="Item">The item.</param>
internal readonly record struct PauseEquipmentSlot(PauseEquipmentCategory Category, int Item);
