using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's head animation once he has risen (#2554): his rise's last update selects his roar, which is the first animation
/// his head plays, so a head animation exists exactly from the end of his rise, and the stages after it hold it
/// (<see cref="KraidStageRun.Risen"/>). Before his rise ends his head holds its pose without an animation
/// (<see cref="KraidEnemyState.HoldHeadUntilRise"/>), and a room he was found defeated in never shows his head.
/// </summary>
public sealed class KraidRisenHead
{
    /// <summary>A head standing at <paramref name="playback"/>.</summary>
    /// <param name="playback">The head animation and where it stands.</param>
    internal KraidRisenHead(AnimationPlayback<KraidHeadSegment> playback) => Playback = playback;

    /// <summary>
    /// The head his rise's last update leaves: the roar selected for his first attack, standing past its opening pose,
    /// while the head's hold runs on as it is.
    /// </summary>
    internal static KraidRisenHead SelectingRoar() => new(KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Playback);

    /// <summary>The playing head animation and where it stands; <see cref="KraidEnemyState.HeadHold"/> paces it.</summary>
    internal AnimationPlayback<KraidHeadSegment> Playback { get; set; }

    /// <summary>
    /// Starts the head animation that starts at <paramref name="animation"/> (#2399): the head enters its opening pose without
    /// showing it, and holds it for that pose's duration.
    /// </summary>
    /// <param name="animation">The animation to start.</param>
    /// <param name="hold">The head's hold on its pose.</param>
    internal void Start(KraidHeadSegment animation, HeldBossCountdown hold) => Enter(KraidHeadAnimations.Starting(animation), hold);

    /// <summary>
    /// Selects the head animation that starts at <paramref name="animation"/> (#2399): the head stands past its opening pose,
    /// as starting it leaves it, while its hold runs on as it is.
    /// </summary>
    /// <param name="animation">The animation to select.</param>
    internal void Select(KraidHeadSegment animation) => Playback = KraidHeadAnimations.Starting(animation).Playback;

    /// <summary>
    /// Resumes the roar as growth does from where the head's animation stands (#2399): the head enters the resumed pose
    /// without showing it, and holds it for that pose's duration.
    /// </summary>
    /// <param name="hold">The head's hold on its pose.</param>
    internal void ResumeRoarForGrowth(HeldBossCountdown hold) =>
        Enter(KraidHeadAnimations.GrowthResume(KraidHeadAnimations.ReadsAt(Playback).Growth), hold);

    /// <summary>Stands the head past the pose <paramref name="entry"/> entered, holding that pose for its duration.</summary>
    private void Enter(KraidHeadEntry entry, HeldBossCountdown hold)
    {
        Playback = entry.Playback;
        hold.LastFor(entry.Delay);
    }
}
