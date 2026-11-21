namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The random choice and chance a Beetom far from Samus reads to choose what it does next.</summary>
internal static class BeetomDecisions
{
    /// <summary>
    /// What a distant Beetom does next, chosen by eight rolls of its sample's lowest field (#2369, #2510): idles for two,
    /// then a short hop left, a long hop right, a short hop left, a short hop right, a long hop left and a long hop right
    /// for one each.
    /// </summary>
    internal static readonly WeightedRandomChoice<BeetomDistantChoice> DistantChoice = new(RandomSampleField.OneOf(8),
        new(2, new BeetomDistantChoice(BeetomAction.StartIdling, null)),
        new(1, new BeetomDistantChoice(BeetomAction.StartShortHop, HorizontalDirection.Left)),
        new(1, new BeetomDistantChoice(BeetomAction.StartLongHop, HorizontalDirection.Right)),
        new(1, new BeetomDistantChoice(BeetomAction.StartShortHop, HorizontalDirection.Left)),
        new(1, new BeetomDistantChoice(BeetomAction.StartShortHop, HorizontalDirection.Right)),
        new(1, new BeetomDistantChoice(BeetomAction.StartLongHop, HorizontalDirection.Left)),
        new(1, new BeetomDistantChoice(BeetomAction.StartLongHop, HorizontalDirection.Right)));

    /// <summary>
    /// Whether a distant Beetom turns to face right (#2479), read from the same random number as
    /// <see cref="DistantChoice"/>: it faces the way a chosen hop goes, and after one idling choice in two it faces right.
    /// </summary>
    internal static readonly RandomChance FacesRight = RandomChance.Highest(1, RandomSampleField.OneOf(2));
}
