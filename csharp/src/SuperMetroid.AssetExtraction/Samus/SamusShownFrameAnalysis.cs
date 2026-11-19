using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Decides, once, the animation frames Samus's body can show in each pose and the picture each of them draws (#1513,
/// #2048): the body sheet holds a whole-body picture for exactly the picture frames, and the installation holds every shown
/// frame with its picture frame (<see cref="SamusShownFrames"/>). A pose some path of the game gives Samus shows the frames
/// the paths that start her animation put it on and the frames the paths that set her animation frame from outside the
/// animation publish for it while she can hold it, each followed on through the animation as her animation advances it. A
/// pose no path gives her shows nothing.
/// </summary>
/// <remarks>
/// <para>
/// The animation holds a picture on each of its delays and on a hold, which keeps the animation on its own frame for a tick.
/// When a frame's delay runs out the animation moves to the next frame, which shows if it is a delay or a hold. A repeat,
/// a restart, the breathing loop, the wall-jump spin selection and the drained fall's handover move it on to the delay they
/// select; a selected frame that is no delay, or one past every animation, fails the game's animation there, so nothing
/// shows. A transition ends the pose: the pose update that follows the animation in the same update replaces it before the
/// draw. A frame on a value a cue carries reads the step the value selects, and a frame past the end of an animation
/// reads the animation laid out after it (#1535).
/// </para>
/// <para>
/// A pose's animation starts on the frames pose initialization and the Crystal Flash, X-ray, death, fast-fall,
/// drained-crouch, Zebes-start and grapple-swing paths start it on. A pose write that keeps the animation running moves the
/// frame to another pose of the same animation, so every pose of an animation starts on the frames any of them starts on. The
/// frames of an animation no start reaches, such as the second and third runs of the running animation and the knockback
/// animation's later runs, show nothing: the body's tables for the pose do not reach them.
/// </para>
/// <para>
/// The paths that publish frames outside the animation are of two kinds. The authored paths are made for the poses they
/// meet: the drained-Samus get-up, the Hyper Beam acquisition's freeze, the rainbow stop, the drained landing and
/// the release on the drained crouches, and the wall contact that rewinds a spin pose. The frames they reach, with the
/// frames the starts reach, are the pose's own animation, whose delays and holds are the pose's picture frames. The
/// unauthored paths meet poses they are not made for: the drained crouch, while she cannot stand, meeting the poses Samus takes after the
/// Baby Metroid's drain ends, the Baby's freeze and rainbow stop meeting a Samus the drain has let go of, and the wall
/// contact rewinding a drained pose that carries a spin's movement. The game draws their frames from the pose's tables
/// past its own pictures, which show other poses' halves or nothing. A frame written with a timer of one just before the
/// animation pass is advanced by it before the draw; any other written frame is drawn as it stands.
/// </para>
/// <para>
/// The analysis follows the frames by the place each takes in the game's layout of the pose's animation
/// (<see cref="SamusAnimationLayout"/>): it reads past animations, onto values cues carry and frames written for another
/// animation, which only that layout orders. What it publishes are the pose's named frames (#2048).
/// </para>
/// <para>
/// Every frame the body shows that is not one of the pose's picture frames, whether an authored path wrote it on an
/// cue's place or past the animation or an unauthored path wrote it, draws the pose's nearest picture frame at or
/// before it: the picture the pose's own animation last showed before that frame in its animation. This is a drawing
/// decision, authorised by the owner's ruling on #1513 that every Samus body frame is a whole Samus of its pose; the
/// animation frame itself, its timing and everything gameplay reads from it stay as the game computes them.
/// </para>
/// </remarks>
internal sealed class SamusShownFrameAnalysis
{
    /// <summary>
    /// The movement types Samus can be initialized in: the movement types of the poses some path gives her. The drained
    /// operations write a drained pose without initializing it, so her movement stays the type of whichever of these she was
    /// last initialized in, and nothing in those operations checks which (the Mother Brain glitch recording carries a spin
    /// jump's movement into the drained standing pose).
    /// </summary>
    private static readonly SamusMovementType[] InitializedMovementTypes =
        [.. Enum.GetValues<SamusPoseId>().Select(SamusPoseMetadata.ReadMovementType).Distinct().Order()];

