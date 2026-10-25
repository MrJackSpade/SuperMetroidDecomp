using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.SpriteObjects;

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
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Supplies the current frame's Samus and room context to registered AI.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Creates native death animations and randomized item drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds Metroid AI to the room, body layers, audio, frame context, and death owner.</summary>
    internal MetroidBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, RoomSpriteObjectPool spriteObjects, EnemyFrameContext frameContext, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _spriteObjects = spriteObjects;
        _frameContext = frameContext;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Carries out one cue of the Metroid's animations: each pulse loop ends with a sound.</summary>
    private MetroidProgram? ApplyMetroidCue(MetroidCue cue)
    {
        switch (cue.Kind)
        {
            case MetroidCueKind.PlayDrainingSound:
                // The attached loop emits library-two sound $50.
                LastMetroidSoundEffectLibrary2 = EnemySoundEffectDefinitions.MetroidAnimationSoundEffect;
                break;
            case MetroidCueKind.PlayRandomCry:
                // GenerateRandomNumber advances exactly once and the low three result bits choose one of the eight
                // cartridge-defined idle cries.
                LastMetroidSoundEffectLibrary2 = MetroidBehaviorDefinitions.RandomCrySoundEffect(_room.Random.Generate());
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MetroidCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Native freeze lifetime once Ice damage exhausts the Metroid's threshold.</summary>
    private const ushort MetroidFrozenDuration = 400;
    /// <summary>Frames for the attached Metroid's Power Bomb escape function.</summary>
    private const ushort MetroidEscapeDuration = 4;
    /// <summary>Whole-pixel limit applied to each axis of Metroid homing velocity.</summary>
    private const int MetroidMaximumVelocity = 3;
    /// <summary>Native number of independently randomized drops after a frozen Metroid dies.</summary>
    private const int MetroidSpecialDropCount = 5;

    /// <summary>Drop publications generated after a frozen Metroid's death animation begins.</summary>
    internal readonly List<MetroidDropRequest> MetroidDropRequests = new();

    /// <summary>Most recent library-two Metroid sound requested during this enemy frame.</summary>
    public SoundEffectId? LastMetroidSoundEffectLibrary2 { get; private set; }


    /// <summary>Ports <c>Metroid_Init</c> at <c>$A3:EA4F</c>.</summary>
    internal void InitializeMetroid(RoomEnemySlot slot)
    {
        ushort graphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));
        RoomSpriteObjectSlot outerBodyA = _spriteObjects.SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyA,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");
        RoomSpriteObjectSlot outerBodyB = _spriteObjects.SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyB,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");

        var state = new MetroidEnemyState(outerBodyA, outerBodyB)
        {
            DrainAccumulator = 0,
            Function = MetroidAiFunction.Homing,
            IceDamageToFreeze = slot.Spawn.Population.Parameter2,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Program = AnimationProgramCursor<MetroidProgram>.Start(MetroidProgram.ChasingSamus);
    }

    /// <summary>Ports <c>Metroid_Main</c> and its four-entry movement dispatch.</summary>
    private void RunMetroidMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Metroid movement requires room level data.");

        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole - 8));
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
                    $"Metroid movement index {(ushort)state.Function} exceeds its four-entry ROM table.");
        }

        // The ordinary body supplies the central map. Both outer bank-$B4 objects are
        // repositioned and re-enabled every non-frozen main call, exactly after movement.
        ushort graphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));
        PositionMetroidOuterBody(state.OuterBodyA, slot, graphicsIndex);
        PositionMetroidOuterBody(state.OuterBodyB, slot, graphicsIndex);
    }

    /// <summary>Ports proportional 16.16 attraction in <c>Metroid_Func_1</c>.</summary>
    private void RunMetroidHoming(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY,
        RoomLevelData level)
    {
        int yDifference = unchecked((short)(slot.Position.Y.Whole - targetY));
        int yVelocity = unchecked(state.Velocity.Y.RawValue - SnesEightEight.FromWhole(yDifference >> 2));
        yVelocity = ClampMetroidVelocity(yVelocity);
        StoreMetroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, slot, yVelocity))
            StoreMetroidYVelocity(state, 0);

        int xDifference = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole));
        int xVelocity = unchecked(state.Velocity.X.RawValue - SnesEightEight.FromWhole(xDifference >> 2));
        xVelocity = ClampMetroidVelocity(xVelocity);
        StoreMetroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMetroidXVelocity(state, 0);
    }

    /// <summary>Ports the byte-oriented fixed-speed seek in <c>Metroid_Func_2</c>.</summary>
    private void RunMetroidClosing(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY,
        RoomLevelData level)
    {
        // The 65816 constructs a temporary word from the coordinate low-byte subtraction,
        // shifts it right three, sign-extends bit twelve, then clamps the whole result.
        // This is observably not a clean full-word delta: crossing a 256-pixel boundary can
        // reverse the short route. Preserve the byte arithmetic instead of “fixing” it.
        int yVelocity = CalculateMetroidClosingVelocity(targetY, slot.Position.Y.Whole);
        StoreMetroidYVelocity(state, SnesSixteenSixteen.FromWhole(yVelocity));
        if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(yVelocity)))
            StoreMetroidYVelocity(state, 0);

        int xVelocity = CalculateMetroidClosingVelocity(samus.Position.X.Whole, slot.Position.X.Whole);
        StoreMetroidXVelocity(state, SnesSixteenSixteen.FromWhole(xVelocity));
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SnesSixteenSixteen.FromWhole(xVelocity)))
            StoreMetroidXVelocity(state, 0);
    }

    /// <summary>Chooses Metroid's byte-oriented closing speed from wrapped distance to Samus.</summary>
    private static int CalculateMetroidClosingVelocity(ushort target, ushort current)
    {
        int lowByteDifference = unchecked((byte)(target - (byte)current));
        int shifted = lowByteDifference << 5;
        // Sign-extend the 13-bit shifted byte difference.
        shifted = (shifted << 19) >> 19;
        return Math.Clamp(
            (int)unchecked((short)shifted),
            -MetroidMaximumVelocity,
            MetroidMaximumVelocity);
    }

    /// <summary>Ports <c>Metroid_Func_3</c>; attachment is a hard positional lock.</summary>
    private static void RunMetroidAttached(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY)
    {
        slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, targetY));
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);
    }

    /// <summary>Ports <c>Metroid_Func_4</c>, including post-decrement timer semantics.</summary>
    internal static void RunMetroidPowerBombEscape(RoomEnemySlot slot, MetroidEnemyState state)
    {
        slot.Position += MetroidBehaviorDefinitions.EscapeDisplacement(state.EscapeTimer.RawValue);
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);

        ushort oldTimer = state.EscapeTimer.RawValue;
        state.EscapeTimer = WrappingCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
            return;

        state.Function = MetroidAiFunction.Homing;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
    }

    /// <summary>Ports the family-specific tail of <c>Metroid_Frozen</c>.</summary>
    internal static void RunMetroidFrozen(RoomEnemySlot slot)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        if (!state.EscapeTimer.IsZero)
        {
            state.EscapeTimer = state.EscapeTimer.Decrement().Remaining;
            slot.FlashTimer = WrappingCountdown.Start(2);
        }

        FreezeMetroidOuterBody(state.OuterBodyA, MetroidOuterBodyDefinitions.OuterBodyAFrozenProgram);
        FreezeMetroidOuterBody(state.OuterBodyB, MetroidOuterBodyDefinitions.OuterBodyBFrozenProgram);
    }

    /// <summary>Ports the palette-bit modulation in custom hurt AI <c>$A3:EB33</c>.</summary>
    internal static void ApplyMetroidHurt(RoomEnemySlot slot)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        if ((slot.FlashTimer.RawValue & 2) != 0)
        {
            state.OuterBodyA.Attributes = state.OuterBodyA.Attributes
                .WithPaletteBits(new SnesObjAttributeWord(checked((ushort)slot.PaletteIndex)).PaletteBits);
            state.OuterBodyB.Attributes = state.OuterBodyB.Attributes
                .WithPaletteBits(new SnesObjAttributeWord(checked((ushort)slot.PaletteIndex)).PaletteBits);
            return;
        }

        state.OuterBodyA.Attributes = state.OuterBodyA.Attributes.WithPaletteIndex(0);
        state.OuterBodyB.Attributes = state.OuterBodyB.Attributes.WithPaletteIndex(0);
    }

    /// <summary>Ports ordinary/contact-damage branches of <c>Metroid_Touch</c>.</summary>
    private void ResolveMetroidTouch(RoomEnemySlot slot, SamusState samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole - 8));
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            if (state.Function != MetroidAiFunction.AttachedToSamus)
            {
                // Native overlapping byte writes produce delta/4 in signed 16.16 form.
                StoreMetroidXVelocity(
                    state,
                    unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) << 14);
                StoreMetroidYVelocity(
                    state,
                    unchecked((short)(slot.Position.Y.Whole - targetY)) << 14);
                state.Function = MetroidAiFunction.Homing;
                SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
            }
            return;
        }

        if (state.Function != MetroidAiFunction.PowerBombEscape)
        {
            if ((_frameContext.RandomEnemyCounter & 7) == 7 && samus.Health >= 30)
                _audio.QueueEnemySound(Library3Sound.IncrementalEnergy, maximumQueued: SoundQueueLimit.Max6);
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Function >= MetroidAiFunction.AttachedToSamus)
            return;

        state.Function = MetroidAiFunction.ClosingOnSamus;
        if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) >= 8 ||
            Math.Abs(unchecked((short)(slot.Position.Y.Whole - targetY))) >= 8)
        {
            return;
        }

        state.Function = MetroidAiFunction.AttachedToSamus;
        samus.SpecialSuperPaletteFlags = 1;
        SetMetroidProgram(slot, state, MetroidProgram.DrainingSamus);
    }

    /// <summary>Subtracts one health at suit-dependent fractional intervals without ordinary hurt flash or knockback.</summary>
    private static void DrainSamusWithMetroid(MetroidEnemyState state, SamusState samus)
    {
        ushort drainRate = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? MetroidBehaviorDefinitions.GravityDrainFraction
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? MetroidBehaviorDefinitions.VariaDrainFraction
                : MetroidBehaviorDefinitions.PowerSuitDrainFraction;
        if (!state.DrainBorrows(drainRate))
            return;

        // Samus_DealDamage(1) performs a saturating subtract and deliberately does not
        // start ordinary hurt flash, invincibility, knockback, or suit division.
        samus.Health = samus.Health == 0 ? 0 : samus.Health - 1;
    }

    /// <summary>Handles non-frozen recoil/ice accumulation after bank-$A0 accepts a shot.</summary>
    internal void ResolveMetroidNonFrozenShot(
        RoomEnemySlot slot,
        ushort collisionProjectileType,
        ushort collisionProjectileDamage,
        Position16<RoomPixels> recoilOrigin,
        SamusState? samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        SamusProjectileFamily family = new SamusProjectileTypeWord(collisionProjectileType).Family;
        if (state.Function == MetroidAiFunction.AttachedToSamus)
        {
            // The bomb collision pass presents the exploding power bomb as family $0500;
            // family $0300 is its earlier expansion/controller record and is not the value
            // tested by Metroid_Shot at $A3:EF38.
            if (family == SamusProjectileFamily.Bomb)
                DetachMetroidWithPowerBomb(slot, state, samus);
            return;
        }

        // Metroid_Shot intentionally reads projectile position slot zero, not the current
        // collision slot. The overlapping byte stores encode delta/8 as signed 16.16.
        StoreMetroidXVelocity(
            state,
            unchecked((short)(slot.Position.X.Whole - recoilOrigin.X)) << 13);
        StoreMetroidYVelocity(
            state,
            unchecked((short)(slot.Position.Y.Whole - recoilOrigin.Y)) << 13);
        state.Function = MetroidAiFunction.Homing;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);

        if (new SamusProjectileTypeWord(collisionProjectileType).BeamCombination.HasIce)
        {
            slot.FlashTimer = WrappingCountdown.Start(4);
            if (collisionProjectileDamage >= state.IceDamageToFreeze)
            {
                state.IceDamageToFreeze = 0;
                slot.FrozenTimer = WrappingCountdown.Start(MetroidFrozenDuration);
                slot.AiStates |= EnemyAiStates.Frozen;
                return;
            }
            state.IceDamageToFreeze -= collisionProjectileDamage;
        }

        LastMetroidSoundEffectLibrary2 = EnemySoundEffectDefinitions.MetroidRecoilSoundEffect;
    }

    /// <summary>Releases an attached Metroid into its four-frame escape and clears Samus's drain palette flag.</summary>
    private static void DetachMetroidWithPowerBomb(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState? samus)
    {
        state.EscapeTimer = WrappingCountdown.Start(MetroidEscapeDuration);
        state.Function = MetroidAiFunction.PowerBombEscape;
        SetMetroidProgram(slot, state, MetroidProgram.ChasingSamus);
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    internal static void FinishMetroidDeath(RoomEnemySlot slot, SamusState? samus)
    {
        MetroidEnemyState state = slot.RequireSpeciesState<MetroidEnemyState>();
        state.Velocity = state.Velocity.WithWholeX(0);
        state.OuterBodyA.Cursor = null;
        state.OuterBodyB.Cursor = null;
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;
    }

    /// <summary>
    /// <c>MetroidDeathItemDropRoutine</c> ($A0:B968) around the origin $A3:EF31/EF38
    /// recorded before <c>EnemyDeath</c> cleared the slot.
    /// </summary>
    internal void RequestMetroidDrops(Position16<RoomPixels> origin)
    {
        for (int dropIndex = 0; dropIndex < MetroidSpecialDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            // Each axis adds a five-bit random byte minus sixteen: -16..+15 pixels.
            var scatter = new Offset16<RoomPixels>(
                unchecked((short)((random & EnemyDropMasks.ScatterBits) - 16)),
                unchecked((short)(((SnesLittleEndian.HighByte(random) & EnemyDropMasks.ScatterBits)) - 16)));
            MetroidDropRequests.Add(new MetroidDropRequest());
            _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(origin + scatter, EnemyDefinitionId.Metroid);
        }
    }

    /// <summary>Clamps the signed whole word of homing speed to three pixels while retaining in-range fractions.</summary>
    private static int ClampMetroidVelocity(int velocity) => EnemyMath.ClampWholeVelocity(velocity, MetroidMaximumVelocity);

    /// <summary>Stores the raw signed 16.16 recoil or homing X speed.</summary>
    private static void StoreMetroidXVelocity(MetroidEnemyState state, int velocity) =>
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromRaw(velocity));

    /// <summary>Stores the raw signed 16.16 recoil or homing Y speed.</summary>
    private static void StoreMetroidYVelocity(MetroidEnemyState state, int velocity) =>
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromRaw(velocity));

    /// <summary>Moves an outer-body sprite to the Metroid's position and enables its selected graphics.</summary>
    private static void PositionMetroidOuterBody(
        RoomSpriteObjectSlot outerBody,
        RoomEnemySlot metroid,
        ushort graphicsIndex)
    {
        outerBody.Position = outerBody.Position.WithWhole(metroid.Position.Whole);
        outerBody.Attributes = new SnesObjAttributeWord(graphicsIndex);
        outerBody.UpdatesDisabled = false;
    }

    /// <summary>Switches an outer-body sprite to frozen art without resetting its retained instruction timer.</summary>
    private static void FreezeMetroidOuterBody(
        RoomSpriteObjectSlot outerBody,
        RoomSpriteObjectProgram frozenProgram)
    {
        outerBody.Attributes = new SnesObjAttributeWord(MetroidOuterBodyDefinitions.FrozenSpritePalette);
        outerBody.UpdatesDisabled = true;
        // Native drawing reads the selected record even while animation is disabled.
        // Show the frozen program's first frame without resetting the retained timer.
        RoomSpriteObjectPool.ShowFirstFrameKeepingTimer(outerBody, frozenProgram);
    }

    /// <summary>Restarts a Metroid animation at timer one after its function changes.</summary>
    private static void SetMetroidProgram(RoomEnemySlot slot, MetroidEnemyState state, MetroidProgram program)
    {
        state.Program = AnimationProgramCursor<MetroidProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Routes bomb-family collisions through Metroid's attachment and recoil rules.</summary>
    internal void AddMetroidBombReactions(EnemySharedReactions table)
    {
        // Metroid shot AI ignores bombs while frozen, detaches an attached body for family $0500, or applies its
        // peculiar slot-zero recoil otherwise.
        table.AddBombSpecial(c => c.Enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                  EnemyCollision.ShotAi(c.Enemy) == EnemyInteractionCallback.MetroidShot,
            c =>
            {
                if (c.Enemy.FrozenTimer.IsZero)
                {
                    SamusProjectileSlot recoilOrigin = c.OrdinaryProjectiles.Slots[0];
                    ResolveMetroidNonFrozenShot(c.Enemy, c.Bomb.PackedType.Raw, checked((ushort)c.Bomb.Damage), recoilOrigin.Position.Whole, c.Samus);
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
        EnemyProgramAnimation.Advance(slot, MetroidAnimations.Programs, MetroidFrameDefinitions.ShownFrames.Of,
            static (MetroidEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyMetroidCue(cue));

    /// <summary>Registers the Metroid Power Bomb callback that clears its outer body before generic death.</summary>
    internal static void AddMetroidPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.MetroidPowerBomb)
        {
            BeforeGenericDeath = FinishMetroidDeath,
        });
    }

    /// <summary>Registers Metroid touch behavior, including attachment and suit-dependent draining.</summary>
    internal void AddMetroidTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Metroid &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MetroidTouch,
            context => ResolveMetroidTouch(context.Slot, context.Samus));
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastMetroidSoundEffectLibrary2 = null;
    }

    /// <summary>Applies frozen missile damage or unfrozen recoil and Ice accumulation after the shared projectile prelude.</summary>
    internal bool ResolveMetroidOrdinaryShot(in EnemyShotContext c)
    {
        RoomEnemySlot enemy = c.Enemy;
        if (!enemy.FrozenTimer.IsZero)
        {
            if (c.Family is not SamusProjectileFamily.Missile and not SamusProjectileFamily.SuperMissile)
            {
                // Frozen Metroid rejects every beam and bomb family. Bank-$A0 has already accepted the overlap, so
                // non-plasma shots retain only the collision mark and never enter normal damage AI.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                return true;
            }

            // Like the ordinary path, the bank-$A0 walker only marks the direction word. $A0:A184 does not reject a
            // projectile an earlier enemy marked this pass: a Super Missile that just hit a Rinka still reaches
            // EnemyShot_Metroid.
            c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);

            byte frozenVulnerability = EnemyCollision.ReadProjectileVulnerability(enemy, c.ProjectileType);
            int frozenDamage = (c.ProjectileDamage >> 1) * EnemyVulnerabilityDefinitions.Multiplier(frozenVulnerability);
            if (!EnemyVulnerabilityDefinitions.IsSentinel(frozenVulnerability) && frozenDamage != 0)
            {
                enemy.FlashTimer = WrappingCountdown.Start(enemy.HurtFlashFrames + 8);
                enemy.AiStates |= EnemyAiStates.Hurt;
                enemy.Health = frozenDamage >= enemy.Health
                    ? 0
                    : enemy.Health - frozenDamage;
            }

            if (enemy.Health == 0)
            {
                // $A3:EF31 records the drop origin, EnemyDeath ($A3:EF4B) spawns the explosion, and only then
                // $A3:EF74 spawns the five drops.
                Position16<RoomPixels> dropOrigin = enemy.Position.Whole;
                FinishMetroidDeath(enemy, c.Samus);
                _deathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 4);
                RequestMetroidDrops(dropOrigin);
            }
            return true;
        }

        // Custom Metroid shot AI does not call normal shot AI. Preserve the collision prelude, then apply recoil/ice
        // or attached power-bomb escape without inventing vulnerability-based health damage.
        c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
        ResolveMetroidNonFrozenShot(enemy, c.Projectile.PackedType.Raw, checked((ushort)c.Projectile.Damage), c.Projectiles.Slots[0].Position.Whole, c.Samus);
        return true;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueEnemySoundIfAny(LastMetroidSoundEffectLibrary2, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>Routes ordinary projectile hits to Metroid-specific frozen and nonfrozen shot handling.</summary>
    internal void AddMetroidShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Metroid &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MetroidShot)
        {
            AdmitsBombFamilies = true,
            AfterSuperMissileQuake = ResolveMetroidOrdinaryShot,
        });
    }

    /// <summary>Registers hurt AI ahead of frozen AI when both native handler bits are set; animation bytecode still advances.</summary>
    internal static void AddMetroidHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.Metroid, (slot, _) => ApplyMetroidHurt(slot));

    /// <summary>Registers the frozen Metroid handler that retains its outer-body state.</summary>
    internal static void AddMetroidFrozenAi(EnemySharedReactions table) =>
        table.AddFrozen(slot => slot.DefinitionId == EnemyDefinitionId.Metroid, RunMetroidFrozen);
}
