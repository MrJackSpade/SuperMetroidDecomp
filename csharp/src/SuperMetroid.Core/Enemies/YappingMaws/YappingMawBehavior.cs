using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class YappingMawBehavior : IEnemySpecies
{
    /// <summary>Supplies the room context needed when the Maw changes Samus' pose.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Creates the four auxiliary body-link projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns the root sprite object accompanying the body links.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Caps camera movement while Samus is held at the Maw's mouth.</summary>
    private readonly SamusScrollCap _scrollCap;

    /// <summary>Connects Maw AI to room, body-link, sprite-object, and Samus camera services.</summary>
    internal YappingMawBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects, SamusScrollCap scrollCap)
    {
        _room = room;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
        _scrollCap = scrollCap;
    }

    /// <summary>Carries out one cue of the Yapping Maw's animations: holding a grabbed Samus at a mouth offset, or the bite sound.</summary>
    private YappingMawProgram? ApplyYappingMawCue(YappingMawEnemyState state, YappingMawCue cue)
    {
        switch (cue.Kind)
        {
            case YappingMawCueKind.HoldSamusUpRight:
                SetYappingMawHeldOffset(state, directionIndex: 1);
                break;
            case YappingMawCueKind.HoldSamusUpLeft:
                SetYappingMawHeldOffset(state, directionIndex: 7);
                break;
            case YappingMawCueKind.HoldSamusDownRight:
                SetYappingMawHeldOffset(state, directionIndex: 3);
                break;
            case YappingMawCueKind.HoldSamusDownLeft:
                SetYappingMawHeldOffset(state, directionIndex: 5);
                break;
            case YappingMawCueKind.HoldSamusUp:
                SetYappingMawHeldOffset(state, directionIndex: 0);
                break;
            case YappingMawCueKind.HoldSamusDown:
                SetYappingMawHeldOffset(state, directionIndex: 4);
                break;
            case YappingMawCueKind.PlayBiteSound:
                PlayYappingMawAttackSound(state);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(YappingMawCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Minimum separation used before moving Samus to the extended mouth.</summary>
    private const ushort YappingMawGrabSafetyDistance = 32;
    /// <summary>Maximum extension curl admitted by the four-link solver.</summary>
    private const ushort YappingMawMaximumCurl = 128;
    /// <summary>Cooldown after Samus is released before the Maw can grab again.</summary>
    private const ushort YappingMawGrabCooldownFrames = 48;
    /// <summary>Delay after full retraction before Samus input is released.</summary>
    private const ushort YappingMawRetractedDelayFrames = 64;

    /// <summary>Per-slot Maw state including body links, extension, and held-Samus offset.</summary>
    internal readonly YappingMawEnemyState?[] _yappingMawStates =
        new YappingMawEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two $2F request emitted by animation opcode $A133.</summary>
    public ushort? LastYappingMawSoundEffect { get; private set; }

    /// <summary>Ports <c>YappingMaw_Init</c> at $A8:A148.</summary>
    internal void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Function = YappingMawAiFunction.WaitingForSamus,
            Origin = slot.Position.Whole,
            ActivationDistance = slot.Parameter1,
            RetractedDelayTimer = NativeCountdown.Start(YappingMawRetractedDelayFrames),
            OriginalPaletteBits = new SnesObjAttributeWord(slot.PaletteIndex).PaletteBits,
        };
        _yappingMawStates[slot.SlotIndex] = state;

        state.Program = AnimationProgramCursor<YappingMawProgram>.Start(slot.Parameter2 == 0
            ? YappingMawProgram.AttackingDown
            : YappingMawProgram.AttackingUp);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        // SpawnEprojWithGfx searches native indexes $22,$20,... downward. The initializer
        // receives parameters 3,2,1,0 and writes those physical indexes into variables
        // 43,42,41,40 respectively. Allocate in that literal order, then expose links in
        // root-to-mouth order so their geometric meaning is clear in a debugger.
        for (int parameter = 3; parameter >= 0; parameter--)
        {
            RoomEnemyProjectileSlot? body = _projectiles.AllocateEnemyProjectile();
            if (body is null)
            {
                throw new InvalidDataException(
                    $"Yapping Maw slot {slot.SlotIndex} could not allocate all four native body projectiles.");
            }

            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                body,
                RoomEnemyProjectileKind.YappingMawBody,
                unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex)));
            body.Position = body.Position.WithWhole(slot.Position.Whole);
            body.InstallAnimation(new YappingMawBodyProjectileAnimation(slot.Parameter2 == 0
                ? YappingMawBodyProjectileProgram.FacingDown
                : YappingMawBodyProjectileProgram.FacingUp));
            state.BodyProjectiles[parameter] = body;
        }

        // CreateSpriteAtPos uses object number 56 for parameter zero and 57 otherwise. Its
        // returned native index is represented by the retained physical slot reference.
        state.RootSpriteObject = _spriteObjects.SpawnRoomSpriteObject(
            new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + (slot.Parameter2 == 0 ? -8 : 8)))),
            slot.Parameter2 == 0
                ? RoomSpriteObjectKind.YappingMawRootVariantZero
                : RoomSpriteObjectKind.YappingMawRootVariantOne,
            unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex)));

        if (state.RootSpriteObject is null)
        {
            throw new InvalidDataException(
                $"Yapping Maw slot {slot.SlotIndex} could not allocate its native root sprite object.");
        }
    }

    /// <summary>Ports $A8:A211 and the four post-dispatch link-position writers.</summary>
    private void RunYappingMawMain(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        state.GrabCooldown = state.GrabCooldown.Decrement().Remaining;
        state.IsOffScreen = IsYappingMawPointOffScreen(slot, camera);

        if (samus is null)
        {
            throw new InvalidOperationException(
                "Yapping Maw main AI requires the active Samus actor for targeting.");
        }

        switch (state.Function)
        {
            case YappingMawAiFunction.WaitingForSamus:
                WaitForSamusWithYappingMaw(state, samus);
                break;
            case YappingMawAiFunction.BeginExtension:
                BeginYappingMawExtension(slot, state);
                break;
            case YappingMawAiFunction.ExtendingOrRetracting:
                AdvanceYappingMawExtension(slot, state, samus);
                break;
            case YappingMawAiFunction.RetractedDelay:
                WaitAfterYappingMawRetraction(state, samus);
                break;
            default:
                throw new InvalidDataException(
                    $"Yapping Maw function $A8:{(ushort)state.Function:X4} is not translated.");
        }

        // The mouth actor owns the final point. The four eprojs own root, quarter, half,
        // and three-quarter points in variables 40..43. Hardware writes coordinates even
        // though their pre-instructions are empty; bank $86 advances only their one-frame
        // map selection and permanent sleep opcode.
        PositionYappingMawBodyProjectile(state, 0, state.Origin);
        PositionYappingMawBodyProjectile(state, 1, YappingMawLinkPosition(state, 0));
        PositionYappingMawBodyProjectile(state, 2, YappingMawLinkPosition(state, 1));
        PositionYappingMawBodyProjectile(state, 3, YappingMawLinkPosition(state, 2));
    }

    /// <summary>Ports $A8:A235, including the deliberately approximate distance helper.</summary>
    private static void WaitForSamusWithYappingMaw(
        YappingMawEnemyState state,
        SamusState samus)
    {
        (ushort distance, ushort angle) = CalculateYappingMawTarget(
            state.Origin,
            samus.Position.Whole);
        state.DistanceToSamus = distance;

        if (unchecked((short)(distance - YappingMawGrabSafetyDistance)) < 0)
        {
            // A target already inside 32 pixels cannot safely trigger an extension and has
            // its grab gate refreshed to 48 every frame. This is why the cooldown must wrap.
            state.GrabCooldown = NativeCountdown.Start(YappingMawGrabCooldownFrames);
            return;
        }

        if (unchecked((short)(distance - state.ActivationDistance)) >= 0)
            return;

        // Native BMI skips the assignment for a shorter target. Inflating a short
        // distance instead makes the mouth overshoot the position it aimed at.
        if (unchecked((short)(state.DistanceToSamus - YappingMawRomData.MaximumTargetDistance)) >= 0)
            state.DistanceToSamus = YappingMawRomData.MaximumTargetDistance;

        state.AimAngle = angle;
        state.Function = YappingMawAiFunction.BeginExtension;
    }

    /// <summary>Ports the one-frame setup function at $A8:A28C.</summary>
    internal static void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.ExtensionWhole = 0;
        state.ExtensionFraction = 0;
        state.QuadraticSpeedByteOffset = 0;
        state.SegmentRadius = unchecked((ushort)(state.DistanceToSamus >> 1));
        state.CurlAngle = unchecked((byte)(64 - state.AimAngle));
        state.MirroredBend = unchecked((short)(state.CurlAngle - 128)) < 0
            ? (ushort)0
            : (ushort)1;

        state.Baseline = new Offset16<RoomPixels>(
            unchecked((short)CalculateYappingMawX(128, state.SegmentRadius)),
            unchecked((short)CalculateYappingMawY(
                128,
                unchecked((ushort)(state.SegmentRadius >> 1)))));

        ushort directionByteOffset = unchecked((ushort)(
            2 * (unchecked((byte)(state.AimAngle + 16)) >> 5)));
        state.DirectionTableByteOffset = directionByteOffset;
        int direction = directionByteOffset / 2;
        state.Program = AnimationProgramCursor<YappingMawProgram>.Start(YappingMawAnimationDefinitions.AttackForDirection(direction));
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        state.HeldSamusOffset = YappingMawRomData.HeldSamusOffset(direction);
        state.Function = YappingMawAiFunction.ExtendingOrRetracting;
    }

    /// <summary>Ports the curved four-link solver and state transition at $A8:A445.</summary>
    private void AdvanceYappingMawExtension(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        ushort bendStep = unchecked((ushort)(state.ExtensionWhole >> 2));
        // $A8:A532 clears carry once, then adds the eight correction words in segment
        // order (X then Y) with ADC alone, so each addition's carry adds one to the next.
        int carry = 0;
        for (int segment = 0; segment < 4; segment++)
        {
            ushort multiple = unchecked((ushort)(bendStep * (segment + 1)));
            ushort linkAngle = state.MirroredBend != 0
                ? unchecked((ushort)(128 + multiple))
                : unchecked((ushort)(128 - multiple));

            ushort rawX = unchecked((ushort)(
                CalculateYappingMawX(linkAngle, state.SegmentRadius) - state.Baseline.X));
            ushort rawY = unchecked((ushort)(
                CalculateYappingMawY(
                    linkAngle,
                    unchecked((ushort)(state.SegmentRadius >> 1))) - state.Baseline.Y));

            // Variables 27..2E rotate a correction whose length is the LOW BYTE of raw X.
            // Passing raw Y here is an attractive cleanup and is wrong; the cartridge's
            // lopsided curve is a direct consequence of reusing raw X for both components.
            ushort correctionX = unchecked((ushort)(
                CalculateYappingMawX(state.CurlAngle, rawX) -
                CalculateYappingMawX(0, rawX)));
            ushort correctionY = unchecked((ushort)(
                CalculateYappingMawY(state.CurlAngle, rawX) -
                CalculateYappingMawY(0, rawX)));
            state.Corrections[segment] = new Offset16<RoomPixels>(
                unchecked((short)correctionX), unchecked((short)correctionY));
            // X is added before Y so its carry feeds the Y addition.
            ushort segmentX = AddWithCarry(rawX, correctionX, ref carry);
            ushort segmentY = AddWithCarry(rawY, correctionY, ref carry);
            state.SegmentOffsets[segment] = new Offset16<RoomPixels>(
                unchecked((short)segmentX), unchecked((short)segmentY));
        }

        slot.Position = slot.Position.WithWhole(YappingMawLinkPosition(state, 3));
        AddYappingMawQuadraticSpeed(state);

        if (unchecked((short)state.ExtensionWhole) >= 0)
        {
            if (unchecked((short)(state.ExtensionWhole - YappingMawMaximumCurl)) >= 0)
            {
                state.ExtensionWhole = YappingMawMaximumCurl;
                state.ExtensionFraction = 0;
                // $A8:A4F2 adds four after the ordinary eight-byte speed-table advance.
                state.QuadraticSpeedByteOffset = unchecked((ushort)(
                    state.QuadraticSpeedByteOffset + 4));
            }

            if (state.HasGrabbedSamus)
                PositionSamusInYappingMaw(slot, state, samus);
            return;
        }

        state.Function = YappingMawAiFunction.RetractedDelay;
        state.GrabCooldown = NativeCountdown.Start(YappingMawGrabCooldownFrames);

        // The six retracted lists are not an eight-way table. Straight-ish sectors share a
        // list, while the vertical sectors at byte offsets four and twelve have dedicated
        // art. Preserve parameter-zero's early-return quirk: on its shared-list branches it
        // skips the final held-Samus placement on this one transition frame.
        bool skipHeldPlacement = false;
        if (slot.Parameter2 != 0)
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.DirectionTableByteOffset switch
            {
                4 => YappingMawProgram.CooldownUpRight,
                12 => YappingMawProgram.CooldownUpLeft,
                _ => YappingMawProgram.CooldownUp,
            });
        }
        else
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.DirectionTableByteOffset switch
            {
                4 => YappingMawProgram.CooldownDownRight,
                12 => YappingMawProgram.CooldownDownLeft,
                _ => YappingMawProgram.CooldownDown,
            });
            skipHeldPlacement = state.DirectionTableByteOffset is not (4 or 12);
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        if (!skipHeldPlacement && state.HasGrabbedSamus)
            PositionSamusInYappingMaw(slot, state, samus);
    }

    /// <summary>Adds two split native words and propagates carry from the low word.</summary>
    private static ushort AddWithCarry(ushort value, ushort addend, ref int carry)
    {
        int sum = value + addend + carry;
        carry = SnesCarry.OutOfWord(sum);
        return unchecked((ushort)sum);
    }

    /// <summary>Ports $A8:A63E's split-word addition and eight-byte table advance.</summary>
    internal static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        uint position = new SnesFixedPosition(state.ExtensionWhole, state.ExtensionFraction).RawValue;
        uint velocity = unchecked((uint)EnemyQuadraticSpeedDefinitions.ReadDisplacement(state.QuadraticSpeedByteOffset));
        uint next = unchecked(position + velocity);
        state.ExtensionWhole = SnesFixedPosition.WholeOf(unchecked((int)next));
        state.ExtensionFraction = unchecked((ushort)next);
        state.QuadraticSpeedByteOffset = unchecked((ushort)(
            state.QuadraticSpeedByteOffset + 8));
    }

    /// <summary>Ports $A8:A68A, including DEC-underflow expiration on frame 65.</summary>
    private void WaitAfterYappingMawRetraction(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (state.HasGrabbedSamus)
            PositionSamusInYappingMaw(state.Owner, state, samus);

        state.RetractedDelayTimer = state.RetractedDelayTimer.Decrement().Remaining;
        if (!state.RetractedDelayTimer.IsNegative || samus.DeathSequence.IsActive)
            return;

        samus.InputLocked = false;
        state.HasGrabbedSamus = false;
        state.GrabCooldown = NativeCountdown.Start(YappingMawGrabCooldownFrames);
        state.RetractedDelayTimer = NativeCountdown.Start(YappingMawRetractedDelayFrames);
        state.Function = YappingMawAiFunction.WaitingForSamus;
    }

    /// <summary>Ports the observable portions of CallSomeSamusCode(3) plus $A8:A665.</summary>
    private void PositionSamusInYappingMaw(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // Samus command three ($90:F152) drops any active grapple via $9B:C8C5;
        // its transitional pose must be committed before the Maw releases input. It normalizes
        // spin-jump/wall-jump bodies. The input handler itself was already replaced by the
        // touch callback; do not force unrelated falling/running poses to standing.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
        else
        {
            SamusMovementType movement = samus.PoseMetadata.ReadMovementKind(_room.LoadedBus);
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // CallSomeSamusCode(3) selects pose $01/$02 from the current direction,
                // then runs the ordinary pose metadata and animation initialization path.
                // Reusing those public ROM-backed helpers prevents a Yapping-Maw-only pose
                // approximation from drifting away from every other Samus transition.
                samus.Pose = samus.PoseMetadata.IsFacingLeft(_room.LoadedBus)
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose;
                samus.PoseMetadata.RefreshCollisionRadii(_room.LoadedBus);
                samus.Presentation.InitializeAnimation(_room.LoadedBus, initialFrame: 0);
                samus.CommitPoseHistory(_room.LoadedBus);
            }
        }

        samus.Position = samus.Position.WithWhole(slot.Position.Whole + state.HeldSamusOffset);
        _scrollCap.CapSamusScrollingSpeed(samus);
    }

    /// <summary>Custom touch callback $A8:A799: grab, never ordinary contact damage.</summary>
    private static void ResolveYappingMawTouch(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (!state.GrabCooldown.IsNegative || state.HasGrabbedSamus)
            return;

        state.GrabCooldown = NativeCountdown.Start(0);
        state.HasGrabbedSamus = true;
        samus.InputLocked = true;
    }

    /// <summary>
    /// Runs after common shot AI exactly like $A8:A7BD. Ice releases a living Maw's captive;
    /// death additionally destroys all four body eprojs and the root sprite-object slot.
    /// </summary>
    private static void ResolveYappingMawShotAfterCommon(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState? samus)
    {
        if (slot.Health != 0)
        {
            if (slot.FrozenTimer.IsZero)
                return;

            ReleaseSamusFromYappingMaw(state, samus);
            return;
        }

        foreach (RoomEnemyProjectileSlot? body in state.BodyProjectiles)
            body?.Clear();
        state.RootSpriteObject?.Clear();
        ReleaseSamusFromYappingMaw(state, samus);
    }

    /// <summary>Unlocks Samus input after retraction unless her death sequence owns it.</summary>
    private static void ReleaseSamusFromYappingMaw(
        YappingMawEnemyState state,
        SamusState? samus)
    {
        if (samus is not null && !samus.DeathSequence.IsActive)
            samus.InputLocked = false;
        state.HasGrabbedSamus = false;
    }

    /// <summary>Custom frozen tail $A8:A835-$A899 for all five auxiliary actors.</summary>
    internal static void RunYappingMawFrozen(
        RoomEnemySlot slot,
        YappingMawEnemyState state)
    {
        ushort paletteBits = SelectYappingMawFrozenPalette(
            state.OriginalPaletteBits,
            slot.FrozenTimer.RawValue);
        foreach (RoomEnemyProjectileSlot? body in state.BodyProjectiles)
        {
            if (body is not null)
            {
                body.GraphicsIndex = new SnesObjAttributeWord(body.GraphicsIndex)
                    .WithPaletteBits(paletteBits);
            }
        }

        if (state.RootSpriteObject is not null)
        {
            state.RootSpriteObject.GraphicsIndex =
                new SnesObjAttributeWord(state.RootSpriteObject.GraphicsIndex)
                    .WithPaletteBits(paletteBits);
        }
    }

    /// <summary>Flashes frozen palette six against original colors during the last 89 ticks.</summary>
    private static ushort SelectYappingMawFrozenPalette(
        ushort originalPaletteBits,
        ushort frozenTimer)
    {
        if (frozenTimer == 0)
            return originalPaletteBits;

        // Frozen palette six ($0C00) flashes back to the actor palette on alternating pairs
        // only during the final 89 ticks. The strict signed comparison excludes timer 90.
        if (unchecked((short)(frozenTimer - 90)) < 0 && (frozenTimer & 2) == 0)
            return originalPaletteBits;
        return EnemyFrozenPaletteDefinitions.PaletteBits;
    }

    /// <summary>Animation callback used by all six held-Samus offset opcodes.</summary>
    internal static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        int directionIndex)
    {
        state.HeldSamusOffset = YappingMawRomData.HeldSamusOffset(directionIndex);
    }

    /// <summary>Publishes the attack sound only when this Maw is visible.</summary>
    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            LastYappingMawSoundEffect = EnemySoundEffectDefinitions.YappingMawAttackSound;
    }

    /// <summary>Moves one of the four required body-link projectiles to its solved point.</summary>
    private static void PositionYappingMawBodyProjectile(
        YappingMawEnemyState state,
        int index,
        Position16<RoomPixels> position)
    {
        RoomEnemyProjectileSlot projectile = state.BodyProjectiles[index] ??
            throw new InvalidOperationException(
                $"Yapping Maw slot {state.Owner.SlotIndex} lost body projectile {index}.");
        projectile.Position = projectile.Position.WithWhole(position);
    }

    /// <summary>The root plus one solved link's signed word offsets, wrapping at sixteen bits.</summary>
    private static Position16<RoomPixels> YappingMawLinkPosition(YappingMawEnemyState state, int segment) =>
        state.Origin + state.SegmentOffsets[segment];

    /// <summary>Checks the Maw against the camera's 256-pixel horizontal and vertical spans.</summary>
    private static bool IsYappingMawPointOffScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(slot.Position.X.Whole - camera.X)) < 0 ||
        unchecked((short)(camera.X + 256 - slot.Position.X.Whole)) < 0 ||
        unchecked((short)(slot.Position.Y.Whole - camera.Y)) < 0 ||
        unchecked((short)(camera.Y + 256 - slot.Position.Y.Whole)) < 0;

    /// <summary>Ports EnemyFunc_ACA8 at $A0:ACA8 for the one family that consumes it.</summary>
    private static (ushort Distance, ushort Angle) CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        ushort deltaX = unchecked((ushort)(samus.X - origin.X));
        ushort absoluteX = EnemyMath.WrappedMagnitude(deltaX);
        if (unchecked((short)(absoluteX - 255)) >= 0)
            return (absoluteX, 0);

        ushort deltaY = unchecked((ushort)(samus.Y - origin.Y));
        ushort absoluteY = EnemyMath.WrappedMagnitude(deltaY);
        if (unchecked((short)(absoluteY - 255)) >= 0)
            return (absoluteY, 0);

        ushort positiveQuadrantAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)absoluteX), unchecked((short)absoluteY)));
        ushort distance = unchecked((ushort)(
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitNegativeSineProduct(
                positiveQuadrantAngle,
                absoluteX))) +
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitCosineProduct(
                positiveQuadrantAngle,
                absoluteY)))));
        ushort signedAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)deltaX), unchecked((short)deltaY)));
        return (distance, signedAngle);
    }

    /// <summary>$A8:A73E: X is the Y helper with a minus-64 angle adjustment.</summary>
    internal static ushort CalculateYappingMawX(ushort angle, ushort length) =>
        CalculateYappingMawY(unchecked((ushort)(angle - 64)), length);

    /// <summary>
    /// Ports $A8:A742 exactly, including the high-byte sine approximation and the unusual
    /// negative-product construction. Only length's low byte reaches WRMPYB.
    /// </summary>
    internal static ushort CalculateYappingMawY(ushort angle, ushort length)
    {
        short signedSample = EnemyTrigonometryTables.SignedSixteenBitSine(
            unchecked((byte)-angle));
        bool negative = signedSample < 0;
        ushort magnitude = unchecked((ushort)(negative ? -signedSample : signedSample));
        ushort product = unchecked((ushort)((SnesLittleEndian.HighByte(magnitude)) * unchecked((byte)length)));
        if (product == 0)
            return 0;

        if (!negative)
            return unchecked((ushort)(2 * (SnesLittleEndian.HighByte(product))));

        ushort negatedProduct = unchecked((ushort)-product);
        return unchecked((ushort)((2 * (SnesLittleEndian.HighByte(negatedProduct))) | EnemyWordMasks.SignExtendedHighByte));
    }

    /// <summary>Requires the state installed by Maw initialization for a given enemy slot.</summary>
    internal YappingMawEnemyState RequireYappingMawState(RoomEnemySlot slot) =>
        _yappingMawStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Yapping Maw slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>Routes bomb hits through the Maw's shared shot callback.</summary>
    internal void RegisterYappingMawBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, RequireYappingMawState(c.Enemy), c.Samus),
        });
    }

    /// <summary>Registers the Maw initializer and its auxiliary actor setup.</summary>
    internal void RegisterYappingMawInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_YappingMaw, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeYappingMaw(slot);
        });
    }

    /// <summary>Routes Maw main AI through its extension, hold, and retraction phases.</summary>
    internal void RegisterYappingMawMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_YappingMaw, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw, (slot, frame, nmiFrameCounter8) =>
        {
            RunYappingMawMain(
                slot,
                RequireYappingMawState(slot),
                frame.Samus,
                frame.Camera);
        });
    }

    /// <summary>Runs the Yapping Maw's bites and cooldowns as typed programs (#641).</summary>
    internal void RegisterYappingMawTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireYappingMawState(slot);
            AnimationProgramCursor<YappingMawProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, YappingMawAnimations.Programs, ref cursor, YappingMawFrameDefinitions.Spritemap,
                    (cue, _) => ApplyYappingMawCue(state, cue));
            state.Program = cursor;
        });


    /// <summary>Handles frozen release and death cleanup after common shot damage.</summary>
    internal void RegisterYappingMawShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = (c, _) => ResolveYappingMawShotAfterCommon(c.Enemy, RequireYappingMawState(c.Enemy), c.Samus),
        });
    }

    /// <summary>Registers contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal void RegisterYappingMawTouch(EnemyAiTable table)
    {
        // $A8:A799 replaces common contact damage entirely. A qualifying overlap transfers Samus input/position
        // ownership to the mouth state machine.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.YappingMawTouch,
            context => ResolveYappingMawTouch(RequireYappingMawState(context.Slot), context.Samus));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastYappingMawSoundEffect = null;
        Array.Clear(_yappingMawStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastYappingMawSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastYappingMawSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }

    /// <summary>Routes frozen palette updates to all body links and the root sprite object.</summary>
    internal void RegisterYappingMawFrozenAi(EnemyAiTable table) =>
        table.AddFrozen(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw,
            slot => RunYappingMawFrozen(slot, RequireYappingMawState(slot)));
}