    /// <summary>The game's layout of Samus's animations, which the analysis reads frames by.</summary>
    private readonly SamusAnimationLayout layout;

    /// <summary>The frames each pose can show, by their place in its animation's layout.</summary>
    private readonly Dictionary<SamusPoseId, SortedSet<int>> shown = [];

    /// <summary>
    /// The poses other than the drained crouches whose animation reaches the drained fall's handover, by reading into a
    /// drained crouch's animation, and so lands on the drained landing's frame when the fall ends (#1535).
    /// </summary>
    private readonly HashSet<SamusPoseId> fallingWithoutDrain = [];

    /// <summary>Each pose's picture frames, by their place in its animation's layout: the delays and holds its own playback shows.</summary>
    private readonly Dictionary<SamusPoseId, SortedSet<int>> pictured = [];

    /// <summary>The picture frame each shown frame draws, by pose and the frames' places in its animation's layout.</summary>
    private readonly Dictionary<SamusPoseId, SortedDictionary<int, int>> pictureFrames = [];

    /// <summary>
    /// Follows every pose's starts and authored paths through its animation to find its own playback and its picture
    /// frames, then the unauthored paths, until no further frame shows; and maps every shown frame to its picture frame.
    /// </summary>
    /// <exception cref="InvalidDataException">A shown frame has no picture frame of its pose at or before it.</exception>
    internal SamusShownFrameAnalysis(SamusAnimationLayout layout)
    {
        this.layout = Ensure.NotNull(layout);
        var pending = new Queue<PoseFrame>();
        foreach (SamusPoseId pose in Poses)
            shown.Add(pose, []);
        foreach (SamusPoseId pose in Poses)
        {
            foreach (int frame in Starts(pose))
                Show(pose, frame, pending);
        }
        var published = new HashSet<PosePublication>();
        Follow(pending, published, AuthoredPublications);
        foreach (SamusPoseId pose in Poses)
            pictured.Add(pose, [.. shown[pose].Where(frame => IsPicture(pose, frame))]);
        Follow(pending, published, pose => [.. AuthoredPublications(pose), .. UnauthoredPublications(pose)]);
        foreach (SamusPoseId pose in Poses)
            pictureFrames.Add(pose, new SortedDictionary<int, int>(shown[pose].ToDictionary(frame => frame, frame => NearestPicture(pose, frame))));
    }

    /// <summary>Every pose some path of the game gives Samus: the poses the body can show.</summary>
    private static IReadOnlyList<SamusPoseId> Poses { get; } = Enum.GetValues<SamusPoseId>();

