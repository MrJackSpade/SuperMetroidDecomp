using SuperMetroid.Core.Samus;
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
            int appliedDamage = c.HealthBefore - c.Enemy.Health;
            ResolvePhantoonShotReaction(c.Enemy, PhantoonEntity.EncounterOf(c.Enemy), c.Bomb.Type, appliedDamage);
        },
    };

    /// <summary>
    /// Phantoon's destroyable flame (#1760): its spawner starts its animation, and a shot one runs its typed shot
    /// animation, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior DestroyableFlame => field ??= new PhantoonFlameBehavior(_room, _audio, _deathsAndDrops,
        PhantoonFlameMotion.CasualFalling, () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableShot));

    /// <summary>
    /// One of Phantoon's intro flames (#1760): its spawner starts its animation, and a shot one runs the destroyable
    /// flame's idle animation, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior StartingFlame => field ??= new PhantoonFlameBehavior(_room, _audio, _deathsAndDrops,
        PhantoonFlameMotion.IntroWaiting, () => new PhantoonFlameAnimation(PhantoonFlameSegment.DestroyableIdle));

    /// <summary>
    /// A Phantoon flame: every flame's motion, from the intro's wait and orbit to the rage and spiral, and the flame
    /// animations' cues. A circling flame holds the encounter that spawned it (#2252), whose body it circles.
    /// </summary>
    /// <param name="room">The loaded room, whose definitions name the flames' drops.</param>
    /// <param name="audio">The enemy sound queue the rain flames' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot destroyable flame leaves.</param>
    /// <param name="initial">The motion the flame starts with.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    private sealed class PhantoonFlameBehavior(
        EnemyRoomBinding room,
        EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops,
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
                    RequestPhantoonFlameDrop(projectile, context.Samus);
                    return;
                case PhantoonFlameCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameCue)} {cue}.");
            }
        }

        /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
        private static void RunPhantoonStartingFlameWaiting(EnemyProjectile flame)
        {
            PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
            if (!orbit.Phantoon.StartingFlames.Orbiting)
                return;
            flame.StartMotion(PhantoonFlameMotion.IntroOrbiting);
            orbit.StartDelay = PhantoonFlameMotionDefinitions.IntroCircleDelay;
            orbit.Radius = PhantoonFlameMotionDefinitions.IntroCircleRadius;
        }

        /// <summary>Advances an intro flame around Phantoon's body; <paramref name="frameCounter"/> times its orbit's closing.</summary>
        private static void RunPhantoonStartingFlameOrbit(EnemyProjectile flame, int frameCounter)
        {
            PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
            PhantoonEnemyState state = orbit.Phantoon;
            if (orbit.StartDelay != 0)
            {
                orbit.StartDelay--;
            }
            // The orbit closes on every second update of the game's frame counter.
            else if (PhantoonCadenceDefinitions.OrbitClosesEverySecondUpdate.IsDue(frameCounter))
            {
                orbit.Radius--;
                if (orbit.Radius == 0)
                {
                    flame.Position = flame.Position.WithWhole(state.Body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre);
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
        private static void RunPhantoonEnragedFlame(EnemyProjectile flame)
        {
            PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
            orbit.Radius += PhantoonFlameMotionDefinitions.RageRadiusStep;
            orbit.Angle += orbit.TurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
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
                // The flame queues its own falling sound as its delay runs out, and falls in that same update.
                audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
            }

            flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
            if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
                return;
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.RainImpact));
            flame.FrameHold = WrappingCountdown.Start(1);
            flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
            flame.HoldStill();
            audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
        }

        /// <summary>Moves a spiral flame around Phantoon's body.</summary>
        private static void RunPhantoonSpiralFlame(EnemyProjectile flame)
        {
            PhantoonOrbitingFlameState orbit = flame.StateOf<PhantoonOrbitingFlameState>();
            orbit.Radius += PhantoonFlameMotionDefinitions.SpiralRadiusStep;
            orbit.Angle += PhantoonFlameMotionDefinitions.SpiralTurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
                orbit.Angle,
                orbit.Radius);
            DeletePhantoonFlameOutsideRoom(flame);
        }

        /// <summary>
        /// Runs the flame's drop step: the flame's exact position and the Phantoon eye's enemy definition go to the
        /// enemy drop spawner, which places the pickup from the definition's item table.
        /// </summary>
        private void RequestPhantoonFlameDrop(EnemyProjectile flame, SamusState samus)
        {
            if (flame.Kind != RoomEnemyProjectileKind.PhantoonDestroyableFlame)
            {
                throw new InvalidOperationException(
                    $"Projectile {flame.Kind} reached Phantoon's flame drop step.");
            }

            RoomEnemyDefinition eye = room.Definitions.ReadEnemyDefinition(EnemyDefinitionId.PhantoonEye);
            deathsAndDrops.SpawnEnemyDropFromChanceTable(
                flame.Position.Whole,
                eye.DropChances,
                samus);
        }
    }

    /// <summary>
    /// Tests Samus's beams and missiles against the body of <paramref name="state"/>'s Phantoon, which his body's entity
    /// resolves, and runs the reaction of the hitbox the first one meets. One projectile resolves per pass.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        PhantoonEnemyState state,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(projectiles);
        Ensure.NotNull(sharedProjectiles);

        RoomEnemyPlace body = state.Body;
        if (CommonEnemyEmptyFrames.ShotsPass(body) ||
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
            // Bombs, Power Bombs and the explosions beams and missiles leave behind never hit Phantoon.
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

            // A Super Missile that meets one of his hitboxes shakes the room before his reaction, so even a hit that
            // does nothing else shakes it, and the shot is still marked as having struck.
            if (family == SamusProjectileFamily.SuperMissile)
            {
                _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            }

            if (hitboxShot == EnemyReaction.Ignore ||
                state.Phase is PhantoonPhase.DyingFadeInOut or PhantoonPhase.DyingExplosions or
                    PhantoonPhase.BeginFinalWavyDeath or PhantoonPhase.DyingFadeOut or PhantoonPhase.AlmostDead or
                    PhantoonPhase.Dead or PhantoonPhase.WaitingOnEye)
            {
                projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
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

            // The shot is marked as having struck before Phantoon's reaction, whose damage does not turn it into an
            // explosion: the shot ends on its own next update, while a Plasma shot that passes through enemies keeps
            // its type, direction and animation.
            projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(projectile,
                body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.HasAnyBeam(SamusBeamFlags.Plasma));

            int healthBefore = body.Health;
            // Phantoon's shot reaction runs common normal-shot damage (#2255), including its charged-beam override, a
            // zero-damage shot's dud, and no death of its own; his own stage follows it. A special attack's particles
            // count as a charged beam, so the Wave Beam's shield attack damages him.
            _ = _collision.ApplyNormalShotDamage(body, projectile, projectileType, projectileDamage, _room.Area);
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
            // A single Super Missile (600 damage) skips ordinary round accumulation
            // and enters the eight-wave rage sequence immediately.
            if (damage >= PhantoonPatternDefinitions.RoundDamageThreshold &&
                family == SamusProjectileFamily.SuperMissile)
            {
                ClosePhantoonAfterDamage(body, state, PhantoonPhase.FadeOutBeforeRage);
                return;
            }

            // The round ends within an update or two of its damage reaching the threshold, so its sum never nears a wrap.
            state.Round.Damage += damage;
            if (state.Round.Damage >= PhantoonPatternDefinitions.RoundDamageThreshold)
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
                    if (state.PhaseTimer >= PhantoonArenaDefinitions.SwoopRequestTimerCap)
                        state.PhaseTimer = PhantoonArenaDefinitions.SwoopRequestTimerCap;
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
        state.Swoop.Requested = false;
        state.Round.Damage = 0;
        state.Translucent = true;
        state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
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
        flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
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
        flame.Position = flame.Position.WithWrappedWholeY(flame.Position.Y.Whole - PhantoonArenaDefinitions.FlameLandingDrop);
        flame.Velocity = flame.Velocity.WithY(PhantoonArenaDefinitions.CasualRiseSpeed);
        // The flame drifts left or right by the game's frame counter.
        flame.Velocity = flame.Velocity.WithX(
            PhantoonCadenceDefinitions.RisingFlameDriftsLeft.IsSetIn(frameCounter) ? -PhantoonArenaDefinitions.CasualDriftSpeed : PhantoonArenaDefinitions.CasualDriftSpeed);
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
            if (casual.Bounces < PhantoonArenaDefinitions.CasualBounces)
            {
                flame.Velocity = flame.Velocity.WithY(casual.Bounces == 1 ? PhantoonArenaDefinitions.CasualFirstBounceSpeed : PhantoonArenaDefinitions.CasualLaterBounceSpeed);
                return;
            }
            RestPhantoonCasualFlame(flame);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Horizontal))
            RestPhantoonCasualFlame(flame);
    }

    /// <summary>Starts a flame's rest after its bounce ends.</summary>
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
        flame.Position = flame.Position.WithWhole(body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre + new Offset16<RoomPixels>(xOffset, yOffset));
    }

    /// <summary>One part of a flame's place on its orbit: the radius times the wave's size, negated over the second half turn.</summary>
    /// <param name="angle">The flame's orbit angle.</param>
    /// <param name="radius">The orbit radius, zero or more; the product uses its place within 256 (<see cref="ShortCircleProduct"/>).</param>
    internal static int ReadPhantoonFlameComponent(TrigonometryAngle angle, int radius)
    {
        // The wave's size is in 256ths, at most one whole: its fraction and its whole part each multiply the radius.
        int sample = Trigonometry.SignedSine(angle.InFirstHalfTurn());
        int orbit = ShortCircleProduct.RadiusInUse(radius);
        int magnitude = EightEight.Multiply(EightEight.FractionOf(sample), orbit) + EightEight.WholeOf(sample) * orbit;
        return angle.IsInSecondHalf ? -magnitude : magnitude;
    }

    /// <summary>Switches a flame to its delete animation after leaving the room bounds.</summary>
    private static void DeletePhantoonFlameOutsideRoom(EnemyProjectile flame)
    {
        if (CoordinateAxis.AsSigned(flame.Position.X.Whole) < 0 || flame.Position.X.Whole >= PhantoonArenaDefinitions.RoomSize ||
            CoordinateAxis.AsSigned(flame.Position.Y.Whole) < 0 || flame.Position.Y.Whole >= PhantoonArenaDefinitions.RoomSize)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Delete));
            flame.FrameHold = WrappingCountdown.Start(1);
        }
    }


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
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
    }

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on <paramref name="enemy"/>'s frame, one of Phantoon's parts,
    /// which its own entity asks for (#2301).
    /// </summary>
    internal static bool FindPhantoonHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // Phantoon's frames' hitboxes are the compiled definitions', never his editable background frames'; the
        // invulnerable body's centre point is a compatibility rule (PhantoonSolidifyingCentre).
        foreach (EnemyCollisionComponent component in
                 PhantoonCollision.ComponentsAt(enemy.Frame))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (EnemyCollisionHitbox hitbox in component.Hitboxes)
            {
                if (!probe.Overlaps(
                        componentOrigin.X + hitbox.Left,
                        componentOrigin.Y + hitbox.Top,
                        componentOrigin.X + hitbox.Right,
                        componentOrigin.Y + hitbox.Bottom))
                    continue;
                reaction = probe.SelectsShot ? hitbox.Shot : hitbox.Touch;
                return true;
            }
        }
        return false;
    }
}
