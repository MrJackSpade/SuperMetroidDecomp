using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival a new game starts with: the moving pad that lowers Samus and the stationary platform behind
/// it, which hides the landing recess until she lands and both leave.
/// </summary>
[RoomLifetime]
public sealed class CeresElevatorArrivalState
{
    /// <summary>The moving pad, which lowers Samus before the platform checks for her landing.</summary>
    internal readonly CeresElevatorProjectile pad;
    /// <summary>The stationary platform, which waits behind the pad until Samus lands.</summary>
    internal readonly CeresElevatorProjectile platform;

    /// <summary>Starts the pad under Samus, waiting before it moves, and the platform at its fixed height below her.</summary>
    public CeresElevatorArrivalState(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Both actors show the shared sheet's pictures in the colours their frames author, with the concealer cleared to
        // black (CeresElevatorArrivalDefinitions.StartingGraphics).
        pad = LoadProjectile(
            CeresElevatorArrivalSegment.Pad,
            MovingPadUnder(samus));
        pad.WaitTimer = CeresElevatorArrivalDefinitions.MovingPadWaitFrames;

        platform = LoadProjectile(
            CeresElevatorArrivalSegment.Platform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both actors leave, once Samus has landed at Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>Runs the pad, then the platform.</summary>
    /// <returns>True on and after the frame that hands Samus control back after the elevator descent.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The pad moves Samus before the platform tests whether she has landed at Y=72, so both leave on the update
        // she lands.
        if (pad.Active)
        {
            if (pad.WaitTimer > 1)
            {
                pad.WaitTimer--;
            }
            else
            {
                if (pad.WaitTimer == 1)
                    pad.WaitTimer = 0;

                // The descent samples the old Samus position for the pad, then increments
                // Samus. That one-pixel separation is visible throughout the descent.
                pad.Position = pad.Position.WithY(MovingPadUnder(samus).Y);
                samus.Position += new Offset16<RoomPixels>(0, 1);
                if (CoordinateAxis.AsSigned(CoordinateAxis.Wrap(
                    samus.Position.Y.Whole - CeresElevatorArrivalDefinitions.LandingThresholdY)) >= 0)
                {
                    samus.Position = samus.Position.WithWholeY(CeresElevatorArrivalDefinitions.LandingSamusY);
                    pad.Start(CeresElevatorArrivalSegment.Delete);
                }
            }
            AdvanceAnimation(pad);
        }

        if (platform.Active)
        {
            if (samus.Position.Y.Whole == CeresElevatorArrivalDefinitions.LandingSamusY)
            {
                platform.Start(CeresElevatorArrivalSegment.Delete);
            }
            AdvanceAnimation(platform);
        }

        return IsComplete;
    }

    /// <summary>
    /// Emits the low-priority frames selected by the live animations.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="presentation">Publishes the platforms from the room's enemy artwork.</param>
    /// <param name="camera">Camera position in whole room pixels.</param>
    internal void Draw(SpriteFrameBuffer sprites, EnemyPresentation presentation, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(presentation);
        DrawProjectile(pad, 0, sprites, presentation, camera);
        DrawProjectile(platform, 1, sprites, presentation, camera);
    }

    /// <summary>The moving pad's position: Samus's whole position, <see cref="CeresElevatorArrivalDefinitions.MovingPadYOffset"/> lower.</summary>
    private static Position16<RoomPixels> MovingPadUnder(SamusState samus) =>
        samus.Position.Whole + new Offset16<RoomPixels>(0, CoordinateAxis.AsSigned(CeresElevatorArrivalDefinitions.MovingPadYOffset));

    /// <summary>Creates one arrival actor playing the animation of <paramref name="segment"/> at its room position.</summary>
    private static CeresElevatorProjectile LoadProjectile(
        CeresElevatorArrivalSegment segment,
        Position16<RoomPixels> position)
    {
        return new CeresElevatorProjectile(
            segment,
            position,
            CeresElevatorArrivalDefinitions.StartingGraphics);
    }

    /// <summary>Plays a projectile's typed animation (#641, #1600) when its frame expires: a new pose, or the delete.</summary>
    private static void AdvanceAnimation(CeresElevatorProjectile projectile)
    {
        if (--projectile.FrameHold != 0)
            return;

        Animation<CeresElevatorArrivalSegment> animation = projectile.Playback.Animation;
        AnimationPlace place = projectile.Playback.Place;
        AnimationShown<CeresElevatorArrivalSegment, CeresElevatorArrivalPose> shown = CeresElevatorArrivalAnimations.Segments.PlayToNextFrame(
            animation, ref place,
            cue => throw new InvalidOperationException($"The Ceres elevator arrival animations raise no event; got {cue}."));
        projectile.Playback = new AnimationPlayback<CeresElevatorArrivalSegment>(animation, place);
        switch (shown)
        {
            case AnimationShown<CeresElevatorArrivalSegment, CeresElevatorArrivalPose>.Frame frame:
                projectile.FrameHold = frame.Duration;
                projectile.Pose = frame.Shown;
                return;
            case AnimationShown<CeresElevatorArrivalSegment, CeresElevatorArrivalPose>.Ended:
                projectile.Active = false;
                return;
            default:
                throw new InvalidOperationException($"A Ceres elevator arrival animation ended an update with {shown}.");
        }
    }

    /// <summary>Draws an active projectile from the room's variant of its owner's pictures, wherever it lies; the display clips it (#1587).</summary>
    private static void DrawProjectile(
        CeresElevatorProjectile projectile,
        int instance,
        SpriteFrameBuffer sprites,
        EnemyPresentation presentation,
        Position16<RoomPixels> camera)
    {
        if (!projectile.Active || projectile.Pose is not { } pose)
            return;

        presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.CeresElevatorArrival, instance),
            EnemySheetDefinitions.ProjectileFrameName(CeresElevatorArrivalAnimations.Frame(pose)), projectile.Position.RelativeToCamera(camera),
            projectile.Graphics);
    }

    /// <summary>One of the two arrival actors: its animation, place and the pad's wait.</summary>
    internal sealed class CeresElevatorProjectile
    {
        /// <summary>Starts an actor's animation at its position, showing its graphics.</summary>
        public CeresElevatorProjectile(
            CeresElevatorArrivalSegment segment,
            Position16<RoomPixels> position,
            SpriteGraphics graphics)
        {
            Playback = new AnimationPlayback<CeresElevatorArrivalSegment>(CeresElevatorArrivalAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
            Position = position;
            Graphics = graphics;
        }

        /// <summary>The spawned-object sheet the projectile draws from and the colours it shows.</summary>
        public SpriteGraphics Graphics { get; }
        /// <summary>Whether the actor is still in the room; it leaves when its animation ends.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whole room position; the per-frame movement only rewrites Y.</summary>
        public Position16<RoomPixels> Position { get; set; }
        /// <summary>Pad-only startup delay before it begins moving Samus toward the platform.</summary>
        public int WaitTimer { get; set; }
        /// <summary>The projectile's animation and where it stands in it, played on when its frame expires.</summary>
        internal AnimationPlayback<CeresElevatorArrivalSegment> Playback { get; set; }

        /// <summary>Starts the animation of <paramref name="segment"/> on the next update.</summary>
        internal void Start(CeresElevatorArrivalSegment segment)
        {
            FrameHold = 1;
            Playback = new AnimationPlayback<CeresElevatorArrivalSegment>(CeresElevatorArrivalAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        }
        /// <summary>Frames until this projectile's next animation pass.</summary>
        public int FrameHold { get; set; } = 1;
        /// <summary>The pose the projectile shows, none before its first frame.</summary>
        internal CeresElevatorArrivalPose? Pose { get; set; }
    }
}
