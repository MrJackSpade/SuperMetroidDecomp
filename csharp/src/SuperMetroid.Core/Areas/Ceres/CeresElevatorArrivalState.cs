using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The two enemy projectiles spawned when a new game enters
/// Ceres: the moving pad under Samus and the stationary elevator platform behind it.
/// </summary>
/// <remarks>
/// These are not host-authored cutscene sprites. Construction consumes compiled copies of
/// the authored definitions; frame stepping executes their
/// animation scripts and frame behaviours. Keeping this tiny
/// pair separate avoids pretending that the larger general enemy-projectile engine has
/// already been modelled.
/// </remarks>
[RoomLifetime]
public sealed class CeresElevatorArrivalState
{
    /// <summary>First spawned projectile, which moves Samus before the platform checks landing.</summary>
    internal readonly CeresElevatorProjectile pad;
    /// <summary>Second spawned projectile, which waits behind the pad until touchdown.</summary>
    internal readonly CeresElevatorProjectile platform;

    /// <summary>Installs both compiled definitions and performs their initialization AIs.</summary>
    public CeresElevatorArrivalState(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Spawning a projectile with graphics first copies enemy slot zero's tile and palette choice. The shared
        // initializer then clears it, so both actors use room-loaded sprite tiles and the
        // palettes authored by their frames. Retaining the transient choice is
        // what previously made the level-data concealer teal until touchdown.
        pad = LoadProjectile(
            CeresElevatorArrivalDefinitions.MovingPad,
            MovingPadUnder(samus));
        pad.WaitTimer = CeresElevatorArrivalDefinitions.MovingPadWaitFrames;

        platform = LoadProjectile(
            CeresElevatorArrivalDefinitions.StationaryPlatform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>
    /// Runs the two freshly spawned projectiles in the projectile pass's order.
    /// </summary>
    /// <returns>True on and after the frame that hands Samus control back after the elevator descent.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Spawning takes the last free slot. The pad is spawned first into the last slot,
        // the platform second into the one before it, and the projectile pass runs from the
        // last slot down; therefore the pad must move Samus before the platform tests Y=72.
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
    /// Emits the low-priority frames selected by the live programs.
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

    /// <summary>The moving pad's position: Samus's whole position, <c>MovingPadYOffset</c> lower.</summary>
    private static Position16<RoomPixels> MovingPadUnder(SamusState samus) =>
        samus.Position.Whole + new Offset16<RoomPixels>(0, CoordinateAxis.AsSigned(CeresElevatorArrivalDefinitions.MovingPadYOffset));

    /// <summary>Creates one arrival actor from its compiled definition and room position.</summary>
    private static CeresElevatorProjectile LoadProjectile(
        CeresElevatorProjectileDefinition definition,
        Position16<RoomPixels> position)
    {
        return new CeresElevatorProjectile(
            definition.Animation,
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
        // The arrival animations play no counted phase; their repeat counter is never loaded.
        WrappingCountdown repeats = WrappingCountdown.Start(0);
        AnimationShown<CeresElevatorArrivalSegment, CeresElevatorArrivalPose> shown = CeresElevatorArrivalAnimations.Segments.PlayToNextFrame(
            animation, ref place, ref repeats,
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
            EnemySheetDefinitions.ProjectileFrameName(CeresElevatorArrivalAnimations.Frame(pose)), projectile.Position.ToScreen(camera),
            projectile.Graphics);
    }

    /// <summary>Small per-slot state for the two Ceres arrival enemy projectiles.</summary>
    internal sealed class CeresElevatorProjectile
    {
        /// <summary>Initializes a projectile's program, position, and graphics.</summary>
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
        /// <summary>Whether the projectile still occupies its arrival slot.</summary>
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
