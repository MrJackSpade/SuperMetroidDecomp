using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Assets;

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
            if (head.PreInstruction == CinematicSpritePreInstruction.RewardHeadFlight)
            {
                Move(head);
                if (unchecked((short)head.WholePosition.Y) < EndingRewardJumpDefinitions.SheetSwitchY) head.Delete();
            }
            head.Step(HandleCue);
        }
        CinematicSpritePreInstruction? bodyPreInstruction = body.PreInstruction;
        if (bodyPreInstruction == CinematicSpritePreInstruction.RewardBodyFlight)
        {
            Move(body);
            if (BodyY < EndingRewardJumpDefinitions.SheetSwitchY)
            {
                FallingSheetSelected = true;
                body.SetAttributes(SnesObjPalettes.Palette(6));
                body.Start(CinematicSpriteProgram.Falling);
                body.InstallPreInstruction(CinematicSpritePreInstruction.RewardLanding);
            }
        }
        else if (bodyPreInstruction == CinematicSpritePreInstruction.RewardLanding)
        {
            if (uploads < EndingRewardJumpDefinitions.UploadCount) queueGraphicsUpload(uploads++);
            Move(body);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.WholePosition = body.WholePosition.WithY(EndingRewardJumpDefinitions.LandingY);
                body.Start(CinematicSpriteProgram.Landing);
                body.InstallPreInstruction(null);
            }
        }
        body.Step(HandleCue);
    }

    /// <summary>Places the active head and body as the supplied sprite entries.</summary>
    public void Draw(ScreenSprites destination, EndingRewardSpritePresentation? installedArt)
    {
        Ensure.NotNull(destination);
        head?.Draw(destination, installedArt: installedArt);
        body.Draw(destination, installedArt: installedArt);
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
                body.SetAttributes(SnesObjPalettes.Palette(7));
                ShotRequested = true;
                return;
            case CinematicSpriteCueKind.SetPreInstruction:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.FadeOutZebesText:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnRinkas0And1:
            case CinematicSpriteCueKind.SpawnRinkas2And3:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartFlyingToZebes:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The reward jump programs publish no {cue} cue.");
        }
    }


    /// <summary>Requires a suited jump's head actor when a head-specific instruction runs.</summary>
    private IntroDiscoverySprite RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    /// <summary>Creates a jump actor from its catalogued initial position, palette, and instruction pointers.</summary>
    private static IntroDiscoverySprite Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActorDefinitions.Get(definition);
        EndingRewardActorInitializationDefinition initialization =
            EndingRewardActorDefinitions.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            initialization.Position,
            initialization.PaletteBits,
            record.Program);
        actor.InstallPreInstruction(record.PreInstruction);
        return actor;
    }
}
