using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's hitboxes, touch, projectiles and hurt palette, and what Samus's weapons do to it.</summary>
[RoomLifetime]
internal sealed class CrocomireCombat
{
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns Crocomire body, skeleton, and melting tile uploads.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing and room effects to Crocomire AI.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves Crocomire body and tongue contact with Samus.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Produces the native distributed drops at skeleton collapse.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
    private readonly CrocomireEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireCombat(EnemyProjectileEngine projectiles, EnemyPresentation presentation, EnemyFrameContext frameContext, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, CrocomireEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _projectiles = projectiles;
        _presentation = presentation;
        _frameContext = frameContext;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Ports Crocomire's eight-color body hurt flash at $A4:8CCB.</summary>
    internal void ApplyCrocomireHurtPalette(RoomEnemySlot body)
    {
        bool white = !body.FlashTimer.IsZero && (_frameContext.RandomEnemyCounter & 2) != 0;
        if (!white)
        {
            (_bossArtwork.Artwork?.CrocomireColors ?? throw new InvalidDataException(
                "Crocomire hurt palette requires installed artwork.")).ApplyFightBody(_presentation.Cgram);
            return;
        }
        _presentation.BossPalettes.CrocomireFightBody.Fill(Bgr555.White);
    }

    /// <summary>Registers component-specific bomb hit routines, including the native no-op gates.</summary>
    internal void RegisterCrocomireBombReactions(EnemySharedReactions table)
    {
        // Specials consulted before the selected literal no-op gate, in the original order.
        // Crocomire has no header shot callback; every live component supplies its own bank-$A4 routine. Bomb
        // family $0500 therefore reaches the same no-op counter, dust, or mouth reaction selected by the map.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                  c.Enemy.Definition.ShotAiPointer == 0 && c.UsesExtendedHitboxes &&
                                  c.SelectedShotAi != EnemyInteractionCallback.NormalEnemyShot,
            c => ResolveCrocomireHitboxShot(c.Bomb.PackedType.Raw, c.Bomb.Position.Whole, c.SelectedShotAi));
    }

