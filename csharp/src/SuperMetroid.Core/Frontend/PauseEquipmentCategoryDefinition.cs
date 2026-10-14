namespace SuperMetroid.Core.Frontend;

/// <summary>One bank-$82 equipment-category table record used by the pause screen.</summary>
internal readonly record struct PauseEquipmentCategoryDefinition(
    PauseEquipmentCategory Category,
    int OffsetTableAddress,
    int TilemapPointerTableAddress,
    int ItemCount,
    int LabelWordCount);
