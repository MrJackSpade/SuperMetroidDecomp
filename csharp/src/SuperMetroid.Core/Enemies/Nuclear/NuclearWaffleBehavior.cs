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
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => new NuclearWaffleEntity(this, occupant, place, common);

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
    internal void InitializeNuclearWaffle(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        NuclearWaffleSpawnSettings settings = occupant.SpawnSettings<NuclearWaffleSpawnSettings>();
        NuclearWaffleSweepDefinition sweep = settings.Sweep;

        // The head starts on the orbit at the sweep's start angle around the placement, and every link spawns there.
        Position16<RoomPixels> origin = place.Position.Whole;
        place.Position = place.Position.WithWhole(NuclearWaffleOrbitPoint(origin, settings.OrbitRadius, sweep.StartAngle));
        Position16<RoomPixels> initialHead = place.Position.Whole;
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(occupant, place.Shading);

        var state = new NuclearWaffleEnemyState(NuclearWaffleAction.Waiting)
        {
            ProjectileLinks = SpawnNuclearWaffleProjectileLinks(place, initialHead, graphics),
            SpriteLinks = SpawnNuclearWaffleSpriteLinks(place, graphics),
            WaitingTimer = UpdateCountdown.For(settings.WaitTime + EnemyWaitDefinitions.RunOutUpdate),
            SweepAngle = sweep.StartAngle,
            Origin = origin,
            Graphics = graphics,
        };
        place.SetSpeciesState(state);
        state.Playback = EnemyAnimationPlayback.Start(place, NuclearWaffleAnimations.StartingWith(NuclearWaffleSegment.BodyLoop));
    }

    /// <summary>Runs the Nuclear Waffle's main AI.</summary>
    private void RunNuclearWaffleMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, NuclearWaffleEnemyState state)
    {
        switch (state.Action)
        {
            case NuclearWaffleAction.Waiting:
                RunNuclearWaffleWaiting(occupant, place, state);
                return;

            case NuclearWaffleAction.Sweeping:
                RunNuclearWaffleSweep(occupant, place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Nuclear Waffle has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Waits before a sweep.</summary>
    private static void RunNuclearWaffleWaiting(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        NuclearWaffleEnemyState state)
    {
        if (!state.WaitingTimer.Step())
            return;

        NuclearWaffleSpawnSettings settings = occupant.SpawnSettings<NuclearWaffleSpawnSettings>();
        state.WaitingTimer = UpdateCountdown.For(settings.WaitTime + EnemyWaitDefinitions.RunOutUpdate);
        state.SweepAngle = settings.Sweep.StartAngle;
        state.Action = NuclearWaffleAction.Sweeping;
        state.RestartTurns();
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
    }

    /// <summary>Runs a sweep: the head and every link stand on the orbit of the spawn settings' radius.</summary>
    private void RunNuclearWaffleSweep(RoomEnemyOccupant occupant, RoomEnemyPlace place, NuclearWaffleEnemyState state)
    {
        NuclearWaffleSpawnSettings settings = occupant.SpawnSettings<NuclearWaffleSpawnSettings>();
        NuclearWaffleOrientation headOrientation = GetNuclearWaffleOrientation(
            settings,
            state.CurrentAngle);
        if (headOrientation.State != state.HeadOrientationState)
        {
            SpawnNuclearWaffleTurnEffects(
                place.Position.Whole,
                state.Graphics,
                headOrientation);
            state.HeadOrientationState = headOrientation.State;
        }

        (NuclearWaffleSweepAngle clampedHeadAngle, _) = ClampNuclearWaffleAngle(settings, state.CurrentAngle);
        place.Position = place.Position.WithWhole(NuclearWaffleOrbitPoint(state.Origin, settings.OrbitRadius, clampedHeadAngle));

        bool sweepFinished = PositionNuclearWaffleProjectileSegments(state, settings);
        PositionNuclearWaffleSpriteSegments(state, settings);
        // The sweep turns at the placement's angular speed the way it runs.
        state.SweepAngle = state.SweepAngle.TurnedOn(settings.AngularSpeed, settings.Direction);
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
            EnemyProjectile? segment = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.NuclearWaffleBody, EnemyProjectileBehavior.Still, graphics);
            if (segment is null)
                throw new InvalidOperationException("Nuclear Waffle exhausted the enemy-projectile pool during room load.");

            // The link starts as its definition has it: its twelve-frame animation, an 8 by 8 pixel size and 64
            // damage. It then starts at the head as one link of the body, which shots cannot destroy.
            segment.StartAnimation(new NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment.Churning));
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
    private bool PositionNuclearWaffleProjectileSegments(NuclearWaffleEnemyState state, NuclearWaffleSpawnSettings settings)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle.TurnedAhead(settings.Sweep.InterleavedSegmentOffset, settings.Direction);
        bool finished = false;
        foreach (NuclearWaffleLink<EnemyProjectile> link in state.ProjectileLinks)
        {
            angle = angle.TurnedBehind(settings.Sweep.SegmentSpacing, settings.Direction);
            EnemyProjectile segment = link.Actor;
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(settings, angle);
            if (orientation.State != link.TurnState)
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                link.TurnState = orientation.State;
            }

            (NuclearWaffleSweepAngle clampedAngle, bool linkFinished) = ClampNuclearWaffleAngle(settings, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state.Origin, settings.OrbitRadius, clampedAngle));
            // The sweep ends when its last damaging link reaches the sweep's end, not when any link does: the head
            // and the leading links can rest at the end while the last link is still unfurling.
            finished = linkFinished;
        }

        return finished;
    }

    /// <summary>Places each cosmetic link along the sweep behind the head.</summary>
    private void PositionNuclearWaffleSpriteSegments(NuclearWaffleEnemyState state, NuclearWaffleSpawnSettings settings)
    {
        NuclearWaffleSweepAngle angle = state.CurrentAngle;
        foreach (NuclearWaffleLink<RoomEffectActor> link in state.SpriteLinks)
        {
            angle = angle.TurnedBehind(settings.Sweep.SegmentSpacing, settings.Direction);
            RoomEffectActor segment = link.Actor;
            NuclearWaffleOrientation orientation = GetNuclearWaffleOrientation(settings, angle);
            if (orientation.State != link.TurnState)
            {
                SpawnNuclearWaffleTurnEffects(
                    segment.Position.Whole,
                    state.Graphics,
                    orientation);
                link.TurnState = orientation.State;
            }

            (NuclearWaffleSweepAngle clampedAngle, _) = ClampNuclearWaffleAngle(settings, angle);
            segment.Position = segment.Position.WithWhole(NuclearWaffleOrbitPoint(state.Origin, settings.OrbitRadius, clampedAngle));
        }
    }

    /// <summary>
    /// Clamps a link angle and reports whether the sweep finished: a link that has reached the sweep's end stands there and
    /// finishes it, and one not yet past its start stands at the start. The sweep's angles count on past the full turn, so
    /// they compare in order along it.
    /// </summary>
    private static NuclearWaffleAngleClamp ClampNuclearWaffleAngle(
        NuclearWaffleSpawnSettings settings,
        NuclearWaffleSweepAngle angle)
    {
        NuclearWaffleSweepDefinition sweep = settings.Sweep;
        if (angle.HasReached(sweep.EndAngle, settings.Direction))
            return new NuclearWaffleAngleClamp(sweep.EndAngle, true);
        return angle.HasReached(sweep.StartAngle, settings.Direction)
            ? new NuclearWaffleAngleClamp(angle, false)
            : new NuclearWaffleAngleClamp(sweep.StartAngle, false);
    }

    /// <summary>
    /// Chooses a link's orientation: past the falling threshold it turns against its rising turn, past the rising
    /// threshold it makes its rising turn, and before both it has turned clockwise, either way the sweep runs.
    /// </summary>
    private static NuclearWaffleOrientation GetNuclearWaffleOrientation(
        NuclearWaffleSpawnSettings settings,
        NuclearWaffleSweepAngle angle)
    {
        NuclearWaffleSweepDefinition sweep = settings.Sweep;
        RotationDirection risingTurn = RisingTurn(settings.Direction);
        if (angle.HasReached(sweep.FallingTurnThreshold, settings.Direction))
            return new NuclearWaffleOrientation(risingTurn.Reversed(), NuclearWaffleTurnState.PastFallingThreshold);
        if (angle.HasReached(sweep.RisingTurnThreshold, settings.Direction))
            return new NuclearWaffleOrientation(risingTurn, NuclearWaffleTurnState.PastRisingThreshold);
        return new NuclearWaffleOrientation(RotationDirection.Clockwise, NuclearWaffleTurnState.BeforeTurns);
    }

    /// <summary>The way a link turns at the rising threshold: clockwise sweeping forward, counterclockwise sweeping backward.</summary>
    private static RotationDirection RisingTurn(NuclearWaffleSweepDirection direction) => direction switch
    {
        NuclearWaffleSweepDirection.Forward => RotationDirection.Clockwise,
        NuclearWaffleSweepDirection.Backward => RotationDirection.Counterclockwise,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Undefined Nuclear Waffle sweep direction."),
    };

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

    /// <summary>
    /// The whole-pixel point at <paramref name="angle"/> on the orbit of <paramref name="radius"/> around
    /// <paramref name="origin"/>.
    /// </summary>
    private static Position16<RoomPixels> NuclearWaffleOrbitPoint(Position16<RoomPixels> origin, int radius, NuclearWaffleSweepAngle angle) =>
        origin + EnemyMath.CircleOffset(angle.ToRotationAngle().ToTrigonometryAngle(), radius);

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
    internal void RunNuclearWaffleMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame _)
    {
        RunNuclearWaffleMain(occupant, place, place.RequireSpeciesState<NuclearWaffleEnemyState>());
    }
}
