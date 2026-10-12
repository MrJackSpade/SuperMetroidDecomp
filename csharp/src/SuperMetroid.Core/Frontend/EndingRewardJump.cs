using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native reward jump/landing actor owner, ending at F604's screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    /// <summary>Stores bus.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Stores helmeted.</summary>
    private readonly bool helmeted;
    /// <summary>Stores queue graphics upload.</summary>
    private readonly Action<int> queueGraphicsUpload;
    /// <summary>Stores body.</summary>
    private readonly IntroDiscoverySprite body;
    /// <summary>Stores head.</summary>
    private readonly IntroDiscoverySprite? head;
    /// <summary>Stores velocity.</summary>
    private int velocity;
    /// <summary>Stores uploads.</summary>
    private int uploads;
    /// <summary>Gets or sets shot requested.</summary>
    public bool ShotRequested { get; private set; }
    /// <summary>Gets or sets object selection.</summary>
    public byte ObjectSelection { get; private set; }
    /// <summary>Gets body y.</summary>
    public short BodyY => unchecked((short)body.WholePosition.Y);

    /// <summary>Initializes a new ending reward jump instance.</summary>
    public EndingRewardJump(ISnesAddressSpace bus, EndingReward reward, Action<int> queueGraphicsUpload)
    {
        this.bus = bus;
        this.queueGraphicsUpload = Ensure.NotNull(queueGraphicsUpload);
        helmeted = reward == EndingReward.Armored;
        if (reward != EndingReward.Suitless)
            head = Spawn(helmeted ? EndingRewardActor.JumpHelmetedHead : EndingRewardActor.JumpHelmetlessHead);
        body = Spawn(reward == EndingReward.Suitless ? EndingRewardActor.JumpSuitlessBody : EndingRewardActor.JumpSuitedBody);
    }

    /// <summary>Steps.</summary>
    public void Step(Func<ushort, ushort>? instructionWord = null)
    {
        // Native fixed slots put the head above the body in the descending handler.
        // Both pre-instructions use the shared Samus velocity, so each call accelerates it.
        if (head is { IsActive: true })
        {
            if (InstalledPreInstruction(head) == EndingRewardJumpPreInstruction.HeadFlight)
            {
                Move(head);
                if (unchecked((short)head.WholePosition.Y) < EndingRewardJumpDefinitions.SheetSwitchY) head.Delete();
            }
            head.Step<EndingRewardJumpInstruction>(Instruction, instructionWord);
        }
        EndingRewardJumpPreInstruction? bodyPreInstruction = InstalledPreInstruction(body);
        if (bodyPreInstruction == EndingRewardJumpPreInstruction.BodyFlight)
        {
            Move(body);
            if (BodyY < EndingRewardJumpDefinitions.SheetSwitchY)
            {
                ObjectSelection = 3;
                body.SetAttributes(SnesObjPalettes.Index6);
                body.Redirect(EndingRewardJumpDefinitions.FallingList);
                body.PreInstructionPointerForDiscovery((ushort)EndingRewardJumpPreInstruction.Landing);
            }
        }
        else if (bodyPreInstruction == EndingRewardJumpPreInstruction.Landing)
        {
            if (uploads < EndingRewardJumpDefinitions.UploadCount) queueGraphicsUpload(uploads++);
            Move(body);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.WholePosition = body.WholePosition.WithY(EndingRewardJumpDefinitions.LandingY);
                body.Redirect(EndingRewardJumpDefinitions.LandedList);
                body.PreInstructionPointerForDiscovery(0);
            }
        }
        body.Step<EndingRewardJumpInstruction>(Instruction, instructionWord);
    }

    /// <summary>Draws.</summary>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        head?.Draw(oam, installedArt: installedArt);
        body.Draw(oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Moves.</summary>
    private void Move(IntroDiscoverySprite actor)
    {
        velocity = unchecked(velocity + EndingRewardJumpDefinitions.Gravity);
        uint position = unchecked(IntroCinematicMotion.Packed(actor.Position.Y) + (uint)velocity);
        actor.Position = actor.Position.WithY(IntroCinematicMotion.Unpacked(position));
    }

    /// <summary>Instruction.</summary>
    private ushort Instruction(EndingRewardJumpInstruction instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardJumpInstruction.Launch:
                velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return cursor;
            case EndingRewardJumpInstruction.PrepareHead:
                RequireHead().WholePosition = new((ushort)(helmeted ? 118 : 120), 120);
                return cursor;
            case EndingRewardJumpInstruction.LaunchHead:
                RequireHead().WholePosition = new((ushort)(helmeted ? 120 : 121), (ushort)(helmeted ? 114 : 116));
                return cursor;
            case EndingRewardJumpInstruction.Shoot:
                body.SetAttributes(SnesObjPalettes.Index7);
                ShotRequested = true;
                return cursor;
            default:
                throw new InvalidOperationException($"Undefined EndingRewardJumpInstruction {instruction}.");
        }
    }

    /// <summary>The jump-owned pre-instruction an actor runs, or null while it runs the shared no-op or none.</summary>
    private static EndingRewardJumpPreInstruction? InstalledPreInstruction(IntroDiscoverySprite actor) =>
        CinematicInstructionWords.TryDecode(actor.PreInstructionPointer, out EndingRewardJumpPreInstruction installed)
            ? installed : null;

    /// <summary>Requires head.</summary>
    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    /// <summary>Spawns.</summary>
    private static IntroDiscoverySprite Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        var (x, y, palette) =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            new((ushort)x, (ushort)y),
            (ushort)palette,
            record.InstructionList);
        actor.PreInstructionPointerForDiscovery(record.PreInstruction);
        return actor;
    }
}
