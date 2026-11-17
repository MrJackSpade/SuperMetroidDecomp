namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The random fields a Beetom far from Samus reads to choose what it does next.</summary>
internal static class BeetomDecisions
{
    /// <summary>The random field that chooses a distant Beetom's next action among idling and the four hops.</summary>
    internal static readonly RandomSampleField NextAction = new(0, 3);

    /// <summary>
    /// What a distant Beetom does for each value of <see cref="NextAction"/> (#2369): idles for two, a short hop left
    /// for two, a long hop right for two, and a short hop right and a long hop left for one each.
    /// </summary>
    internal static readonly BeetomDistantChoice[] DistantChoices =
    [
        new(BeetomAction.StartIdling, null),
        new(BeetomAction.StartIdling, null),
        new(BeetomAction.StartShortHop, HorizontalDirection.Left),
        new(BeetomAction.StartLongHop, HorizontalDirection.Right),
        new(BeetomAction.StartShortHop, HorizontalDirection.Left),
        new(BeetomAction.StartShortHop, HorizontalDirection.Right),
        new(BeetomAction.StartLongHop, HorizontalDirection.Left),
        new(BeetomAction.StartLongHop, HorizontalDirection.Right),
    ];

    /// <summary>The random choice that turns a distant Beetom to face right: an odd sample, the parity of <see cref="NextAction"/>'s choice, so an odd action choice faces right.</summary>
    internal static readonly RandomSampleField FaceRight = new(0, 1);
}
