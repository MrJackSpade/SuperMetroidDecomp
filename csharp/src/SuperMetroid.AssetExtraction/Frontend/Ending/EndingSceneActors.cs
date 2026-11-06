using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Programs;

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
        foreach (EndingSpriteDefinition cloud in EndingActorDefinitions.EscapeClouds)
            draws.AddProgram(CinematicSpriteFamily.EndingCloud, cloud.Program, cloud.Colouring);
    }

    /// <summary>The planet explosion's actors, the result text, the clear time's digits and the animals' escape pod.</summary>
    private static void Explosion(EndingSheetDraws draws)
    {
        foreach (EndingSpriteDefinition actor in EndingActorDefinitions.ExplosionActors)
            draws.AddProgram(CinematicSpriteFamily.EndingExplosion, actor.Program, actor.Colouring);
        foreach (EndingSpriteDefinition text in EndingActorDefinitions.ResultTextActors)
            draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, text.Program, text.Colouring);
        foreach (CinematicSpriteProgram digit in EndingCreditsState.ClearTimeDigitPrograms)
            draws.AddProgram(CinematicSpriteFamily.EndingCompletionText, digit, EndingActorDefinitions.ClearTimeDigitColouring);
        draws.AddProgram(CinematicSpriteFamily.EndingAnimalEscape, EndingAnimalEscapeDefinitions.Program, EndingAnimalEscapeDefinitions.Colouring);
    }

    /// <summary>
    /// The post-credits actors of every reward whose scenes draw from reward sheet <paramref name="sheet"/>: the idle
    /// Samus, the gesture and the jump until it falls from her falling sheet, each in its colouring, and the stars.
    /// </summary>
    private static void Rewards(EndingSheetDraws draws, SpriteSheetId sheet)
    {
        foreach (EndingReward reward in Enum.GetValues<EndingReward>())
        {
            if (EndingSpriteSheets.For(EndingObjectSheet.Reward, reward) != sheet)
                continue;
            foreach (EndingSpriteDefinition idle in EndingRewardCast.Idle(reward))
                draws.AddProgram(CinematicSpriteFamily.EndingReward, idle.Program, idle.Colouring);
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
    /// shooting colouring; the logo the scene ends on; and the stars.
    /// </summary>
    private static void Shooting(EndingSheetDraws draws)
    {
        draws.AddProgram(CinematicSpriteFamily.EndingReward, EndingRewardJump.FallingProgram, EndingRewardJump.FallingColouring);
        EndingSpriteColouring landing = EndingRewardJump.FallingColouring;
        foreach (AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> step in
            CinematicSpriteAnimations.Programs.Steps(EndingRewardJump.LandingProgram))
        {
            if (step is AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.CueStep cue
                && cue.Cue.Kind == EndingRewardJump.ShootCue)
                landing = EndingRewardJump.ShootingColouring;
            else if (step is AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.ShowStep shown)
                draws.AddPose(CinematicSpriteFamily.EndingReward, shown.Frame, landing);
            else if (step is AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.GoToStep
                or AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.FallIntoStep)
                throw new InvalidDataException("The landing program runs on into another program, whose colourings the shooting sheet does not follow.");
        }
        for (int actor = 0; actor < EndingLogoDefinitions.ActorKinds.Length; actor++)
            draws.AddProgram(CinematicSpriteFamily.EndingLogo, EndingLogoActors.Actor(actor).Program, EndingLogoDefinitions.Colouring);
        draws.AddStars();
    }

    /// <summary>Records <paramref name="actor"/>'s poses in the colouring its initialization gives it.</summary>
    private static void AddRewardActor(EndingSheetDraws draws, EndingRewardActor actor)
    {
        EndingRewardActorDefinition record = EndingRewardActors.Get(actor);
        draws.AddProgram(CinematicSpriteFamily.EndingReward, record.Program, EndingRewardActors.GetInitialization(record.Initialization).Colouring);
    }
}
