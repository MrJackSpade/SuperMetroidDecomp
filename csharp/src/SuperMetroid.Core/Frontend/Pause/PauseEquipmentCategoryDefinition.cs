namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>One equipment category of the pause screen: how many items it lists and how many cells each item's label spans.</summary>
internal readonly record struct PauseEquipmentCategoryDefinition(
    PauseEquipmentCategory Category,
    int ItemCount,
    int LabelWordCount);
