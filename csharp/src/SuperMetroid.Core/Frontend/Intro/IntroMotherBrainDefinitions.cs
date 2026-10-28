namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed actor and explosion-layout definitions for the intro Mother Brain scene.</summary>
internal static class IntroMotherBrainDefinitions
{
    /// <summary>The sprite row Mother Brain's hurt flash turns white and back.</summary>
    public const int HurtFlashSpriteRow = 7;

    /// <summary><c>$8B:CE55</c>, intro Mother Brain initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition MotherBrain =>
        new(CinematicSpriteProgram.IntroMotherBrain);

    /// <summary><c>$8B:CF15</c>, large explosion initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition BigExplosionActor =>
        new(CinematicSpriteProgram.IntroMotherBrainExplosionBig);

    /// <summary><c>$8B:CF1B</c>, small explosion initialization, pre-instruction, and initial list.</summary>
    public static IntroMotherBrainActorDefinition SmallExplosionActor =>
        new(CinematicSpriteProgram.IntroMotherBrainExplosionSmall);

    /// <summary><c>$8B:B773</c> fixes Mother Brain's cinematic origin at (56,111).</summary>
    public static Position16<ScreenPixels> MotherBrainOrigin => new(0x0038, 0x006f);

    /// <summary>Number of records in the <c>$8B:B9B6-$B9D3</c> large-explosion tables.</summary>
    public const int BigExplosionCount = 5;

    /// <summary>Number of records in the <c>$8B:B9FD-$BA0E</c> small-explosion tables.</summary>
    public const int SmallExplosionCount = 3;

    /// <summary>Returns one complete <c>$8B:B98D</c> large-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement BigExplosion(int index) => Ensure.Element(BigExplosions, index);

    /// <summary>The values <see cref="BigExplosion"/> selects, in index order.</summary>
    private static readonly IntroMotherBrainExplosionPlacement[] BigExplosions =
    [
        new(new(0, 0), 1),
        new(new(16, -16), 16),
        new(new(-16, 8), 32),
        new(new(-8, -16), 48),
        new(new(8, 8), 64),
    ];

    /// <summary>Returns one complete <c>$8B:B9D4</c> small-explosion placement record.</summary>
    public static IntroMotherBrainExplosionPlacement SmallExplosion(int index) => Ensure.Element(SmallExplosions, index);

    /// <summary>The values <see cref="SmallExplosion"/> selects, in index order.</summary>
    private static readonly IntroMotherBrainExplosionPlacement[] SmallExplosions =
    [
        new(new(16, 0), 1),
        new(new(-16, 4), 8),
        new(new(-16, -8), 16),
    ];
}
