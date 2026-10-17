namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The four title cards' typed steps, in place of their bank-$8B lists ($8B:A03D-$A0C6): each card reveals its letters
/// one frame at a time for eight updates, holds its final frame, then triggers its Mode 7 scene and deletes itself.
/// The extraction pipeline walks the cartridge lists beside these steps; <see cref="TitleCards"/> selects a card.
/// </summary>
internal static class TitleCardDefinitions
{
    /// <summary>The opening's duration before the year's first letter, in title updates.</summary>
    private const int OpeningFrames = 60;
    /// <summary>Each reveal frame's duration before the final hold, in title updates.</summary>
    private const int RevealFrames = 8;
    /// <summary>The final hold of the year, Nintendo and Presents cards, in title updates.</summary>
    private const int CardHoldFrames = 45;
    /// <summary>The final hold of the Metroid 3 card, in title updates.</summary>
    private const int MetroidThreeHoldFrames = 120;

    /// <summary>$8B:A03D: the Year card's 60-update blank opening, its four letters and the scene-zero trigger.</summary>
    internal static readonly TitleCardStep[] Year =
    [
        TitleCardStep.Show(OpeningFrames, null),
        TitleCardStep.Show(RevealFrames, TitleSprite.YearReveal1),
        TitleCardStep.Show(RevealFrames, TitleSprite.YearReveal2),
        TitleCardStep.Show(RevealFrames, TitleSprite.YearReveal3),
        TitleCardStep.Show(CardHoldFrames, TitleSprite.YearReveal4),
        TitleCardStep.Run(TitleSequenceInstruction.TriggerScene0),
        TitleCardStep.Run(TitleSequenceInstruction.Delete),
    ];

    /// <summary>$8B:A055: the Nintendo card's eight reveals and the scene-one trigger.</summary>
    internal static readonly TitleCardStep[] Nintendo =
    [
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal1),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal2),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal3),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal4),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal5),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal6),
        TitleCardStep.Show(RevealFrames, TitleSprite.NintendoReveal7),
        TitleCardStep.Show(CardHoldFrames, TitleSprite.NintendoReveal8),
        TitleCardStep.Run(TitleSequenceInstruction.TriggerScene1),
        TitleCardStep.Run(TitleSequenceInstruction.Delete),
    ];

    /// <summary>$8B:A079: the Presents card's eight reveals and the scene-two trigger.</summary>
    internal static readonly TitleCardStep[] Presents =
    [
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal1),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal2),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal3),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal4),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal5),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal6),
        TitleCardStep.Show(RevealFrames, TitleSprite.PresentsReveal7),
        TitleCardStep.Show(CardHoldFrames, TitleSprite.PresentsReveal8),
        TitleCardStep.Run(TitleSequenceInstruction.TriggerScene2),
        TitleCardStep.Run(TitleSequenceInstruction.Delete),
    ];

    /// <summary>$8B:A09D: the Metroid 3 card's nine reveals, its 120-update final hold and the scene-three trigger.</summary>
    internal static readonly TitleCardStep[] MetroidThree =
    [
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal1),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal2),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal3),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal4),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal5),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal6),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal7),
        TitleCardStep.Show(RevealFrames, TitleSprite.MetroidThreeReveal8),
        TitleCardStep.Show(MetroidThreeHoldFrames, TitleSprite.MetroidThreeReveal9),
        TitleCardStep.Run(TitleSequenceInstruction.TriggerScene3),
        TitleCardStep.Run(TitleSequenceInstruction.Delete),
    ];
}
