using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's wing beat (#2329): the pose of the beat his wings show, and the countdown to the next pose, which each update
/// counts down at the pace his flight speed sets. Once the countdown runs below zero the wings take the beat's next pose,
/// the first after the last, and the countdown starts again.
/// </summary>
public sealed class RidleyWingBeat
{
    /// <summary>Begins the beat showing <paramref name="firstPose"/>, with the countdown at zero.</summary>
    /// <param name="firstPose">The pose the wings show first; the beat shows it once.</param>
    /// <exception cref="ArgumentException">The beat does not show <paramref name="firstPose"/> exactly once.</exception>
    internal RidleyWingBeat(RidleyWingPose firstPose)
    {
        Pose = PlaceOf(firstPose);
    }

    /// <summary>Gets the pose of the beat the wings show.</summary>
    public ListStep<RidleyWingPose> Pose { get; private set; }

    /// <summary>Gets the countdown to the wings' next pose, which starts again from <see cref="RidleyWingDefinitions.BeatCountdown"/> once it runs below zero.</summary>
    public PacedCountdown Countdown { get; } = new();

    /// <summary>
    /// Counts <paramref name="pace"/> off the countdown, and once it runs below zero shows the beat's next pose and starts
    /// the countdown again.
    /// </summary>
    /// <param name="pace">The countdown the update takes off, which Ridley's flight speed sets.</param>
    internal void Beat(int pace)
    {
        if (!Countdown.CountOff(pace))
            return;
        Countdown.Restart(RidleyWingDefinitions.BeatCountdown);
        Pose = Pose.NextInCycle;
    }

    /// <summary>The place of <paramref name="pose"/> in the wing beat.</summary>
    /// <exception cref="ArgumentException">The beat does not show <paramref name="pose"/> exactly once.</exception>
    private static ListStep<RidleyWingPose> PlaceOf(RidleyWingPose pose)
    {
        IReadOnlyList<RidleyWingPose> beat = RidleySupplementalVisualDefinitions.WingBeat;
        int[] places = [.. Enumerable.Range(0, beat.Count).Where(place => beat[place] == pose)];
        return places.Length == 1
            ? ListStep<RidleyWingPose>.At(beat, places[0])
            : throw new ArgumentException($"Ridley's wing beat shows {pose} {places.Length} times, not once.", nameof(pose));
    }
}
