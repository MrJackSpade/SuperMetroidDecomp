using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Metroid species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MetroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Metroid];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MetroidEntity(this, slot, common);

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
    /// <summary>Frames for the attached Metroid's Power Bomb escape function.</summary>
    private const int MetroidEscapeDuration = 4;
    /// <summary>Whole-pixel limit applied to each axis of Metroid homing velocity.</summary>
    private const int MetroidMaximumVelocity = 3;
    /// <summary>Number of independently randomized drops after a frozen Metroid dies.</summary>
    private const int MetroidSpecialDropCount = 5;

    /// <summary>Drop publications generated after a frozen Metroid's death animation begins.</summary>
    internal readonly List<MetroidDropRequest> MetroidDropRequests = new();

    /// <summary>Initializes the Metroid.</summary>
    internal void InitializeMetroid(RoomEnemySlot slot)
    {
        SpriteGraphics graphics = SpriteGraphics.SpawnedBy(slot);
        RoomSpriteObject outerBodyA = _spriteObjects.SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyA,
            graphics);
        RoomSpriteObject outerBodyB = _spriteObjects.SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyB,
            graphics);

        var state = new MetroidEnemyState(outerBodyA, outerBodyB)
        {
            DrainAccumulator = 0,
            Function = MetroidAiFunction.Homing,
            IceDamageToFreeze = slot.SpawnSettings<MetroidSpawnSettings>().IceDamageToFreeze,
        };
        slot.SetSpeciesState(state);
        state.Playback = new(MetroidAnimations.StartingWith(MetroidProgram.ChasingSamus), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs a Metroid's per-frame behaviour and its four-entry movement dispatch.</summary>
    private void RunMetroidMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Metroid movement requires room level data.");

        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        int targetY = CoordinateAxis.Wrap(samus.Position.Y.Whole - 8);
        switch (state.Function)
        {
            case MetroidAiFunction.Homing:
                RunMetroidHoming(slot, state, samus, targetY, level);
                break;
            case MetroidAiFunction.ClosingOnSamus:
                RunMetroidClosing(slot, state, samus, targetY, level);
                break;
            case MetroidAiFunction.AttachedToSamus:
                RunMetroidAttached(slot, state, samus, targetY);
                break;
            case MetroidAiFunction.PowerBombEscape:
                RunMetroidPowerBombEscape(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Metroid movement {state.Function} is not one of its four movements.");
        }

        // The ordinary body supplies the central map. Both outer objects are
        // repositioned and re-enabled every non-frozen main call, exactly after movement.
        PositionMetroidOuterBody(state.OuterBodyA, slot, SpriteGraphics.SpawnedBy(slot));
        PositionMetroidOuterBody(state.OuterBodyB, slot, SpriteGraphics.SpawnedBy(slot));
    }

    /// <summary>Runs the Metroid's proportional 16.16 attraction toward Samus.</summary>
    private void RunMetroidHoming(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        int targetY,
        RoomLevelData level)
    {
        int yDifference = CoordinateAxis.Difference(slot.Position.Y.Whole, targetY);
        SignedSixteenSixteen yVelocity = ClampMetroidVelocity(state.Velocity.Y - EnemyMath.HomingPull(yDifference));
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, slot, yVelocity))
            StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        int xDifference = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole);
        SignedSixteenSixteen xVelocity = ClampMetroidVelocity(state.Velocity.X - EnemyMath.HomingPull(xDifference));
        StoreMetroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the Metroid's byte-oriented fixed-speed seek as it closes on Samus.</summary>
    private void RunMetroidClosing(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        int targetY,
        RoomLevelData level)
    {
        // The game constructs a temporary word from the coordinate low-byte subtraction,
        // shifts it right three, sign-extends bit twelve, then clamps the whole result.
        // This is observably not a clean full-word delta: crossing a 256-pixel boundary can
        // reverse the short route. Preserve the byte arithmetic instead of “fixing” it.
        SignedSixteenSixteen yVelocity = SignedSixteenSixteen.FromWholePixels(
            CalculateMetroidClosingVelocity(targetY, slot.Position.Y.Whole));
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, slot, yVelocity))
            StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        SignedSixteenSixteen xVelocity = SignedSixteenSixteen.FromWholePixels(
            CalculateMetroidClosingVelocity(samus.Position.X.Whole, slot.Position.X.Whole));
        StoreMetroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>The span within which a Metroid measures its distance to its target.</summary>
    private const int ClosingSpan = 256;

    /// <summary>Chooses Metroid's byte-oriented closing speed from wrapped distance to Samus.</summary>
    private static int CalculateMetroidClosingVelocity(int target, int current)
    {
        // The closing speed is 32 times the signed distance between the two coordinates' places within a 256-pixel
        // span, -128 through 127: a distance of 128 or more one way reads as the other way.
        int nearDistance = CoordinateAxis.Wrap(target - current) % ClosingSpan;
        if (nearDistance >= ClosingSpan / 2)
            nearDistance -= ClosingSpan;
        return Math.Clamp(
            32 * nearDistance,
            -MetroidMaximumVelocity,
            MetroidMaximumVelocity);
    }

    /// <summary>Holds an attached Metroid on Samus; attachment is a hard positional lock.</summary>
    private static void RunMetroidAttached(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        int targetY)
    {
        slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, targetY));
        StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the Metroid's escape from a power bomb, including post-decrement timer semantics.</summary>
    internal static void RunMetroidPowerBombEscape(RoomEnemySlot slot, MetroidEnemyState state)
    {
        slot.Position += MetroidReactions.EscapeDisplacement(state.EscapeTimer);
        StoreMetroidXVelocity(state, SignedSixteenSixteen.Zero);
        StoreMetroidYVelocity(state, SignedSixteenSixteen.Zero);

        if (--state.EscapeTimer != 0)
            return;

        state.Function = MetroidAiFunction.Homing;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
    }

    /// <summary>Runs the family-specific tail of a frozen Metroid's update.</summary>
    internal static void RunMetroidFrozen(RoomEnemySlot slot)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        if (state.EscapeTimer != 0)
        {
            state.EscapeTimer--;
            slot.FlashTimer = 2;
        }

        FreezeMetroidOuterBody(state.OuterBodyA, MetroidOuterBodyDefinitions.OuterBodyAFrozenProgram);
        FreezeMetroidOuterBody(state.OuterBodyB, MetroidOuterBodyDefinitions.OuterBodyBFrozenProgram);
    }

    /// <summary>
    /// Flashes the Metroid's outer bodies while its custom hurt AI runs: its hit flash on alternate pairs of updates,
    /// its own colours between.
    /// </summary>
    internal static void ApplyMetroidHurt(RoomEnemySlot slot)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        if (MetroidCadenceDefinitions.HurtOuterBodiesShowOwnColours.IsSetIn(slot.FlashTimer))
        {
            state.OuterBodyA.Graphics = state.OuterBodyA.Graphics.In(slot.Shading);
            state.OuterBodyB.Graphics = state.OuterBodyB.Graphics.In(slot.Shading);
            return;
        }

        state.OuterBodyA.Graphics = state.OuterBodyA.Graphics.In(EnemyShades.HurtFlashLook);
        state.OuterBodyB.Graphics = state.OuterBodyB.Graphics.In(EnemyShades.HurtFlashLook);
    }

    /// <summary>Resolves the ordinary and contact-damage branches of a Metroid's touch.</summary>
    private void ResolveMetroidTouch(RoomEnemySlot slot, SamusState samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        int targetY = CoordinateAxis.Wrap(samus.Position.Y.Whole - 8);
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            if (state.Function != MetroidAiFunction.AttachedToSamus)
            {
                // Overlapping byte writes produce delta/4 in signed 16.16 form.
                StoreMetroidXVelocity(
                    state,
                    SignedSixteenSixteen.FromSixteenthsOfPixel(4 * CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole)));
                StoreMetroidYVelocity(
                    state,
                    SignedSixteenSixteen.FromSixteenthsOfPixel(4 * CoordinateAxis.Difference(slot.Position.Y.Whole, targetY)));
                state.Function = MetroidAiFunction.Homing;
                SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
            }
            return;
        }

        if (state.Function != MetroidAiFunction.PowerBombEscape)
        {
            if (MetroidCadenceDefinitions.DrainSoundEveryEighthEnemyPass.IsDue(_frameContext.RandomEnemyCounter) && samus.Health >= 30)
                _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Function is MetroidAiFunction.AttachedToSamus or MetroidAiFunction.PowerBombEscape)
            return;

        state.Function = MetroidAiFunction.ClosingOnSamus;
        if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) >= 8 ||
            CoordinateAxis.Distance(slot.Position.Y.Whole, targetY) >= 8)
        {
            return;
        }

        state.Function = MetroidAiFunction.AttachedToSamus;
        samus.MetroidAttachmentPalette = SamusMetroidAttachmentPalette.SpeedBoostNext;
        SetMetroidProgram(slot, state, MetroidProgram.DrainingSamus);
    }

    /// <summary>Subtracts one health at suit-dependent fractional intervals without ordinary hurt flash or knockback.</summary>
    private static void DrainSamusWithMetroid(MetroidEnemyState state, SamusState samus)
    {
        int drainRate = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? MetroidBehaviorDefinitions.GravityDrainFraction
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? MetroidBehaviorDefinitions.VariaDrainFraction
                : MetroidBehaviorDefinitions.PowerSuitDrainFraction;
        if (!state.DrainBorrows(drainRate))
            return;

        // Dealing Samus one damage performs a saturating subtract and deliberately does not
        // start ordinary hurt flash, invincibility, knockback, or suit division.
        samus.Health = samus.Health == 0 ? 0 : samus.Health - 1;
    }

    /// <summary>Handles non-frozen recoil/ice accumulation after accepts a shot.</summary>
    internal void ResolveMetroidNonFrozenShot(
        RoomEnemySlot slot,
        SamusProjectileType collisionProjectileType,
        int collisionProjectileDamage,
        Position16<RoomPixels> recoilOrigin,
        SamusState? samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        SamusProjectileFamily family = collisionProjectileType.Family;
        if (state.Function == MetroidAiFunction.AttachedToSamus)
        {
            // The bomb collision pass presents the exploding power bomb as its explosion;
            // its earlier expansion record is not what the Metroid's shot reaction tests.
            if (family == SamusProjectileFamily.Bomb)
                DetachMetroidWithPowerBomb(slot, state, samus);
            return;
        }

        // The Metroid's shot reaction recoils from the first projectile (MetroidRecoilOrigin), not the current
        // collision slot. The overlapping byte stores encode delta/8 as signed 16.16.
        StoreMetroidXVelocity(
            state,
            SignedSixteenSixteen.FromSixteenthsOfPixel(2 * CoordinateAxis.Difference(slot.Position.X.Whole, recoilOrigin.X)));
        StoreMetroidYVelocity(
            state,
            SignedSixteenSixteen.FromSixteenthsOfPixel(2 * CoordinateAxis.Difference(slot.Position.Y.Whole, recoilOrigin.Y)));
        state.Function = MetroidAiFunction.Homing;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);

        if (collisionProjectileType.BeamCombination.HasIce)
        {
            slot.FlashTimer = 4;
            if (collisionProjectileDamage >= state.IceDamageToFreeze)
            {
                state.IceDamageToFreeze = 0;
                slot.FrozenTimer = MetroidFrozenDuration;
                slot.AiStates |= EnemyAiStates.Frozen;
                return;
            }
            state.IceDamageToFreeze -= collisionProjectileDamage;
        }

        _audio.QueueEnemySound(EnemySoundEffectDefinitions.MetroidRecoilSoundEffect, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Releases an attached Metroid into its four-frame escape and clears Samus's drain palette flag.</summary>
    private static void DetachMetroidWithPowerBomb(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState? samus)
    {
        state.EscapeTimer = MetroidEscapeDuration;
        state.Function = MetroidAiFunction.PowerBombEscape;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
        if (samus is not null)
            samus.MetroidAttachmentPalette = SamusMetroidAttachmentPalette.Inactive;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    internal static void FinishMetroidDeath(RoomEnemySlot slot, SamusState? samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        state.Velocity = state.Velocity.WithWholeX(0);
        state.OuterBodyA.End();
        state.OuterBodyB.End();
        if (samus is not null)
            samus.MetroidAttachmentPalette = SamusMetroidAttachmentPalette.Inactive;
    }

    /// <summary>
    /// Spawns the Metroid's death drops around the origin
    /// recorded before its death cleared the slot.
    /// </summary>
    internal void RequestMetroidDrops(Position16<RoomPixels> origin)
    {
        for (int dropIndex = 0; dropIndex < MetroidSpecialDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            // Each axis adds a five-bit random selection minus sixteen: -16..+15 pixels.
            var scatter = new Offset16<RoomPixels>(
                EnemyDropScatterDefinitions.HorizontalScatter.Of(random) - 16,
                EnemyDropScatterDefinitions.VerticalScatter.Of(random) - 16);
            MetroidDropRequests.Add(new MetroidDropRequest());
            _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(origin + scatter, EnemyDefinitionId.Metroid);
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
        RoomEnemySlot metroid,
        SpriteGraphics graphics)
    {
        outerBody.Position = outerBody.Position.WithWhole(metroid.Position.Whole);
        outerBody.Graphics = graphics;
        outerBody.UpdatesDisabled = false;
    }

    /// <summary>Switches an outer-body sprite to frozen art without resetting its retained instruction timer.</summary>
    private static void FreezeMetroidOuterBody(
        RoomSpriteObject outerBody,
        RoomSpriteObjectProgram frozenProgram)
    {
        outerBody.Graphics = MetroidOuterBodyDefinitions.FrozenGraphics;
        outerBody.UpdatesDisabled = true;
        // Drawing reads the selected record even while animation is disabled.
        // Show the frozen program's first frame without resetting the retained timer.
        outerBody.ShowFirstFrameKeepingTimer(frozenProgram);
    }

    /// <summary>Restarts a Metroid animation at timer one after its function changes.</summary>
    private static void SetMetroidProgram(RoomEnemySlot slot, MetroidEnemyState state, MetroidProgram program)
    {
        state.Playback = new(MetroidAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>Routes bomb-family collisions through Metroid's attachment and recoil rules.</summary>
    internal void AddMetroidBombReactions(EnemySharedReactions table)
    {
        // Metroid shot AI ignores bombs while frozen, detaches an attached body for a power-bomb explosion, or applies its
        // peculiar recoil from the first projectile otherwise (MetroidRecoilOrigin).
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                  EnemyCollision.ShotAi(c.Enemy) == EnemyReaction.MetroidShot,
            c =>
            {
                if (c.Enemy.FrozenTimer == 0)
                {
                    SamusProjectile recoilOrigin = MetroidRecoilOrigin.Of(c.OrdinaryProjectiles.Slots);
                    ResolveMetroidNonFrozenShot(c.Enemy, c.Bomb.Type, Ensure.AtLeastZero(c.Bomb.Damage), recoilOrigin.Position.Whole, c.Samus);
                }
            });
    }

    /// <summary>Ports Metroid's main AI.</summary>
    internal void RunMetroidMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMetroidMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Metroid's typed animation.</summary>
    internal void AdvanceMetroidTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, MetroidAnimations.Segments, MetroidFrameDefinitions.ShownFrames.Of,
            static (MetroidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyMetroidCue(cue);
                return null;
            });

    /// <summary>Registers the Metroid Power Bomb callback that clears its outer body before generic death.</summary>
    internal static void AddMetroidPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyReaction.MetroidPowerBomb)
        {
            BeforeGenericDeath = FinishMetroidDeath,
        });
    }

    /// <summary>Registers Metroid touch behavior, including attachment and suit-dependent draining.</summary>
    internal void AddMetroidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Metroid &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.MetroidTouch,
            context => ResolveMetroidTouch(context.Slot, context.Samus));
    }

    /// <summary>Applies frozen missile damage or unfrozen recoil and Ice accumulation after the shared projectile prelude.</summary>
    internal bool ResolveMetroidOrdinaryShot(in EnemyShotContext c)
    {
        RoomEnemySlot enemy = c.Enemy;
        if (enemy.FrozenTimer != 0)
        {
            if (c.Family is not SamusProjectileFamily.Missile and not SamusProjectileFamily.SuperMissile)
            {
                // Frozen Metroid rejects every beam and bomb family. has already accepted the overlap, so
                // non-plasma shots retain only the collision mark and never enter normal damage AI.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);
                return true;
            }

            // Like the ordinary path, the walker only marks the direction word. The damage path does not reject a
            // projectile an earlier enemy marked this pass: a Super Missile that just hit a Rinka still reaches
            // the Metroid's shot reaction.
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile, c.MarksCollision);

            EnemyWeaponVulnerability frozenVulnerability = EnemyCollision.ReadProjectileVulnerability(enemy, c.ProjectileType);
            int frozenDamage = (c.ProjectileDamage >> 1) * frozenVulnerability.Multiplier;
            if (!frozenVulnerability.FreezesWithoutDamage && frozenDamage != 0)
            {
                enemy.FlashTimer = enemy.HurtFlashFrames + 8;
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
        ResolveMetroidNonFrozenShot(enemy, c.Projectile.Type, Ensure.AtLeastZero(c.Projectile.Damage), MetroidRecoilOrigin.Of(c.Projectiles.Slots).Position.Whole, c.Samus);
        return true;
    }

    /// <summary>Routes ordinary projectile hits to Metroid-specific frozen and nonfrozen shot handling.</summary>
    internal void AddMetroidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MetroidShot)
        {
            AdmitsBombFamilies = true,
            AfterSuperMissileQuake = ResolveMetroidOrdinaryShot,
        });
    }

    /// <summary>Registers hurt AI ahead of frozen AI when both handler bits are set; animation bytecode still advances.</summary>
    internal static void AddMetroidHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.Metroid, (slot, _) => ApplyMetroidHurt(slot));

    /// <summary>Registers the frozen Metroid handler that retains its outer-body state.</summary>
    internal static void AddMetroidFrozenAi(EnemySharedReactions table) =>
        table.AddFrozen(slot => slot.DefinitionId == EnemyDefinitionId.Metroid, RunMetroidFrozen);
}
