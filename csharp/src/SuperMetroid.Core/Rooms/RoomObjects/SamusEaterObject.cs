namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A Brinstar plant that has caught Samus (#1554). Its first update bites: from its second update on, before its timing
/// runs, it holds Samus at the place she was caught, until the update it releases her. It chews its plant's number of
/// chews, each a run of images some of which bite first; then it releases Samus with her release immunity, shows its
/// closed mouth, then its idle image for one update, and leaves the room.
/// </summary>
/// <param name="owner">The room's plants, which reach Samus.</param>
/// <param name="plant">The plant's mounting and authored chew.</param>
/// <param name="heldPosition">Where the plant holds Samus.</param>
[RoomLifetime]
internal sealed class SamusEaterObject(SamusEaterObjects owner, SamusEaterObjectDefinition plant, Position16<RoomPixels> heldPosition)
    : RoomObjectBehaviour
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

    /// <summary>The chew image the plant shows.</summary>
    private int _frame;

    /// <summary>The updates left before the plant's next step.</summary>
    private int _updatesBeforeNextStep = 1;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (Holding)
            _owner.HoldSamus(HeldPosition);
        if (--_updatesBeforeNextStep != 0)
            return;
        switch (Phase)
        {
            case SamusEaterPhase.Biting:
                Holding = true;
                ChewsLeft = Plant.Chews;
                Phase = SamusEaterPhase.Chewing;
                ShowChew(place, context, 0);
                return;
            case SamusEaterPhase.Chewing:
                if (_frame + 1 < Plant.Chew.Count)
                {
                    ShowChew(place, context, _frame + 1);
                    return;
                }
                ChewsLeft--;
                if (ChewsLeft != 0)
                {
                    ShowChew(place, context, 0);
                    return;
                }
                _owner.ReleaseSamus();
                Holding = false;
                Show(place, context, Plant.Released, SamusEaterObjectDefinitions.ReleaseDuration, SamusEaterPhase.Released);
                return;
            case SamusEaterPhase.Released:
                Show(place, context, Plant.Idle, 1, SamusEaterPhase.Resting);
                return;
            case SamusEaterPhase.Resting:
                context.RoomObjects.DeleteObject(place);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterPhase)} {Phase}.");
        }
    }

    /// <summary>Bites as chew image <paramref name="frame"/> asks, then shows it.</summary>
    private void ShowChew(RoomObjectSlot place, RoomObjectStepContext context, int frame)
    {
        _frame = frame;
        SamusEaterChewFrame chew = Plant.Chew[frame];
        switch (chew.BiteBefore)
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
                throw new InvalidOperationException($"Undefined {nameof(SamusEaterBite)} {chew.BiteBefore}.");
        }
        Show(place, context, chew.Image, chew.Updates, SamusEaterPhase.Chewing);
    }

    /// <summary>Draws <paramref name="image"/> and holds it for <paramref name="updates"/> in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectSlot place, RoomObjectStepContext context, SamusEaterDraw image, int updates, SamusEaterPhase phase)
    {
        context.Draw(place, RoomObjectDraw.Of(image));
        _updatesBeforeNextStep = updates;
        Phase = phase;
    }
}
