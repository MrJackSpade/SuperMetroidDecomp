namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One equipment-screen selector anchor: the category and item it marks and its name.</summary>
/// <param name="Category">The equipment category.</param>
/// <param name="Item">The item within the category.</param>
/// <param name="Name">The anchor's name.</param>
public readonly record struct PauseSelectorAnchor(PauseEquipmentCategory Category, int Item, string Name);
