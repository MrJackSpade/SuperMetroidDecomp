using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Maridia's glass tube (#1554). On its first update, a tube broken on an earlier visit restores the room's water and
/// leaves; otherwise it shows the intact tube. From its second update on, before its timing runs, a Power Bomb that
/// struck its block wakes it to wait for input, and any other strike plays the ineffective-shot sound; every strike is
/// then read. While it waits, a newly pressed accepted action breaks it on that update: Samus is locked, the crack
/// appears and the tube plays its cracking (#1994), then shatters with its sound, shards, bubbles and earthquake, and
/// once the open tube's time is up the broken event is set, the water returns, Samus is released and the tube leaves.
/// </summary>
/// <param name="owner">The room's tube owner, which reaches Samus, the events, the water, the earthquake and the debris.</param>
[RoomLifetime]
internal sealed class NoobTubeObject(NoobTubeObjects owner) : SilentSequencedRoomObject<NoobTubeSegment, TerrainChangeFrame>(NoobTubeAnimations.Segments), IShootableRoomObject
{
    /// <summary>The room's tube owner.</summary>
    private readonly NoobTubeObjects _owner = Ensure.NotNull(owner);

    /// <summary>Where the tube is in its course.</summary>
    private NoobTubePhase Phase { get; set; } = NoobTubePhase.Starting;

    /// <summary>The strike on the tube's block it has yet to take (#2562).</summary>
    private readonly StrikeAwaitingRoomObject<ResidentObjectProjectileHit> strike = new();

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => strike.Land(hit);

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        Wake(place, context);
        base.Update(place, context);
    }

    /// <summary>What <see cref="SequenceEnded"/> does for each <see cref="NoobTubePhase"/> member, each member's own arm.</summary>
    private IdentityTable<NoobTubePhase, Action<RoomObjectPlace, RoomObjectStepContext>> SequenceEndedForNoobTubePhase => field ??= new(new Dictionary<NoobTubePhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [NoobTubePhase.Starting] = (place, context) =>
        {
            if (_owner.IsBroken())
            {
                _owner.EnableWaterPhysics();
                context.RoomObjects.DeleteObject(place);
            }
            else
            {
                Phase = NoobTubePhase.WaitingForPowerBomb;
                Play(NoobTubeAnimations.Intact, place, context);
            }
        },
        // The tube rests until a wake starts its next step.
        [NoobTubePhase.WaitingForPowerBomb] = static (_, _) => { },
        [NoobTubePhase.WaitingForInput] = static (_, _) => { },
        [NoobTubePhase.Cracking] = (place, context) =>
        {
            context.QueueSound(NoobTubeObjectDefinitions.BreakSound, NoobTubeObjectDefinitions.BreakSoundPriority);
            _owner.SpawnShardsAndBubbles(place);
            _owner.TriggerEarthquake();
            Phase = NoobTubePhase.Shattered;
            Play(NoobTubeAnimations.Shattered, place, context);
        },
        [NoobTubePhase.Shattered] = (place, context) =>
        {
            _owner.SetBrokenEvent();
            _owner.EnableWaterPhysics();
            _owner.UnlockSamus();
            context.RoomObjects.DeleteObject(place);
        },
    });

    /// <inheritdoc/>
    private protected override void SequenceEnded(RoomObjectPlace place, RoomObjectStepContext context)
    {
        SequenceEndedForNoobTubePhase[Phase](place, context);
    }

    /// <inheritdoc/>
    private protected override void Show(TerrainChangeFrame frame, RoomObjectPlace place, RoomObjectStepContext context) =>
        context.Draw(place, frame.Draw);

    /// <summary>What <c>Wake</c> does for each <see cref="NoobTubePhase"/> member, each member's own arm.</summary>
    private IdentityTable<NoobTubePhase, Action<RoomObjectPlace, RoomObjectStepContext>> WakeForNoobTubePhase => field ??= new(new Dictionary<NoobTubePhase, Action<RoomObjectPlace, RoomObjectStepContext>>
    {
        [NoobTubePhase.WaitingForPowerBomb] = (place, context) =>
        {
            ResidentObjectProjectileHit? hit = strike.Take();
            if (hit?.Family == SamusProjectileFamily.PowerBomb)
            {
                Phase = NoobTubePhase.WaitingForInput;
                EndOnNextUpdate();
            }
            else if (hit is not null)
            {
                context.QueueSound(NoobTubeObjectDefinitions.IneffectiveShotSound, SoundRequestPriority.Normal);
            }
        },
        [NoobTubePhase.Starting] = (place, context) => { },
        [NoobTubePhase.WaitingForInput] = (place, context) =>
        {
            if ((context.Frame.ControllerNewInput & NoobTubeObjectDefinitions.AcceptedWakeInputMask) != GameplayAction.None)
            {
                // The break starts on this update.
                _owner.LockSamus();
                _owner.SpawnCrack(place);
                Phase = NoobTubePhase.Cracking;
                PlayFromNextUpdate(NoobTubeAnimations.Cracking);
            }
        },
        [NoobTubePhase.Cracking] = (place, context) => { },
        [NoobTubePhase.Shattered] = (place, context) => { },
    });

    /// <summary>
    /// Reads the update's strike or input while the tube waits for one, and starts the step it wakes: a Power Bomb's
    /// strike sets it waiting for input, and accepted input breaks it, locking Samus and spawning the crack before the
    /// cracking's first image shows on this same update.
    /// </summary>
    private void Wake(RoomObjectPlace place, RoomObjectStepContext context)
    {
        WakeForNoobTubePhase[Phase](place, context);
    }
}
