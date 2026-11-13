namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed actor and explosion-layout definitions for the intro Mother Brain scene.</summary>
internal static class IntroMotherBrainDefinitions
{
    /// <summary>The intro Mother Brain's initialization, frame behaviour, and initial animation.</summary>
    public static IntroMotherBrainActorDefinition MotherBrain =>
        new(CinematicSpriteSegment.IntroMotherBrain);

    /// <summary>A large explosion's initialization, frame behaviour, and initial animation.</summary>
    public static IntroMotherBrainActorDefinition BigExplosionActor =>
        new(CinematicSpriteSegment.IntroMotherBrainExplosionBig);

    /// <summary>A small explosion's initialization, frame behaviour, and initial animation.</summary>
    public static IntroMotherBrainActorDefinition SmallExplosionActor =>
        new(CinematicSpriteSegment.IntroMotherBrainExplosionSmall);

    /// <summary>Mother Brain's initializer fixes her cinematic origin at (56,111).</summary>
    public static Position16<ScreenPixels> MotherBrainOrigin => new(56, 111);

    /// <summary>Number of records in the large-explosion tables.</summary>
    public const int BigExplosionCount = 5;

    /// <summary>Number of records in the small-explosion tables.</summary>
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
