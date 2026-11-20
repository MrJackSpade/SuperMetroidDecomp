using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The Hibashi species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class HibashiBehavior : IEnemySpecies, IEnemySpawnFactory<HibashiSpawnSettings>
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
    /// <summary>The hitbox part each visible geyser found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _hitboxes = new();
    /// <summary>Receives each eruption's sound in the order the game queues them.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds paired Hibashi parts to the room's enemy places and the enemy audio queue.</summary>
    internal HibashiBehavior(EnemyPlaces places, EnemyAudioQueue audio)
    {
        _places = places;
        _audio = audio;
    }

    /// <summary>Carries out one cue of the Hibashi's eruption: its sound, an eruption frame's hitbox, or finishing.</summary>
    private void ApplyHibashiCue(RoomEnemyPlace place, HibashiEnemyState hibashiEnemyState, HibashiCue cue)
    {
        switch (cue)
        {
            case HibashiCue.Signal { Kind: HibashiCueKind.PlayEruptionSound }:
                PlayHibashiEruptionSound();
                break;
            case HibashiCue.EruptionStep step:
                ApplyHibashiEruptionStep(place, hibashiEnemyState, step.Step);
                break;
            case HibashiCue.Signal { Kind: HibashiCueKind.FinishActivity }:
                FinishHibashiActivity(place, hibashiEnemyState);
                break;
            default:
                throw new InvalidOperationException($"Undefined Hibashi cue {cue}.");
        }
    }

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
        var state = new HibashiEnemyState(HibashiAction.Inactive);
        _hitboxes.Place(place, _places.Member(place, HibashiPartListingDefinitions.Listing, HibashiMember.Flame, HibashiMember.Hitbox));
        state.InactiveTimer = UpdateCountdown.For(HibashiDefinitions.FirstWait);
        state.Playback = EnemyAnimationPlayback.Start(place, HibashiAnimations.StartingWith(HibashiSegment.Flame));
        state.SpawnY = place.Position.Y.Whole;

        // The visible part must never participate through its definition's radius: its width is a real zero, which the
        // overlap tests measure as they measure any width. Its following part receives the dynamic radius from animation cues.
        place.Radius = place.Radius.WithX(0);
        return state;
    }

    /// <summary>Runs the Hibashi's main AI.</summary>
    private void RunHibashiMain(RoomEnemyOccupant occupant, HibashiFlameSpawnSettings hibashiFlameSpawnSettings, RoomEnemyPlace place, HibashiEnemyState state)
    {
        switch (state.Action)
        {
            case HibashiAction.Inactive:
                RunHibashiInactive(place, state);
                return;

            case HibashiAction.Active:
                return;

            case HibashiAction.Finished:
                RestHibashi(occupant, hibashiFlameSpawnSettings, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Hibashi has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the inactive Hibashi.</summary>
    private void RunHibashiInactive(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        // The wait lasts its whole duration: the update that takes it to zero erupts (#2340).
        if (!state.InactiveTimer.StepEnds())
            return;

        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);
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
    private static void RestHibashi(RoomEnemyOccupant occupant, HibashiFlameSpawnSettings hibashiFlameSpawnSettings, HibashiEnemyState state)
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
    private void ApplyHibashiEruptionStep(RoomEnemyPlace flame, HibashiEnemyState state, HibashiEruptionStep step)
    {
        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);
        hitbox.Position = hitbox.Position.WithWholeY(state.SpawnY - step.YOffset);
        hitbox.Radius = hitbox.Radius.WithY(step.YRadius);
        if (step.Width == HibashiStepWidth.Restored)
            hitbox.Radius = hitbox.Radius.WithX(HibashiDefinitions.RestoredHalfWidth);
    }

    /// <summary>
    /// Animation cue: finishes the eruption (#2439). The flame hides at its spawn height, its hitbox stops touching anything,
    /// and it has finished: its next update rests it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The flame is not erupting.</exception>
    private void FinishHibashiActivity(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);

        if (state.Action != HibashiAction.Active)
            throw new InvalidOperationException($"A Hibashi's eruption finished while it was {state.Action}.");
        state.Action = HibashiAction.Finished;
        hitbox.Radius = new(0, 0);
        flame.Position = flame.Position.WithWholeY(state.SpawnY);
        flame.Presence = flame.Presence.With(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>The hitbox part the visible geyser found when it was placed; every room that has the geyser lists one right after it.</summary>
    private RoomEnemyPlace GetFollowingHibashiPart(RoomEnemyPlace flame) => _hitboxes.Of(flame);

    /// <summary>Runs the flame's main AI.</summary>
    internal void RunHibashiMainAi(RoomEnemyOccupant occupant, HibashiFlameSpawnSettings hibashiFlameSpawnSettings, RoomEnemyPlace place, HibashiEnemyState hibashiEnemyState) =>
        RunHibashiMain(occupant, hibashiFlameSpawnSettings, place, hibashiEnemyState);

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
