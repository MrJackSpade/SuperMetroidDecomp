namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A Brinstar plant that has caught Samus (#1554). Its first update bites: from its second update on, before its timing
/// runs, it holds Samus at the place she was caught, until the update it releases her. It plays its chew its plant's
/// number of times (#1994), each a run of images some of which bite first; then it releases Samus with her release
/// immunity, shows its fangs out, then its idle image for one update, and leaves the room.
/// </summary>
[RoomLifetime]
internal sealed partial class SamusEaterObject : SilentSequencedRoomObject<SamusEaterSegment, SamusEaterChewFrame>
{
    /// <summary>A plant that has just caught Samus.</summary>
    /// <param name="owner">The room's plants, which reach Samus.</param>
    /// <param name="plant">The plant's mounting and authored chew.</param>
    /// <param name="heldPosition">Where the plant holds Samus.</param>
    internal SamusEaterObject(SamusEaterObjects owner, SamusEaterObjectDefinition plant, Position16<RoomPixels> heldPosition)
        : this(owner, plant, heldPosition, new SamusEaterAnimations(plant))
    {
    }

    /// <summary>A plant that has just caught Samus, playing <paramref name="animations"/>, its chew's.</summary>
    private SamusEaterObject(SamusEaterObjects owner, SamusEaterObjectDefinition plant, Position16<RoomPixels> heldPosition,
        SamusEaterAnimations animations)
        : base(animations.Segments)
    {
        _owner = Ensure.NotNull(owner);
        Plant = Ensure.NotNull(plant);
        HeldPosition = heldPosition;
        _animations = animations;
    }

    /// <summary>The room's plants.</summary>
    private readonly SamusEaterObjects _owner;

    /// <summary>The plant's animations, from its authored chew (#2480).</summary>
    private readonly SamusEaterAnimations _animations;

    /// <summary>The plant's mounting and authored chew.</summary>
    internal SamusEaterObjectDefinition Plant { get; }

    /// <summary>Where the plant holds Samus.</summary>
    internal Position16<RoomPixels> HeldPosition { get; }

    /// <summary>Where the plant is in its course.</summary>
    internal SamusEaterPhase Phase { get; set; } = SamusEaterPhase.Biting;

    /// <summary>Whether the plant holds Samus: while it chews, from the end of its first update's bite until it releases her.</summary>
    internal bool Holding => Phase == SamusEaterPhase.Chewing;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (Holding)
            _owner.HoldSamus(HeldPosition);
        base.Update(place, context);
    }

    /// <summary>What <see cref="SequenceEnded"/> does for each <see cref="SamusEaterPhase"/> member, each member's own arm.</summary>
    private IdentityTable<SamusEaterPhase, Action<RoomObjectPlace, RoomObjectStepContext>> SequenceEndedForSamusEaterPhase => field ??= new(new Dictionary<SamusEaterPhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [SamusEaterPhase.Biting] = (place, context) =>
        {
            Phase = SamusEaterPhase.Chewing;
            Play(_animations.Chewing, place, context);
        },
        [SamusEaterPhase.Chewing] = (place, context) =>
        {
            _owner.ReleaseSamus();
            Phase = SamusEaterPhase.Released;
            Play(_animations.Released, place, context);
        },
        [SamusEaterPhase.Released] = (place, context) =>
        {
            Phase = SamusEaterPhase.Resting;
            Play(_animations.Idle, place, context);
        },
        [SamusEaterPhase.Resting] = (place, context) => context.RoomObjects.DeleteObject(place),
    });

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        SequenceEndedForSamusEaterPhase[Phase](place, context);
    }

    /// <summary>What <c>Show</c> does for each <see cref="SamusEaterBite"/> member, each member's own arm.</summary>
    private IdentityTable<SamusEaterBite, Action<RoomObjectStepContext>> ShowForSamusEaterBite => field ??= new(new Dictionary<SamusEaterBite, Action<RoomObjectStepContext>>
    {
        [SamusEaterBite.None] = context =>
        {
        },
        [SamusEaterBite.SoundAndDamage] = context =>
        {
            context.QueueSound(SamusEaterObjectDefinitions.ChewSound, SamusEaterObjectDefinitions.ChewSoundPriority);
            _owner.DamageSamus();
        },
        [SamusEaterBite.Damage] = context => _owner.DamageSamus(),
    });

    /// <summary>Bites as <paramref name="frame"/> asks, then shows it.</summary>
    private protected override void Show(SamusEaterChewFrame frame, RoomObjectPlace place, RoomObjectStepContext context)
    {
        ShowForSamusEaterBite[frame.BiteBefore](context);
        context.Draw(place, SamusEaterObjectDraws.Family, frame.Image);
    }
}
