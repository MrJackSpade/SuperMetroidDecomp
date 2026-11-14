using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// What each ending scene's actors draw from its sheet (#1513), from Core's actor definitions: every pose each actor's
/// program shows while its scene draws from that sheet, in the colourings the actor draws it in. A sheet pictures exactly
/// these poses, so no frame is composed over characters its scene never loads for it.
/// </summary>
internal static class EndingSceneActors
{
    /// <summary>Each ending sheet's recording of what its scene's actors draw from it.</summary>
    private static readonly Dictionary<SpriteSheetId, Action<EndingSheetDraws>> Scenes = new()
    {
        [SpriteSheetId.EndingCloudSprites] = Clouds,
        [SpriteSheetId.EndingExplosionSprites] = Explosion,
        [SpriteSheetId.EndingSuitedRewardSprites] = draws => Rewards(draws, SpriteSheetId.EndingSuitedRewardSprites),
        [SpriteSheetId.EndingSuitlessRewardSprites] = draws => Rewards(draws, SpriteSheetId.EndingSuitlessRewardSprites),
        [SpriteSheetId.EndingShootingSprites] = Shooting,
    };

    /// <summary>The escapes' clouds: the side-cloud escape's four and the top-and-bottom-cloud escape's four (#1513).</summary>
    private static readonly EndingSpriteDefinition[] EscapeClouds =
    [
        EndingActorDefinitions.SideCloudRightLeading, EndingActorDefinitions.SideCloudLeftLeading,
        EndingActorDefinitions.SideCloudRightTrailing, EndingActorDefinitions.SideCloudLeftTrailing,
        EndingActorDefinitions.TopCloudPattern, EndingActorDefinitions.TopCloudEdge,
        EndingActorDefinitions.BottomCloudEdge, EndingActorDefinitions.BottomCloudPattern,
    ];

    /// <summary>The planet explosion's actors: the planet, its lava, glow, stars and silhouette, the side stars and the afterglow (#1513).</summary>
    private static readonly EndingSpriteDefinition[] ExplosionActors =
    [
        EndingActorDefinitions.ExplodingZebes, EndingActorDefinitions.ExplosionLava, EndingActorDefinitions.ExplosionGlow,
        EndingActorDefinitions.ExplosionStars, EndingActorDefinitions.ExplosionSilhouette, EndingActorDefinitions.ExplosionStarsRight,
        EndingActorDefinitions.ExplosionStarsLeft, EndingActorDefinitions.ExplosionAfterglow,
    ];

    /// <summary>The result text's actors: the completion message's two lines, the clear-time label and its colon (#1513).</summary>
    private static readonly EndingSpriteDefinition[] ResultTextActors =
    [
        EndingActorDefinitions.OperationWasText, EndingActorDefinitions.CompletedSuccessfullyText,
        EndingActorDefinitions.ClearTimeText, EndingActorDefinitions.ClearTimeColon,
    ];

    /// <summary>What the scene of ending sheet <paramref name="sheet"/> draws from it.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet is not an ending sprite sheet.</exception>
    internal static EndingSheetDraws Of(SpriteSheetId sheet)
    {
        if (!Scenes.TryGetValue(sheet, out Action<EndingSheetDraws>? scene))
            throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending sprite sheet.");
        var draws = new EndingSheetDraws();
        scene(draws);
        return draws;
    }

    /// <summary>The escapes' clouds, each in its definition's colouring.</summary>
    private static void Clouds(EndingSheetDraws draws)
    {
        foreach (EndingSpriteDefinition cloud in EscapeClouds)
            draws.AddProgram(CinematicSpriteFamily.EndingCloud, cloud.Animation, cloud.Colouring);
    }

    /// <summary>The planet explosion's actors, the result text, the clear time's digits and the animals' escape pod.</summary>
    private static void Explosion(EndingSheetDraws draws)
    {
        foreach (EndingSpriteDefinition actor in ExplosionActors)
            draws.AddProgram(CinematicSpriteFamily.EndingExplosion, actor.Animation, actor.Colouring);
        foreach (EndingSpriteDefinition text in ResultTextActors)
            draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, text.Animation, text.Colouring);
        foreach (CinematicSpriteSegment digit in EndingCreditsState.ClearTimeDigitAnimations)
            draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, digit, EndingActorDefinitions.ClearTimeDigitColouring);
        draws.AddProgram(CinematicSpriteFamily.EndingAnimalEscape, EndingAnimalEscapeDefinitions.Animation, EndingAnimalEscapeDefinitions.Colouring);
    }

    /// <summary>
    /// The post-credits actors of every reward whose scenes draw from reward sheet <paramref name="sheet"/>: the idle
    /// Samus, the gesture and the jump until she falls back as the falling Samus, each in its colouring, and the stars.
    /// </summary>
    private static void Rewards(EndingSheetDraws draws, SpriteSheetId sheet)
    {
        foreach (EndingReward reward in Enum.GetValues<EndingReward>())
        {
            if (EndingSpriteSheets.Reward(reward) != sheet)
                continue;
            foreach (EndingSpriteDefinition idle in EndingRewardCast.Idle(reward))
                draws.AddProgram(CinematicSpriteFamily.EndingReward, idle.Animation, idle.Colouring);
            foreach (EndingRewardActor actor in EndingRewardCast.Gesture(reward))
                AddRewardActor(draws, actor);
            if (EndingRewardCast.JumpHead(reward) is { } head)
                AddRewardActor(draws, head);
            AddRewardActor(draws, EndingRewardCast.JumpBody(reward));
        }
        draws.AddStars();
    }

    /// <summary>
    /// The falling Samus, who falls and lands in her falling colouring until the landing's shoot cue turns her to her
    /// shooting colouring, and the logo the scene ends on. The stars draw from the reward sheets (#1626).
    /// </summary>
    private static void Shooting(EndingSheetDraws draws)
    {
        draws.AddProgram(CinematicSpriteFamily.EndingReward, EndingRewardJump.FallingAnimation, EndingRewardJump.FallingColouring);
        EndingSpriteColouring landing = EndingRewardJump.FallingColouring;
        if (CinematicSpriteAnimations.StartingWith(EndingRewardJump.LandingAnimation).Phases.Any(phase => phase.Segment != EndingRewardJump.LandingAnimation))
            throw new InvalidDataException("The landing animation runs on into another segment, whose colourings the shooting sheet does not follow.");
        foreach (AnimationBeat<CinematicSpritePose, CinematicSpriteCue> beat in CinematicSpriteAnimations.Segments.Beats(EndingRewardJump.LandingAnimation))
        {
            if (beat is AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Event raised
                && raised.Raised.Kind == EndingRewardJump.ShootCue)
                landing = EndingRewardJump.ShootingColouring;
            else if (beat is AnimationBeat<CinematicSpritePose, CinematicSpriteCue>.Frame shown)
                draws.AddPose(CinematicSpriteFamily.EndingReward, shown.Shown, landing);
        }
        for (int actor = 0; actor < EndingLogoDefinitions.ActorKinds.Length; actor++)
            draws.AddProgram(CinematicSpriteFamily.EndingLogo, EndingLogoActors.Actor(actor).Animation, EndingLogoDefinitions.Colouring);
    }

    /// <summary>Records <paramref name="actor"/>'s poses in the colouring its initialization gives it.</summary>
    private static void AddRewardActor(EndingSheetDraws draws, EndingRewardActor actor)
    {
        EndingRewardActorDefinition record = EndingRewardActors.Get(actor);
        draws.AddProgram(CinematicSpriteFamily.EndingReward, record.Animation, EndingRewardActors.GetInitialization(record.Initialization).Colouring);
    }
}
