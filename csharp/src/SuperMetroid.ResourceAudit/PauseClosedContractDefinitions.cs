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
            [new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs", "E8A9B128F75BCF48880806780D84B370D2B80E4C90C17304F60FBAAB97400406"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelDefinitions.cs", "FB0A9FBCF625EE898442D894E0AB9C7E2E3AEB08DAEF8F5EEFEB8E0EEA894C95"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "B349A95CBC583DE582CE2848298C484045CB10AE88939C4ADD651B3BCC9F80EF"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "965C7A5234E0AD1256900579421573E9DEE3AB921C1EF90D561AD984786E2188"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs", "CCFF10C196B7896E09CDF913BB52CF420B698477740A7DEA7D30F67FDBA71797"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseTileGrid.cs", "B1F7A92EBD1EB0C4C3A97504F17D52156DA2B3970C2AB91A52BAA6DE61143496")],
            "Private construction requires all fourteen ordinary labels, Hyper and nine blank cells, compiled independently. ApplyLabel selects exact category/item tuples; only Plasma may extend a five-word beam patch to nine words using loaded Varia art. Label identities reuse the reviewed selector cases within the three equipment categories; no public mutable key table remains. All alias and category source dependencies are pinned. Placement, inventory rules and the VAR glitch's visual outcome are not certified."),
        new("SuperMetroid.Core.Assets.PauseSelectorPresentation", "pause-selector-complete-anchors-and-bound-phases", ["Anchor", "NormalizePhase", "Duration", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorPresentation.cs", "C23DB77B861BD32B8B477BAE85498450C80617AC2A7CA2205A81364012F69177"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorVisual.cs", "D4D66D24ABF8CD200D46B1999799A6D13124BB952622AF8EF556D2466029E422"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs", "C4BA6B9C643AB08E2E1AC9E38C07DF932AA484C9577554FE4EC6AC269F89E747"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseSelectorDefinitions.cs", "B349A95CBC583DE582CE2848298C484045CB10AE88939C4ADD651B3BCC9F80EF"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseMenuDefinitions.cs", "965C7A5234E0AD1256900579421573E9DEE3AB921C1EF90D561AD984786E2188")],
            "Private construction requires all sixteen named category/item anchors and nonempty positive-timing phases. Anchor coordinates use bounded category geometry with independently captured author edits. Every phase is validated against installed Reserve/Beam/Equipment compositions before publication; each group stores its first visual and only authored phase differences. Stock visuals calculate cursor/grid parts in the native paint order without retained sprite records; edited visuals use the checked compiler. Nonnegative phases normalize modulo the installed count. Unknown category/item tuples and negative phases fail; controller/navigation behavior is not certified."),
        new("SuperMetroid.Core.Assets.PauseReserveTankPresentation", "pause-reserve-complete-anchors-and-sprites", ["Anchor", "Draw"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankPresentation.cs", "BE414A5B857D739C065F4E6D66D830EDCCF3C9FC0DB260F2DDE0EFAD96DE5A36"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveTankDefinitions.cs", "6D2F0A0909C53A11C3125A98E8381E1C947AD18DB9830D7AF6D74895307FF5E1"),
             new("csharp/src/SuperMetroid.Core/Frontend/PauseReserveTankRomData.cs", "6081A77E52F44FCF02F2A93F7BFAF662EA405F501BF4081A3B2CD37F542E8D06")],
            "Private construction validates six bounded anchor inputs, calculates stock strip geometry and captures independent coordinate edits. It requires every one of ten named reserve compositions, draws stock single-sprite geometry directly and compiles only authored differences into named fields. Draw selects explicit visual-role cases with explicitly checked anchor indices. Supply/fill selection and tank animation are not certified."),
    ];
}
