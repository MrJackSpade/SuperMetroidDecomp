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
/// already been translated.
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

        // SpawnEprojWithGfx first copies enemy slot zero's tile/palette word. The shared
        // initializer then clears it, so both actors use room-loaded sprite tiles and the
        // palettes authored by their spritemaps. Retaining the transient word is
        // what previously made the level-data concealer teal until touchdown.
        pad = LoadProjectile(
            CeresElevatorArrivalDefinitions.MovingPad,
            MovingPadUnder(samus));
        pad.WaitTimer = WrappingCountdown.Start(CeresElevatorArrivalDefinitions.MovingPadWaitFrames);

        platform = LoadProjectile(
            CeresElevatorArrivalDefinitions.StationaryPlatform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both projectiles delete themselves when Samus reaches Y=72.</summary>
    public bool IsComplete => !pad.Active && !platform.Active;

    /// <summary>
    /// Runs the two freshly spawned projectiles in the projectile pass's order.
    /// </summary>
    /// <returns>True on and after the frame that calls Samus command <c>$0E</c>.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Spawning takes the last free slot. The pad is spawned first into the last slot,
        // the platform second into the one before it, and the projectile pass runs from the
        // last slot down; therefore the pad must move Samus before the platform tests Y=72.
        if (pad.Active)
        {
            if (pad.WaitTimer.IsAbove(1))
            {
                pad.WaitTimer = pad.WaitTimer.Decrement().Remaining;
            }
            else
            {
                if (pad.WaitTimer.IsAt(1))
                    pad.WaitTimer = WrappingCountdown.Start(0);

                // The descent samples the old Samus position for the pad, then increments
                // Samus. That one-pixel separation is visible throughout the descent.
                pad.Position = pad.Position.WithY(MovingPadUnder(samus).Y);
                samus.Position += new Offset16<RoomPixels>(0, 1);
                if (CoordinateAxis.AsSigned(CoordinateAxis.Wrap(
                    samus.Position.Y.Whole - CeresElevatorArrivalDefinitions.LandingThresholdY)) >= 0)
                {
                    samus.Position = samus.Position.WithWholeY(CeresElevatorArrivalDefinitions.LandingSamusY);
                    pad.Start(CeresElevatorArrivalProgram.Delete);
                }
            }
            ProcessAnimationScript(pad);
        }

        if (platform.Active)
        {
            if (samus.Position.Y.Whole == CeresElevatorArrivalDefinitions.LandingSamusY)
            {
                platform.Start(CeresElevatorArrivalProgram.Delete);
            }
            ProcessAnimationScript(platform);
        }

        return IsComplete;
    }

    /// <summary>
    /// Emits the low-priority spritemaps selected by the live instruction lists.
    /// </summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="presentation">Publishes the platforms from the loaded graphics set's sheets.</param>
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
            definition.Program,
            position,
            CeresElevatorArrivalDefinitions.StartingGraphics);
    }

    /// <summary>Runs a projectile's typed program (#641) when its frame expires: a new pose, or the delete.</summary>
    private static void ProcessAnimationScript(CeresElevatorProjectile projectile)
    {
        CountdownStep frameStep = projectile.FrameHold.Decrement();
        projectile.FrameHold = frameStep.Remaining;
        if (!frameStep.IsZero)
            return;

        AnimationProgramCursor<CeresElevatorArrivalProgram> cursor = projectile.Program;
        // The arrival lists use no counted loops; their counter is never loaded.
        WrappingCountdown counter = WrappingCountdown.Start(0);
        AnimationProgramAdvance<CeresElevatorArrivalPose> advance = CeresElevatorArrivalAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, _) => throw new InvalidOperationException($"The Ceres elevator arrival programs publish no cue; got {cue}."));
        projectile.Program = cursor;
        switch (advance)
        {
            case AnimationProgramAdvance<CeresElevatorArrivalPose>.Show shown:
                projectile.FrameHold = WrappingCountdown.FromRaw(shown.Duration);
                projectile.Pose = shown.Frame;
                return;
            case AnimationProgramAdvance<CeresElevatorArrivalPose>.Stop:
                projectile.Active = false;
                return;
            default:
                throw new InvalidOperationException($"A Ceres elevator arrival program ended an update with {advance}.");
        }
    }

    /// <summary>Draws an active on-screen projectile from its graphics set's sheet.</summary>
    private static void DrawProjectile(
        CeresElevatorProjectile projectile,
        int instance,
        SpriteFrameBuffer sprites,
        EnemyPresentation presentation,
        Position16<RoomPixels> camera)
    {
        if (!projectile.Active || projectile.Pose is not { } pose)
            return;

        Position16<ScreenPixels> screen = projectile.Position.ToScreen(camera);
        // Only origins within 128 pixels of the screen on each axis draw: X + 128 and Y + 128 below 512.
        if (screen.X + 128 >= 512 || screen.Y + 128 >= 512)
        {
            return;
        }

        presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.CeresElevatorArrival, instance),
            EnemySheetDefinitions.ProjectileFrameName(CeresElevatorArrivalAnimations.Frame(pose)), screen,
            projectile.Graphics);
    }

    /// <summary>Small per-slot state for the two Ceres arrival enemy projectiles.</summary>
    internal sealed class CeresElevatorProjectile
    {
        /// <summary>Initializes a projectile's program, position, and graphics.</summary>
        public CeresElevatorProjectile(
            CeresElevatorArrivalProgram program,
            Position16<RoomPixels> position,
            SpriteGraphics graphics)
        {
            Program = AnimationProgramCursor<CeresElevatorArrivalProgram>.Start(program);
            Position = position;
            Graphics = graphics;
        }

        /// <summary>The spawned-object sheet the projectile draws from and the colours it shows.</summary>
        public SpriteGraphics Graphics { get; }
        /// <summary>Whether the projectile still occupies its arrival slot.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whole room position; the pre-instruction only rewrites Y.</summary>
        public Position16<RoomPixels> Position { get; set; }
        /// <summary>Pad-only startup delay before it begins moving Samus toward the platform.</summary>
        public WrappingCountdown WaitTimer { get; set; }
        /// <summary>Where the projectile's typed program resumes when its frame expires.</summary>
        internal AnimationProgramCursor<CeresElevatorArrivalProgram> Program { get; set; }

        /// <summary>Starts <paramref name="program"/> on the next update.</summary>
        internal void Start(CeresElevatorArrivalProgram program)
        {
            FrameHold = WrappingCountdown.Start(1);
            Program = AnimationProgramCursor<CeresElevatorArrivalProgram>.Start(program);
        }
        /// <summary>Frames until this projectile's next instruction pass.</summary>
        public WrappingCountdown FrameHold { get; set; } = WrappingCountdown.Start(1);
        /// <summary>The pose the projectile shows, none before its first frame.</summary>
        internal CeresElevatorArrivalPose? Pose { get; set; }
    }
}
