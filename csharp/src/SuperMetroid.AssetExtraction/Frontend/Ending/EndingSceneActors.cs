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

    /// <summary>The result text's typed texts: the completion message's two lines and the clear-time label (#1513, #2195).</summary>
    private static readonly EndingTextDefinition[] ResultTexts =
    [
        EndingActorDefinitions.OperationWasText, EndingActorDefinitions.CompletedSuccessfullyText, EndingActorDefinitions.ClearTimeText,
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
            draws.AddProgram(CinematicSpriteFamily.EndingCloud, EndingSceneAnimations.Pictures, cloud.Animation, cloud.Colouring);
    }

    /// <summary>The planet explosion's actors, the result text, the clear time's digits and the animals' escape pod.</summary>
    private static void Explosion(EndingSheetDraws draws)
    {
        foreach (EndingSpriteDefinition actor in ExplosionActors)
            draws.AddProgram(CinematicSpriteFamily.EndingExplosion, EndingSceneAnimations.Pictures, actor.Animation, actor.Colouring);
        foreach (EndingTextDefinition text in ResultTexts)
            draws.AddText(text.Text, text.Colouring);
        draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, EndingSceneAnimations.Pictures, EndingActorDefinitions.ClearTimeColon.Animation,
            EndingActorDefinitions.ClearTimeColon.Colouring);
        foreach (EndingSceneAnimation digit in EndingCreditsState.ClearTimeDigitAnimations)
            draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, EndingSceneAnimations.Pictures, digit, EndingActorDefinitions.ClearTimeDigitColouring);
        draws.AddProgram(CinematicSpriteFamily.EndingAnimalEscape, EndingSceneAnimations.Pictures, EndingAnimalEscapeDefinitions.Animation,
            EndingAnimalEscapeDefinitions.Colouring);
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
                draws.AddProgram(CinematicSpriteFamily.EndingReward, EndingSceneAnimations.Pictures, idle.Animation, idle.Colouring);
            foreach (EndingRewardActor actor in EndingRewardCast.Gesture(reward))
                AddRewardActor(draws, EndingRewardAnimations.Gesture, actor);
            if (EndingRewardCast.JumpHead(reward) is { } head)
                AddRewardActor(draws, EndingRewardAnimations.Jump, head);
            AddRewardActor(draws, EndingRewardAnimations.Jump, EndingRewardCast.JumpBody(reward));
        }
        draws.AddStars();
    }

    /// <summary>
    /// The falling Samus, who falls and lands in her falling colouring until the landing's shoot cue turns her to her
    /// shooting colouring, and the logo the scene ends on. The stars draw from the reward sheets (#1626).
    /// </summary>
    private static void Shooting(EndingSheetDraws draws)
    {
        draws.AddProgram(CinematicSpriteFamily.EndingReward, EndingRewardAnimations.Jump, EndingRewardJump.FallingAnimation, EndingRewardJump.FallingColouring);
        EndingSpriteColouring landing = EndingRewardJump.FallingColouring;
        if (EndingRewardAnimations.Jump.StartingWith(EndingRewardJump.LandingAnimation).Phases.Any(phase => phase.Segment != EndingRewardJump.LandingAnimation))
            throw new InvalidDataException("The landing animation runs on into another segment, whose colourings the shooting sheet does not follow.");
        foreach (AnimationBeat<CinematicSpritePose, EndingRewardJumpCue> beat in EndingRewardAnimations.Jump.Segments.Beats(EndingRewardJump.LandingAnimation))
        {
            if (beat is AnimationBeat<CinematicSpritePose, EndingRewardJumpCue>.Event { Raised: EndingRewardJump.ShootCue })
                landing = EndingRewardJump.ShootingColouring;
            else if (beat.ShownFrame is { } shown)
                draws.AddPose(CinematicSpriteFamily.EndingReward, shown, landing);
        }
        foreach (EndingLogoActorDefinition actor in EndingLogoDefinitions.Actors)
            draws.AddProgram(CinematicSpriteFamily.EndingLogo, EndingLogoAnimations.Logo, actor.Animation, EndingLogoDefinitions.Colouring);
    }

    /// <summary>Records <paramref name="actor"/>'s poses in the colouring its initialization gives it.</summary>
    private static void AddRewardActor<TCue>(EndingSheetDraws draws, CinematicAnimations<EndingRewardAnimation, CinematicSpritePose, TCue> animations,
        EndingRewardActor actor)
        where TCue : notnull
    {
        EndingRewardActorDefinition record = EndingRewardActors.Get(actor);
        draws.AddProgram(CinematicSpriteFamily.EndingReward, animations, record.Animation, record.Start.Colouring);
    }
}
