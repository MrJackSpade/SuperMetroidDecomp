using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's scene actors: where each starts, which of the screen's colours it draws in and which cinematic
/// animation it runs.
/// </summary>
internal static class EndingActorDefinitions
{
    /// <summary>The side-cloud escape's leading right cloud: origin (320,192) pixels.</summary>
    internal static readonly EndingSpriteDefinition SideCloudRightLeading =
        new(new(320, 192), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingRightSideCloud);
    /// <summary>The side-cloud escape's leading left cloud: origin (-64,64) pixels.</summary>
    internal static readonly EndingSpriteDefinition SideCloudLeftLeading =
        new(new(-64, 64), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingLeftSideCloud);
    /// <summary>The side-cloud escape's right cloud trailing below the leading one: origin (320,448) pixels.</summary>
    internal static readonly EndingSpriteDefinition SideCloudRightTrailing =
        new(new(320, 448), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingRightSideCloud);
    /// <summary>The side-cloud escape's left cloud trailing above the leading one: origin (-64,-192) pixels.</summary>
    internal static readonly EndingSpriteDefinition SideCloudLeftTrailing =
        new(new(-64, -192), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingLeftSideCloud);
    /// <summary>The top-and-bottom-cloud escape's upper cloud pattern, origin (128,-96).</summary>
    internal static readonly EndingSpriteDefinition TopCloudPattern =
        new(new(128, -96), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingTopCloudPattern);
    /// <summary>The edge of the top-and-bottom-cloud escape's upper clouds, origin (128,-32).</summary>
    internal static readonly EndingSpriteDefinition TopCloudEdge =
        new(new(128, -32), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingTopCloudEdge);
    /// <summary>The edge of the top-and-bottom-cloud escape's lower clouds, origin (128,288).</summary>
    internal static readonly EndingSpriteDefinition BottomCloudEdge =
        new(new(128, 288), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingBottomCloudEdge);
    /// <summary>The top-and-bottom-cloud escape's lower cloud pattern, origin (128,352).</summary>
    internal static readonly EndingSpriteDefinition BottomCloudPattern =
        new(new(128, 352), EndingSpriteColouring.Clouds, CinematicSpriteSegment.EndingBottomCloudPattern);
    /// <summary>Planet actor at (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplodingZebes =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, CinematicSpriteSegment.ZebesExplosionPlanet);
    /// <summary>Explosion lava overlay at (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionLava =
        new(new(128, 128), EndingSpriteColouring.ExplosionLava, CinematicSpriteSegment.ZebesExplosionLava);
    /// <summary>Purple-glow actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionGlow =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, CinematicSpriteSegment.ZebesExplosionGlow);
    /// <summary>Explosion-stars actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStars =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, CinematicSpriteSegment.ZebesExplosionStars);
    /// <summary>Explosion silhouette: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionSilhouette =
        new(new(128, 128), EndingSpriteColouring.ExplosionLava, CinematicSpriteSegment.ZebesExplosionSilhouette);
    /// <summary>Right-hand explosion-stars actor: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsRight =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, CinematicSpriteSegment.ZebesExplosionRightStars);
    /// <summary>Left-hand explosion-stars actor: origin (-128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsLeft =
        new(new(-128, 128), EndingSpriteColouring.ExplodingPlanet, CinematicSpriteSegment.ZebesExplosionLeftStars);
    /// <summary>Zebes afterglow: origin (128,128) pixels.</summary>
    internal static readonly EndingSpriteDefinition ExplosionAfterglow =
        new(new(128, 128), EndingSpriteColouring.ExplosionAfterglow, CinematicSpriteSegment.ZebesExplosionAfterglow);
    /// <summary>THE OPERATION WAS actor: origin (128,96) pixels.</summary>
    internal static readonly EndingSpriteDefinition OperationWasText =
        new(new(128, 96), EndingSpriteColouring.CompletionText, CinematicSpriteSegment.OperationWasText);
    /// <summary>COMPLETED SUCCESSFULLY actor: origin (128,96) pixels.</summary>
    internal static readonly EndingSpriteDefinition CompletedSuccessfullyText =
        new(new(128, 96), EndingSpriteColouring.CompletionText, CinematicSpriteSegment.CompletedSuccessfullyText);
    /// <summary>The clear-time label actor at (128,160) pixels.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeText =
        new(new(128, 160), EndingSpriteColouring.ClearTimeLabel, CinematicSpriteSegment.ClearTimeText);
    /// <summary>Clear-time colon actor at (172,160) pixels, in the clear-time digits' colours.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeColon =
        new(new(172, 160), EndingSpriteColouring.ClearTimeDigits, CinematicSpriteSegment.ClearTimeColon);
    /// <summary>Shared clear-time digit origin Y=160 screen pixels, aligned with the label and colon.</summary>
    public const int ClearTimeDigitY = 160;
    /// <summary>Suitless idle-body actor, at the suitless Samus's start.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardBody =
        new(EndingRewardStartDefinitions.Suitless.Position, EndingRewardStartDefinitions.Suitless.Colouring, CinematicSpriteSegment.SuitlessIdle);
    /// <summary>Suitless idle-legs actor, at the suitless Samus's start, which the body shares.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardLegs =
        new(EndingRewardStartDefinitions.Suitless.Position, EndingRewardStartDefinitions.Suitless.Colouring, CinematicSpriteSegment.SuitlessLegsIdle);
    /// <summary>Suited idle-body actor, at the suited body's start.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardBody =
        new(EndingRewardStartDefinitions.SuitedBody.Position, EndingRewardStartDefinitions.SuitedBody.Colouring, CinematicSpriteSegment.SuitedIdle);
    /// <summary>Helmetless suited-head actor, at the helmetless head's start.</summary>
    internal static readonly EndingSpriteDefinition HelmetlessRewardHead =
        new(EndingRewardStartDefinitions.HelmetlessHead.Position, EndingRewardStartDefinitions.HelmetlessHead.Colouring, CinematicSpriteSegment.HeadIdle);
    /// <summary>Helmeted suited-head actor, at the helmeted head's start.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardHead =
        new(EndingRewardStartDefinitions.HelmetedHead.Position, EndingRewardStartDefinitions.HelmetedHead.Colouring, CinematicSpriteSegment.HelmetIdle);
    /// <summary>The colouring the clear time's digits draw in, as the colon does.</summary>
    internal const EndingSpriteColouring ClearTimeDigitColouring = EndingSpriteColouring.ClearTimeDigits;

    /// <summary>The suitless reward's idle actors, its body then its legs.</summary>
    internal static readonly EndingSpriteDefinition[] SuitlessRewardIdle = [SuitlessRewardBody, SuitlessRewardLegs];

    /// <summary>The helmetless reward's idle actors, its suited body then its bare head.</summary>
    internal static readonly EndingSpriteDefinition[] HelmetlessRewardIdle = [ArmoredRewardBody, HelmetlessRewardHead];

    /// <summary>The armored reward's idle actors, its suited body then its helmeted head.</summary>
    internal static readonly EndingSpriteDefinition[] ArmoredRewardIdle = [ArmoredRewardBody, ArmoredRewardHead];
}
