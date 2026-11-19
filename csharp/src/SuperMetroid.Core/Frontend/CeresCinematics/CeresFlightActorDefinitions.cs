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
    public static readonly SignedEightEight FrontStarAcceleration = SignedEightEight.FromPixelFraction(1, 2);

    /// <summary>Where the opening star field starts; the rear view's actors start at their installed placements.</summary>
    public static readonly Offset16<ScreenPixels> FrontStarsStart = new(112, 87);

    /// <summary>
    /// The front star definition: opening star field and the speed its rush starts at, four
    /// pixels an update toward the top-left.
    /// </summary>
    /// <remarks>
    /// The opening stars and the rear view's stars share this star animation: the star picture for ten updates, again and
    /// again while its scene keeps the actor. Which stars they are changes their motion, not their animation.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        Animation: CeresAnimation.Stars,
        Colouring: CeresSpriteColouring.SpaceObjects,
        InitialVelocity: SignedEightEight.FromPixels(-4),
        HorizontalDelta: SignedSixteenSixteen.Zero,
        DriftsRoundLoop: false);

    /// <summary>The large asteroids of the Ceres explosion.
    /// It is coloured in the space objects' colours; it drifts right a quarter pixel each update round the scenes' drift loop;
    /// its animation is the large-asteroid loop. The destruction scene substitutes its own definition and animation.</summary>
    internal static CeresFlightActorDefinition LargeAsteroids =>
        new(CeresAnimation.LargeAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromPixelFraction(1, 4), true);

    /// <summary>The space station under attack.
    /// It is coloured in the station's colours; it drifts right a sixteenth of a pixel each update round the scenes' drift loop;
    /// its animation is the station-under-attack loop.</summary>
    internal static CeresFlightActorDefinition StationUnderAttack =>
        new(CeresAnimation.StationUnderAttack,
            CeresSpriteColouring.Station, SignedEightEight.Zero, SignedSixteenSixteen.FromPixelFraction(1, 16), true);

    /// <summary>The small asteroids around Ceres.
    /// It is coloured in the space objects' colours; it drifts right a thirty-second of a pixel each update round the scenes' drift loop;
    /// its animation is the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    internal static CeresFlightActorDefinition SmallAsteroids =>
        new(CeresAnimation.SmallAsteroid,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, SignedSixteenSixteen.FromPixelFraction(1, 32), true);

    /// <summary>The moving purple space vortex.
    /// It is coloured in the space objects' colours and keeps moving;
    /// its animation alternates two frames. The stationary destruction-scene vortex is its other variant.</summary>
    internal static CeresFlightActorDefinition MovingVortex =>
        new(CeresAnimation.Vortex,
            CeresSpriteColouring.SpaceObjects, SignedEightEight.Zero, VortexHorizontalDelta, false);

    /// <summary>The stars around Ceres, the rear view's variant.
    /// It is coloured in the space objects' colours and drifts with the vortex instead of rushing like the opening stars. It shares FrontStars' definition and animation.</summary>
    internal static CeresFlightActorDefinition RearStars => FrontStars with
    {
        InitialVelocity = SignedEightEight.Zero,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>The vortex's drift: an eighth of a pixel leftward each update, never coming round the drift loop.</summary>
    private static SignedSixteenSixteen VortexHorizontalDelta { get; } = -SignedSixteenSixteen.FromPixelFraction(1, 8);
}
