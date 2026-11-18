using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// A starfield's motion with the gunship's flyaway (#1903, #2489): once waiting, the starfield starts moving as the fast
/// planet escape starts, a quarter pixel an update to the right, slowing by 1/2048 of a pixel each update, and it leaves at
/// the handoff out of the explosion's last phase.
/// </summary>
/// <param name="motion">How the starfield starts moving with the flyaway, or null for one that does not yet wait for it.</param>
internal sealed class EndingFlyawayStarsMotion(EndingStarsMotion? motion) : EndingActorMotion
{
    /// <summary>The left starfield's motion: it waits for the flyaway from its spawn on.</summary>
    internal static EndingFlyawayStarsMotion WaitingFromSpawn() => new(EndingStarsMotion.WaitingForFlyaway);

    /// <summary>The right starfield's motion: it waits for the flyaway only once its animation has it wait (<see cref="WaitForFlyaway"/>).</summary>
    internal static EndingFlyawayStarsMotion WaitingWhenCued() => new(null);

    /// <summary>How the starfield moves with the flyaway, or null while it does not yet wait for it.</summary>
    internal EndingStarsMotion? Motion { get; private set; } = motion;

    /// <summary>The starfield's speed across the screen once the flyaway starts it.</summary>
    private SignedSixteenSixteen velocity;

    /// <summary>Has the starfield wait for the flyaway to carry it away.</summary>
    internal void WaitForFlyaway() => Motion = EndingStarsMotion.WaitingForFlyaway;

    /// <summary>Starts or continues the starfield's flyaway, then deletes it once the completion text starts.</summary>
    internal override void Step(ICinematicActor<EndingSceneCue> actor, IEndingActorScene scene)
    {
        Ensure.NotNull(actor);
        Ensure.NotNull(scene);
        switch (Motion)
        {
            case null:
                break;
            case EndingStarsMotion.WaitingForFlyaway:
                if (scene.Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    Motion = EndingStarsMotion.Moving;
                    velocity = EndingMotionDefinitions.StarsFlyawayInitialVelocity;
                }
                break;
            case EndingStarsMotion.Moving:
                velocity -= EndingMotionDefinitions.StarsFlyawayDeceleration;
                actor.Position = actor.Position.WithX(actor.Position.X + velocity);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingStarsMotion)} {Motion}.");
        }
        if (scene.Phase == EndingCreditsPhase.OperationSuccessfulText)
            actor.Delete();
    }
}
