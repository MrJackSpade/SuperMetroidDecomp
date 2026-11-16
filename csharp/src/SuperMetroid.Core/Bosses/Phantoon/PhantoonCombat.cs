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
    /// <summary>Publishes screen shake during Phantoon's death sequence.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues encounter sounds in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves Phantoon and flame contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonCombat(EnemyRoomBinding room, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _earthquake = earthquake;
        _audio = audio;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>The bomb hit response for vulnerable Phantoon phases.</summary>
    internal EnemyBombReaction PhantoonBombReaction => field ??= new()
    {
        // Phantoon's death is his own staged sequence, never the common one.
        RunsGenericDeath = false,
        // Phantoon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        // A bomb meets the same common damage as a beam or a missile, and the damage it does feeds the round's
        // damage; a lethal hit begins his death.
        AfterCommon = c =>
        {
            PhantoonEnemyState phantoon = Phantoon ??
                throw new InvalidOperationException("Phantoon's shot reaction ran without Phantoon's fight state.");
            int appliedDamage = c.HealthBefore - c.Enemy.Health;
            ResolvePhantoonShotReaction(c.Enemy, phantoon, c.Bomb.Type, appliedDamage);
        },
    };

    /// <summary>
    /// Phantoon's destroyable flame (#1760): its spawner starts its animation, and a shot one runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior DestroyableFlame => field ??= new PhantoonFlameBehavior(_room, _audio, _deathsAndDrops, _encounter,
        PhantoonFlameMotion.CasualFalling, () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableShot));

    /// <summary>
    /// One of Phantoon's intro flames (#1760): its spawner starts its animation, and a shot one runs the destroyable
    /// flame's idle animation, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior StartingFlame => field ??= new PhantoonFlameBehavior(_room, _audio, _deathsAndDrops, _encounter,
        PhantoonFlameMotion.IntroWaiting, () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableIdle));

    /// <summary>A Phantoon flame: every flame's motion, from the intro's wait and orbit to the rage and spiral, and the flame animations' cues.</summary>
    /// <param name="room">The loaded room, whose definitions name the flames' drops.</param>
    /// <param name="audio">The enemy sound queue the rain flames' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot destroyable flame leaves.</param>
    /// <param name="encounter">Phantoon's fight state, whose body the orbiting flames circle.</param>
    /// <param name="initial">The motion the flame starts with.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    private sealed class PhantoonFlameBehavior(
        EnemyRoomBinding room,
        EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops,
        PhantoonEncounter encounter,
        PhantoonFlameMotion initial,
        Func<EnemyProjectileAnimation> shot)
        : EnemyProjectileMotionBehavior<PhantoonFlameMotion>(initial, shot), IEnemyProjectileCues<PhantoonFlameCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, PhantoonFlameMotion motion)
        {
            EnemyProjectile flame = context.Projectile;
            switch (motion)
            {
                case PhantoonFlameMotion.IntroWaiting:
                    RunPhantoonStartingFlameWaiting(flame);
                    break;
                case PhantoonFlameMotion.IntroOrbiting:
                    RunPhantoonStartingFlameOrbit(flame, context.FrameCounter);
                    break;
                case PhantoonFlameMotion.CasualFalling:
                    RunPhantoonCasualFlameFalling(flame, context.Level);
                    break;
                case PhantoonFlameMotion.CasualLanding:
                    RunPhantoonCasualFlameImpactPause(flame, context.FrameCounter);
                    break;
                case PhantoonFlameMotion.CasualBouncing:
                    RunPhantoonCasualFlameBouncing(flame, context.Level);
                    break;
                case PhantoonFlameMotion.RageOrbiting:
                    RunPhantoonEnragedFlame(flame);
                    break;
                case PhantoonFlameMotion.RainFalling:
                    RunPhantoonRainFlame(flame, context.Level);
                    break;
                case PhantoonFlameMotion.SpiralOrbiting:
                    RunPhantoonSpiralFlame(flame);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a flame animation's cue: the flame's drop, or its deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, PhantoonFlameCue cue)
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
        }

        /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
        private void RunPhantoonStartingFlameWaiting(EnemyProjectile flame)
        {
            PhantoonEnemyState? state = encounter.Phantoon;
            if (state is null || !state.StartingFlames.Orbiting)
                return;
            flame.StartMotion(PhantoonFlameMotion.IntroOrbiting);
            PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
            orbit.StartDelay = PhantoonFlameMotionDefinitions.IntroCircleDelay;
            orbit.Radius = PhantoonFlameMotionDefinitions.IntroCircleRadius;
        }

        /// <summary>Advances an intro flame around Phantoon's body; <paramref name="frameCounter"/> times its orbit's closing.</summary>
        private void RunPhantoonStartingFlameOrbit(EnemyProjectile flame, int frameCounter)
        {
            if (encounter.Phantoon is not { } state)
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
            else if (PhantoonCadenceDefinitions.OrbitClosesEverySecondUpdate.IsDue(frameCounter))
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

        /// <summary>Advances a rage-phase flame through its authored movement.</summary>
        private void RunPhantoonEnragedFlame(EnemyProjectile flame)
        {
            if (encounter.Phantoon is not { } state)
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
        private void RunPhantoonRainFlame(
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
                audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
            }

            flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
            if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
                return;
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.RainImpact));
            flame.FrameHold = WrappingCountdown.Start(1);
            flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole + 8);
            flame.HoldStill();
            audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
        }

        /// <summary>Moves a spiral flame around Phantoon's body.</summary>
        private void RunPhantoonSpiralFlame(EnemyProjectile flame)
        {
            if (encounter.Phantoon is not { } state)
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

        /// <summary>
        /// Runs the flame's drop step: the flame's exact position and the Phantoon eye's enemy definition go to the
        /// enemy drop spawner, which places the pickup from the definition's item table.
        /// </summary>
        private void RequestPhantoonFlameDrop(EnemyProjectile flame)
        {
            if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
            {
                throw new InvalidOperationException(
                    $"Projectile {flame.Kind} reached Phantoon's flame drop step.");
            }

            RoomEnemyDefinition eye = room.Definitions.ReadEnemyDefinition(EnemyDefinitionId.PhantoonEye);
            deathsAndDrops.SpawnEnemyDropFromChanceTable(
                flame.Position.Whole,
                eye.DropChances);
        }
    }

    /// <summary>
    /// Tests Samus's beams and missiles against Phantoon's body and runs the reaction of the hitbox the first one meets.
    /// One projectile resolves per pass.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);

        if (Phantoon is not { } state)
            return 0;

        RoomEnemyPlace body = state.Body;
        if (body.DefinitionId != EnemyDefinitionId.PhantoonBody ||
            CommonEnemyEmptyFrames.ShotsPass(body) ||
            body.InvincibilityTimer != 0 ||
            body.Presence.HasAny(EnemyPresence.Deleted) ||
            body.Contact.HasAny(EnemyContact.IgnoresSamus))
        {
            return 0;
        }

        foreach (SamusProjectile projectile in projectiles.Shots)
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
                state.Phase is PhantoonPhase.DyingFadeInOut or PhantoonPhase.DyingExplosions or
                    PhantoonPhase.BeginFinalWavyDeath or PhantoonPhase.DyingFadeOut or PhantoonPhase.AlmostDead or
                    PhantoonPhase.Dead or PhantoonPhase.WaitingOnEye)
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
            // the projectile's own update owns the following-frame deletion, while
            // penetrating Plasma retains its type, direction, and animation.
            projectiles.EnemyImpacts.ApplyFrameHitboxCollisionPrelude(projectile,
                body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.HasAnyBeam(SamusBeamFlags.Plasma));

            int healthBefore = body.Health;
            // Phantoon calls common normal-shot damage, including its charged-beam
            // override. Special attack particles retain that bit; reading only the uncharged
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
                        body.InvincibilityTimer = EnemyCollisionDefinitions.PlasmaShotInvincibilityUpdates;
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
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusProjectileType projectileType,
        int damage)
    {
        if (body.Health == 0)
        {
            _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.ShotCry);
            state.Round.RearmHurtFlash();
            body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
            BeginPhantoonDeathSequence(state);
            return;
        }

        if ((body.AiStates & EnemyAiStates.Hurt) == EnemyAiStates.None)
            return;

        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.ShotCry);
        SamusProjectileFamily family = projectileType.Family;
        PhantoonPhase phase = state.Phase;
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

            // The round ends within an update or two of its damage reaching the threshold, so its sum never nears a wrap.
            state.Round.Damage += damage;
            if (state.Round.Damage >= PhantoonBehavior.PhantoonDamageThreshold)
            {
                if (swooping)
                    state.PhaseTimer = 1;
                else
                    ClosePhantoonAfterDamage(body, state, PhantoonPhase.FadeOutWhileSwooping);
                state.Round.RearmHurtFlash();
                return;
            }

            if (vulnerableWindow)
            {
                // A vulnerable hit draws a random number the game keeps but nothing reads; the draw itself advances the
                // shared random sequence.
                _ = _room.Random.Next(PhantoonRandomDefinitions.ShotPattern);
                state.Round.RearmHurtFlash();
                if (!state.Swoop.Requested)
                {
                    state.Swoop.Requested = true;
                    if (state.PhaseTimer >= 16)
                        state.PhaseTimer = 16;
                }
                return;
            }
        }

        // A valid damaging shot outside the three reaction states still re-arms the flash.
        state.Round.RearmHurtFlash();
    }

    /// <summary>Closes the eye and ends the vulnerable interval after a damaging hit.</summary>
    private static void ClosePhantoonAfterDamage(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        PhantoonPhase fadeOut)
    {
        state.Phase = fadeOut;
        state.PhaseTimer = 0;
        RoomEnemyPlace eye = state.Eye;
        state.Swoop.Requested = false;
        state.Round.Damage = 0;
        state.Translucent = true;
        PhantoonPresentation.StartPhantoonAnimation(state, body, PhantoonPartSegment.InvulnerableBody);
        PhantoonPresentation.StartPhantoonAnimation(state, eye, PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyFade.Complete = false;
        state.Round.RearmHurtFlash();
    }

    /// <summary>Switches from combat to the fatal swoop and death phases.</summary>
    private static void BeginPhantoonDeathSequence(
        PhantoonEnemyState state)
    {
        state.Phase = state.Phase is
            PhantoonPhase.Swooping or
            PhantoonPhase.FadeOutWhileSwooping
                ? PhantoonPhase.FinishFatalSwoop
                : PhantoonPhase.DyingFadeInOut;
        state.FigureEight.Reversed = false;
        state.BodyFade.Complete = false;
        state.BodyWave.InitializeDeathScroll();
        state.Translucent = true;
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
        flame.StartMotion(PhantoonFlameMotion.CasualLanding);
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

        flame.StartMotion(PhantoonFlameMotion.CasualBouncing);
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
        flame.HoldStill();
    }

    /// <summary>Converts an orbit phase into a world-space flame position around the body.</summary>
    internal static void PositionPhantoonFlameAroundBody(
        EnemyProjectile flame,
        RoomEnemyPlace body,
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

    /// <summary>Phantoon's fight state, which his flames share.</summary>
    private readonly PhantoonEncounter _encounter = new();

    /// <summary>Phantoon's encounter state once his body has initialized in the room.</summary>
    public PhantoonEnemyState? Phantoon
    {
        get => _encounter.Phantoon;
        internal set => _encounter.Phantoon = value;
    }

    /// <summary>Requires Phantoon's living body before handling one of his parts.</summary>
    internal PhantoonEnemyState RequirePhantoonState(RoomEnemyPlace place)
    {
        if (Phantoon is null ||
            Phantoon.Body.DefinitionId != EnemyDefinitionId.PhantoonBody)
        {
            throw new InvalidOperationException(
                $"Enemy {place.DefinitionId} requires Phantoon's body.");
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
        _collision.ResolveNormalEnemyTouch(context.Place, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
    }

    /// <summary>The reaction of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindPhantoonHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        if (!(PhantoonCombat.IsPhantoonPartDefinition(enemy.DefinitionId)))
            return null;

        // Phantoon's frames' hitboxes are the compiled definitions', never his editable background frames'; the
        // invulnerable body's centre point is a compatibility rule (PhantoonSolidifyingCentre).
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
