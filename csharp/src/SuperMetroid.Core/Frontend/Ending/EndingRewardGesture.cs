using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Runs the reward gesture actors until their jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    /// <summary>Reward actor set in spawn order, advanced before inactive actors are removed.</summary>
    private readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Signals that the jumping-actor handoff must be consumed before another gesture tick.</summary>
    public bool JumpRequested { get; private set; }

    /// <summary>Spawns the suitless pair or the suited head, body, and arm in the reward's allocation order.</summary>
    public EndingRewardGesture(EndingReward reward)
    {
        // The cast keeps the allocation order: a suited reward's head first, then body, then arm.
        foreach (EndingRewardActor actor in EndingRewardCast.Gesture(reward))
            Spawn(actor);
    }

    /// <summary>Advances active gesture actors once and records any cue-triggered jump handoff.</summary>
    public void Step()
    {
        if (JumpRequested) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        foreach (IntroDiscoverySprite actor in actors)
            actor.Step(HandleCue);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    /// <summary>Places current actors as the supplied sprite entries.</summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation installedArt)
    {
        Ensure.NotNull(destination);
        foreach (IntroDiscoverySprite actor in actors)
            actor.Draw(destination, installedArt: installedArt);
    }

    /// <summary>Converts the jump cue into a suited or suitless handoff.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnSuitlessJump:
                JumpRequested = true;
                return;
            case CinematicSpriteCueKind.SpawnSuitedJump:
                JumpRequested = true;
                return;
            case CinematicSpriteCueKind.SetFrameBehavior:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionColours:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnStrikingRinkaPair:
            case CinematicSpriteCueKind.SpawnMissingRinkaPair:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The reward gesture's animations publish no {cue} cue.");
        }
    }

    /// <summary>Creates an actor from compiled initialization and binds its discovery frame behaviour.</summary>
    private void Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition authored = EndingRewardActors.Get(definition);
        EndingRewardActorInitializationDefinition initialization =
            EndingRewardActors.GetInitialization(authored.Initialization);
        var actor = new IntroDiscoverySprite(
            initialization.Position,
            EndingShades.Drawn(initialization.Colouring),
            authored.Animation);
        actor.InstallFrameBehavior(authored.FrameBehavior);
        actors.Add(actor);
    }
}
