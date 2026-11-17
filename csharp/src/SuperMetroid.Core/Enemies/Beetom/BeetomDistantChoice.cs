namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>What a Beetom far from Samus does next (#2369): the action it begins, and the way it hops, if it hops.</summary>
/// <param name="Action">The action it begins.</param>
/// <param name="Heading">The way it hops, or none when it idles.</param>
internal readonly record struct BeetomDistantChoice(BeetomAction Action, HorizontalDirection? Heading);
