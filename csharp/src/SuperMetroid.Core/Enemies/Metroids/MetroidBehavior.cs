using SuperMetroid.Core.Input;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Metroid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MetroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Metroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new MetroidEntity(this, place, common);

    /// <summary>Supplies room RNG for the five scattered death drops and idle cries.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Probes terrain while the Metroid seeks or recoils.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues Metroid-specific animation, ice, and recoil sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns the two sprite objects composited over the Metroid body.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Supplies the current frame's Samus and room context to registered AI.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Creates death animations and randomized item drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds Metroid AI to the room, body layers, audio, frame context, and death owner.</summary>
    internal MetroidBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, RoomSpriteObjects spriteObjects, EnemyFrameContext frameContext, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _spriteObjects = spriteObjects;
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
                // The shared random sequence advances exactly once and the result's remainder by eight chooses one of the eight
                // authored idle cries.
                _audio.QueueEnemySound(MetroidReactions.RandomCrySoundEffect(_room.Random.Generate()), priority: SoundRequestPriority.Normal);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MetroidCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Freeze lifetime once Ice damage exhausts the Metroid's threshold.</summary>
    private const int MetroidFrozenDuration = 400;
    /// <summary>Frames for the attached Metroid's Power Bomb escape.</summary>
    private const int MetroidEscapeDuration = 4;
    /// <summary>Whole-pixel limit applied to each axis of Metroid homing velocity.</summary>
    private const int MetroidMaximumVelocity = 3;
    /// <summary>Number of independently randomized drops after a frozen Metroid dies.</summary>
    private const int MetroidSpecialDropCount = 5;

    /// <summary>Initializes the Metroid.</summary>
    internal void InitializeMetroid(RoomEnemyPlace place)
    {
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(place);
        RoomSpriteObject electricity = _spriteObjects.SpawnRoomSpriteObject(
            place.Position.Whole,
            RoomSpriteObjectKind.MetroidElectricity,
            graphics);
        RoomSpriteObject shell = _spriteObjects.SpawnRoomSpriteObject(
            place.Position.Whole,
            RoomSpriteObjectKind.MetroidShell,
            graphics);

        var state = new MetroidEnemyState(electricity, shell)
        {
            Drain = FractionalEnergyDrain.Start,
            Movement = MetroidMovement.Homing,
            IceDamageToFreeze = place.SpawnSettings<MetroidSpawnSettings>().IceDamageToFreeze,
        };
        place.SetSpeciesState(state);
        state.Playback = new(MetroidAnimations.StartingWith(MetroidSegment.ChasingSamus), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a Metroid's per-frame behaviour and whichever of its four movements it is in.</summary>
    private void RunMetroidMain(RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        int targetY = CoordinateAxis.Wrap(samus.Position.Y.Whole - MetroidBehaviorDefinitions.AimAboveSamus);
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
        PositionMetroidOuterBody(state.Electricity, place, SpriteGraphics.SpawnedBy(place));
        PositionMetroidOuterBody(state.Shell, place, SpriteGraphics.SpawnedBy(place));
    }

    /// <summary>Runs the Metroid's proportional 1/65536-pixel attraction toward Samus.</summary>
    private void RunMetroidHoming(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus,
        int targetY,
        RoomLevelData level)
    {
        int yDifference = CoordinateAxis.Difference(place.Position.Y.Whole, targetY);
        SignedSixteenSixteen yVelocity = ClampMetroidVelocity(state.Velocity.Y - EnemyMath.HomingPull(yDifference));
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, place, yVelocity))
            StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        int xDifference = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole);
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
            -MetroidMaximumVelocity,
            MetroidMaximumVelocity);
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

    /// <summary>Runs the Metroid's escape from a power bomb, including post-decrement timer semantics.</summary>
    internal static void RunMetroidPowerBombEscape(RoomEnemyPlace place, MetroidEnemyState state)
    {
        place.Position += MetroidReactions.EscapeDisplacement(state.EscapeTimer);
        StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        if (--state.EscapeTimer != 0)
            return;

        state.Movement = MetroidMovement.Homing;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
    }

    /// <summary>Runs the family-specific tail of a frozen Metroid's update.</summary>
    internal static void RunMetroidFrozen(RoomEnemyPlace place)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        if (state.EscapeTimer != 0)
        {
            state.EscapeTimer--;
            place.FlashTimer = MetroidBehaviorDefinitions.EscapeFlashDuration;
        }

        FreezeMetroidOuterBody(state.Electricity, MetroidOuterBodyDefinitions.ElectricityFrozenAnimation);
        FreezeMetroidOuterBody(state.Shell, MetroidOuterBodyDefinitions.ShellFrozenAnimation);
    }

    /// <summary>
    /// Flashes the Metroid's outer bodies while its custom hurt AI runs: its hit flash on alternate pairs of updates,
    /// its own colours between.
    /// </summary>
    internal static void ApplyMetroidHurt(RoomEnemyPlace place)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        if (MetroidCadenceDefinitions.HurtOuterBodiesShowOwnColours.IsSetIn(place.FlashTimer))
        {
            state.Electricity.Graphics = state.Electricity.Graphics.In(place.Shading);
            state.Shell.Graphics = state.Shell.Graphics.In(place.Shading);
            return;
        }

        state.Electricity.Graphics = state.Electricity.Graphics.In(EnemyShades.HurtFlashLook);
        state.Shell.Graphics = state.Shell.Graphics.In(EnemyShades.HurtFlashLook);
    }

    /// <summary>Resolves the ordinary and contact-damage branches of a Metroid's touch.</summary>
    internal void ResolveMetroidTouch(RoomEnemyPlace place, SamusState samus)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        int targetY = CoordinateAxis.Wrap(samus.Position.Y.Whole - MetroidBehaviorDefinitions.AimAboveSamus);
        if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
        {
            if (state.Movement != MetroidMovement.AttachedToSamus)
            {
                // The stores produce delta/4 in signed 1/65536-pixel form.
                StoreMetroidXVelocity(
                    state,
                    SignedSixteenSixteen.FromSixteenthsOfPixel(4 * CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole)));
                StoreMetroidYVelocity(
                    state,
                    SignedSixteenSixteen.FromSixteenthsOfPixel(4 * CoordinateAxis.Difference(place.Position.Y.Whole, targetY)));
                state.Movement = MetroidMovement.Homing;
                SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
            }
            return;
        }

        if (state.Movement != MetroidMovement.PowerBombEscape)
        {
            if (MetroidCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter) && samus.Health >= MetroidBehaviorDefinitions.DrainSoundLeastHealth)
                _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Movement is MetroidMovement.AttachedToSamus or MetroidMovement.PowerBombEscape)
            return;

        state.Movement = MetroidMovement.ClosingOnSamus;
        if (CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) >= 8 ||
            CoordinateAxis.Distance(place.Position.Y.Whole, targetY) >= 8)
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
        int drainFraction = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? MetroidBehaviorDefinitions.GravityDrainFraction
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? MetroidBehaviorDefinitions.VariaDrainFraction
                : MetroidBehaviorDefinitions.PowerSuitDrainFraction;
        if (!state.DrainTakesEnergy(drainFraction))
            return;

        // Dealing Samus one damage performs a saturating subtract and deliberately does not
        // start ordinary hurt flash, invincibility, knockback, or suit division.
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
        // hit it now. Each velocity is an eighth of the distance, signed, in 1/65536 pixels.
        StoreMetroidXVelocity(
            state,
            SignedSixteenSixteen.FromSixteenthsOfPixel(2 * CoordinateAxis.Difference(place.Position.X.Whole, recoilOrigin.X)));
        StoreMetroidYVelocity(
            state,
            SignedSixteenSixteen.FromSixteenthsOfPixel(2 * CoordinateAxis.Difference(place.Position.Y.Whole, recoilOrigin.Y)));
        state.Movement = MetroidMovement.Homing;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);

        if (collisionProjectileType.BeamCombination.HasIce)
        {
            place.FlashTimer = MetroidBehaviorDefinitions.IceFlashDuration;
            if (collisionProjectileDamage >= state.IceDamageToFreeze)
            {
                state.IceDamageToFreeze = 0;
                place.FrozenTimer = MetroidFrozenDuration;
                place.AiStates |= EnemyAiStates.Frozen;
                return;
            }
            state.IceDamageToFreeze -= collisionProjectileDamage;
        }

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.MetroidRecoilSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Releases an attached Metroid into its four-frame escape and clears Samus's drain colour flag.</summary>
    private static void DetachMetroidWithPowerBomb(
        RoomEnemyPlace place,
        MetroidEnemyState state,
        SamusState samus)
    {
        state.EscapeTimer = MetroidEscapeDuration;
        state.Movement = MetroidMovement.PowerBombEscape;
        SetMetroidAnimation(place, state, MetroidSegment.ChasingSamus);
        if (samus is not null)
            samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    internal static void FinishMetroidDeath(RoomEnemyPlace place, SamusState samus)
    {
        MetroidEnemyState state = place.RequireSpeciesState<MetroidEnemyState>();
        state.Velocity = state.Velocity.WithWholeX(0);
        state.Electricity.End();
        state.Shell.End();
        if (samus is not null)
            samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.Inactive;
    }

    /// <summary>
    /// Spawns the Metroid's death drops around the origin
    /// recorded before its death cleared the place.
    /// </summary>
    internal void RequestMetroidDrops(Position16<RoomPixels> origin)
    {
        for (int dropIndex = 0; dropIndex < MetroidSpecialDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            // Each axis adds a random 0 to 31 minus sixteen: -16..+15 pixels.
            var scatter = new Offset16<RoomPixels>(
                EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - 16,
                EnemyDropScatterDefinitions.VerticalScatter.Of(random) - 16);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(origin + scatter, EnemyDefinitionId.Metroid);
        }
    }

    /// <summary>Clamps the signed whole pixels of homing speed to three while retaining in-range fractions.</summary>
    private static SignedSixteenSixteen ClampMetroidVelocity(SignedSixteenSixteen velocity) => EnemyMath.ClampWholeVelocity(velocity, MetroidMaximumVelocity);

    /// <summary>Stores the recoil or homing X speed.</summary>
    private static void StoreMetroidXVelocity(MetroidEnemyState state, SignedSixteenSixteen velocity) =>
        state.Velocity = state.Velocity.WithX(velocity);

    /// <summary>Stores the recoil or homing Y speed.</summary>
    private static void StoreMetroidYVelocity(MetroidEnemyState state, SignedSixteenSixteen velocity) =>
        state.Velocity = state.Velocity.WithY(velocity);

    /// <summary>Moves an outer-body sprite to the Metroid's position and enables its selected graphics.</summary>
    private static void PositionMetroidOuterBody(
        RoomSpriteObject outerBody,
        RoomEnemyPlace metroid,
        SpriteGraphics graphics)
    {
        outerBody.Position = outerBody.Position.WithWhole(metroid.Position.Whole);
        outerBody.Graphics = graphics;
        outerBody.UpdatesDisabled = false;
    }

    /// <summary>Switches an outer-body sprite to frozen art, starting its frozen animation from its first frame.</summary>
    private static void FreezeMetroidOuterBody(
        RoomSpriteObject outerBody,
        RoomSpriteObjectSegment frozenSegment)
    {
        outerBody.Graphics = outerBody.Graphics.In(EnemyShades.FrozenLook);
        outerBody.UpdatesDisabled = true;
        // Drawing shows the first frame while animation is disabled.
        outerBody.StartAnimation(frozenSegment);
    }

    /// <summary>Restarts a Metroid animation at timer one after its movement changes.</summary>
    private static void SetMetroidAnimation(RoomEnemyPlace place, MetroidEnemyState state, MetroidSegment segment)
    {
        state.Playback = new(MetroidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>
    /// Routes a bomb through Metroid's attachment and recoil rules: Metroid shot AI ignores bombs while frozen, detaches an
    /// attached body for a power-bomb explosion, or applies its peculiar recoil from the first projectile otherwise
    /// (MetroidRecoilOrigin).
    /// </summary>
    internal void ReactToMetroidBomb(EnemyBombContext c)
    {
        if (c.Enemy.FrozenTimer == 0)
        {
            SamusProjectile recoilOrigin = MetroidRecoilOrigin.Of(c.OrdinaryProjectiles.Shots);
            ResolveMetroidNonFrozenShot(c.Enemy, c.Bomb.Type, Ensure.AtLeastZero(c.Bomb.Damage), recoilOrigin.Position.Whole, c.Samus);
        }
    }

    /// <summary>Runs Metroid's main AI.</summary>
    internal void RunMetroidMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMetroidMain(place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Metroid's typed animation.</summary>
    internal void AdvanceMetroidTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
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
        BeforeGenericDeath = c => FinishMetroidDeath(c.Enemy, c.Samus),
    };

    /// <summary>Applies frozen missile damage or unfrozen recoil and Ice accumulation after the shared projectile prelude.</summary>
    internal bool ResolveMetroidOrdinaryShot(in EnemyShotContext c)
    {
        RoomEnemyPlace enemy = c.Enemy;
        if (enemy.FrozenTimer != 0)
        {
            if (c.Family is not SamusProjectileFamily.Missile and not SamusProjectileFamily.SuperMissile)
            {
                // A frozen Metroid rejects every beam and bomb. The shot has already met it, so a non-plasma shot
                // keeps only its collision mark and never deals ordinary damage.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                return true;
            }

            // Like the ordinary path, the walker only marks the direction. The damage path does not reject a
            // projectile an earlier enemy marked this pass: a Super Missile that just hit a Rinka still reaches
            // the Metroid's shot reaction.
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);

            EnemyWeaponVulnerability frozenVulnerability = EnemyCollision.ReadProjectileVulnerability(enemy, c.ProjectileType);
            int frozenDamage = frozenVulnerability.DamageFrom(c.ProjectileDamage);
            if (!frozenVulnerability.FreezesWithoutDamage && frozenDamage != 0)
            {
                enemy.FlashTimer = enemy.HurtFlashFrames + MetroidBehaviorDefinitions.FrozenHurtFlashExtension;
                enemy.AiStates |= EnemyAiStates.Hurt;
                enemy.Health = frozenDamage >= enemy.Health
                    ? 0
                    : enemy.Health - frozenDamage;
            }

            if (enemy.Health == 0)
            {
                // The death records the drop origin, the enemy's death spawns the explosion, and only then
                // the five drops spawn.
                Position16<RoomPixels> dropOrigin = enemy.Position.Whole;
                FinishMetroidDeath(enemy, c.Samus);
                _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: EnemyDeathExplosion.Big);
                RequestMetroidDrops(dropOrigin);
            }
            return true;
        }

        // Custom Metroid shot AI does not call normal shot AI. Preserve the collision prelude, then apply recoil/ice
        // or attached power-bomb escape without inventing vulnerability-based health damage.
        c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
        ResolveMetroidNonFrozenShot(enemy, c.Projectile.Type, Ensure.AtLeastZero(c.Projectile.Damage), MetroidRecoilOrigin.Of(c.Projectiles.Shots).Position.Whole, c.Samus);
        return true;
    }

    /// <summary>The Metroid shot reaction, which routes ordinary projectile hits to its frozen and nonfrozen handling.</summary>
    internal EnemyShotReaction ShotReaction => field ??= new()
    {
        AdmitsBombFamilies = true,
        AfterSuperMissileQuake = ResolveMetroidOrdinaryShot,
    };
}
