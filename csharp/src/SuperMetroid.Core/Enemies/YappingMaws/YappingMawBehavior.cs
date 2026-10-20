using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class YappingMawBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.YappingMaw];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new YappingMawEntity(this, slot, common);

    /// <summary>Creates the four auxiliary body-link projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns the root sprite object accompanying the body links.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Caps camera movement while Samus is held at the Maw's mouth.</summary>
    private readonly SamusScrollCap _scrollCap;

    /// <summary>Connects Maw AI to room, body-link, sprite-object, and Samus camera services.</summary>
    internal YappingMawBehavior(EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects, SamusScrollCap scrollCap)
    {
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
    /// <summary>Maximum extension curl admitted by the four-link solver, in whole angle units.</summary>
    private const int YappingMawMaximumCurl = 128;
    /// <summary>Cooldown after Samus is released before the Maw can grab again.</summary>
    private const ushort YappingMawGrabCooldownFrames = 48;
    /// <summary>Delay after full retraction before Samus input is released.</summary>
    private const ushort YappingMawRetractedDelayFrames = 64;

    /// <summary>Most recent library-two $2F request emitted by animation opcode $A133.</summary>
    public SoundEffectId? LastYappingMawSoundEffect { get; private set; }

    /// <summary>Ports <c>YappingMaw_Init</c> at $A8:A148.</summary>
    internal void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Function = YappingMawAiFunction.WaitingForSamus,
            Origin = slot.Position.Whole,
            ActivationDistance = slot.Spawn.Population.Parameter1,
            RetractedDelayTimer = NativeCountdown.Start(YappingMawRetractedDelayFrames),
            OriginalPalette = SnesObjAttributeWord.FromPaletteBits(new SnesObjAttributeWord(checked((ushort)slot.PaletteIndex)).PaletteBits),
        };
        slot.SetSpeciesState(state);

        state.Program = AnimationProgramCursor<YappingMawProgram>.Start(slot.Spawn.Population.Parameter2 == 0
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
            body.InstallAnimation(new YappingMawBodyProjectileAnimation(slot.Spawn.Population.Parameter2 == 0
                ? YappingMawBodyProjectileProgram.FacingDown
                : YappingMawBodyProjectileProgram.FacingUp));
            state.BodyProjectiles[parameter] = body;
        }

        // CreateSpriteAtPos uses object number 56 for parameter zero and 57 otherwise. Its
        // returned native index is represented by the retained physical slot reference.
        state.RootSpriteObject = _spriteObjects.SpawnRoomSpriteObject(
            new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + (slot.Spawn.Population.Parameter2 == 0 ? -8 : 8)))),
            slot.Spawn.Population.Parameter2 == 0
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
        (ushort distance, SnesBearing bearing) = CalculateYappingMawTarget(
            state.Origin,
            samus.Position.Whole);
        state.DistanceToSamus = distance;

        if (distance < YappingMawGrabSafetyDistance)
        {
            // A target already inside 32 pixels cannot safely trigger an extension and has
            // its grab gate refreshed to 48 every frame. This is why the cooldown must wrap.
            state.GrabCooldown = NativeCountdown.Start(YappingMawGrabCooldownFrames);
            return;
        }

        if (!state.IsWithinActivationDistance(distance))
            return;

        // Native BMI skips the assignment for a shorter target. Inflating a short
        // distance instead makes the mouth overshoot the position it aimed at.
        if (state.DistanceToSamus >= YappingMawRomData.MaximumTargetDistance)
            state.DistanceToSamus = YappingMawRomData.MaximumTargetDistance;

        state.AimAngle = bearing;
        state.Function = YappingMawAiFunction.BeginExtension;
    }

    /// <summary>Ports the one-frame setup function at $A8:A28C.</summary>
    internal static void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.Extension = SnesSignedSixteenSixteen.Zero;
        state.QuadraticSpeedRecord = 0;
        state.QuadraticSpeedNegative = false;
        state.SegmentRadius = state.DistanceToSamus / 2;
        state.CurlAngle = state.AimAngle.ToTableAngle().ToAngle();
        state.MirroredBend = state.CurlAngle.TableIndex >= SnesAngle.HalfTurn.TableIndex;

        state.Baseline = new Offset16<RoomPixels>(
            unchecked((short)CalculateYappingMawX(SnesByteAngle.HalfTurn, checked((ushort)state.SegmentRadius))),
            unchecked((short)CalculateYappingMawY(
                SnesByteAngle.HalfTurn,
                checked((ushort)(state.SegmentRadius / 2)))));

        // Eight 32-unit sectors, each centred on its direction: the aim is turned a half sector before quantizing.
        int direction = unchecked((byte)(state.AimAngle.RawValue + 16)) / 32;
        state.DirectionIndex = direction;
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
        // The function only runs while the curl's whole part is in [0, YappingMawMaximumCurl]: setup starts it at zero,
        // the clamp below caps it, and the update that carries it negative leaves this function. In that range a
        // quarter of the signed whole part equals the cartridge's two logical right shifts of the word; the checked
        // conversion fails loudly if that range is ever violated.
        ushort bendStep = checked((ushort)(state.Extension.WholePixels / 4));
        // $A8:A532 clears carry once, then adds the eight correction words in segment
        // order (X then Y) with ADC alone, so each addition's carry adds one to the next.
        bool carry = false;
        for (int segment = 0; segment < 4; segment++)
        {
            ushort multiple = unchecked((ushort)(bendStep * (segment + 1)));
            ushort linkAngle = state.MirroredBend
                ? unchecked((ushort)(128 + multiple))
                : unchecked((ushort)(128 - multiple));

            ushort rawX = unchecked((ushort)(
                CalculateYappingMawX(new SnesByteAngle(unchecked((byte)(linkAngle))), checked((ushort)state.SegmentRadius)) - state.Baseline.X));
            ushort rawY = unchecked((ushort)(
                CalculateYappingMawY(
                    new SnesByteAngle(unchecked((byte)(linkAngle))),
                    checked((ushort)(state.SegmentRadius / 2))) - state.Baseline.Y));

            // Variables 27..2E rotate a correction whose length is the LOW BYTE of raw X.
            // Passing raw Y here is an attractive cleanup and is wrong; the cartridge's
            // lopsided curve is a direct consequence of reusing raw X for both components.
            ushort correctionX = unchecked((ushort)(
                CalculateYappingMawX(state.CurlAngle.ToByteAngle(), rawX) -
                CalculateYappingMawX(new SnesByteAngle(0), rawX)));
            ushort correctionY = unchecked((ushort)(
                CalculateYappingMawY(state.CurlAngle.ToByteAngle(), rawX) -
                CalculateYappingMawY(new SnesByteAngle(0), rawX)));
            state.Corrections[segment] = new Offset16<RoomPixels>(
                unchecked((short)correctionX), unchecked((short)correctionY));
            // X is added before Y so its carry feeds the Y addition.
            ushort segmentX = SnesCarry.AddWithCarry(rawX, correctionX, ref carry);
            ushort segmentY = SnesCarry.AddWithCarry(rawY, correctionY, ref carry);
            state.SegmentOffsets[segment] = new Offset16<RoomPixels>(
                unchecked((short)segmentX), unchecked((short)segmentY));
        }

        slot.Position = slot.Position.WithWhole(YappingMawLinkPosition(state, 3));
        AddYappingMawQuadraticSpeed(state);

        if (state.Extension.WholePixels >= 0)
        {
            // A nonnegative whole part minus 128 cannot wrap, so the cartridge's signed difference test is this
            // ordinary comparison.
            if (state.Extension.WholePixels >= YappingMawMaximumCurl)
            {
                state.Extension = SnesSignedSixteenSixteen.FromParts(checked(YappingMawMaximumCurl), 0);
                // $A8:A4F2 adds four bytes after the ordinary eight-byte record advance: the same record's
                // negative half, or the next record's positive half from a negative one.
                if (state.QuadraticSpeedNegative)
                    state.QuadraticSpeedRecord++;
                state.QuadraticSpeedNegative = !state.QuadraticSpeedNegative;
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
        if (slot.Spawn.Population.Parameter2 != 0)
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.DirectionIndex switch
            {
                2 => YappingMawProgram.CooldownUpRight,
                6 => YappingMawProgram.CooldownUpLeft,
                _ => YappingMawProgram.CooldownUp,
            });
        }
        else
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.DirectionIndex switch
            {
                2 => YappingMawProgram.CooldownDownRight,
                6 => YappingMawProgram.CooldownDownLeft,
                _ => YappingMawProgram.CooldownDown,
            });
            skipHeldPlacement = state.DirectionIndex is not (2 or 6);
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        if (!skipHeldPlacement && state.HasGrabbedSamus)
            PositionSamusInYappingMaw(slot, state, samus);
    }

    /// <summary>Ports $A8:A63E: adds the current quadratic speed to the curl, carrying the fraction, then advances the record.</summary>
    internal static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        state.Extension += SnesSignedSixteenSixteen.FromRaw(EnemyMath.ReadQuadraticEnemySpeed(
            checked((ushort)state.QuadraticSpeedRecord), state.QuadraticSpeedNegative));
        state.QuadraticSpeedRecord++;
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

        samus.Control.Unlock();
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
            SamusMovementType movement = samus.PoseMetadata.ReadMovementKind();
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // CallSomeSamusCode(3) selects pose $01/$02 from the current direction,
                // then runs the ordinary pose metadata and animation initialization path.
                // Reusing those public ROM-backed helpers prevents a Yapping-Maw-only pose
                // approximation from drifting away from every other Samus transition.
                samus.Pose = samus.PoseMetadata.IsFacing(SamusFacingDirection.Left)
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose;
                samus.PoseMetadata.RefreshCollisionRadii();
                samus.Presentation.InitializeAnimation(initialFrame: 0);
                samus.CommitPoseHistory();
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
        samus.Control.LockHandlers();
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
            samus.Control.Unlock();
        state.HasGrabbedSamus = false;
    }

    /// <summary>Custom frozen tail $A8:A835-$A899 for all five auxiliary actors.</summary>
    internal static void RunYappingMawFrozen(
        RoomEnemySlot slot,
        YappingMawEnemyState state)
    {
        ushort paletteBits = SelectYappingMawFrozenPalette(
            state.OriginalPalette.PaletteBits,
            slot.FrozenTimer.RawValue);
        foreach (RoomEnemyProjectileSlot? body in state.BodyProjectiles)
        {
            if (body is not null)
            {
                body.GraphicsIndex = new SnesObjAttributeWord(checked((ushort)body.GraphicsIndex))
                    .WithPaletteBits(paletteBits);
            }
        }

        if (state.RootSpriteObject is not null)
        {
            state.RootSpriteObject.Attributes =
                state.RootSpriteObject.Attributes.WithPaletteBits(paletteBits);
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
            LastYappingMawSoundEffect = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EnemySoundEffectDefinitions.YappingMawAttackSound);
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
    private static (ushort Distance, SnesBearing Bearing) CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        ushort deltaX = unchecked((ushort)(samus.X - origin.X));
        ushort absoluteX = EnemyMath.WrappedMagnitude(deltaX);
        if (unchecked((short)(absoluteX - 255)) >= 0)
            return (absoluteX, default);

        ushort deltaY = unchecked((ushort)(samus.Y - origin.Y));
        ushort absoluteY = EnemyMath.WrappedMagnitude(deltaY);
        if (unchecked((short)(absoluteY - 255)) >= 0)
            return (absoluteY, default);

        SnesByteAngle positiveQuadrantAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)absoluteX), unchecked((short)absoluteY))).ReinterpretAsTableAngle();
        ushort distance = unchecked((ushort)(
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitNegativeSineProduct(
                positiveQuadrantAngle,
                absoluteX))) +
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitCosineProduct(
                positiveQuadrantAngle,
                absoluteY)))));
        return (distance, EnemyMath.CalculateCartridgeAngle(new(unchecked((short)deltaX), unchecked((short)deltaY))));
    }

    /// <summary>$A8:A73E: X is the Y helper with a minus-64 angle adjustment.</summary>
    internal static ushort CalculateYappingMawX(SnesByteAngle angle, ushort length) =>
        CalculateYappingMawY(angle.AddTableUnits(-SnesByteAngle.QuarterTurn.RawValue), length);

    /// <summary>
    /// Ports $A8:A742 exactly, including the high-byte sine approximation and the unusual
    /// negative-product construction. Only length's low byte reaches WRMPYB.
    /// </summary>
    internal static ushort CalculateYappingMawY(SnesByteAngle angle, ushort length)
    {
        short signedSample = EnemyTrigonometryTables.SignedSixteenBitSine(
            new SnesByteAngle(unchecked((byte)-angle.RawValue)));
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

    /// <summary>Routes bomb hits through the Maw's shared shot callback.</summary>
    internal static void RegisterYappingMawBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<YappingMawEnemyState>(), c.Samus),
        });
    }

    /// <summary>Ports YappingMaw's initialization AI.</summary>
    internal void RunYappingMawInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeYappingMaw(slot);
    }

    /// <summary>Ports YappingMaw's main AI.</summary>
    internal void RunYappingMawMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunYappingMawMain(
            slot,
            slot.RequireSpeciesState<YappingMawEnemyState>(),
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances YappingMaw's typed animation.</summary>
    internal void AdvanceYappingMawTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, YappingMawAnimations.Programs, YappingMawFrameDefinitions.ShownFrames.Of,
            static (YappingMawEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyYappingMawCue(state, cue));

    /// <summary>Handles frozen release and death cleanup after common shot damage.</summary>
    internal static void RegisterYappingMawShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = (c, _) => ResolveYappingMawShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<YappingMawEnemyState>(), c.Samus),
        });
    }

    /// <summary>Registers contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal static void RegisterYappingMawTouch(EnemySharedReactions table)
    {
        // $A8:A799 replaces common contact damage entirely. A qualifying overlap transfers Samus input/position
        // ownership to the mouth state machine.
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.YappingMawTouch,
            context => ResolveYappingMawTouch(context.Slot.RequireSpeciesState<YappingMawEnemyState>(), context.Samus));
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastYappingMawSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastYappingMawSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>Routes frozen palette updates to all body links and the root sprite object.</summary>
    internal static void RegisterYappingMawFrozenAi(EnemySharedReactions table) =>
        table.AddFrozen(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw,
            slot => RunYappingMawFrozen(slot, slot.RequireSpeciesState<YappingMawEnemyState>()));
}
