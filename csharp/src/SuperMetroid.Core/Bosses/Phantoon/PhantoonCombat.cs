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
    /// <param name="state">The encounter, which the reaction acts on.</param>
    internal EnemyBombReaction PhantoonBombReactionOf(PhantoonEnemyState state) => new()
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
            ResolvePhantoonShotReaction(c.Enemy, state, c.Bomb.Type, appliedDamage);
        },
    };

    /// <summary>
    /// Phantoon's destroyable flame (#1760): its spawner starts its animation, and a shot one dies, leaves its drop and is
    /// deleted, the animation its definition names.
    /// </summary>
    /// <param name="flame">What the flame keeps: its orbit or its rain, or nothing for a casual flame until it lands.</param>
    internal IPhantoonFlame DestroyableFlame(PhantoonFlameKeeps flame) => flame switch
    {
        PhantoonFlameKeeps.Orbit orbit => new OrbitingFlameBehavior(_deathsAndDrops, orbit.Circling, PhantoonFlameSegment.Dying, orbit.State),
        PhantoonFlameKeeps.Rain rain => new RainFlameBehavior(_audio, _deathsAndDrops, PhantoonFlameSegment.Dying, rain.State),
        PhantoonFlameKeeps.Casual => new FallingCasualFlameBehavior(_deathsAndDrops),
        _ => throw new InvalidOperationException($"A Phantoon flame keeps {flame}, which no case answers."),
    };

    /// <summary>
    /// One of Phantoon's intro flames (#1760): its spawner starts its animation, and a shot one burns as the destroyable
    /// flame idles, the animation its definition names.
    /// </summary>
    /// <param name="orbit">The flame's orbit round Phantoon's body.</param>
    internal IPhantoonFlame StartingFlame(PhantoonOrbitingFlameState orbit) => new OrbitingFlameBehavior(_deathsAndDrops,
        PhantoonFlameMotion.IntroWaiting, PhantoonFlameSegment.Burning, orbit);

    /// <summary>One of Phantoon's flames: its behaviour, which owns its animations' cues (#2554).</summary>
    internal interface IPhantoonFlame : IEnemyProjectileCues<PhantoonFlameCue>
    {
        /// <summary>The flame's behaviour, bound to it at its spawn.</summary>
        EnemyProjectileBehavior Behavior { get; }
    }

    /// <summary>
    /// A Phantoon flame: the flame animations' cues; each kind of flame moves by its own behaviour, which keeps what its
    /// motions read.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot destroyable flame leaves.</param>
    /// <param name="initial">The motion the flame starts with.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <typeparam name="TMotion">The motions of the kind of flame.</typeparam>
    private abstract class PhantoonFlameBehavior<TMotion>(
        EnemyDeathsAndDrops deathsAndDrops,
        TMotion initial,
        PhantoonFlameSegment shot)
        : EnemyProjectileMotionBehavior<TMotion>(initial), IPhantoonFlame
        where TMotion : struct, Enum
    {
        /// <inheritdoc/>
        public EnemyProjectileBehavior Behavior => this;

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new PhantoonFlameAnimation(shot, this);

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

        /// <summary>The room's drops, which a shot destroyable flame leaves.</summary>
        private protected EnemyDeathsAndDrops Drops => deathsAndDrops;

        /// <summary>Holds an intro flame until its delayed orbit begins.</summary>
        private protected static void RunPhantoonStartingFlameWaiting(EnemyProjectile flame, PhantoonOrbitingFlameState orbit,
            EnemyProjectileMotionBehavior<PhantoonFlameMotion> circling)
        {
            if (!orbit.Phantoon.StartingFlames.Orbiting)
                return;
            circling.StartMotion(flame, PhantoonFlameMotion.IntroOrbiting);
            orbit.FullRadiusHold.LastFor(PhantoonFlameMotionDefinitions.IntroCircleHoldUpdates);
            orbit.Radius = PhantoonFlameMotionDefinitions.IntroCircleRadius;
        }

        /// <summary>Advances an intro flame around Phantoon's body; <paramref name="frameCounter"/> times its orbit's closing.</summary>
        private protected static void RunPhantoonStartingFlameOrbit(EnemyProjectile flame, PhantoonOrbitingFlameState orbit, int frameCounter, IEnemyProjectileCues<PhantoonFlameCue> cues)
        {
            PhantoonEnemyState state = orbit.Phantoon;
            // Once its hold at its full radius has run out, the orbit closes on every second update of the game's frame
            // counter.
            if (orbit.FullRadiusHold.StepRunsOut() && PhantoonCadenceDefinitions.OrbitClosesEverySecondUpdate.IsDue(frameCounter))
            {
                orbit.Radius--;
                if (orbit.Radius == 0)
                {
                    flame.Position = flame.Position.WithWhole(state.Body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre);
                    flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Delete, cues));
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
        private protected static void RunPhantoonEnragedFlame(EnemyProjectile flame, PhantoonOrbitingFlameState orbit, IEnemyProjectileCues<PhantoonFlameCue> cues)
        {
            orbit.Radius += PhantoonFlameMotionDefinitions.RageRadiusStep;
            orbit.Angle += orbit.TurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
                orbit.Angle,
                orbit.Radius);
            DeletePhantoonFlameOutsideRoom(flame, cues);
        }

        /// <summary>Updates a delayed rain flame as it enters and falls through the room.</summary>
        private protected static void RunPhantoonRainFlame(
            EnemyProjectile flame,
            PhantoonRainFlameState rain,
            EnemyAudioQueue audio,
            RoomLevelData level,
            IEnemyProjectileCues<PhantoonFlameCue> cues)
        {
            if (rain.HangWait is { } hang)
            {
                if (hang.StepKeepsRunning())
                    return;
                // The flame queues its own falling sound as its hang runs out, and falls in that same update.
                rain.HangWait = null;
                audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
            }

            flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
            if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
                return;
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.RainImpact, cues));
            flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
            flame.HoldStill();
            audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
        }

        /// <summary>Moves a spiral flame around Phantoon's body.</summary>
        private protected static void RunPhantoonSpiralFlame(EnemyProjectile flame, PhantoonOrbitingFlameState orbit, IEnemyProjectileCues<PhantoonFlameCue> cues)
        {
            orbit.Radius += PhantoonFlameMotionDefinitions.SpiralRadiusStep;
            orbit.Angle += PhantoonFlameMotionDefinitions.SpiralTurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
                orbit.Angle,
                orbit.Radius);
            DeletePhantoonFlameOutsideRoom(flame, cues);
        }

        /// <summary>
        /// Runs the flame's drop step: the flame's exact position and the Phantoon eye's enemy definition go to the
        /// enemy drop spawner, which places the pickup from the definition's item table.
        /// </summary>
        private void RequestPhantoonFlameDrop(EnemyProjectile flame, SamusState samus)
        {
            deathsAndDrops.SpawnEnemyDropFromDefinition(flame.Position.Whole, EnemyDefinitionId.PhantoonEye, samus);
        }
    }

    /// <summary>A flame that circles Phantoon's body: an intro flame, a rage flame or a spiral flame (#2252).</summary>
    /// <param name="deathsAndDrops">The drops a shot destroyable flame leaves.</param>
    /// <param name="initial">The motion the flame starts with.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="orbit">The flame's orbit, holding the encounter whose body it circles.</param>
    private sealed class OrbitingFlameBehavior(EnemyDeathsAndDrops deathsAndDrops, PhantoonFlameMotion initial,
        PhantoonFlameSegment shot, PhantoonOrbitingFlameState orbit)
        : PhantoonFlameBehavior<PhantoonFlameMotion>(deathsAndDrops, initial, shot)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, PhantoonFlameMotion motion)
        {
            EnemyProjectile flame = context.Projectile;
            switch (motion)
            {
                case PhantoonFlameMotion.IntroWaiting:
                    RunPhantoonStartingFlameWaiting(flame, orbit, this);
                    break;
                case PhantoonFlameMotion.IntroOrbiting:
                    RunPhantoonStartingFlameOrbit(flame, orbit, context.FrameCounter, this);
                    break;
                case PhantoonFlameMotion.RageOrbiting:
                    RunPhantoonEnragedFlame(flame, orbit, this);
                    break;
                case PhantoonFlameMotion.SpiralOrbiting:
                    RunPhantoonSpiralFlame(flame, orbit, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonFlameMotion)} {motion}.");
            }
        }
    }

    /// <summary>A rain flame, which hangs, then falls through the room.</summary>
    /// <param name="audio">The enemy sound queue the rain flame's sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot flame leaves.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="rain">What the flame keeps: its hang.</param>
    private sealed class RainFlameBehavior(EnemyAudioQueue audio, EnemyDeathsAndDrops deathsAndDrops, PhantoonFlameSegment shot,
        PhantoonRainFlameState rain)
        : PhantoonFlameBehavior<SingleMotion>(deathsAndDrops, SingleMotion.Moving, shot)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            RunPhantoonRainFlame(context.Projectile, rain, audio, context.Level, this);
    }

    /// <summary>A casual flame falling to the floor; on landing it keeps its landing pause and bounces.</summary>
    /// <param name="deathsAndDrops">The drops a shot flame leaves.</param>
    private sealed class FallingCasualFlameBehavior(EnemyDeathsAndDrops deathsAndDrops)
        : PhantoonFlameBehavior<SingleMotion>(deathsAndDrops, SingleMotion.Moving, PhantoonFlameSegment.Dying)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, SingleMotion motion)
        {
            if (RunPhantoonCasualFlameFalling(context.Projectile, context.Level, this))
                context.Projectile.Rebind(new LandedCasualFlameBehavior(Drops, new PhantoonCasualFlameState()));
        }
    }

    /// <summary>A casual flame that has landed: it pauses, then bounces and rests.</summary>
    /// <param name="deathsAndDrops">The drops a shot flame leaves.</param>
    /// <param name="casual">What the flame keeps: its landing pause and its bounces.</param>
    private sealed class LandedCasualFlameBehavior(EnemyDeathsAndDrops deathsAndDrops, PhantoonCasualFlameState casual)
        : PhantoonFlameBehavior<PhantoonLandedFlameMotion>(deathsAndDrops, PhantoonLandedFlameMotion.CasualLanding, PhantoonFlameSegment.Dying)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, PhantoonLandedFlameMotion motion)
        {
            switch (motion)
            {
                case PhantoonLandedFlameMotion.CasualLanding:
                    RunPhantoonCasualFlameLandingPause(context.Projectile, casual, context.FrameCounter, this);
                    break;
                case PhantoonLandedFlameMotion.CasualBouncing:
                    RunPhantoonCasualFlameBouncing(context.Projectile, casual, context.Level, this);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(PhantoonLandedFlameMotion)} {motion}.");
            }
        }
    }

    /// <summary>
    /// Tests Samus's beams and missiles against the body of <paramref name="state"/>'s Phantoon, which his body's entity
    /// resolves, and runs the reaction of the hitbox the first one meets. One projectile resolves per pass.
    /// </summary>
    public int ResolvePhantoonProjectileHits(
        PhantoonEnemyState state,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem samusBombs)
    {
        Ensure.NotNull(state);
        Ensure.NotNull(projectiles);
        Ensure.NotNull(samusBombs);

        RoomEnemyPlace body = state.Body;
        if (CommonEnemyEmptyFrames.ShotsPass(body) ||
            body.Invincibility.IsRunning ||
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
                projectiles.EnemyImpacts.TouchEnemyWithoutImpact(
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
            projectiles.EnemyImpacts.TouchEnemyWithoutImpact(projectile,
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

            // The round ends within an update or two of its damage reaching the threshold.
            state.Round.Damage += damage;
            if (state.Round.Damage >= PhantoonPatternDefinitions.RoundDamageThreshold)
            {
                if (swooping)
                    state.PhaseWait.LastFor(PhantoonArenaDefinitions.RoundEndingSwoopUpdates);
                else
                    ClosePhantoonAfterDamage(body, state, PhantoonPhase.FadeOutWhileSwooping);
                state.Round.RearmHurtFlash();
                return;
            }

            if (vulnerableWindow)
            {
                // A vulnerable hit draws a random number whose only effect is to advance the shared random
                // sequence.
                _ = _room.Random.Generate();
                state.Round.RearmHurtFlash();
                if (state.WindowEnd != PhantoonWindowEnd.Swoop)
                {
                    state.WindowEnd = PhantoonWindowEnd.Swoop;
                    if (state.PhaseWait.UpdatesLeft >= PhantoonArenaDefinitions.SwoopRequestTimerCap)
                        state.PhaseWait.LastFor(PhantoonArenaDefinitions.SwoopRequestTimerCap);
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
        state.WindowEnd = PhantoonWindowEnd.EyeCloses;
        state.Round.Damage = 0;
        state.Translucent = true;
        state.BodyPart.StartAnimation(PhantoonPartSegment.InvulnerableBody);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyFade.Resume();
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
        state.FigureEight.Direction = PhantoonPathDirection.Forwards;
        state.BodyFade.Resume();
        state.BodyWave.InitializeDeathScroll();
        state.Translucent = true;
    }

    /// <summary>Moves a casual flame downward before floor contact; returns whether it landed.</summary>
    internal static bool RunPhantoonCasualFlameFalling(
        EnemyProjectile flame,
        RoomLevelData level,
        IEnemyProjectileCues<PhantoonFlameCue> cues)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return false;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualHitGround, cues));
        flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
        return true;
    }

    /// <summary>Holds the flame briefly at the floor after impact.</summary>
    internal static void RunPhantoonCasualFlameLandingPause<TFlame>(
        EnemyProjectile flame,
        PhantoonCasualFlameState casual,
        int frameCounter,
        TFlame landed)
        where TFlame : EnemyProjectileMotionBehavior<PhantoonLandedFlameMotion>, IEnemyProjectileCues<PhantoonFlameCue>
    {
        if (!casual.LandingPause.StepRunsOut())
            return;

        landed.StartMotion(flame, PhantoonLandedFlameMotion.CasualBouncing);
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Burning, landed));
        flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole - PhantoonArenaDefinitions.FlameLandingDrop);
        flame.Velocity = flame.Velocity.WithY(PhantoonArenaDefinitions.CasualRiseSpeed);
        // The flame drifts left or right by the game's frame counter.
        flame.Velocity = flame.Velocity.WithX(
            PhantoonArenaDefinitions.CasualDriftSpeed.Toward(
                PhantoonCadenceDefinitions.RisingFlameDriftsLeft.IsSetIn(frameCounter) ? HorizontalDirection.Left : HorizontalDirection.Right));
    }

    /// <summary>Advances the flame's post-impact bounce motion.</summary>
    internal static void RunPhantoonCasualFlameBouncing(
        EnemyProjectile flame,
        PhantoonCasualFlameState casual,
        RoomLevelData level,
        IEnemyProjectileCues<PhantoonFlameCue> cues)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
        {
            casual.Bounces++;
            if (casual.Bounces < PhantoonArenaDefinitions.CasualBounces)
            {
                flame.Velocity = flame.Velocity.WithY(casual.Bounces == 1 ? PhantoonArenaDefinitions.CasualFirstBounceSpeed : PhantoonArenaDefinitions.CasualLaterBounceSpeed);
                return;
            }
            RestPhantoonCasualFlame(flame, cues);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Horizontal))
            RestPhantoonCasualFlame(flame, cues);
    }

    /// <summary>Starts a flame's rest after its bounce ends.</summary>
    private static void RestPhantoonCasualFlame(EnemyProjectile flame, IEnemyProjectileCues<PhantoonFlameCue> cues)
    {
        flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.CasualResting, cues));
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

    /// <summary>
    /// One part of a flame's place on its orbit: the radius laid along the angle's sine, rounded toward zero
    /// (<see cref="UnitComponent.ReachOf"/>).
    /// </summary>
    /// <param name="angle">The flame's orbit angle.</param>
    /// <param name="radius">The orbit radius, zero or more; the product uses its place within 256 (<see cref="ShortCircleProduct"/>).</param>
    internal static int ReadPhantoonFlameComponent(TrigonometryAngle angle, int radius) =>
        UnitComponent.SineOf(angle).ReachOf(ShortCircleProduct.RadiusInUse(radius));

    /// <summary>
    /// Switches a flame to its delete animation after leaving the room bounds: a place before the room, counted back from
    /// the far end of the room's coordinates (<see cref="OffRoomPlaces"/>), lies past the room's far edges too.
    /// </summary>
    private static void DeletePhantoonFlameOutsideRoom(EnemyProjectile flame, IEnemyProjectileCues<PhantoonFlameCue> cues)
    {
        if (flame.Position.X.Whole >= PhantoonArenaDefinitions.RoomSize || flame.Position.Y.Whole >= PhantoonArenaDefinitions.RoomSize)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(PhantoonFlameSegment.Delete, cues));
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
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
    }

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on <paramref name="enemy"/>'s frame, one of Phantoon's parts,
    /// which its own entity asks for (#2301).
    /// </summary>
    internal static bool FindPhantoonHitboxReaction(RoomEnemyPlace enemy, EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;

        // Phantoon's frames' hitboxes are his collision definitions', never his editable background frames'; the
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
