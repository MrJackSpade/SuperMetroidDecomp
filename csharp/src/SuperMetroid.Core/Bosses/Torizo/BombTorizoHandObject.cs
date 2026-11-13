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

    /// <summary>The crumble step the hand shows, or -1 before its crumble starts.</summary>
    internal int CrumbleStep { get; private set; } = -1;

    /// <summary>The updates left before the hand's next step.</summary>
    private int _updatesBeforeNextStep = 1;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && _owner.SamusHasBombs())
        {
            WakeArmed = false;
            Phase = BombTorizoHandPhase.Crumbling;
            _updatesBeforeNextStep = 1;
        }
        if (--_updatesBeforeNextStep != 0)
            return;
        switch (Phase)
        {
            case BombTorizoHandPhase.Starting:
                context.Draw(place, RoomObjectDraw.Of(BombTorizoHandCrumbleDefinitions.RestingImage));
                _updatesBeforeNextStep = 1;
                Phase = BombTorizoHandPhase.Arming;
                return;
            case BombTorizoHandPhase.Arming:
                WakeArmed = true;
                _updatesBeforeNextStep = 1;
                Phase = BombTorizoHandPhase.Resting;
                return;
            case BombTorizoHandPhase.Resting:
                _updatesBeforeNextStep = 1;
                return;
            case BombTorizoHandPhase.Crumbling:
                CrumbleStep++;
                if (CrumbleStep == BombTorizoHandCrumbleDefinitions.Crumble.Count)
                {
                    _owner.QueueMusic();
                    context.RoomObjects.DeleteObject(place);
                    return;
                }
                BombTorizoHandCrumbleStep step = BombTorizoHandCrumbleDefinitions.Crumble[CrumbleStep];
                if (step.FragmentBefore is { } fragment)
                    _owner.SpawnFragment(place, fragment);
                context.Draw(place, RoomObjectDraw.Of(step.Image));
                _updatesBeforeNextStep = step.Updates;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BombTorizoHandPhase)} {Phase}.");
        }
    }
}
