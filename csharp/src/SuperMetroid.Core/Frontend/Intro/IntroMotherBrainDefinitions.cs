namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed actor and explosion-layout definitions for the intro Mother Brain scene.</summary>
internal static class IntroMotherBrainDefinitions
{
    /// <summary>The intro Mother Brain's first animation.</summary>
    public static IntroMotherBrainActorDefinition MotherBrain =>
        new(CinematicSpriteSegment.IntroMotherBrain);

    /// <summary>A large explosion's first animation.</summary>
    public static IntroMotherBrainActorDefinition BigExplosionActor =>
        new(CinematicSpriteSegment.IntroMotherBrainExplosionBig);

    /// <summary>A small explosion's first animation.</summary>
    public static IntroMotherBrainActorDefinition SmallExplosionActor =>
        new(CinematicSpriteSegment.IntroMotherBrainExplosionSmall);

    /// <summary>Mother Brain starts at the cinematic origin (56,111).</summary>
    public static Position16<ScreenPixels> MotherBrainOrigin => new(56, 111);

    /// <summary>The number of large explosions.</summary>
    public const int BigExplosionCount = 5;

    /// <summary>The number of small explosions.</summary>
    public const int SmallExplosionCount = 3;

    /// <summary>The values <see cref="IntroMotherBrainExplosionPlacements.BigExplosion"/> selects, in index order.</summary>
    internal static readonly IntroMotherBrainExplosionPlacement[] BigExplosions =
    [
        new(new(0, 0), 1),
        new(new(16, -16), 16),
        new(new(-16, 8), 32),
        new(new(-8, -16), 48),
        new(new(8, 8), 64),
    ];

    /// <summary>The values <see cref="IntroMotherBrainExplosionPlacements.SmallExplosion"/> selects, in index order.</summary>
    internal static readonly IntroMotherBrainExplosionPlacement[] SmallExplosions =
    [
        new(new(16, 0), 1),
        new(new(-16, 4), 8),
        new(new(-16, -8), 16),
    ];
}
