using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A starfield's motion with the gunship's flyaway (#1903, #2489): once waiting, the starfield starts moving as the fast
/// planet escape starts, to the right and slowing each update, and it leaves at
/// the handoff out of the explosion's last phase.
/// </summary>
/// <param name="motion">How the starfield starts moving with the flyaway.</param>
internal sealed partial class EndingFlyawayStarsMotion(EndingStarsMotion motion) : EndingActorMotion
{
    /// <summary>
    /// A starfield's motion waiting for the flyaway: the left starfield's from its spawn on, and the right one's once its
    /// animation's cue (<see cref="EndingSceneCue.WaitForFlyaway"/>) has it wait, before it has moved.
    /// </summary>
    internal static EndingFlyawayStarsMotion WaitingFromSpawn() => new(EndingStarsMotion.WaitingForFlyaway);

    /// <summary>
    /// The right starfield's motion: it waits for the flyaway only once its animation has it wait
    /// (<see cref="EndingSceneCue.WaitForFlyaway"/>).
    /// </summary>
    internal static EndingFlyawayStarsMotion WaitingWhenCued() => new(EndingStarsMotion.NotYetWaiting);

    /// <summary>How the starfield moves with the flyaway.</summary>
    internal EndingStarsMotion Motion { get; private set; } = motion;

    /// <summary>The starfield's speed across the screen once the flyaway starts it.</summary>
    private SignedSixteenSixteen velocity;

    /// <summary>Starts or continues the starfield's flyaway, then deletes it once the completion text starts.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        Ensure.NotNull(scene);
        if (Motion == EndingStarsMotion.WaitingForFlyaway)
        {
            if (EndingCreditsPhaseGroups.StartingFlyaway.Contains(scene.Phase))
            {
                Motion = EndingStarsMotion.Moving;
                velocity = EndingMotionDefinitions.StarsFlyawayInitialVelocity;
            }
        }
        else if (Motion == EndingStarsMotion.Moving)
        {
            velocity -= EndingMotionDefinitions.StarsFlyawayDeceleration;
            actor.Position = actor.Position.WithX(actor.Position.X + velocity);
        }
        if (EndingCreditsPhaseGroups.CompletionText.Contains(scene.Phase))
            actor.Delete();
    }
}
