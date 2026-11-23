using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The Hibashi species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed partial class HibashiBehavior : IEnemySpecies, IEnemySpawnFactory<HibashiSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Hibashi];

    /// <summary>The Hibashi's flame, or the collision part its placement names (#2342).</summary>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, HibashiSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        settings is HibashiFlameSpawnSettings flame
            ? EnemyOccupant.Running(new HibashiEntity(this, occupant, place, common, flame))
            : EnemyOccupant.PlacedOnly(new HibashiCollisionPartEntity(occupant, place, common));

    /// <summary>The room's enemy places, where each visible geyser finds its hitbox when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Receives each eruption's sound in the order the game queues them.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds paired Hibashi parts to the room's enemy places and the enemy audio queue.</summary>
    internal HibashiBehavior(EnemyPlaces places, EnemyAudioQueue audio)
    {
        _places = places;
        _audio = audio;
    }

    /// <summary>What <see cref="ApplyHibashiCue"/> does for each <see cref="HibashiCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<HibashiCueKind, Action<RoomEnemyPlace, HibashiEnemyState>> ApplyHibashiCueForHibashiCueKind => field ??= new(new Dictionary<HibashiCueKind, Action<RoomEnemyPlace, HibashiEnemyState>>
    {
        [HibashiCueKind.PlayEruptionSound] = (_, _) => PlayHibashiEruptionSound(),
        [HibashiCueKind.FinishActivity] = FinishHibashiActivity,
    });

    /// <summary>Carries out one cue of the Hibashi's eruption: its sound, an eruption frame's hitbox, or finishing.</summary>
    private void ApplyHibashiCue(RoomEnemyPlace place, HibashiEnemyState hibashiEnemyState, HibashiCue cue)
    {
        cue.React(ApplyHibashiCueReactions, new ApplyHibashiCueContext(place, hibashiEnemyState, this));
    }

    /// <summary>What <see cref="ApplyHibashiCue"/> does on each <see cref="HibashiCue"/>, built once (#2553).</summary>
    private static HibashiCueReactions<ApplyHibashiCueContext> ApplyHibashiCueReactions { get; } = new(
        EruptionStep: static (step, context) => ApplyHibashiEruptionStep(context.State, step.Step),
        Signal: static (signal, context) =>
        {
            context.Owner.ApplyHibashiCueForHibashiCueKind[signal.Kind](context.Place, context.State);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The flame's state the cases read.</param>
    /// <param name="Owner">The <see cref="HibashiBehavior"/> reacting.</param>
    private readonly partial record struct ApplyHibashiCueContext(RoomEnemyPlace Place, HibashiEnemyState State, HibashiBehavior Owner);

    /// <summary>Initializes the collision part, which holds the one-frame hitbox animation.</summary>
    /// <returns>The state the collision part holds from its setup.</returns>
    internal static HibashiCollisionPartState InitializeHibashiCollisionPart(RoomEnemyPlace place) =>
        new()
        {
            Playback = EnemyAnimationPlayback.Start(place, HibashiAnimations.StartingWith(HibashiSegment.Hitbox)),
        };

    /// <summary>
    /// Initializes the flame: it sets up its first wait (<see cref="HibashiDefinitions.FirstWait"/>) and the collision part
    /// it finds, and starts the eruption animation, whose first step plays on its first update.
    /// </summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal HibashiEnemyState InitializeHibashi(RoomEnemyPlace place)
    {
        var state = new HibashiEnemyState(HibashiAction.Inactive,
            hitbox: _places.Member(place, HibashiPartListingDefinitions.Listing, HibashiMember.Flame, HibashiMember.Hitbox));
        state.InactiveTimer = UpdateCountdown.For(HibashiDefinitions.FirstWait);
        state.Playback = EnemyAnimationPlayback.Start(place, HibashiAnimations.StartingWith(HibashiSegment.Flame));
        state.SpawnY = place.Position.Y.Whole;

        // The visible part must never participate through its definition's radius: its width is a real zero, which the
        // overlap tests measure as they measure any width. Its following part receives the dynamic radius from animation cues.
        place.Radius = place.Radius.WithX(0);
        return state;
    }

    /// <summary>What <see cref="RunHibashiMain"/> does for each <see cref="HibashiAction"/> member, each member's own arm.</summary>
    private static IdentityTable<HibashiAction, Action<HibashiFlameSpawnSettings, RoomEnemyPlace, HibashiEnemyState>> RunHibashiMainForHibashiAction => field ??= new(new Dictionary<HibashiAction, Action<HibashiFlameSpawnSettings, RoomEnemyPlace, HibashiEnemyState>>
    {
        [HibashiAction.Inactive] = (_, place, state) => RunHibashiInactive(place, state),
        [HibashiAction.Active] = (_, place, state) => { },
        [HibashiAction.Finished] = (settings, place, state) => RestHibashi(settings, state),
    });

    /// <summary>Runs the Hibashi's main AI.</summary>
    private static void RunHibashiMain(HibashiFlameSpawnSettings hibashiFlameSpawnSettings, RoomEnemyPlace place, HibashiEnemyState state)
    {
        RunHibashiMainForHibashiAction[state.Action](hibashiFlameSpawnSettings, place, state);
    }

    /// <summary>Runs the inactive Hibashi.</summary>
    private static void RunHibashiInactive(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        // The wait lasts its whole duration: the update that takes it to zero erupts (#2340).
        if (!state.InactiveTimer.StepEnds())
            return;

        RoomEnemyPlace hitbox = state.Hitbox;
        state.Action = HibashiAction.Active;
        state.Playback = EnemyAnimationPlayback.Start(flame, HibashiAnimations.StartingWith(HibashiSegment.Flame));

        // The flame shows, and its unseen hitbox part starts touching Samus, her shots and the grapple: the hitbox
        // part stays unseen through the whole eruption.
        flame.Presence = flame.Presence.Without(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.Without(EnemyContact.IgnoresSamus);
    }

    /// <summary>
    /// Rests the flame whose eruption has finished, which hid it: its wait restarts at its placement's inactive time and the
    /// update it runs out on (#2340).
    /// </summary>
    internal static void RestHibashi(HibashiFlameSpawnSettings hibashiFlameSpawnSettings, HibashiEnemyState state)
    {
        state.InactiveTimer = UpdateCountdown.For(hibashiFlameSpawnSettings.InactiveTime + EnemyWaitDefinitions.RunOutUpdate);
        state.Action = HibashiAction.Inactive;
    }

    /// <summary>Animation cue: plays the eruption sound.</summary>
    private void PlayHibashiEruptionSound()
    {
        // Each geyser queues its own sound; two erupting in one frame queue two.
        _audio.QueueEnemySound(HibashiDefinitions.EruptionSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Runs a rising cue: moves the collision part to the step's height above the spawn and gives it the step's
    /// half-height, and the first step its half-width.
    /// </summary>
    internal static void ApplyHibashiEruptionStep(HibashiEnemyState state, HibashiEruptionStep step)
    {
        RoomEnemyPlace hitbox = state.Hitbox;
        hitbox.Position = hitbox.Position.WithWholeY(state.SpawnY - step.YOffset);
        hitbox.Radius = hitbox.Radius.WithY(step.YRadius);
        if (step.Width == HibashiStepWidth.Restored)
            hitbox.Radius = hitbox.Radius.WithX(HibashiDefinitions.RestoredHalfWidth);
    }

    /// <summary>
    /// Animation cue: finishes the eruption (#2439). The flame hides at its spawn height, its hitbox stops touching anything,
    /// and it has finished: its next update rests it.
    /// </summary>
    private static void FinishHibashiActivity(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        RoomEnemyPlace hitbox = state.Hitbox;

        state.Action = HibashiAction.Finished;
        hitbox.Radius = new(0, 0);
        flame.Position = flame.Position.WithWholeY(state.SpawnY);
        flame.Presence = flame.Presence.With(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs the flame's main AI.</summary>
    internal static void RunHibashiMainAi(HibashiFlameSpawnSettings hibashiFlameSpawnSettings, RoomEnemyPlace place, HibashiEnemyState hibashiEnemyState) =>
        RunHibashiMain(hibashiFlameSpawnSettings, place, hibashiEnemyState);

    /// <summary>Advances the collision part's hitbox hold.</summary>
    internal static void AdvanceHibashiCollisionPartTypedAnimation(RoomEnemyPlace place, HibashiCollisionPartState hibashiCollisionPartState) =>
        EnemyAnimationPlayback.Advance(place, hibashiCollisionPartState, HibashiAnimations.Segments,
            static (HibashiCollisionPartState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Hibashi collision part's animation raised {cue}."));

    /// <summary>Advances the flame's eruption animation.</summary>
    internal void AdvanceHibashiTypedAnimation(RoomEnemyPlace place, HibashiEnemyState hibashiEnemyState) =>
        EnemyAnimationPlayback.Advance(place, hibashiEnemyState, HibashiAnimations.Segments,
            static (HibashiEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyHibashiCue(place, hibashiEnemyState, cue);
                return null;
            });
}
