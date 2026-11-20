using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's lints, nails and spat rocks.</summary>
[RoomLifetime]
internal sealed class KraidProjectiles
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves nail and foot movement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidProjectiles(EnemyRoomBinding room, EnemyTerrain terrain, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, KraidEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Initializes one lint part.</summary>
    /// <param name="lint">The part's place.</param>
    /// <param name="state">The encounter his body began, which the part joins.</param>
    /// <param name="expected">Which of his lints it is.</param>
    internal KraidEnemyState InitializeKraidLint(RoomEnemyPlace lint, KraidEnemyState state, KraidLintPart expected)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(lint);
            return state;
        }
        // The lint joins the encounter its body began, which it holds from here (#2218).
        KraidLintState part = state.Lint(expected);
        lint.Shading = state.Parts.Body.Shading;
        part.ShowPose(KraidPartSegment.LintInitial);
        part.Phase = KraidLintPhase.Inactive;
        part.LintXOffset = KraidArenaDefinitions.LintXOffsets[expected];
        return state;
    }

    /// <summary>
    /// Sets a fingernail up when the room spawns it. Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once his body's health reads below one.
    /// </summary>
    /// <param name="nail">The part's place.</param>
    /// <param name="state">The encounter his body began, which the part joins.</param>
    /// <param name="expected">Which of his nails it is.</param>
    internal static KraidEnemyState InitializeKraidNail(RoomEnemyPlace nail, KraidEnemyState state, KraidNailPart expected)
    {
        // The fingernail joins the encounter its body began, which it holds from here (#2218).
        KraidNailState nailPart = state.Nail(expected);
        nail.Shading = state.Parts.Body.Shading;
        // A fingernail is set up at rest, so its sibling's launch reads it as stopped, as it reads a killed one.
        nailPart.NailVelocity = FixedVector<RoomPixels>.Zero;
        // A fingernail set up, at the room's first setup or a respawn, has taken no alternate spawn, as a killed one has not.
        nailPart.AlternateSpawn = false;
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
        // The nail shows its spin's first pose, and the spin waits until its flight starts it from its beginning.
        _ = nailPart.ShowFirstPose(KraidPartSegment.NailLoop);
        nailPart.Phase = KraidNailPhase.WaitBeforeLaunch;
        nailPart.PhaseWait.LastFor(KraidArenaDefinitions.NailLaunchDelayUpdates);
        return state;
    }

    /// <summary>The rocks Kraid's rise throws up (#1760): they fly from their spawn, and their spawner starts their animations.</summary>
    internal static RockBehavior RisingRocks { get; } = new(KraidRockMotion.Flying);

    /// <summary>The ceiling rocks Kraid's roar shakes down (#1760): they fall from their spawn, and their spawner starts their animations.</summary>
    internal static RockBehavior CeilingRocks { get; } = new(KraidRockMotion.CeilingRockFalling);

    /// <summary>
    /// A rock Kraid spits (#1760): it flies from its spawn, its spawner starts its animation, and a shot one runs its typed
    /// burst, the animation its definition names.
    /// </summary>
    internal static RockBehavior SpitRock { get; } = new(KraidRockMotion.Flying, KraidRockSegment.SpitRockShot);

    /// <summary>
    /// One of Kraid's rocks: a rock's flight with room collision, drag and gravity, a ceiling rock's fall, and the rock
    /// animations' cues.
    /// </summary>
    /// <param name="initial">How the rock moves from its spawn.</param>
    /// <param name="shotBurst">The burst a shot rock runs, or null for the shared delete animation.</param>
    internal sealed class RockBehavior(KraidRockMotion initial, KraidRockSegment? shotBurst = null)
        : EnemyProjectileMotionBehavior<KraidRockMotion>(initial), IEnemyProjectileCues<KraidRockCue>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() =>
            shotBurst is { } burst ? new KraidRockAnimation(burst, this) : base.ShotAnimation();

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, KraidRockMotion motion)
        {
            switch (motion)
            {
                case KraidRockMotion.Flying:
                    FlyKraidRock(context.Projectile, context.Level);
                    break;
                case KraidRockMotion.CeilingRockFalling:
                    DropKraidCeilingRock(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KraidRockMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a rock animation's cue: stopping its flight from the next pass, or deletion.</summary>
        public void RunCue(EnemyProjectileCueContext context, KraidRockCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case KraidRockCue.StopFlyingFromNextPass:
                    projectile.HoldStill();
                    return;
                case KraidRockCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KraidRockCue)} {cue}.");
            }
        }
    }

    /// <summary>Checks lint contact against Samus without altering the body phase.</summary>
    private void ResolveKraidLintContact(RoomEnemyPlace lint, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        if (lint.Contact.HasAny(EnemyContact.IgnoresSamus) || !samus.InvincibilityTimer.IsZero)
            return;
        int edge = lint.Position.X.Whole + KraidContactDefinitions.LintLeadingEdge;
        if (samus.Position.X.Whole + samus.Kinematics.Radius.X < edge ||
            samus.Position.X.Whole - samus.Kinematics.Radius.X >= edge ||
            samus.Position.Y.Whole + samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole < KraidContactDefinitions.LintTop ||
            samus.Position.Y.Whole - samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole >= KraidContactDefinitions.LintBottom)
            return;
        // The lint pushes Samus left until her body clears its edge by its push distance and one pixel more.
        int push = -(samus.Kinematics.Radius.X + KraidContactDefinitions.LintPush + KraidContactDefinitions.LintPushClearancePixels) +
            samus.Kinematics.ExtraDisplacement.X.WholePixels;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(
            Math.Min(push, KraidContactDefinitions.LintPush));
        _collision.ResolveOrdinaryTouch(lint, samus, pseudoScrew);
        lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs a nail's active flight or rebound phase.</summary>
    internal void RunKraidNailMain(RoomEnemyPlace nail, KraidNailState part, RoomLevelData level)
    {
        if (part.Kraid.Parts.Body.Health == 0)
        {
            nail.Presence = nail.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        switch (part.Phase)
        {
            case KraidNailPhase.WaitBeforeLaunch:
                if (part.PhaseWait.StepRunsOut())
                    part.Phase = KraidNailPhase.Initialize;
                return;

            case KraidNailPhase.Initialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidNailPhase.WaitForLint:
                if (part.Kraid.Parts.TopLint.Position.X.Whole >= KraidArenaDefinitions.SecondScreenX)
                {
                    part.Phase = KraidNailPhase.Fire;
                    nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
                    nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
                }
                return;

            case KraidNailPhase.Fire:
                TickKraidNailFlight(nail, part, level);
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(KraidNailPhase)} {part.Phase}.");
        }
    }

    /// <summary>Seeds a released nail's velocity, animation, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemyPlace nail, KraidNailState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        // The sibling is read in its place whatever the place holds (RoomEnemyPlace.FoundEnemy): a sibling killed since it
        // was set up has a cleared place, which reads as a nail at rest that took no alternate spawn, so the launch goes up.
        KraidNailState? siblingPart = KraidEncounterAccess.FoundKraidPart<KraidNailState>(part.SiblingPlace);
        int random = _room.Random.Read();
        part.NailVelocity = KraidNailLaunch.FromSiblingVelocity((siblingPart?.NailVelocity ?? FixedVector<RoomPixels>.Zero).Y);
        nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
        nail.AnimationClock.StepOnNextUpdate();
        part.SetAnimation(KraidPartSegment.NailLoop);
        part.Phase = KraidNailPhase.Fire;

        if (!KraidRandomDefinitions.NailWaitsForLint.IsTakenBy(random) || siblingPart is { AlternateSpawn: true })
        {
            part.AlternateSpawn = false;
            RoomEnemyPlace body = part.Kraid.Parts.Body;
            nail.Position = nail.Position.WithWhole(new(RoomGridConversions.BlockStart(
                body.Position.X.Whole - body.Radius.X - nail.Radius.X), part.Kraid.Parts.Arm.Position.Y.Whole + KraidArenaDefinitions.NailLaunchBelowArm));
            return;
        }

        part.AlternateSpawn = true;
        nail.Position = nail.Position.WithWhole(KraidArenaDefinitions.AlternateNailStart);
        part.NailVelocity = KraidNailLaunchDefinitions.Parked;
        part.Phase = KraidNailPhase.WaitForLint;
        nail.Contact = nail.Contact.With(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Advances nail position and terrain response for one enemy update.</summary>
    internal void TickKraidNailFlight(RoomEnemyPlace nail, KraidNailState part, RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, part.NailVelocity.X))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));
        else if (KraidBodyContourReflectsNail(nail, part))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));

        if (_terrain.MoveEnemyVertically(level, nail, part.NailVelocity.Y))
            part.NailVelocity = part.NailVelocity.WithY(SignedSixteenSixteen.Zero - part.NailVelocity.Y);
    }

    /// <summary>Checks whether a nail crosses the body contour that reverses its flight.</summary>
    internal bool KraidBodyContourReflectsNail(RoomEnemyPlace nail, KraidNailState part)
    {
        RoomEnemyPlace body = part.Kraid.Parts.Body;
        int relativeY = nail.Position.Y.Whole - body.Position.Y.Whole;
        // A nail above every ledge is pushed back at the contour entry the walk reads after them.
        int leftOffset = KraidNailContour.LedgeLeftOffset(relativeY) ??
            _room.Session.BossOverreads.KraidNailPastLedges.LeftOffset(relativeY);
        int contourX = body.Position.X.Whole + leftOffset;
        return nail.Position.X.Whole + nail.Radius.X >= contourX &&
            part.NailVelocity.X.WholePixels >= 0;
    }

    /// <summary>
    /// A wall or Kraid's contour reverses a flying nail's horizontal speed. A flying nail moves in whole pixels: its launch
    /// and its wait for the lint set speeds with no fraction (<see cref="KraidNailLaunchDefinitions"/>), and a reversal
    /// keeps a speed with no fraction one, so the reversal is the plain opposite speed (#1620).
    /// </summary>
    private static SignedSixteenSixteen ReflectKraidNailHorizontalVelocity(SignedSixteenSixteen velocity) => -velocity;

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemyPlace lint, KraidLintState part, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        ResolveKraidLintContact(lint, samus, pseudoScrew);
        switch (part.Phase)
        {
            case KraidLintPhase.Inactive:
                return;
            case KraidLintPhase.AlignToKraidBeforeProducing:
                AlignKraidLint(lint, part);
                return;
            case KraidLintPhase.Produce:
                ProduceKraidLint(lint, part);
                return;
            case KraidLintPhase.Charge:
                ChargeKraidLint(lint, part);
                return;
            case KraidLintPhase.Fire:
                FireKraidLint(lint, part, samus);
                return;
            default:
                throw new InvalidDataException($"Undefined {nameof(KraidLintPhase)} {part.Phase} for Kraid lint place {lint.Ordinal}.");
        }
    }

    /// <summary>Places a lint at its body-relative offset while its phase wait runs, and produces it on the wait's last update.</summary>
    internal static void AlignKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Position = lint.Position.WithWholeX(part.Kraid.Parts.Body.Position.X.Whole - lint.Radius.X);
        if (part.PhaseWait.StepRunsOut())
            part.Phase = KraidLintPhase.Produce;
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private static void ProduceKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Contact = lint.Contact.Without(EnemyContact.IgnoresSamus);
        lint.Presence = lint.Presence.Without(EnemyPresence.Hidden);
        lint.Position = lint.Position.WithWholeX(
            part.LintXOffset + part.Kraid.Parts.Body.Position.X.Whole - part.LintTravel);
        part.LintTravel++;
        if (part.LintTravel >= KraidArenaDefinitions.LintEmergePixels)
        {
            part.Phase = KraidLintPhase.Charge;
            part.PhaseWait.LastFor(KraidArenaDefinitions.LintChargeUpdates);
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Shading = KraidCadenceDefinitions.LintChargeShowsOwnColours.IsSetIn(part.PhaseWait.UpdatesLeft)
            ? part.Kraid.Colours.Parts
            : EnemyShades.HurtFlashLook;
        lint.Position = lint.Position.WithWholeX(
            part.LintXOffset + part.Kraid.Parts.Body.Position.X.Whole - part.LintTravel);
        if (part.PhaseWait.StepRunsOut())
        {
            part.Phase = KraidLintPhase.Fire;
            _access.RequestKraidSound(KraidSoundDefinitions.LintFire);
        }
    }

    /// <summary>Launches a lint projectile toward Samus from the part's position.</summary>
    private static void FireKraidLint(RoomEnemyPlace lint, KraidLintState part, SamusState samus)
    {
        lint.Position = lint.Position.AdvanceX(KraidPlatformMovement.FiringDisplacement);
        if (lint.Position.X.Whole < KraidArenaDefinitions.LintIntangibleX)
            lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
        if (lint.Position.X.Whole < KraidArenaDefinitions.LintReturnX)
        {
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
            part.Phase = KraidLintPhase.AlignToKraidBeforeProducing;
            part.PhaseWait.LastFor(KraidArenaDefinitions.LintReloadUpdates);
            part.LintTravel = 0;
        }

        // Checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement step, rather than teleporting
        // her here. The clamp changes only the whole pixels; the fraction of a pixel keeps what the subtraction left.
        if (EnemyCollision.IsSamusStandingOn(lint, samus))
        {
            SignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X + KraidPlatformMovement.FiringDisplacement;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                carry.WithWholePixelsAtLeast(KraidPlatformMovement.MinimumCarryWholePixels));
        }
    }

    /// <summary>Updates a spit rock's flight and removes it at its boundary.</summary>
    internal static void FlyKraidRock(
        EnemyProjectile rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Vertical))
        {
            rock.Delete();
            return;
        }

        SignedEightEight horizontal = rock.Velocity.X + KraidRockLaunchDefinitions.RockDrift;
        if (horizontal >= KraidRockLaunchDefinitions.RockDriftRestart)
            horizontal = -KraidRockLaunchDefinitions.RockDriftRestart;
        rock.Velocity = rock.Velocity.WithX(horizontal);
        rock.Velocity = rock.Velocity.WithY(rock.Velocity.Y + KraidRockLaunchDefinitions.RockGravity);
    }

    /// <summary>Updates a falling ceiling rock and its contact with room terrain.</summary>
    internal static void DropKraidCeilingRock(
        EnemyProjectile rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Vertical))
        {
            rock.Delete();
            return;
        }
        rock.Velocity = rock.Velocity.WithY(rock.Velocity.Y + KraidRockLaunchDefinitions.CeilingRockGravity);
    }

    /// <summary>Runs a nail's touch: the shared contact damage, then the nail's death.</summary>
    internal void ReactToKraidNailTouch(EnemyTouchContext context)
    {
        _collision.ResolveOrdinaryTouch(context);
        // A fingernail the touch left in its place dies in the explosion the health the touch left names
        // (LeftoverNumberDeathExplosion).
        if (!context.Place.IsFree)
            _deathsAndDrops.StartGenericEnemyDeath(context.Place,
                EnemyDeathExplosionDefinitions.LeftoverNumbered.Named(context.Place.Health), context.Samus);
    }

}
