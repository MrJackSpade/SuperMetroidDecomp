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
    /// <summary>Whether the gesture has handed off to the jump, after which it takes no further step.</summary>
    private bool handedOff;

    /// <summary>Spawns the suitless pair or the suited head, body, and arm, in the order they step and draw.</summary>
    public EndingRewardGesture(EndingReward reward)
    {
        // The cast lists the actors in the order they step and draw: a suited reward's head first, then body, then arm.
        foreach (EndingRewardActor actor in EndingRewardCast.Gesture(reward))
            Spawn(actor);
    }

    /// <summary>Advances active gesture actors once.</summary>
    /// <returns>Whether the step's cue hands off to the jumping actors (#2562), which the scene's owner shows at once.</returns>
    public bool Step()
    {
        if (handedOff) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        bool jumps = false;
        foreach (CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> actor in actors)
            actor.Step(cue => jumps |= HandleCue(cue));
        actors.RemoveAll(actor => !actor.IsActive);
        handedOff = jumps;
        return jumps;
    }

    /// <summary>Places current actors as the supplied sprite entries.</summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation installedArt)
    {
        Ensure.NotNull(destination);
        foreach (CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardGestureCue> actor in actors)
            actor.Draw(destination, installedArt: installedArt);
    }

    /// <summary>Converts the jump cue into a suited or suitless handoff: whether <paramref name="cue"/> starts the jump.</summary>
    private static bool HandleCue(EndingRewardGestureCue cue)
    {
        switch (cue)
        {
            case EndingRewardGestureCue.StartJump:
                return true;
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
