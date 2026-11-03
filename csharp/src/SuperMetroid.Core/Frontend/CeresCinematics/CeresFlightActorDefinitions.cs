using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>Number of actors allocated for the flight.</summary>
    public const int RearViewActorCount = 5;

    /// <summary>Signed-8.8 acceleration applied to the opening star field.</summary>
    public const int FrontStarAcceleration = 128;

    /// <summary>
    /// The star definition with initializer parameter zero: opening star field and its
    /// signed-8.8 motion accumulator.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #1014: front stars and RearViewActor(4) alias the
    /// same star definition and animation. Its four
    /// steps match the original data: duration
    /// 10, the star spritemap, then a goto back to the start.
    /// The initializer parameter changes actor motion, not this list.
    /// The exact bounded rule displays the same authored star sheet for
    /// ten handler calls and repeats while its scene owns the actor.
    /// IntroDiscoverySprite.Step never falls into the following animation.
    /// Both aliases select the same installed visual frame.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        ActiveFrameBehavior: CinematicSpriteFrameBehavior.CeresStars,
        Program: CinematicSpriteProgram.CeresStars,
        Position: new(112, 87),
        Colouring: CeresSpriteColouring.SpaceObjects,
        InitialTimer: 64512,
        HorizontalDelta: 0,
        WrapX: false);

    /// <summary>CinematicSpriteObjectDefs_CeresExplosionLargeAsteroids.
    /// Its initializer places it at (80,159), palette0800; its callback adds 1/4 pixel and masks X to nine bits;
    /// its animation is the large-asteroid loop. The destruction scene substitutes its own definition and animation.</summary>
    internal static CeresFlightActorDefinition LargeAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresLargeAsteroids, CinematicSpriteProgram.CeresLargeAsteroid,
            new(80, 159), CeresSpriteColouring.SpaceObjects, 0, 16384, true);

    /// <summary>CinematicSpriteObjectDefinitions_CeresUnderAttack.
    /// Its initializer places it at (116,160), palette0C00; its callback adds 1/16 pixel with nine-bit X wrap;
    /// its animation is the station-under-attack loop.</summary>
    internal static CeresFlightActorDefinition StationUnderAttack =>
        new(CinematicSpriteFrameBehavior.CeresUnderAttack, CinematicSpriteProgram.CeresUnderAttack,
            new(116, 160), CeresSpriteColouring.Station, 0, 4096, true);

    /// <summary>CinematicSpriteObjectDefinitions_CeresSmallAsteroids.
    /// Its initializer places it at (128,96), palette0800; its callback adds 1/32 pixel with nine-bit X wrap;
    /// its animation is the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    internal static CeresFlightActorDefinition SmallAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresSmallAsteroid, CinematicSpriteProgram.CeresSmallAsteroid,
            new(128, 96), CeresSpriteColouring.SpaceObjects, 0, 2048, true);

    /// <summary>CinematicSpriteObjectDefinitions_CeresPurpleSpaceVortex, parameter1.
    /// Its initializer takes the nonzero branch to (224,87), palette0800, preserving the moving callback;
    /// its animation alternates two frames. Parameter0 is the stationary destruction-scene variant.</summary>
    internal static CeresFlightActorDefinition MovingVortex =>
        new(CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex, CinematicSpriteProgram.CeresVortex,
            new(224, 87), CeresSpriteColouring.SpaceObjects, 0, VortexHorizontalDelta, false);

    /// <summary>CinematicSpriteObjectDefinitions_CeresStars, parameter1.
    /// Its initializer takes the nonzero branch: X=-32, Y87, palette0800, timer remains cleared;
    /// it replaces the star-field callback with the drifting one. The definition and animation alias FrontStars.</summary>
    internal static CeresFlightActorDefinition RearStars => FrontStars with
    {
        ActiveFrameBehavior = CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex,
        Position = FrontStars.Position.WithX(WordWrap.Unsigned(-32)),
        InitialTimer = 0,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>The drifting callback's subtraction: signed16.16 movement of -1/8 pixel,
    /// with borrow into the whole X word and no nine-bit mask.</summary>
    private const int VortexHorizontalDelta = -8192;
}
