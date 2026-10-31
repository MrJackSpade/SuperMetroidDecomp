using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
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

    /// <summary>Receives the species' end-of-update sound requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Maw AI to room, body-link, sprite-object, and Samus camera services.</summary>
    internal YappingMawBehavior(EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects, SamusScrollCap scrollCap, EnemyAudioQueue audio)
    {
        _audio = audio;
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

    /// <summary>Ports <c>YappingMaw_Init</c> at $A8:A148.</summary>
    internal void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Function = YappingMawAiFunction.WaitingForSamus,
            Origin = slot.Position.Whole,
            ActivationDistance = slot.SpawnSettings<YappingMawSpawnSettings>().ActivationDistance,
            RetractedDelayTimer = WrappingCountdown.Start(YappingMawRetractedDelayFrames),
            OriginalColouring = slot.Colouring,
        };
        slot.SetSpeciesState(state);

        state.Program = AnimationProgramCursor<YappingMawProgram>.Start(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
            ? YappingMawProgram.AttackingDown
            : YappingMawProgram.AttackingUp);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

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
                slot.Graphics);
            body.Position = body.Position.WithWhole(slot.Position.Whole);
            body.InstallAnimation(new YappingMawBodyProjectileAnimation(slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
                ? YappingMawBodyProjectileProgram.FacingDown
                : YappingMawBodyProjectileProgram.FacingUp));
            state.BodyProjectiles[parameter] = body;
        }

        // CreateSpriteAtPos uses object number 56 for parameter zero and 57 otherwise. Its
        // returned native index is represented by the retained physical slot reference.
        state.RootSpriteObject = _spriteObjects.SpawnRoomSpriteObject(
            new(slot.Position.X.Whole, unchecked((ushort)(slot.Position.Y.Whole + (slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down ? -8 : 8)))),
            slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Down
                ? RoomSpriteObjectKind.YappingMawRootVariantZero
                : RoomSpriteObjectKind.YappingMawRootVariantOne,
            slot.Graphics);

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
                    $"Yapping Maw function {state.Function} is not translated.");
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
        (int distance, Bearing bearing) = CalculateYappingMawTarget(
            state.Origin,
            samus.Position.Whole);
        state.DistanceToSamus = distance;

        if (distance < YappingMawGrabSafetyDistance)
        {
            // A target already inside 32 pixels cannot safely trigger an extension and has
            // its grab gate refreshed to 48 every frame. This is why the cooldown must wrap.
            state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);
            return;
        }

        if (!state.IsWithinActivationDistance(distance))
            return;

        // Native BMI skips the assignment for a shorter target. Inflating a short
        // distance instead makes the mouth overshoot the position it aimed at.
        if (state.DistanceToSamus >= YappingMawAttackGeometry.MaximumTargetDistance)
            state.DistanceToSamus = YappingMawAttackGeometry.MaximumTargetDistance;

        state.AimAngle = bearing;
        state.Function = YappingMawAiFunction.BeginExtension;
    }

    /// <summary>Ports the one-frame setup function at $A8:A28C.</summary>
    internal static void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.Extension = SignedSixteenSixteen.Zero;
        state.QuadraticSpeedRecord = 0;
        state.QuadraticSpeedNegative = false;
        state.SegmentRadius = state.DistanceToSamus / 2;
        state.CurlAngle = state.AimAngle.ToTableAngle().ToAngle();
        state.MirroredBend = state.CurlAngle.TableIndex >= RotationAngle.HalfTurn.TableIndex;

        state.Baseline = new Offset16<RoomPixels>(
            unchecked((short)CalculateYappingMawX(ByteAngle.HalfTurn, checked((ushort)state.SegmentRadius))),
            unchecked((short)CalculateYappingMawY(
                ByteAngle.HalfTurn,
                checked((ushort)(state.SegmentRadius / 2)))));

        // Eight 32-unit sectors, each centred on its direction: the aim is turned a half sector before quantizing.
        int direction = unchecked((byte)(state.AimAngle.RawValue + 16)) / 32;
        YappingMawAim aim = YappingMawAnimationDefinitions.Aims.At(direction);
        state.Aim = aim;
        state.Program = AnimationProgramCursor<YappingMawProgram>.Start(YappingMawBites.AttackForDirection(aim));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(direction);
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
        int bendStep = checked((ushort)(state.Extension.WholePixels / 4));
        // $A8:A532 corrects the eight offsets in one chain, link by link and X before Y: an offset whose correction
        // wraps it past the last pixel of its word moves the next offset in the chain one pixel further.
        int wrappedIntoNext = 0;
        for (int segment = 0; segment < 4; segment++)
        {
            int multiple = unchecked((ushort)(bendStep * (segment + 1)));
            int linkAngle = state.MirroredBend
                ? unchecked((ushort)(128 + multiple))
                : unchecked((ushort)(128 - multiple));

            int rawX = unchecked((ushort)(
                CalculateYappingMawX(new ByteAngle(unchecked((byte)(linkAngle))), checked((ushort)state.SegmentRadius)) - state.Baseline.X));
            int rawY = unchecked((ushort)(
                CalculateYappingMawY(
                    new ByteAngle(unchecked((byte)(linkAngle))),
                    checked((ushort)(state.SegmentRadius / 2))) - state.Baseline.Y));

            // Variables 27..2E rotate a correction whose length is the LOW BYTE of raw X.
            // Passing raw Y here is an attractive cleanup and is wrong; the cartridge's
            // lopsided curve is a direct consequence of reusing raw X for both components.
            int correctionX = unchecked((ushort)(
                CalculateYappingMawX(state.CurlAngle.ToByteAngle(), rawX) -
                CalculateYappingMawX(new ByteAngle(0), rawX)));
            int correctionY = unchecked((ushort)(
                CalculateYappingMawY(state.CurlAngle.ToByteAngle(), rawX) -
                CalculateYappingMawY(new ByteAngle(0), rawX)));
            state.Corrections[segment] = new Offset16<RoomPixels>(
                unchecked((short)correctionX), unchecked((short)correctionY));
            int segmentX = Corrected(rawX, correctionX);
            int segmentY = Corrected(rawY, correctionY);
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
                state.Extension = SignedSixteenSixteen.FromParts(checked(YappingMawMaximumCurl), 0);
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
        state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);

        // The six retracted lists are not an eight-way table. Straight-ish sectors share a
        // list, while the vertical sectors at byte offsets four and twelve have dedicated
        // art. Preserve parameter-zero's early-return quirk: on its shared-list branches it
        // skips the final held-Samus placement on this one transition frame.
        bool skipHeldPlacement = false;
        if (slot.SpawnSettings<YappingMawSpawnSettings>().Attack == VerticalDirection.Up)
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.Aim switch
            {
                YappingMawAim.Right => YappingMawProgram.CooldownUpRight,
                YappingMawAim.Left => YappingMawProgram.CooldownUpLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawProgram.CooldownUp,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            });
        }
        else
        {
            state.Program = AnimationProgramCursor<YappingMawProgram>.Start(state.Aim switch
            {
                YappingMawAim.Right => YappingMawProgram.CooldownDownRight,
                YappingMawAim.Left => YappingMawProgram.CooldownDownLeft,
                YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
                    YappingMawAim.DownLeft or YappingMawAim.UpLeft => YappingMawProgram.CooldownDown,
                _ => throw new InvalidOperationException($"Yapping Maw aim {state.Aim} is not an aim."),
            });
            skipHeldPlacement = state.Aim is not (YappingMawAim.Right or YappingMawAim.Left);
        }
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        if (!skipHeldPlacement && state.HasGrabbedSamus)
            PositionSamusInYappingMaw(slot, state, samus);

        // One offset of the chain, a word, moved by its correction, a word, and by one pixel when the previous offset
        // wrapped; records whether this one wraps in turn.
        int Corrected(int offset, int correction)
        {
            int sum = checked((ushort)offset) + checked((ushort)correction) + wrappedIntoNext;
            wrappedIntoNext = sum > ushort.MaxValue ? 1 : 0;
            return unchecked((ushort)sum);
        }
    }

    /// <summary>Ports $A8:A63E: adds the current quadratic speed to the curl, carrying the fraction, then advances the record.</summary>
    internal static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        state.Extension += SignedSixteenSixteen.FromRaw(EnemyQuadraticSpeeds.Row(checked((ushort)state.QuadraticSpeedRecord)).Toward(state.QuadraticSpeedNegative ? EnemySpeedSign.Negative : EnemySpeedSign.Positive));
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
        state.GrabCooldown = WrappingCountdown.Start(YappingMawGrabCooldownFrames);
        state.RetractedDelayTimer = WrappingCountdown.Start(YappingMawRetractedDelayFrames);
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
            SamusMovementType movement = SamusPoseMetadata.ReadMovementKind(samus);
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // CallSomeSamusCode(3) selects pose $01/$02 from the current direction,
                // then runs the ordinary pose metadata and animation initialization path.
                // Reusing those public ROM-backed helpers prevents a Yapping-Maw-only pose
                // approximation from drifting away from every other Samus transition.
                samus.Pose = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose;
                SamusPoseMetadata.RefreshCollisionRadii(samus);
                SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
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

        state.GrabCooldown = WrappingCountdown.Start(0);
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
        SpriteColouring colouring = SelectYappingMawFrozenColouring(
            state.OriginalColouring,
            slot.FrozenTimer.RemainingFrames);
        foreach (RoomEnemyProjectileSlot? body in state.BodyProjectiles)
        {
            if (body is not null)
                body.Graphics = body.Graphics.In(colouring);
        }

        if (state.RootSpriteObject is not null)
            state.RootSpriteObject.Graphics = state.RootSpriteObject.Graphics.In(colouring);
    }

    /// <summary>Flashes the frozen colours against the original colours during the last 89 ticks.</summary>
    private static SpriteColouring SelectYappingMawFrozenColouring(
        SpriteColouring originalColouring,
        int frozenTimer)
    {
        if (frozenTimer == 0)
            return originalColouring;

        // The frozen colours flash back to the actor's own on alternating pairs only
        // during the final 89 ticks. The strict signed comparison excludes timer 90.
        if (unchecked((short)(frozenTimer - 90)) < 0 && (frozenTimer & 2) == 0)
            return originalColouring;
        return SharedSpriteColouring.Frozen;
    }

    /// <summary>Animation callback used by all six held-Samus offset opcodes.</summary>
    internal static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        int directionIndex)
    {
        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(directionIndex);
    }

    /// <summary>Publishes the attack sound only when this Maw is visible.</summary>
    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            _audio.RequestSoundAtUpdateEnd(EnemySoundSource.YappingMaw, EnemySoundEffectDefinitions.YappingMawAttackSound, priority: SoundRequestPriority.Normal);
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
    private static YappingMawTarget CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        int deltaX = unchecked((ushort)(samus.X - origin.X));
        int absoluteX = EnemyMath.WrappedMagnitude(deltaX);
        if (unchecked((short)(absoluteX - 255)) >= 0)
            return new YappingMawTarget(absoluteX, default);

        int deltaY = unchecked((ushort)(samus.Y - origin.Y));
        int absoluteY = EnemyMath.WrappedMagnitude(deltaY);
        if (unchecked((short)(absoluteY - 255)) >= 0)
            return new YappingMawTarget(absoluteY, default);

        ByteAngle positiveQuadrantAngle = EnemyMath.CalculateBearing(
            new(unchecked((short)absoluteX), unchecked((short)absoluteY))).ReinterpretAsTableAngle();
        int distance = unchecked((ushort)(
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitNegativeSineProduct(
                positiveQuadrantAngle,
                absoluteX))) +
            EnemyMath.WrappedMagnitude(unchecked((ushort)EnemyMath.ReadEightBitCosineProduct(
                positiveQuadrantAngle,
                absoluteY)))));
        return new YappingMawTarget(distance, EnemyMath.CalculateBearing(new(unchecked((short)deltaX), unchecked((short)deltaY))));
    }

    /// <summary>$A8:A73E: X is the Y helper with a minus-64 angle adjustment.</summary>
    internal static ushort CalculateYappingMawX(ByteAngle angle, int length) =>
        CalculateYappingMawY(angle.AddTableUnits(-ByteAngle.QuarterTurn.RawValue), length);

    /// <summary>
    /// Ports $A8:A742 exactly, including the high-byte sine approximation and the unusual
    /// negative-product construction. Only length's low byte reaches WRMPYB.
    /// </summary>
    internal static ushort CalculateYappingMawY(ByteAngle angle, int length)
    {
        int signedSample = Trigonometry.SignedSixteenBitSine(
            new ByteAngle(unchecked((byte)-angle.RawValue)));
        bool negative = signedSample < 0;
        int magnitude = unchecked((ushort)(negative ? -signedSample : signedSample));
        int product = unchecked((ushort)(UnsignedEightEight.Whole(magnitude) * unchecked((byte)length)));
        if (product == 0)
            return 0;

        if (!negative)
            return unchecked((ushort)(2 * UnsignedEightEight.Whole(product)));

        int negatedProduct = unchecked((ushort)-product);
        return EnemyMath.FillHighByte(2 * UnsignedEightEight.Whole(negatedProduct));
    }

    /// <summary>Routes bomb hits through the Maw's shared shot callback.</summary>
    internal static void AddYappingMawBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.YappingMaw &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<YappingMawEnemyState>(), c.Samus),
        });
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
    internal static void AddYappingMawShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.YappingMaw &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.YappingMawShot)
        {
            AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<YappingMawEnemyState>(), c.Samus),
        });
    }

    /// <summary>Registers contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal static void AddYappingMawTouch(EnemySharedReactions table)
    {
        // $A8:A799 replaces common contact damage entirely. A qualifying overlap transfers Samus input/position
        // ownership to the mouth state machine.
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.YappingMaw &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.YappingMawTouch,
            context => ResolveYappingMawTouch(context.Slot.RequireSpeciesState<YappingMawEnemyState>(), context.Samus));
    }

    /// <summary>Routes frozen palette updates to all body links and the root sprite object.</summary>
    internal static void AddYappingMawFrozenAi(EnemySharedReactions table) =>
        table.AddFrozen(slot => slot.DefinitionId == EnemyDefinitionId.YappingMaw,
            slot => RunYappingMawFrozen(slot, slot.RequireSpeciesState<YappingMawEnemyState>()));
}
