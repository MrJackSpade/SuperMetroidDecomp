using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Reward jump/landing actor owner, ending at the screen-shot request.</summary>
internal sealed class EndingRewardJump
{
    /// <summary>
    /// The animation Samus's body falls by once she passes above the screen. A suited jump's head flies ahead of the body
    /// and is deleted as it passes the height at which the body starts to fall, so only the body falls and lands.
    /// </summary>
    internal const EndingRewardAnimation FallingAnimation = EndingRewardAnimation.Falling;

    /// <summary>The animation Samus's body lands and shoots by.</summary>
    internal const EndingRewardAnimation LandingAnimation = EndingRewardAnimation.Landing;

    /// <summary>The colouring Samus's body falls and lands in.</summary>
    internal const EndingSpriteColouring FallingColouring = EndingSpriteColouring.FallingSamus;

    /// <summary>The colouring the landing animation's shoot cue turns Samus's body to, which it keeps to the end.</summary>
    internal const EndingSpriteColouring ShootingColouring = EndingSpriteColouring.ShootingSamus;

    /// <summary>The landing animation's cue that turns Samus's body to <see cref="ShootingColouring"/> and requests the shot.</summary>
    internal const EndingRewardJumpCue ShootCue = EndingRewardJumpCue.Shoot;

    /// <summary>Selects the armored head offsets used during the launch animation.</summary>
    private readonly bool helmeted;
    /// <summary>Body actor whose vertical position controls the fall and landing.</summary>
    private readonly CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> body;
    /// <summary>Separate head actor present for the two suited ending rewards.</summary>
    private readonly CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue>? head;
    /// <summary>Whether the suited jump's head flies, as its animation starts it doing ahead of the body.</summary>
    private bool headFlying;
    /// <summary>How Samus's body moves: rising on her jump, falling back, or none before the jump and after landing.</summary>
    private EndingRewardBodyMotion? bodyMotion;
    /// <summary>Shared vertical velocity; each active actor movement applies gravity to it.</summary>
    internal SignedSixteenSixteen Velocity { get; private set; }
    /// <summary>Whether the shooting cue has requested the ending screen capture.</summary>
    public bool ShotRequested { get; private set; }
    /// <summary>Set when the body passes above the screen and Samus starts to fall back, as the falling Samus.</summary>
    public bool Falling { get; private set; }
    /// <summary>Whole-pixel body height used to select the falling and landed animations.</summary>
    public int BodyY => body.ScreenPlace.Y;

    /// <summary>Spawns the body and, for suited rewards, its helmeted or helmetless head actor.</summary>
    public EndingRewardJump(EndingReward reward)
    {
        helmeted = reward == EndingReward.Armored;
        if (EndingRewardCast.JumpHead(reward) is { } headActor)
            head = Spawn(headActor);
        body = Spawn(EndingRewardCast.JumpBody(reward));
    }

    /// <summary>Advances head before body, including shared gravity and cue effects.</summary>
    public void Step()
    {
        // The head steps before the body, as it runs ahead of it.
        // Both motions use the shared Samus velocity, so each update accelerates it.
        if (head is { IsActive: true })
        {
            if (headFlying)
            {
                head.Position = Accelerated(head.Position);
                if (head.ScreenPlace.Y < EndingRewardJumpDefinitions.FallStartY) head.Delete();
            }
            head.Step(HandleCue);
        }
        if (bodyMotion == EndingRewardBodyMotion.Rising)
        {
            body.Position = Accelerated(body.Position);
            if (BodyY < EndingRewardJumpDefinitions.FallStartY)
            {
                Falling = true;
                body.SetColouring(EndingShades.Drawn(FallingColouring));
                body.Start(FallingAnimation);
                bodyMotion = EndingRewardBodyMotion.Falling;
            }
        }
        else if (bodyMotion == EndingRewardBodyMotion.Falling)
        {
            body.Position = Accelerated(body.Position);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.ScreenPlace = body.ScreenPlace.WithY(EndingRewardJumpDefinitions.LandingY);
                body.Start(LandingAnimation);
                bodyMotion = null;
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

    /// <summary>Applies gravity to the shared velocity and returns where an actor at <paramref name="position"/> moves by it.</summary>
    private FixedVector<ScreenPixels> Accelerated(FixedVector<ScreenPixels> position)
    {
        Velocity += EndingRewardJumpDefinitions.Gravity;
        return position.WithY(position.Y + Velocity);
    }

    /// <summary>Handles launch, head placement, the flights and the shoot event the jump animations publish.</summary>
    private void HandleCue(EndingRewardJumpCue cue)
    {
        switch (cue)
        {
            case EndingRewardJumpCue.Launch:
                Velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return;
            case EndingRewardJumpCue.PrepareHead:
                RequireHead().ScreenPlace = helmeted ? EndingRewardJumpDefinitions.HelmetedHeadReadied : EndingRewardJumpDefinitions.HelmetlessHeadReadied;
                return;
            case EndingRewardJumpCue.LaunchHead:
                RequireHead().ScreenPlace = helmeted ? EndingRewardJumpDefinitions.HelmetedHeadLaunched : EndingRewardJumpDefinitions.HelmetlessHeadLaunched;
                return;
            case ShootCue:
                body.SetColouring(EndingShades.Drawn(ShootingColouring));
                ShotRequested = true;
                return;
            case EndingRewardJumpCue.StartBodyFlight:
                bodyMotion = EndingRewardBodyMotion.Rising;
                return;
            case EndingRewardJumpCue.StartHeadFlight:
                headFlying = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingRewardJumpCue)} {cue}.");
        }
    }


    /// <summary>Requires a suited jump's head actor when a head-specific cue runs.</summary>
    private CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    /// <summary>Creates a jump actor from its catalogued initial position, colouring, and animations.</summary>
    private static CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition authored = EndingRewardActors.Get(definition);
        var actor = new CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue>(
            authored.Start.Position,
            EndingShades.Drawn(authored.Start.Colouring),
            EndingRewardAnimations.Jump,
            authored.Animation);
        return actor;
    }
}
