using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The Bomb Torizo statue's hand (#1554). It shows the intact hand on its first update and arms its wake on its second;
/// from its third update on, the first update on which Samus has Bombs wakes it, before its timing runs, and it starts
/// its crumble at once: each step drops its fragment, shows its image and holds it for its updates. After the last step
/// it queues the boss music and leaves the room, so Bomb Torizo sees it gone
/// (<see cref="BombTorizoHandObjects.HandRemains"/>).
/// </summary>
/// <param name="owner">The room's hand owner, which drops the fragments, queues the music and reads Samus.</param>
[RoomLifetime]
internal sealed class BombTorizoHandObject(BombTorizoHandObjects owner) : RoomObjectBehaviour
{
    /// <summary>The room's hand owner.</summary>
    private readonly BombTorizoHandObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the hand is in its course.</summary>
    internal BombTorizoHandPhase Phase { get; private set; } = BombTorizoHandPhase.Starting;

    /// <summary>Whether Samus's Bombs wake the hand: from its second update until they do.</summary>
    internal bool WakeArmed { get; private set; }

    /// <summary>The step of its crumble the hand shows; none before its crumble starts (#1627, #2332).</summary>
    internal ListStep<BombTorizoHandCrumbleStep>? CrumbleStep { get; private set; }

    /// <summary>How long the hand holds before its next step: its first update, then each step's own updates.</summary>
    private readonly BossCountdown _holdBeforeNextStep = BossCountdown.Lasting(1);

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && _owner.SamusHasBombs())
        {
            WakeArmed = false;
            Phase = BombTorizoHandPhase.Crumbling;
            _holdBeforeNextStep.LastFor(1);
        }
        if (_holdBeforeNextStep.StepKeepsRunning())
            return;
        switch (Phase)
        {
            case BombTorizoHandPhase.Starting:
                context.Draw(place, BombTorizoHandDraws.Family, BombTorizoHandCrumbleDefinitions.RestingImage);
                _holdBeforeNextStep.LastFor(1);
                Phase = BombTorizoHandPhase.Arming;
                return;
            case BombTorizoHandPhase.Arming:
                WakeArmed = true;
                _holdBeforeNextStep.LastFor(1);
                Phase = BombTorizoHandPhase.Resting;
                return;
            case BombTorizoHandPhase.Resting:
                _holdBeforeNextStep.LastFor(1);
                return;
            case BombTorizoHandPhase.Crumbling:
                ListStep<BombTorizoHandCrumbleStep>? next = CrumbleStep is { } shown
                    ? shown.Following
                    : ListStep<BombTorizoHandCrumbleStep>.FirstOf(BombTorizoHandCrumbleDefinitions.Crumble);
                if (next is not { } nextStep)
                {
                    _owner.QueueMusic();
                    context.RoomObjects.DeleteObject(place);
                    return;
                }
                CrumbleStep = nextStep;
                BombTorizoHandCrumbleStep step = nextStep.Entry;
                if (step.FragmentBefore is { } fragment)
                    BombTorizoHandObjects.ThrowFragment(place, fragment, context.Outcome);
                context.Draw(place, BombTorizoHandDraws.Family, step.Image);
                _holdBeforeNextStep.LastFor(step.Updates);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandPhase)} {Phase}.");
        }
    }
}
