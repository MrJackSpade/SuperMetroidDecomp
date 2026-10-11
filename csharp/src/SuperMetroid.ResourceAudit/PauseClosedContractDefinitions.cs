namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored pause providers; this does not certify inventory logic, input, sound or menu pixels.</summary>
internal static class PauseClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.PauseEquipmentBasePresentation", "pause-equipment-complete-base-image",
            ["CreateTilemap", "RebindBaseInto", "RebindBeforeInventoryRefreshInto"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs", "3DA0E70000CC9F9A44B70094D4B26E86939F29C7BFC4F91C9D615EA4B3E71B40"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "5344EBE981424B92D64D6794B092977A9A2F47AF139A2D24CD30DF3D6DA265BB"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs", "8A1A1C78A73DD86445BE1F06AA4BDBE44D1A5F4CA9DF3916BDB9414ED245D3F4"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "8824F79E6DEAF496E13824D2B2705491D527C6B58D552BCB529F31BB48C99B08"),
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
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs", "A97725D412DFA49C6F0087F3BD7747A13C0BCE0AC84CA44413EF4FA09CE830FF"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "8824F79E6DEAF496E13824D2B2705491D527C6B58D552BCB529F31BB48C99B08"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "61D2E4392089E2E7CA91A9886B6B514FB4C4DA6E965C6BC743C65C275F7106B0"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "8DA0EB3ACAC6D10737322F7DF139EB50153CBF7B7804FE8EF93711661911FD01"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "5344EBE981424B92D64D6794B092977A9A2F47AF139A2D24CD30DF3D6DA265BB"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "11692E4C29574E1E271A9909DE1D2B7C610CFBC09DF70D2E3ACAB84D6510C92D"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs", "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301")]),
        new("SuperMetroid.Core.Assets.PauseSelectorPresentation", "pause-selector-complete-anchors-and-bound-phases", ["Anchor", "NormalizePhase", "Duration", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorPresentation.cs", "47A3E4AECB488ACFB5A23C340FAAFF3107999232784A536951D9A64F27C3ADC6"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorVisual.cs", "085D77032BAD5D490B25B1174F26748F491E6FB3E5E6999EED2B8D9419669B55"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "B4CACFDB54AC66C639716216773559EB030B1AF1AAD935EEBC4A78404C17E71F"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "61D2E4392089E2E7CA91A9886B6B514FB4C4DA6E965C6BC743C65C275F7106B0"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "8DA0EB3ACAC6D10737322F7DF139EB50153CBF7B7804FE8EF93711661911FD01")]),
        new("SuperMetroid.Core.Assets.PauseReserveTankPresentation", "pause-reserve-complete-anchors-and-sprites", ["Anchor", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankPresentation.cs", "1BD7D5DF9A8D584BEB6FEB7B4299258529E240D5610326851A610F79775E7CAC"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankDefinitions.cs", "A65792E8E83A012C5C67926E62DDC9B357E8CEEF6298E547EB46456DE74FB9E3"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseReserveTankRomData.cs", "011A0581AA6A3B766B1DE09A6CE4675773A0A7858473E2010261DD9507A58EB1")]),
    ];
}
