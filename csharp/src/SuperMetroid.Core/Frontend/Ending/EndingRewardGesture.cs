using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Runs E342's reward gesture actors until their native jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    /// <summary>Construction address space retained by the gesture API; actor setup uses compiled definitions.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Reward actor set in native spawn order, advanced before inactive actors are removed.</summary>
    private readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Signals that the jumping-actor handoff must be consumed before another gesture tick.</summary>
    public bool JumpRequested { get; private set; }
    /// <summary>Distinguishes the suitless jump request from the suited handoff.</summary>
    public bool SuitlessJumpRequested { get; private set; }

    /// <summary>Spawns the suitless pair or the suited head, body, and arm in E342 allocation order.</summary>
    public EndingRewardGesture(ISnesAddressSpace bus, EndingReward reward)
    {
        this.bus = bus;
        if (reward == EndingReward.Suitless)
        {
            Spawn(EndingRewardActor.SuitlessUpper);
            Spawn(EndingRewardActor.SuitlessLower);
        }
        else
        {
            // Keep E342's allocation order: head first, then body, then arm.
            Spawn(reward == EndingReward.Helmetless ? EndingRewardActor.HelmetlessHead
                : EndingRewardActor.HelmetedHead);
            Spawn(EndingRewardActor.SuitedBody);
            Spawn(EndingRewardActor.SuitedArm);
        }
    }

    /// <summary>Advances active gesture actors once and records any instruction-triggered jump handoff.</summary>
    public void Step()
    {
        if (JumpRequested) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        foreach (IntroDiscoverySprite actor in actors)
            actor.Step(HandleCue);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    /// <summary>Draws current actors into a supplied OAM frame or creates and finalizes a standalone frame.</summary>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        foreach (IntroDiscoverySprite actor in actors)
            actor.Draw(oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Converts the jump cue into a suited or suitless handoff.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.SpawnSuitlessJump:
                SuitlessJumpRequested = true;
                JumpRequested = true;
                return;
            case CinematicSpriteCueKind.SpawnSuitedJump:
                JumpRequested = true;
                return;
            default:
                throw new InvalidOperationException($"The reward gesture programs publish no {cue} cue.");
        }
    }

    /// <summary>Creates an actor from compiled initialization and binds its discovery pre-instruction.</summary>
    private void Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        var (x, y, palette) =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            new((ushort)x, (ushort)y),
            (ushort)palette,
            record.Program);
        actor.PreInstructionPointerForDiscovery(record.PreInstruction);
        actors.Add(actor);
    }
}
