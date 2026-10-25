using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The Spore Spawn species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SporeSpawnBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.SporeSpawn];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SporeSpawnEntity(this, slot, common);

    /// <summary>Provides boss event state, installed colors, and the room RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates stalks, spore emitters, spores, and hardening dust effects.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns visual dust and dying explosions.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Receives live and death palette writes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Exposes the enemy-frame counter and process-all-enemies override.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves core-hitbox touch damage through shared enemy collision.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Creates item drops from Spore Spawn and its spores.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Captures the room and combat systems used by boss and spore behavior.</summary>
    internal SporeSpawnBehavior(SporeSpawnBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _spriteObjects = dependencies.SpriteObjects;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>Fixed horizontal arena center used for hardening and the death trajectory.</summary>
    private const ushort SporeSpawnDeathCenterX = 128;
    /// <summary>Fixed vertical arena center used for hardening and the death trajectory.</summary>
    private const ushort SporeSpawnDeathCenterY = 624;
    /// <summary>Horizontal sway amplitude in room pixels the descent leaves for the fight's first sway.</summary>
    private const int SporeSpawnDescentMaximumXRadius = 48;
    /// <summary>Sway amplitude in room pixels below which each closing widens the sway.</summary>
    private const int SporeSpawnWidenedRadiusLimit = 40;
    /// <summary>Room pixels each closing adds to a sway narrower than the limit.</summary>
    private const int SporeSpawnRadiusWidening = 8;
    /// <summary>Ceiling Y coordinate anchoring the four stalk projectiles.</summary>
    private const ushort SporeSpawnCeilingY = 560;

    /// <summary>Frame-local drop publications from boss death and individual spores.</summary>
    internal readonly List<SporeSpawnDropRequest> SporeSpawnDropRequests = new();

    /// <summary>Last sound requested by the boss or one of its instructions in the current enemy frame.</summary>
    public SoundEffectId? LastSporeSpawnSoundEffect { get; private set; }

    /// <summary>
    /// Runs the global scrolling-finished callback installed by live Spore Spawn at
    /// <c>$A5:EAD7-$EADA</c>. Bank $90 invokes it after both ordinary camera axes have
    /// finished and before background streaming observes the final layer-one words.
    /// </summary>
    /// <param name="camera">Live room camera whose layer-one Y is raised to the arena minimum of 464 room pixels when the hook is active.</param>
    /// <exception cref="ArgumentNullException"><paramref name="camera"/> is null.</exception>
    public void RunScrollingFinishedHook(ScrollBoundaryCamera camera)
    {
        Ensure.NotNull(camera);
        if (SporeSpawn?.ScrollClampHookActive != true)
            return;

        // `$90:9589` loads $01D0, compares it with layer1_y_pos, and returns on BCC.
        // Equality deliberately performs the harmless native store; a larger camera Y is
        // already below the arena ceiling and is therefore left untouched.
        if (SporeSpawnScrollingHooks.FightMinimumLayerOneY < camera.PixelPosition.Y)
            return;

        camera.SetLayerOneYFromScrollingFinishedHook(
            SporeSpawnScrollingHooks.FightMinimumLayerOneY);
    }

    /// <summary>Gets the single boss state and verifies it belongs to the supplied body slot.</summary>
    private SporeSpawnEnemyState RequireSporeSpawnState(RoomEnemySlot slot)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn main AI ran without an initialized boss extension.");
        if (!ReferenceEquals(state.Body, slot))
            throw new InvalidOperationException("Spore Spawn extension belongs to another enemy slot.");
        return state;
    }

    /// <summary>Ports <c>SporeSpawn_Init</c> at $A5:EA2A.</summary>
    internal void InitializeSporeSpawn(RoomEnemySlot body)
    {
        if (SporeSpawn is not null)
            throw new InvalidDataException("A room population contains more than one Spore Spawn body.");

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            StalkAnchor = body.Position.Whole + new Offset16<RoomPixels>(0, -72),
            MovementCenter = body.Position.Whole,
        };
        SporeSpawn = state;

        // Native writes these sixteen colors to target-palette row F. Room loading currently
        // presents completed fades directly, so install the identical final words in CGRAM
        // while retaining the independent target copy above.
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            Bgr555 value = (_bossArtwork.Artwork?.SporeSpawnColors ?? throw new InvalidDataException(
                "Spore Spawn spore palette requires installed artwork.")).ResolveSpore(color);
            state.WriteTargetColor(BossColourDefinitions.SporeSpawnSpores, color, value);
            _presentation.BossColours().SporeSpawnSpores.Write(color, value);
        }

        // SpawnEprojWithGfx searches the eighteen-slot pool downward. Four calls therefore
        // occupy physical slots 17..14, which SporeSpawn_Func_5 later addresses directly.
        for (int argument = 0; argument < 4; argument++)
            SpawnSporeSpawnStalk(body, argument);

        state.LoadedAsDefeated = _room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss);
        if (state.LoadedAsDefeated)
        {
            state.BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.InitialDead);
            body.Properties = body.Properties.With(EnemyProperties.SolidToSamus);
            UpdateSporeSpawnStalks(state);
            PublishSporeSpawnPlm(header: PlmHeaderId.ClearSporeSpawnCeiling);
            return;
        }

        state.BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.InitialAlive);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 128)));

        // `flag_process_all_enemies = FFFF` is a global bank-$A0 activity override, not an
        // enemy property. Preserve that distinction so all room actors use the same scan.
        _frameContext.RequestFullSweep();
        state.ScrollClampHookActive = true;
        for (int argument = 0; argument < 4; argument++)
            SpawnSporeSpawnSpawner(body, argument);
        UpdateSporeSpawnStalks(state);
    }

    /// <summary>Dispatches <c>SporeSpawn_Main</c> at $A5:EB13.</summary>
    private void RunSporeSpawnMain(
        RoomEnemySlot body,
        SporeSpawnEnemyState state)
    {
        switch (state.Function)
        {
            case SporeSpawnFunction.Idle:
                return;

            case SporeSpawnFunction.Descending:
                RunSporeSpawnDescent(body, state);
                return;

            case SporeSpawnFunction.Moving:
                RunSporeSpawnMovement(body, state);
                return;

            case SporeSpawnFunction.SetupDeath:
                SetupSporeSpawnDeathVelocity(body, state);
                return;

            case SporeSpawnFunction.Dying:
                RunSporeSpawnDying(body, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Spore Spawn function {state.Function} is not translated.");
        }
    }

    /// <summary>Ports <c>SporeSpawn_Func_1</c> at $A5:EB1B.</summary>
    private void RunSporeSpawnDescent(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - SporeSpawnDeathCenterY)) >= 0)
        {
            state.BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.FightStarted);
            body.InstructionTimer = WrappingCountdown.Start(1);
        }

        state.MaximumXRadius = SporeSpawnDescentMaximumXRadius;
        state.AngleDelta = 1;
        state.Angle = RotationAngle.ThreeQuarterTurn;
    }

    /// <summary>Ports the table-backed ellipse in <c>SporeSpawn_Func_2</c> at $A5:EB52.</summary>
    private void RunSporeSpawnMovement(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        UpdateSporeSpawnStalks(state);
        body.Position = body.Position.WithWholeX(unchecked((ushort)(
            state.MovementCenter.X +
            EnemyMath.ReadEightBitCosineProduct(state.Angle.ToByteAngle(), checked((ushort)state.MaximumXRadius)))));
        body.Position = body.Position.WithWholeY(unchecked((ushort)(
            state.MovementCenter.Y +
            EnemyMath.ReadEightBitNegativeSineProduct(
                new ByteAngle(unchecked((byte)(2 * (state.Angle.TableIndex - RotationAngle.QuarterTurn.TableIndex)))),
                unchecked((ushort)(state.MaximumXRadius - 16))))));

        // The phase advances by the signed delta and wraps around the turn.
        state.Angle = state.Angle.AddTableUnits(state.AngleDelta);
    }

    /// <summary>Ports <c>SporeSpawn_Func_3</c> at $A5:EB9B.</summary>
    private static void SetupSporeSpawnDeathVelocity(RoomEnemySlot body, SporeSpawnEnemyState state)
    {
        ByteAngle direction = EnemyMath.CalculateBearing(
            new(unchecked((short)(SporeSpawnDeathCenterX - body.Position.X.Whole)), unchecked((short)(SporeSpawnDeathCenterY - body.Position.Y.Whole)))).ToTableAngle();
        state.DeathAngle = RotationAngle.FromTableIndex(direction.RawValue);
        state.DeathVelocityMagnitude = new(
            SignedSixteenSixteen.FromRaw(EnemyMath.ReadUnsignedSineMagnitudeProduct(direction, 1, 64)),
            SignedSixteenSixteen.FromRaw(EnemyMath.ReadUnsignedSineMagnitudeProduct(direction, 1, 128)));
    }

    /// <summary>Ports <c>SporeSpawn_Func_4</c> at $A5:EBEE.</summary>
    private void RunSporeSpawnDying(
        RoomEnemySlot body,
        SporeSpawnEnemyState state)
    {
        int xDisplacement = EnemyMath.CosineIsNegative(state.DeathAngle.ToByteAngle())
            ? -state.DeathVelocityMagnitude.X.RawValue
            : state.DeathVelocityMagnitude.X.RawValue;
        int yDisplacement = EnemyMath.SineIsNegative(state.DeathAngle.ToByteAngle())
            ? -state.DeathVelocityMagnitude.Y.RawValue
            : state.DeathVelocityMagnitude.Y.RawValue;
        body.Position = body.Position.AdvanceX(SignedSixteenSixteen.FromRaw(xDisplacement));
        body.Position = body.Position.AdvanceY(SignedSixteenSixteen.FromRaw(yDisplacement));

        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(body.Position.X.Whole - SporeSpawnDeathCenterX))) < 8 &&
            EnemyMath.WrappedMagnitude(unchecked((ushort)(body.Position.Y.Whole - SporeSpawnDeathCenterY))) < 8)
        {
            state.Function = SporeSpawnFunction.Idle;
        }

        UpdateSporeSpawnStalks(state);
        // $A5:E9F7 tests the 16-bit NMI_FrameCounter, not the 8-bit $05B5 copy.
        if (_frameContext.FrameCounter % 16 == 0)
        {
            int random = _room.Random.Generate();
            _ = _spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(random % 64 + 96)), unchecked((ushort)(SnesLittleEndian.HighByte(random) % 16 + 480))),
                RoomSpriteObjectKind.DustCloud,
                graphicsIndex: 0);
        }
    }

    /// <summary>Ports the raw slot interpolation in <c>SporeSpawn_Func_5</c> at $A5:EC49.</summary>
    private void UpdateSporeSpawnStalks(SporeSpawnEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnDeathCenterX,
            anchor: state.StalkAnchor.X,
            end: body.Position.X.Whole,
            axis: Axis.Horizontal);
        WriteSporeSpawnStalkAxis(
            firstPosition: SporeSpawnCeilingY,
            anchor: state.StalkAnchor.Y,
            end: unchecked((ushort)(body.Position.Y.Whole - 40)),
            axis: Axis.Vertical);
    }

    /// <summary>Interpolates one axis of four physical stalk slots from the ceiling toward the body.</summary>
    private void WriteSporeSpawnStalkAxis(
        int firstPosition,
        int anchor,
        int end,
        Axis axis)
    {
        // The signed distance from the anchor to the body is at most 32768 either way, so its
        // quarters never wrap; only the interpolated coordinates keep sixteen bits.
        int delta = unchecked((short)(end - anchor));
        bool negative = delta < 0;
        int magnitude = Math.Abs(delta);
        int quarter = magnitude >> 2;
        int half = magnitude >> 1;
        int threeQuarters = quarter + half;
        Span<ushort> positions = stackalloc ushort[4]
        {
            checked((ushort)firstPosition),
            unchecked((ushort)(anchor + (negative ? -quarter : quarter))),
            unchecked((ushort)(anchor + (negative ? -half : half))),
            unchecked((ushort)(anchor + (negative ? -threeQuarters : threeQuarters))),
        };

        // The boss writes physical projectile slots 14,15,16,17 in this order even though
        // its four initializer calls allocated them in reverse order.
        for (int segment = 0; segment < positions.Length; segment++)
        {
            RoomEnemyProjectileSlot stalk = _projectiles.EnemyProjectiles[14 + segment];
            if (axis == Axis.Horizontal)
                stalk.Position = stalk.Position.WithWholeX(positions[segment]);
            else
                stalk.Position = stalk.Position.WithWholeY(positions[segment]);
        }
    }

    /// <summary>Requests the arena ceiling PLM at its fixed room block position.</summary>
    private void PublishSporeSpawnPlm(PlmHeaderId header) =>
        _frameResults.SporeSpawnCeiling = new PlmSpawnRequest(new(7, 30), header);

    /// <summary>Routes accepted bomb hits through the boss-specific post-damage response.</summary>
    internal void AddSporeSpawnBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.SporeSpawnShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Binds spore movement, emitter timing, impact properties, and drop opcodes.</summary>
    internal void AddSporeSpawnProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunSporeSpawnSporePreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.Spores);

        table.AddPreInstruction(context =>
        {
            RunSporeSpawnSpawnerPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.SporeSpawner);

        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnSpore,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.SporeShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnSpawner,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.SpawnerClosed));
        table.AddShotAnimation(RoomEnemyProjectileKind.SporeSpawnStalk,
            () => new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.Stalk));
        table.AddCue<SporeSpawnProjectileCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case SporeSpawnProjectileCue.SpawnSpore:
                    SpawnSporeSpawnSpore(projectile.Position.Whole);
                    return;
                case SporeSpawnProjectileCue.SetImpactProperties:
                    SetSporeSpawnImpactProperties(projectile);
                    return;
                case SporeSpawnProjectileCue.QueueKilledSound:
                    _deathsAndDrops.QueueEnemyKilledSound();
                    return;
                case SporeSpawnProjectileCue.RequestDrop:
                    RequestSporeSpawnSporeDrop(projectile);
                    return;
                case SporeSpawnProjectileCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SporeSpawnProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>Ports SporeSpawn's main AI.</summary>
    internal void RunSporeSpawnMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunSporeSpawnMain(slot, RequireSporeSpawnState(slot));
    }

    /// <summary>Runs Spore Spawn's body program where the enemy instruction interpreter would.</summary>
    internal void AdvanceSporeSpawnAnimation(RoomEnemySlot body)
    {
        if (!EnemyProgramAnimation.Expire(body))
            return;
        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        AnimationProgramCursor<SporeSpawnBodyProgram> cursor = state.BodyProgram;
        // The open-pulse and death-burst repeats count the body's timer word, as the native timer commands do.
        EnemyProgramAnimation.Run(body, SporeSpawnAnimations.Programs, ref cursor, SporeSpawnFrameDefinitions.ShownFrames.Of,
            (cue, continuation) =>
            {
                ApplySporeSpawnCue(body, state, cue);
                return null;
            });
        state.BodyProgram = cursor;
    }

    /// <summary>Restricts core-hitbox damage to accepted projectile families and handles boss death.</summary>
    internal void AddSporeSpawnShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SporeSpawnShot)
        {
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Spore Spawn requires its authored extended-spritemap hitboxes.");
                if (c.HitboxShotAi == EnemyInteractionCallback.DudShot)
                {
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (c.HitboxShotAi != EnemyInteractionCallback.SporeSpawnShot)
                {
                    throw new InvalidDataException(
                        $"Spore Spawn hitbox shot AI {c.HitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                if (!SporeSpawnAcceptsProjectile(c.ProjectileType))
                {
                    // The extended collision walker has already selected the vulnerable
                    // core. $ED5A rejects this projectile family before common damage.
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    return true;
                }
                return false;
            },
            ExemptFromGenericDeath = true,
            AfterCommon = c => ResolveSporeSpawnShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Routes vulnerable hitbox touch through common damage while excluding no-op hitboxes.</summary>
    internal void AddSporeSpawnTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.SporeSpawn &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.SporeSpawnTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Spore Spawn requires its authored extended-spritemap hitboxes.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.NoOp)
                    return;
                if (context.HitboxTouchAi != EnemyInteractionCallback.SporeSpawnTouch)
                {
                    throw new InvalidDataException(
                        $"Spore Spawn hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveSporeSpawnDeathAfterCommon(context.Slot);
            });
    }
    /// <summary>Checks the projectile family bits accepted by the vulnerable core.</summary>
    private static bool SporeSpawnAcceptsProjectile(SamusProjectileTypeWord projectileType) =>
        (projectileType.Raw & SporeSpawnCollisionDefinitions.AcceptedFamilyBits) != 0 || projectileType.IsChargedBeam;

    /// <summary>Ports the post-common body of <c>SporeSpawn_Shot</c> at $A5:ED5A.</summary>
    private void ResolveSporeSpawnShotAfterCommon(RoomEnemySlot body)
    {
        SporeSpawnEnemyState state = RequireSporeSpawnState(body);
        if (!body.FlashTimer.IsZero)
        {
            state.Function = SporeSpawnFunction.Moving;

            // Below 400 health the native code doubles angular speed while preserving its
            // current sign. At exactly 400 it leaves the existing delta untouched.
            if (body.Health < 400)
            {
                state.AngleDelta = state.AngleDelta < 0 ? -2 : 2;
            }

            // Only the first damaging hit during an open phase reverses direction and
            // closes the head. Opcode $E771 clears this guard when the list permits it.
            if (!state.Damaged)
            {
                state.AngleDelta = -state.AngleDelta;
                state.Damaged = true;
                state.BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.CloseAndMove);
                body.InstructionTimer = WrappingCountdown.Start(1);

                int healthFrame = body.Health >= 770
                    ? 0
                    : body.Health >= 410
                        ? 1
                        : body.Health >= 70
                            ? 2
                            : 3;
                if (body.Health != state.PreviousHealth)
                {
                    state.PreviousHealth = body.Health;
                    LoadSporeSpawnHealthPalette(healthFrame);
                }
            }
        }

        ResolveSporeSpawnDeathAfterCommon(body, state);
    }

    /// <summary>Ports <c>SporeSpawn_Func_6</c> at $A5:EDF3.</summary>
    private void ResolveSporeSpawnDeathAfterCommon(
        RoomEnemySlot body,
        SporeSpawnEnemyState? suppliedState = null)
    {
        if (body.Health != 0)
            return;
        SporeSpawnEnemyState state = suppliedState ?? RequireSporeSpawnState(body);
        if (state.DeathStarted)
            return;

        state.DeathStarted = true;
        state.SporeGenerationSuppressed = false;
        body.InvincibilityTimer = WrappingCountdown.Start(0);
        body.FlashTimer = WrappingCountdown.Start(0);
        body.AiStates = EnemyAiStates.None;
        // Literal property $0400 removes the dead body from the interactive-enemy list.
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

        // The 65C816 loop clears native projectile indexes $1A..$00: physical slots 13..0.
        // Slots 14..17 are the stalk and deliberately survive for the death animation.
        for (int projectileSlot = 0; projectileSlot <= 13; projectileSlot++)
            _projectiles.EnemyProjectiles[projectileSlot].Clear();

        state.BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(SporeSpawnBodyProgram.Death);
        body.InstructionTimer = WrappingCountdown.Start(1);
        _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
        state.ScrollClampHookActive = false;
        PublishSporeSpawnPlm(header: PlmHeaderId.CrumbleSporeSpawnCeiling);
    }

    /// <summary>Carries out one cue of Spore Spawn's body program.</summary>
    internal void ApplySporeSpawnCue(RoomEnemySlot body, SporeSpawnEnemyState state, SporeSpawnCue cue)
    {
        switch (cue)
        {
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.IncreaseMaxXRadius }:
                if (state.MaximumXRadius < SporeSpawnWidenedRadiusLimit)
                    state.MaximumXRadius += SporeSpawnRadiusWidening;
                return;
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.ClearDamagedFlag }:
                state.Damaged = false;
                return;
            case SporeSpawnCue.SetMaxXRadiusAndAngleDelta setMaxXRadiusAndAngleDelta:
                state.MaximumXRadius = setMaxXRadiusAndAngleDelta.MaximumXRadius;
                state.AngleDelta = setMaxXRadiusAndAngleDelta.AngleDelta;
                return;
            case SporeSpawnCue.SetSporeGeneration setSporeGeneration:
                state.SporeGenerationSuppressed = setSporeGeneration.SporeGenerationSuppressed;
                return;
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.Harden }:
                body.Position = body.Position.WithWhole(new(SporeSpawnDeathCenterX, SporeSpawnDeathCenterY));
                body.Properties = body.Properties
                    .With(EnemyProperties.SolidToSamus | EnemyProperties.ProcessInstructions)
                    .Without(EnemyProperties.IgnoreSamusCollision);
                return;
            case SporeSpawnCue.QueueSound queueSound:
                LastSporeSpawnSoundEffect = queueSound.Sound ?? throw new InvalidDataException("A Spore Spawn sound cue carries no sound.");
                return;
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.RequestDeathDrops }:
                RequestSporeSpawnDeathDrops(state);
                return;
            case SporeSpawnCue.SetFunction setFunction:
                state.Function = setFunction.Function;
                return;
            case SporeSpawnCue.LoadDeathPalette loadDeathPalette:
                LoadSporeSpawnDeathPalette(loadDeathPalette.DeathPaletteStep, copy: SporeSpawnPaletteCopy.CurrentAndTarget);
                return;
            case SporeSpawnCue.LoadDeathTargetPalette loadDeathTargetPalette:
                LoadSporeSpawnDeathPalette(loadDeathTargetPalette.DeathPaletteStep, copy: SporeSpawnPaletteCopy.TargetOnly);
                return;
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.SpawnHardeningDust }:
                SpawnSporeSpawnHardeningDust(state);
                return;
            case SporeSpawnCue.Command { Kind: SporeSpawnCueKind.SpawnDyingExplosion }:
                SpawnSporeSpawnDyingExplosion(state);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SporeSpawnCueKind)} {cue}.");
        }
    }

    /// <summary>Ports <c>SporeSpawn_Func_7</c> at $A5:EE4A: shows health-stage body image <paramref name="frame"/>.</summary>
    internal void LoadSporeSpawnHealthPalette(int frame)
    {
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            _presentation.BossColours().SporeSpawnBody.Write(
                color,
                (_bossArtwork.Artwork?.SporeSpawnColors ?? throw new InvalidDataException(
                    "Spore Spawn health palette requires installed artwork.")).ResolveHealth(frame, color));
        }
    }

    /// <summary>Loads sprite, level, and background death colors into CGRAM or fade targets.</summary>
    internal void LoadSporeSpawnDeathPalette(int step, SporeSpawnPaletteCopy copy)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "Spore Spawn palette instruction ran without the boss state.");
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Sprite,
            step,
            state,
            copy == SporeSpawnPaletteCopy.TargetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Level,
            step,
            state,
            copy == SporeSpawnPaletteCopy.TargetOnly);
        CopySporeSpawnPaletteRow(
            SporeSpawnDeathPaletteLayer.Background,
            step,
            state,
            copy == SporeSpawnPaletteCopy.TargetOnly);
    }

    /// <summary>Writes one installed death-color row to its native destination or target buffer.</summary>
    private void CopySporeSpawnPaletteRow(
        SporeSpawnDeathPaletteLayer layer,
        int frame,
        SporeSpawnEnemyState state,
        bool targetOnly)
    {
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            Bgr555 value = (_bossArtwork.Artwork?.SporeSpawnColors ?? throw new InvalidDataException(
                "Spore Spawn death palette requires installed artwork.")).ResolveDeath(layer, frame, color);
            if (targetOnly)
                state.WriteTargetColor(DeathColours(layer), color, value);
            else
                _presentation.BossColours().SporeSpawnDeath(layer).Write(color, value);
        }
    }

    /// <summary>The colours Spore Spawn's death fades on <paramref name="layer"/>.</summary>
    private static IColourSetRun DeathColours(SporeSpawnDeathPaletteLayer layer) => layer switch
    {
        SporeSpawnDeathPaletteLayer.Sprite => BossColourDefinitions.SporeSpawnBody,
        SporeSpawnDeathPaletteLayer.Level => BossColourDefinitions.SporeSpawnDeathLevel,
        SporeSpawnDeathPaletteLayer.Background => BossColourDefinitions.SporeSpawnDeathBackground,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined Spore Spawn death palette layer."),
    };

    /// <summary>Uses one RNG word to place a hardening dust burst and request its sound.</summary>
    private void SpawnSporeSpawnHardeningDust(SporeSpawnEnemyState state)
    {
        int random = _room.Random.Generate();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(state.Body.Position.X.Whole + random % 128 - 64)), unchecked((ushort)(state.Body.Position.Y.Whole + SnesLittleEndian.HighByte(random) % 128 - 64))),
            MiscDustAnimation.BigDustCloud);
        LastSporeSpawnSoundEffect = SporeSpawnSoundDefinitions.HardeningDust;
    }

    /// <summary>Uses one RNG word to place a dying explosion sprite and request its sound.</summary>
    private void SpawnSporeSpawnDyingExplosion(SporeSpawnEnemyState state)
    {
        int random = _room.Random.Generate();
        _ = _spriteObjects.SpawnRoomSpriteObject(
            new(unchecked((ushort)(state.Body.Position.X.Whole + random % 128 - 64)), unchecked((ushort)(state.Body.Position.Y.Whole + SnesLittleEndian.HighByte(random) % 64 - 32))),
            RoomSpriteObjectKind.SporeSpawnDyingExplosion,
            graphicsIndex: 0);
        LastSporeSpawnSoundEffect = SporeSpawnSoundDefinitions.DyingExplosion;
    }

    /// <summary>Generates sixteen RNG-positioned boss drops and marks the death drop as requested.</summary>
    private void RequestSporeSpawnDeathDrops(SporeSpawnEnemyState state)
    {
        for (int index = 0; index < 16; index++)
        {
            int random = _room.Random.Generate();
            Position16<RoomPixels> drop = new((ushort)(random % 128 + 64), (ushort)(SnesLittleEndian.HighByte(random) % 64 + 528));
            SporeSpawnDropRequests.Add(new SporeSpawnDropRequest());
            _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(drop, EnemyDefinitionId.SporeSpawn);
        }
        state.DeathDropRequested = true;
    }
    /// <summary>Graphics bits assigned to newly spawned spores.</summary>
    private static readonly ushort SporeSpawnGraphicsIndex = EnemyPaletteBits.Palette1;

    /// <summary>Ports projectile definition $86:DE6C and initializer $86:DCA3.</summary>
    internal void SpawnSporeSpawnStalk(RoomEnemySlot body, int spawnArgument)
    {
        if (spawnArgument is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        RoomEnemyProjectileSlot? stalk = _projectiles.AllocateEnemyProjectile();
        if (stalk is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            stalk,
            RoomEnemyProjectileKind.SporeSpawnStalk,
            unchecked((ushort)(body.TileIndexBase | body.PaletteIndex)));
        stalk.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.Stalk));
        stalk.Position = stalk.Position.WithWholeX(body.Position.X.Whole);
        stalk.Position = stalk.Position.WithWholeY(unchecked((ushort)(
            body.Position.Y.Whole +
            SporeSpawnProjectileDefinitions.StalkYOffset(spawnArgument))));
    }

    /// <summary>Ports projectile definition $86:DE88 and initializer $86:DCD4.</summary>
    internal void SpawnSporeSpawnSpawner(RoomEnemySlot body, int spawnArgument)
    {
        if (spawnArgument is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(spawnArgument));
        RoomEnemyProjectileSlot? spawner = _projectiles.AllocateEnemyProjectile();
        if (spawner is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spawner,
            RoomEnemyProjectileKind.SporeSpawnSpawner,
            unchecked((ushort)(body.TileIndexBase | body.PaletteIndex)));
        spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.SpawnerClosed));
        spawner.Position = spawner.Position.WithWholeX(SporeSpawnProjectileDefinitions.SpawnerX(spawnArgument));
        spawner.Position = spawner.Position.WithWholeY(520);
    }

    /// <summary>Ports projectile definition $86:DE7A and initializer $86:DC8D.</summary>
    internal void SpawnSporeSpawnSpore(Position16<RoomPixels> position)
    {
        RoomEnemyProjectileSlot? spore = _projectiles.AllocateEnemyProjectile();
        if (spore is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spore,
            RoomEnemyProjectileKind.SporeSpawnSpore,
            SporeSpawnGraphicsIndex);
        spore.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.Spore));
        spore.Position = spore.Position.WithWhole(position);

        // Var1 retains the original X coordinate. Bit seven alone mirrors horizontal motion,
        // so emitters at $20/$A0 move one way and $60/$E0 move the other.
        spore.Variable1 = position.X;
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_Spores</c> at $86:DCEE.</summary>
    internal static void RunSporeSpawnSporePreInstruction(RoomEnemyProjectileSlot spore)
    {
        int movementOffset = unchecked((byte)spore.Variable0);
        Offset16<RoomPixels> movement =
            SporeSpawnProjectileDefinitions.MovementAt(movementOffset);
        int xDelta = movement.X;
        if (SnesLittleEndian.LowByte(spore.Variable1) >= 128)
            xDelta = -xDelta;
        spore.Position = spore.Position.WithWholeX(unchecked((ushort)(spore.Position.X.Whole + xDelta)));

        int yDelta = movement.Y;
        spore.Position = spore.Position.WithWholeY(unchecked((ushort)(spore.Position.Y.Whole + yDelta + yDelta)));
        if (unchecked((short)(spore.Position.Y.Whole - 768)) >= 0)
            spore.Clear();

        // The native word keeps only the wrapped low byte after each two-byte vector pair.
        spore.Variable0 = unchecked((byte)(movementOffset + 2));
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_SporeSpawner</c> at $86:DD46.</summary>
    internal void RunSporeSpawnSpawnerPreInstruction(RoomEnemyProjectileSlot spawner)
    {
        SporeSpawnEnemyState state = SporeSpawn ?? throw new InvalidOperationException(
            "A Spore Spawn ceiling emitter survived without its owning boss state.");
        if (state.SporeGenerationSuppressed)
            return;

        if (spawner.Variable1 == 0)
        {
            spawner.StartAnimation(new SporeSpawnProjectileAnimation(SporeSpawnProjectileProgram.SpawnerRelease));
            spawner.Variable1 = unchecked((ushort)(_room.Random.Generate() % 512));
        }
        spawner.Variable1 = WrappingCounter.Decrement(checked((ushort)spawner.Variable1)).Value;
    }

    /// <summary>Ports projectile instruction $86:DC5A's complete property-word replace.</summary>
    internal static void SetSporeSpawnImpactProperties(RoomEnemyProjectileSlot spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn property opcode $DC5A.");
        }

        // Literal $3000: damage zero, high priority, Samus collision enabled, projectile
        // collision disabled, and deletion on contact. Priority is not split by this host's
        // draw queue, but every collision bit is retained explicitly.
        spore.Damage = 0;
        spore.CanDamageSamus = true;
        spore.PersistsOnSamusContact = false;
        spore.BlocksSamusProjectiles = false;
    }

    /// <summary>Ports projectile instruction $86:DC61.</summary>
    internal void RequestSporeSpawnSporeDrop(RoomEnemyProjectileSlot spore)
    {
        if (spore.Kind != RoomEnemyProjectileKind.SporeSpawnSpore)
        {
            throw new InvalidOperationException(
                $"Projectile {spore.Kind} reached Spore Spawn drop opcode $DC61.");
        }
        SporeSpawnDropRequests.Add(new SporeSpawnDropRequest());
        _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
            spore.Position.Whole,
            EnemyDefinitionId.SporeSpawnStalk);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastSporeSpawnSoundEffect = null;
        SporeSpawnDropRequests.Clear();
    }
    /// <summary>Single active boss state, absent outside a room containing Spore Spawn.</summary>
    internal SporeSpawnEnemyState? SporeSpawn { get; set; }

    /// <summary>Publishes Spore Spawn's pending target palette writes, if the boss is present.</summary>
    internal void ConsumeTargetColors(Action<SporeSpawnStagedColour> write) => SporeSpawn?.ConsumeTargetColors(write);

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastSporeSpawnSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindSporeSpawnHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(enemy.DefinitionId == EnemyDefinitionId.SporeSpawn &&
            SporeSpawnCollisionDefinitions.IsFrame(enemy.Frame)))
            return null;

        // These twelve OAM frames are selected by Spore Spawn. Old visual
        // overrides may carry Draygon-prefixed keys, but every rectangle
        // and callback comes from the compiled gameplay catalog.
        foreach (SporeSpawnCollisionComponent component in
                 SporeSpawnCollisionDefinitions.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (SporeSpawnCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        unchecked((ushort)(componentOrigin.X + hitbox.Left)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Top)),
                        unchecked((ushort)(componentOrigin.X + hitbox.Right)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Bottom))))
                    continue;
                callback = probe.SelectShotCallback
                    ? SporeSpawnHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : SporeSpawnHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }
}
