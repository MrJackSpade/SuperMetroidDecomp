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
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
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
        lint.FrameHold = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartProgram(lint, KraidPartProgram.LintInitial);
        lint.Frame = EnemyFrameId.KraidLint00;
        _parts.LintStateOf(state, lint).Function = KraidAiFunction.LintInactive;
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
        nail.Presence = nail.Presence.With(EnemyPresence.Hidden);
        nail.FrameHold = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        nail.Frame = EnemyFrameId.KraidNail0;
        _parts.NailStateOf(state, nail).NextFunction = KraidAiFunction.FingernailInitialize;
        _parts.NailStateOf(state, nail).Function = KraidAiFunction.HandleFunctionTimer;
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

    /// <summary>Ports KraidLintTop's initialization AI.</summary>
    internal void RunKraidLintTopInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Top);
    }

    /// <summary>Ports KraidLintMiddle's initialization AI.</summary>
    internal void RunKraidLintMiddleInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Middle);
    }

    /// <summary>Ports KraidLintBottom's initialization AI.</summary>
    internal void RunKraidLintBottomInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, KraidLintPart.Bottom);
    }

    /// <summary>Ports KraidNail's initialization AI.</summary>
    internal void RunKraidNailInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, _parts.FirstNail);
    }

    /// <summary>Ports KraidNailBad's initialization AI.</summary>
    internal void RunKraidNailBadInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, _parts.SecondNail);
    }

    /// <summary>Registers the spat rock's typed burst and the rock programs' cues.</summary>
    internal static void AddKraidRockAnimations(EnemyProjectileHandlerTable table)
    {
        table.AddShotAnimation(RoomEnemyProjectileKind.KraidSpitRock,
            () => new KraidRockAnimation(KraidRockProgram.SpitRockShot));
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
        switch (_parts.NailStateOf(state, nail).Function)
        {
            case KraidAiFunction.HandleFunctionTimer:
                KraidEncounterAccess.TickKraidFunctionTimer(part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (CoordinateAxis.Difference(_parts.TopLint.Position.X.Whole, 256) >= 0)
                {
                    _parts.NailStateOf(state, nail).Function = part.NextFunction;
                    nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
                    nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
                }
                return;

            case KraidAiFunction.FingernailFire:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Active Kraid fingernail movement requires room collision data.");
                }
                TickKraidNailFlight(nail, level);
                return;

            case KraidAiFunction.NoOperation:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowBringBodyInFront:
            case KraidAiFunction.GrowShowLowerBodyInFront:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
            case KraidAiFunction.MainloopThinking:
            case KraidAiFunction.SecondPhaseThinking:
            case KraidAiFunction.MouthOpenReaction:
            case KraidAiFunction.InitializeEyeGlow:
            case KraidAiFunction.GlowEye:
            case KraidAiFunction.UnglowEye:
            case KraidAiFunction.LintInactive:
            case KraidAiFunction.LintProduce:
            case KraidAiFunction.LintCharge:
            case KraidAiFunction.LintFire:
            case KraidAiFunction.AlignPartToKraid:
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
            case KraidAiFunction.FootFirstPhaseThinking:
            case KraidAiFunction.ProcessHeadInstructionAndTimer:
            case KraidAiFunction.FootSecondPhaseThinking:
            case KraidAiFunction.FootSecondPhaseWalkingRight:
            case KraidAiFunction.FootSecondPhaseWalkToStart:
            case KraidAiFunction.FootSecondPhaseInitialize:
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
            case KraidAiFunction.MainAttackWithMouthOpen:
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseRetreat:
            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathShowUpperBody:
            case KraidAiFunction.DeathShowLowerBody:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathHideUpperBody:
            case KraidAiFunction.DeathHideLowerBody:
            case KraidAiFunction.DeathWaitBeforeFadeIn1:
            case KraidAiFunction.DeathWaitBeforeFadeIn2:
            case KraidAiFunction.DeathWaitBeforeFadeIn3:
            case KraidAiFunction.DeathWaitBeforeFadeIn4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseShowLowerBody:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Kraid fingernail function {_parts.NailStateOf(state, nail).Function} is not translated.");
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
        part.NailLaunched = true;
        nail.Contact = nail.Contact.Without(EnemyContact.IgnoresSamus);
        nail.Presence = nail.Presence.Without(EnemyPresence.Hidden);
        nail.FrameHold = WrappingCountdown.Start(1);
        _access.SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        part.Function = KraidAiFunction.FingernailFire;

        if (!KraidRandomDefinitions.NailWaitsForLint.IsSetIn(random) || siblingPart.AlternateSpawn)
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
        part.Function = KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
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
    /// Horizontal wall/contour reflections and the nail's reflection negate
    /// each part independently (<see cref="SignedSixteenSixteen.NegatedPartsSeparately"/>): the whole pixels change sign
    /// and a nonzero fraction becomes its complement within the pixel. A nail with a fraction therefore rebounds one
    /// pixel an update to the right of its true reversal, whichever way it arrived; the quirk is the original's and
    /// changes the nail's path.
    /// </summary>
    private static SignedSixteenSixteen ReflectKraidNailHorizontalVelocity(SignedSixteenSixteen velocity) =>
        velocity.NegatedPartsSeparately();

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = _access.RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
        lint.FrameHold = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        KraidLintState part = _parts.LintStateOf(state, lint);
        switch (_parts.LintStateOf(state, lint).Function)
        {
            case KraidAiFunction.LintInactive:
                return;
            case KraidAiFunction.AlignPartToKraid:
                AlignKraidPart(lint, part);
                return;
            case KraidAiFunction.LintProduce:
                ProduceKraidLint(lint);
                return;
            case KraidAiFunction.LintCharge:
                ChargeKraidLint(lint);
                return;
            case KraidAiFunction.LintFire:
                FireKraidLint(lint, part, samus);
                return;
            case KraidAiFunction.NoOperation:
            case KraidAiFunction.GrowBreakCeilingPlatforms:
            case KraidAiFunction.GrowBringBodyInFront:
            case KraidAiFunction.GrowShowLowerBodyInFront:
            case KraidAiFunction.GrowDrawRoomBackground:
            case KraidAiFunction.GrowFadeInRoomBackground:
            case KraidAiFunction.MainloopThinking:
            case KraidAiFunction.SecondPhaseThinking:
            case KraidAiFunction.MouthOpenReaction:
            case KraidAiFunction.InitializeEyeGlow:
            case KraidAiFunction.GlowEye:
            case KraidAiFunction.UnglowEye:
            case KraidAiFunction.FingernailWaitForLint:
            case KraidAiFunction.HandleFunctionTimer:
            case KraidAiFunction.DecrementFunctionTimerAndStartWalk:
            case KraidAiFunction.FootFirstPhaseThinking:
            case KraidAiFunction.ProcessHeadInstructionAndTimer:
            case KraidAiFunction.FootSecondPhaseThinking:
            case KraidAiFunction.FootSecondPhaseWalkingRight:
            case KraidAiFunction.FootSecondPhaseWalkToStart:
            case KraidAiFunction.FootSecondPhaseInitialize:
            case KraidAiFunction.FootSecondPhaseWalkingLeft:
            case KraidAiFunction.MainAttackWithMouthOpen:
            case KraidAiFunction.FingernailInitialize:
            case KraidAiFunction.FingernailFire:
            case KraidAiFunction.FootPrepareFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseLunge:
            case KraidAiFunction.FootFirstPhaseRetreat:
            case KraidAiFunction.GrowReleaseCamera:
            case KraidAiFunction.DeathInitialize:
            case KraidAiFunction.DeathFadeOut:
            case KraidAiFunction.DeathShowUpperBody:
            case KraidAiFunction.DeathShowLowerBody:
            case KraidAiFunction.DeathSink:
            case KraidAiFunction.DeathHideUpperBody:
            case KraidAiFunction.DeathHideLowerBody:
            case KraidAiFunction.DeathWaitBeforeFadeIn1:
            case KraidAiFunction.DeathWaitBeforeFadeIn2:
            case KraidAiFunction.DeathWaitBeforeFadeIn3:
            case KraidAiFunction.DeathWaitBeforeFadeIn4:
            case KraidAiFunction.DeathFadeInBackground:
            case KraidAiFunction.DeathFinishedWasAlive:
            case KraidAiFunction.DeathFinishedWasDead:
            case KraidAiFunction.RestrictSamusToFirstScreen:
            case KraidAiFunction.RaiseKraidThroughFloor:
            case KraidAiFunction.RaiseShowLowerBody:
            case KraidAiFunction.RaiseRocksEvery16Frames:
            case KraidAiFunction.RaiseRocksEvery8Frames:
            case KraidAiFunction.RaiseBody:
            case KraidAiFunction.None:
            default:
                throw new InvalidDataException(
                    $"Kraid lint slot {lint.SlotIndex} function {_parts.LintStateOf(state, lint).Function} " +
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
            _access.LintState(lint).Function = KraidAiFunction.LintCharge;
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
            _access.LintState(lint).Function = KraidAiFunction.LintFire;
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
            part.Function = KraidAiFunction.AlignPartToKraid;
            part.FunctionTimer = 300;
            part.NextFunction = KraidAiFunction.LintProduce;
            part.LintTravel = 0;
        }

        // Checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement dispatcher, rather than teleporting
        // her here. The clamp changes only the whole pixels; the subpixels keep their borrow.
        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(lint, samus))
        {
            SignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X + KraidPlatformMovement.FiringDisplacement;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                carry.WithWholePixelsAtLeast(KraidPlatformMovement.MinimumCarryWholePixels));
        }
    }

    /// <summary>Adds signed horizontal motion to the body position with word wraparound.</summary>
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
                // The callback passes the common routine's remaining-health accumulator to the enemy's death as the explosion.
                if (context.Slot.DefinitionId != EnemyDefinitionId.None)
                    _deathsAndDrops.StartGenericEnemyDeath(context.Slot, EnemyDeathExplosions.ForLeftoverNumber(context.Slot.Health));
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
