using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres elevator arrival a new game starts with: the moving pad that lowers Samus and the stationary platform behind
/// it, which hides the landing recess until she lands and both leave.
/// </summary>
[RoomLifetime]
public sealed class CeresElevatorArrivalState
{
    /// <summary>The moving pad, which lowers Samus before the platform checks for her landing, and its wait before it moves.</summary>
    private readonly CeresElevatorPad pad;
    /// <summary>The stationary platform, which waits behind the pad until Samus lands.</summary>
    private readonly CeresElevatorArrivalActor platform;

    /// <summary>Starts the pad under Samus, waiting before it moves, and the platform at its fixed height below her.</summary>
    public CeresElevatorArrivalState(SamusState samus)
    {
        Ensure.NotNull(samus);

        // Both actors show the shared sheet's pictures in the colours their frames author, with the concealer cleared to
        // black (CeresElevatorArrivalDefinitions.StartingGraphics).
        pad = new CeresElevatorPad(CreateActor(
            CeresElevatorArrivalSegment.Pad,
            MovingPadUnder(samus)));

        platform = CreateActor(
            CeresElevatorArrivalSegment.Platform,
            samus.Position.Whole.WithY(CeresElevatorArrivalDefinitions.StationaryPlatformY));
    }

    /// <summary>True after both actors leave, once Samus has landed at Y=72.</summary>
    public bool IsComplete => !pad.Actor.Active && !platform.Active;

    /// <summary>Runs the pad, then the platform.</summary>
    /// <returns>True on and after the frame that hands Samus control back after the elevator descent.</returns>
    public bool Step(SamusState samus)
    {
        Ensure.NotNull(samus);

        // The pad moves Samus before the platform tests whether she has landed at Y=72, so both leave on the update
        // she lands, without showing another pose (#2424).
        if (pad.Actor.Active)
        {
            if (LowerSamus(samus))
                pad.Actor.Active = false;
            else
                pad.Actor.Animation.Step();
        }

        if (platform.Active)
        {
            if (samus.Position.Y.Whole == CeresElevatorArrivalDefinitions.LandingSamusY)
                platform.Active = false;
            else
                platform.Animation.Step();
        }

        return IsComplete;
    }

    /// <summary>
    /// Runs one update of the pad's descent: it waits, then lowers Samus a pixel an update, riding under her, until she
    /// lands.
    /// </summary>
    /// <returns>True on the update Samus lands.</returns>
    private bool LowerSamus(SamusState samus)
    {
        if (pad.WaitOneUpdate())
            return false;
        // The descent samples the old Samus position for the pad, then increments
        // Samus. That one-pixel separation is visible throughout the descent.
        pad.Actor.Position = pad.Actor.Position.WithY(MovingPadUnder(samus).Y);
        samus.Position += CeresElevatorArrivalDefinitions.SamusDescentStep;
        // Samus starts the descent at the room's top and moves down a pixel an update, so she reaches the landing
        // threshold from above.
        if (samus.Position.Y.Whole < CeresElevatorArrivalDefinitions.LandingThresholdY)
            return false;
        samus.Position = samus.Position.WithWholeY(CeresElevatorArrivalDefinitions.LandingSamusY);
        return true;
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
        DrawActor(pad.Actor, sprites, presentation, camera);
        DrawActor(platform, sprites, presentation, camera);
    }

    /// <summary>The moving pad's position: Samus's whole position, <see cref="CeresElevatorArrivalDefinitions.MovingPadYOffset"/> lower.</summary>
    private static Position16<RoomPixels> MovingPadUnder(SamusState samus) =>
        samus.Position.Whole + new Offset16<RoomPixels>(0, CeresElevatorArrivalDefinitions.MovingPadYOffset);

    /// <summary>Creates one arrival actor playing the animation of <paramref name="segment"/> at its room position.</summary>
    private static CeresElevatorArrivalActor CreateActor(
        CeresElevatorArrivalSegment segment,
        Position16<RoomPixels> position)
    {
        return new CeresElevatorArrivalActor(
            segment,
            position,
            CeresElevatorArrivalDefinitions.StartingGraphics);
    }

    /// <summary>
    /// Draws an actor in the room while it stays, its pose's own picture of the room's shared spawned-object sheet (#2425),
    /// wherever it lies; the display clips it (#1587).
    /// </summary>
    private static void DrawActor(
        CeresElevatorArrivalActor actor,
        SpriteFrameBuffer sprites,
        EnemyPresentation presentation,
        Position16<RoomPixels> camera)
    {
        if (!actor.Active || actor.Pose is not { } pose)
            return;

        presentation.PublishSpawnedFrame(sprites, new SpriteSource(SpriteSourceKind.CeresElevatorArrival),
            CeresElevatorArrivalPictures.SheetName(pose), actor.Position.RelativeToCamera(camera), actor.Graphics);
    }

    /// <summary>
    /// The moving pad: its actor and the wait before it first moves Samus, which only the pad has (#2426).
    /// </summary>
    /// <param name="actor">The pad's actor.</param>
    internal sealed class CeresElevatorPad(CeresElevatorArrivalActor actor)
    {
        /// <summary>The pad's actor: its animation and place.</summary>
        public CeresElevatorArrivalActor Actor { get; } = Ensure.NotNull(actor);

        /// <summary>
        /// The pad's wait, over on the update it first moves Samus; once it is over the pad moves her on every update.
        /// </summary>
        private readonly UpdateCountdown wait = UpdateCountdown.Lasting(CeresElevatorArrivalDefinitions.MovingPadWaitUpdates);

        /// <summary>Counts one update of the pad's wait.</summary>
        /// <returns>Whether the pad still waits on this update: false on the update its wait runs out and on every one after.</returns>
        internal bool WaitOneUpdate() => !wait.Step();
    }

    /// <summary>One of the two arrival actors, the pad or the platform: its animation and place.</summary>
    internal sealed class CeresElevatorArrivalActor
    {
        /// <summary>Starts an actor's animation at its position, showing its graphics: its first update shows its first pose.</summary>
        public CeresElevatorArrivalActor(
            CeresElevatorArrivalSegment segment,
            Position16<RoomPixels> position,
            SpriteGraphics graphics)
        {
            Animation = AnimationPlayer<CeresElevatorArrivalSegment, CeresElevatorArrivalPose, NoAnimationCue>.Starting(
                CeresElevatorArrivalAnimations.Segments, CeresElevatorArrivalAnimations.StartingWith(segment));
            Position = position;
            Graphics = graphics;
        }

        /// <summary>The spawned-object sheet the actor draws from and the colours it shows.</summary>
        public SpriteGraphics Graphics { get; }
        /// <summary>Whether the actor is still in the room; it leaves on the update Samus lands.</summary>
        public bool Active { get; set; } = true;
        /// <summary>Whole room position; the per-frame movement only rewrites Y.</summary>
        public Position16<RoomPixels> Position { get; set; }
        /// <summary>
        /// The actor's typed animation (#641, #1600) on the shared player (#2480): a new pose when the shown one's updates
        /// run out, or a pose shown for good, which holds until the actor leaves.
        /// </summary>
        internal AnimationPlayer<CeresElevatorArrivalSegment, CeresElevatorArrivalPose, NoAnimationCue> Animation { get; }

        /// <summary>The pose the actor shows, none before its first update.</summary>
        internal CeresElevatorArrivalPose? Pose => Animation.Shown;
    }
}
