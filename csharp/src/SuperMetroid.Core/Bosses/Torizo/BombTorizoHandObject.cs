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
/// <param name="fragments">The room's enemy projectiles, among which the hand's fragments spawn.</param>
[RoomLifetime]
internal sealed partial class BombTorizoHandObject(BombTorizoHandObjects owner, IBombTorizoStatueFragmentSpawner fragments) : RoomObjectBehaviour
{
    /// <summary>The room's hand owner.</summary>
    private readonly BombTorizoHandObjects _owner = Ensure.NotNull(owner);

    /// <summary>The room's enemy projectiles, among which the hand's fragments spawn.</summary>
    private readonly IBombTorizoStatueFragmentSpawner _fragments = Ensure.NotNull(fragments);

    /// <summary>Where the hand is in its course.</summary>
    internal BombTorizoHandPhase Phase { get; private set; } = BombTorizoHandPhase.Starting;

    /// <summary>Whether Samus's Bombs wake the hand: while it rests, from its second update until they do (#2560).</summary>
    internal bool WakeArmed => Phase == BombTorizoHandPhase.Resting;

    /// <summary>The step of its crumble the hand shows; none before its crumble starts (#1627, #2332).</summary>
    internal ListStep<BombTorizoHandCrumbleStep>? CrumbleStep { get; private set; }

    /// <summary>How long the hand holds before its next step: its first update, then each step's own updates.</summary>
    private readonly BossCountdown _holdBeforeNextStep = BossCountdown.LastingOneUpdate();

    /// <summary>What <c>Update</c> does for each <see cref="BombTorizoHandPhase"/> member, each member's own arm.</summary>
    private IdentityTable<BombTorizoHandPhase, Action<RoomObjectPlace, RoomObjectStepContext>> UpdateForBombTorizoHandPhase => field ??= new(new Dictionary<BombTorizoHandPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [BombTorizoHandPhase.Starting] = (place, context) =>
        {
            context.Draw(place, BombTorizoHandDraws.Family, BombTorizoHandCrumbleDefinitions.RestingImage);
            _holdBeforeNextStep.LastFor(1);
            Phase = BombTorizoHandPhase.Arming;
        },
        [BombTorizoHandPhase.Arming] = (place, context) =>
        {
            _holdBeforeNextStep.LastFor(1);
            Phase = BombTorizoHandPhase.Resting;
        },
        [BombTorizoHandPhase.Resting] = (place, context) => _holdBeforeNextStep.LastFor(1),
        [BombTorizoHandPhase.Crumbling] = (place, context) =>
        {
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
                BombTorizoHandObjects.ThrowFragment(place, fragment, context.Outcome, _fragments);
            context.Draw(place, BombTorizoHandDraws.Family, step.Image);
            _holdBeforeNextStep.LastFor(step.Updates);
        },
    });

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && _owner.SamusHasBombs())
        {
            Phase = BombTorizoHandPhase.Crumbling;
            _holdBeforeNextStep.LastFor(1);
        }
        if (_holdBeforeNextStep.StepKeepsRunning())
            return;
        UpdateForBombTorizoHandPhase[Phase](place, context);
    }
}
