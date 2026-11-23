using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Reward jump/landing actor owner, ending at the screen-shot request.</summary>
internal sealed partial class EndingRewardJump
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

    /// <summary>Body actor whose vertical position controls the fall and landing.</summary>
    private readonly CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> body;
    /// <summary>Separate head actor present for the two suited ending rewards.</summary>
    private readonly CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue>? head;
    /// <summary>How the suited jump's head moves: standing until its animation starts it flying ahead of the body, then rising.</summary>
    private EndingRewardJumpMotion headMotion = EndingRewardJumpMotion.Standing;
    /// <summary>How Samus's body moves (#2541): standing until her jump, rising, falling back once above the screen, then landed.</summary>
    private EndingRewardJumpMotion bodyMotion = EndingRewardJumpMotion.Standing;
    /// <summary>
    /// Handles the cues the body's animation raises: a suited jump's body also places its head as it prepares and launches.
    /// </summary>
    private readonly Func<EndingRewardJumpCue, bool> bodyCues;
    /// <summary>Shared vertical velocity; each active actor movement applies gravity to it.</summary>
    internal SignedSixteenSixteen Velocity { get; set; }
    /// <summary>Whether Samus has passed above the screen and shows as the falling Samus, falling back or landed.</summary>
    internal bool FellBack => bodyMotion is EndingRewardJumpMotion.Falling or EndingRewardJumpMotion.Landed;
    /// <summary>Whole-pixel body height used to select the falling and landed animations.</summary>
    internal int BodyY => body.ScreenPlace.Y;

    /// <summary>Spawns the body and, for suited rewards, its helmeted or helmetless head actor.</summary>
    public EndingRewardJump(EndingReward reward)
    {
        if (EndingRewardDefinitions.Rewards[reward].JumpHead is { } suitedHead)
        {
            CinematicActor<EndingRewardAnimation, CinematicSpritePose, EndingRewardJumpCue> spawned = Spawn(suitedHead.Actor);
            head = spawned;
            bodyCues = cue => HandleSuitedBodyCue(cue, suitedHead, place => spawned.ScreenPlace = place);
        }
        else
            bodyCues = HandleCue;
        body = Spawn(EndingRewardCast.JumpBody(reward));
    }

    /// <summary>Advances head before body, including shared gravity and cue effects.</summary>
    /// <returns>Whether the step's shooting cue asks for the ending's shot (#2562), which the scene's owner shows at once.</returns>
    public bool Step()
    {
        bool shoots = false;
        // The head steps before the body, as it runs ahead of it.
        // Both motions use the shared Samus velocity, so each update accelerates it.
        if (head is { IsActive: true })
        {
            if (headMotion == EndingRewardJumpMotion.Rising)
            {
                head.Position = Accelerated(head.Position);
                if (head.ScreenPlace.Y < EndingRewardJumpDefinitions.FallStartY) head.Delete();
            }
            head.Step(cue => shoots |= HandleCue(cue));
        }
        if (bodyMotion == EndingRewardJumpMotion.Rising)
        {
            body.Position = Accelerated(body.Position);
            if (BodyY < EndingRewardJumpDefinitions.FallStartY)
            {
                body.SetColouring(EndingShades.Drawn(FallingColouring));
                body.Start(FallingAnimation);
                bodyMotion = EndingRewardJumpMotion.Falling;
            }
        }
        else if (bodyMotion == EndingRewardJumpMotion.Falling)
        {
            body.Position = Accelerated(body.Position);
            if (BodyY >= EndingRewardJumpDefinitions.LandingY)
            {
                body.ScreenPlace = body.ScreenPlace.WithY(EndingRewardJumpDefinitions.LandingY);
                body.Start(LandingAnimation);
                bodyMotion = EndingRewardJumpMotion.Landed;
            }
        }
        body.Step(cue => shoots |= bodyCues(cue));
        return shoots;
    }

    /// <summary>
    /// Draws the active head and body among the screen's sprites: the jumping Samus's frames from
    /// <paramref name="jumpingArt"/>, and the falling Samus's, once she falls back, from <paramref name="fallingArt"/>.
    /// </summary>
    public void Draw(ScreenSprites destination, IIntroCinematicSpritePresentation jumpingArt, IIntroCinematicSpritePresentation fallingArt)
    {
        Ensure.NotNull(destination);
        head?.Draw(destination, installedArt: jumpingArt);
        body.Draw(destination, installedArt: FellBack ? fallingArt : jumpingArt);
    }

    /// <summary>Applies gravity to the shared velocity and returns where an actor at <paramref name="position"/> moves by it.</summary>
    private FixedVector<ScreenPixels> Accelerated(FixedVector<ScreenPixels> position)
    {
        Velocity += EndingRewardJumpDefinitions.Gravity;
        return position.WithY(position.Y + Velocity);
    }

    /// <summary>
    /// Handles launch, the flights and the shoot event the jump animations publish: whether
    /// <paramref name="cue"/> is the shoot event.
    /// </summary>
    private bool HandleCue(EndingRewardJumpCue cue) => CueReactions[cue]();

    /// <summary>What each cue of the jump animations does, and whether it is the shoot event.</summary>
    private IdentityTable<EndingRewardJumpCue, Func<bool>> CueReactions => field ??= new(new Dictionary<EndingRewardJumpCue, Func<bool>>
    {
        [EndingRewardJumpCue.Launch] = () =>
        {
            Velocity = EndingRewardJumpDefinitions.LaunchVelocity;
            return false;
        },
        // Only a suited jump's body raises these, which places the head it spawned (HandleSuitedBodyCue).
        [EndingRewardJumpCue.PrepareHead] = () => throw new InvalidOperationException($"Only a suited jump's body raises {EndingRewardJumpCue.PrepareHead}."),
        [EndingRewardJumpCue.LaunchHead] = () => throw new InvalidOperationException($"Only a suited jump's body raises {EndingRewardJumpCue.LaunchHead}."),
        [ShootCue] = () =>
        {
            body.SetColouring(EndingShades.Drawn(ShootingColouring));
            return true;
        },
        [EndingRewardJumpCue.StartBodyFlight] = () =>
        {
            bodyMotion = EndingRewardJumpMotion.Rising;
            return false;
        },
        [EndingRewardJumpCue.StartHeadFlight] = () =>
        {
            headMotion = EndingRewardJumpMotion.Rising;
            return false;
        },
    });

    /// <summary>
    /// Handles a cue a suited jump's body raises: its preparing and launching place the head it spawned with
    /// <paramref name="placeHead"/> at <paramref name="suited"/>'s places, and every other cue is one any jump raises.
    /// </summary>
    private bool HandleSuitedBodyCue(EndingRewardJumpCue cue, EndingRewardJumpHead suited, Action<Offset16<ScreenPixels>> placeHead)
    {
        if (cue == EndingRewardJumpCue.PrepareHead)
        {
            placeHead(suited.Readied);
            return false;
        }
        if (cue == EndingRewardJumpCue.LaunchHead)
        {
            placeHead(suited.Launched);
            return false;
        }
        return HandleCue(cue);
    }

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
