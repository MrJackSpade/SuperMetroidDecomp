using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Reward jump/landing actor owner, ending at the screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    /// <summary>
    /// The program Samus's body falls by once she passes above the screen. A suited jump's head flies ahead of the body
    /// and is deleted as it passes the height at which the body starts to fall, so only the body falls and lands.
    /// </summary>
    internal const CinematicSpriteProgram FallingProgram = CinematicSpriteProgram.Falling;

    /// <summary>The program Samus's body lands and shoots by.</summary>
    internal const CinematicSpriteProgram LandingProgram = CinematicSpriteProgram.Landing;

    /// <summary>The colouring Samus's body falls and lands in.</summary>
    internal const EndingSpriteColouring FallingColouring = EndingSpriteColouring.FallingSamus;

    /// <summary>The colouring the landing program's shoot cue turns Samus's body to, which it keeps to the end.</summary>
    internal const EndingSpriteColouring ShootingColouring = EndingSpriteColouring.ShootingSamus;

    /// <summary>The landing program's cue that turns Samus's body to <see cref="ShootingColouring"/> and requests the shot.</summary>
    internal const CinematicSpriteCueKind ShootCue = CinematicSpriteCueKind.Shoot;

    /// <summary>Selects the armored head offsets used during the launch animation.</summary>
    private readonly bool helmeted;
    /// <summary>Body actor whose vertical position controls the fall and landing.</summary>
    private readonly IntroDiscoverySprite body;
    /// <summary>Separate head actor present for the two suited ending rewards.</summary>
    private readonly IntroDiscoverySprite? head;
    /// <summary>Shared vertical velocity; each active actor movement applies gravity to it.</summary>
    internal SignedSixteenSixteen Velocity { get; private set; }
    /// <summary>Whether the shooting instruction has requested the ending screen capture.</summary>
    public bool ShotRequested { get; private set; }
    /// <summary>Set when the body passes above the screen and Samus starts to fall back, as the falling Samus.</summary>
    public bool Falling { get; private set; }
    /// <summary>Whole-pixel body height used to select the falling and landed programs.</summary>
    public int BodyY => CoordinateAxis.AsSigned(body.WholePosition.Y);

    /// <summary>Spawns the body and, for suited rewards, its helmeted or helmetless head actor.</summary>
    public EndingRewardJump(EndingReward reward)
    {
        helmeted = reward == EndingReward.Armored;
        if (EndingRewardCast.JumpHead(reward) is { } headActor)
            head = Spawn(headActor);
        body = Spawn(EndingRewardCast.JumpBody(reward));
    }

    /// <summary>Advances head before body, including shared gravity and instruction effects.</summary>
    public void Step()
    {
        // Fixed slots put the head above the body in the descending handler.
        // Both frame behaviours use the shared Samus velocity, so each call accelerates it.
        if (head is { IsActive: true })
        {
            if (head.FrameBehavior == CinematicSpriteFrameBehavior.RewardHeadFlight)
            {
                Move(head);
                if (CoordinateAxis.AsSigned(head.WholePosition.Y) < EndingRewardJumpDefinitions.FallStartY) head.Delete();
            }
            head.Step(HandleCue);
        }
        CinematicSpriteFrameBehavior? bodyFrameBehavior = body.FrameBehavior;
        if (bodyFrameBehavior == CinematicSpriteFrameBehavior.RewardBodyFlight)
        {
            Move(body);
            if (BodyY < EndingRewardJumpDefinitions.FallStartY)
            {
                Falling = true;
                body.SetColouring(EndingShades.Drawn(FallingColouring));
                body.Start(FallingProgram);
                body.InstallFrameBehavior(CinematicSpriteFrameBehavior.RewardLanding);
            }
        }
        else if (bodyFrameBehavior == CinematicSpriteFrameBehavior.RewardLanding)
        {
            Move(body);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.WholePosition = body.WholePosition.WithY(EndingRewardJumpDefinitions.LandingY);
                body.Start(LandingProgram);
                body.InstallFrameBehavior(null);
            }
        }
        body.Step(HandleCue);
    }

    /// <summary>
    /// Places the active head and body as the supplied sprite entries: the jumping Samus's frames from
    /// <paramref name="jumpingArt"/>, and the falling Samus's, once she falls back, from <paramref name="fallingArt"/>.
    /// </summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation jumpingArt, IIntroCinematicSpritePresentation fallingArt)
    {
        Ensure.NotNull(destination);
        head?.Draw(destination, installedArt: jumpingArt);
        body.Draw(destination, installedArt: Falling ? fallingArt : jumpingArt);
    }

    /// <summary>Applies gravity to shared velocity and moves an actor by it.</summary>
    private void Move(IntroDiscoverySprite actor)
    {
        Velocity += EndingRewardJumpDefinitions.Gravity;
        actor.Position = actor.Position.WithY(actor.Position.Y.MovedBy(Velocity));
    }

    /// <summary>Handles launch, head placement, and the shoot event the jump programs publish.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.Launch:
                Velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return;
            case CinematicSpriteCueKind.PrepareHead:
                RequireHead().WholePosition = new(helmeted ? 118 : 120, 120);
                return;
            case CinematicSpriteCueKind.LaunchHead:
                RequireHead().WholePosition = new(helmeted ? 120 : 121, helmeted ? 114 : 116);
                return;
            case ShootCue:
                body.SetColouring(EndingShades.Drawn(ShootingColouring));
                ShotRequested = true;
                return;
            case CinematicSpriteCueKind.SetFrameBehavior:
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
    /// <summary>Creates a jump actor from its catalogued initial position, palette, and programs.</summary>
    private static IntroDiscoverySprite Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition record = EndingRewardActors.Get(definition);
        EndingRewardActorInitializationDefinition initialization =
            EndingRewardActors.GetInitialization(record.Initialization);
        var actor = new IntroDiscoverySprite(
            initialization.Position,
            EndingShades.Drawn(initialization.Colouring),
            record.Program);
        actor.InstallFrameBehavior(record.FrameBehavior);
        return actor;
    }
}
