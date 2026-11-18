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
        NuclearWaffleSpawnSettings settings = place.SpawnSettings<NuclearWaffleSpawnSettings>();
        NuclearWaffleSweepDefinition sweep = NuclearWaffleSweeps.Sweep(settings.Direction);

        // The head starts on the orbit at the sweep's start angle around the placement, and every link spawns there.
        Position16<RoomPixels> origin = place.Position.Whole;
        place.Position = place.Position.WithWhole(NuclearWaffleOrbitPoint(origin, settings.OrbitRadius, sweep.StartAngle));
        Position16<RoomPixels> initialHead = place.Position.Whole;
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(place);

        var state = new NuclearWaffleEnemyState(NuclearWaffleAction.Waiting)
        {
            ProjectileLinks = SpawnNuclearWaffleProjectileLinks(place, initialHead, graphics),
            SpriteLinks = SpawnNuclearWaffleSpriteLinks(place, graphics),
            OrbitRadius = settings.OrbitRadius,
            WaitingTimerReset = settings.WaitTime + EnemyWaitDefinitions.RunOutUpdate,
            WaitingTimer = settings.WaitTime + EnemyWaitDefinitions.RunOutUpdate,
            Direction = settings.Direction,
            Sweep = sweep,
            SweepAngle = sweep.StartAngle,
            // A backward sweep turns at the placement's angular speed, negated.
            AngularSpeed = settings.Direction == NuclearWaffleSweepDirection.Forward ? settings.AngularSpeed : -settings.AngularSpeed,
            Origin = origin,
            Graphics = graphics,
        };
        place.SetSpeciesState(state);
        state.Playback = EnemyAnimationPlayback.Start(place, NuclearWaffleAnimations.StartingWith(NuclearWaffleSegment.BodyLoop));
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
        if (--state.WaitingTimer > 0)
            return;

        state.WaitingTimer = state.WaitingTimerReset;
        state.SweepAngle = state.Sweep.StartAngle;
        state.Action = NuclearWaffleAction.Sweeping;
        state.RestartTurns();
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

    /// <summary>Allocates the four damaging projectile links in association order, each at the head's starting point.</summary>
    private NuclearWaffleLink<EnemyProjectile>[] SpawnNuclearWaffleProjectileLinks(
        RoomEnemyPlace owner,
        Position16<RoomPixels> initialHead,
        SpriteGraphics graphics)
    {
        // Each of the four links is spawned for its own place along the waffle's arm, in the order the update later
        // walks them from the last.
        var links = new NuclearWaffleLink<EnemyProjectile>[NuclearWaffleDefinitions.ProjectileLinks];
        for (int segmentIndex = 0; segmentIndex < links.Length; segmentIndex++)
        {
            EnemyProjectile? segment = _projectiles.AllocateEnemyProjectile();
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // The link starts as its definition has it: its twelve-frame animation, an 8 by 8 pixel size and 64
            // damage. It then starts at the head as one link of the body, which shots cannot destroy.
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                segment,
                RoomEnemyProjectileKind.NuclearWaffleBody,
                EnemyProjectileBehavior.Still,
                graphics);
            segment.StartAnimation(new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Initial));
            segment.Position = owner.Position.WithWhole(initialHead);
            // The four links stop shots without being destroyed: a beam that strikes one is a dud, and the link
            // stays rather than playing its definition's destruction.
            segment.CollisionOption = EnemyProjectileCollisionOption.IndestructibleDud;
            links[segmentIndex] = new NuclearWaffleLink<EnemyProjectile>(segment);
        }
        return links;
    }

    /// <summary>Allocates the noncolliding sprite links that interleave with projectile links, each at the head.</summary>
    private NuclearWaffleLink<RoomEffectActor>[] SpawnNuclearWaffleSpriteLinks(
        RoomEnemyPlace place,
        SpriteGraphics graphics)
    {
        var links = new NuclearWaffleLink<RoomEffectActor>[NuclearWaffleDefinitions.SpriteLinks];
        for (int segmentIndex = 0; segmentIndex < links.Length; segmentIndex++)
        {
            links[segmentIndex] = new NuclearWaffleLink<RoomEffectActor>(_effectActors.SpawnRoomEffectActor(
                place.Position.Whole,
                RoomEffectActorKind.NuclearWaffleBody,
                graphics));
        }
        return links;
    }

    /// <summary>Places each damaging link along the sweep behind the head, and reports whether the last has reached the sweep's end.</summary>
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle.TurnedBySteps(state.Sweep.InterleavedSegmentOffset);
        bool finished = false;
        foreach (NuclearWaffleLink<EnemyProjectile> link in state.ProjectileLinks)
        {
            angle = angle.TurnedBySteps(-state.Sweep.SegmentSpacing);
            EnemyProjectile segment = link.Actor;
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != link.TurnState)
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                link.TurnState = orientation.State;
            }

            (NuclearWaffleSweepAngle clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(state, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state, clampedAngle));
            // The sweep tests the flag left by the final link's second clamp. It is
            // deliberately not an OR across the chain: the head and leading links can sit
            // at the endpoint while the final projectile link is still unfurling.
            finished = linkFinished;
        }

        return finished;
    }

    /// <summary>Places each cosmetic link along the sweep behind the head.</summary>
    private void PositionNuclearWaffleSpriteSegments(NuclearWaffleEnemyState state)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle;
        foreach (NuclearWaffleLink<RoomEffectActor> link in state.SpriteLinks)
        {
            angle = angle.TurnedBySteps(-state.Sweep.SegmentSpacing);
            RoomEffectActor segment = link.Actor;
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(state, angle);
            if (orientation.State != link.TurnState)
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                link.TurnState = orientation.State;
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
        NuclearWaffleSweepDefinition sweep = state.Sweep;
        if (state.Direction == NuclearWaffleSweepDirection.Backward)
        {
            if (angle >= sweep.EndAngle)
            {
                return angle < sweep.StartAngle
                    ? new NuclearWaffleAngleClamp(angle, false)
                    : new NuclearWaffleAngleClamp(sweep.StartAngle, false);
            }
            return new NuclearWaffleAngleClamp(sweep.EndAngle, true);
        }

        if (angle >= sweep.EndAngle)
            return new NuclearWaffleAngleClamp(sweep.EndAngle, true);
        return angle < sweep.StartAngle
            ? new NuclearWaffleAngleClamp(sweep.StartAngle, false)
            : new NuclearWaffleAngleClamp(angle, false);
    }

    /// <summary>Chooses a link's orientation.</summary>
    private static NuclearWaffleOrientation GetNuclearWaffleOrientation(
        NuclearWaffleEnemyState state,
        NuclearWaffleSweepAngle angle)
    {
        NuclearWaffleSweepDefinition sweep = state.Sweep;
        if (state.Direction == NuclearWaffleSweepDirection.Forward)
        {
            if (angle >= sweep.FallingTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Counterclockwise, NuclearWaffleTurnState.PastFallingThreshold);
            if (angle >= sweep.RisingTurnThreshold)
                return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastRisingThreshold);
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
        }

        if (angle < sweep.FallingTurnThreshold)
            return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.PastFallingThreshold);
        if (angle < sweep.RisingTurnThreshold)
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
            RoomEffectActorKind.NuclearWaffleSplash,
            graphics);
        _ = _effectActors.SpawnRoomEffectActor(
            position,
            orientation.Turn == RotationDirection.Counterclockwise
                ? RoomEffectActorKind.NuclearWaffleLeftExplosion
                : RoomEffectActorKind.NuclearWaffleRightExplosion,
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
    /// The whole-pixel point on the orbit at <paramref name="angle"/>, wrapping around the room. The orbit reads only the
    /// angle's direction within one turn.
    /// </summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(NuclearWaffleEnemyState state, NuclearWaffleSweepAngle angle) =>
        NuclearWaffleOrbitPoint(state.Origin, state.OrbitRadius, angle);

    /// <summary>
    /// The whole-pixel point at <paramref name="angle"/> on the orbit of <paramref name="radius"/> around
    /// <paramref name="origin"/>, wrapping around the room.
    /// </summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(Position16<RoomPixels> origin, int radius, NuclearWaffleSweepAngle angle) =>
        origin + EnemyMath.CircleOffset(angle.ToRotationAngle().ToTrigonometryAngle(), radius);

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
