using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's flames as projectiles, its hitboxes and body touch, and what Samus's weapons do to it.</summary>
[RoomLifetime]
internal sealed class PhantoonCombat
{
    /// <summary>Room services and tilemap state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during Phantoon's death sequence.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the body, eye, mouth, and tentacle slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Resolves Phantoon and flame contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Flame-destruction drop requests published during the current room encounter.</summary>
    internal readonly List<PhantoonFlameDropRequest> PhantoonFlameDropRequests = new();

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonCombat(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Binds bomb hit responses for vulnerable Phantoon phases.</summary>
    internal void AddPhantoonBombReactions(EnemySharedReactions table)
    {
        // Common bomb damage. Rinka, Skree, and Powamp deliberately call the no-death-animation form of common shot
        // AI; their private callbacks own slot cleanup or a staged death. Bank-$A8 callbacks reuse the same
        // translated post-common routines as beams. Normal bombs cannot freeze, but their damage, staged-death,
        // child synchronization, recoil, release, and attack side effects are otherwise the same callback tails.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.PhantoonBody &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.PhantoonShot)
        {
            RunsGenericDeath = false,
            // Phantoon's `$A7:DD9B` hitbox callback enters the same no-death common-damage routine for bombs as it
            // does for beams and missiles, then consumes the exact damage delta in its encounter reaction state
            // machine. A lethal hit begins the authored death sequence, while sub-threshold damage feeds the
            // eye-close/rage accumulator instead of deleting a slot.
            AfterCommon = c =>
            {
                PhantoonEnemyState phantoon = Phantoon ??
                    throw new InvalidOperationException("Phantoon shot callback has no encounter state.");
                int appliedDamage = c.HealthBefore - c.Enemy.Health;
                phantoon.AcceptedProjectileHits++;
                ResolvePhantoonShotReaction(c.Enemy, phantoon, c.Bomb.PackedType, appliedDamage);
            },
        });
    }

    /// <summary>Registers Phantoon flame initializers, pre-instructions, and instructions.</summary>
    internal void AddPhantoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunPhantoonStartingFlameWaiting(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonStartingFlames);

