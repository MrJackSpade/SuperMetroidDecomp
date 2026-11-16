using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>The nuclear waffle species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class NuclearWaffleBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.NuclearWaffle];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new NuclearWaffleEntity(this, place, common);

    /// <summary>Allocates the four collidable links in the articulated body.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Allocates visual body links and turn overlays.</summary>
    private readonly RoomEffectActors _effectActors;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Captures the enemy projectiles and effect actors used for body links.</summary>
    internal NuclearWaffleBehavior(EnemyProjectileEngine projectiles, RoomEffectActors effectActors, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
        _effectActors = effectActors;
    }

    /// <summary>Initializes the Nuclear Waffle.</summary>
    internal void InitializeNuclearWaffle(RoomEnemyPlace place)
    {
        var state = new NuclearWaffleEnemyState(NuclearWaffleAction.Waiting);
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, NuclearWaffleAnimations.StartingWith(NuclearWaffleSegment.BodyLoop));

        NuclearWaffleSpawnSettings settings = place.SpawnSettings<NuclearWaffleSpawnSettings>();
        state.OrbitRadius = settings.OrbitRadius;
        state.WaitingTimerReset = settings.WaitTime;
        state.WaitingTimer = state.WaitingTimerReset;

        NuclearWaffleSweepDefinition sweep = NuclearWaffleSweeps.Sweep(settings.Direction);
        state.SweepsForward = settings.Direction == NuclearWaffleSweepDirection.Forward;
        state.SweepStartAngle = sweep.StartAngle;
        state.SweepAngle = sweep.StartAngle;
        state.SweepEndAngle = sweep.EndAngle;
        state.SegmentSpacing = sweep.SegmentSpacing;
        state.InterleavedSegmentOffset = sweep.InterleavedSegmentOffset;
        state.RisingTurnThreshold = sweep.RisingTurnThreshold;
        state.FallingTurnThreshold = sweep.FallingTurnThreshold;

        // A backward sweep turns at the placement's angular speed, negated.
        state.AngularSpeed = state.SweepsForward ? settings.AngularSpeed : -settings.AngularSpeed;

        state.Origin = place.Position.Whole;
        PositionNuclearWaffleHead(place, state, state.SweepStartAngle);
        state.InitialHead = place.Position.Whole;
        state.Graphics = SpriteGraphics.SpawnedBy(place);

        SpawnNuclearWaffleProjectileSegments(place, state);
        SpawnNuclearWaffleSpriteSegments(place, state);
    }

    /// <summary>Runs the Nuclear Waffle's main AI.</summary>
    private void RunNuclearWaffleMain(RoomEnemyPlace place, NuclearWaffleEnemyState state)
    {
        switch (state.Action)
        {
            case NuclearWaffleAction.Waiting:
                RunNuclearWaffleWaiting(place, state);
                return;

            case NuclearWaffleAction.Sweeping:
                RunNuclearWaffleSweep(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Nuclear Waffle has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Waits before a sweep.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemyPlace place,
        NuclearWaffleEnemyState state)
    {
        if (--state.WaitingTimer >= 0)
            return;

        state.WaitingTimer = state.WaitingTimerReset;
        state.SweepAngle = state.SweepStartAngle;
        state.Action = NuclearWaffleAction.Sweeping;
        state.HeadOrientationState = NuclearWaffleTurnState.BeforeTurns;
        Array.Clear(state.ProjectileOrientationStates);
        Array.Clear(state.SpriteOrientationStates);
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>Runs a sweep.</summary>
    private void RunNuclearWaffleSweep(RoomEnemyPlace place, NuclearWaffleEnemyState state)
    {
        NuclearWaffleOrientation headOrientation = GetNuclearWaffleOrientation(
            state,
            state.CurrentAngle);
        if (headOrientation.State != state.HeadOrientationState)
        {
            SpawnNuclearWaffleTurnEffects(
                place.Position.Whole,
                state.Graphics,
                headOrientation);
            state.HeadOrientationState = headOrientation.State;
        }

        (NuclearWaffleSweepAngle clampedHeadAngle, _) = ClampNuclearWaffleAngle(state, state.CurrentAngle);
        PositionNuclearWaffleHead(place, state, clampedHeadAngle);

        bool sweepFinished = PositionNuclearWaffleProjectileSegments(state);
        PositionNuclearWaffleSpriteSegments(state);
        AddNuclearWaffleAngularSpeed(state);
        if (!sweepFinished)
            return;

        state.Action = NuclearWaffleAction.Waiting;
        place.Processing = place.Processing.Without(EnemyProcessing.OffScreen);
    }

    /// <summary>Allocates the four damaging projectile links in association order.</summary>
    private void SpawnNuclearWaffleProjectileSegments(
        RoomEnemyPlace owner,
        NuclearWaffleEnemyState state)
    {
        // Each of the four links is spawned for its own place along the waffle's arm, in the order the update later
        // walks them from the last.
        for (int segmentIndex = 0; segmentIndex < state.ProjectileSegments.Length; segmentIndex++)
        {
            EnemyProjectile? segment = _projectiles.AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // Spawning installs the link's definition before
            // its initializer runs. That definition owns the twelve-frame animation, the link's
            // frame behaviour, 8x8 radii, interaction flags, and 64 damage. The
            // initializer below only copies the live owner coordinates, associates this
            // projectile with one articulated link, and makes it an indestructible dud.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                segment,
                RoomEnemyProjectileKind.NuclearWaffleBody,
                EnemyProjectileBehavior.Still,
                state.Graphics);
            segment.StartAnimation(new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Initial));
            segment.Position = owner.Position.WithWhole(state.InitialHead);
            // The initializer makes these four links indestructible duds. Their
            // shot-blocking property admits the beam overlap; this independent flag selects the persistent
            // dud response rather than the definition's delete-on-shot animation.
            segment.CollisionOption = EnemyProjectileCollisionOption.IndestructibleDud;
            state.ProjectileSegments[segmentIndex] = segment;
        }
    }

    /// <summary>Allocates the noncolliding sprite links that interleave with projectile links.</summary>
    private void SpawnNuclearWaffleSpriteSegments(
        RoomEnemyPlace place,
        NuclearWaffleEnemyState state)
    {
        for (int segmentIndex = 0; segmentIndex < state.SpriteSegments.Length; segmentIndex++)
        {
            state.SpriteSegments[segmentIndex] = _effectActors.SpawnRoomEffectActor(
                place.Position.Whole,
                RoomEffectActorKind.NuclearWaffleBody,
                state.Graphics);
        }
    }

    /// <summary>Runs the projectile-link loop.</summary>
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle.TurnedBySteps(state.InterleavedSegmentOffset);
        bool finished = false;
        for (int index = 0; index < state.ProjectileSegments.Length; index++)
        {
            angle = angle.TurnedBySteps(-state.SegmentSpacing);
            EnemyProjectile segment = state.ProjectileSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle projectile link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.ProjectileOrientationStates[index])
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                state.ProjectileOrientationStates[index] = orientation.State;
            }

            (NuclearWaffleSweepAngle clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
            // The sweep tests the flag left by the final link's second clamp call. It is
            // deliberately not an OR across the chain: the head and leading links can sit
            // at the endpoint while the final projectile link is still unfurling.
            finished = linkFinished;
        }

        return finished;
    }

    /// <summary>Runs the effect-actor-link loop.</summary>
    private void PositionNuclearWaffleSpriteSegments(NuclearWaffleEnemyState state)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle;
        for (int index = 0; index < state.SpriteSegments.Length; index++)
        {
            angle = angle.TurnedBySteps(-state.SegmentSpacing);
            RoomEffectActor segment = state.SpriteSegments[index]
                ?? throw new InvalidOperationException("Nuclear Waffle effect-actor link was not allocated.");
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != state.SpriteOrientationStates[index])
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                state.SpriteOrientationStates[index] = orientation.State;
            }

            (NuclearWaffleSweepAngle clampedAngle, _) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
        }
    }

    /// <summary>
    /// Clamps a link angle and reports whether the sweep finished. The sweep's angles count on past the full turn, so
    /// they compare in order along it.
    /// </summary>
    private static NuclearWaffleAngleClamp ClampNuclearWaffleAngle(
        NuclearWaffleEnemyState state,
        NuclearWaffleSweepAngle angle)
    {
        if (!state.SweepsForward)
        {
            if (angle >= state.SweepEndAngle)
            {
                return angle < state.SweepStartAngle
                    ? new NuclearWaffleAngleClamp(angle, false)
                    : new NuclearWaffleAngleClamp(state.SweepStartAngle, false);
            }
            return new NuclearWaffleAngleClamp(state.SweepEndAngle, true);
        }

        if (angle >= state.SweepEndAngle)
            return new NuclearWaffleAngleClamp(state.SweepEndAngle, true);
        return angle < state.SweepStartAngle
            ? new NuclearWaffleAngleClamp(state.SweepStartAngle, false)
            : new NuclearWaffleAngleClamp(angle, false);
    }

    /// <summary>Chooses a link's orientation.</summary>
    private static NuclearWaffleOrientation GetNuclearWaffleOrientation(
        NuclearWaffleEnemyState state,
        NuclearWaffleSweepAngle angle)
    {
        if (state.SweepsForward)
        {
            if (angle >= state.FallingTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Counterclockwise, NuclearWaffleTurnState.PastFallingThreshold);
            if (angle >= state.RisingTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastRisingThreshold);
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
        }

        if (angle < state.FallingTurnThreshold)
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastFallingThreshold);
        if (angle < state.RisingTurnThreshold)
            return new NuclearWaffleOrientation(RotationDirection.Counterclockwise, NuclearWaffleTurnState.PastRisingThreshold);
        return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
    }

    /// <summary>Emits a turn overlay and direction sprite, queuing sound on the rising turn.</summary>
    private void SpawnNuclearWaffleTurnEffects(
        Position16<RoomPixels> position,
        SpriteGraphics graphics,
        NuclearWaffleOrientation orientation)
    {
        _ = _effectActors.SpawnRoomEffectActor(
            position,
            RoomEffectActorKind.NuclearWaffleTurnOverlay,
            graphics);
        _ = _effectActors.SpawnRoomEffectActor(
            position,
            orientation.Turn == RotationDirection.Counterclockwise
                ? RoomEffectActorKind.NuclearWaffleTurnCounterClockwise
                : RoomEffectActorKind.NuclearWaffleTurnClockwise,
            graphics);

        // The falling turn is the silent second half-turn; every other change queues the turn sound.
        if (orientation.State != NuclearWaffleTurnState.PastFallingThreshold)
            _audio.QueueEnemySound(NuclearWaffleDefinitions.TurnSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Places the head at the requested angle around its captured room origin.</summary>
    private static void PositionNuclearWaffleHead(
        RoomEnemyPlace place,
        NuclearWaffleEnemyState state,
        NuclearWaffleSweepAngle angle)
    {
        place.Position = place.Position.WithWhole(NuclearWaffleOrbitPoint(state, angle));
    }

    /// <summary>
    /// The whole-pixel point on the orbit at <paramref name="angle"/>, wrapping at sixteen bits. The orbit reads only the
    /// angle's direction within one turn.
    /// </summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(NuclearWaffleEnemyState state, NuclearWaffleSweepAngle angle) =>
        state.Origin + EnemyMath.CircleOffset(angle.ToRotationAngle().ToTrigonometryAngle(), state.OrbitRadius);

    /// <summary>Advances the sweep angle by the angular speed, carrying from its fraction.</summary>
    private static void AddNuclearWaffleAngularSpeed(NuclearWaffleEnemyState state) =>
        state.SweepAngle += state.AngularSpeed;

    /// <summary>Advances the Nuclear Waffle's typed animation.</summary>
    internal static void AdvanceNuclearWaffleTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        NuclearWaffleEnemyState state = place.SpeciesState<NuclearWaffleEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy place {place.Ordinal} has no initialized Nuclear Waffle state.");
        AnimationPlayback<NuclearWaffleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, NuclearWaffleAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"Nuclear Waffle's cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Nuclear Waffle's main AI.</summary>
    internal void RunNuclearWaffleMainAi(RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunNuclearWaffleMain(place, place.RequireSpeciesState<NuclearWaffleEnemyState>());
    }
}
