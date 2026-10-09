using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Runs E342's reward gesture actors until their native jumping-actor handoff.</summary>
internal sealed class EndingRewardGesture
{
    /// <summary>Cartridge address space used by each reward actor while advancing and drawing.</summary>
    private readonly ISnesAddressSpace bus;

    /// <summary>Active actors spawned for the selected reward, kept in native allocation order.</summary>
    private readonly List<IntroDiscoverySprite> actors = [];

    /// <summary>Gets whether the native jumping-actor handoff has been requested.</summary>
    public bool JumpRequested { get; private set; }

    /// <summary>Gets whether that handoff uses the suitless jump actor.</summary>
    public bool SuitlessJumpRequested { get; private set; }

    /// <summary>Creates the actors for a reward and initializes their native starting state.</summary>
    /// <param name="bus">Address space used by the actors' instruction and sprite data.</param>
    /// <param name="reward">Reward appearance that determines which head, body, and arm actors are created.</param>
    public EndingRewardGesture(ISnesAddressSpace bus, EndingReward reward)
    {
        this.bus = bus;
        if (reward == EndingReward.Suitless)
        {
            Spawn(EndingRewardActorDefinitions.SuitlessUpper);
            Spawn(EndingRewardActorDefinitions.SuitlessLower);
        }
        else
        {
            // Keep E342's allocation order: head first, then body, then arm.
            Spawn(reward == EndingReward.Helmetless ? EndingRewardActorDefinitions.HelmetlessHead
                : EndingRewardActorDefinitions.HelmetedHead);
            Spawn(EndingRewardActorDefinitions.SuitedBody);
            Spawn(EndingRewardActorDefinitions.SuitedArm);
        }
    }

    /// <summary>Advances all active actors once and records any jump handoff they request.</summary>
    /// <param name="instructionWord">Optional instruction-word reader supplied by verification; null uses actor defaults.</param>
    public void Step(Func<ushort, ushort>? instructionWord = null)
    {
        if (JumpRequested) throw new InvalidOperationException("Reward gesture handoff must be consumed before advancing again.");
        foreach (IntroDiscoverySprite actor in actors)
            actor.Step(bus, HandleInstruction, instructionWord);
        actors.RemoveAll(actor => !actor.IsActive);
    }

    /// <summary>Draws the active reward actors into a new frame or the caller's existing OAM buffer.</summary>
    /// <param name="installedArt">Optional editable artwork used in place of each actor's built-in composition.</param>
    /// <param name="destination">Existing frame buffer to append to; when omitted, this method creates and finalizes a new frame.</param>
    /// <returns>The buffer containing the rendered actors.</returns>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        foreach (IntroDiscoverySprite actor in actors)
            actor.Draw(bus, oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Handles the two reward-specific spawn opcodes that transfer control to a jump actor.</summary>
    /// <param name="instruction">Opcode returned by the shared discovery-actor interpreter.</param>
    /// <param name="cursor">Instruction cursor following the opcode.</param>
    /// <returns>The cursor to retain for a handled handoff, or null so the shared interpreter can reject an unsupported opcode.</returns>
    private ushort? HandleInstruction(ushort instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardActorDefinitions.SpawnSuitlessJump:
                SuitlessJumpRequested = true;
                JumpRequested = true;
                return cursor;
            case EndingRewardActorDefinitions.SpawnSuitedJump:
                JumpRequested = true;
                return cursor;
            default:
                return null; // The shared interpreter reports the exact unsupported opcode.
        }
    }

    /// <summary>Creates and registers one actor from its catalog definition and native initialization record.</summary>
    /// <param name="definition">Catalog identity selecting the actor's initialization and instruction list.</param>
    private void Spawn(ushort definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        var (x, y, palette) =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            (ushort)x,
            (ushort)y,
            (ushort)palette,
            record.InstructionList);
        actor.PreInstructionPointerForDiscovery(record.PreInstruction);
        actors.Add(actor);
    }
}
