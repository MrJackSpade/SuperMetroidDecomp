using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native reward jump/landing actor owner, ending at F604's screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    /// <summary>Selects the armored head offsets used during the launch animation.</summary>
    private readonly bool helmeted;
    /// <summary>Queues one ending graphics sheet per landing update until all sheets are installed.</summary>
    private readonly Action<int> queueGraphicsUpload;
    /// <summary>Body actor whose vertical position controls the sheet switch and landing.</summary>
    private readonly IntroDiscoverySprite body;
    /// <summary>Separate head actor present for the two suited ending rewards.</summary>
    private readonly IntroDiscoverySprite? head;
    /// <summary>Shared packed vertical velocity; each active actor movement applies gravity to it.</summary>
    internal int velocity;
    /// <summary>Number of landing graphics sheets already queued for upload.</summary>
    private int uploads;
    /// <summary>Whether the shooting instruction has requested the ending screen capture.</summary>
    public bool ShotRequested { get; private set; }
    /// <summary>Set when the body crosses the graphics sheet switch height and Samus is drawn from her falling sheet.</summary>
    public bool FallingSheetSelected { get; private set; }
    /// <summary>Whole-pixel body height used to select the falling and landed instruction lists.</summary>
    public short BodyY => unchecked((short)body.WholePosition.Y);

    /// <summary>Spawns the body and, for suited rewards, its helmeted or helmetless head actor.</summary>
    public EndingRewardJump(EndingReward reward, Action<int> queueGraphicsUpload)
    {
        this.queueGraphicsUpload = Ensure.NotNull(queueGraphicsUpload);
        helmeted = reward == EndingReward.Armored;
        if (reward != EndingReward.Suitless)
            head = Spawn(helmeted ? EndingRewardActor.JumpHelmetedHead : EndingRewardActor.JumpHelmetlessHead);
        body = Spawn(reward == EndingReward.Suitless ? EndingRewardActor.JumpSuitlessBody : EndingRewardActor.JumpSuitedBody);
    }

    /// <summary>Advances head before body, including shared gravity, landing uploads, and instruction effects.</summary>
    public void Step()
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
            head.Step(HandleCue);
        }
        EndingRewardJumpPreInstruction? bodyPreInstruction = InstalledPreInstruction(body);
        if (bodyPreInstruction == EndingRewardJumpPreInstruction.BodyFlight)
        {
            Move(body);
            if (BodyY < EndingRewardJumpDefinitions.SheetSwitchY)
            {
                FallingSheetSelected = true;
                body.SetAttributes(SnesObjPalettes.Index6);
                body.Start(CinematicSpriteProgram.Falling);
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
                body.Start(CinematicSpriteProgram.Landing);
                body.PreInstructionPointerForDiscovery(0);
            }
        }
        body.Step(HandleCue);
    }

    /// <summary>Draws the active head and body into a supplied frame or a newly finalized OAM buffer.</summary>
    public OamBuffer Draw(EndingRewardSpritePresentation? installedArt = null, OamBuffer? destination = null)
    {
        var oam = destination ?? new OamBuffer();
        if (destination is null) oam.BeginFrame();
        head?.Draw(oam, installedArt: installedArt);
        body.Draw(oam, installedArt: installedArt);
        if (destination is null) oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Applies gravity to shared velocity and moves an actor using packed fixed-point height.</summary>
    private void Move(IntroDiscoverySprite actor)
    {
        velocity = unchecked(velocity + EndingRewardJumpDefinitions.Gravity);
        uint position = unchecked(IntroCinematicMotion.Packed(actor.Position.Y) + (uint)velocity);
        actor.Position = actor.Position.WithY(IntroCinematicMotion.Unpacked(position));
    }

    /// <summary>Handles launch, head placement, and the shoot event the jump programs publish.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.Launch:
                velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return;
            case CinematicSpriteCueKind.PrepareHead:
                RequireHead().WholePosition = new((ushort)(helmeted ? 118 : 120), 120);
                return;
            case CinematicSpriteCueKind.LaunchHead:
                RequireHead().WholePosition = new((ushort)(helmeted ? 120 : 121), (ushort)(helmeted ? 114 : 116));
                return;
            case CinematicSpriteCueKind.Shoot:
                body.SetAttributes(SnesObjPalettes.Index7);
                ShotRequested = true;
                return;
            default:
                throw new InvalidOperationException($"The reward jump programs publish no {cue} cue.");
        }
    }

    /// <summary>The jump-owned pre-instruction an actor runs, or null while it runs the shared no-op or none.</summary>
    private static EndingRewardJumpPreInstruction? InstalledPreInstruction(IntroDiscoverySprite actor) =>
        CinematicInstructionWords.TryDecode(actor.PreInstructionPointer, out EndingRewardJumpPreInstruction installed)
            ? installed : null;

    /// <summary>Requires a suited jump's head actor when a head-specific instruction runs.</summary>
    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    /// <summary>Creates a jump actor from its catalogued initial position, palette, and instruction pointers.</summary>
    private static IntroDiscoverySprite Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        var (x, y, palette) =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            new((ushort)x, (ushort)y),
            (ushort)palette,
            record.Program);
        actor.PreInstructionPointerForDiscovery(record.PreInstruction);
        return actor;
    }
}
