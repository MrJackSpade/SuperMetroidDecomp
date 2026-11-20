using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Metroid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MetroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Metroid];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new MetroidEntity(this, occupant, place, common));

    /// <summary>Supplies room RNG for the scattered death drops and idle cries.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Probes terrain while the Metroid seeks or recoils.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues Metroid-specific animation, ice, and recoil sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns the two effect actors composited over the Metroid body.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Creates death animations and randomized item drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds Metroid AI to the room, body layers, audio, frame context, and death owner.</summary>
    internal MetroidBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, RoomEffectActors effectActors, EnemyFrameContext frameContext, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _effectActors = effectActors;
        _frameContext = frameContext;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Carries out one cue of the Metroid's animations: each pulse loop ends with a sound.</summary>
    private void ApplyMetroidCue(MetroidCue cue)
    {
        switch (cue.Kind)
        {
            case MetroidCueKind.PlayDrainingSound:
                // The attached loop emits the Metroid's animation sound.
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MetroidAnimationSoundEffect, priority: SoundRequestPriority.Normal);
                break;
            case MetroidCueKind.PlayRandomCry:
                // One random number, drawn from the shared random sequence, chooses one of the eight idle cries.
                _audio.QueueEnemySound(MetroidBehaviorDefinitions.Cry.Choose(_room.Random.Generate()), priority: SoundRequestPriority.Normal);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MetroidCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes the Metroid.</summary>
    internal void InitializeMetroid(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(occupant, place.Shading);
        RoomEffectActor electricity = _effectActors.SpawnRoomEffectActor(
            place.Position.Whole,
            RoomEffectActorKind.MetroidElectricity,
            graphics);
        RoomEffectActor shell = _effectActors.SpawnRoomEffectActor(
            place.Position.Whole,
            RoomEffectActorKind.MetroidShell,
            graphics);

        var state = new MetroidEnemyState(electricity, shell)
        {
            Drain = FractionalEnergyDrain.Start,
            Movement = MetroidMovement.Homing,
            IceDamageToFreeze = occupant.SpawnSettings<MetroidSpawnSettings>().IceDamageToFreeze,
        };
        place.SetSpeciesState(state);
        state.Playback = new(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a Metroid for one update and whichever of its four movements it is in.</summary>
    private void RunMetroidMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        int targetY = samus.Position.Y.Whole - MetroidBehaviorDefinitions.AimAboveSamus;
        switch (state.Movement)
        {
            case MetroidMovement.Homing:
                RunMetroidHoming(place, state, samus, targetY, level);
                break;
            case MetroidMovement.ClosingOnSamus:
                RunMetroidClosing(place, state, samus, targetY, level);
                break;
            case MetroidMovement.AttachedToSamus:
                RunMetroidAttached(place, state, samus, targetY);
                break;
            case MetroidMovement.PowerBombEscape:
                RunMetroidPowerBombEscape(place, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Metroid movement {state.Movement} is not one of its four movements.");
        }

        // The ordinary body shows the central picture. Both outer objects are
        // repositioned and re-enabled every update the Metroid is not frozen, right after it moves.
        PositionMetroidOuterBody(state.Electricity, place, SpriteGraphics.SpawnedBy(occupant, place.Shading));
        PositionMetroidOuterBody(state.Shell, place, SpriteGraphics.SpawnedBy(occupant, place.Shading));
    }

    /// <summary>Pulls the Metroid's velocity toward Samus, harder the farther she is.</summary>
    private void RunMetroidHoming(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus,
        int targetY,
        RoomLevelData level)
    {
        int yDifference = place.Position.Y.Whole - targetY;
        SignedSixteenSixteen yVelocity = ClampMetroidVelocity(state.Velocity.Y - EnemyMath.HomingPull(yDifference));
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, place, yVelocity))
            StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        int xDifference = place.Position.X.Whole - samus.Position.X.Whole;
        SignedSixteenSixteen xVelocity = ClampMetroidVelocity(state.Velocity.X - EnemyMath.HomingPull(xDifference));
        StoreMetroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, xVelocity))
            StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the Metroid's fixed-speed seek as it closes on Samus.</summary>
    private void RunMetroidClosing(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus,
        int targetY,
        RoomLevelData level)
    {
        // The Metroid reckons each distance within a 256-pixel span (MetroidClosingDistance), so a target it has fallen
        // 128 pixels or more behind reads as lying the other way.
        SignedSixteenSixteen yVelocity = SignedSixteenSixteen.FromWholePixels(
            CalculateMetroidClosingVelocity(targetY, place.Position.Y.Whole));
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, place, yVelocity))
            StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        SignedSixteenSixteen xVelocity = SignedSixteenSixteen.FromWholePixels(
            CalculateMetroidClosingVelocity(samus.Position.X.Whole, place.Position.X.Whole));
        StoreMetroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, xVelocity))
            StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>Chooses a closing Metroid's speed on one axis from its distance to its target.</summary>
    private static int CalculateMetroidClosingVelocity(int target, int current)
    {
        // The closing speed is a multiple of the distance the Metroid reckons (MetroidClosingDistance), clamped.
        return Math.Clamp(
            MetroidBehaviorDefinitions.ClosingSpeedPerDistance * MetroidClosingDistance.Between(target, current),
            -MetroidBehaviorDefinitions.FastestChase,
            MetroidBehaviorDefinitions.FastestChase);
    }

    /// <summary>Holds an attached Metroid on Samus; attachment is a hard positional lock.</summary>
    private static void RunMetroidAttached(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus,
        int targetY)
    {
        place.Position = place.Position.WithWhole(new(samus.Position.X.Whole, targetY));
        StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>Makes the escaping Metroid's next move; after its last it chases Samus.</summary>
    private static void RunMetroidPowerBombEscape(RoomEnemyPlace place, MetroidEnemyState state)
    {
        place.Position += MetroidReactions.EscapeDisplacement(state.EscapeMovesLeft.UpdatesLeft);
        StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        if (!state.EscapeMovesLeft.Count())
            return;

        state.Movement = MetroidMovement.Homing;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
    }

    /// <summary>Runs a frozen Metroid's own step after the common thaw.</summary>
    internal static void RunMetroidFrozen(RoomEnemyPlace place)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        if (state.EscapeMovesLeft.IsRunning)
        {
            state.EscapeMovesLeft.Count();
            place.HurtFlash.Restart(MetroidBehaviorDefinitions.EscapeFlashDuration);
        }

        FreezeMetroidOuterBody(state.Electricity);
        FreezeMetroidOuterBody(state.Shell);
    }

    /// <summary>
    /// Flashes the Metroid's outer bodies while it is hurt: its hit flash on alternate pairs of updates,
    /// its own colours between.
    /// </summary>
    internal static void ApplyMetroidHurt(RoomEnemyPlace place)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        if (MetroidCadenceDefinitions.HurtOuterBodiesShowOwnColours.IsSetIn(place.HurtFlash.UpdatesLeft))
        {
            state.Electricity.Graphics = state.Electricity.Graphics.In(place.Shading);
            state.Shell.Graphics = state.Shell.Graphics.In(place.Shading);
            return;
        }

        state.Electricity.Graphics = state.Electricity.Graphics.In(EnemyShades.HurtFlashLook);
        state.Shell.Graphics = state.Shell.Graphics.In(EnemyShades.HurtFlashLook);
    }

    /// <summary>Resolves a Metroid's touch of Samus: her body attack throws it away from her; otherwise it drains her.</summary>
    internal void ResolveMetroidTouch(RoomEnemyPlace place, SamusState samus)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        int targetY = samus.Position.Y.Whole - MetroidBehaviorDefinitions.AimAboveSamus;
        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
        {
            if (state.Movement != MetroidMovement.AttachedToSamus)
            {
                // The body attack throws the Metroid away from Samus at a share of their distance an update.
                StoreMetroidXVelocity(
                    state,
                    MetroidBehaviorDefinitions.BodyAttackThrowPerDistance * (place.Position.X.Whole - samus.Position.X.Whole));
                StoreMetroidYVelocity(
                    state,
                    MetroidBehaviorDefinitions.BodyAttackThrowPerDistance * (place.Position.Y.Whole - targetY));
                state.Movement = MetroidMovement.Homing;
                SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
            }
            return;
        }

        if (state.Movement != MetroidMovement.PowerBombEscape)
        {
            if (MetroidCadenceDefinitions.DrainSound.IsDue(_frameContext.EnemyPassCount) && samus.Health >= MetroidBehaviorDefinitions.DrainSoundLeastHealth)
                _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Movement is MetroidMovement.AttachedToSamus or MetroidMovement.PowerBombEscape)
            return;

        state.Movement = MetroidMovement.ClosingOnSamus;
        if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) >= MetroidBehaviorDefinitions.LatchDistance ||
            Math.Abs(place.Position.Y.Whole - targetY) >= MetroidBehaviorDefinitions.LatchDistance)
        {
            return;
        }

        state.Movement = MetroidMovement.AttachedToSamus;
        samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.SpeedBoostNext;
        SetMetroidAnimation(place, state, MetroidSegment.DrainingSamus);
    }

    /// <summary>Subtracts one health at suit-dependent fractional intervals without ordinary hurt flash or knockback.</summary>
    private static void DrainSamusWithMetroid(MetroidEnemyState state, SamusState samus)
    {
        EnergyAmount drainFraction = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? MetroidBehaviorDefinitions.GravityDrainFraction
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? MetroidBehaviorDefinitions.VariaDrainFraction
                : MetroidBehaviorDefinitions.PowerSuitDrainFraction;
        if (!state.DrainTakesEnergy(drainFraction))
            return;

        // The Metroid drains one energy, never below none, without the hurt flash, invincibility, knockback or suit
        // protection an ordinary hit brings.
        samus.Health = samus.Health == 0 ? 0 : samus.Health - 1;
    }

    /// <summary>Handles an unfrozen Metroid's recoil and ice build-up after it takes a shot.</summary>
    internal void ResolveMetroidNonFrozenShot(
        RoomEnemyPlace place,
        SamusProjectileType collisionProjectileType,
        int collisionProjectileDamage,
        Position16<RoomPixels> recoilOrigin,
        SamusState samus)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        SamusProjectileFamily family = collisionProjectileType.Family;
        if (state.Movement == MetroidMovement.AttachedToSamus)
        {
            // The bomb collision pass presents the exploding power bomb as its explosion;
            // its earlier expansion is not what the Metroid's shot reaction tests.
            if (family == SamusProjectileFamily.Bomb)
                DetachMetroidWithPowerBomb(place, state, samus);
            return;
        }

        // The Metroid's shot reaction recoils from the first projectile (MetroidRecoilOrigin), not the shot that
        // hit it now. It recoils away from that origin at a share of its distance from it an update. That
        // projectile need not lie in the room: a shot lives on a little past the view's left and top edges, and
        // a Wave shot passes through the room's walls, so one beyond the room's left or top edge, with the view at
        // that edge, still pushes the Metroid away from it the short way, as DistanceAcrossRoomEdge.Apart gives it.
        StoreMetroidXVelocity(
            state,
            MetroidBehaviorDefinitions.ShotRecoilPerDistance * DistanceAcrossRoomEdge.Apart(place.Position.X.Whole, recoilOrigin.X));
        StoreMetroidYVelocity(
            state,
            MetroidBehaviorDefinitions.ShotRecoilPerDistance * DistanceAcrossRoomEdge.Apart(place.Position.Y.Whole, recoilOrigin.Y));
        state.Movement = MetroidMovement.Homing;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);

        if (collisionProjectileType.BeamCombination.HasIce)
        {
            place.HurtFlash.Restart(MetroidBehaviorDefinitions.IceFlashDuration);
            if (collisionProjectileDamage >= state.IceDamageToFreeze)
            {
                state.IceDamageToFreeze = 0;
                place.Freeze.Restart(MetroidBehaviorDefinitions.FrozenDuration);
                place.AiStates |= EnemyAiStates.Frozen;
                return;
            }
            state.IceDamageToFreeze -= collisionProjectileDamage;
        }

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.MetroidRecoilSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Releases an attached Metroid into its escape (<see cref="MetroidBehaviorDefinitions.EscapeDuration"/>) and ends the shade its attachment gives Samus (<see cref="SamusMetroidAttachmentShade.Inactive"/>).</summary>
    private static void DetachMetroidWithPowerBomb(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus)
    {
        state.EscapeMovesLeft = UpdateCountdown.For(MetroidBehaviorDefinitions.EscapeDuration);
        state.Movement = MetroidMovement.PowerBombEscape;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
        samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    internal static void FinishMetroidDeath(RoomEnemyPlace place, SamusState samus)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        state.Electricity.End();
        state.Shell.End();
        samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
    }

    /// <summary>
    /// Spawns the Metroid's death drops around the origin
    /// recorded before its death cleared the place.
    /// </summary>
    internal void RequestMetroidDrops(Position16<RoomPixels> origin, SamusState samus)
    {
        for (int dropIndex = 0; dropIndex < MetroidBehaviorDefinitions.FrozenDeathDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            // Each drop lands within its scatter around the origin on each axis.
            var scatter = new Offset16<RoomPixels>(
                EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring,
                EnemyDropScatterDefinitions.VerticalScatter.Of(random) - EnemyDropScatterDefinitions.ScatterCentring);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(origin + scatter, EnemyDefinitionId.Metroid, samus);
        }
    }

    /// <summary>Clamps the signed whole pixels of homing speed to <see cref="MetroidBehaviorDefinitions.FastestChase"/> while retaining in-range fractions.</summary>
    private static SignedSixteenSixteen ClampMetroidVelocity(SignedSixteenSixteen velocity) => EnemyMath.LimitedEitherWay(velocity, MetroidBehaviorDefinitions.FastestHoming);

    /// <summary>Stores the recoil or homing X speed.</summary>
    private static void StoreMetroidXVelocity(MetroidEnemyState state, SignedSixteenSixteen velocity) =>
        state.Velocity = state.Velocity.WithX(velocity);

    /// <summary>Stores the recoil or homing Y speed.</summary>
    private static void StoreMetroidYVelocity(MetroidEnemyState state, SignedSixteenSixteen velocity) =>
        state.Velocity = state.Velocity.WithY(velocity);

    /// <summary>Moves an outer-body sprite to the Metroid's position and enables its selected graphics.</summary>
    private static void PositionMetroidOuterBody(
        RoomEffectActor outerBody,
        RoomEnemyPlace metroid,
        SpriteGraphics graphics)
    {
        outerBody.Position = outerBody.Position.WithWhole(metroid.Position.Whole);
        outerBody.Graphics = graphics;
        outerBody.AnimationClock.Resume();
    }

    /// <summary>
    /// Switches an outer-body sprite to its frozen colours, restarting its own animation from its first frame; it keeps
    /// drawing from the Metroid's own sheet, in the frozen colours every spawned-object sheet holds (#1881).
    /// </summary>
    private static void FreezeMetroidOuterBody(RoomEffectActor outerBody)
    {
        outerBody.Graphics = outerBody.Graphics.In(EnemyShades.FrozenLook);
        outerBody.AnimationClock.Suspend();
        // Drawing shows the first frame while its clock is suspended.
        outerBody.RestartAnimation();
    }

    /// <summary>Restarts a Metroid's animation after its movement changes, its first step playing on the next update.</summary>
    private static void SetMetroidAnimation(RoomEnemyPlace place, MetroidEnemyState state, MetroidSegment segment)
    {
        state.Playback = new(MetroidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// Routes a bomb through Metroid's attachment and recoil rules: a frozen Metroid ignores bombs, a power-bomb explosion
    /// detaches an attached one, and any other bomb applies its peculiar recoil from the first projectile
    /// (MetroidRecoilOrigin).
    /// </summary>
    internal void ReactToMetroidBomb(EnemyBombContext c)
    {
        if (c.Enemy.Freeze.IsOver)
        {
            SamusProjectile recoilOrigin = MetroidRecoilOrigin.Of(c.OrdinaryProjectiles.Shots);
            ResolveMetroidNonFrozenShot(c.Enemy, c.PlacedBomb.Type, c.PlacedBomb.Damage, recoilOrigin.Position.Whole, c.Samus);
        }
    }

    /// <summary>Runs Metroid's main AI.</summary>
    internal void RunMetroidMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMetroidMain(occupant, place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Metroid's typed animation.</summary>
    internal void AdvanceMetroidTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, MetroidAnimations.Segments,
            static (MetroidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyMetroidCue(cue);
                return null;
            });

    /// <summary>The Metroid Power Bomb reaction, which clears its outer body before generic death.</summary>
    internal static EnemyPowerBombReaction PowerBombReaction { get; } = new()
    {
        Lethal = new EnemyPowerBombLethal.AfterStage(c => FinishMetroidDeath(c.Enemy, c.Samus)),
    };

    /// <summary>Applies frozen missile damage or unfrozen recoil and Ice accumulation once the shot has marked its
    /// hit.</summary>
    internal bool ResolveMetroidOrdinaryShot(in EnemyShotContext c)
    {
        RoomEnemyPlace enemy = c.Enemy;
        if (enemy.Freeze.IsRunning)
        {
            if (c.Family is not SamusProjectileFamily.Missile and not SamusProjectileFamily.SuperMissile)
            {
                // A frozen Metroid rejects every beam and bomb. The shot has already met it, so a non-plasma shot
                // keeps only its collision mark and never deals ordinary damage.
                c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
                return true;
            }

            // Like the ordinary path, the collision only marks the direction. The damage path does not reject a
            // projectile an earlier enemy marked this pass: a Super Missile that just hit a Rinka still reaches
            // the Metroid's shot reaction.
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);

            EnemyWeaponVulnerability frozenVulnerability = EnemyCollision.ReadProjectileVulnerability(c.Occupant, c.ProjectileType);
            int frozenDamage = frozenVulnerability.DamageFrom(c.ProjectileDamage);
            if (!frozenVulnerability.FreezesWithoutDamage && frozenDamage != 0)
            {
                enemy.HurtFlash.Restart(enemy.HurtFlashFrames + MetroidBehaviorDefinitions.FrozenHurtFlashExtension);
                enemy.AiStates |= EnemyAiStates.Hurt;
                enemy.Health = frozenDamage >= enemy.Health
                    ? 0
                    : enemy.Health - frozenDamage;
            }

            if (enemy.Health == 0)
            {
                // The death records the drop origin, the enemy's death spawns the explosion, and only then
                // the drops spawn.
                Position16<RoomPixels> dropOrigin = enemy.Position.Whole;
                FinishMetroidDeath(enemy, c.Samus);
                _deathsAndDrops.StartGenericEnemyDeath(c.Occupant, EnemyDeathExplosion.Big, c.Samus);
                RequestMetroidDrops(dropOrigin, c.Samus);
            }
            return true;
        }

        // A shot on a Metroid takes no common shot reaction and does it no health damage: the collision marks the shot,
        // then the Metroid recoils, freezes or, attached, escapes as a Power Bomb shakes it loose.
        c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
        ResolveMetroidNonFrozenShot(enemy, c.Shot.Type, c.Shot.Damage, MetroidRecoilOrigin.Of(c.Projectiles.Shots).Position.Whole, c.Samus);
        return true;
    }

    /// <summary>The Metroid shot reaction, which routes ordinary projectile hits to its frozen and nonfrozen handling.</summary>
    internal EnemyShotReaction ShotReaction => field ??= new()
    {
        AdmitsBombFamilies = true,
        AfterSuperMissileQuake = ResolveMetroidOrdinaryShot,
    };
}