    /// <summary>Registers Crocomire's owned projectile setup, movement, and instruction handlers.</summary>
    internal void RegisterCrocomireProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            StartCrocomireProjectileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomiresProjectile_Setup);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireProjectileFlight(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomiresProjectile_Fired);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireSpikeWallPiece(context.Projectile);
        }, EnemyProjectilePreInstruction.CrocomireSpikeWallPieces);

        table.RegisterPreInstruction(context =>
        {
            RunCrocomireBridgeFragment(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.CrocomireBridgeCrumbling);
    }

    /// <summary>Routes power-bomb hits on each Crocomire component to their authored reactions.</summary>
    internal void RegisterCrocomirePowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.CrocomirePowerBomb)
        {
            ReplacesCommon = (enemy, _) => ResolveCrocomirePowerBombReaction(enemy),
        });
    }

    /// <summary>Routes body, mouth, and auxiliary hitboxes to their distinct shot reactions.</summary>
    internal void RegisterCrocomireShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                                                     enemy.Definition.ShotAiPointer == 0 ||
                                                     enemy.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.NormalEnemyShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (c.HitboxShotAi == EnemyInteractionCallback.NormalEnemyShot)
                    return false;
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Crocomire components require extended-spritemap collision.");
                c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ResolveCrocomireHitboxShot(c.Projectile.PackedType.Raw, c.Projectile.Position.Whole, c.HitboxShotAi);
                return true;
            },
        });
    }

    /// <summary>Registers Crocomire component contact, including normal enemy touch damage.</summary>
    internal void RegisterCrocomireTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.CrocomireHeaderTouch ||
                               slot.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.NormalEnemyTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Crocomire components require extended-spritemap collision.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.CrocomireHeaderTouch)
                    return; // $A4:B950 is a literal RTL.
                if (context.HitboxTouchAi is not (
                        EnemyInteractionCallback.CrocomireClawTouch or EnemyInteractionCallback.NormalEnemyTouch))
                {
                    throw new InvalidDataException(
                        $"Crocomire hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }

                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                if (context.HitboxTouchAi == EnemyInteractionCallback.CrocomireClawTouch)
                {
                    CrocomireEnemyState crocomire = _access.Crocomire ??
                        throw new InvalidOperationException("Crocomire claw has no body owner.");
                    crocomire.FightFlags |= CrocomireFightFlags.ClawTouchedSamus;
                    context.Samus.Kinematics.ExtraDisplacement = context.Samus.Kinematics.ExtraDisplacement.WithWholeX(-4);
                }
            });
    }

    /// <summary>Dispatches $A4:B951/$B968/$BA05/$BAB4 after multibox collision.</summary>
    private void ResolveCrocomireHitboxShot(
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile,
        EnemyInteractionCallback? callback)
    {
        CrocomireEnemyState state = _access.Crocomire ??
            throw new InvalidOperationException("Crocomire hitbox has no body owner.");
        RoomEnemySlot body = state.Body;

        switch (callback)
        {
            case EnemyInteractionCallback.CrocomireNoOpHitboxShot:
            {
                // $B951 increments a temporary low-nibble value, then ORs it back into the
                // flags word instead of replacing the nibble. Preserve that odd instruction
                // sequence; changing it to an ordinary counter disagrees for several values.
                state.HitCount |= CrocomireFightFlagBits.NextHitCount(state.HitCount);
                return;
            }

            case EnemyInteractionCallback.CrocomireDustHitboxShot:
            case EnemyInteractionCallback.CrocomireAlternateDustHitboxShot:
                SpawnCrocomireShotDust(projectileType, projectile);
                return;

            case EnemyInteractionCallback.CrocomireMouthShot:
                ResolveCrocomireMouthShot(
                    state,
                    body,
                    projectileType,
                    projectile);
                return;

            case EnemyInteractionCallback.CrocomireHeaderTouch:
                // Some extended-map records deliberately point at the body header's RTL.
                return;

            case EnemyInteractionCallback.NormalEnemyTouch:
            case EnemyInteractionCallback.NormalEnemyShot:
            case EnemyInteractionCallback.DudShot:
            case EnemyInteractionCallback.NoOp:
            case EnemyInteractionCallback.NoOpShortReturn:
            case EnemyInteractionCallback.OwtchShot:
            case EnemyInteractionCallback.DragonTouch:
            case EnemyInteractionCallback.DragonShot:
            case EnemyInteractionCallback.DragonPowerBomb:
            case EnemyInteractionCallback.GRipperRipper2Shot:
            case EnemyInteractionCallback.MamaTurtleTouch:
            case EnemyInteractionCallback.BabyTurtleTouch:
            case EnemyInteractionCallback.BabyTurtleShot:
            case EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch:
            case EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch:
            case EnemyInteractionCallback.MaridiaLargeSnailShot:
            case EnemyInteractionCallback.RinkaTouch:
            case EnemyInteractionCallback.RinkaShot:
            case EnemyInteractionCallback.RinkaPowerBomb:
            case EnemyInteractionCallback.VerticalShutterTouch:
            case EnemyInteractionCallback.ShootableVerticalShutterShot:
            case EnemyInteractionCallback.DestroyableVerticalShutterShot:
            case EnemyInteractionCallback.VerticalShutterPowerBomb:
            case EnemyInteractionCallback.HorizontalShutterTouch:
            case EnemyInteractionCallback.HorizontalShutterShot:
            case EnemyInteractionCallback.HorizontalShutterPowerBomb:
            case EnemyInteractionCallback.SkreeShot:
            case EnemyInteractionCallback.MetareeShot:
            case EnemyInteractionCallback.FirefleaTouch:
            case EnemyInteractionCallback.FirefleaPowerBomb:
            case EnemyInteractionCallback.FirefleaShot:
            case EnemyInteractionCallback.MochtroidTouch:
            case EnemyInteractionCallback.MochtroidShot:
            case EnemyInteractionCallback.MetroidTouch:
            case EnemyInteractionCallback.MetroidShot:
            case EnemyInteractionCallback.MetroidPowerBomb:
            case EnemyInteractionCallback.YardTouch:
            case EnemyInteractionCallback.YardShot:
            case EnemyInteractionCallback.PlatformNoOpTouch:
            case EnemyInteractionCallback.TripperShot:
            case EnemyInteractionCallback.CrocomireClawTouch:
            case EnemyInteractionCallback.CrocomirePowerBomb:
            case EnemyInteractionCallback.SporeSpawnTouch:
            case EnemyInteractionCallback.SporeSpawnShot:
            case EnemyInteractionCallback.DraygonTouch:
            case EnemyInteractionCallback.DraygonShot:
            case EnemyInteractionCallback.DraygonPowerBomb:
            case EnemyInteractionCallback.FakeKraidTouch:
            case EnemyInteractionCallback.FakeKraidShot:
            case EnemyInteractionCallback.CeresSteamTouch:
            case EnemyInteractionCallback.RidleyExtendedTouch:
            case EnemyInteractionCallback.RidleyShot:
            case EnemyInteractionCallback.RidleyPowerBomb:
            case EnemyInteractionCallback.ZebetiteTouch:
            case EnemyInteractionCallback.ZebetiteShot:
            case EnemyInteractionCallback.KraidBackgroundTouch:
            case EnemyInteractionCallback.KraidArmTouch:
            case EnemyInteractionCallback.KraidNoOpShot:
            case EnemyInteractionCallback.KraidArmShot:
            case EnemyInteractionCallback.KraidNailTouch:
            case EnemyInteractionCallback.KraidBadNailTouch:
            case EnemyInteractionCallback.EvirTouch:
            case EnemyInteractionCallback.EvirPowerBomb:
            case EnemyInteractionCallback.EvirShot:
            case EnemyInteractionCallback.MagdollitePowerBomb:
            case EnemyInteractionCallback.MagdolliteTouch:
            case EnemyInteractionCallback.MagdolliteShot:
            case EnemyInteractionCallback.BeetomTouch:
            case EnemyInteractionCallback.BeetomShot:
            case EnemyInteractionCallback.PowampTouch:
            case EnemyInteractionCallback.PowampShot:
            case EnemyInteractionCallback.PowampPowerBomb:
            case EnemyInteractionCallback.WorkRobotTouch:
            case EnemyInteractionCallback.WorkRobotNoPowerShot:
            case EnemyInteractionCallback.WorkRobotShot:
            case EnemyInteractionCallback.BullShot:
            case EnemyInteractionCallback.KagoShot:
            case EnemyInteractionCallback.KiHunterShot:
            case EnemyInteractionCallback.SparkShot:
            case EnemyInteractionCallback.BlueBrinstarFaceBlockShot:
            case EnemyInteractionCallback.YappingMawTouch:
            case EnemyInteractionCallback.YappingMawShot:
            case EnemyInteractionCallback.MotherBrainBodyShot:
            case EnemyInteractionCallback.MotherBrainHeadShot:
            case EnemyInteractionCallback.MotherBrainBodyTouch:
            case EnemyInteractionCallback.MotherBrainHeadTouch:
            case EnemyInteractionCallback.DeadTorizoTouchAndShot:
            case EnemyInteractionCallback.DeadTorizoPowerBomb:
            case EnemyInteractionCallback.DeadSidehopperTouch:
            case EnemyInteractionCallback.DeadSidehopperShot:
            case EnemyInteractionCallback.DeadSidehopperPowerBomb:
            case EnemyInteractionCallback.ShitroidTouch:
            case EnemyInteractionCallback.ShitroidShot:
            case EnemyInteractionCallback.ShitroidPowerBomb:
            case EnemyInteractionCallback.BombTorizoTouch:
            case EnemyInteractionCallback.BombTorizoShot:
            case EnemyInteractionCallback.TorizoStandUpSitDownShot:
            case EnemyInteractionCallback.GoldenTorizoShot:
            case EnemyInteractionCallback.ShaktoolTouch:
            case EnemyInteractionCallback.ShaktoolShot:
            case EnemyInteractionCallback.SpacePiratePowerBomb:
            case EnemyInteractionCallback.SpacePirateTouch:
            case EnemyInteractionCallback.SpacePirateShot:
            case EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot:
            case EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot:
            case EnemyInteractionCallback.BotwoonTouch:
            case EnemyInteractionCallback.BotwoonShot:
            case EnemyInteractionCallback.BotwoonPowerBomb:
            case EnemyInteractionCallback.PhantoonTouch:
            case EnemyInteractionCallback.PhantoonShot:
            case EnemyInteractionCallback.DeadZoomerTouchAndShot:
            case EnemyInteractionCallback.DeadZoomerPowerBomb:
            case EnemyInteractionCallback.DeadRipperTouchAndShot:
            case EnemyInteractionCallback.DeadRipperPowerBomb:
            case EnemyInteractionCallback.DeadSkreeTouchAndShot:
            case EnemyInteractionCallback.DeadSkreePowerBomb:
            case EnemyInteractionCallback.CeresDoorNoOpShot:
            case EnemyInteractionCallback.ChozoNoOpShot:
            default:
                throw new InvalidOperationException($"Crocomire hitbox shot AI {callback?.ToString() ?? "(unnamed)"} is not translated.");
        }
    }

    /// <summary>Ports Crocomire's mouth reaction at $A4:BA05.</summary>
    private void ResolveCrocomireMouthShot(
        CrocomireEnemyState state,
        RoomEnemySlot body,
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile)
    {
        body.InvincibilityTimer = NativeCountdown.Start(0);

        // The off-screen branch bypasses projectile-family selection and still publishes
        // the pending-hit state. This wrapped signed comparison is the cartridge's exact
        // `body left edge - (camera + 256)` test.
        bool bodyLeftEdgeIsOnScreen = unchecked((short)(
            body.Position.X.Whole - body.Radius.X - 256 - state.Camera.X)) < 0;
        int stepCount = 0;
        SamusProjectileFamily family = projectileType.Family;
        if (bodyLeftEdgeIsOnScreen)
        {
            if (family == 0)
            {
                if (!projectileType.IsChargedBeam)
                {
                    body.InstructionTimer = NativeCountdown.Start(8);
                    SpawnCrocomireShotDust(projectileType, projectile);
                    return;
                }
                stepCount = 2;
            }
            else if (family == SamusProjectileFamily.Missile)
            {
                stepCount = 1;
            }
            else if (family == SamusProjectileFamily.SuperMissile)
            {
                stepCount = 3;
            }
        }

        if (!bodyLeftEdgeIsOnScreen || stepCount != 0)
        {
            state.StepCounter += stepCount;
            int hitCount = CrocomireFightFlagBits.NextHitCount(state.HitCount);
            if ((state.FightFlags & CrocomireFightFlags.DamageReactionPending) == 0)
            {
                // Both regional constants at $A4:8692/$8694 are eight. Keep the branch
                // explicit because the source distinguishes projectile-attack state even
                // though this ROM revision stores identical delay values.
                body.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(
                    body.InstructionTimer.RawValue +
                    (state.FightFunction == CrocomireFightFunction.ProjectileAttack ? 8 : 8))));
            }
            state.HitCount = hitCount;
            state.FightFlags = (state.FightFlags & ~(CrocomireFightFlags.DamageReactionPending | CrocomireFightFlags.ClawTouchedSamus)) |
                CrocomireFightFlags.DamageReactionPending;
            state.ReactionTimer = 10;
        }

        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 14)));
        body.AiStates |= EnemyAiStates.Hurt;
    }

    /// <summary>Places a dust effect at the shot impact while preserving its component-relative offset.</summary>
    private void SpawnCrocomireShotDust(
        SamusProjectileTypeWord projectileType,
        Position16<RoomPixels> projectile)
    {
        MiscDustAnimation animationIndex = projectileType.Family != SamusProjectileFamily.SuperMissile
            ? MiscDustAnimation.DudShotTinyExplosion
            : MiscDustAnimation.BigExplosion;
        _projectiles.SpawnRoomGraphicsDustExplosion(projectile, animationIndex);
    }

    /// <summary>Ports $A4:B992 without applying normal power-bomb HP damage.</summary>
    private void ResolveCrocomirePowerBombReaction(RoomEnemySlot body)
    {
        CrocomireEnemyState state = _access.RequireCrocomire(body);
        if (state.DeathSequenceIndex != CrocomireDeathPhase.Fighting)
            return;

        state.StepCounter = 3;
        if (state.FightFunction == CrocomireFightFunction.PowerBombCharge)
            return;

        state.HitCount = 0;
        state.FightFlags = (state.FightFlags & ~(CrocomireFightFlags.ClawTouchedSamus | CrocomireFightFlags.Engaged)) |
            CrocomireFightFlags.Engaged;
        state.ReactionTimer = 10;
        body.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(body.FlashTimer.RawValue + 4)));
        body.AiStates |= EnemyAiStates.Hurt;
        state.FightFunction = CrocomireFightFunction.PowerBombCharge;

        CrocomireEncounterAccess.StartCrocomireBodyProgram(state, SelectCrocomirePowerBombProgram(state));
    }

    /// <summary>
    /// Selects the fully-open, part-open or closed reaction from the mouth of the frame shown, as $A4:B9D8 does
    /// by scanning the frame's spritemap. Before any frame is shown the body has no spritemap and reacts closed.
    /// </summary>
    private static CrocomireBodyProgram SelectCrocomirePowerBombProgram(CrocomireEnemyState state) =>
        state.BodyFrame is { } frame
            ? CrocomirePowerBombReactionDefinitions.ForFrame(frame)
            : CrocomireBodyProgram.PowerBombReactionMouthNotOpen;

    /// <summary>Ports the one-frame vector setup at $86:906B.</summary>
    internal void StartCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // The native setup performs one horizontal collision move and deliberately ignores
        // carry. It also writes the graphics word of physical projectile slot zero rather
        // than the current slot; retain that shipped indexing bug for debugger parity.
        EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        _projectiles.EnemyProjectiles[0].GraphicsIndex = EnemyPaletteBits.Palette5;
        projectile.GeneralTimer = unchecked((ushort)(
            projectile.GeneralTimer + projectile.Velocity.X.RawValue));

        // Spawn parameters are 2,4,...,18. The last index reads one word beyond the declared
        // nine-word gradient table. The compiled definition includes those exact setup
        // instruction bytes; do not clamp the final shot to the last authored gradient.
        short gradient = CrocomireProjectileRomData.GradientForSpawnParameter(checked((ushort)projectile.DirectionParameter));
        // The native angle starts at up, not right: Y uses negative cosine.
        projectile.Velocity = CalculateCrocomireProjectileVelocity(EnemyMath.CalculateCartridgeAngle(new(-64, gradient)));
        projectile.PreInstruction =
            EnemyProjectilePreInstruction.CrocomiresProjectile_Fired;
    }

    /// <summary>Ports <c>sub_8690B3</c>: delete on the first horizontal/vertical wall hit.</summary>
    internal static void RunCrocomireProjectileFlight(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Clear();
        }
    }

    /// <summary>Projects a projectile angle into signed 8.8 X/Y speed using native word shifts.</summary>
    internal static EightEightVector<RoomPixels> CalculateCrocomireProjectileVelocity(SnesBearing bearing)
    {
        // Preserve the two native word shifts, including negative two's-complement bits. The
        // table is read with the bearing unconverted, so X is its sine and Y its negative cosine.
        return new(
            unchecked((short)(EnemyTrigonometryTables.SignedSine(bearing.ReinterpretAsTableAngle()) *
                CrocomireProjectileRomData.VelocityMultiplier)),
            unchecked((short)(EnemyTrigonometryTables.SignedNegativeCosineWord(bearing.ReinterpretAsTableAngle().RawValue) *
                CrocomireProjectileRomData.VelocityMultiplier)));
    }

    /// <summary>Ports the byte-precise slot-dependent shard motion at $86:9115.</summary>
    internal void RunCrocomireSpikeWallPiece(RoomEnemyProjectileSlot projectile)
    {
        int slot = projectile.SlotIndex;
        ushort maximumAcceleration = CrocomireSpikeMotionDefinitions.MaximumAcceleration(slot);
        ushort acceleration = checked((ushort)projectile.Variable0);
        if (acceleration != maximumAcceleration)
        {
            acceleration = unchecked((ushort)(
                acceleration + CrocomireSpikeMotionDefinitions.AccelerationDelta(slot)));
            if (acceleration >= maximumAcceleration)
                acceleration = maximumAcceleration;
        }
        projectile.Variable0 = acceleration;

        // The 65816 switches to 8-bit A here. The high acceleration byte is added to the
        // fractional velocity byte; carry advances the signed integral velocity byte, which
        // is then capped by this physical slot's maximum.
        int velocityFractionSum = (SnesEightEight.FractionOf(projectile.Velocity.X.RawValue)) + (SnesLittleEndian.HighByte(acceleration));
        byte velocityFraction = unchecked((byte)velocityFractionSum);
        byte velocityWhole = unchecked((byte)(
            (SnesEightEight.WholeOf(projectile.Velocity.X.RawValue)) + (SnesCarry.OutOfByte(velocityFractionSum))));
        byte maximumVelocity = CrocomireSpikeMotionDefinitions.MaximumVelocity(slot);
        if (unchecked((sbyte)(velocityWhole - maximumVelocity)) >= 0)
            velocityWhole = maximumVelocity;
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(unchecked(SnesLittleEndian.Word(velocityFraction, velocityWhole))));

        // Move X with the same split-byte math. Only the high subposition byte participates;
        // its low byte remains untouched, exactly like EnemyProjectile_XSubPositions+1,X.
        SnesFixedPosition x = new SnesFixedPosition(projectile.Position.X.Whole, projectile.Position.X.Fraction)
            .AddEightEightToHighFraction(SnesLittleEndian.Word(velocityFraction, velocityWhole));
        projectile.Position = projectile.Position.WithFractionX(x.Fraction).WithWholeX(x.Whole);

        // Y velocity and position are ordinary signed 16.16 pairs, with $3000 acceleration.
        uint velocity = unchecked(SnesSixteenSixteen.Compose(projectile.Velocity.Y.RawValue, checked((ushort)projectile.Variable1)));
        velocity = unchecked(velocity + 12288u);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)))));
        projectile.Variable1 = unchecked((ushort)velocity);
        uint position = unchecked(projectile.Position.Y.RawValue);
        position = unchecked(position + velocity);
        projectile.Position = projectile.Position.WithY(SnesFixedPosition.FromRaw(unchecked(position)));

        if (projectile.Position.Y.Whole < 168)
            return;

        ushort impactX = projectile.Position.X.Whole;
        ushort impactY = projectile.Position.Y.Whole;
        int nativeIndex = projectile.SlotIndex * 2;
        projectile.Clear();
        if ((nativeIndex & 2) == 0)
            _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.Crumble;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(impactX, impactY), MiscDustAnimation.BigDustCloud);
        _access.LastCrocomireSoundEffect = CrocomireSoundDefinitions.BigExplosion;
    }

    /// <summary>Ports bridge-fragment vertical collision and gravity at $86:92BA.</summary>
    internal static void RunCrocomireBridgeFragment(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Clear();
            return;
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            (projectile.Velocity.Y.RawValue + 24) & 16383))));
    }

    /// <summary>
    /// Registers the mouth projectile's typed shot program and its cues: the drop is allocated with Crocomire's
    /// chances before the deletion frees the exploding actor.
    /// </summary>
    internal void RegisterCrocomireDropProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterShotAnimation(RoomEnemyProjectileKind.CrocomireProjectile,
            () => new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Shot));
        table.RegisterCue<CrocomireProjectileCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case CrocomireProjectileCue.SpawnDrops:
                    _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(projectile.Position.Whole, EnemyDefinitionId.Crocomire);
                    return;
                case CrocomireProjectileCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(CrocomireProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindCrocomireBodyHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
            enemy.Definition.Bank == CrocomireBodyCollisionDefinitions.Bank &&
            CrocomireBodyCollisionDefinitions.HasFrame(enemy.Frame)))
            return null;

        // Physical body components and their callback lists are compiled
        // independently of Crocomire's replaceable OAM/BG2 presentation.
        // Preserve native component/hitbox order: the first overlap wins.
        foreach (CrocomireBodyCollisionComponent component in
                 CrocomireBodyCollisionDefinitions.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (CrocomireBodyCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback ? CrocomireHitboxCallbackCodec.ToCallback(hitbox.Shot) : CrocomireHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindCrocomireTongueHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.EnemyDefinitionPointer == EnemyDefinitionId.CrocomireTongue &&
            enemy.Definition.Bank == CrocomireTongueCollisionDefinitions.Bank &&
            (CrocomireTongueCollisionDefinitions.HasFrame(EnemyFrames.Spritemap(enemy.Frame)) ||
            probe.ArtworkInstalled)))
            return null;

        // These nine selected frames have one component each, but both
        // referenced native hitbox lists are empty. Validate the exact
        // engine-owned frame identity and return no touch/shot callback.
        CrocomireTongueCollisionComponent component =
            CrocomireTongueCollisionDefinitions.ComponentAt(EnemyFrames.Spritemap(enemy.Frame));
        _ = CrocomireTongueCollisionDefinitions.HitboxCountAt(
            component.HitboxPointer);
        return false;
    
    }
}
