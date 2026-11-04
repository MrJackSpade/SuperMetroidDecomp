namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The order Mother Brain's encounter shows its sprite pages in (#1406, #1513): the steps of its artwork sequence, whose
/// pre-assembled pictures the installed sheets hold. Each sheet shows page by page: her legs and the attack pages they
/// covered as she rises, the Baby, her attack pages again as the Baby's cutscene ends, her corpse, the escape timer and
/// message, and the exploded escape door.
/// </summary>
internal static class MotherBrainSpritePageDefinitions
{
    /// <summary>Every page in the order the encounter shows them; a page shown again takes its next step.</summary>
    internal static IReadOnlyList<MotherBrainSpritePage> Sequence { get; } =
    [
        .. Pages(MotherBrainSpritePageSheet.Legs),
        .. Pages(MotherBrainSpritePageSheet.Attack),
        .. Pages(MotherBrainSpritePageSheet.BabyMetroid),
        .. Pages(MotherBrainSpritePageSheet.Attack),
        .. Pages(MotherBrainSpritePageSheet.Corpse),
        .. Pages(MotherBrainSpritePageSheet.EscapeTimer),
        .. Pages(MotherBrainSpritePageSheet.EscapeText),
        .. Pages(MotherBrainSpritePageSheet.ExplodedDoor),
    ];

    /// <summary>Every page of <paramref name="sheet"/>, from its first.</summary>
    private static IEnumerable<MotherBrainSpritePage> Pages(MotherBrainSpritePageSheet sheet) =>
        Enumerable.Range(0, MotherBrainSpritePage.PageCount(sheet)).Select(page => MotherBrainSpritePage.Of(sheet, page));
}