        table.AddPreInstruction(context =>
        {
            RunPhantoonStartingFlameOrbit(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonStartingFlames_Activated);

        table.AddPreInstruction(context =>
        {
            RunPhantoonCasualFlameFalling(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Falling);

        table.AddPreInstruction(context =>
        {
            RunPhantoonCasualFlameImpactPause(context.Projectile, _projectiles.CurrentFrame16);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround);

        table.AddPreInstruction(context =>
        {
            RunPhantoonCasualFlameBouncing(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing);

        table.AddPreInstruction(context =>
        {
            RunPhantoonEnragedFlame(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Enraged);

        table.AddPreInstruction(context =>
        {
            RunPhantoonRainFlame(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Rain);

        table.AddPreInstruction(context =>
        {
            RunPhantoonSpiralFlame(context.Projectile);
        }, EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Spiral);

        table.AddShotAnimation(RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.PhantoonStartingFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameProgram.DestroyableIdle));
        table.AddCue<PhantoonFlameCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case PhantoonFlameCue.SpawnDrop:
                    RequestPhantoonFlameDrop(projectile);
                    return;
                case PhantoonFlameCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameCue)} {cue}.");
            }
        });
    }

    /// <summary>
    /// Runs bank-$A0's five-slot beam/missile walk for Phantoon, then dispatches the
    /// selected bank-$A7 hitbox callback. One projectile resolves per pass, matching the
    /// native early exit after the first overlapping enemy/component pair.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);
        _room.EnsureLoaded();

        if (Phantoon is not { } state)
            return 0;

        RoomEnemySlot body = state.Body;
        if (body.DefinitionId != EnemyDefinitionId.PhantoonBody ||
            CommonEnemyEmptyExtendedFrameDefinitions.DrawsNothing(body.Frame) ||
            !body.InvincibilityTimer.IsZero ||
            body.Properties.HasAny(
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return 0;
        }

        foreach (SamusProjectileSlot projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload)
                continue;

            SamusProjectileTypeWord projectileType = projectile.PackedType;
            int projectileDamage = projectile.Damage;
            SamusProjectileFamily family = projectile.PackedType.Family;
            // A0:9BE6..9BF6 excludes bombs and every family at/above beam
            // explosions. Lingering missile explosions must not reach shot AI.
            if (family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or
                >= SamusProjectileFamily.BeamExplosion)
                continue;

            if (!body.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap))
            {
                throw new InvalidDataException(
                    "Phantoon lost the extended-spritemap property required by his hitboxes.");
            }

            if (!_collision.TryFindExtendedHitboxCallback(
                    body,
                    projectile.Position.Whole,
                    projectile.Radius,
                    callbackKind: HitboxCallbackKind.Shot,
                    out EnemyInteractionCallback? hitboxShotAi))
            {
                continue;
            }

            // `$A0:9B7F` applies this before calling the selected component function. A
            // no-op shell hit therefore still marks the beam and shakes for a Super Missile.
            if (family == SamusProjectileFamily.SuperMissile)
            {
                _earthquake.EarthquakeTimer = WrappingCountdown.Start(SuperMissileDefinitions.EarthquakeDuration);
                _earthquake.EarthquakeType = SuperMissileDefinitions.EnemyHitEarthquakeType;
            }

            if (hitboxShotAi == EnemyInteractionCallback.NoOp ||
                Part(body).Function >= PhantoonAiFunction.DyingFadeInOut)
            {
                projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(
                    projectile.SlotIndex,
                    body.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                        !projectile.PackedType.HasAnyBeam(SamusBeamFlags.Plasma));
                return 1;
            }

            if (hitboxShotAi != EnemyInteractionCallback.PhantoonShot)
            {
                throw new InvalidDataException(
                    $"Phantoon hitbox shot AI {hitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
            }

            // The native extended-hitbox walker marks the projectile before $A7:DD9B.
            // That callback's damage helper does not convert it into an explosion:
            // the projectile pre-instruction owns the following-frame deletion, while
            // penetrating Plasma retains its type, direction, and instruction stream.
            projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(projectile.SlotIndex,
                body.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !projectileType.HasAnyBeam(SamusBeamFlags.Plasma));

            int healthBefore = body.Health;
            // Phantoon calls common normal-shot damage, including its charged-beam
            // override. SBA particles retain that bit; reading only the uncharged
            // combination row made Wave Shield disappear without damaging the boss.
            EnemyCollision.NormalShotVulnerability vulnerability =
                EnemyCollision.ReadNormalShotVulnerability(body, projectileType);
            if (vulnerability.FreezeImmediately)
            {
                body.FrozenTimer = WrappingCountdown.Start(400);
                body.AiStates |= EnemyAiStates.Frozen;
                body.InvincibilityTimer = WrappingCountdown.Start(10);
            }
            else
            {
                int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
                if (damage != 0)
                {
                    body.FlashTimer = WrappingCountdown.Start(body.HurtFlashFrames + 8);
                    body.AiStates |= EnemyAiStates.Hurt;
                    // Plasma alone installs this clock; ordinary missiles retain
                    // the timer behavior of the shared native damage helper.
                    if (projectileType.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = WrappingCountdown.Start(EnemyShotTiming.PlasmaInvincibilityFrames);
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                }
            }

            int appliedDamage = healthBefore - body.Health;
            state.AcceptedProjectileHits++;
            ResolvePhantoonShotReaction(
                body,
                state,
                projectileType,
                appliedDamage);
            return 1;
        }

        return 0;
    }

    /// <summary>Ports the boss-owned tail of $A7:DD9B after common vulnerability damage.</summary>
    internal void ResolvePhantoonShotReaction(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusProjectileTypeWord projectileType,
        int damage)
    {
        if (body.Health == 0)
        {
            state.LastCombatSoundEffect = PhantoonSoundDefinitions.ShotCry;
            state.Parts[state.Tentacles.SlotIndex].HitReaction = 1;
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            BeginPhantoonDeathSequence(body, state);
            return;
        }

        if ((body.AiStates & EnemyAiStates.Hurt) == EnemyAiStates.None)
            return;

        state.LastCombatSoundEffect = PhantoonSoundDefinitions.ShotCry;
        SamusProjectileFamily family = projectileType.Family;
        PhantoonAiFunction function = state.Parts[body.SlotIndex].Function;
        bool vulnerableWindow = function is
            PhantoonAiFunction.EyeTracksSamus or
            PhantoonAiFunction.TrackSamusDuringFlameRain;
        bool swooping = function == PhantoonAiFunction.Swooping;

        if (vulnerableWindow || swooping)
        {
            // A single Super Missile (600 retail damage) skips ordinary round accumulation
            // and enters the eight-wave rage sequence immediately.
            if (damage >= PhantoonBehavior.PhantoonDamageThreshold &&
                family == SamusProjectileFamily.SuperMissile)
            {
                ClosePhantoonAfterDamage(body, state, PhantoonAiFunction.FadeOutBeforeRage);
                return;
            }

            state.Parts[state.Tentacles.SlotIndex].RoundDamage = unchecked((ushort)(
                state.Parts[state.Tentacles.SlotIndex].RoundDamage + damage));
            if (state.Parts[state.Tentacles.SlotIndex].RoundDamage >= PhantoonBehavior.PhantoonDamageThreshold)
            {
                if (swooping)
                    state.Parts[body.SlotIndex].FunctionTimer = 1;
                else
                    ClosePhantoonAfterDamage(body, state, PhantoonAiFunction.FadeOutWhileSwooping);
                state.Parts[state.Tentacles.SlotIndex].HitReaction = 2;
                return;
            }

            if (vulnerableWindow)
            {
                int random = _room.Random.Generate() & 7;
                state.Parts[state.Eye.SlotIndex].ShotMarker = PhantoonPatternDefinitions.ShotEyeMarkers[random];
                state.Parts[state.Mouth.SlotIndex].ShotPattern = random;
                state.Parts[state.Tentacles.SlotIndex].HitReaction = 1;
                if (!state.Parts[state.Tentacles.SlotIndex].SwoopRequested)
                {
                    state.Parts[state.Tentacles.SlotIndex].SwoopRequested = true;
                    if (state.Parts[body.SlotIndex].FunctionTimer >= 16)
                        state.Parts[body.SlotIndex].FunctionTimer = 16;
                }
                return;
            }
        }

        // Native writes this otherwise-unused marker even when a valid damaging shot lands
        // outside the three reaction states. Retain it because the four records are exposed
        // to the debugger and later reverse-engineering may identify another consumer.
        state.Parts[state.Tentacles.SlotIndex].HitReaction = 2;
    }

    /// <summary>Closes the eye and ends the vulnerable interval after a damaging hit.</summary>
    private static void ClosePhantoonAfterDamage(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        PhantoonAiFunction fadeOut)
    {
        state.Parts[body.SlotIndex].Function = fadeOut;
        state.Parts[body.SlotIndex].FunctionTimer = 0;
        RoomEnemySlot tentacles = state.Tentacles;
        RoomEnemySlot eye = state.Eye;
        state.Parts[tentacles.SlotIndex].SwoopRequested = false;
        state.Parts[tentacles.SlotIndex].RoundDamage = 0;
        state.SemiTransparent = true;
        PhantoonPresentation.InstallPhantoonProgram(state, body, PhantoonPartProgram.InvulnerableBody);
        PhantoonPresentation.InstallPhantoonProgram(state, eye, PhantoonPartProgram.EyeClose);
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Parts[eye.SlotIndex].FadeComplete = false;
        state.Parts[tentacles.SlotIndex].HitReaction = 2;
    }

    /// <summary>Switches from combat to the fatal swoop and death phases.</summary>
    private static void BeginPhantoonDeathSequence(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.Parts[body.SlotIndex].Function = state.Parts[body.SlotIndex].Function is
            PhantoonAiFunction.Swooping or
            PhantoonAiFunction.FadeOutWhileSwooping
                ? PhantoonAiFunction.FinishFatalSwoop
                : PhantoonAiFunction.DyingFadeInOut;
        state.Parts[state.Eye.SlotIndex].FigureEightReversed = false;
        state.Parts[state.Eye.SlotIndex].FadeComplete = false;
        state.Wave.InitializeDeathScroll(state.Bg2Scroll.X);
        state.SemiTransparent = true;
        state.Parts[state.Mouth.SlotIndex].ShotPattern = 1;
    }

    /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
    internal void RunPhantoonStartingFlameWaiting(RoomEnemyProjectileSlot flame)
    {
        PhantoonEnemyState? state = Phantoon;
        if (state is null || !state.Parts[state.Body.SlotIndex].StartingFlamesActive)
            return;
        flame.PreInstruction = PhantoonBehavior.PhantoonStartingFlameOrbitPreInstruction;
        flame.Velocity = new(new SnesSignedEightEight(180), new SnesSignedEightEight(48));
    }

    /// <summary>Advances an intro flame around Phantoon's body.</summary>
    internal void RunPhantoonStartingFlameOrbit(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
        }
        // $86:9B4C reads the 16-bit NMI_FrameCounter.
        else if ((_projectiles.CurrentFrame16 & 1) != 0)
        {
            flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue - 1))));
            if (flame.Velocity.Y.RawValue == 0)
            {
                flame.Position = flame.Position.WithWhole(new(state.Body.Position.X.Whole, unchecked((ushort)(state.Body.Position.Y.Whole + 16))));
                flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.Delete));
                flame.InstructionTimer = WrappingCountdown.Start(1);
                return;
            }
        }

        flame.Variable0 = unchecked((byte)(flame.Variable0 + 1));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked(new SnesByteAngle(unchecked((byte)((ushort)flame.Variable0)))),
            unchecked((byte)flame.Velocity.Y.RawValue));
    }

    /// <summary>Moves a casual flame downward before floor contact.</summary>
    internal static void RunPhantoonCasualFlameFalling(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.PreInstruction =
            EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_HitGround;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualHitGround));
        flame.InstructionTimer = WrappingCountdown.Start(1);
        flame.Variable0 = 8;
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
    }

    /// <summary>Holds the flame briefly at the floor after impact.</summary>
    internal static void RunPhantoonCasualFlameImpactPause(
        RoomEnemyProjectileSlot flame,
        int frameCounter)
    {
        CountdownStep pauseStep = WrappingCounter.Decrement(checked((ushort)flame.Variable0));
        flame.Variable0 = pauseStep.Value;
        if (!pauseStep.IsZeroOrNegative)
            return;

        flame.PreInstruction =
            EnemyProjectilePreInstruction.PhantoonDestroyableFlame_Casual_Bouncing;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualBouncing));
        flame.InstructionTimer = WrappingCountdown.Start(1);
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole - 8)));
        flame.Velocity = flame.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-768));
        flame.Variable0 = 0;
        // $86:99EB reads the 16-bit NMI_FrameCounter.
        flame.Velocity = flame.Velocity.WithX(SnesSignedEightEight.FromSignedRaw((frameCounter & 1) == 0 ? (short)128 : (short)-128));
    }

    /// <summary>Advances the flame's post-impact bounce motion.</summary>
    internal static void RunPhantoonCasualFlameBouncing(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
        {
            flame.Variable0 = unchecked((ushort)(flame.Variable0 + 1));
            if (flame.Variable0 < 3)
            {
                flame.Velocity = flame.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(flame.Variable0 == 1 ? (short)-512 : (short)-256));
                return;
            }
            RestPhantoonCasualFlame(flame);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Horizontal))
            RestPhantoonCasualFlame(flame);
    }

    /// <summary>Installs the resting phase after a flame's bounce ends.</summary>
    private static void RestPhantoonCasualFlame(RoomEnemyProjectileSlot flame)
    {
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.CasualResting));
        flame.InstructionTimer = WrappingCountdown.Start(1);
        flame.PreInstruction = EnemyProjectilePreInstruction.RTS_869A44;
    }

    /// <summary>Advances a rage-phase flame through its authored movement.</summary>
    internal void RunPhantoonEnragedFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionDefinitions.RageRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + (byte)flame.Velocity.X.RawValue));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked(new SnesByteAngle(unchecked((byte)((ushort)flame.Variable0)))),
            unchecked((byte)flame.Velocity.Y.RawValue));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Updates a delayed rain flame as it enters and falls through the room.</summary>
    internal void RunPhantoonRainFlame(
        RoomEnemyProjectileSlot flame,
        RoomLevelData level)
    {
        if (flame.Velocity.X.RawValue != 0)
        {
            flame.Velocity = flame.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.X.RawValue - 1))));
            if (flame.Velocity.X.RawValue != 0 && unchecked((short)flame.Velocity.X.RawValue) >= 0)
                return;
            // Expiry falls through to movement on this call, including signed
            // underflow. Keep each actor's native queue call rather than a last-SFX latch.
            _audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundQueueCapacity);
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + 16))));
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.RainImpact));
        flame.InstructionTimer = WrappingCountdown.Start(1);
        flame.Position = flame.Position.WithWholeY(unchecked((ushort)(flame.Position.Y.Whole + 8)));
        flame.PreInstruction = EnemyProjectilePreInstruction.RTS_869A44;
        _audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundQueueCapacity);
    }

    /// <summary>Moves a spiral flame around Phantoon's body.</summary>
    internal void RunPhantoonSpiralFlame(RoomEnemyProjectileSlot flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Clear();
            return;
        }

        flame.Velocity = flame.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(flame.Velocity.Y.RawValue + PhantoonFlameMotionDefinitions.SpiralRadiusStep))));
        flame.Variable0 = unchecked((byte)(flame.Variable0 + PhantoonFlameMotionDefinitions.SpiralAngleStep));
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            checked(new SnesByteAngle(unchecked((byte)((ushort)flame.Variable0)))),
            unchecked((byte)flame.Velocity.Y.RawValue));
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Converts an orbit phase into a world-space flame position around the body.</summary>
    internal static void PositionPhantoonFlameAroundBody(
        RoomEnemyProjectileSlot flame,
        RoomEnemySlot body,
        SnesByteAngle angle,
        int radius)
    {
        // Unlike the common byte-table enemy sine routine, this callback uses a
        // word sample. Its high byte preserves the exact full radius at cardinal angles.
        int xOffset = ReadPhantoonFlameComponent(angle, radius);
        int yOffset = ReadPhantoonFlameComponent(angle.AddTableUnits(-SnesByteAngle.QuarterTurn.RawValue), radius);
        flame.Position = flame.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + xOffset)), unchecked((ushort)(body.Position.Y.Whole + 16 + yOffset))));
    }

    /// <summary>Ports $86:9BA2/$9BF3: unsigned half-wave multiplication followed by whole-result negation.</summary>
    /// <param name="angle">The flame's orbit angle.</param>
    /// <param name="radius">The orbit radius, of which only the low eight bits are read.</param>
    internal static int ReadPhantoonFlameComponent(SnesByteAngle angle, int radius)
    {
        int byteAngle = angle.RawValue;
        int sample = EnemyTrigonometryTables.SignedSine(new SnesByteAngle((byte)(byteAngle & 127)));
        int magnitude = (SnesEightEight.Multiply(unchecked((byte)sample), unchecked((byte)radius))) + SnesEightEight.WholeOf(sample) * unchecked((byte)radius);
        return byteAngle < 128 ? magnitude : -magnitude;
    }

    /// <summary>Switches a flame to its delete program after leaving the room bounds.</summary>
    private static void DeletePhantoonFlameOutsideRoom(RoomEnemyProjectileSlot flame)
    {
        if (unchecked((short)flame.Position.X.Whole) < 0 || flame.Position.X.Whole >= 256 ||
            unchecked((short)flame.Position.Y.Whole) < 0 || flame.Position.Y.Whole >= 256)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameProgram.Delete));
            flame.InstructionTimer = WrappingCountdown.Start(1);
        }
    }

    /// <summary>
    /// Ports projectile instruction <c>$86:980E</c>. The cartridge forwards the flame's
    /// exact position and enemy header <c>$A0:E4FF</c> to <c>Spawn_Enemy_Drops</c>; the
    /// translated pickup owner is still an outer subsystem, so publish the same semantic
    /// request with the header-derived item table rather than silently discarding the drop.
    /// </summary>
    internal void RequestPhantoonFlameDrop(RoomEnemyProjectileSlot flame)
    {
        if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
        {
            throw new InvalidOperationException(
                $"Projectile {flame.Kind} reached Phantoon drop opcode $980E.");
        }

        RoomEnemyDefinition eye = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.PhantoonEye);
        PhantoonFlameDropRequests.Add(new PhantoonFlameDropRequest());
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            flame.Position.Whole,
            eye.DropChances);
    }

    /// <summary>Active four-slot encounter state when retail Phantoon occupies slot zero.</summary>
    public PhantoonEnemyState? Phantoon { get; internal set; }

    /// <summary>Requires the native slot-zero body before handling another Phantoon part.</summary>
    internal PhantoonEnemyState RequirePhantoonState(RoomEnemySlot slot)
    {
        if (Phantoon is null ||
            _slotTable.Slots[0].DefinitionId != EnemyDefinitionId.PhantoonBody)
        {
            throw new InvalidOperationException(
                $"Enemy {slot.DefinitionId} requires Phantoon's retail body in slot zero.");
        }
        return Phantoon;
    }

    /// <summary>Recognizes the four native enemy definitions that share Phantoon encounter state.</summary>
    internal static bool IsPhantoonPartDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PhantoonBody or EnemyDefinitionId.PhantoonEye or
        EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth;

    /// <summary>Binds Phantoon body contact to the shared enemy collision path.</summary>
    internal void AddPhantoonBodyTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.PhantoonBody &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.PhantoonTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Phantoon requires his authored extended touch hitboxes.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.NoOp)
                    return;
                if (context.HitboxTouchAi != EnemyInteractionCallback.PhantoonTouch)
                {
                    throw new InvalidDataException(
                        $"Phantoon hitbox touch AI {context.HitboxTouchAi?.ToString() ?? "(unnamed)"} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
            });
    }

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindPhantoonHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        if (!(PhantoonCombat.IsPhantoonPartDefinition(enemy.DefinitionId)))
            return null;

        // Phantoon's extended frame roots include hitbox-list pointers,
        // but its editable BG2 tilemap writes must never become collision
        // data. Preserve the native component and rectangle order from
        // the compiled bank-$A7 definitions, including point no-ops.
        foreach (PhantoonCollisionComponent component in
                 PhantoonCollisionDefinitions.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (PhantoonCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        unchecked((ushort)(componentOrigin.X + hitbox.Left)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Top)),
                        unchecked((ushort)(componentOrigin.X + hitbox.Right)),
                        unchecked((ushort)(componentOrigin.Y + hitbox.Bottom))))
                    continue;
                callback = probe.SelectShotCallback
                    ? PhantoonHitboxCallbackCodec.ToCallback(hitbox.Shot)
                    : PhantoonHitboxCallbackCodec.ToCallback(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }

    /// <summary>The record words of the Phantoon part in <paramref name="slot"/>.</summary>
    internal PhantoonPartState Part(RoomEnemySlot slot) => RequirePhantoonState(slot).Parts[slot.SlotIndex];
}
