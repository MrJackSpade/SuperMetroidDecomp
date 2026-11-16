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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new HibashiEntity(this, place, common);

    /// <summary>The room's enemy places, where each visible geyser finds its hitbox when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The hitbox part each visible geyser found when it was placed, which its room lists right after it.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _hitboxes = new();
    /// <summary>Receives each eruption's sound in call order, as the game queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds paired Hibashi parts to the room's enemy places and the enemy audio queue.</summary>
    internal HibashiBehavior(EnemyPlaces places, EnemyAudioQueue audio)
    {
        _places = places;
        _audio = audio;
    }

    /// <summary>Carries out one cue of the Hibashi's eruption: its sound, an eruption frame's hitbox, or finishing.</summary>
    private HibashiSegment? ApplyHibashiCue(RoomEnemyPlace place, HibashiCue cue)
    {
        switch (cue)
        {
            case HibashiCue.Signal { Kind: HibashiCueKind.PlayEruptionSound }:
                PlayHibashiEruptionSound();
                break;
            case HibashiCue.EruptionStep step:
                ApplyHibashiEruptionStep(place, step.Step);
                break;
            case HibashiCue.Signal { Kind: HibashiCueKind.FinishActivity }:
                FinishHibashiActivity(place);
                break;
            default:
                throw new InvalidOperationException($"Undefined Hibashi cue {cue}.");
        }
        return null;
    }

    /// <summary>Initializes the Hibashi.</summary>
    internal void InitializeHibashi(RoomEnemyPlace place)
    {
        HibashiSpawnSettings settings = place.SpawnSettings<HibashiSpawnSettings>();

        // Every part starts with the one-frame hitbox animation. Only the flame replaces it with
        // the eruption animation and initializes the state-machine values below.
        AnimationPlayback<HibashiSegment> hitboxMap = EnemyAnimationPlayback.Start(place, HibashiAnimations.StartingWith(HibashiSegment.Hitbox));
        if (settings.Part == HibashiPart.CollisionPart)
        {
            place.SetSpeciesState(new HibashiCollisionPartState { Playback = hitboxMap });
            return;
        }

        var state = new HibashiEnemyState(HibashiAction.Inactive) { InactiveTimerReset = settings.InactiveTime };
        place.SetSpeciesState(state);
        _hitboxes.Place(place, _places.ListedAfter(place, 1));
        state.Playback = new(HibashiAnimations.StartingWith(HibashiSegment.Flame), AnimationPlace.StartOfAnimation);
        state.SpawnY = place.Position.Y.Whole;

        // The visible part must never participate through its definition's radius.
        // Its following part receives the dynamic radius from animation cues.
        place.Radius = place.Radius.WithX(0);
    }

    /// <summary>Runs the Hibashi's main AI.</summary>
    private void RunHibashiMain(RoomEnemyPlace place, HibashiEnemyState state)
    {
        switch (state.Action)
        {
            case HibashiAction.Inactive:
                RunHibashiInactive(place, state);
                return;

            case HibashiAction.Active:
                RunHibashiActive(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Hibashi has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the inactive Hibashi.</summary>
    private void RunHibashiInactive(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        // The countdown expires through zero, so timer zero deliberately becomes -1 and erupts immediately. A
        // reset value N later produces N+1 inactive actor frames because zero is nonnegative.
        if (--state.InactiveTimer >= 0)
            return;

        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);
        state.Action = HibashiAction.Active;
        state.ActivityFinished = false;
        state.Playback = EnemyAnimationPlayback.Start(flame, HibashiAnimations.StartingWith(HibashiSegment.Flame));

        // The invisibility property controls rendering only. The intangibility property controls admission to
        // Samus/projectile/grapple collision. The bottom remains visually invisible for the
        // entire eruption while becoming a live collision body here.
        flame.Presence = flame.Presence.Without(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.Without(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs the active Hibashi.</summary>
    private static void RunHibashiActive(RoomEnemyPlace flame, HibashiEnemyState state)
    {
        if (!state.ActivityFinished)
            return;

        state.InactiveTimer = state.InactiveTimerReset;
        flame.Presence = flame.Presence.With(EnemyPresence.Hidden);
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
    internal void ApplyHibashiEruptionStep(RoomEnemyPlace flame, HibashiEruptionStep step)
    {
        HibashiEnemyState state = flame.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);
        hitbox.Position = hitbox.Position.WithWrappedWholeY(
            state.SpawnY - step.YOffset);
        hitbox.Radius = hitbox.Radius.WithY(step.YRadius);
        if (step.Width == HibashiStepWidth.Restored)
            hitbox.Radius = hitbox.Radius.WithX(HibashiDefinitions.RestoredHalfWidth);
    }

    /// <summary>Animation cue: finishes the activity.</summary>
    private void FinishHibashiActivity(RoomEnemyPlace flame)
    {
        HibashiEnemyState state = flame.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemyPlace hitbox = GetFollowingHibashiPart(flame);

        state.ActivityFinished = true;
        hitbox.Radius = new(0, 0);
        flame.Position = flame.Position.WithWholeY(state.SpawnY);
        flame.Presence = flame.Presence.With(EnemyPresence.Hidden);
        hitbox.Contact = hitbox.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>The hitbox part the visible geyser found when it was placed; retail populations always list one right after it.</summary>
    private RoomEnemyPlace GetFollowingHibashiPart(RoomEnemyPlace flame) => _hitboxes.Of(flame);

    /// <summary>Runs Hibashi's main AI.</summary>
    internal void RunHibashiMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        // The collision part takes no action of its own; its graphics part moves it.
        if (IsCollisionPart(place))
            return;
        RunHibashiMain(place, place.RequireSpeciesState<HibashiEnemyState>());
    }

    /// <summary>Whether <paramref name="place"/>'s placement makes it a Hibashi's collision part.</summary>
    private static bool IsCollisionPart(RoomEnemyPlace place) =>
        place.SpawnSettings<HibashiSpawnSettings>().Part == HibashiPart.CollisionPart;

    /// <summary>Advances Hibashi's typed animation: the graphics part's eruption, or the collision part's hitbox hold.</summary>
    internal void AdvanceHibashiTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (IsCollisionPart(place))
        {
            EnemyAnimationPlayback.Advance(place, HibashiAnimations.Segments,
                static (HibashiCollisionPartState state) => state.Playback, static (state, playback) => state.Playback = playback,
                static (_, cue) => throw new InvalidOperationException($"The Hibashi collision part's animation raised {cue}."));
            return;
        }
        EnemyAnimationPlayback.Advance(place, HibashiAnimations.Segments,
            static (HibashiEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyHibashiCue(place, cue) is { } target
                ? new AnimationContinuation<HibashiSegment>(HibashiAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
    }
}
