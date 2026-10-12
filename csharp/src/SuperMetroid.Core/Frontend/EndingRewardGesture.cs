using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Runs E342's reward gesture actors until their native jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Stores actors.</summary>
    private readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Gets or sets jump requested.</summary>
    public bool JumpRequested { get; private set; }
    /// <summary>Gets or sets suitless jump requested.</summary>
    public bool SuitlessJumpRequested { get; private set; }

    /// <summary>Initializes a new ending reward gesture instance.</summary>
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

    /// <summary>Steps.</summary>
    public void Step(Func<ushort, ushort>? instructionWord = null)
    {
        if (JumpRequested) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        foreach (IntroDiscoverySprite actor in actors)
            actor.Step<EndingRewardGestureInstruction>(HandleInstruction, instructionWord);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    /// <summary>Draws.</summary>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        foreach (IntroDiscoverySprite actor in actors)
            actor.Draw(oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Handles instruction.</summary>
    private ushort HandleInstruction(EndingRewardGestureInstruction instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardGestureInstruction.SpawnSuitlessJump:
                SuitlessJumpRequested = true;
                JumpRequested = true;
                return cursor;
            case EndingRewardGestureInstruction.SpawnSuitedJump:
                JumpRequested = true;
                return cursor;
            default:
                throw new InvalidOperationException($"Undefined EndingRewardGestureInstruction {instruction}.");
        }
    }

    /// <summary>Spawns.</summary>
    private void Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        var (x, y, palette) =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            new((ushort)x, (ushort)y),
            (ushort)palette,
            record.InstructionList);
        actor.PreInstructionPointerForDiscovery(record.PreInstruction);
        actors.Add(actor);
    }
}
