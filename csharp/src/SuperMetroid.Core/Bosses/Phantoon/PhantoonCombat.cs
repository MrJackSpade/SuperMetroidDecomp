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
internal sealed partial class PhantoonCombat
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
        AfterCommon = ResolvePhantoonBombReaction,
    };

    /// <summary>Feeds a bomb's common damage to the round's damage; a lethal hit begins his death.</summary>
    /// <param name="c">The bomb's hit.</param>
    private void ResolvePhantoonBombReaction(EnemyBombHitContext c)
    {
        int appliedDamage = c.HealthBefore - c.Enemy.Health;
        ResolvePhantoonShotReaction(c.Enemy, PhantoonEntity.EncounterOf(c.Enemy), c.PlacedBomb.Type, appliedDamage);
    }

    /// <summary>
    /// Phantoon's destroyable flame (#1760): its spawner starts its animation, and a shot one dies, leaves its drop and is
    /// deleted, the animation its definition names.
    /// </summary>
    internal PhantoonFlameBehavior DestroyableFlame => field ??= new PhantoonFlameBehavior(_audio, _deathsAndDrops,
        StartsCasualFalling, PhantoonFlameSegment.Dying);

    /// <summary>A destroyable flame starts falling to the floor.</summary>
    private static EnemyProjectileOwnedMotion StartsCasualFalling(PhantoonFlameBehavior flames) => flames.CasualFalling;

    /// <summary>An intro flame starts waiting at its place around Phantoon.</summary>
    private static EnemyProjectileOwnedMotion StartsIntroWaiting(PhantoonFlameBehavior flames) => flames.IntroWaiting;

    /// <summary>
    /// One of Phantoon's intro flames (#1760): its spawner starts its animation, and a shot one burns as the destroyable
    /// flame idles, the animation its definition names.
    /// </summary>
    internal PhantoonFlameBehavior StartingFlame => field ??= new PhantoonFlameBehavior(_audio, _deathsAndDrops,
        StartsIntroWaiting, PhantoonFlameSegment.Burning);

    /// <summary>
    /// A Phantoon flame: every flame's motion, from the intro's wait and orbit to the rage and spiral, and the flame
    /// animations' cues. A circling flame holds the encounter that spawned it (#2252), whose body it circles.
    /// </summary>
    /// <param name="audio">The enemy sound queue the rain flames' sounds join.</param>
    /// <param name="deathsAndDrops">The drops a shot destroyable flame leaves.</param>
    /// <param name="initial">Picks the behaviour's motion the flame starts with.</param>
    /// <param name="shot">Where the animation a destroying shot starts begins.</param>
    internal sealed partial class PhantoonFlameBehavior(
        EnemyAudioQueue audio,
        EnemyDeathsAndDrops deathsAndDrops,
        Func<PhantoonFlameBehavior, EnemyProjectileOwnedMotion> initial,
        PhantoonFlameSegment shot)
        : EnemyProjectileBehavior, IEnemyProjectileCues<PhantoonFlameCue>
    {
        /// <summary>Each orbiting flame's Phantoon, angle, radius and turn, given as he sets it orbiting.</summary>
        internal EnemyProjectileStates<PhantoonOrbitingFlameState> Orbits { get; } = new();

        /// <summary>How long each rain flame hangs before it falls, given as he drops it.</summary>
        internal EnemyProjectileStates<PhantoonRainFlameState> Rains { get; } = new();

        /// <summary>Each casual flame's landing pause and bounces, given as it lands.</summary>
        internal EnemyProjectileStates<PhantoonCasualFlameState> Casuals { get; } = new();

        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() => new PhantoonFlameAnimation(this, shot);

        /// <inheritdoc/>
        internal override EnemyProjectileMotion? InitialMotion(RoomEnemyProjectileKind kind) => initial(this);

        /// <summary>An intro flame waiting at its place around Phantoon until the intro flames begin to orbit.</summary>
        internal EnemyProjectileOwnedMotion IntroWaiting => field ??= new(nameof(IntroWaiting), MoveIntroWaiting);

        /// <summary>An intro flame orbiting Phantoon's body as its orbit closes.</summary>
        internal EnemyProjectileOwnedMotion IntroOrbiting => field ??= new(nameof(IntroOrbiting), MoveIntroOrbiting);

        /// <summary>A casual flame falling to the floor.</summary>
        internal EnemyProjectileOwnedMotion CasualFalling => field ??= new(nameof(CasualFalling), MoveCasualFalling);

        /// <summary>A casual flame pausing on the floor where it landed.</summary>
        internal EnemyProjectileOwnedMotion CasualLanding => field ??= new(nameof(CasualLanding), MoveCasualLanding);

        /// <summary>A casual flame bouncing across the floor after its pause.</summary>
        internal EnemyProjectileOwnedMotion CasualBouncing => field ??= new(nameof(CasualBouncing), MoveCasualBouncing);

        /// <summary>A rage flame spiralling out from Phantoon's body.</summary>
        internal EnemyProjectileOwnedMotion RageOrbiting => field ??= new(nameof(RageOrbiting), MoveRageOrbiting);

        /// <summary>A rain flame hanging, then falling to the floor.</summary>
        internal EnemyProjectileOwnedMotion RainFalling => field ??= new(nameof(RainFalling), MoveRainFalling);

        /// <summary>A spiral flame turning out from Phantoon's body.</summary>
        internal EnemyProjectileOwnedMotion SpiralOrbiting => field ??= new(nameof(SpiralOrbiting), MoveSpiralOrbiting);

        /// <summary>Moves an intro flame waiting (<see cref="IntroWaiting"/>).</summary>
        private void MoveIntroWaiting(EnemyProjectileMotionContext context) => RunPhantoonStartingFlameWaiting(context.Projectile);

        /// <summary>Moves an intro flame orbiting (<see cref="IntroOrbiting"/>).</summary>
        private void MoveIntroOrbiting(EnemyProjectileMotionContext context) =>
            RunPhantoonStartingFlameOrbit(context.Projectile, context.FrameCounter);

        /// <summary>Moves a casual flame falling (<see cref="CasualFalling"/>).</summary>
        private void MoveCasualFalling(EnemyProjectileMotionContext context) =>
            RunPhantoonCasualFlameFalling(this, context.Projectile, context.Level);

        /// <summary>Moves a casual flame pausing where it landed (<see cref="CasualLanding"/>).</summary>
        private void MoveCasualLanding(EnemyProjectileMotionContext context) =>
            RunPhantoonCasualFlameLandingPause(this, context.Projectile, context.FrameCounter);

        /// <summary>Moves a casual flame bouncing (<see cref="CasualBouncing"/>).</summary>
        private void MoveCasualBouncing(EnemyProjectileMotionContext context) =>
            RunPhantoonCasualFlameBouncing(this, context.Projectile, context.Level);

        /// <summary>Moves a rage flame spiralling out (<see cref="RageOrbiting"/>).</summary>
        private void MoveRageOrbiting(EnemyProjectileMotionContext context) => RunPhantoonEnragedFlame(context.Projectile);

        /// <summary>Moves a rain flame (<see cref="RainFalling"/>).</summary>
        private void MoveRainFalling(EnemyProjectileMotionContext context) => RunPhantoonRainFlame(context.Projectile, context.Level);

        /// <summary>Moves a spiral flame (<see cref="SpiralOrbiting"/>).</summary>
        private void MoveSpiralOrbiting(EnemyProjectileMotionContext context) => RunPhantoonSpiralFlame(context.Projectile);

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
        private void RunPhantoonStartingFlameWaiting(EnemyProjectile flame)
        {
            PhantoonOrbitingFlameState orbit = Orbits.Of(flame);
            if (orbit.Phantoon.Phase != PhantoonPhase.WaitForStartingFlamesToDisappear)
                return;
            flame.StartMotion(IntroOrbiting);
            orbit.FullRadiusHold.LastFor(PhantoonFlameMotionDefinitions.IntroCircleHoldUpdates);
            orbit.Radius = PhantoonFlameMotionDefinitions.IntroCircleRadius;
        }

        /// <summary>Advances an intro flame around Phantoon's body; <paramref name="frameCounter"/> times its orbit's closing.</summary>
        private void RunPhantoonStartingFlameOrbit(EnemyProjectile flame, int frameCounter)
        {
            PhantoonOrbitingFlameState orbit = Orbits.Of(flame);
            PhantoonEnemyState state = orbit.Phantoon;
            // Once its hold at its full radius has run out, the orbit closes at its cadence's place in the game's frame
            // counter.
            if (orbit.FullRadiusHold.StepRunsOut() && PhantoonCadenceDefinitions.OrbitCloses.IsDue(frameCounter))
            {
                orbit.Radius--;
                if (orbit.Radius == 0)
                {
                    flame.Position = flame.Position.WithWhole(state.Body.Position.Whole + PhantoonArenaDefinitions.FlameCircleCentre);
                    flame.StartAnimation(new PhantoonFlameAnimation(this, PhantoonFlameSegment.Delete));
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
            PhantoonOrbitingFlameState orbit = Orbits.Of(flame);
            orbit.Radius += PhantoonFlameMotionDefinitions.RageRadiusStep;
            orbit.Angle += orbit.TurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
                orbit.Angle,
                orbit.Radius);
            DeletePhantoonFlameOutsideRoom(this, flame);
        }

        /// <summary>Updates a delayed rain flame as it enters and falls through the room.</summary>
        private void RunPhantoonRainFlame(
            EnemyProjectile flame,
            RoomLevelData level)
        {
            PhantoonRainFlameState rain = Rains.Of(flame);
            if (!rain.Falls)
            {
                if (rain.HangWait.StepKeepsRunning())
                    return;
                // The flame queues its own falling sound as its hang runs out, and falls in that same update.
                rain.Falls = true;
                audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
            }

            flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
            if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
                return;
            flame.StartAnimation(new PhantoonFlameAnimation(this, PhantoonFlameSegment.RainImpact));
            flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
            flame.HoldStill();
            audio.QueueEnemySound(PhantoonFlameMotionDefinitions.RainFallSound, PhantoonFlameMotionDefinitions.RainSoundPriority);
        }

        /// <summary>Moves a spiral flame around Phantoon's body.</summary>
        private void RunPhantoonSpiralFlame(EnemyProjectile flame)
        {
            PhantoonOrbitingFlameState orbit = Orbits.Of(flame);
            orbit.Radius += PhantoonFlameMotionDefinitions.SpiralRadiusStep;
            orbit.Angle += PhantoonFlameMotionDefinitions.SpiralTurnPerUpdate;
            PositionPhantoonFlameAroundBody(
                flame,
                orbit.Phantoon.Body,
                orbit.Angle,
                orbit.Radius);
            DeletePhantoonFlameOutsideRoom(this, flame);
        }

        /// <summary>
        /// Runs the flame's drop step: the enemy drop spawner places a pickup at the flame's exact position from the shot
        /// flame's own drop chances (<see cref="PhantoonFlameSpawnDefinitions.ShotFlameDropChances"/>).
        /// </summary>
        private void RequestPhantoonFlameDrop(EnemyProjectile flame, SamusState samus)
        {
            deathsAndDrops.SpawnEnemyDropFromChanceTable(flame.Position.Whole, PhantoonFlameSpawnDefinitions.ShotFlameDropChances, samus);
        }
    }

    /// <summary>
    /// Tests Samus's beams and missiles against the body of <paramref name="state"/>'s Phantoon, which his body's entity
    /// resolves, and runs the reaction of the hitbox the first one meets. One projectile resolves per pass.
    /// </summary>
    public void ResolvePhantoonProjectileHits(
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
            return;
        }

        foreach (SamusProjectile projectile in projectiles.Shots)
        {
            if (projectile.Occupant is not { HasEnemyCollisionPayload: true } held)
                continue;

            SamusProjectileType projectileType = held.Type;
            int projectileDamage = held.Damage;
            SamusProjectileFamily family = held.Type.Family;
            // Bombs, Power Bombs and the explosions beams and missiles leave behind never hit Phantoon.
            if (family.IsBombOrExplosion)
                continue;

            if (EnemyCollision.FindFrameHitboxContact(
                    body,
                    projectile.Position.Whole,
                    projectile.Radius,
                    contactKind: HitboxContactKind.Shot) is not EnemyHitboxContact.StruckHitbox { Reaction: var hitboxShot })
            {
                continue;
            }

            // A Super Missile that meets one of his hitboxes shakes the room before his reaction, so even a hit that
            // does nothing else shakes it, and the shot is still marked as having struck.
            if (family == SamusProjectileFamily.SuperMissile)
            {
                _earthquake.Start(SuperMissileDefinitions.EnemyHitEarthquake, SuperMissileDefinitions.EarthquakeDuration);
            }

            if (hitboxShot == EnemyReaction.Ignore || state.Phase.Hit == PhantoonHitResponse.Passes)
            {
                projectiles.EnemyImpacts.TouchEnemyWithoutImpact(
                    held,
                    body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                        !held.Type.HasAnyBeam(SamusBeamFlags.Plasma));
                return;
            }

            if (hitboxShot != PhantoonReactions.Shot)
            {
                throw new InvalidDataException(
                    $"Phantoon has no response to shot reaction {hitboxShot}.");
            }

            // The shot is marked as having struck before Phantoon's reaction, whose damage does not turn it into an
            // explosion: the shot ends on its own next update, while a Plasma shot that passes through enemies keeps
            // its type, direction and animation.
            projectiles.EnemyImpacts.TouchEnemyWithoutImpact(held,
                body.Contact.HasAny(EnemyContact.BlocksPlasmaBeam) ||
                !projectileType.HasAnyBeam(SamusBeamFlags.Plasma));

            // Phantoon's shot reaction runs common normal-shot damage (#2255), including its charged-beam override, a
            // zero-damage shot's dud, and no death of its own; his own stage follows it. A special attack's particles
            // count as a charged beam, so the Wave Beam's shield attack damages him.
            int appliedDamage = _collision.ApplyNormalShotDamage(state.BodyOccupant, held, projectileType, projectileDamage, _room.Area).HealthLost;
            ResolvePhantoonShotReaction(
                body,
                state,
                projectileType,
                appliedDamage);
            return;
        }
    }

    /// <summary>Phantoon's own reaction to a damaging shot, after the common damage.</summary>
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

        if (!body.IsHurt)
            return;

        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.ShotCry);
        SamusProjectileFamily family = projectileType.Family;
        PhantoonPhase phase = state.Phase;
        bool vulnerableWindow = phase.Hit == PhantoonHitResponse.OpensVulnerableWindow;
        bool swooping = phase.Hit == PhantoonHitResponse.Swooping;

        if (vulnerableWindow || swooping)
        {
            // A single Super Missile skips ordinary round accumulation
            // and enters the rage sequence immediately.
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
                _room.Random.Advance();
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
        state.ShowBody(PhantoonBodyHitArea.SolidifyingCentre);
        state.EyePart.StartAnimation(PhantoonPartSegment.EyeClose);
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.BodyFade.Resume();
        state.Round.RearmHurtFlash();
    }

    /// <summary>Switches from combat to the fatal swoop and death phases.</summary>
    private static void BeginPhantoonDeathSequence(
        PhantoonEnemyState state)
    {
        state.Phase = state.Phase == PhantoonPhase.Swooping || state.Phase == PhantoonPhase.FadeOutWhileSwooping
                ? PhantoonPhase.FinishFatalSwoop
                : PhantoonPhase.DyingFadeInOut;
        state.FigureEight.Direction = PhantoonPathDirection.Forwards;
        state.BodyFade.Resume();
        state.BodyWave.InitializeDeathScroll();
        state.Translucent = true;
    }

    /// <summary>Moves a casual flame downward before floor contact.</summary>
    /// <param name="owner">The flame's behaviour, which owns its motions and carries out its animations' events.</param>
    /// <param name="flame">The falling flame.</param>
    /// <param name="level">The room's blocks the flame falls onto.</param>
    internal static void RunPhantoonCasualFlameFalling(
        PhantoonFlameBehavior owner,
        EnemyProjectile flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (!EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
            return;

        flame.CanDamageSamus = true;
        flame.BlocksSamusProjectiles = true;
        flame.StartMotion(owner.CasualLanding);
        flame.StartAnimation(new PhantoonFlameAnimation(owner, PhantoonFlameSegment.CasualHitGround));
        owner.Casuals.Start(flame, new PhantoonCasualFlameState());
        flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole + PhantoonArenaDefinitions.FlameLandingDrop);
    }

    /// <summary>Holds the flame briefly at the floor after impact.</summary>
    /// <param name="owner">The flame's behaviour, which owns its motions and carries out its animations' events.</param>
    /// <param name="flame">The landed flame.</param>
    /// <param name="frameCounter">The game's frame counter, whose bit chooses the rising flame's drift.</param>
    internal static void RunPhantoonCasualFlameLandingPause(
        PhantoonFlameBehavior owner,
        EnemyProjectile flame,
        int frameCounter)
    {
        PhantoonCasualFlameState casual = owner.Casuals.Of(flame);
        if (!casual.LandingPause.StepRunsOut())
            return;

        flame.StartMotion(owner.CasualBouncing);
        flame.StartAnimation(new PhantoonFlameAnimation(owner, PhantoonFlameSegment.Burning));
        flame.Position = flame.Position.WithWholeY(flame.Position.Y.Whole - PhantoonArenaDefinitions.FlameLandingDrop);
        flame.Velocity = flame.Velocity.WithY(PhantoonArenaDefinitions.CasualRiseSpeed);
        // The flame drifts left or right by the game's frame counter.
        flame.Velocity = flame.Velocity.WithX(
            PhantoonArenaDefinitions.CasualDriftSpeed.Toward(
                PhantoonCadenceDefinitions.RisingFlameDriftsLeft.IsInSecondHalfAt(frameCounter) ? HorizontalDirection.Left : HorizontalDirection.Right));
    }

    /// <summary>Advances the flame's post-impact bounce motion.</summary>
    /// <param name="owner">The flame's behaviour, which carries out its animations' events.</param>
    /// <param name="flame">The bouncing flame.</param>
    /// <param name="level">The room's blocks the flame bounces on.</param>
    internal static void RunPhantoonCasualFlameBouncing(
        PhantoonFlameBehavior owner,
        EnemyProjectile flame,
        RoomLevelData level)
    {
        flame.Velocity = flame.Velocity.WithY(flame.Velocity.Y + PhantoonFlameMotionDefinitions.FlameGravity);
        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Vertical))
        {
            PhantoonCasualFlameState casual = owner.Casuals.Of(flame);
            casual.Bounces++;
            if (casual.Bounces < PhantoonArenaDefinitions.CasualBounces)
            {
                flame.Velocity = flame.Velocity.WithY(casual.Bounces == 1 ? PhantoonArenaDefinitions.CasualFirstBounceSpeed : PhantoonArenaDefinitions.CasualLaterBounceSpeed);
                return;
            }
            RestPhantoonCasualFlame(owner, flame);
            return;
        }

        if (EnemyProjectileEngine.MoveProjectileAxis(flame, level, axis: Axis.Horizontal))
            RestPhantoonCasualFlame(owner, flame);
    }

    /// <summary>Starts a flame's rest after its bounce ends.</summary>
    private static void RestPhantoonCasualFlame(PhantoonFlameBehavior owner, EnemyProjectile flame)
    {
        flame.StartAnimation(new PhantoonFlameAnimation(owner, PhantoonFlameSegment.CasualResting));
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
    private static void DeletePhantoonFlameOutsideRoom(PhantoonFlameBehavior owner, EnemyProjectile flame)
    {
        if (flame.Position.X.Whole >= PhantoonArenaDefinitions.RoomSize || flame.Position.Y.Whole >= PhantoonArenaDefinitions.RoomSize)
        {
            flame.StartAnimation(new PhantoonFlameAnimation(owner, PhantoonFlameSegment.Delete));
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
                $"Phantoon has no response to touch reaction {context.HitboxTouch}.");
        }
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
    }

    /// <summary>
    /// The reaction of the first hitbox the probe overlaps on <paramref name="enemy"/>'s frame, one of Phantoon's parts,
    /// which its own entity asks for (#2301), his body's picture having <paramref name="bodyHitArea"/> live.
    /// </summary>
    internal static EnemyHitboxContact FindPhantoonHitboxReaction(RoomEnemyPlace enemy, PhantoonBodyHitArea bodyHitArea, EnemyHitboxProbe probe)
    {
        // His pictures' hit areas are his collision definitions', never his editable pictures'; the invulnerable body's
        // centre point is a compatibility rule (PhantoonSolidifyingCentre).
        foreach (EnemyCollisionComponent component in
                 PhantoonCollision.ComponentsAt(enemy.Frame, bodyHitArea))
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
                return EnemyHitboxContact.Struck(probe.Contact == HitboxContactKind.Shot ? hitbox.Shot : hitbox.Touch);
            }
        }
        return EnemyHitboxContact.Missed;
    }
}
