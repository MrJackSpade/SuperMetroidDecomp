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
    internal const CinematicSpriteSegment FallingAnimation = CinematicSpriteSegment.Falling;

    /// <summary>The animation Samus's body lands and shoots by.</summary>
    internal const CinematicSpriteSegment LandingAnimation = CinematicSpriteSegment.Landing;

    /// <summary>The colouring Samus's body falls and lands in.</summary>
    internal const EndingSpriteColouring FallingColouring = EndingSpriteColouring.FallingSamus;

    /// <summary>The colouring the landing animation's shoot cue turns Samus's body to, which it keeps to the end.</summary>
    internal const EndingSpriteColouring ShootingColouring = EndingSpriteColouring.ShootingSamus;

    /// <summary>The landing animation's cue that turns Samus's body to <see cref="ShootingColouring"/> and requests the shot.</summary>
    internal const CinematicSpriteCueKind ShootCue = CinematicSpriteCueKind.Shoot;

    /// <summary>Selects the armored head offsets used during the launch animation.</summary>
    private readonly bool helmeted;
    /// <summary>Body actor whose vertical position controls the fall and landing.</summary>
    private readonly CinematicActor body;
    /// <summary>Separate head actor present for the two suited ending rewards.</summary>
    private readonly CinematicActor? head;
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
        // Both motions use the shared Samus velocity, so each call accelerates it.
        if (head is { IsActive: true })
        {
            if (headFlying)
            {
                Move(head);
                if (head.ScreenPlace.Y < EndingRewardJumpDefinitions.FallStartY) head.Delete();
            }
            head.Step(HandleCue);
        }
        if (bodyMotion == EndingRewardBodyMotion.Rising)
        {
            Move(body);
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
            Move(body);
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

    /// <summary>Applies gravity to shared velocity and moves an actor by it.</summary>
    private void Move(CinematicActor actor)
    {
        Velocity += EndingRewardJumpDefinitions.Gravity;
        actor.Position = actor.Position.WithY(actor.Position.Y + Velocity);
    }

    /// <summary>Handles launch, head placement, the flights and the shoot event the jump animations publish.</summary>
    private void HandleCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.Launch:
                Velocity = EndingRewardJumpDefinitions.LaunchVelocity;
                return;
            case CinematicSpriteCueKind.PrepareHead:
                RequireHead().ScreenPlace = helmeted ? EndingRewardJumpDefinitions.HelmetedHeadReadied : EndingRewardJumpDefinitions.HelmetlessHeadReadied;
                return;
            case CinematicSpriteCueKind.LaunchHead:
                RequireHead().ScreenPlace = helmeted ? EndingRewardJumpDefinitions.HelmetedHeadLaunched : EndingRewardJumpDefinitions.HelmetlessHeadLaunched;
                return;
            case ShootCue:
                body.SetColouring(EndingShades.Drawn(ShootingColouring));
                ShotRequested = true;
                return;
            case CinematicSpriteCueKind.StartBodyFlight:
                bodyMotion = EndingRewardBodyMotion.Rising;
                return;
            case CinematicSpriteCueKind.StartHeadFlight:
                headFlying = true;
                return;
            case CinematicSpriteCueKind.StartMotherBrainCrossFade:
            case CinematicSpriteCueKind.DeleteEggAfterCrossFade:
            case CinematicSpriteCueKind.StartRepeatingExplosions:
            case CinematicSpriteCueKind.WaitForFlyaway:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionColours:
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
            case CinematicSpriteCueKind.SpawnStrikingRinkaPair:
            case CinematicSpriteCueKind.SpawnMissingRinkaPair:
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
                throw new InvalidOperationException($"The reward jump's animations publish no {cue} cue.");
        }
    }


    /// <summary>Requires a suited jump's head actor when a head-specific cue runs.</summary>
    private CinematicActor RequireHead() => head ?? throw new InvalidDataException("Suited jump requested a missing head actor.");
    /// <summary>Creates a jump actor from its catalogued initial position, colouring, and animations.</summary>
    private static CinematicActor Spawn(EndingRewardActor definition)
    {
        EndingRewardActorDefinition authored = EndingRewardActors.Get(definition);
        var actor = new CinematicActor(
            authored.Start.Position,
            EndingShades.Drawn(authored.Start.Colouring),
            authored.Animation);
        return actor;
    }
}
