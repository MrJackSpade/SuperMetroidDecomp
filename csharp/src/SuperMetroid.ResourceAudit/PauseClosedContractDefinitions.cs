namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored pause providers; this does not certify inventory logic, input, sound or menu pixels.</summary>
internal static class PauseClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.PauseEquipmentBasePresentation", "pause-equipment-complete-base-image",
            ["CreateTilemap", "RebindBaseInto", "RebindBeforeInventoryRefreshInto"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.PauseBackdropPresentation", "pause-backdrop-complete-area-images", ["CreateButtonTilemap", "LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs")]),
        new("SuperMetroid.Core.Assets.PauseWireframePresentation", "pause-wireframe-complete-suit-images", ["ApplyTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs")]),
        new("SuperMetroid.Core.Assets.PauseEquipmentLabelPresentation", "pause-label-complete-category-item-domain", ["ApplyInventory", "ApplyLabel", "OwnsLiveCell"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.PauseSelectorPresentation", "pause-selector-complete-anchors-and-bound-phases", ["Anchor", "NormalizePhase", "Duration", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorVisual.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.PauseReserveTankPresentation", "pause-reserve-complete-anchors-and-sprites", ["Anchor", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseReserveTankRomData.cs")]),
    ];
}
