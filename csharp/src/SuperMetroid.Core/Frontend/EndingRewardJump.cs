using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    public int VerticalVelocity => velocity;

    public EndingRewardJump(ISnesAddressSpace bus, EndingReward reward, Action<int> queueGraphicsUpload)
    {
        this.bus = bus;
        this.queueGraphicsUpload = queueGraphicsUpload ?? throw new ArgumentNullException(nameof(queueGraphicsUpload));
        helmeted = reward == EndingReward.Armored;
        if (reward != EndingReward.Suitless)
            head = Spawn(helmeted ? EndingRewardJumpDefinitions.HelmetedHead : EndingRewardJumpDefinitions.HelmetlessHead);
        body = Spawn(reward == EndingReward.Suitless ? EndingRewardJumpDefinitions.SuitlessBody : EndingRewardJumpDefinitions.SuitedBody);
    }

    public void Step()
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
            head.Step(bus, Instruction);
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
        body.Step(bus, Instruction);
    }

    public OamBuffer Draw()
    {
        var oam = new OamBuffer(); oam.BeginFrame();
        head?.Draw(bus, oam); body.Draw(bus, oam);
        oam.FinalizeFrame(); return oam;
    }

    private void Move(IntroDiscoverySprite actor)
    {
        velocity = unchecked(velocity + EndingRewardJumpDefinitions.Gravity);
        uint position = ((uint)actor.YPosition << 16) | actor.YSubPosition;
        position = unchecked(position + (uint)velocity);
        actor.YPosition = (ushort)(position >> 16);
        actor.YSubPosition = (ushort)position;
    }

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

    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    private IntroDiscoverySprite Spawn(ushort definition)
    {
        ushort initialization = RomDataReader.ReadWordFixedBank(bus, IntroCinematicRomData.Banks.CinematicCode | definition);
        var (x, y, palette) = EndingRewardActorDefinitions.GetInitialization(initialization);
        ushort list = RomDataReader.ReadWordFixedBank(bus, IntroCinematicRomData.Banks.CinematicCode | (definition + 4));
        return new IntroDiscoverySprite((ushort)x, (ushort)y, (ushort)palette, list);
    }
}
