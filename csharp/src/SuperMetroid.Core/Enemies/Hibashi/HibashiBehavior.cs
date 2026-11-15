using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The Hibashi species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class HibashiBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Hibashi];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new HibashiEntity(this, slot, common);

    /// <summary>The room's enemy places, where each visible geyser finds its hitbox when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The hitbox part each visible geyser found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _hitboxes = new();
    /// <summary>Receives each eruption's sound in call order, as the game queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds paired Hibashi parts to the room enemy slot table and the enemy audio queue.</summary>
    internal HibashiBehavior(EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _slotTable = slotTable;
        _audio = audio;
    }

    /// <summary>Carries out one cue of the Hibashi's eruption: its sound, an eruption frame's hitbox, or finishing.</summary>
    private HibashiSegment? ApplyHibashiCue(RoomEnemySlot slot, HibashiCue cue)
    {
        switch (cue.Kind)
        {
            case HibashiCueKind.PlayEruptionSound:
                PlayHibashiEruptionSound();
                break;
            case HibashiCueKind.EruptionStep:
                ApplyHibashiEruptionStep(slot, cue.Step ?? throw new InvalidOperationException("A Hibashi rising cue names no eruption step."));
                break;
            case HibashiCueKind.FinishActivity:
                FinishHibashiActivity(slot);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(HibashiCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Initializes the Hibashi.</summary>
    internal void InitializeHibashi(RoomEnemySlot slot)
    {
        HibashiSpawnSettings settings = slot.SpawnSettings<HibashiSpawnSettings>();

        // Every part starts with the one-frame hitbox map. Only the graphics part replaces it with
        // the eruption animation and initializes the state-machine values below.
        AnimationPlayback<HibashiSegment> hitboxMap = EnemyAnimationPlayback.Start(slot, HibashiAnimations.StartingWith(HibashiSegment.Hitbox));
        if (settings.Part == HibashiPart.CollisionPart)
        {
            slot.SetSpeciesState(new HibashiCollisionPartState { Playback = hitboxMap });
            return;
        }

        var state = new HibashiEnemyState(HibashiAction.Inactive) { InactiveTimerReset = settings.InactiveTime };
        slot.SetSpeciesState(state);
        _hitboxes.Place(slot, _slotTable.ListedAfter(slot, 1));
        state.Playback = new(HibashiAnimations.StartingWith(HibashiSegment.Graphics), AnimationPlace.StartOfAnimation);
        state.SpawnY = slot.Position.Y.Whole;

        // The visible part must never participate through its definition's radius.
        // Its following part receives the dynamic radius from animation cues.
        slot.Radius = slot.Radius.WithX(0);
    }

    /// <summary>Runs the Hibashi's main AI.</summary>
    private void RunHibashiMain(RoomEnemySlot slot, HibashiEnemyState state)
    {
        switch (state.Action)
        {
            case HibashiAction.Inactive:
                RunHibashiInactive(slot, state);
                return;

            case HibashiAction.Active:
                RunHibashiActive(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Hibashi has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the inactive Hibashi.</summary>
    private void RunHibashiInactive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        // The countdown expires through zero, so timer zero deliberately becomes -1 and erupts immediately. A
        // reset value N later produces N+1 inactive actor frames because zero is nonnegative.
        if (--state.InactiveTimer >= 0)
            return;

        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        state.Action = HibashiAction.Active;
        state.ActivityFinished = false;
        state.Playback = EnemyAnimationPlayback.Start(graphics, HibashiAnimations.StartingWith(HibashiSegment.Graphics));

        // The invisibility property controls rendering only. The intangibility property controls admission to
        // Samus/projectile/grapple collision. The bottom remains visually invisible for the
        // entire eruption while becoming a live collision body here.
        graphics.Presence = graphics.Presence.Without(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.Without(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs the active Hibashi.</summary>
    private static void RunHibashiActive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        if (!state.ActivityFinished)
            return;

        state.InactiveTimer = state.InactiveTimerReset;
        graphics.Presence = graphics.Presence.With(EnemyPresence.Hidden);
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
    /// half-height, and the first step its eight-pixel half-width.
    /// </summary>
    internal void ApplyHibashiEruptionStep(RoomEnemySlot graphics, HibashiEruptionStep step)
    {
        HibashiEnemyState state = graphics.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        hitbox.Position = hitbox.Position.WithWrappedWholeY(
            state.SpawnY - step.YOffset);
        hitbox.Radius = hitbox.Radius.WithY(step.YRadius);
        if (step.Width == HibashiStepWidth.Restored)
            hitbox.Radius = hitbox.Radius.WithX(8);
    }

    /// <summary>Animation cue: finishes the activity.</summary>
    private void FinishHibashiActivity(RoomEnemySlot graphics)
    {
        HibashiEnemyState state = graphics.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);

        state.ActivityFinished = true;
        hitbox.Radius = new(0, 0);
        graphics.Position = graphics.Position.WithWholeY(state.SpawnY);
        graphics.Presence = graphics.Presence.With(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>The hitbox part the visible geyser found when it was placed; retail populations always list one right after it.</summary>
    private RoomEnemySlot GetFollowingHibashiPart(RoomEnemySlot graphics) => _hitboxes.Of(graphics);

    /// <summary>Runs Hibashi's main AI.</summary>
    internal void RunHibashiMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        // The collision part takes no action of its own; its graphics part moves it.
        if (IsCollisionPart(slot))
            return;
        RunHibashiMain(slot, slot.RequireSpeciesState<HibashiEnemyState>());
    }

    /// <summary>Whether <paramref name="slot"/>'s placement makes it a Hibashi's collision part.</summary>
    private static bool IsCollisionPart(RoomEnemySlot slot) =>
        slot.SpawnSettings<HibashiSpawnSettings>().Part == HibashiPart.CollisionPart;

    /// <summary>Advances Hibashi's typed animation: the graphics part's eruption, or the collision part's hitbox hold.</summary>
    internal void AdvanceHibashiTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (IsCollisionPart(slot))
        {
            EnemyAnimationPlayback.Advance(slot, HibashiAnimations.Segments,
                static (HibashiCollisionPartState state) => state.Playback, static (state, playback) => state.Playback = playback,
                static (_, cue) => throw new InvalidOperationException($"The Hibashi collision part's animation raised {cue}."));
            return;
        }
        EnemyAnimationPlayback.Advance(slot, HibashiAnimations.Segments,
            static (HibashiEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyHibashiCue(slot, cue) is { } target
                ? new AnimationContinuation<HibashiSegment>(HibashiAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
    }
}
