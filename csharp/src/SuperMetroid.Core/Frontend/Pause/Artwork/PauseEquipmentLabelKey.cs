namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Which equipment-screen label artwork names an item: the category and item and its label artwork key.</summary>
/// <param name="Category">The equipment category.</param>
/// <param name="Item">The item within the category.</param>
/// <param name="Key">The label's artwork key.</param>
public readonly record struct PauseEquipmentLabelKey(PauseEquipmentCategory Category, int Item, string Key);
