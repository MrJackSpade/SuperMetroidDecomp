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
    /// <summary>Kraid's state and part lookups, part animations, function timer, random numbers and the sound he last requested.</summary>
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

    /// <summary>Initializes one lint part and verifies it is in that part's place.</summary>
    private void InitializeKraidLint(RoomEnemySlot lint, KraidLintPart expected)
    {
        KraidEnemyState state = _access.RequireKraidState(lint);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(lint);
            return;
        }
        KraidParts.Require(lint, _parts.Lint(expected), "lint");
        _access.AttachKraidPart(lint, state);
        lint.Shading = _parts.Body.Shading;
        lint.AnimationClock.HoldFor(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartAnimation(lint, KraidPartSegment.LintInitial);
        lint.Frame = EnemyFrameId.KraidLintInitial;
        _parts.LintStateOf(state, lint).Phase = KraidPhase.LintInactive;
        _parts.LintStateOf(state, lint).LintXOffset = expected == KraidLintPart.Top ? 0 : -16;
    }

    /// <summary>
    /// Sets a fingernail up when the room spawns it. Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once his body's health reads below one.
    /// </summary>
    internal void InitializeKraidNail(RoomEnemySlot nail, RoomEnemySlot expected)
    {
        KraidEnemyState state = _access.RequireKraidState(nail);
        KraidParts.Require(nail, expected, "fingernail");
        _access.AttachKraidPart(nail, state);
        nail.Shading = _parts.Body.Shading;
        KraidNailState nailPart = _parts.NailStateOf(state, nail);
        // A fingernail is set up at rest with its horizontal fraction primed: the room's first setup starts it at rest, and
        // a respawn follows the kill that stopped it (KilledFingernailStops).
        nailPart.NailVelocity = FixedVector<RoomPixels>.FromStoredParts(0, 40, 0, 0);
        // A fingernail set up, at the room's first setup or a respawn, has taken no alternate spawn (KilledFingernailStops).
        nailPart.AlternateSpawn = false;
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
        nail.AnimationClock.HoldFor(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartAnimation(nail, KraidPartSegment.NailLoop);
        nail.Frame = EnemyFrameId.KraidNailLoopFirst;
        _parts.NailStateOf(state, nail).PhaseToResume = KraidPhase.FingernailInitialize;
        _parts.NailStateOf(state, nail).Phase = KraidPhase.HandleFunctionTimer;
        _parts.NailStateOf(state, nail).FunctionTimer = 64;
    }

    /// <summary>Registers the rock projectile initializers and per-frame handlers.</summary>
    internal static void AddKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunKraidRockFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.KraidRockFlight);

        table.AddFrameBehavior(context =>
        {
            RunKraidCeilingRockFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.KraidCeilingRockFall);
    }

    /// <summary>Runs KraidLintTop's initialization AI.</summary>
    internal void RunKraidLintTopInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Top);
    }

    /// <summary>Runs KraidLintMiddle's initialization AI.</summary>
    internal void RunKraidLintMiddleInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Middle);
    }

    /// <summary>Runs KraidLintBottom's initialization AI.</summary>
    internal void RunKraidLintBottomInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Bottom);
    }

    /// <summary>Runs KraidNail's initialization AI.</summary>
    internal void RunKraidNailInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, _parts.FirstNail);
    }

    /// <summary>Runs KraidNailBad's initialization AI.</summary>
    internal void RunKraidNailBadInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, _parts.SecondNail);
    }

    /// <summary>Registers the spat rock's typed burst and the rock animations' cues.</summary>
    internal static void AddKraidRockAnimations(EnemyProjectileHandlerTable table)
    {
        table.AddShotAnimation(RoomEnemyProjectileKind.KraidSpitRock,
            () => new KraidRockAnimation(KraidRockSegment.SpitRockShot));
        table.AddCue<KraidRockCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case KraidRockCue.DrawFromSharedSheetFromNextPass:
                    projectile.FrameBehavior = EnemyProjectileFrameBehavior.KraidShotRockRecolor;
                    return;
                case KraidRockCue.Delete:
                    projectile.Delete();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KraidRockCue)} {cue}.");
            }
        });
    }

    /// <summary>Checks lint contact against Samus without altering the body phase.</summary>
    private void ResolveKraidLintContact(RoomEnemySlot lint, SamusState samus)
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
        _collision.ResolveNormalEnemyTouch(lint, samus);
        lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Dispatches a nail's active flight or rebound function.</summary>
    internal void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = _access.RequireKraidState(nail);
        if (_parts.Body.Health == 0)
        {
            nail.Presence = nail.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }

        KraidNailState part = _parts.NailStateOf(state, nail);
        switch (_parts.NailStateOf(state, nail).Phase)
        {
            case KraidPhase.HandleFunctionTimer:
                KraidEncounterAccess.TickKraidFunctionTimer(part);
                return;

            case KraidPhase.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidPhase.FingernailWaitForLint:
                if (CoordinateAxis.Difference(_parts.TopLint.Position.X.Whole, 256) >= 0)
                {
                    _parts.NailStateOf(state, nail).Phase = part.PhaseToResume;
                    nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
                    nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
                }
                return;

            case KraidPhase.FingernailFire:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Active Kraid fingernail movement requires room collision data.");
                }
                TickKraidNailFlight(nail, level);
                return;

            case KraidPhase.NoOperation:
            case KraidPhase.GrowBreakCeilingPlatforms:
            case KraidPhase.GrowBringBodyInFront:
            case KraidPhase.GrowShowLowerBodyInFront:
            case KraidPhase.GrowDrawRoomBackground:
            case KraidPhase.GrowFadeInRoomBackground:
            case KraidPhase.MainloopThinking:
            case KraidPhase.SecondPhaseThinking:
            case KraidPhase.MouthOpenReaction:
            case KraidPhase.InitializeEyeGlow:
            case KraidPhase.GlowEye:
            case KraidPhase.UnglowEye:
            case KraidPhase.LintInactive:
            case KraidPhase.LintProduce:
            case KraidPhase.LintCharge:
            case KraidPhase.LintFire:
            case KraidPhase.AlignPartToKraid:
            case KraidPhase.DecrementFunctionTimerAndStartWalk:
            case KraidPhase.FootFirstPhaseThinking:
            case KraidPhase.AdvanceHeadAnimationAndTimer:
            case KraidPhase.FootSecondPhaseThinking:
            case KraidPhase.FootSecondPhaseWalkingRight:
            case KraidPhase.FootSecondPhaseWalkToStart:
            case KraidPhase.FootSecondPhaseInitialize:
            case KraidPhase.FootSecondPhaseWalkingLeft:
            case KraidPhase.MainAttackWithMouthOpen:
            case KraidPhase.FootPrepareFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseRetreat:
            case KraidPhase.GrowReleaseCamera:
            case KraidPhase.DeathInitialize:
            case KraidPhase.DeathFadeOut:
            case KraidPhase.DeathShowUpperBody:
            case KraidPhase.DeathShowLowerBody:
            case KraidPhase.DeathSink:
            case KraidPhase.DeathHideUpperBody:
            case KraidPhase.DeathHideLowerBody:
            case KraidPhase.DeathWaitBeforeFadeIn1:
            case KraidPhase.DeathWaitBeforeFadeIn2:
            case KraidPhase.DeathWaitBeforeFadeIn3:
            case KraidPhase.DeathWaitBeforeFadeIn4:
            case KraidPhase.DeathFadeInBackground:
            case KraidPhase.DeathFinishedWasAlive:
            case KraidPhase.DeathFinishedWasDead:
            case KraidPhase.RestrictSamusToFirstScreen:
            case KraidPhase.RaiseKraidThroughFloor:
            case KraidPhase.RaiseShowLowerBody:
            case KraidPhase.RaiseRocksEvery16Frames:
            case KraidPhase.RaiseRocksEvery8Frames:
            case KraidPhase.RaiseBody:
            case KraidPhase.None:
            default:
                throw new InvalidDataException(
                    $"Kraid fingernail function {_parts.NailStateOf(state, nail).Phase} is not translated.");
        }
    }

    /// <summary>Seeds a released nail's velocity, animation, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemySlot nail, KraidNailState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        RoomEnemySlot sibling = ReferenceEquals(nail, _parts.FirstNail) ? _parts.SecondNail : _parts.FirstNail;
        KraidNailState siblingPart = _parts.NailStateOf(_access.RequireKraidState(nail), sibling);
        int random = _room.Random.Read();
        // A sibling killed since it was set up holds its place without its state, and launches nothing.
        bool siblingKilled = !ReferenceEquals(sibling.SpeciesState<KraidPartState>(), siblingPart);
        part.NailVelocity = KraidNailLaunch.FromSiblingVelocity(
            KilledFingernailStops.SiblingSpeed(siblingPart.NailVelocity.Y.WholePixels, stopped: 0, siblingKilled));
        nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
        nail.AnimationClock.StepOnNextUpdate();
        _access.SetKraidPartAnimation(nail, KraidPartSegment.NailLoop);
        part.Phase = KraidPhase.FingernailFire;

        if (!KraidRandomDefinitions.NailWaitsForLint.IsSetIn(random) ||
            KilledFingernailStops.SiblingTookAlternateSpawn(siblingKilled ? null : siblingPart, static nail => nail.AlternateSpawn))
        {
            part.AlternateSpawn = false;
            RoomEnemySlot body = _parts.Body;
            nail.Position = nail.Position.WithWhole(new(CoordinateAxis.Wrap(
                (body.Position.X.Whole - body.Radius.X - nail.Radius.X) & ~15), CoordinateAxis.Wrap(_parts.Arm.Position.Y.Whole + 128)));
            return;
        }

        part.AlternateSpawn = true;
        nail.Position = nail.Position.WithWhole(new(50, 240));
        part.NailVelocity = FixedVector<RoomPixels>.FromStoredParts(1, 0, 0, 0);
        part.Phase = KraidPhase.FingernailWaitForLint;
        part.PhaseToResume = KraidPhase.FingernailFire;
        nail.Contact = nail.Contact.With(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Advances nail position and terrain response for one enemy update.</summary>
    internal void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        KraidNailState part = _access.NailState(nail);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, part.NailVelocity.X))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));
        else if (KraidBodyContourReflectsNail(nail))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));

        if (_terrain.MoveEnemyVertically(level, nail, part.NailVelocity.Y))
            part.NailVelocity = part.NailVelocity.WithY(SignedSixteenSixteen.Zero - part.NailVelocity.Y);
    }

    /// <summary>Checks whether a nail crosses the body contour that reverses its flight.</summary>
    internal bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _parts.Body;
        int relativeY = CoordinateAxis.Wrap(nail.Position.Y.Whole - body.Position.Y.Whole);
        // A nail above every ledge is pushed back at the record the contour walk reads after them.
        int leftOffset = KraidNailContour.LedgeLeftOffset(relativeY) ??
            _room.BossOverreads.KraidNailPastLedges.LeftOffset(relativeY);
        int contourX = CoordinateAxis.Wrap(body.Position.X.Whole + leftOffset);
        return CoordinateAxis.AsSigned(nail.Position.X.Whole + nail.Radius.X - contourX) >= 0 &&
            _access.NailState(nail).NailVelocity.X.WholePixels >= 0;
    }

    /// <summary>
    /// A wall or Kraid's contour reverses a flying nail's horizontal speed. A flying nail moves in whole pixels: its launch
    /// and its wait for the lint set speeds with no fraction (<see cref="KraidNailLaunchDefinitions"/>), and a reversal
    /// keeps a speed with no fraction one, so the reversal is the plain opposite speed (#1620).
    /// </summary>
    private static SignedSixteenSixteen ReflectKraidNailHorizontalVelocity(SignedSixteenSixteen velocity) => -velocity;

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = _access.RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
        lint.AnimationClock.HoldFor(KraidBehavior.KraidStillPartTimer);
        KraidLintState part = _parts.LintStateOf(state, lint);
        switch (_parts.LintStateOf(state, lint).Phase)
        {
            case KraidPhase.LintInactive:
                return;
            case KraidPhase.AlignPartToKraid:
                AlignKraidPart(lint, part);
                return;
            case KraidPhase.LintProduce:
                ProduceKraidLint(lint);
                return;
            case KraidPhase.LintCharge:
                ChargeKraidLint(lint);
                return;
            case KraidPhase.LintFire:
                FireKraidLint(lint, part, samus);
                return;
            case KraidPhase.NoOperation:
            case KraidPhase.GrowBreakCeilingPlatforms:
            case KraidPhase.GrowBringBodyInFront:
            case KraidPhase.GrowShowLowerBodyInFront:
            case KraidPhase.GrowDrawRoomBackground:
            case KraidPhase.GrowFadeInRoomBackground:
            case KraidPhase.MainloopThinking:
            case KraidPhase.SecondPhaseThinking:
            case KraidPhase.MouthOpenReaction:
            case KraidPhase.InitializeEyeGlow:
            case KraidPhase.GlowEye:
            case KraidPhase.UnglowEye:
            case KraidPhase.FingernailWaitForLint:
            case KraidPhase.HandleFunctionTimer:
            case KraidPhase.DecrementFunctionTimerAndStartWalk:
            case KraidPhase.FootFirstPhaseThinking:
            case KraidPhase.AdvanceHeadAnimationAndTimer:
            case KraidPhase.FootSecondPhaseThinking:
            case KraidPhase.FootSecondPhaseWalkingRight:
            case KraidPhase.FootSecondPhaseWalkToStart:
            case KraidPhase.FootSecondPhaseInitialize:
            case KraidPhase.FootSecondPhaseWalkingLeft:
            case KraidPhase.MainAttackWithMouthOpen:
            case KraidPhase.FingernailInitialize:
            case KraidPhase.FingernailFire:
            case KraidPhase.FootPrepareFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseLunge:
            case KraidPhase.FootFirstPhaseRetreat:
            case KraidPhase.GrowReleaseCamera:
            case KraidPhase.DeathInitialize:
            case KraidPhase.DeathFadeOut:
            case KraidPhase.DeathShowUpperBody:
            case KraidPhase.DeathShowLowerBody:
            case KraidPhase.DeathSink:
            case KraidPhase.DeathHideUpperBody:
            case KraidPhase.DeathHideLowerBody:
            case KraidPhase.DeathWaitBeforeFadeIn1:
            case KraidPhase.DeathWaitBeforeFadeIn2:
            case KraidPhase.DeathWaitBeforeFadeIn3:
            case KraidPhase.DeathWaitBeforeFadeIn4:
            case KraidPhase.DeathFadeInBackground:
            case KraidPhase.DeathFinishedWasAlive:
            case KraidPhase.DeathFinishedWasDead:
            case KraidPhase.RestrictSamusToFirstScreen:
            case KraidPhase.RaiseKraidThroughFloor:
            case KraidPhase.RaiseShowLowerBody:
            case KraidPhase.RaiseRocksEvery16Frames:
            case KraidPhase.RaiseRocksEvery8Frames:
            case KraidPhase.RaiseBody:
            case KraidPhase.None:
            default:
                throw new InvalidDataException(
                    $"Kraid lint slot {lint.SlotIndex} function {_parts.LintStateOf(state, lint).Phase} " +
                    "is not translated.");
        }
    }

    /// <summary>Places a physical part at its body-relative offset.</summary>
    internal void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.Position = partSlot.Position.WithWrappedWholeX(_parts.Body.Position.X.Whole - partSlot.Radius.X);
        KraidEncounterAccess.TickKraidFunctionTimer(part);
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private void ProduceKraidLint(RoomEnemySlot lint)
    {
        lint.Contact = lint.Contact.Without(EnemyContact.IgnoresSamus);
        lint.Presence = lint.Presence.Without(EnemyPresence.Hidden);
        lint.Position = lint.Position.WithWrappedWholeX(
            _access.LintState(lint).LintXOffset + _parts.Body.Position.X.Whole - _access.LintState(lint).LintTravel);
        _access.LintState(lint).LintTravel++;
        if (_access.LintState(lint).LintTravel >= 32)
        {
            _access.LintState(lint).Phase = KraidPhase.LintCharge;
            _access.LintState(lint).FunctionTimer = 30;
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemySlot lint)
    {
        lint.Shading = KraidCadenceDefinitions.LintChargeShowsOwnColours.IsSetIn(_access.LintState(lint).FunctionTimer)
            ? _access.RequireKraidState(lint).Colours.Parts
            : EnemyShades.HurtFlashLook;
        lint.Position = lint.Position.WithWrappedWholeX(
            _access.LintState(lint).LintXOffset + _parts.Body.Position.X.Whole - _access.LintState(lint).LintTravel);
        if (--_access.LintState(lint).FunctionTimer == 0)
        {
            _access.LintState(lint).Phase = KraidPhase.LintFire;
            _access.RequestKraidSound(KraidSoundDefinitions.LintFire);
        }
    }

    /// <summary>Launches a lint projectile toward Samus from the part's position.</summary>
    private static void FireKraidLint(RoomEnemySlot lint, KraidLintState part, SamusState? samus)
    {
        AddSignedKraidHorizontalDisplacement(lint, KraidPlatformMovement.FiringDisplacement);
        if (CoordinateAxis.Difference(lint.Position.X.Whole, 56) < 0)
            lint.Contact = lint.Contact.With(EnemyContact.IgnoresSamus);
        if (CoordinateAxis.Difference(lint.Position.X.Whole, 32) < 0)
        {
            lint.Presence = lint.Presence.With(EnemyPresence.Hidden);
            part.Phase = KraidPhase.AlignPartToKraid;
            part.FunctionTimer = 300;
            part.PhaseToResume = KraidPhase.LintProduce;
            part.LintTravel = 0;
        }

        // Checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement dispatcher, rather than teleporting
        // her here. The clamp changes only the whole pixels; the subpixels keep what the subtraction left.
        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(lint, samus))
        {
            SignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X + KraidPlatformMovement.FiringDisplacement;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                carry.WithWholePixelsAtLeast(KraidPlatformMovement.MinimumCarryWholePixels));
        }
    }

    /// <summary>Adds signed horizontal motion to the body position, wrapping around the coordinate range.</summary>
    private static void AddSignedKraidHorizontalDisplacement(
        RoomEnemySlot slot,
        SignedSixteenSixteen displacement)
    {
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(displacement));
    }

    /// <summary>Updates a spit rock's flight and removes it at its boundary.</summary>
    internal static void RunKraidRockFrameBehavior(
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
    internal static void RunKraidCeilingRockFrameBehavior(
        EnemyProjectile rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Vertical))
        {
            rock.Delete();
            return;
        }
        rock.Velocity = rock.Velocity.WithY(
            RestartingFallSpeed.Accelerate(rock.Velocity.Y, KraidRockLaunchDefinitions.CeilingRockGravity));
    }

    /// <summary>Binds nail touch callbacks and their shared contact-damage path.</summary>
    internal void AddKraidNailTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.KraidNail &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.KraidNailTouch ||
                               slot.DefinitionId == EnemyDefinitionId.KraidNailBad &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.KraidBadNailTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                // A fingernail the touch left in its place dies in the explosion its remaining health names.
                if (context.Slot.DefinitionId != EnemyDefinitionId.None)
                    _deathsAndDrops.StartGenericEnemyDeath(context.Slot,
                        KraidNailTouchDeathExplosion.AfterTouch(context.Slot.Health, EnemyDeathExplosionDefinitions.AuthoredOrder));
            });
    }

    /// <summary>Adds the shared frame behaviour used by spit rocks.</summary>
    internal static void AddKraidRockSpitFrameBehavior(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            context.Projectile.Graphics = SpriteGraphics.SharedStandard;
        }, EnemyProjectileFrameBehavior.KraidShotRockRecolor);
    }
}
