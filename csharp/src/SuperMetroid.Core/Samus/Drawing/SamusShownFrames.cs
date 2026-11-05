using System.Runtime.CompilerServices;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// The animation frames Samus's body can show in each pose (#1513): the body sheet holds pictures for exactly these, and
/// the draw rejects any other. A pose shows the frames its animation program holds a picture on, and the frames the
/// paths that set her animation frame from outside the program publish for it, each followed on through the program as
/// her animation advances it.
/// </summary>
/// <remarks>
/// <para>
/// The program holds a picture on each of its delays and on a hold, which keeps the animation on its own frame for a tick.
/// When a frame's delay runs out the animation moves to the next frame, which shows if it is a delay or a hold. A repeat,
/// a restart, the breathing loop, the wall-jump spin selection and the drained fall's handover move it on to the delay they
/// select; a selected frame that is no delay, a frame between two runs, or one past every program fails the game's
/// animation there, so nothing shows. A transition ends the pose: the pose update that follows the animation in the same
/// update replaces it before the draw. A frame past the end of a program reads the program laid out after it.
/// </para>
/// <para>
/// The paths that publish frames outside the program: the drained-Samus get-up handlers, the Hyper Beam acquisition's
/// freeze, the rainbow stop, the drained landing and the release; and the wall contact that rewinds a spin, for every pose
/// drawn with the spin's movement. A frame written with a timer of one just before the animation pass is advanced by it
/// before the draw; any other written frame is drawn as it stands. Pose initialization starts a pose only on one of its
/// delays, and the Crystal Flash, X-ray, death, fast-fall, standing-shot, Zebes-start, grapple-swing and Speed Booster
/// paths publish only delays of their poses' programs; they add no frame, but a frame they start a pose on follows a
/// frame of whatever Samus showed before.
/// </para>
/// </remarks>
internal sealed class SamusShownFrames
{
    /// <summary>The frames computed for each continuation an animation reads past its programs with.</summary>
    private static readonly ConditionalWeakTable<LaidOutProgramContinuation<SamusPoseId>, SamusShownFrames> computed = new();

    /// <summary>What a frame past a program's end reads.</summary>
    private readonly LaidOutProgramContinuation<SamusPoseId> continuation;

    /// <summary>The frames each pose can show.</summary>
    private readonly Dictionary<SamusPoseId, SortedSet<int>> shown = [];

    /// <summary>
    /// The frames each pose can be started or published on, which follow a frame of whatever pose Samus showed before
    /// rather than one of the pose's own.
    /// </summary>
    private readonly Dictionary<SamusPoseId, HashSet<int>> entries = [];

    /// <summary>The frames of its own pose each shown frame can follow as the animation advances.</summary>
    private readonly Dictionary<SamusPoseFrame, HashSet<int>> predecessors = [];

    /// <summary>Follows every pose's program and every outside path through it until no further frame shows.</summary>
    private SamusShownFrames(LaidOutProgramContinuation<SamusPoseId> continuation)
    {
        this.continuation = continuation;
        var pending = new Queue<SamusPoseFrame>();
        foreach (SamusPoseId pose in Poses)
        {
            shown.Add(pose, []);
            entries.Add(pose, [.. InitialFrames(pose)]);
        }
        foreach (SamusPoseId pose in Poses)
        {
            SamusAnimationProgram program = SamusAnimationDelays.ProgramForPose(pose);
            for (int frame = 0; frame < program.FrameCount; frame++)
            {
                if (program.TryStepAt(frame) is SamusAnimationDelay or SamusAnimationHold)
                    Show(pose, frame, pending);
            }
        }
        // A publication can depend on the frames a pose shows, which publications add to, so they are applied until none
        // is new.
        var published = new HashSet<PosePublication>();
        do
        {
            while (pending.TryDequeue(out SamusPoseFrame from))
            {
                foreach (int next in Next(from.Pose, from.Frame))
                {
                    PredecessorsOf(from.Pose, next).Add(from.Frame);
                    Show(from.Pose, next, pending);
                }
            }
            foreach (SamusPoseId pose in Poses)
            {
                foreach (SamusPublishedFrame publication in Publications(pose))
                {
                    if (published.Add(new PosePublication(pose, publication)))
                        Publish(pose, publication, pending);
                }
            }
        }
        while (pending.Count > 0);
    }

    /// <summary>The frames Samus's body can show, for the programs <paramref name="continuation"/> reads past.</summary>
    internal static SamusShownFrames Of(LaidOutProgramContinuation<SamusPoseId> continuation) =>
        computed.GetValue(Ensure.NotNull(continuation), static key => new SamusShownFrames(key));

