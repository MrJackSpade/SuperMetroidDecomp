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
    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during Phantoon's death sequence.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves Phantoon and flame contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonCombat(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>The bomb hit response for vulnerable Phantoon phases.</summary>
    internal EnemyBombReaction PhantoonBombReaction => field ??= new()
    {
        // Common bomb damage. Rinka, Skree, and Powamp deliberately call the no-death-animation form of common shot
        // AI; their private reactions own the enemy's removal or a staged death. Gameplay-enemy reactions reuse the same
        // post-common steps as beams. Normal bombs cannot freeze, but their damage, staged-death,
        // child synchronization, recoil, release, and attack side effects are otherwise the same reaction tails.
        RunsGenericDeath = false,
        // Phantoon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        // Phantoon's hitbox reaction enters the same no-death common damage for bombs as it
        // does for beams and missiles, then consumes the exact damage delta in its encounter reaction state
        // machine. A lethal hit begins the authored death sequence, while sub-threshold damage feeds the
        // eye-close/rage counter instead of deleting a slot.
        AfterCommon = c =>
        {
            PhantoonEnemyState phantoon = Phantoon ??
                throw new InvalidOperationException("Phantoon's shot reaction ran without Phantoon's fight state.");
            int appliedDamage = c.HealthBefore - c.Enemy.Health;
            ResolvePhantoonShotReaction(c.Enemy, phantoon, c.Bomb.Type, appliedDamage);
        },
    };

    /// <summary>Adds Phantoon's flame initializers, frame behaviours, and animation steps.</summary>
    internal void AddPhantoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunPhantoonStartingFlameWaiting(context.Projectile);
        }, EnemyProjectileFrameBehavior.PhantoonIntroFlameWaiting);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonStartingFlameOrbit(context.Projectile);
        }, EnemyProjectileFrameBehavior.PhantoonIntroFlameOrbit);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonCasualFlameFalling(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.PhantoonCasualFlameFall);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonCasualFlameImpactPause(context.Projectile, _projectiles.CurrentFrame16);
        }, EnemyProjectileFrameBehavior.PhantoonCasualFlameLanding);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonCasualFlameBouncing(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.PhantoonCasualFlameBounce);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonEnragedFlame(context.Projectile);
        }, EnemyProjectileFrameBehavior.PhantoonRageFlameOrbit);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonRainFlame(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.PhantoonFlameRainFall);

        table.AddFrameBehavior(context =>
        {
            RunPhantoonSpiralFlame(context.Projectile);
        }, EnemyProjectileFrameBehavior.PhantoonSpiralFlameOrbit);

        table.AddShotAnimation(RoomEnemyProjectileKind.PhantoonDestroyableFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableShot));
        table.AddShotAnimation(RoomEnemyProjectileKind.PhantoonStartingFlame,
            () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableIdle));
        table.AddCue<PhantoonFlameCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case PhantoonFlameCue.SpawnDrop:
                    RequestPhantoonFlameDrop(projectile);
                    return;
                case PhantoonFlameCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameCue)} {cue}.");
            }
        });
    }

    /// <summary>
    /// Runs 's five-slot beam/missile walk for Phantoon, then dispatches the
    /// selected hitbox reaction. One projectile resolves per pass, matching the
    /// early exit after the first overlapping enemy/component pair.
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
            CommonEnemyEmptyFrames.ShotsPass(body) ||
            body.InvincibilityTimer != 0 ||
            body.Presence.HasAny(EnemyPresence.Deleted) ||
            body.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return 0;
        }

        foreach (SamusProjectile projectile in projectiles.Slots)
        {
            if (!projectile.HasEnemyCollisionPayload)
                continue;

            SamusProjectileType projectileType = projectile.Type;
            int projectileDamage = projectile.Damage;
            SamusProjectileFamily family = projectile.Type.Family;
            // Phantoon's shot filter excludes bombs and every family at/above beam
            // explosions. Lingering missile explosions must not reach shot AI.
            if (family is SamusProjectileFamily.PowerBomb or SamusProjectileFamily.Bomb or
                SamusProjectileFamily.BeamExplosion or SamusProjectileFamily.MissileExplosion)
                continue;

            if (!EnemyCollision.TryFindFrameHitboxReaction(
                    body,
                    projectile.Position.Whole,
                    projectile.Radius,
                    contactKind: HitboxContactKind.Shot,
                    out EnemyReaction? hitboxShot))
            {
                continue;
            }

            // The extended-hitbox walker applies this before running the selected part's reaction. A
            // no-op shell hit therefore still marks the beam and shakes for a Super Missile.
            if (family == SamusProjectileFamily.SuperMissile)
            {
                _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            }

            if (hitboxShot == EnemyReaction.Ignore ||
                state.BodyState.Phase is PhantoonPhase.DyingFadeInOut or PhantoonPhase.DyingExplosions or
                    PhantoonPhase.BeginFinalWavyDeath or PhantoonPhase.DyingFadeOut or PhantoonPhase.AlmostDead or
                    PhantoonPhase.Dead or PhantoonPhase.NoOperation)
            {
                projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(
                    projectile,
                    body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                        !projectile.Type.HasAnyBeam(SamusBeamFlags.Plasma));
                return 1;
            }

            if (hitboxShot != PhantoonReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Phantoon has no response to shot reaction {hitboxShot?.ToString() ?? "(unnamed)"}.");
            }

            // The extended-hitbox walker marks the projectile before Phantoon's hitbox reaction.
            // That reaction's damage helper does not convert it into an explosion:
            // the projectile's frame behaviour owns the following-frame deletion, while
            // penetrating Plasma retains its type, direction, and animation.
            projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(projectile,
                body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.HasAnyBeam(SamusBeamFlags.Plasma));

            int healthBefore = body.Health;
            // Phantoon calls common normal-shot damage, including its charged-beam
            // override. SBA particles retain that bit; reading only the uncharged
            // combination row made Wave Shield disappear without damaging the boss.
            EnemyCollision.NormalShotVulnerability vulnerability =
                EnemyCollision.ReadNormalShotVulnerability(body, projectileType);
            if (vulnerability.FreezeImmediately)
            {
                body.FrozenTimer = 400;
                body.AiStates |= EnemyAiStates.Frozen;
                body.InvincibilityTimer = 10;
            }
            else
            {
                int damage = vulnerability.DamageFrom(projectileDamage);
                if (damage != 0)
                {
                    body.FlashTimer = body.HurtFlashFrames + 8;
                    body.AiStates |= EnemyAiStates.Hurt;
                    // Plasma alone installs this clock; ordinary missiles retain
                    // the timer behavior of the shared damage helper.
                    if (projectileType.HasAnyBeam(SamusBeamFlags.Plasma))
                        body.InvincibilityTimer = EnemyShotTiming.PlasmaInvincibilityFrames;
                    body.Health = damage >= body.Health
                        ? 0
                        : body.Health - damage;
                }
            }

            int appliedDamage = healthBefore - body.Health;
            ResolvePhantoonShotReaction(
                body,
                state,
                projectileType,
                appliedDamage);
            return 1;
        }

        return 0;
    }

    /// <summary>Runs the boss-owned tail of Phantoon's hitbox reaction after common vulnerability damage.</summary>
    internal void ResolvePhantoonShotReaction(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusProjectileType projectileType,
        int damage)
    {
        if (body.Health == 0)
        {
            _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.ShotCry);
            state.TentaclesState.RearmHurtFlash();
            body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
            BeginPhantoonDeathSequence(state);
            return;
        }

        if ((body.AiStates & EnemyAiStates.Hurt) == EnemyAiStates.None)
            return;

        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.ShotCry);
        SamusProjectileFamily family = projectileType.Family;
        PhantoonPhase phase = state.BodyState.Phase;
        bool vulnerableWindow = phase is
            PhantoonPhase.EyeTracksSamus or
            PhantoonPhase.TrackSamusDuringFlameRain;
        bool swooping = phase == PhantoonPhase.Swooping;

        if (vulnerableWindow || swooping)
        {
            // A single Super Missile (600 retail damage) skips ordinary round accumulation
            // and enters the eight-wave rage sequence immediately.
            if (damage >= PhantoonBehavior.PhantoonDamageThreshold &&
                family == SamusProjectileFamily.SuperMissile)
            {
                ClosePhantoonAfterDamage(body, state, PhantoonPhase.FadeOutBeforeRage);
                return;
            }

            // The round's damage wraps when a sum passes its counter's range, as the game's does.
            state.TentaclesState.RoundDamage = WrappingCounter.AddCount(
                state.TentaclesState.RoundDamage, damage);
            if (state.TentaclesState.RoundDamage >= PhantoonBehavior.PhantoonDamageThreshold)
            {
                if (swooping)
                    state.BodyState.PhaseTimer = 1;
                else
                    ClosePhantoonAfterDamage(body, state, PhantoonPhase.FadeOutWhileSwooping);
                state.TentaclesState.RearmHurtFlash();
                return;
            }

            if (vulnerableWindow)
            {
                // A vulnerable hit draws a random number the game keeps but nothing reads; the draw itself advances the
                // shared random sequence.
                _ = _room.Random.Next(PhantoonRandomDefinitions.ShotPattern);
                state.TentaclesState.RearmHurtFlash();
                if (!state.TentaclesState.SwoopRequested)
                {
                    state.TentaclesState.SwoopRequested = true;
                    if (state.BodyState.PhaseTimer >= 16)
                        state.BodyState.PhaseTimer = 16;
                }
                return;
            }
        }

        // A valid damaging shot outside the three reaction states still re-arms the flash.
        state.TentaclesState.RearmHurtFlash();
    }

    /// <summary>Closes the eye and ends the vulnerable interval after a damaging hit.</summary>
    private static void ClosePhantoonAfterDamage(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        PhantoonPhase fadeOut)
    {
        state.BodyState.Phase = fadeOut;
        state.BodyState.PhaseTimer = 0;
        RoomEnemySlot eye = state.Eye;
        state.TentaclesState.SwoopRequested = false;
        state.TentaclesState.RoundDamage = 0;
        state.Translucent = true;
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.InvulnerableBody);
        PhantoonPresentation.StartPhantoonAnimation(state, eye, PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.EyeState.FadeComplete = false;
        state.TentaclesState.RearmHurtFlash();
    }

    /// <summary>Switches from combat to the fatal swoop and death phases.</summary>
    private static void BeginPhantoonDeathSequence(
        PhantoonEnemyState state)
    {
        state.BodyState.Phase = state.BodyState.Phase is
            PhantoonPhase.Swooping or
            PhantoonPhase.FadeOutWhileSwooping
                ? PhantoonPhase.FinishFatalSwoop
                : PhantoonPhase.DyingFadeInOut;
        state.EyeState.FigureEightReversed = false;
        state.EyeState.FadeComplete = false;
        state.BodyWave.InitializeDeathScroll(state.BodyLayerPlacement.X);
        state.Translucent = true;
    }

    /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
    internal void RunPhantoonStartingFlameWaiting(EnemyProjectile flame)
    {
        PhantoonEnemyState? state = Phantoon;
        if (state is null || !state.BodyState.StartingFlamesActive)
            return;
        flame.FrameBehavior = PhantoonBehavior.PhantoonStartingFlameOrbitFrameBehavior;
        PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
        orbit.StartDelay = PhantoonFlameMotionDefinitions.IntroCircleDelay;
        orbit.Radius = PhantoonFlameMotionDefinitions.IntroCircleRadius;
    }

    /// <summary>Advances an intro flame around Phantoon's body.</summary>
    internal void RunPhantoonStartingFlameOrbit(EnemyProjectile flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Delete();
            return;
        }

        PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
        if (orbit.StartDelay != 0)
        {
            orbit.StartDelay--;
        }
        // The orbit reads the frame counter that wraps at 65536.
        else if (PhantoonCadenceDefinitions.OrbitClosesEverySecondUpdate.IsDue(_projectiles.CurrentFrame16))
        {
            orbit.Radius--;
            if (orbit.Radius == 0)
            {
                flame.Position = flame.Position.WithWhole(new(state.Body.Position.X.Whole, CoordinateAxis.Wrap(state.Body.Position.Y.Whole + 16)));
                flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Delete));
                flame.FrameHold = WrappingCountdown.Start(1);
                return;
            }
        }

        orbit.Angle += PhantoonFlameMotionDefinitions.StartingTurnPerUpdate;
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            orbit.Angle,
            orbit.Radius);
    }

    /// <summary>Moves a casual flame downward before floor contact.</summary>
    internal static void RunPhantoonCasualFlameFalling(
        EnemyProjectile flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonCasualFlameLanding;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualHitGround));
        flame.FrameHold = WrappingCountdown.Start(1);
        flame.State = new PhantoonCasualFlameState();
        flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole + 8);
    }

    /// <summary>Holds the flame briefly at the floor after impact.</summary>
    internal static void RunPhantoonCasualFlameImpactPause(
        EnemyProjectile flame,
        int frameCounter)
    {
        PhantoonCasualFlameState casual = flame.StateOf<PhantoonCasualFlameState>();
        casual.ImpactPause--;
        if (casual.ImpactPause != 0)
            return;

        flame.FrameBehavior =
            EnemyProjectileFrameBehavior.PhantoonCasualFlameBounce;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualBouncing));
        flame.FrameHold = WrappingCountdown.Start(1);
        flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole - 8);
        flame.Velocity = flame.Velocity.WithY(SignedEightEight.FromPixels(-3));
        // The flame's drift reads the frame counter that wraps at 65536.
        flame.Velocity = flame.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            PhantoonCadenceDefinitions.RisingFlameDriftsLeft.IsSetIn(frameCounter) ? -128 : 128));
    }

    /// <summary>Advances the flame's post-impact bounce motion.</summary>
    internal static void RunPhantoonCasualFlameBouncing(
        EnemyProjectile flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
        {
            PhantoonCasualFlameState casual = flame.StateOf<PhantoonCasualFlameState>();
            casual.Bounces++;
            if (casual.Bounces < 3)
            {
                flame.Velocity = flame.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(casual.Bounces == 1 ? -512 : -256));
                return;
            }
            RestPhantoonCasualFlame(flame);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Horizontal))
            RestPhantoonCasualFlame(flame);
    }

    /// <summary>Installs the resting phase after a flame's bounce ends.</summary>
    private static void RestPhantoonCasualFlame(EnemyProjectile flame)
    {
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualResting));
        flame.FrameHold = WrappingCountdown.Start(1);
        flame.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
    }

    /// <summary>Advances a rage-phase flame through its authored movement.</summary>
    internal void RunPhantoonEnragedFlame(EnemyProjectile flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Delete();
            return;
        }

        PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
        orbit.Radius += PhantoonFlameMotionDefinitions.RageRadiusStep;
        orbit.Angle += orbit.TurnPerUpdate;
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            orbit.Angle,
            orbit.Radius);
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Updates a delayed rain flame as it enters and falls through the room.</summary>
    internal void RunPhantoonRainFlame(
        EnemyProjectile flame,
        RoomLevelData level)
    {
        PhantoonRainFlameState rain = flame.StateOf<PhantoonRainFlameState>();
        if (rain.FallDelay != 0)
        {
            rain.FallDelay--;
            if (rain.FallDelay != 0)
                return;
            // Expiry falls through to movement on this call, including a delay
            // counted past zero. Keep each actor's queue call rather than a last-SFX latch.
            _audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
        }

        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.RainImpact));
        flame.FrameHold = WrappingCountdown.Start(1);
        flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole + 8);
        flame.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
        _audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
    }

    /// <summary>Moves a spiral flame around Phantoon's body.</summary>
    internal void RunPhantoonSpiralFlame(EnemyProjectile flame)
    {
        if (Phantoon is not { } state)
        {
            flame.Delete();
            return;
        }

        PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
        orbit.Radius += PhantoonFlameMotionDefinitions.SpiralRadiusStep;
        orbit.Angle += PhantoonFlameMotionDefinitions.SpiralTurnPerUpdate;
        PositionPhantoonFlameAroundBody(
            flame,
            state.Body,
            orbit.Angle,
            orbit.Radius);
        DeletePhantoonFlameOutsideRoom(flame);
    }

    /// <summary>Converts an orbit phase into a world-space flame position around the body.</summary>
    internal static void PositionPhantoonFlameAroundBody(
        EnemyProjectile flame,
        RoomEnemySlot body,
        TrigonometryAngle angle,
        int radius)
    {
        // Unlike the common enemy sine, which never quite reaches one, this flame's
        // sine reaches exactly one, so the flame lies at the exact full radius at cardinal angles.
        int xOffset = ReadPhantoonFlameComponent(angle, radius);
        int yOffset = ReadPhantoonFlameComponent(angle.TurnedBy(TrigonometryAngle.HalfTurn).TurnedBy(TrigonometryAngle.QuarterTurn), radius);
        flame.Position = flame.Position.WithWhole(new(CoordinateAxis.Wrap(body.Position.X.Whole + xOffset), CoordinateAxis.Wrap(body.Position.Y.Whole + 16 + yOffset)));
    }

    /// <summary>Unsigned half-wave multiplication followed by whole-result negation, as the flame orbit computes it.</summary>
    /// <param name="angle">The flame's orbit angle.</param>
    /// <param name="radius">The orbit radius, zero or more; the product uses its place within 256 (<see cref="ShortCircleProduct"/>).</param>
    internal static int ReadPhantoonFlameComponent(TrigonometryAngle angle, int radius)
    {
        // The half-wave sample is an unsigned value in 256ths of at most one: its fraction and its whole part each multiply
        // the radius.
        int sample = Trigonometry.SignedSine(angle.InFirstHalfTurn());
        int orbit = ShortCircleProduct.RadiusInUse(radius);
        int magnitude = EightEight.Multiply(EightEight.FractionOf(sample), orbit) + EightEight.WholeOf(sample) * orbit;
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>Switches a flame to its delete animation after leaving the room bounds.</summary>
    private static void DeletePhantoonFlameOutsideRoom(EnemyProjectile flame)
    {
        if (CoordinateAxis.AsSigned(flame.Position.X.Whole) < 0 || flame.Position.X.Whole >= 256 ||
            CoordinateAxis.AsSigned(flame.Position.Y.Whole) < 0 || flame.Position.Y.Whole >= 256)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Delete));
            flame.FrameHold = WrappingCountdown.Start(1);
        }
    }

    /// <summary>
    /// Runs the flame's drop step: the flame's exact position and the Phantoon eye's enemy definition go to the
    /// enemy drop spawner, which places the pickup from the definition's item table.
    /// </summary>
    internal void RequestPhantoonFlameDrop(EnemyProjectile flame)
    {
        if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
        {
            throw new InvalidOperationException(
                $"Projectile {flame.Kind} reached Phantoon's flame drop step.");
        }

        RoomEnemyDefinition eye = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.PhantoonEye);
        _deathsAndDrops.SpawnEnemyDropFromChanceTable(
            flame.Position.Whole,
            eye.DropChances);
    }

    /// <summary>Phantoon's encounter state once his body has initialized in the room.</summary>
    public PhantoonEnemyState? Phantoon { get; internal set; }

    /// <summary>Requires Phantoon's living body before handling one of his parts.</summary>
    internal PhantoonEnemyState RequirePhantoonState(RoomEnemySlot slot)
    {
        if (Phantoon is null ||
            Phantoon.Body.DefinitionId != EnemyDefinitionId.PhantoonBody)
        {
            throw new InvalidOperationException(
                $"Enemy {slot.DefinitionId} requires Phantoon's body.");
        }
        return Phantoon;
    }

    /// <summary>Recognizes the four enemy definitions that share Phantoon encounter state.</summary>
    internal static bool IsPhantoonPartDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.PhantoonBody or EnemyDefinitionId.PhantoonEye or
        EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth;

    /// <summary>Runs Phantoon's body contact through the shared enemy collision path.</summary>
    internal void ReactToPhantoonBodyTouch(EnemyTouchContext context)
    {
        if (!context.UsesFrameHitboxes)
            throw new InvalidDataException("Phantoon requires his authored extended touch hitboxes.");
        if (context.HitboxTouch == EnemyReaction.Ignore)
            return;
        if (context.HitboxTouch != PhantoonReactions.Touch)
        {
            throw new InvalidDataException(
                $"Phantoon has no response to touch reaction {context.HitboxTouch?.ToString() ?? "(unnamed)"}.");
        }
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindPhantoonHitboxReaction(RoomEnemySlot enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(PhantoonCombat.IsPhantoonPartDefinition(enemy.DefinitionId)))
            return null;

        // Phantoon's frames include hitbox lists,
        // but its editable background changes must never become collision
        // data. Preserve the component and rectangle order from
        // the compiled definitions, including point no-ops.
        foreach (EnemyCollisionComponent component in
                 PhantoonCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Left),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Top),
                        CoordinateAxis.Wrap(componentOrigin.X + hitbox.Right),
                        CoordinateAxis.Wrap(componentOrigin.Y + hitbox.Bottom)))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }
}
