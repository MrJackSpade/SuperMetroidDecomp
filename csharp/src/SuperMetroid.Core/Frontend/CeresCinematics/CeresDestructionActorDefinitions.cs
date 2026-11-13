using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical motion for Ceres destruction and Zebes reveal.</summary>
internal static class CeresDestructionActorDefinitions
{
    /// <summary>Number of persistent scene actors allocated when the destruction begins.</summary>
    public const int InitialActorCount = 3;

    /// <summary>Number of reveal actors allocated when Zebes is revealed.</summary>
    public const int ZebesActorCount = 6;

    /// <summary>
    /// The placements the Zebes reveal's layout holds: one for each of the <see cref="ZebesActorCount"/> actors the reveal
    /// allocates, in spawn order, then the Japanese subtitle's, which the PLANET ZEBES title spawns with Japanese text.
    /// </summary>
    public const int RevealPlacementCount = ZebesActorCount + 1;

    /// <summary>The place of the Japanese subtitle's placement in the reveal's layout: after the six actors'.</summary>
    public const int ZebesSubtitlePlacement = ZebesActorCount;

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

    /// <summary>The four star sheets' animations, indexed by row-major quadrant.</summary>
    internal static readonly CinematicSpriteSegment[] ZebesStarSheets =
    [
        CinematicSpriteSegment.ZebesStarSheet0, CinematicSpriteSegment.ZebesStarSheet1,
        CinematicSpriteSegment.ZebesStarSheet2, CinematicSpriteSegment.ZebesStarSheet3,
    ];
}
