namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops: six small, then six large.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>Six small explosion frames (two one-part, then four-part), then six large (one one-part, then four-part).</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("small-explosion-0", new(41), 1),
        new("small-explosion-1", new(42), 1),
        new("small-explosion-2", new(43), 4),
        new("small-explosion-3", new(44), 4),
        new("small-explosion-4", new(45), 4),
        new("small-explosion-5", new(46), 4),
        new("big-explosion-0", new(67), 1),
        new("big-explosion-1", new(68), 4),
        new("big-explosion-2", new(69), 4),
        new("big-explosion-3", new(70), 4),
        new("big-explosion-4", new(71), 4),
        new("big-explosion-5", new(72), 4),
    ];
}
