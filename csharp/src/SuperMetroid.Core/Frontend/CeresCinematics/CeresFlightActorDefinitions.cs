using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>The rear view's actors in the order its setup spawns them, which is the order they update and draw in.</summary>
    internal static IReadOnlyList<CeresRearViewActorRole> RearViewSpawnOrder { get; } =
    [
        CeresRearViewActorRole.LargeAsteroids, CeresRearViewActorRole.StationUnderAttack, CeresRearViewActorRole.SmallAsteroids,
        CeresRearViewActorRole.Vortex, CeresRearViewActorRole.RearStars,
    ];

    /// <summary>The acceleration applied to the opening star field on each update.</summary>
    public static readonly SignedEightEight FrontStarAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>Where the opening star field starts; the rear view's actors start at their installed placements.</summary>
    public static readonly Position16<ScreenPixels> FrontStarsStart = new(112, 87);

    /// <summary>
    /// The front star definition: opening star field and the speed its rush starts at, four
    /// pixels an update toward the top-left.
    /// </summary>
    /// <remarks>
    /// The opening stars and the rear view's stars share this star animation: the star picture for ten updates, again and
    /// again while its scene keeps the actor. Which stars they are changes their motion, not their animation.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        ActiveFrameBehavior: CinematicSpriteFrameBehavior.CeresStars,
        Animation: CinematicSpriteSegment.CeresStars,
        Colouring: CeresSpriteColouring.SpaceObjects,
        InitialVelocity: SignedEightEight.FromPixels(-4),
        HorizontalDelta: SignedSixteenSixteen.Zero,
        WrapX: false);

    /// <summary>The large asteroids of the Ceres explosion.
    /// Its initializer colours it in the space objects' colours; its motion adds 1/4 pixel, wrapping X past 511;
    /// its animation is the large-asteroid loop. The destruction scene substitutes its own definition and animation.</summary>
    internal static CeresFlightActorDefinition LargeAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresLargeAsteroids, CinematicSpriteSegment.CeresLargeAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromSixteenthsOfPixel(4), true);

    /// <summary>The space station under attack.
    /// Its initializer colours it in the station's colours; its motion adds 1/16 pixel, wrapping X past 511;
    /// its animation is the station-under-attack loop.</summary>
    internal static CeresFlightActorDefinition StationUnderAttack =>
        new(CinematicSpriteFrameBehavior.CeresUnderAttack, CinematicSpriteSegment.CeresUnderAttack,
            CeresSpriteColouring.Station, SignedEightEight.Zero, SignedSixteenSixteen.FromSixteenthsOfPixel(1), true);

    /// <summary>The small asteroids around Ceres.
    /// Its initializer colours it in the space objects' colours; its motion adds 1/32 pixel, wrapping X past 511;
    /// its animation is the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    internal static CeresFlightActorDefinition SmallAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresSmallAsteroid, CinematicSpriteSegment.CeresSmallAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(8), true);

    /// <summary>The moving purple space vortex.
    /// It is coloured in the space objects' colours and keeps moving;
    /// its animation alternates two frames. The stationary destruction-scene vortex is its other variant.</summary>
    internal static CeresFlightActorDefinition MovingVortex =>
        new(CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex, CinematicSpriteSegment.CeresVortex,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, VortexHorizontalDelta, false);

    /// <summary>The stars around Ceres, the rear view's variant.
    /// It is coloured in the space objects' colours and drifts with the vortex instead of rushing like the opening stars. The definition and animation alias FrontStars.</summary>
    internal static CeresFlightActorDefinition RearStars => FrontStars with
    {
        ActiveFrameBehavior = CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex,
        InitialVelocity = SignedEightEight.Zero,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>The drifting motion: an eighth of a pixel leftward, taking from the whole X as the fraction passes zero, with no
    /// wrap past 511.</summary>
    private static SignedSixteenSixteen VortexHorizontalDelta { get; } = -SignedSixteenSixteen.FromSixteenthsOfPixel(2);
}
