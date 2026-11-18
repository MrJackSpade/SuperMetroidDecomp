using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Runs the reward gesture actors until their jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    /// <summary>Reward actor set in spawn order, advanced before inactive actors are removed.</summary>
    private readonly List<CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue>> actors = [];
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
        foreach (CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> actor in actors)
            actor.Step(HandleCue);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    /// <summary>Places current actors as the supplied sprite entries.</summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation installedArt)
    {
        Ensure.NotNull(destination);
        foreach (CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> actor in actors)
            actor.Draw(destination, installedArt: installedArt);
    }

    /// <summary>Converts the jump cue into a suited or suitless handoff.</summary>
    private void HandleCue(EndingRewardGestureCue cue)
    {
        switch (cue)
        {
            case EndingRewardGestureCue.StartJump:
                JumpRequested = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingRewardGestureCue)} {cue}.");
        }
    }

    /// <summary>Creates an actor from its defined start and animation.</summary>
    private void Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition authored = EndingRewardActors.Get(definition);
        var actor = new CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue>(
            authored.Start.Position,
            EndingShades.Drawn(authored.Start.Colouring),
            EndingRewardAnimations.Gesture,
            authored.Animation);
        actors.Add(actor);
    }
}
