using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>
    /// The persistent actors behind the explosion in the order the destruction's setup spawns them, which is the order
    /// they update in.
    /// </summary>
    internal static IReadOnlyList<CeresDestructionActorRole> DestructionSpawnOrder { get; } =
        [CeresDestructionActorRole.LargeAsteroids, CeresDestructionActorRole.SmallAsteroids, CeresDestructionActorRole.Vortex];

    /// <summary>
    /// The Zebes reveal's actors in the order its setup spawns them. The Japanese subtitle is not among them: the title
    /// spawns it when Japanese text is selected.
    /// </summary>
    internal static IReadOnlyList<ZebesRevealActorRole> RevealSpawnOrder { get; } =
    [
        ZebesRevealActorRole.Planet, ZebesRevealActorRole.StarsUpperLeft, ZebesRevealActorRole.StarsUpperRight,
        ZebesRevealActorRole.StarsLowerLeft, ZebesRevealActorRole.StarsLowerRight, ZebesRevealActorRole.Title,
    ];

    /// <summary>The Zebes planet actor: its initializer,
    /// waiting behaviour and animation; its initializer's values set its placement/palette.
    /// Its slide behaviour accelerates by 64/256 pixels.</summary>
    internal static CeresDestructionActorDefinition ZebesPlanet =>
        new(CinematicSpriteFrameBehavior.Zebes, CinematicSpriteSegment.ZebesPlanet,
            CeresSpriteColouring.Zebes, SignedSixteenSixteen.Zero, false, SignedEightEight.FromParts(0, 64),
            CinematicSpriteFrameBehavior.ZebesSlideSceneAway, false);

    /// <summary>The PLANET ZEBES text actor: its initializer,
    /// an empty behaviour and its animation, in the cinematic's first sprite colours.
    /// Its animation deletes before scene sliding, so it has no slide behaviour or acceleration.</summary>
    internal static CeresDestructionActorDefinition ZebesTitle =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.ZebesTitle,
            CeresSpriteColouring.ZebesTitle, SignedSixteenSixteen.Zero, false, SignedEightEight.Zero, null, false);

    /// <summary>
    /// The Japanese PLANET ZEBES subtitle, which the title spawns below itself as its first hold ends when Japanese text is
    /// selected: an empty behaviour and its animation, which shows the subtitle for 192 updates and deletes it, in the
    /// title's sprite colours, so the caption's fades show on it too. It is deleted before the scene slides away.
    /// </summary>
    internal static CeresDestructionActorDefinition ZebesSubtitle =>
        new(CinematicSpriteFrameBehavior.NoOp, CinematicSpriteSegment.ZebesJapaneseTitle,
            CeresSpriteColouring.ZebesTitle, SignedSixteenSixteen.Zero, false, SignedEightEight.Zero, null, false);

    /// <summary>The animation each quarter of the reveal's star field shows.</summary>
    internal static IReadOnlyDictionary<ZebesRevealActorRole, CinematicSpriteSegment> StarSheets { get; } =
        new Dictionary<ZebesRevealActorRole, CinematicSpriteSegment>
        {
            [ZebesRevealActorRole.StarsUpperLeft] = CinematicSpriteSegment.ZebesStarsUpperLeft,
            [ZebesRevealActorRole.StarsUpperRight] = CinematicSpriteSegment.ZebesStarsUpperRight,
            [ZebesRevealActorRole.StarsLowerLeft] = CinematicSpriteSegment.ZebesStarsLowerLeft,
            [ZebesRevealActorRole.StarsLowerRight] = CinematicSpriteSegment.ZebesStarsLowerRight,
        };

    /// <summary>The quarter of the star field whose leaving the screen ends the reveal and starts the game.</summary>
    internal const ZebesRevealActorRole SceneCompletingStars = ZebesRevealActorRole.StarsLowerRight;
}