    /// <summary>Every pose with a record, in the game's numbering.</summary>
    private static IEnumerable<SamusPoseId> Poses =>
        Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).Select(SamusPoseNumbering.FromNumber);

    /// <summary>Whether <paramref name="pose"/> can show animation frame <paramref name="frame"/>.</summary>
    internal bool Shows(SamusPoseId pose, int frame) => shown.TryGetValue(pose, out SortedSet<int>? frames) && frames.Contains(frame);

    /// <summary>
    /// Requires the body to be able to show <paramref name="pose"/> at <paramref name="frame"/> while its movement is
    /// <paramref name="movementType"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">No path of the game shows that body.</exception>
    internal void RequireShown(SamusMovementType movementType, SamusPoseId pose, int frame)
    {
        if (!Shows(pose, frame))
            throw new InvalidOperationException($"Samus's body cannot show pose {pose} at animation frame {frame}: no path of the game reaches it.");
        if (!SamusBodyDrawing.MovementTypesDrawnWith(pose).Contains(movementType))
            throw new InvalidOperationException($"Samus's body cannot show pose {pose} while her movement is {movementType}.");
    }

    /// <summary>
    /// Every full body Samus's body can be drawn as with <paramref name="body"/>'s pictures: each shown frame of each pose
    /// that shows installed pictures, with every movement type the pose is drawn with and every bottom picture the frame
    /// can show, in pose, frame, movement and bottom order.
    /// </summary>
    internal IEnumerable<SamusShownBody> Bodies(InstalledSamusBodyArtwork body)
    {
        Ensure.NotNull(body);
        Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>> bottoms = BottomsShown(body);
        foreach (SamusPoseId pose in Poses)
        {
            foreach (int frame in shown[pose])
            {
                if (!body.TryFrame(pose, frame, out SamusBodyFramePictures pictures))
                    continue;
                SamusBodyPicture?[] frameBottoms = [.. bottoms[new SamusPoseFrame(pose, frame)].OrderBy(bottom => bottom?.Ordinal ?? -1)];
                foreach (SamusMovementType movementType in SamusBodyDrawing.MovementTypesDrawnWith(pose))
                {
                    SamusBodyFrame drawn = SamusBodyDrawing.Frame(movementType, pose, frame, body);
                    foreach (SamusBodyPicture? bottom in frameBottoms)
                        yield return new SamusShownBody(drawn, new SamusBodyPictures(pictures.Top, bottom));
                }
            }
        }
    }

    /// <summary>
    /// The bottom pictures each shown frame with installed pictures can show (#1513): its own when it names one; otherwise
    /// the bottom picture the frame before it showed, for the frames of its own pose it follows, and for a frame a pose is
    /// started or published on any bottom picture a frame names, or none before any frame has named one.
    /// </summary>
    private Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>> BottomsShown(InstalledSamusBodyArtwork body)
    {
        var pictures = new Dictionary<SamusPoseFrame, SamusBodyFramePictures>();
        foreach (SamusPoseId pose in Poses)
        {
            foreach (int frame in shown[pose])
            {
                if (body.TryFrame(pose, frame, out SamusBodyFramePictures framePictures))
                    pictures.Add(new SamusPoseFrame(pose, frame), framePictures);
            }
        }
        var named = new HashSet<SamusBodyPicture?> { null };
        foreach (SamusBodyFramePictures framePictures in pictures.Values)
        {
            if (framePictures.Bottom is { } bottom)
                named.Add(bottom);
        }
        var bottoms = new Dictionary<SamusPoseFrame, HashSet<SamusBodyPicture?>>();
        foreach (KeyValuePair<SamusPoseFrame, SamusBodyFramePictures> frame in pictures)
        {
            var frameBottoms = new HashSet<SamusBodyPicture?>();
            if (frame.Value.Bottom is { } own)
                frameBottoms.Add(own);
            else if (entries[frame.Key.Pose].Contains(frame.Key.Frame))
                frameBottoms.UnionWith(named);
            bottoms.Add(frame.Key, frameBottoms);
        }
        // A frame that names no bottom keeps the bottom of the frame before it, which may itself have kept an earlier one.
        bool changed;
        do
        {
            changed = false;
            foreach (KeyValuePair<SamusPoseFrame, HashSet<SamusBodyPicture?>> frame in bottoms)
            {
                if (pictures[frame.Key].Bottom is not null || !predecessors.TryGetValue(frame.Key, out HashSet<int>? before))
                    continue;
                foreach (int earlier in before)
                {
                    // A frame that follows itself keeps what it already shows.
                    if (earlier == frame.Key.Frame ||
                        !bottoms.TryGetValue(new SamusPoseFrame(frame.Key.Pose, earlier), out HashSet<SamusBodyPicture?>? earlierBottoms))
                        continue;
                    foreach (SamusBodyPicture? bottom in earlierBottoms)
                        changed |= frame.Value.Add(bottom);
                }
            }
        }
        while (changed);
        return bottoms;
    }

    /// <summary>Shows <paramref name="frame"/> of <paramref name="pose"/>, following it on through the program when it is new.</summary>
    private void Show(SamusPoseId pose, int frame, Queue<SamusPoseFrame> pending)
    {
        if (shown[pose].Add(frame))
            pending.Enqueue(new SamusPoseFrame(pose, frame));
    }

    /// <summary>The frames of its own pose <paramref name="frame"/> can follow.</summary>
    private HashSet<int> PredecessorsOf(SamusPoseId pose, int frame)
    {
        if (!predecessors.TryGetValue(new SamusPoseFrame(pose, frame), out HashSet<int>? before))
        {
            before = [];
            predecessors.Add(new SamusPoseFrame(pose, frame), before);
        }
        return before;
    }

    /// <summary>
    /// Shows a frame an outside path publishes: the frame itself when it is drawn as written, or the frames the animation
    /// advances it to first. Either starts the pose from whatever Samus showed before.
    /// </summary>
    private void Publish(SamusPoseId pose, SamusPublishedFrame publication, Queue<SamusPoseFrame> pending)
    {
        int[] started = publication.Drawing == PublishedFrameDrawing.AfterAdvancing ? Next(pose, publication.Frame) : [publication.Frame];
        foreach (int frame in started)
        {
            entries[pose].Add(frame);
            Show(pose, frame, pending);
        }
    }

    /// <summary>The frames the animation can show after <paramref name="frame"/> of <paramref name="pose"/> when its delay runs out.</summary>
    private int[] Next(SamusPoseId pose, int frame) => Reach(pose, frame + 1, FrameReach.Advancing);

    /// <summary>
    /// The frames the animation shows on reaching <paramref name="frame"/> of <paramref name="pose"/>: the frame when it is a
    /// delay, or a hold reached by advancing; the delays an instruction there selects; or none where the animation fails or
    /// the pose ends. <paramref name="reach"/> says whether an instruction selected the frame, which must then be a delay.
    /// </summary>
    /// <exception cref="InvalidDataException">A program holds a step the animation has no handler for.</exception>
    private int[] Reach(SamusPoseId pose, int frame, FrameReach reach)
    {
        SamusAnimationProgramStep? step = SamusAnimationDelays.TryStepFor(pose, frame, continuation);
        if (step is SamusAnimationDelay)
            return [frame];
        if (reach == FrameReach.Selection || step is null)
            return [];
        return step switch
        {
            SamusAnimationHold => [frame],
            SamusAnimationRepeat repeat => Reach(pose, repeat.Frame, FrameReach.Selection),
            SamusAnimationRestart => Reach(pose, 0, FrameReach.Selection),
            SamusAnimationLoopUnlessLowEnergy => [.. Reach(pose, frame + 1, FrameReach.Selection), .. Reach(pose, 0, FrameReach.Selection)],
            SamusAnimationWallJumpSelect select =>
            [
                .. Reach(pose, select.OrdinaryFrame, FrameReach.Selection),
                .. Reach(pose, select.SpaceJumpFrame, FrameReach.Selection),
                .. Reach(pose, select.ScrewAttackFrame, FrameReach.Selection),
            ],
            // The drained fall's handover fails for any pose but the drained crouch.
            SamusAnimationInstallDrainedFall => pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose
                ? Reach(pose, frame + 1, FrameReach.Selection) : [],
            SamusAnimationTransition or SamusAnimationTurnTransition or SamusAnimationItemTransition or
                SamusAnimationItemAirborneTransition => [],
            _ => throw new InvalidDataException($"Samus animation step {step} has no handler."),
        };
    }

    /// <summary>
    /// The frames the paths outside <paramref name="pose"/>'s program can publish for it, given the frames it shows so far,
    /// each as <see cref="SamusDrainedState"/> or the spin's wall contact writes it.
    /// </summary>
    private List<SamusPublishedFrame> Publications(SamusPoseId pose)
    {
        SortedSet<int> frames = shown[pose];
        var publications = new List<SamusPublishedFrame>();
        // The able-to-stand handler moves the left-facing drained crouch to its stand-up on Up, just before the animation.
        if (pose == SamusPoseId.DrainedCrouchingLeftPose && frames.Max >= SamusDrainedAnimationDefinitions.AbleToStandFirstFrame)
            publications.Add(new(SamusDrainedAnimationDefinitions.AbleToStandFrame, PublishedFrameDrawing.AfterAdvancing));
        // The unable-to-stand handler stays installed until Mother Brain dies and checks only the frame, so on Up it moves
        // whatever pose shows one of its frames, just before the animation.
        if (frames.GetViewBetween(SamusDrainedAnimationDefinitions.UnableToStandFirstFrame,
                SamusDrainedAnimationDefinitions.UnableToStandLastFrame).Count > 0)
            publications.Add(new(SamusDrainedAnimationDefinitions.UnableToStandFrame, PublishedFrameDrawing.AfterAdvancing));
        if (SamusPoseMetadata.IsDrainedPose(pose))
        {
            // The Baby Metroid's Hyper Beam and rainbow-stop commands write the drained pose's frame during enemy AI, after
            // the animation, so the draw shows it as written.
            publications.Add(new(SamusDrainedAnimationDefinitions.HyperBeamAcquisitionFrame, PublishedFrameDrawing.AsWritten));
            publications.Add(new(SamusDrainedAnimationDefinitions.RainbowStopFrame, PublishedFrameDrawing.AsWritten));
        }
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
        {
            // The drained fall lands on its landing frame for eight ticks; the release moves the crouch on during enemy AI.
            publications.Add(new(SamusDrainedAnimationDefinitions.LandingFrame, PublishedFrameDrawing.AsWritten));
            publications.Add(new(SamusDrainedAnimationDefinitions.ReleaseCrouchingFrame, PublishedFrameDrawing.AsWritten));
        }
        if (pose is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose)
            publications.Add(new(SamusDrainedAnimationDefinitions.ReleaseStandingFrame, PublishedFrameDrawing.AsWritten));
        // Wall contact rewinds whatever pose holds the spin's movement, during movement and with a timer of one.
        if (SamusBodyDrawing.MovementTypesDrawnWith(pose).Contains(SamusMovementType.SpinJumping))
        {
            publications.Add(new(SamusPoseMetadata.IsScrewAttackPose(pose)
                ? SamusWallContactAnimationDefinitions.ScrewAttackRewindFrame
                : SamusWallContactAnimationDefinitions.SpinJumpRewindFrame, PublishedFrameDrawing.AfterAdvancing));
        }
        return publications;
    }

    /// <summary>
    /// The frames a pose can be started on from whatever Samus showed before: the frames pose initialization starts it on,
    /// and the delays the Crystal Flash, X-ray, death, fast-fall, Zebes-start and grapple-swing paths publish for it.
    /// </summary>
    private static IEnumerable<int> InitialFrames(SamusPoseId pose)
    {
        yield return 0;
        // A spin jump starts on its second frame; a standing pose aiming up entered from one keeps the gun raised on it.
        if (SamusPoseMetadata.IsSpinJumpPose(pose) ||
            (SamusPoseMetadata.ReadMovementType(pose) == SamusMovementType.Standing &&
             SamusPoseMetadata.ReadShotDirection(pose) is 0 or 9))
            yield return 1;
        if (pose is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
            yield return SamusDrainedAnimationDefinitions.FallInitialFrame;
        if (pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose or
            SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose)
            yield return SamusMovementDefinitions.FastFallAnimationFrame;
        if (pose is SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose)
        {
            foreach (int frame in SamusDeathSequenceDefinitions.InitialFrames.Values)
                yield return frame;
        }
        // The angle, the Crystal Flash's stages and the Zebes start select these poses' frames directly.
        if (pose is SamusPoseId.XrayingStandingRightPose or SamusPoseId.XrayingStandingLeftPose or
            SamusPoseId.XrayingCrouchingRightPose or SamusPoseId.XrayingCrouchingLeftPose or
            SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose or
            SamusPoseId.CrystalFlashRightPose or SamusPoseId.CrystalFlashLeftPose or
            SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose)
        {
            for (int frame = 0; frame < SamusAnimationDelays.ProgramForPose(pose).FrameCount; frame++)
                yield return frame;
        }
    }

    /// <summary>A frame an outside path publishes for a pose.</summary>
    /// <param name="Frame">The animation frame written.</param>
    /// <param name="Drawing">Whether the draw shows the frame as written or after the animation pass advances it.</param>
    private readonly record struct SamusPublishedFrame(int Frame, PublishedFrameDrawing Drawing);

    /// <summary>A frame an outside path publishes, with the pose it publishes it for.</summary>
    /// <param name="Pose">The pose.</param>
    /// <param name="Publication">The frame published.</param>
    private readonly record struct PosePublication(SamusPoseId Pose, SamusPublishedFrame Publication);

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

        /// <summary>An instruction selected the frame, which must be a delay.</summary>
        Selection,
    }
}
