using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>Number of actors allocated for the flight.</summary>
    public const int RearViewActorCount = 5;

    /// <summary>The acceleration applied to the opening star field on each update.</summary>
    public static readonly SignedEightEight FrontStarAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>Where the opening star field starts; the rear view's actors start at their installed placements.</summary>
    public static readonly Position16<ScreenPixels> FrontStarsStart = new(112, 87);

    /// <summary>
    /// The star definition with initializer parameter zero: opening star field and the speed its rush starts at, four
    /// pixels an update toward the top-left.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #1014: front stars and RearViewActor(4) alias the
    /// same star definition and animation. Its four
    /// steps match the authored data: duration
    /// 10, the star frame, then a jump back to the start.
    /// The initializer parameter changes actor motion, not this list.
    /// The exact bounded rule displays the same authored star sheet for
    /// ten handler calls and repeats while its scene owns the actor.
    /// IntroDiscoverySprite.Step never falls into the following animation.
    /// Both aliases select the same installed visual frame.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        ActiveFrameBehavior: CinematicSpriteFrameBehavior.CeresStars,
        Animation: CinematicSpriteSegment.CeresStars,
        Colouring: CeresSpriteColouring.SpaceObjects,
        InitialVelocity: SignedEightEight.FromPixels(-4),
        HorizontalDelta: SignedSixteenSixteen.Zero,
        WrapX: false);

    /// <summary>The large asteroids of the Ceres explosion.
    /// Its initializer colours it with palette0800; its callback adds 1/4 pixel and masks X to nine bits;
    /// its animation is the large-asteroid loop. The destruction scene substitutes its own definition and animation.</summary>
    internal static CeresFlightActorDefinition LargeAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresLargeAsteroids, CinematicSpriteSegment.CeresLargeAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromSixteenthsOfPixel(4), true);

    /// <summary>The space station under attack.
    /// Its initializer colours it in the station's colours; its callback adds 1/16 pixel, wrapping X past 511;
    /// its animation is the station-under-attack loop.</summary>
    internal static CeresFlightActorDefinition StationUnderAttack =>
        new(CinematicSpriteFrameBehavior.CeresUnderAttack, CinematicSpriteSegment.CeresUnderAttack,
            CeresSpriteColouring.Station, SignedEightEight.Zero, SignedSixteenSixteen.FromSixteenthsOfPixel(1), true);

    /// <summary>The small asteroids around Ceres.
    /// Its initializer colours it with palette0800; its callback adds 1/32 pixel with nine-bit X wrap;
    /// its animation is the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    internal static CeresFlightActorDefinition SmallAsteroids =>
        new(CinematicSpriteFrameBehavior.CeresSmallAsteroid, CinematicSpriteSegment.CeresSmallAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(8), true);

    /// <summary>The purple space vortex, parameter1.
    /// Its initializer takes the nonzero branch, palette0800, preserving the moving callback;
    /// its animation alternates two frames. Parameter0 is the stationary destruction-scene variant.</summary>
    internal static CeresFlightActorDefinition MovingVortex =>
        new(CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex, CinematicSpriteSegment.CeresVortex,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, VortexHorizontalDelta, false);

    /// <summary>The stars around Ceres, parameter1.
    /// Its initializer takes the nonzero branch: palette0800, timer remains cleared;
    /// it replaces the star-field callback with the drifting one. The definition and animation alias FrontStars.</summary>
    internal static CeresFlightActorDefinition RearStars => FrontStars with
    {
        ActiveFrameBehavior = CinematicSpriteFrameBehavior.CeresPurpleSpaceVortex,
        InitialVelocity = SignedEightEight.Zero,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>The drifting callback's movement: an eighth of a pixel leftward, taking from the whole X as the fraction passes zero, with no
    /// nine-bit wrap.</summary>
    private static SignedSixteenSixteen VortexHorizontalDelta { get; } = -SignedSixteenSixteen.FromSixteenthsOfPixel(2);
}
