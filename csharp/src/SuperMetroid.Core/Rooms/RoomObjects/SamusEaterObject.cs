namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A Brinstar plant that has caught Samus (#1554). Its first update bites: from its second update on, before its timing
/// runs, it holds Samus at the place she was caught, until the update it releases her. It plays its chew its plant's
/// number of times (#1994), each a run of images some of which bite first; then it releases Samus with her release
/// immunity, shows its fangs out, then its idle image for one update, and leaves the room.
/// </summary>
/// <param name="owner">The room's plants, which reach Samus.</param>
/// <param name="plant">The plant's mounting and authored chew.</param>
/// <param name="heldPosition">Where the plant holds Samus.</param>
[RoomLifetime]
internal sealed class SamusEaterObject(SamusEaterObjects owner, SamusEaterObjectDefinition plant, Position16<RoomPixels> heldPosition)
    : SequencedRoomObject<SamusEaterChewFrame>
{
    /// <summary>The room's plants.</summary>
    private readonly SamusEaterObjects _owner = Ensure.NotNull(owner);

    /// <summary>The plant's mounting and authored chew.</summary>
    internal SamusEaterObjectDefinition Plant { get; } = Ensure.NotNull(plant);

    /// <summary>Where the plant holds Samus.</summary>
    internal Position16<RoomPixels> HeldPosition { get; } = heldPosition;

    /// <summary>Where the plant is in its course.</summary>
    internal SamusEaterPhase Phase { get; private set; } = SamusEaterPhase.Biting;

    /// <summary>Whether the plant holds Samus: from its first update until it releases her.</summary>
    internal bool Holding { get; private set; }

    /// <summary>The chews left, counting the one under way.</summary>
    internal int ChewsLeft { get; private set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (Holding)
            _owner.HoldSamus(HeldPosition);
        base.Update(place, context);
    }

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (Phase)
        {
            case SamusEaterPhase.Biting:
                Holding = true;
                ChewsLeft = Plant.Chews;
                Phase = SamusEaterPhase.Chewing;
                Play(Plant.Chew, place, context);
                return;
            case SamusEaterPhase.Chewing:
                ChewsLeft--;
                if (ChewsLeft != 0)
                {
                    Play(Plant.Chew, place, context);
                    return;
                }
                _owner.ReleaseSamus();
                Holding = false;
                Phase = SamusEaterPhase.Released;
                Play([new(Plant.Released, SamusEaterObjectDefinitions.ReleaseDuration, SamusEaterBite.None)], place, context);
                return;
            case SamusEaterPhase.Released:
                Phase = SamusEaterPhase.Resting;
                Play([new(Plant.Idle, 1, SamusEaterBite.None)], place, context);
                return;
            case SamusEaterPhase.Resting:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterPhase)} {Phase}.");
        }
    }

    /// <summary>Bites as <paramref name="step"/> asks, then shows it; returns the updates it stays.</summary>
    private protected override int Show(SamusEaterChewFrame step, RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (step.BiteBefore)
        {
            case SamusEaterBite.None:
                break;
            case SamusEaterBite.SoundAndDamage:
                context.QueueSound(SamusEaterObjectDefinitions.ChewSound, SamusEaterObjectDefinitions.ChewSoundPriority);
                _owner.DamageSamus();
                break;
            case SamusEaterBite.Damage:
                _owner.DamageSamus();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterBite)} {step.BiteBefore}.");
        }
        context.Draw(place, SamusEaterObjectDraws.Family, step.Image);
        return step.Updates;
    }
}
