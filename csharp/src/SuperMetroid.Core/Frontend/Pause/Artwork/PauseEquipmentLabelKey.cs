namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Which equipment-screen label names an item: the category and item and its label.</summary>
/// <param name="Category">The equipment category.</param>
/// <param name="Item">The item within the category.</param>
/// <param name="Label">The item's label.</param>
public readonly record struct PauseEquipmentLabelKey(PauseEquipmentCategory Category, int Item, PauseEquipmentLabelId Label);
