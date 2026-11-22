namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed actor and explosion-layout definitions for the intro Mother Brain scene.</summary>
internal static partial class IntroMotherBrainDefinitions
{
    /// <summary>The intro Mother Brain's first animation.</summary>
    public static CinematicActorDefinition<IntroMotherBrainAnimation> MotherBrain =>
        new(IntroMotherBrainAnimation.Pulsing);

    /// <summary>A large explosion's first animation.</summary>
    public static CinematicActorDefinition<IntroMotherBrainExplosionAnimation> BigExplosionActor =>
        new(IntroMotherBrainExplosionAnimation.Big);

    /// <summary>A small explosion's first animation.</summary>
    public static CinematicActorDefinition<IntroMotherBrainExplosionAnimation> SmallExplosionActor =>
        new(IntroMotherBrainExplosionAnimation.Small);

    /// <summary>The Missile hits that make Mother Brain explode.</summary>
    public const int MissileHitsToExplode = 4;

    /// <summary>The room pixel a missile flying left strikes the brain on passing: left of it, it hits (#2280).</summary>
    public const int MissileImpactLine = 84;

    /// <summary>The updates each Missile hit flashes her.</summary>
    public const int HurtFlashUpdates = 8;

    /// <summary>The updates she explodes before the page turns to its second page.</summary>
    public const int ExplodingUpdates = 128;

    /// <summary>How far her room shakes up and down from its scroll while she explodes, in pixels.</summary>
    public const int RoomShakePixels = 4;

    /// <summary>Where Mother Brain starts in the cinematic.</summary>
    public static Offset16<ScreenPixels> MotherBrainOrigin => new(56, 111);

    /// <summary>The small explosion right of Mother Brain's origin, the first to burst.</summary>
    internal static IntroMotherBrainExplosionPlacement SmallRight { get; } = new(new(16, 0), 1);

    /// <summary>The small explosion below and left of Mother Brain's origin.</summary>
    internal static IntroMotherBrainExplosionPlacement SmallLowerLeft { get; } = new(new(-16, 4), 8);

    /// <summary>The small explosion above and left of Mother Brain's origin.</summary>
    internal static IntroMotherBrainExplosionPlacement SmallUpperLeft { get; } = new(new(-16, -8), 16);

    /// <summary>The large explosion at Mother Brain's origin, the first to burst.</summary>
    internal static IntroMotherBrainExplosionPlacement BigCentre { get; } = new(new(0, 0), 1);

    /// <summary>The large explosion above and right of Mother Brain's origin.</summary>
    internal static IntroMotherBrainExplosionPlacement BigUpperRight { get; } = new(new(16, -16), 16);

    /// <summary>The large explosion below and left of Mother Brain's origin.</summary>
    internal static IntroMotherBrainExplosionPlacement BigLowerLeft { get; } = new(new(-16, 8), 32);

    /// <summary>The large explosion above and a little left of Mother Brain's origin.</summary>
    internal static IntroMotherBrainExplosionPlacement BigUpperLeft { get; } = new(new(-8, -16), 48);

    /// <summary>The large explosion below and right of Mother Brain's origin, the last to burst.</summary>
    internal static IntroMotherBrainExplosionPlacement BigLowerRight { get; } = new(new(8, 8), 64);

    /// <summary>The small explosions in the order they spawn.</summary>
    internal static IReadOnlyList<IntroMotherBrainExplosionPlacement> SmallExplosions { get; } = [SmallRight, SmallLowerLeft, SmallUpperLeft];

    /// <summary>The large explosions in the order they spawn, after the small ones.</summary>
    internal static IReadOnlyList<IntroMotherBrainExplosionPlacement> BigExplosions { get; } =
        [BigCentre, BigUpperRight, BigLowerLeft, BigUpperLeft, BigLowerRight];
}