    /// <summary>
    /// What the installation holds of the analysis, for every pose with a record: every frame the pose shows, with the
    /// picture frame it draws, each named in the pose's animation. A pose no path gives Samus shows none.
    /// </summary>
    internal SamusShownFrames Installed() => new(SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        IEnumerable<KeyValuePair<int, int>> frames = pictureFrames.TryGetValue(pose, out SortedDictionary<int, int>? shownFrames) ? shownFrames : [];
        return (IReadOnlyDictionary<SamusAnimationFrame, SamusAnimationFrame>)frames.ToDictionary(
            pair => SamusAnimationLayout.FrameAt(animation, pair.Key), pair => SamusAnimationLayout.FrameAt(animation, pair.Value));
    }));

    /// <summary>The place <paramref name="place"/> takes in the game's layout of its own animation, which it lands at in any other.</summary>
    private static int Number(SamusAnimationPlace place) => SamusAnimationLayout.Offset(place);

    /// <summary>The step <paramref name="pose"/>'s animation reads at <paramref name="frame"/> places into its layout.</summary>
    private SamusPoseAnimationStep? StepAt(SamusPoseId pose, int frame) => layout.Read(SamusAnimationDelays.AnimationForPose(pose), frame)?.Step;

    /// <summary>
    /// Follows every pending frame on through its animation, and applies <paramref name="publications"/> to every pose,
    /// until no further frame shows. A publication can depend on the frames a pose shows, which publications add to, so
    /// they are applied until none is new.
    /// </summary>
    private void Follow(Queue<PoseFrame> pending, HashSet<PosePublication> published,
        Func<SamusPoseId, List<SamusPublishedFrame>> publications)
    {
        do
        {
            while (pending.TryDequeue(out PoseFrame from))
            {
                foreach (int next in Next(from.Pose, from.Frame))
                    Show(from.Pose, next, pending);
            }
            foreach (SamusPoseId pose in Poses)
            {
                foreach (SamusPublishedFrame publication in publications(pose))
                {
                    if (published.Add(new PosePublication(pose, publication)))
                        Publish(pose, publication, pending);
                }
            }
        }
        while (pending.Count > 0);
    }

    /// <summary>Shows <paramref name="frame"/> of <paramref name="pose"/>, following it on through the animation when it is new.</summary>
    private void Show(SamusPoseId pose, int frame, Queue<PoseFrame> pending)
    {
        if (shown[pose].Add(frame))
            pending.Enqueue(new PoseFrame(pose, frame));
    }

    /// <summary>
    /// Shows a frame an outside path publishes: the frame itself when it is drawn as written, or the frames the animation
    /// advances it to first. Either starts the pose from whatever Samus showed before.
    /// </summary>
    private void Publish(SamusPoseId pose, SamusPublishedFrame publication, Queue<PoseFrame> pending)
    {
        int[] started = publication.Drawing == PublishedFrameDrawing.AfterAdvancing ? Next(pose, publication.Frame) : [publication.Frame];
        foreach (int frame in started)
            Show(pose, frame, pending);
    }

    /// <summary>
    /// The picture frame <paramref name="frame"/> of <paramref name="pose"/> draws: the pose's greatest picture frame at or
    /// before it.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose has no picture frame at or before the frame.</exception>
    private int NearestPicture(SamusPoseId pose, int frame)
    {
        SortedSet<int> pictures = pictured[pose];
        SortedSet<int> atOrBefore = pictures.GetViewBetween(int.MinValue, frame);
        return atOrBefore.Count > 0
            ? atOrBefore.Max
            : throw new InvalidDataException($"Samus pose {pose} shows animation frame {frame} before any picture its own animation shows.");
    }

    /// <summary>Whether <paramref name="frame"/> is a delay or a hold of <paramref name="pose"/>'s own animation.</summary>
    private static bool IsPicture(SamusPoseId pose, int frame) =>
        SamusAnimationLayout.OwnStepAt(SamusAnimationDelays.AnimationForPose(pose), frame) is SamusAnimationDelay or SamusAnimationHold;

    /// <summary>The frames the animation can show after <paramref name="frame"/> of <paramref name="pose"/> when its delay runs out.</summary>
    private int[] Next(SamusPoseId pose, int frame) => Reach(pose, frame + 1, FrameReach.Advancing);

    /// <summary>
    /// The frames the animation shows on reaching <paramref name="frame"/> of <paramref name="pose"/>: the frame when it is a
    /// delay, or a hold reached by advancing; the delays a cue there selects; or none where the animation fails or
    /// the pose ends. <paramref name="reach"/> says whether a cue selected the frame, which must then be a delay.
    /// </summary>
    /// <exception cref="InvalidDataException">An animation holds a step its playback cannot play.</exception>
    private int[] Reach(SamusPoseId pose, int frame, FrameReach reach)
    {
        SamusPoseAnimationStep? step = StepAt(pose, frame);
        if (step is SamusAnimationDelay)
            return [frame];
        if (reach == FrameReach.Selection || step is null)
            return [];
        return step switch
        {
            SamusAnimationHold => [frame],
            SamusAnimationRepeat repeat => Reach(pose, layout.TargetOffset(SamusAnimationDelays.AnimationForPose(pose), frame, repeat.Target), FrameReach.Selection),
            SamusAnimationRestart => Reach(pose, 0, FrameReach.Selection),
            SamusAnimationLoopUnlessLowEnergy => [.. Reach(pose, frame + 1, FrameReach.Selection), .. Reach(pose, 0, FrameReach.Selection)],
            SamusAnimationWallJumpSelect select =>
            [
                .. Reach(pose, layout.TargetOffset(SamusAnimationDelays.AnimationForPose(pose), frame, select.OrdinaryFrame), FrameReach.Selection),
                .. Reach(pose, layout.TargetOffset(SamusAnimationDelays.AnimationForPose(pose), frame, select.SpaceJumpFrame), FrameReach.Selection),
                .. Reach(pose, layout.TargetOffset(SamusAnimationDelays.AnimationForPose(pose), frame, select.ScrewAttackFrame), FrameReach.Selection),
            ],
            // The drained fall's handover installs the fall whatever the pose, then moves on to the next frame (#1535).
            SamusAnimationInstallDrainedFall => InstallDrainedFall(pose, frame),
            SamusAnimationTransition or SamusAnimationTurnTransition or SamusAnimationItemTransition or
                SamusAnimationItemAirborneTransition or SamusAnimationVerticalSpeedTransition => [],
            _ => throw new InvalidDataException($"Samus animation step {step} has no playback."),
        };
    }

    /// <summary>
    /// The frames the drained fall's handover at <paramref name="frame"/> of <paramref name="pose"/> moves on to, noting a
    /// pose other than the drained crouches that reaches it.
    /// </summary>
    private int[] InstallDrainedFall(SamusPoseId pose, int frame)
    {
        if (pose is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose))
            fallingWithoutDrain.Add(pose);
        return Reach(pose, frame + 1, FrameReach.Selection);
    }

    /// <summary>
    /// The frames the paths made for <paramref name="pose"/> publish for it, given the frames it shows so far, each as
    /// <see cref="SamusDrainedState"/> or the spin's wall contact writes it. Their frames are the pose's own animation; a
    /// frame they write on a cue's place or past the animation draws the pose's nearest picture frame before it.
    /// </summary>
    private List<SamusPublishedFrame> AuthoredPublications(SamusPoseId pose)
    {
        SortedSet<int> frames = shown[pose];
        var publications = new List<SamusPublishedFrame>();
        // Once she can stand, the drained get-up moves the left-facing drained crouch to its stand-up on Up, just before the animation.
        if (pose == SamusPoseId.DrainedCrouchingLeftPose && frames.Max >= Number(SamusDrainedAnimationDefinitions.AbleToStandFirstFrame))
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.AbleToStandFrame), PublishedFrameDrawing.AfterAdvancing));
        // While she cannot stand, the drained get-up starts with the left-facing drained crouch and moves it, on Up, to its failed
        // attempt to stand, just before the animation.
        if (pose == SamusPoseId.DrainedCrouchingLeftPose && ShowsUnableToStandFrame(frames))
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.UnableToStandFrame), PublishedFrameDrawing.AfterAdvancing));
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
        {
            // The Baby Metroid's Hyper Beam freeze and rainbow stop write the frame during enemy AI, after the animation, so
            // the draw shows it as written. The Baby puts Samus crouching at the ceiling before either. The left-facing
            // crouch's Hyper Beam frame, and both crouches' rainbow-stop frame, land on a cue's place, and the
            // right-facing crouch's Hyper Beam frame past its animation: the game's tables hold no composition there, so the
            // body vanishes; the model draws the crouch's picture before it.
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame), PublishedFrameDrawing.AsWritten));
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.RainbowStopFrame), PublishedFrameDrawing.AsWritten));
            // The drained fall lands on its landing frame for eight ticks; the release moves the crouch on during enemy AI.
            // The release follows the Tourian Metroid's drain, whose fall leaves Samus crouching, so it never meets a
            // drained standing pose. The right-facing crouch's landing frame is a cue's place, drawn as the
            // crouch's picture before it, the same picture the game's tables hold there.
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.LandingFrame), PublishedFrameDrawing.AsWritten));
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame), PublishedFrameDrawing.AsWritten));
        }
        // Wall contact rewinds a spin pose, during movement and with a timer of one, into the animation's frames.
        if (SamusPoseMetadata.ReadMovementType(pose) == SamusMovementType.SpinJumping)
            publications.Add(new(WallContactRewindFrame(pose), PublishedFrameDrawing.AfterAdvancing));
        return publications;
    }

    /// <summary>
    /// The frames paths not made for <paramref name="pose"/> publish for it, given the frames it shows so far. The game
    /// draws them from the pose's tables past its own pictures: scattered halves of other poses, a Screw Attack's outline
    /// without Samus, or nothing. Each draws the pose's nearest picture frame at or before it, the picture its own
    /// animation last showed.
    /// </summary>
    private List<SamusPublishedFrame> UnauthoredPublications(SamusPoseId pose)
    {
        var publications = new List<SamusPublishedFrame>();
        // The drained get-up for when she cannot stand runs until Mother Brain dies and checks only the frame, so on Up it moves
        // whatever pose shows one of its frames, just before the animation: every pose Samus takes in the fight that follows.
        if (pose != SamusPoseId.DrainedCrouchingLeftPose && UnableToStandMeets(pose) && ShowsUnableToStandFrame(shown[pose]))
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.UnableToStandFrame), PublishedFrameDrawing.AfterAdvancing));
        // The Baby's Hyper Beam freeze and rainbow stop write the frame without checking the pose, so a Samus the drain has
        // let go of takes them in whatever pose she holds, drawn as written.
        if (BabyFreezeAndStopMeetFree(pose))
        {
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame), PublishedFrameDrawing.AsWritten));
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.RainbowStopFrame), PublishedFrameDrawing.AsWritten));
        }
        // The drained fall a pose's animation installed by reading into a drained crouch's animation lands it on the
        // landing frame, during movement and with the landing's timer, so the draw shows it as written.
        if (fallingWithoutDrain.Contains(pose))
            publications.Add(new(Number(SamusDrainedAnimationDefinitions.LandingFrame), PublishedFrameDrawing.AsWritten));
        // Wall contact rewinds whatever pose holds the spin's movement: a drained pose written over a spin keeps it.
        if (SamusPoseMetadata.ReadMovementType(pose) != SamusMovementType.SpinJumping &&
            MovementTypesHeldIn(pose).Contains(SamusMovementType.SpinJumping))
            publications.Add(new(WallContactRewindFrame(pose), PublishedFrameDrawing.AfterAdvancing));
        return publications;
    }

    /// <summary>Whether <paramref name="frames"/> holds a frame from which the drained crouch accepts Up while she cannot stand.</summary>
    private static bool ShowsUnableToStandFrame(SortedSet<int> frames) =>
        frames.GetViewBetween(Number(SamusDrainedAnimationDefinitions.UnableToStandFirstFrame),
            Number(SamusDrainedAnimationDefinitions.UnableToStandLastFrame)).Count > 0;

    /// <summary>The frame the wall contact rewinds <paramref name="pose"/> to: the Screw Attack's longer run, or the spin's.</summary>
    private static int WallContactRewindFrame(SamusPoseId pose) =>
        Number(SamusPoseMetadata.IsScrewAttackPose(pose)
            ? SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame
            : SamusWallContactAnimationDefinitions.SpinJumpRewindFrame);

    /// <summary>
    /// The movement types Samus's movement can be in while she holds <paramref name="pose"/>: the pose's own, except for the
    /// drained poses, which the drained controllers write without initializing the pose, so her movement stays that of
    /// whichever pose she was last initialized in (<see cref="InitializedMovementTypes"/>).
    /// </summary>
    private static SamusMovementType[] MovementTypesHeldIn(SamusPoseId pose) =>
        SamusPoseMetadata.IsDrainedPose(pose) ? InitializedMovementTypes : [SamusPoseMetadata.ReadMovementType(pose)];

    /// <summary>
    /// Whether the drained get-up for when she cannot stand can meet <paramref name="pose"/>. Mother Brain installs it only with her rainbow
    /// beam, which turns Samus to the left-facing knockback and lets her fall into the left-facing drained crouch that she
    /// is held in until the Baby Metroid stands her up and puts her crouching again, both facing left; so the right-facing
    /// drained poses never meet it. The fall initializes the crouch, so the standing pose the Baby writes keeps the
    /// crouch's own movement: no wall contact rewinds it into the get-up's frames. Once the Baby's drain ends, Samus
    /// takes her ordinary poses for the rest of the fight, each of which the get-up can meet.
    /// </summary>
    private static bool UnableToStandMeets(SamusPoseId pose) =>
        pose is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedStandingRightPose or
            SamusPoseId.DrainedStandingLeftPose);

    /// <summary>
    /// Whether the Baby's Hyper Beam freeze and rainbow stop can meet <paramref name="pose"/> on a Samus the drain has let go
    /// of: any pose but the drained crouches, which the freeze and stop are made for, and the death poses, since enemy AI,
    /// which runs the Baby's freeze and stop, stops while Samus dies.
    /// </summary>
    private static bool BabyFreezeAndStopMeetFree(SamusPoseId pose) =>
        pose is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
            SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose);

    /// <summary>
    /// The picture frames <paramref name="pose"/>'s animation starts on: the frames any pose animating by its animation starts
    /// on, since a pose write that keeps the animation running moves the frame between them.
    /// </summary>
    private static int[] Starts(SamusPoseId pose)
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        return [.. Poses.Where(other => ReferenceEquals(SamusAnimationDelays.AnimationForPose(other), animation))
            .SelectMany(InitialFrames).Distinct().Order().Where(frame => IsPicture(pose, frame))];
    }

    /// <summary>
    /// The frames a pose can be started on from whatever Samus showed before: the frames pose initialization starts it on,
    /// and the frames the Crystal Flash, X-ray, death, fast-fall, drained-crouch, Zebes-start and grapple-swing paths start
    /// it on.
    /// </summary>
    private static IEnumerable<int> InitialFrames(SamusPoseId pose)
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        bool dying = pose is SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose;
        // Pose initialization starts a pose on its first frame; the death setup overwrites that frame before any draw.
        if (!dying)
            yield return SamusAnimationLayout.Offset(animation, animation.Start);
        // A spin jump starts on its second frame; a standing pose aiming up entered from one keeps the gun raised on it.
        if (SamusPoseMetadata.IsSpinJumpPose(pose) ||
            (SamusPoseMetadata.ReadMovementType(pose) == SamusMovementType.Standing &&
             SamusPoseMetadata.ReadAim(pose).AimsStraightUp))
            yield return SamusAnimationLayout.Offset(animation, animation.AfterStart);
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
        {
            yield return Number(SamusDrainedAnimationDefinitions.FallInitialFrame);
            yield return Number(SamusDrainedAnimationDefinitions.PutCrouchingFrame);
        }
        if (pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose)
            yield return Number(SamusAnimationFrames.FastFallStart);
        // The death pose starts on the frame the movement Samus dies in selects.
        if (dying)
        {
            foreach (SamusMovementType movementType in InitializedMovementTypes)
                yield return SamusAnimationLayout.Offset(animation, SamusDeathSequenceFrames.InitialFrame(movementType));
        }
        // The angle, the Crystal Flash's stages and the Zebes start select these poses' frames directly.
        if (pose is SamusPoseId.XrayingStandingRightPose or SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or SamusPoseId.XrayingCrouchingLeftPose or
            SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose or
            SamusPoseId.CrystalFlashRightPose or SamusPoseId.CrystalFlashLeftPose or
            SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose)
        {
            for (int frame = 0; frame < SamusAnimationLayout.StepCount(animation); frame++)
                yield return frame;
        }
    }

    /// <summary>A frame of a pose, by its place in the layout of the pose's animation.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="Frame">The frame's place in the layout of the pose's animation.</param>
    private readonly record struct PoseFrame(SamusPoseId Pose, int Frame);

    /// <summary>A frame an outside path publishes, with the pose it publishes it for.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="Publication">The frame published.</param>
    private readonly record struct PosePublication(SamusPoseId Pose, SamusPublishedFrame Publication);

    /// <summary>A frame an outside path publishes for a pose.</summary>
    /// <param name="Frame">The animation frame written, by its place in the pose's animation's layout.</param>
    /// <param name="Drawing">Whether the draw shows the frame as written or after the animation pass advances it.</param>
    private readonly record struct SamusPublishedFrame(int Frame, PublishedFrameDrawing Drawing);

    /// <summary>How the draw shows a frame an outside path publishes.</summary>
    private enum PublishedFrameDrawing
    {
        /// <summary>The frame is drawn as written: the path writes it after the animation pass.</summary>
        AsWritten,

        /// <summary>
        /// The animation pass advances the frame before the draw, as it does a frame written with a timer of one just before
        /// it.
        /// </summary>
        AfterAdvancing,
    }

    /// <summary>How the animation reaches a frame.</summary>
    private enum FrameReach
    {
        /// <summary>A delay ran out and the animation moved on to the next frame.</summary>
        Advancing,

        /// <summary>A cue selected the frame, which must be a delay.</summary>
        Selection,
    }
}
