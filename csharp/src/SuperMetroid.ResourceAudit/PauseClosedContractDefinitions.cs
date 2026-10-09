namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored pause providers; this does not certify inventory logic, input, sound or menu pixels.</summary>
internal static class PauseClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.PauseEquipmentBasePresentation", "pause-equipment-complete-base-image",
            ["CreateTilemap", "RebindBaseInto", "RebindBeforeInventoryRefreshInto"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs", "3DA0E70000CC9F9A44B70094D4B26E86939F29C7BFC4F91C9D615EA4B3E71B40"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "C7586C36B3638F4524B95D9D3A731784E7FAB790B88BD074E23E5CD31DE4410B"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs", "8A1A1C78A73DD86445BE1F06AA4BDBE44D1A5F4CA9DF3916BDB9414ED245D3F4"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "B4DBBAEF840C54C2B1AAA679183B2E1E38E55A49970479D1D1A6C2ADB067B532"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "11692E4C29574E1E271A9909DE1D2B7C610CFBC09DF70D2E3ACAB84D6510C92D"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs", "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "8DA0EB3ACAC6D10737322F7DF139EB50153CBF7B7804FE8EF93711661911FD01")]),
        new("SuperMetroid.Core.Assets.PauseBackdropPresentation", "pause-backdrop-complete-area-images", ["CreateButtonTilemap", "LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs", "7A1D73D3BA8D2B405361B80B7937CDDE2F511FBF3A8BB741D85E76EEE0F846B5"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropDefinitions.cs", "7499F8AE9B15E860C7F6343E6F376F0DD5578BE728D4978B0FF44C401FCF8EB0"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "11692E4C29574E1E271A9909DE1D2B7C610CFBC09DF70D2E3ACAB84D6510C92D"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "34967995C1C41594033A1B1824A26C2181EE2D90862E68A15EF36D53899F35B5")]),
        new("SuperMetroid.Core.Assets.PauseWireframePresentation", "pause-wireframe-complete-suit-images", ["ApplyTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs", "133B9D1360CBC2CB93430014A710E1F5DB9DC283507AF6664A3FE953DDC57BDD"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs", "8A1A1C78A73DD86445BE1F06AA4BDBE44D1A5F4CA9DF3916BDB9414ED245D3F4"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "11692E4C29574E1E271A9909DE1D2B7C610CFBC09DF70D2E3ACAB84D6510C92D")]),
        new("SuperMetroid.Core.Assets.PauseEquipmentLabelPresentation", "pause-label-complete-category-item-domain", ["ApplyInventory", "ApplyLabel", "OwnsLiveCell"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs", "C3EB77C021FA1E5F63C998FE437A13BE7CA29B7B046E7C6D18573321B9E37A60"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "B4DBBAEF840C54C2B1AAA679183B2E1E38E55A49970479D1D1A6C2ADB067B532"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "8AD8DC65936856C9F9659DBB12FA0B8802CD0F9942C7539F0E4607BF7E014010"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "8DA0EB3ACAC6D10737322F7DF139EB50153CBF7B7804FE8EF93711661911FD01"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "C7586C36B3638F4524B95D9D3A731784E7FAB790B88BD074E23E5CD31DE4410B"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "11692E4C29574E1E271A9909DE1D2B7C610CFBC09DF70D2E3ACAB84D6510C92D"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs", "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301")]),
        new("SuperMetroid.Core.Assets.PauseSelectorPresentation", "pause-selector-complete-anchors-and-bound-phases", ["Anchor", "NormalizePhase", "Duration", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorPresentation.cs", "A9A0662FD3B02505A32220D0A9D051F296B17E0750A32F6FB552E6CD7226B49D"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorVisual.cs", "085D77032BAD5D490B25B1174F26748F491E6FB3E5E6999EED2B8D9419669B55"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "B4CACFDB54AC66C639716216773559EB030B1AF1AAD935EEBC4A78404C17E71F"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "8AD8DC65936856C9F9659DBB12FA0B8802CD0F9942C7539F0E4607BF7E014010"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "8DA0EB3ACAC6D10737322F7DF139EB50153CBF7B7804FE8EF93711661911FD01")]),
        new("SuperMetroid.Core.Assets.PauseReserveTankPresentation", "pause-reserve-complete-anchors-and-sprites", ["Anchor", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankPresentation.cs", "E4E1A69CD63241C9C542EE7AB69BB88D2A9F16202BD297CE0C688E71BD5CB9EB"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankDefinitions.cs", "551886AC446BBD0F48A12E373BF71C42CFBD60ABE8C673E4D6D2E269C7E57A78"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseReserveTankRomData.cs", "F0BF08D4CF7499A161FFC46FD12639F4246B297B97A47CB646DDA83DB2F48DF4")]),
    ];
}
