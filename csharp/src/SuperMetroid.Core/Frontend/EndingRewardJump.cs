using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native reward jump/landing actor owner, ending at F604's screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    /// <summary>Address-space context used to advance and draw the ending actor sprites.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Whether this reward uses the armored helmet placement during the jump sequence.</summary>
    private readonly bool helmeted;
    /// <summary>Host callback that queues one indexed upload of the reward graphics.</summary>
    private readonly Action<int> queueGraphicsUpload;
    /// <summary>Active body sprite whose flight and landing determine the shared jump motion.</summary>
    private readonly IntroDiscoverySprite body;
    /// <summary>Optional separate head actor used by suited rewards; suitless rewards have no head sprite.</summary>
    private readonly IntroDiscoverySprite? head;
    /// <summary>Shared signed fixed-point vertical velocity accelerated by each active actor's movement step.</summary>
    private int velocity;
    /// <summary>Number of reward graphics uploads already requested during the landing sequence.</summary>
    private int uploads;
    /// <summary>Signals that the native shoot instruction ran and the ending flow should show its screenshot.</summary>
    public bool ShotRequested { get; private set; }
    /// <summary>OBJ palette and size selection used after the body reaches the sheet-switch height.</summary>
    public byte ObjectSelection { get; private set; }
    /// <summary>Current body Y coordinate interpreted as a signed screen position.</summary>
    public short BodyY => unchecked((short)body.YPosition);

    /// <summary>Creates the reward actors selected by the ending reward and retains the host graphics-upload callback.</summary>
    /// <param name="bus">Address-space context used by the actors' instruction lists and rendering.</param>
    /// <param name="reward">Reward appearance that determines whether the actor is suitless, helmetless, or armored.</param>
    /// <param name="queueGraphicsUpload">Callback invoked with each upload index as the body lands.</param>
    public EndingRewardJump(ISnesAddressSpace bus, EndingReward reward, Action<int> queueGraphicsUpload)
    {
        this.bus = bus;
        this.queueGraphicsUpload = queueGraphicsUpload ?? throw new ArgumentNullException(nameof(queueGraphicsUpload));
        helmeted = reward == EndingReward.Armored;
        if (reward != EndingReward.Suitless)
            head = Spawn(helmeted ? EndingRewardJumpDefinitions.HelmetedHead : EndingRewardJumpDefinitions.HelmetlessHead);
        body = Spawn(reward == EndingReward.Suitless ? EndingRewardJumpDefinitions.SuitlessBody : EndingRewardJumpDefinitions.SuitedBody);
    }

    /// <summary>Advances active actors, their instruction lists, shared jump motion, landing uploads, and screenshot request state.</summary>
    /// <param name="instructionWord">Optional reader for native instruction words; when absent, each sprite uses its built-in instruction source.</param>
    public void Step(Func<ushort, ushort>? instructionWord = null)
    {
        // Native fixed slots put the head above the body in the descending handler.
        // Both pre-instructions use the shared Samus velocity, so each call accelerates it.
        if (head is { IsActive: true })
        {
            if (head.PreInstructionPointer == EndingRewardJumpDefinitions.HeadFlight)
            {
                Move(head);
                if (unchecked((short)head.YPosition) < EndingRewardJumpDefinitions.SheetSwitchY) head.Delete();
            }
            head.Step(bus, Instruction, instructionWord);
        }
        if (body.PreInstructionPointer == EndingRewardJumpDefinitions.BodyFlight)
        {
            Move(body);
            if (BodyY < EndingRewardJumpDefinitions.SheetSwitchY)
            {
                ObjectSelection = 3;
                body.SetAttributes(SnesObjPalettes.Index6);
                body.Redirect(EndingRewardJumpDefinitions.FallingList);
                body.PreInstructionPointerForDiscovery(EndingRewardJumpDefinitions.Landing);
            }
        }
        else if (body.PreInstructionPointer == EndingRewardJumpDefinitions.Landing)
        {
            if (uploads < EndingRewardJumpDefinitions.UploadCount) queueGraphicsUpload(uploads++);
            Move(body);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.YPosition = EndingRewardJumpDefinitions.LandingY;
                body.Redirect(EndingRewardJumpDefinitions.LandedList);
                body.PreInstructionPointerForDiscovery(0);
            }
        }
        body.Step(bus, Instruction, instructionWord);
    }

    /// <summary>Draws the active head and body sprites, creating and finalizing an OAM frame when no destination is supplied.</summary>
    /// <param name="installedArt">Optional installed artwork used to resolve the reward sprites.</param>
    /// <param name="destination">Existing OAM buffer to append to; caller owns its frame lifecycle when provided.</param>
    /// <returns>The supplied buffer or a newly created buffer containing the actor entries.</returns>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        head?.Draw(bus, oam, installedArt: installedArt);
        body.Draw(bus, oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Applies gravity and advances one actor's Y coordinate using the shared fixed-point velocity.</summary>
    /// <param name="actor">Sprite whose integer and fractional Y position are updated.</param>
    private void Move(IntroDiscoverySprite actor)
    {
        velocity = unchecked(velocity + EndingRewardJumpDefinitions.Gravity);
        uint position = ((uint)actor.YPosition << 16) | actor.YSubPosition;
        position = unchecked(position + (uint)velocity);
        actor.YPosition = (ushort)(position >> 16);
        actor.YSubPosition = (ushort)position;
    }

    /// <summary>Handles reward-specific launch, head placement, and shot instructions, leaving unrelated words to the sprite interpreter.</summary>
    /// <param name="instruction">Native instruction word currently being dispatched.</param>
    /// <param name="cursor">Instruction cursor to retain when this handler consumes the word.</param>
    /// <returns>The unchanged cursor for handled words, or null when the instruction belongs to the default interpreter.</returns>
    private ushort? Instruction(ushort instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardJumpDefinitions.Launch:
                velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return cursor;
            case EndingRewardJumpDefinitions.PrepareHead:
                RequireHead().XPosition = (ushort)(helmeted ? 118 : 120);
                RequireHead().YPosition = 120;
                return cursor;
            case EndingRewardJumpDefinitions.LaunchHead:
                RequireHead().XPosition = (ushort)(helmeted ? 120 : 121);
                RequireHead().YPosition = (ushort)(helmeted ? 114 : 116);
                return cursor;
            case EndingRewardJumpDefinitions.Shoot:
                body.SetAttributes(SnesObjPalettes.Index7);
                ShotRequested = true;
                return cursor;
            default: return null;
        }
    }

    /// <summary>Returns the separate head actor required by suited jump instructions.</summary>
    /// <returns>The configured head sprite.</returns>
    /// <exception cref="InvalidDataException">A suited instruction requests a head when this reward has none.</exception>
    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");

    /// <summary>Creates a reward sprite from its definition, initialization record, and instruction-list pointer.</summary>
    /// <param name="definition">Actor definition pointer selecting the sprite's initial position, palette, and instruction list.</param>
    /// <returns>The initialized sprite with its native pre-instruction installed.</returns>
    private static IntroDiscoverySprite Spawn(ushort definition)
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
        return actor;
    }
}
