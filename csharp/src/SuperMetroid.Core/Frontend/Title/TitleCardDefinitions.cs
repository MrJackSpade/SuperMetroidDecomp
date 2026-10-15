namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// The four title cards' typed steps, in place of their bank-$8B lists ($8B:A03D-$A0C6): each card reveals its letters
/// one frame at a time for eight updates, holds its final frame, then triggers its Mode 7 scene and deletes itself.
/// </summary>
internal static class TitleCardDefinitions
{
    /// <summary><c>TitleSequenceSpritemaps_1</c> at $8C:8862; first year letter.</summary>
    private const ushort YearFirstLetter = 0x8862;
    /// <summary><c>TitleSequenceSpritemaps_N</c> at $8C:88CE; first Nintendo letter.</summary>
    private const ushort NintendoFirstLetter = 0x88ce;
    /// <summary><c>TitleSequenceSpritemaps_P</c> at $8C:8A46; first Presents letter.</summary>
    private const ushort PresentsFirstLetter = 0x8a46;
    /// <summary><c>TitleSequenceSpritemaps_M</c> at $8C:8BBE; first three Metroid letters.</summary>
    private const ushort MetroidFirstLetter = 0x8bbe;
    /// <summary><c>TitleSequenceSpritemaps_METR</c> at $8C:85C8; four/five-letter lists precede unused debug copyright artwork.</summary>
    private const ushort MetroidFourLetters = 0x85c8;
    /// <summary><c>TitleSequenceSpritemaps_METROI</c> at $8C:867D; final lists follow the debug copyright.</summary>
    private const ushort MetroidSixLetters = 0x867d;
    /// <summary>The blank spritemap the Year card holds for its 60-update opening.</summary>
    private const ushort OpeningBlank = 0;
    /// <summary>The opening's duration before the year's first letter, in title updates.</summary>
    private const ushort OpeningFrames = 60;
    /// <summary>Each reveal frame's duration before the final hold, in title updates.</summary>
    private const ushort RevealFrames = 8;

    /// <summary>$8B:A03D: the Year card's 60-update blank opening, its four letters and the scene-zero trigger.</summary>
    private static readonly TitleCardStep[] Year =
        [TitleCardStep.Show(OpeningFrames, OpeningBlank), .. Card(4, 45, YearFirstLetter, TitleSequenceInstruction.TriggerScene0)];
    /// <summary>$8B:A055: the Nintendo card's eight reveals and the scene-one trigger.</summary>
    private static readonly TitleCardStep[] Nintendo = Card(8, 45, NintendoFirstLetter, TitleSequenceInstruction.TriggerScene1);
    /// <summary>$8B:A079: the Presents card's eight reveals and the scene-two trigger.</summary>
    private static readonly TitleCardStep[] Presents = Card(8, 45, PresentsFirstLetter, TitleSequenceInstruction.TriggerScene2);
    /// <summary>$8B:A09D: the Metroid 3 card's nine reveals, its 120-update final hold and the scene-three trigger.</summary>
    private static readonly TitleCardStep[] MetroidThree = Card(9, 120, MetroidFirstLetter, TitleSequenceInstruction.TriggerScene3);

    /// <summary>The steps of the card <paramref name="phase"/> shows.</summary>
    internal static IReadOnlyList<TitleCardStep> Steps(TitleSequencePhase phase) => phase switch
    {
        TitleSequencePhase.YearText => Year,
        TitleSequencePhase.NintendoText => Nintendo,
        TitleSequencePhase.PresentsText => Presents,
        TitleSequencePhase.MetroidThreeText => MetroidThree,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Phase shows no title card."),
    };

    /// <summary>A card's reveal frames (the last held <paramref name="finalHold"/> updates), its scene trigger and deletion.</summary>
    private static TitleCardStep[] Card(int frames, ushort finalHold, ushort firstLetter, TitleSequenceInstruction sceneTrigger) =>
    [
        .. Enumerable.Range(0, frames).Select(frame =>
            TitleCardStep.Show(frame == frames - 1 ? finalHold : RevealFrames, Spritemap(firstLetter, frame))),
        TitleCardStep.Run(sceneTrigger),
        TitleCardStep.Run(TitleSequenceInstruction.Delete),
    ];

    /// <summary>The spritemap of reveal <paramref name="frame"/>, whose letter count grows by one each frame.</summary>
    private static ushort Spritemap(ushort firstLetter, int frame)
    {
        if (firstLetter != MetroidFirstLetter || frame < 3)
            return ProgressiveFrame(firstLetter, frame, firstLetterCount: 1);
        if (frame < 5) return ProgressiveFrame(MetroidFourLetters, frame - 3, firstLetterCount: 4);
        // The penultimate reveal adds a blank space, so its map has no extra letter objects.
        return (ushort)(ProgressiveFrame(MetroidSixLetters, frame - 5, firstLetterCount: 6) - (frame == 8 ? 10 : 0));
    }

    /// <summary>Advances over spritemaps whose visible letter count grows by one on each reveal.</summary>
    private static ushort ProgressiveFrame(ushort start, int frame, int firstLetterCount) =>
        // Each spritemap has a two-byte count and two five-byte OAM objects per visible letter.
        (ushort)(start + frame * 2 + 10 * (frame * firstLetterCount + frame * (frame - 1) / 2));
}
