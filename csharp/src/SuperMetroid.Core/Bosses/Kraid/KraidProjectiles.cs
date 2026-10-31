using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's lints, nails and spat rocks.</summary>
[RoomLifetime]
internal sealed class KraidProjectiles
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves nail and foot movement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Provides the native body and seven part slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Checks mouth, body, and part collisions against Samus and shots.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies contact damage, death state, and resulting drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidProjectiles(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops, KraidEncounterAccess access)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
        _access = access;
    }

    /// <summary>Initializes one lint part and verifies its expected physical slot.</summary>
    private void InitializeKraidLint(RoomEnemySlot lint, int expectedSlot)
    {
        KraidEnemyState state = _access.RequireKraidState(lint);
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(lint);
            return;
        }
        KraidEncounterAccess.EnsureKraidSlot(lint, expectedSlot, "lint");
        KraidEncounterAccess.AttachKraidPart(lint, state);
        lint.Colouring = _slotTable.Slots[0].Colouring;
        lint.InstructionTimer = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartProgram(lint, KraidPartProgram.LintInitial);
        lint.Frame = EnemyFrameId.KraidLint00;
        state.Parts[lint.SlotIndex].Function = KraidAiFunction.LintInactive;
        state.Parts[lint.SlotIndex].LintXOffset = expectedSlot == 2 ? (ushort)0 : unchecked((ushort)-16);
    }

    /// <summary>
    /// Ports <c>InitAI_KraidNail_Common</c> ($A7:BCF2). Unlike the other parts it has no
    /// dead-room branch: in a defeated Kraid's room the fingernails initialize and run,
    /// deleting themselves only once slot zero's health reads below one ($A7:BD34).
    /// </summary>
    internal void InitializeKraidNail(RoomEnemySlot nail, int expectedSlot)
    {
        KraidEnemyState state = _access.RequireKraidState(nail);
        KraidEncounterAccess.EnsureKraidSlot(nail, expectedSlot, "fingernail");
        KraidEncounterAccess.AttachKraidPart(nail, state);
        nail.Colouring = _slotTable.Slots[0].Colouring;
        KraidPartState nailPart = state.Parts[nail.SlotIndex];
        nailPart.NailVelocity = nailPart.NailVelocity.WithX(nailPart.NailVelocity.X.WithFraction(40));
        nail.Properties = nail.Properties.With(EnemyProperties.Invisible);
        nail.InstructionTimer = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        _access.SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        nail.Frame = EnemyFrameId.KraidNail0;
        state.Parts[nail.SlotIndex].NextFunction = KraidAiFunction.FingernailInitialize;
        state.Parts[nail.SlotIndex].Function = KraidAiFunction.HandleFunctionTimer;
        state.Parts[nail.SlotIndex].FunctionTimer = 64;
    }

    /// <summary>Registers the rock projectile initializers and per-frame handlers.</summary>
    internal static void AddKraidProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunKraidRockPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KraidRocks);

        table.AddPreInstruction(context =>
        {
            RunKraidCeilingRockPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.KraidCeilingRocks);
    }

    /// <summary>Ports KraidLintTop's initialization AI.</summary>
    internal void RunKraidLintTopInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, expectedSlot: 2);
    }

    /// <summary>Ports KraidLintMiddle's initialization AI.</summary>
    internal void RunKraidLintMiddleInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, expectedSlot: 3);
    }

    /// <summary>Ports KraidLintBottom's initialization AI.</summary>
    internal void RunKraidLintBottomInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidLint(slot, expectedSlot: 4);
    }

    /// <summary>Ports KraidNail's initialization AI.</summary>
    internal void RunKraidNailInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, expectedSlot: 6);
    }

    /// <summary>Ports KraidNailBad's initialization AI.</summary>
    internal void RunKraidNailBadInitializationAi(RoomEnemySlot slot)
    {
        InitializeKraidNail(slot, expectedSlot: 7);
    }

    /// <summary>Registers the spat rock's typed burst and the rock programs' cues.</summary>
    internal static void AddKraidRockAnimations(EnemyProjectileHandlerTable table)
    {
        table.AddShotAnimation(RoomEnemyProjectileKind.KraidSpitRock,
            () => new KraidRockAnimation(KraidRockProgram.SpitRockShot));
        table.AddCue<KraidRockCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case KraidRockCue.UsePaletteZero:
                    projectile.PreInstruction = EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0;
                    return;
                case KraidRockCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(KraidRockCue)} {cue}.");
            }
        });
    }

    /// <summary>Checks lint contact against Samus without altering the body phase.</summary>
    private void ResolveKraidLintContact(RoomEnemySlot lint, SamusState samus)
    {
        if (lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) || !samus.InvincibilityTimer.IsZero)
            return;
        int edge = unchecked((ushort)(lint.Position.X.Whole + KraidContactDefinitions.LintLeadingEdge));
        if (unchecked((short)(samus.Position.X.Whole + samus.Kinematics.Radius.X - edge)) < 0 ||
            unchecked((short)(samus.Position.X.Whole - samus.Kinematics.Radius.X - edge)) >= 0 ||
            unchecked((short)(samus.Position.Y.Whole + samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintTop)) < 0 ||
            unchecked((short)(samus.Position.Y.Whole - samus.Kinematics.Radius.Y -
                lint.Position.Y.Whole - KraidContactDefinitions.LintBottom)) >= 0)
            return;
        int push = unchecked((ushort)(~(samus.Kinematics.Radius.X + KraidContactDefinitions.LintPush) +
            samus.Kinematics.ExtraDisplacement.X.WholePixels));
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(unchecked((short)(
            unchecked((short)(push - KraidContactDefinitions.LintPush)) < 0 ? push : KraidContactDefinitions.LintPush)));
        _collision.ResolveNormalEnemyTouch(lint, samus);
        lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Dispatches a nail's active flight or rebound function.</summary>
    internal void RunKraidNailMain(RoomEnemySlot nail, RoomLevelData? level)
    {
        KraidEnemyState state = _access.RequireKraidState(nail);
        if (_slotTable.Slots[0].Health == 0)
        {
            nail.Properties = nail.Properties.With(
                EnemyProperties.Deleted | EnemyProperties.Invisible);
            return;
        }

        KraidPartState part = state.Parts[nail.SlotIndex];
        switch (state.Parts[nail.SlotIndex].Function)
        {
            case KraidAiFunction.HandleFunctionTimer:
                KraidEncounterAccess.TickKraidFunctionTimer(part);
                return;

            case KraidAiFunction.FingernailInitialize:
                InitializeKraidNailFlight(nail, part);
                return;

            case KraidAiFunction.FingernailWaitForLint:
                if (unchecked((short)(_slotTable.Slots[2].Position.X.Whole - 256)) >= 0)
                {
                    state.Parts[nail.SlotIndex].Function = part.NextFunction;
                    nail.Properties = nail.Properties.Without(
                        EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
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
                    $"Kraid fingernail function {state.Parts[nail.SlotIndex].Function} is not translated.");
        }
    }

    /// <summary>Seeds a released nail's velocity, instruction list, and collision state.</summary>
    internal void InitializeKraidNailFlight(RoomEnemySlot nail, KraidPartState part)
    {
        // The pair coordinates its next launch through the other actor's previous
        // velocity and spawn flag. Read that actor before overwriting this one's state.
        int siblingIndex = nail.SlotIndex == 6 ? 7 : 6;
        KraidPartState siblingPart = _access.RequireKraidState(nail).Parts[siblingIndex];
        int random = _room.Random.Read();
        part.NailVelocity = KraidNailLaunch.FromSiblingVelocity(siblingPart.NailVelocity.Y.WholeWord);
        part.NailLaunched = true;
        nail.Properties = nail.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        nail.InstructionTimer = WrappingCountdown.Start(1);
        _access.SetKraidPartProgram(nail, KraidPartProgram.NailLoop);
        part.Function = KraidAiFunction.FingernailFire;

        if ((random & 1) == 0 || siblingPart.AlternateSpawn)
        {
            part.AlternateSpawn = false;
            RoomEnemySlot body = _slotTable.Slots[0];
            nail.Position = nail.Position.WithWhole(new(unchecked((ushort)(
                (body.Position.X.Whole - body.Radius.X - nail.Radius.X) & ~15)), unchecked((ushort)(_slotTable.Slots[1].Position.Y.Whole + 128))));
            return;
        }

        part.AlternateSpawn = true;
        nail.Position = nail.Position.WithWhole(new(50, 240));
        part.NailVelocity = FixedVector<RoomPixels>.FromWords(1, 0, 0, 0);
        part.Function = KraidAiFunction.FingernailWaitForLint;
        part.NextFunction = KraidAiFunction.FingernailFire;
        nail.Properties = nail.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Advances nail position and terrain response for one enemy update.</summary>
    internal void TickKraidNailFlight(RoomEnemySlot nail, RoomLevelData level)
    {
        KraidPartState part = _access.Part(nail);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, nail, part.NailVelocity.X.RawValue))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));
        else if (KraidBodyContourReflectsNail(nail))
            part.NailVelocity = part.NailVelocity.WithX(ReflectKraidNailHorizontalVelocity(part.NailVelocity.X));

        if (_terrain.MoveEnemyVertically(level, nail, part.NailVelocity.Y.RawValue))
            part.NailVelocity = part.NailVelocity.WithY(SignedSixteenSixteen.FromRaw(unchecked(-part.NailVelocity.Y.RawValue)));
    }

    /// <summary>Checks whether a nail crosses the body contour that reverses its flight.</summary>
    internal bool KraidBodyContourReflectsNail(RoomEnemySlot nail)
    {
        RoomEnemySlot body = _slotTable.Slots[0];
        int contourX = unchecked((ushort)(
            body.Position.X.Whole + KraidNailContour.LeftOffset(
                unchecked((ushort)(nail.Position.Y.Whole - body.Position.Y.Whole)))));
        return unchecked((short)(nail.Position.X.Whole + nail.Radius.X - contourX)) >= 0 &&
            _access.Part(nail).NailVelocity.X.WholePixels >= 0;
    }

    /// <summary>
    /// Native horizontal wall/contour reflections at $A7:BE9E and BEE2 negate
    /// each word independently. Do not propagate fractional borrow as a normal
    /// fixed-point negation would; the resulting nonzero-fraction quirk is native.
    /// </summary>
    private static SignedSixteenSixteen ReflectKraidNailHorizontalVelocity(SignedSixteenSixteen velocity) =>
        SignedSixteenSixteen.FromParts(unchecked((short)-velocity.WholeWord), unchecked((ushort)-velocity.Fraction));

    /// <summary>Advances a lint part through idle, charge, fire, and reattachment states.</summary>
    internal void RunKraidLintMain(RoomEnemySlot lint, SamusState? samus)
    {
        KraidEnemyState state = _access.RequireKraidState(lint);
        if (samus is not null)
            ResolveKraidLintContact(lint, samus);
        lint.InstructionTimer = WrappingCountdown.Start(KraidBehavior.KraidStillPartTimer);
        KraidPartState part = state.Parts[lint.SlotIndex];
        switch (state.Parts[lint.SlotIndex].Function)
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
                    $"Kraid lint slot {lint.SlotIndex} function {state.Parts[lint.SlotIndex].Function} " +
                    "is not translated.");
        }
    }

    /// <summary>Places a physical part at its body-relative offset.</summary>
    internal void AlignKraidPart(RoomEnemySlot partSlot, KraidPartState part)
    {
        partSlot.Position = partSlot.Position.WithWholeX(unchecked((ushort)(_slotTable.Slots[0].Position.X.Whole - partSlot.Radius.X)));
        KraidEncounterAccess.TickKraidFunctionTimer(part);
    }

    /// <summary>Starts a lint projectile from the attached part.</summary>
    private void ProduceKraidLint(RoomEnemySlot lint)
    {
        lint.Properties = lint.Properties.Without(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            _access.Part(lint).LintXOffset + _slotTable.Slots[0].Position.X.Whole - _access.Part(lint).LintTravel)));
        _access.Part(lint).LintTravel = unchecked((ushort)(_access.Part(lint).LintTravel + 1));
        if (unchecked((short)(_access.Part(lint).LintTravel - 32)) >= 0)
        {
            _access.Part(lint).Function = KraidAiFunction.LintCharge;
            _access.Part(lint).FunctionTimer = 30;
        }
    }

    /// <summary>Advances the visible lint charge before firing.</summary>
    private void ChargeKraidLint(RoomEnemySlot lint)
    {
        lint.Colouring = (_access.Part(lint).FunctionTimer & 1) != 0
            ? new GraphicsSetMemberColouring(EnemyDefinitionId.Kraid)
            : SharedSpriteColouring.Common;
        lint.Position = lint.Position.WithWholeX(unchecked((ushort)(
            _access.Part(lint).LintXOffset + _slotTable.Slots[0].Position.X.Whole - _access.Part(lint).LintTravel)));
        CountdownStep charge = WrappingCountdown.FromRaw(checked((ushort)_access.Part(lint).FunctionTimer)).Decrement();
        _access.Part(lint).FunctionTimer = charge.Remaining.RemainingFrames;
        if (charge.IsZero)
        {
            _access.Part(lint).Function = KraidAiFunction.LintFire;
            _access.RequestKraidSound(KraidSoundDefinitions.LintFire);
        }
    }

    /// <summary>Launches a lint projectile toward Samus from the part's position.</summary>
    private static void FireKraidLint(RoomEnemySlot lint, KraidPartState part, SamusState? samus)
    {
        AddSignedKraidHorizontalDisplacement(lint, KraidPlatformMovement.FiringDisplacement);
        if (unchecked((short)(lint.Position.X.Whole - 56)) < 0)
            lint.Properties = lint.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (unchecked((short)(lint.Position.X.Whole - 32)) < 0)
        {
            lint.Properties = lint.Properties.With(EnemyProperties.Invisible);
            part.Function = KraidAiFunction.AlignPartToKraid;
            part.FunctionTimer = 300;
            part.NextFunction = KraidAiFunction.LintProduce;
            part.LintTravel = 0;
        }

        // Native checks support after moving and even on the frame that hides the lint.
        // Publish carry for Samus's later movement dispatcher, rather than teleporting
        // her here. The clamp changes only the whole word; retain the fractional borrow.
        if (samus is not null && EnemyCollision.IsEnemyTouchingSamusFromBelow(lint, samus))
        {
            SignedSixteenSixteen carry = samus.Kinematics.ExtraDisplacement.X +
                SignedSixteenSixteen.FromRaw(KraidPlatformMovement.FiringDisplacement);
            int whole = carry.WholePixels;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(carry.WithWholePixels((short)(
                unchecked((short)(whole - KraidPlatformMovement.MinimumCarryWholePixels)) < 0
                    ? KraidPlatformMovement.MinimumCarryWholePixels : whole)));
        }
    }

    /// <summary>Adds native signed horizontal motion to the body position with word wraparound.</summary>
    private static void AddSignedKraidHorizontalDisplacement(
        RoomEnemySlot slot,
        int displacement)
    {
        uint position = slot.Position.X.RawValue;
        position = unchecked(position + (uint)displacement);
        slot.Position = slot.Position.WithX(FixedCoordinate.FromRaw(unchecked(position)));
    }

    /// <summary>Updates a spit rock's flight and removes it at its native boundary.</summary>
    internal static void RunKraidRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Vertical))
        {
            rock.Clear();
            return;
        }

        int horizontal = unchecked((short)(rock.Velocity.X.RawValue + 8));
        if (horizontal >= 256)
            horizontal = -256;
        rock.Velocity = rock.Velocity.WithX(new SignedEightEight(unchecked((ushort)horizontal)));
        rock.Velocity = rock.Velocity.WithY(new SignedEightEight(unchecked((ushort)(rock.Velocity.Y.RawValue + 64))));
    }

    /// <summary>Updates a falling ceiling rock and its contact with room terrain.</summary>
    internal static void RunKraidCeilingRockPreInstruction(
        RoomEnemyProjectileSlot rock,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(rock, level, axis: Axis.Vertical))
        {
            rock.Clear();
            return;
        }
        rock.Velocity = rock.Velocity.WithY(new SignedEightEight(unchecked((ushort)((rock.Velocity.Y.RawValue + 24) & 16383))));
    }

    /// <summary>Binds nail touch callbacks and their shared contact-damage path.</summary>
    internal void AddKraidNailTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.KraidNail &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidNailTouch ||
                               slot.DefinitionId == EnemyDefinitionId.KraidNailBad &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.KraidBadNailTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus);
                // The callback passes the common routine's remaining-health accumulator to EnemyDeath as the explosion.
                if (context.Slot.DefinitionId != EnemyDefinitionId.None)
                    _deathsAndDrops.StartGenericEnemyDeath(context.Slot, EnemyDeathExplosions.ForLeftoverNumber(context.Slot.Health));
            });
    }

    /// <summary>Registers the shared pre-instruction used by spit rocks.</summary>
    internal static void AddKraidRockSpitPreInstruction(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            context.Projectile.Graphics = default;
        }, EnemyProjectilePreInstruction.KraidRockSpit_UsePalette0);
    }
}
