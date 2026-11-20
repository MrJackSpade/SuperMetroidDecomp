using System.Diagnostics.CodeAnalysis;
using SamusFramesBeyondAnimation = SuperMetroid.Core.Compatibility.FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusAnimationName, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame, SuperMetroid.Core.Samus.Poses.SamusAnimationPlaceName, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep>;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Which animation each Samus pose animates by (#1473), and how her animation reads its frames (#2048).</summary>
internal static class SamusAnimationDelays
{
    /// <summary>Every animation by its name.</summary>
    private static readonly Dictionary<SamusAnimationName, SamusPoseAnimation> ByName = NameEveryAnimation();

    /// <summary>The animation <paramref name="pose"/> animates by.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseAnimation AnimationForPose(SamusPoseId pose) => SamusPoseTraits.Of(pose).Animation;

    /// <summary>The animation named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The name names no animation.</exception>
    internal static SamusPoseAnimation AnimationNamed(SamusAnimationName name) =>
        ByName.TryGetValue(name, out SamusPoseAnimation? animation)
            ? animation
            : throw new ArgumentOutOfRangeException(nameof(name), name, "No Samus animation has this name.");

    /// <summary>The step of <paramref name="pose"/>'s animation at <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException">The frame reads no step of the pose's animation.</exception>
    /// <remarks>
    /// A frame on a value a cue carries reads the step the value selects, and a frame past the animation's end reads the
    /// frame of another animation the import decoded for it, both through <paramref name="framesBeyondAnimation"/>, which
    /// names each animation by its own name (#1535, #2048); a cue read there names its frames in that animation
    /// (<see cref="Target"/>).
    /// </remarks>
    /// <param name="pose">The pose whose animation is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="framesBeyondAnimation">What a frame that is no step of its animation reads.</param>
    internal static SamusPoseAnimationStep StepFor(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation) =>
        TryStepFor(pose, frame, framesBeyondAnimation, out SamusPoseAnimationStep? step) ? step : throw new InvalidDataException(
            $"Samus animation frame {frame} is not a step of pose {pose}'s animation nor of an animation a frame read past its end reaches.");

    /// <summary>
    /// Whether <paramref name="frame"/> of <paramref name="pose"/>'s animation reads a step, read as <see cref="StepFor"/> reads
    /// it, and the <paramref name="step"/>: a frame past every animation, or on a cue value whose step would publish a record
    /// no gameplay path gives Samus, reads none, where the animation fails.
    /// </summary>
    /// <param name="pose">The pose whose animation is read.</param>
    /// <param name="frame">Samus's animation frame.</param>
    /// <param name="step">The step read, when one is.</param>
    /// <param name="framesBeyondAnimation">What a frame that is no step of its animation reads.</param>
    /// <exception cref="InvalidDataException">A read past the animation reaches a frame that names no step.</exception>
    internal static bool TryStepFor(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation,
        [NotNullWhen(true)] out SamusPoseAnimationStep? step)
    {
        Ensure.NotNull(framesBeyondAnimation);
        SamusPoseAnimation animation = AnimationForPose(pose);
        if (TryOwnStep(animation, frame, framesBeyondAnimation, out step))
            return true;
        if (!framesBeyondAnimation.TryReadPast(animation.Name, frame, out SamusFramesBeyondAnimation.Read? read))
            return false;
        if (!TryOwnStep(AnimationNamed(read.Animation), read.Frame, framesBeyondAnimation, out step))
            throw new InvalidDataException(
                $"Samus animation frame {frame} of pose {pose} reads frame {read.Frame} of the {read.Animation} animation, which names no step.");
        return true;
    }

    /// <summary>
    /// The frame after <paramref name="frame"/> of <paramref name="pose"/>'s animation, past its end and past a segment's cue
    /// included (<see cref="SamusPoseAnimation.Next"/>).
    /// </summary>
    internal static SamusAnimationFrame Next(SamusPoseId pose, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation) =>
        AnimationForPose(pose).Next(frame, framesBeyondAnimation);

    /// <summary>
    /// The frame of <paramref name="pose"/>'s animation a cue read at <paramref name="at"/> moves it to when it names
    /// <paramref name="target"/>: the named frame of the pose's own animation for a cue of its own, or, for a cue a read past
    /// the animation reaches, the frame the import decoded for the frame the cue names in the later animation (#1535).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame <paramref name="at"/> reads no step, or the read names no move to the target.</exception>
    internal static SamusAnimationFrame Target(
        SamusPoseId pose,
        SamusAnimationFrame at,
        SamusAnimationFrame target,
        SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(framesBeyondAnimation);
        SamusPoseAnimation animation = AnimationForPose(pose);
        if (TryOwnStep(animation, at, framesBeyondAnimation, out _))
            return animation.Require(target);
        if (!framesBeyondAnimation.TryReadPast(animation.Name, at, out SamusFramesBeyondAnimation.Read? read))
            throw new InvalidDataException($"Samus animation frame {at} of pose {pose} reads no step whose cue could name a frame.");
        SamusAnimationFrame named = AnimationNamed(read.Animation).Require(target);
        return read.Moves.TryGetValue(named, out SamusAnimationFrame moved)
            ? moved
            : throw new InvalidDataException(
                $"Samus animation frame {at} of pose {pose} reads the {read.Animation} animation, whose frame {named} it has no installed move to.");
    }

    /// <summary>
    /// The frame of <paramref name="pose"/>'s animation <paramref name="place"/> sets: the named frame itself when the pose
    /// animates by the place's animation, otherwise the frame the import decoded for the place in the pose's (#2048).
    /// </summary>
    internal static SamusAnimationFrame Land(SamusPoseId pose, SamusAnimationPlace place, SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(place.Animation);
        Ensure.NotNull(framesBeyondAnimation);
        SamusPoseAnimation animation = AnimationForPose(pose);
        return ReferenceEquals(animation, place.Animation) ? place.Frame : framesBeyondAnimation.Landing(place.Name, animation.Name);
    }

    /// <summary>
    /// Whether <paramref name="frame"/> of <paramref name="pose"/>'s animation has reached <paramref name="place"/>: lies at
    /// it or after it, as the frame <paramref name="place"/> sets on the pose would (<see cref="Land"/>).
    /// </summary>
    internal static bool HasReached(
        SamusPoseId pose,
        SamusAnimationFrame frame,
        SamusAnimationPlace place,
        SamusFramesBeyondAnimation framesBeyondAnimation) =>
        AnimationForPose(pose).Compare(frame, Land(pose, place, framesBeyondAnimation)) >= 0;

    /// <summary>Whether <paramref name="frame"/> of <paramref name="pose"/>'s animation lies past <paramref name="place"/>.</summary>
    internal static bool HasPassed(
        SamusPoseId pose,
        SamusAnimationFrame frame,
        SamusAnimationPlace place,
        SamusFramesBeyondAnimation framesBeyondAnimation) =>
        AnimationForPose(pose).Compare(frame, Land(pose, place, framesBeyondAnimation)) > 0;

    /// <summary>
    /// The step <paramref name="frame"/> of <paramref name="animation"/> names itself: one of its delays or cues, or a value
    /// one of its cues carries; null for a frame read past the animation.
    /// </summary>
    private static bool TryOwnStep(SamusPoseAnimation animation, SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation,
        [NotNullWhen(true)] out SamusPoseAnimationStep? step)
    {
        step = animation.TryStepAt(frame) ?? framesBeyondAnimation.CueValueStep(animation.Name, frame);
        return step is not null;
    }

    /// <summary>Every animation of <see cref="SamusAnimationDelayDefinitions.All"/> by its name, which each has once.</summary>
    /// <exception cref="InvalidOperationException">An animation's name is missing from the definitions.</exception>
    private static Dictionary<SamusAnimationName, SamusPoseAnimation> NameEveryAnimation()
    {
        Dictionary<SamusAnimationName, SamusPoseAnimation> byName = SamusAnimationDelayDefinitions.All.ToDictionary(animation => animation.Name);
        return byName.Count == Enum.GetValues<SamusAnimationName>().Length
            ? byName
            : throw new InvalidOperationException("A Samus animation name has no animation.");
    }
}
