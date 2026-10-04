namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored pause providers; this does not certify inventory logic, input, sound or menu pixels.</summary>
internal static class PauseClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.PauseEquipmentBasePresentation", "pause-equipment-complete-base-image",
            ["CreateTilemap", "RebindBaseInto", "RebindBeforeInventoryRefreshInto"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs", "F5504CCB66E43D89032C40B4DD937D4B77C1F134CB6C90C52B8384CB2202C612"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "CCFF10C196B7896E09CDF913BB52CF420B698477740A7DEA7D30F67FDBA71797"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "B1F7A92EBD1EB0C4C3A97504F17D52156DA2B3970C2AB91A52BAA6DE61143496")],
            "The sole private-constructor loader compiles exactly 1024 cells into independent bytes. CreateTilemap returns a copy; rebind methods require a complete destination and select only loaded base cells. Live ownership/placement and inventory behavior are not certified."),
        new("SuperMetroid.Core.Assets.PauseBackdropPresentation", "pause-backdrop-complete-area-images", ["CreateButtonTilemap", "LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs", "E30BBFE6C6019E6453685FCB7301001895DFB529BE60F504D5C893B6A2172E7A"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseBackdropDefinitions.cs", "21BF029ADA264FAAE22EE06085DFBD80B4D00ED1AED0214A987E018E6EC27D91"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "B1F7A92EBD1EB0C4C3A97504F17D52156DA2B3970C2AB91A52BAA6DE61143496"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs", "6B88F8EE1B5ED42848833924AD9B5844B632D174C964D73E78D8E3774922E834")],
            "Private construction requires all seven exact 1024-cell area images and the 512-cell buttons image. Compiled arrays are independent; area selection is checked and buttons are copied. This covers resources, not map positioning or VRAM placement."),
        new("SuperMetroid.Core.Assets.PauseWireframePresentation", "pause-wireframe-complete-suit-images", ["ApplyTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs", "55F06F27CCC6FF9C314E42ADE63EE695AC7656EE6FCD29BF5BDF7D4AE7A593EF"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseWireframeDefinitions.cs", "B12FA0FD2AD83C5F455953C933E7C6EEE93F8C0CB6CBBEEE8801B4BDA58CC629"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "B1F7A92EBD1EB0C4C3A97504F17D52156DA2B3970C2AB91A52BAA6DE61143496")],
            "Private construction requires each of the four 136-cell wireframes and compiles independent bytes. ApplyTo guards kind and full-page destination length. Equipment selection and visible art are unchanged and not certified."),
        new("SuperMetroid.Core.Assets.PauseEquipmentLabelPresentation", "pause-label-complete-category-item-domain", ["ApplyInventory", "ApplyLabel", "OwnsLiveCell"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs", "B652CF77F4BFE115175ECBBE96155D1670139D26CB5A4812F16B15E4654689C4"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "69CCCDD37AE62BFB262EC3189A76928BAE61950D5A1E539C9B89A67FD1B3F22A"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "CCFF10C196B7896E09CDF913BB52CF420B698477740A7DEA7D30F67FDBA71797"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "B1F7A92EBD1EB0C4C3A97504F17D52156DA2B3970C2AB91A52BAA6DE61143496")],
            "Private construction requires all fourteen ordinary labels, Hyper and nine blank cells, compiled independently. ApplyLabel selects exact category/item tuples; only Plasma may extend a five-word beam patch to nine words using loaded Varia art. An external Core reference to the public mutable Keys array revokes this proof. Placement, inventory rules and the VAR glitch's visual outcome are not certified."),
        new("SuperMetroid.Core.Assets.PauseSelectorPresentation", "pause-selector-complete-anchors-and-bound-phases", ["Anchor", "NormalizePhase", "Duration", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorPresentation.cs", "23F4401B476541F0B7A50729ACD05C83C08AAF53C088378B638F00A51AF40CEE"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "C4BA6B9C643AB08E2E1AC9E38C07DF932AA484C9577554FE4EC6AC269F89E747"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "307F672D431D3F455957C36F30D5D1A6D0BCA5EA5429337EDCAFD96C318273DB")],
            "Private construction requires all sixteen category/item anchors and nonempty positive-timing phases. Every phase is bound to installed Reserve/Beam/Equipment compositions before publication. Nonnegative phases normalize modulo the installed count. Unknown category/item tuples and negative phases fail; controller/navigation behavior is not certified."),
        new("SuperMetroid.Core.Assets.PauseReserveTankPresentation", "pause-reserve-complete-anchors-and-sprites", ["Anchor", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankPresentation.cs", "278CF9945AC885F6E84AAF608BA6CB811C45FA9A90507F82B8104E11960EB578"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankDefinitions.cs", "CA93D5E2EA98F22A3E1D5AEA89F526FB22AC40FAC314397EF4EFB5808DFA3572"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseReserveTankRomData.cs", "6081A77E52F44FCF02F2A93F7BFAF662EA405F501BF4081A3B2CD37F542E8D06")],
            "Private construction requires six bounded immutable anchor records and every one of ten sparse named reserve compositions. Draw selects only installed identities with CLR-checked anchor indices. Supply/fill selection and tank animation are not certified."),
    ];
}
