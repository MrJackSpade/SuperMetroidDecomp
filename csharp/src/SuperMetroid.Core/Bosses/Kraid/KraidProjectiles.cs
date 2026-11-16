using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
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
    /// <summary>Kraid's eight parts in his room, in the order they run.</summary>
    private readonly KraidParts _parts;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidProjectiles(EnemyRoomBinding room, EnemyTerrain terrain, KraidParts parts, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, KraidEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _parts = parts;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Initializes one lint part.</summary>
    internal void InitializeKraidLint(RoomEnemyPlace lint, KraidLintPart expected)
    {
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(lint);
            return;
        }
        // The lint joins the encounter its body began, which it holds from here (#2218).
        KraidEnemyState state = KraidEntity.EncounterOf(_parts.Body);
        _access.AttachKraidPart(lint, state);
        lint.Shading = _parts.Body.Shading;
        // A lint shows the pose its behaviour sets; its animation never steps, and nothing starts its clock again.
        lint.AnimationClock.Stop();
        _access.SetKraidPartAnimation(state, lint, KraidPartSegment.LintInitial);
        lint.Frame = EnemyFrameId.KraidLintBudding;
        KraidLintState part = _parts.LintStateOf(state, lint);
        part.Phase = KraidLintPhase.Inactive;
        part.LintXOffset = expected == KraidLintPart.Top ? 0 : -16;
    }

    /// <summary>
    /// Sets a fingernail up when the room spawns it. Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once his body's health reads below one.
    /// </summary>
    internal void InitializeKraidNail(RoomEnemyPlace nail)
    {
        // The fingernail joins the encounter its body began, which it holds from here (#2218).
        KraidEnemyState state = KraidEntity.EncounterOf(_parts.Body);
        _access.AttachKraidPart(nail, state);
        nail.Shading = _parts.Body.Shading;
        KraidNailState nailPart = _parts.NailStateOf(state, nail);
        // A fingernail is set up at rest, so its sibling's launch reads it as stopped, as it reads a killed one.
        nailPart.NailVelocity = FixedVector<RoomPixels>.Zero;
        // A fingernail set up, at the room's first setup or a respawn, has taken no alternate spawn, as a killed one has not.
        nailPart.AlternateSpawn = false;
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
        // The nail's spin waits until its flight starts the clock.
        nail.AnimationClock.Stop();
        _access.SetKraidPartAnimation(state, nail, KraidPartSegment.NailLoop);
        nail.Frame = EnemyFrameId.KraidNailTurned0;
        nailPart.Phase = KraidNailPhase.WaitBeforeLaunch;
        nailPart.PhaseTimer = 64;
    }

    /// <summary>The rocks Kraid's rise throws up and the ceiling rocks his roar shakes down (#1760): their spawners start their animations.</summary>
    internal static EnemyProjectileBehavior Rocks { get; } = new RockBehavior();

    /// <summary>
    /// A rock Kraid spits (#1760): its spawner starts its animation, and a shot one runs its typed burst, the animation its
    /// definition names.
    /// </summary>
    internal static EnemyProjectileBehavior SpitRock { get; } = new RockBehavior(() => new KraidRockAnimation(KraidRockSegment.SpitRockShot));

    /// <summary>
    /// One of Kraid's rocks: a rock's flight with room collision, drag and gravity, a ceiling rock's fall, and the rock
    /// animations' cues.
    /// </summary>
    private sealed class RockBehavior(Func<EnemyProjectileAnimation>? shot = null)
        : EnemyProjectileMotionBehavior<KraidRockMotion>(StartingMotions, shot), IEnemyProjectileCues<KraidRockCue>
    {
        /// <summary>A spat or rising rock flies, and a ceiling rock falls, from its spawn.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, KraidRockMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.KraidSpitRock] = KraidRockMotion.Flying,
            [RoomEnemyProjectileKind.KraidRisingRockLeft] = KraidRockMotion.Flying,
            [RoomEnemyProjectileKind.KraidRisingRockRight] = KraidRockMotion.Flying,
            [RoomEnemyProjectileKind.KraidCeilingRock] = KraidRockMotion.CeilingRockFalling,
        };

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
        int edge = CoordinateAxis.Wrap(lint.Position.X.Whole + KraidContactDefinitions.LintLeadingEdge);
        if (CoordinateAxis.AsSigned(samus.Position.X.Whole + samus.Kinematics.Radius.X - edge) < 0 ||
            CoordinateAxis.Difference(samus.Position.X.Whole - samus.Kinematics.Radius.X, edge) >= 0 ||
            CoordinateAxis.AsSigned(samus.Position.Y.Whole + samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintTop) < 0 ||
            CoordinateAxis.Difference(samus.Position.Y.Whole - samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole, KraidContactDefinitions.LintBottom) >= 0)
            return;
        // The lint pushes Samus left until her body clears its edge by its push distance and one pixel more.
        int push = -(samus.Kinematics.Radius.X + KraidContactDefinitions.LintPush + KraidContactDefinitions.LintPushClearancePixels) +
            samus.Kinematics.ExtraDisplacement.X.WholePixels;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(
            Math.Min(push, KraidContactDefinitions.LintPush));
        _collision.ResolveNormalEnemyTouch(lint, samus, pseudoScrew);
        lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Runs a nail's active flight or rebound phase.</summary>
    internal void RunKraidNailMain(RoomEnemyPlace nail, KraidNailState part, RoomLevelData level)
    {
        if (_parts.Body.Health == 0)
        {
            nail.Presence = nail.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        switch (part.Phase)
        {
            case KraidNailPhase.WaitBeforeLaunch:
                if (part.CountPhaseTimerDown())
                    part.Phase = KraidNailPhase.Initialize;
                return;

            case KraidNailPhase.Initialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidNailPhase.WaitForLint:
                if (CoordinateAxis.Difference(_parts.TopLint.Position.X.Whole, 256) >= 0)
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
        // The sibling is read in its place whatever the place holds (RoomEnemyPlace.FoundState): a sibling killed since it
        // was set up has a cleared place, which reads as a nail at rest that took no alternate spawn, so the launch goes up.
        RoomEnemyPlace sibling = ReferenceEquals(nail, _parts.FirstNail) ? _parts.SecondNail : _parts.FirstNail;
        var siblingPart = (KraidNailState?)sibling.FoundState<KraidPartState>();
        int random = _room.Random.Read();
        part.NailVelocity = KraidNailLaunch.FromSiblingVelocity((siblingPart?.NailVelocity ?? FixedVector<RoomPixels>.Zero).Y);
        nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
        nail.AnimationClock.StepOnNextUpdate();
        _access.SetKraidPartAnimation(part.Kraid, nail, KraidPartSegment.NailLoop);
        part.Phase = KraidNailPhase.Fire;

        if (!KraidRandomDefinitions.NailWaitsForLint.IsSetIn(random) || siblingPart is { AlternateSpawn: true })
        {
            part.AlternateSpawn = false;
            RoomEnemyPlace body = _parts.Body;
            nail.Position = nail.Position.WithWhole(new(RoomGridConversions.BlockStart(CoordinateAxis.Wrap(
                body.Position.X.Whole - body.Radius.X - nail.Radius.X)), CoordinateAxis.Wrap(_parts.Arm.Position.Y.Whole + 128)));
            return;
        }

        part.AlternateSpawn = true;
        nail.Position = nail.Position.WithWhole(new(50, 240));
        part.NailVelocity = FixedVector<RoomPixels>.FromWholePixels(new Offset16<RoomPixels>(1, 0));
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
        RoomEnemyPlace body = _parts.Body;
        int relativeY = CoordinateAxis.Wrap(nail.Position.Y.Whole - body.Position.Y.Whole);
        // A nail above every ledge is pushed back at the contour entry the walk reads after them.
        int leftOffset = KraidNailContour.LedgeLeftOffset(relativeY) ??
            _room.Session.BossOverreads.KraidNailPastLedges.LeftOffset(relativeY);
        int contourX = CoordinateAxis.Wrap(body.Position.X.Whole + leftOffset);
        return CoordinateAxis.AsSigned(nail.Position.X.Whole + nail.Radius.X - contourX) >= 0 &&
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

    /// <summary>Places a lint at its body-relative offset while its timer runs, then produces it.</summary>
    internal void AlignKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Position = lint.Position.WithWrappedWholeX(_parts.Body.Position.X.Whole - lint.Radius.X);
        if (part.CountPhaseTimerDown())
            part.Phase = KraidLintPhase.Produce;
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private void ProduceKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Contact = lint.Contact.Without(EnemyContact.IgnoresSamus);
        lint.Presence = lint.Presence.Without(EnemyPresence.Hidden);
        lint.Position = lint.Position.WithWrappedWholeX(
            part.LintXOffset + _parts.Body.Position.X.Whole - part.LintTravel);
        part.LintTravel++;
        if (part.LintTravel >= 32)
        {
            part.Phase = KraidLintPhase.Charge;
            part.PhaseTimer = 30;
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemyPlace lint, KraidLintState part)
    {
        lint.Shading = KraidCadenceDefinitions.LintChargeShowsOwnColours.IsSetIn(part.PhaseTimer)
            ? part.Kraid.Colours.Parts
            : EnemyShades.HurtFlashLook;
        lint.Position = lint.Position.WithWrappedWholeX(
            part.LintXOffset + _parts.Body.Position.X.Whole - part.LintTravel);
        if (--part.PhaseTimer == 0)
        {
            part.Phase = KraidLintPhase.Fire;
            _access.RequestKraidSound(KraidSoundDefinitions.LintFire);
        }
    }

    /// <summary>Launches a lint projectile toward Samus from the part's position.</summary>
    private static void FireKraidLint(RoomEnemyPlace lint, KraidLintState part, SamusState samus)
    {
        AddSignedKraidHorizontalDisplacement(lint, KraidPlatformMovement.FiringDisplacement);
        if (CoordinateAxis.Difference(lint.Position.X.Whole, 56) < 0)
            lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
        if (CoordinateAxis.Difference(lint.Position.X.Whole, 32) < 0)
        {
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
            part.Phase = KraidLintPhase.AlignToKraidBeforeProducing;
            part.PhaseTimer = 300;
            part.LintTravel = 0;
        }

        // Checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement step, rather than teleporting
        // her here. The clamp changes only the whole pixels; the subpixels keep what the subtraction left.
        if (EnemyCollision.IsSamusStandingOn(lint, samus))
        {
            SignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X + KraidPlatformMovement.FiringDisplacement;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                carry.WithWholePixelsAtLeast(KraidPlatformMovement.MinimumCarryWholePixels));
        }
    }

    /// <summary>Adds signed horizontal motion to the body position, wrapping around the coordinate range.</summary>
    private static void AddSignedKraidHorizontalDisplacement(
        RoomEnemyPlace place,
        SignedSixteenSixteen displacement)
    {
        place.Position = place.Position.WithX(place.Position.X.MovedBy(displacement));
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
        _collision.ResolveNormalEnemyTouch(context);
        // A fingernail the touch left in its place dies in the explosion the health the touch left names
        // (LeftoverNumberDeathExplosion).
        if (!context.Place.IsFree)
            _deathsAndDrops.StartGenericEnemyDeath(context.Place,
                LeftoverNumberDeathExplosion.Named(context.Place.Health, EnemyDeathExplosionDefinitions.AuthoredOrder), context.Samus);
    }

}
