using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's lints, nails and spat rocks.</summary>
internal sealed partial class KraidProjectiles
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
    /// <returns>The encounter the lint joins, which it holds from its setup.</returns>
    internal KraidEnemyState InitializeKraidLint(RoomEnemyPlace lint, KraidLintPart expected, KraidEnemyState state)
    {
        // The lint joins the encounter its body began, which it holds from here (#2218).
        Ensure.NotNull(state);
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(lint);
            return state;
        }
        KraidLintState part = state.Lint(expected);
        lint.Shading = state.Parts.Body.Shading;
        part.ShowPose(KraidPartSegment.LintInitial);
        part.Phase = KraidLintPhase.Inactive;
        part.Emergence = KraidArenaDefinitions.LintEmergences[expected];
        return state;
    }

    /// <summary>
    /// Sets a fingernail up when the room spawns it. Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once his body's health reads below one.
    /// </summary>
    /// <returns>The encounter the fingernail joins, which it holds from its setup.</returns>
    internal static KraidEnemyState InitializeKraidNail(RoomEnemyPlace nail, KraidNailPart expected, KraidEnemyState state)
    {
        // The fingernail joins the encounter its body began, which it holds from here (#2218).
        Ensure.NotNull(state);
        KraidNailState nailPart = state.Nail(expected);
        nail.Shading = state.Parts.Body.Shading;
        // A fingernail is set up at rest, so its sibling's launch reads it as stopped, as it reads a killed one.
        nailPart.NailVelocity = FixedVector<RoomPixels>.Zero;
        // A fingernail set up, at the room's first setup or a respawn, has taken no alternate spawn, as a killed one has not.
        nailPart.LaunchSide = KraidNailLaunchSide.BodySide;
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
        // The nail shows its spin's first pose, and the spin waits until its flight starts it from its beginning.
        _ = nailPart.ShowFirstPose(KraidPartSegment.NailLoop);
        nailPart.Phase = KraidNailPhase.WaitBeforeLaunch;
        nailPart.PhaseWait.LastFor(KraidArenaDefinitions.NailLaunchDelayUpdates);
        return state;
    }

    /// <summary>The rocks Kraid's rise throws up and the ceiling rocks his roar shakes down (#1760): their spawners start their animations.</summary>
    internal RockBehavior Rocks { get; } = new();

    /// <summary>
    /// A rock Kraid spits (#1760): its spawner starts its animation, and a shot one runs its typed burst, the animation its
    /// definition names.
    /// </summary>
    internal RockBehavior SpitRock { get; } = new(KraidRockSegment.SpitRockShot);

    /// <summary>
    /// One of Kraid's rocks: a rock's flight with room collision, drag and gravity, a ceiling rock's fall, and the rock
    /// animations' cues.
    /// </summary>
    /// <param name="shot">Where the animation a destroying shot starts begins, or null for the shared delete animation.</param>
    internal sealed partial class RockBehavior(KraidRockSegment? shot = null)
        : EnemyProjectileMotionBehavior<KraidRockMotion>(StartingMotions), IEnemyProjectileCues<KraidRockCue>
    {
        /// <inheritdoc/>
        internal override EnemyProjectileAnimation ShotAnimation() =>
            shot is { } segment ? new KraidRockAnimation(this, segment) : base.ShotAnimation();

        /// <summary>A spat or rising rock flies, and a ceiling rock falls, from its spawn.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, KraidRockMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.KraidSpitRock] = KraidRockMotion.Flying,
            [RoomEnemyProjectileKind.KraidRisingRock] = KraidRockMotion.Flying,
            [RoomEnemyProjectileKind.KraidCeilingRock] = KraidRockMotion.CeilingRockFalling,
        };

        /// <summary>What <c>Move</c> does for each <see cref="KraidRockMotion"/> member, each member's own arm.</summary>
        private static IdentityTable<KraidRockMotion, Action<EnemyProjectileMotionContext>> MoveForKraidRockMotion => field ??= new(new Dictionary<KraidRockMotion, Action<EnemyProjectileMotionContext>>
        {
            [KraidRockMotion.Flying] = static context => FlyKraidRock(context.Projectile, context.Level),
            [KraidRockMotion.CeilingRockFalling] = static context => DropKraidCeilingRock(context.Projectile, context.Level),
        });

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, KraidRockMotion motion)
        {
            MoveForKraidRockMotion[motion](context);
        }

        /// <summary>What <see cref="RunCue"/> does for each <see cref="KraidRockCue"/> member, each member's own arm.</summary>
        private static IdentityTable<KraidRockCue, Action<EnemyProjectile>> RunCueForKraidRockCue => field ??= new(new Dictionary<KraidRockCue, Action<EnemyProjectile>>
        {
            [KraidRockCue.StopFlyingFromNextPass] = static projectile => projectile.HoldStill(),
        });

        /// <summary>Carries out a rock animation's cue: stopping its flight from the next pass.</summary>
        public void RunCue(EnemyProjectileCueContext context, KraidRockCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            RunCueForKraidRockCue[cue](projectile);
        }
    }

    /// <summary>Checks lint contact against Samus without altering the body phase.</summary>
    private void ResolveKraidLintContact(RoomEnemyOccupant lintOccupant, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace lint = lintOccupant.Place;
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
        _collision.ResolveOrdinaryTouch(lintOccupant, samus, pseudoScrew);
        lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>What <see cref="RunKraidNailMain"/> does for each <see cref="KraidNailPhase"/> member, each member's own arm.</summary>
    private IdentityTable<KraidNailPhase, Action<RoomEnemyPlace, KraidNailState, RoomLevelData>> RunKraidNailMainForKraidNailPhase => field ??= new(new Dictionary<KraidNailPhase, Action<RoomEnemyPlace, KraidNailState, RoomLevelData>>
    {
        [KraidNailPhase.WaitBeforeLaunch] = (nail, part, level) =>
        {
            if (part.PhaseWait.StepRunsOut())
                part.Phase = KraidNailPhase.Initialize;
        },
        [KraidNailPhase.Initialize] = (nail, part, level) => InitializeKraidNailFlight(nail, part),
        [KraidNailPhase.WaitForLint] = (nail, part, level) =>
        {
            if (part.Kraid.Parts.TopLint.Position.X.Whole >= KraidArenaDefinitions.SecondScreenX)
            {
                part.Phase = KraidNailPhase.Fire;
                nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
                nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
            }
        },
        [KraidNailPhase.Fire] = (nail, part, level) => TickKraidNailFlight(nail, part, level),
    });

    /// <summary>Runs a nail's active flight or rebound phase.</summary>
    internal void RunKraidNailMain(RoomEnemyPlace nail, KraidNailState part, RoomLevelData level)
    {
        if (part.Kraid.Parts.Body.Health == 0)
        {
            nail.Presence = nail.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        RunKraidNailMainForKraidNailPhase[part.Phase](nail, part, level);
    }

    /// <summary>Seeds a released nail's velocity, animation, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemyPlace nail, KraidNailState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        // The sibling is read in its place whatever the place holds (RoomEnemyPlace.FoundEntity): a sibling killed since it
        // was set up has a cleared place, which reads as a nail at rest that took no alternate spawn, so the launch goes up.
        KraidNailState? siblingPart = part.SiblingPlace.FoundEntity is KraidNailEntity sibling ? sibling.Part : null;
        int random = _room.Random.Read();
        part.NailVelocity = KraidNailLaunch.FromSiblingVelocity((siblingPart?.NailVelocity ?? FixedVector<RoomPixels>.Zero).Y);
        nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
        nail.AnimationClock.StepOnNextUpdate();
        part.SetAnimation(KraidPartSegment.NailLoop);
        part.Phase = KraidNailPhase.Fire;

        if (!KraidRandomDefinitions.NailWaitsForLint.IsTakenBy(random) || siblingPart is { LaunchSide: KraidNailLaunchSide.FixedLeft })
        {
            part.LaunchSide = KraidNailLaunchSide.BodySide;
            RoomEnemyPlace body = part.Kraid.Parts.Body;
            nail.Position = nail.Position.WithWhole(new(RoomGridConversions.BlockStart(
                body.Position.X.Whole - body.Radius.X - nail.Radius.X), part.Kraid.Parts.Arm.Position.Y.Whole + KraidArenaDefinitions.NailLaunchBelowArm));
            return;
        }

        part.LaunchSide = KraidNailLaunchSide.FixedLeft;
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
        // A nail above every ledge is pushed back where KraidNailPastLedges places it.
        int leftOffset = KraidNailContour.LedgeLeftOffset(relativeY) ??
            _room.Session.BossOverreads.KraidNailPastLedges.LeftOffset;
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

    /// <summary>What <see cref="RunKraidLintMain"/> does for each <see cref="KraidLintPhase"/> member, each member's own arm.</summary>
    private IdentityTable<KraidLintPhase, Action<RoomEnemyPlace, KraidLintState, SamusState>> RunKraidLintMainForKraidLintPhase => field ??= new(new Dictionary<KraidLintPhase, Action<RoomEnemyPlace, KraidLintState, SamusState>>
    {
        [KraidLintPhase.Inactive] = (lint, part, samus) => { },
        [KraidLintPhase.AlignToKraidBeforeProducing] = (lint, part, samus) => AlignKraidLint(lint, part),
        [KraidLintPhase.Produce] = (lint, part, samus) => ProduceKraidLint(lint, part),
        [KraidLintPhase.Charge] = (lint, part, samus) => ChargeKraidLint(lint, part),
        [KraidLintPhase.Fire] = (lint, part, samus) => FireKraidLint(lint, part, samus),
    });

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemyOccupant lintOccupant, KraidLintState part, SamusState samus, PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace lint = lintOccupant.Place;
        ResolveKraidLintContact(lintOccupant, samus, pseudoScrew);
        RunKraidLintMainForKraidLintPhase[part.Phase](lint, part, samus);
    }

    /// <summary>Places a lint at its body-relative offset while its phase wait runs, and produces it on the wait's last update.</summary>
    internal static void AlignKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Position = lint.Position.WithWholeX(part.Kraid.Parts.Body.Position.X.Whole - lint.Radius.X);
        if (part.PhaseWait.StepRunsOut())
            part.Phase = KraidLintPhase.Produce;
    }

    /// <summary>Where a lint stands as it emerges from his belly (#2557): its offset from his body, less how far it has come.</summary>
    private static int EmergedLintX(KraidLintState part) => part.LintXOffset + part.Kraid.Parts.Body.Position.X.Whole - part.LintTravel;

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private static void ProduceKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Contact = lint.Contact.Without(EnemyContact.IgnoresSamus);
        lint.Presence = lint.Presence.Without(EnemyPresence.Hidden);
        lint.Position = lint.Position.WithWholeX(EmergedLintX(part));
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
        lint.Shading = KraidCadenceDefinitions.LintChargeShowsOwnColours.IsInSecondHalfAt(part.PhaseWait.UpdatesLeft)
            ? part.Kraid.Colours.Parts
            : EnemyShades.HurtFlashLook;
        lint.Position = lint.Position.WithWholeX(EmergedLintX(part));
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
            _deathsAndDrops.StartGenericEnemyDeath(context.Occupant,
                EnemyDeathExplosionDefinitions.LeftoverNumbered.Named(context.Place.Health), context.Samus);
    }

}
