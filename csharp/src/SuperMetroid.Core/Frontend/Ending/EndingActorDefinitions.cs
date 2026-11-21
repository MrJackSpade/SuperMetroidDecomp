using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's scene actors: where each starts, which of the screen's colours it draws in and which cinematic
/// animation it runs.
/// </summary>
internal static class EndingActorDefinitions
{
    /// <summary>The side-cloud escape's leading right cloud, starting past the screen's right.</summary>
    internal static readonly EndingSpriteDefinition SideCloudRightLeading =
        new(new(320, 192), EndingSpriteColouring.Clouds, EndingSceneAnimation.RightSideCloud);
    /// <summary>The side-cloud escape's leading left cloud, starting past the screen's left.</summary>
    internal static readonly EndingSpriteDefinition SideCloudLeftLeading =
        new(new(-64, 64), EndingSpriteColouring.Clouds, EndingSceneAnimation.LeftSideCloud);
    /// <summary>The side-cloud escape's right cloud trailing below the leading one.</summary>
    internal static readonly EndingSpriteDefinition SideCloudRightTrailing =
        new(new(320, 448), EndingSpriteColouring.Clouds, EndingSceneAnimation.RightSideCloud);
    /// <summary>The side-cloud escape's left cloud trailing above the leading one.</summary>
    internal static readonly EndingSpriteDefinition SideCloudLeftTrailing =
        new(new(-64, -192), EndingSpriteColouring.Clouds, EndingSceneAnimation.LeftSideCloud);
    /// <summary>The top-and-bottom-cloud escape's upper cloud pattern, above the screen.</summary>
    internal static readonly EndingSpriteDefinition TopCloudPattern =
        new(new(128, -96), EndingSpriteColouring.Clouds, EndingSceneAnimation.TopCloudPattern);
    /// <summary>The edge of the top-and-bottom-cloud escape's upper clouds, just above the screen.</summary>
    internal static readonly EndingSpriteDefinition TopCloudEdge =
        new(new(128, -32), EndingSpriteColouring.Clouds, EndingSceneAnimation.TopCloudEdge);
    /// <summary>The edge of the top-and-bottom-cloud escape's lower clouds, just below the screen.</summary>
    internal static readonly EndingSpriteDefinition BottomCloudEdge =
        new(new(128, 288), EndingSpriteColouring.Clouds, EndingSceneAnimation.BottomCloudEdge);
    /// <summary>The top-and-bottom-cloud escape's lower cloud pattern, below the screen.</summary>
    internal static readonly EndingSpriteDefinition BottomCloudPattern =
        new(new(128, 352), EndingSpriteColouring.Clouds, EndingSceneAnimation.BottomCloudPattern);
    /// <summary>Planet actor at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplodingZebes =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, EndingSceneAnimation.ExplodingPlanet);
    /// <summary>Explosion lava overlay at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionLava =
        new(new(128, 128), EndingSpriteColouring.ExplosionLava, EndingSceneAnimation.ExplosionLava);
    /// <summary>Purple-glow actor at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionGlow =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, EndingSceneAnimation.ExplosionGlow);
    /// <summary>Explosion-stars actor at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStars =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, EndingSceneAnimation.ExplosionStars);
    /// <summary>Explosion silhouette at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionSilhouette =
        new(new(128, 128), EndingSpriteColouring.ExplosionLava, EndingSceneAnimation.ExplosionSilhouette);
    /// <summary>Right-hand explosion-stars actor at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsRight =
        new(new(128, 128), EndingSpriteColouring.ExplodingPlanet, EndingSceneAnimation.ExplosionRightStars);
    /// <summary>Left-hand explosion-stars actor, a screen's width left of the right-hand one.</summary>
    internal static readonly EndingSpriteDefinition ExplosionStarsLeft =
        new(new(-128, 128), EndingSpriteColouring.ExplodingPlanet, EndingSceneAnimation.ExplosionLeftStars);
    /// <summary>Zebes afterglow at the screen's centre.</summary>
    internal static readonly EndingSpriteDefinition ExplosionAfterglow =
        new(new(128, 128), EndingSpriteColouring.ExplosionAfterglow, EndingSceneAnimation.ExplosionAfterglow);
    /// <summary>THE OPERATION WAS actor, centred across the screen.</summary>
    internal static readonly EndingTextDefinition OperationWasText =
        new(new(128, 96), EndingSpriteColouring.CompletionText, EndingTypedText.OperationWas);
    /// <summary>COMPLETED SUCCESSFULLY actor, centred across the screen.</summary>
    internal static readonly EndingTextDefinition CompletedSuccessfullyText =
        new(new(128, 96), EndingSpriteColouring.CompletionText, EndingTypedText.CompletedSuccessfully);
    /// <summary>The clear-time label actor, on the clear-time row (<see cref="ClearTimeDigitY"/>).</summary>
    internal static readonly EndingTextDefinition ClearTimeText =
        new(new(128, 160), EndingSpriteColouring.ClearTimeLabel, EndingTypedText.ClearTime);
    /// <summary>Clear-time colon actor on the clear-time row, in the clear-time digits' colours.</summary>
    internal static readonly EndingSpriteDefinition ClearTimeColon =
        new(new(172, 160), EndingSpriteColouring.ClearTimeDigits, EndingSceneAnimation.ClearTimeColon);
    /// <summary>Shared clear-time digit origin Y in screen pixels, aligned with the label and colon.</summary>
    public const int ClearTimeDigitY = 160;
    /// <summary>Suitless idle-body actor, at the suitless Samus's start.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardBody =
        new(EndingRewardStartDefinitions.Suitless.Position, EndingRewardStartDefinitions.Suitless.Colouring, EndingSceneAnimation.SuitlessIdle);
    /// <summary>Suitless idle-legs actor, at the suitless Samus's start, which the body shares.</summary>
    internal static readonly EndingSpriteDefinition SuitlessRewardLegs =
        new(EndingRewardStartDefinitions.Suitless.Position, EndingRewardStartDefinitions.Suitless.Colouring, EndingSceneAnimation.SuitlessLegsIdle);
    /// <summary>Suited idle-body actor, at the suited body's start.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardBody =
        new(EndingRewardStartDefinitions.SuitedBody.Position, EndingRewardStartDefinitions.SuitedBody.Colouring, EndingSceneAnimation.SuitedIdle);
    /// <summary>Helmetless suited-head actor, at the helmetless head's start.</summary>
    internal static readonly EndingSpriteDefinition HelmetlessRewardHead =
        new(EndingRewardStartDefinitions.HelmetlessHead.Position, EndingRewardStartDefinitions.HelmetlessHead.Colouring, EndingSceneAnimation.HeadIdle);
    /// <summary>Helmeted suited-head actor, at the helmeted head's start.</summary>
    internal static readonly EndingSpriteDefinition ArmoredRewardHead =
        new(EndingRewardStartDefinitions.HelmetedHead.Position, EndingRewardStartDefinitions.HelmetedHead.Colouring, EndingSceneAnimation.HelmetIdle);
    /// <summary>The colouring the clear time's digits draw in, as the colon does.</summary>
    internal const EndingSpriteColouring ClearTimeDigitColouring = EndingSpriteColouring.ClearTimeDigits;

    /// <summary>The suitless reward's idle actors, its body then its legs.</summary>
    internal static readonly EndingSpriteDefinition[] SuitlessRewardIdle = [SuitlessRewardBody, SuitlessRewardLegs];

    /// <summary>The helmetless reward's idle actors, its suited body then its bare head.</summary>
    internal static readonly EndingSpriteDefinition[] HelmetlessRewardIdle = [ArmoredRewardBody, HelmetlessRewardHead];

    /// <summary>The armored reward's idle actors, its suited body then its helmeted head.</summary>
    internal static readonly EndingSpriteDefinition[] ArmoredRewardIdle = [ArmoredRewardBody, ArmoredRewardHead];
}
