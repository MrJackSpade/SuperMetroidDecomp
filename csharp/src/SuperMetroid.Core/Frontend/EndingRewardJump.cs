using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native reward jump/landing actor owner, ending at F604's screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    private readonly ISnesAddressSpace bus;
    private readonly bool helmeted;
    private readonly Action<int> queueGraphicsUpload;
    private readonly IntroDiscoverySprite body;
    private readonly IntroDiscoverySprite? head;
    private int velocity;
    private int uploads;
    public bool ShotRequested { get; private set; }
    public byte ObjectSelection { get; private set; }
    public short BodyY => unchecked((short)body.YPosition);

    public EndingRewardJump(ISnesAddressSpace bus, EndingReward reward, Action<int> queueGraphicsUpload)
    {
        this.bus = bus;
        this.queueGraphicsUpload = queueGraphicsUpload ?? throw new ArgumentNullException(nameof(queueGraphicsUpload));
        helmeted = reward == EndingReward.Armored;
        if (reward != EndingReward.Suitless)
            head = Spawn(helmeted ? EndingRewardActor.JumpHelmetedHead : EndingRewardActor.JumpHelmetlessHead);
        body = Spawn(reward == EndingReward.Suitless ? EndingRewardActor.JumpSuitlessBody : EndingRewardActor.JumpSuitedBody);
    }

    public void Step(Func<ushort, ushort>? instructionWord = null)
    {
        // Native fixed slots put the head above the body in the descending handler.
        // Both pre-instructions use the shared Samus velocity, so each call accelerates it.
        if (head is { IsActive: true })
        {
            if (InstalledPreInstruction(head) == EndingRewardJumpPreInstruction.HeadFlight)
            {
                Move(head);
                if (unchecked((short)head.YPosition) < EndingRewardJumpDefinitions.SheetSwitchY) head.Delete();
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
                body.YPosition = EndingRewardJumpDefinitions.LandingY;
                body.Redirect(EndingRewardJumpDefinitions.LandedList);
                body.PreInstructionPointerForDiscovery(0);
            }
        }
        body.Step<EndingRewardJumpInstruction>(Instruction, instructionWord);
    }

    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        head?.Draw(oam, installedArt: installedArt);
        body.Draw(oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    private void Move(IntroDiscoverySprite actor)
    {
        velocity = unchecked(velocity + EndingRewardJumpDefinitions.Gravity);
        uint position = ((uint)actor.YPosition << 16) | actor.YSubPosition;
        position = unchecked(position + (uint)velocity);
        actor.YPosition = (ushort)(position >> 16);
        actor.YSubPosition = (ushort)position;
    }

    private ushort Instruction(EndingRewardJumpInstruction instruction, ushort cursor)
    {
        switch (instruction)
        {
            case EndingRewardJumpInstruction.Launch:
                velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return cursor;
            case EndingRewardJumpInstruction.PrepareHead:
                RequireHead().XPosition = (ushort)(helmeted ? 118 : 120);
                RequireHead().YPosition = 120;
                return cursor;
            case EndingRewardJumpInstruction.LaunchHead:
                RequireHead().XPosition = (ushort)(helmeted ? 120 : 121);
                RequireHead().YPosition = (ushort)(helmeted ? 114 : 116);
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

    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    private static IntroDiscoverySprite Spawn(EndingRewardActor definition)
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
