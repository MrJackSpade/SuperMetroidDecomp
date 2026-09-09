using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 function word stored in Yapping Maw's native variable A. Keeping the
/// cartridge addresses makes a debugger trace line up with WRAM and with $A8:A211's jump
/// dispatcher without a second, invented host state numbering scheme.
/// </summary>
public enum YappingMawAiFunction : ushort
{
    WaitingForSamus = 0xa235,
    BeginExtension = 0xa28c,
    ExtendingOrRetracting = 0xa445,
    RetractedDelay = 0xa68a,
}

/// <summary>
/// Typed projection of the unusually large Yapping Maw extension blocks at $7E:7800,
/// $7E:8000, and $7E:8800. Names describe the native words' actual consumers; arrays keep
/// the four identical tongue-link calculations inspectable without four copies of every
/// property.
/// </summary>
public sealed class YappingMawEnemyState
{
    internal YappingMawEnemyState(RoomEnemySlot owner) => Owner = owner;

    public RoomEnemySlot Owner { get; }
    public YappingMawAiFunction Function { get; internal set; }

    /// <summary>The immovable population coordinate retained in native variables 0C/0D.</summary>
    public ushort OriginX { get; internal set; }
    public ushort OriginY { get; internal set; }

    /// <summary>
    /// Final signed word offsets for the four successive tongue points. Link zero is nearest
    /// the root, link three is the mouth; the first three also position decorative eprojs.
    /// </summary>
    public ushort[] SegmentXOffsets { get; } = new ushort[4];
    public ushort[] SegmentYOffsets { get; } = new ushort[4];

    /// <summary>Approximate bank-$A0 distance used by the strict activation comparisons.</summary>
    public ushort DistanceToSamus { get; internal set; }

    /// <summary>Half of the clamped target distance, used as the tongue's X radius.</summary>
    public ushort SegmentRadius { get; internal set; }

    /// <summary>Zero-up, clockwise byte angle returned by CalculateAngleFromXY.</summary>
    public ushort AimAngle { get; internal set; }

    /// <summary>Byte-wrapped 64-minus-aim angle used to rotate the curled link corrections.</summary>
    public ushort CurlAngle { get; internal set; }

    /// <summary>Native variable 2F: zero bends one way, one bends the mirrored way.</summary>
    public ushort MirroredBend { get; internal set; }

    /// <summary>Reference X/Y samples subtracted before rotating each link.</summary>
    public ushort BaselineX { get; internal set; }
    public ushort BaselineY { get; internal set; }

    /// <summary>
    /// Native B/C signed 16.16 curl amount. It accelerates past +128, wraps negative, and
    /// thereby signals that the retracting tongue has completed its full ROM curve.
    /// </summary>
    public ushort ExtensionWhole { get; internal set; }
    public ushort ExtensionFraction { get; internal set; }

    /// <summary>Native D byte offset into CommonEnemySpeeds_QuadraticallyIncreasing.</summary>
    public ushort QuadraticSpeedByteOffset { get; internal set; }

    /// <summary>Native E: 64-frame hold after the tongue fully retracts.</summary>
    public ushort RetractedDelayTimer { get; internal set; }

    /// <summary>Native F copied from population parameter one.</summary>
    public ushort ActivationDistance { get; internal set; }

    /// <summary>Native variable 30: the mouth currently owns Samus's input/position.</summary>
    public bool HasGrabbedSamus { get; internal set; }

    /// <summary>Animation-selected offsets added to the moving mouth while Samus is held.</summary>
    public ushort HeldSamusXOffset { get; internal set; }
    public ushort HeldSamusYOffset { get; internal set; }

    /// <summary>Even byte offset zero through fourteen selecting one of eight facing lists.</summary>
    public ushort DirectionTableByteOffset { get; internal set; }

    /// <summary>
    /// Native variable 35. Main AI decrements it unconditionally; touch may grab only while
    /// its sign bit is set. Values 48 and zero therefore form real cooldown states, not bools.
    /// </summary>
    public ushort GrabCooldown { get; internal set; }

    /// <summary>Result of the point-based $A0:AD70 viewport test sampled before movement.</summary>
    public bool IsOffScreen { get; internal set; }

    /// <summary>Original OBJ palette bits restored after frozen flashing.</summary>
    public ushort OriginalPaletteBits { get; internal set; }

    /// <summary>The four physical bank-$86 body-link slots, ordered root to mouth.</summary>
    public RoomEnemyProjectileSlot?[] BodyProjectiles { get; } =
        new RoomEnemyProjectileSlot?[4];

    /// <summary>The bank-$B4 root/base object selected by population parameter two.</summary>
    public RoomSpriteObjectSlot? RootSpriteObject { get; internal set; }

    // Native variables 27..2E hold one correction pair per link. They are public through
    // read-only list views because they are valuable breakpoint watches, but only bank-$A8
    // calculations are allowed to mutate the backing arrays.
    internal ushort[] CorrectionX { get; } = new ushort[4];
    internal ushort[] CorrectionY { get; } = new ushort[4];
    public IReadOnlyList<ushort> SegmentXCorrections => CorrectionX;
    public IReadOnlyList<ushort> SegmentYCorrections => CorrectionY;
}

/// <summary>
/// Complete translation of retail Yapping Maw definition $E7BF at $A8:A0C7-$A899. The
/// mouth is an ordinary enemy slot, its root is a bank-$B4 sprite object, and its four body
/// links are bank-$86 enemy projectiles. All three pools remain separate here just as they
/// are on hardware; the state object only retains their physical native relationships.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort YappingMawDefinition = 0xe7bf;

    private const ushort YappingMawInitialInstruction = 0x9f6f;
    private const ushort YappingMawAlternateInitialInstruction = 0x9fc7;
    private const ushort YappingMawBodyProjectileInstruction = 0xec5c;
    private const ushort YappingMawAlternateBodyProjectileInstruction = 0xec56;
    private const ushort YappingMawGrabSafetyDistance = 32;
    private const ushort YappingMawMinimumTargetDistance = 64;
    private const ushort YappingMawMaximumCurl = 128;
    private const ushort YappingMawGrabCooldownFrames = 48;
    private const ushort YappingMawRetractedDelayFrames = 64;
    private const ushort YappingMawAttackSound = 0x002f;
    private const int YappingMawQuadraticSpeedTable = 0xa0838f;
    private const int YappingMawSignedSineTable = 0xa0b1c3;

    // $A8:A097. The selector is already an even byte offset, so dividing by two yields the
    // eight 45-degree direction sectors in the exact ROM order.
    private static readonly ushort[] YappingMawDirectionInstructions =
    [
        0x9f6f, 0x9f85, 0x9f9b, 0x9fb1,
        0x9fc7, 0x9fdd, 0x9ff3, 0xa009,
    ];

    // $A8:A0A7-$A0C6. These offsets are consumed both immediately after direction selection
    // and later by animation opcodes as the mouth changes shape during retraction.
    private static readonly (short X, short Y)[] YappingMawHeldSamusOffsets =
    [
        (  0, -16), (  8,  -8), ( 16,   0), (  8,   8),
        (  0,  16), ( -8,   8), (-16,   0), ( -8,  -8),
    ];

    private readonly YappingMawEnemyState?[] _yappingMawStates =
        new YappingMawEnemyState?[MaximumEnemyCount];

    /// <summary>Typed native state for every loaded Yapping Maw slot.</summary>
    public IReadOnlyList<YappingMawEnemyState?> YappingMawStates => _yappingMawStates;

    /// <summary>Most recent library-two $2F request emitted by animation opcode $A133.</summary>
    public ushort? LastYappingMawSoundEffect { get; private set; }

    /// <summary>Ports <c>YappingMaw_Init</c> at $A8:A148.</summary>
    private void InitializeYappingMaw(RoomEnemySlot slot)
    {
        var state = new YappingMawEnemyState(slot)
        {
            Function = YappingMawAiFunction.WaitingForSamus,
            OriginX = slot.XPosition,
            OriginY = slot.YPosition,
            ActivationDistance = slot.Parameter1,
            RetractedDelayTimer = YappingMawRetractedDelayFrames,
            OriginalPaletteBits = new SnesObjAttributeWord(slot.PaletteIndex).PaletteBits,
        };
        _yappingMawStates[slot.SlotIndex] = state;

        slot.CurrentInstruction = slot.Parameter2 == 0
            ? YappingMawAlternateInitialInstruction
            : YappingMawInitialInstruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        // SpawnEprojWithGfx searches native indexes $22,$20,... downward. The initializer
        // receives parameters 3,2,1,0 and writes those physical indexes into variables
        // 43,42,41,40 respectively. Allocate in that literal order, then expose links in
        // root-to-mouth order so their geometric meaning is clear in a debugger.
        for (int parameter = 3; parameter >= 0; parameter--)
        {
            RoomEnemyProjectileSlot? body = AllocateEnemyProjectile();
            if (body is null)
            {
                throw new InvalidDataException(
                    $"Yapping Maw slot {slot.SlotIndex} could not allocate all four native body projectiles.");
            }

            InitializeEnemyProjectileFromDefinition(
                body,
                RoomEnemyProjectileKind.YappingMawBody,
                unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex)));
            body.XPosition = slot.XPosition;
            body.YPosition = slot.YPosition;
            body.InstructionPointer = slot.Parameter2 == 0
                ? YappingMawAlternateBodyProjectileInstruction
                : YappingMawBodyProjectileInstruction;
            state.BodyProjectiles[parameter] = body;
        }

        // CreateSpriteAtPos uses object number 56 for parameter zero and 57 otherwise. Its
        // returned native index is represented by the retained physical slot reference.
        state.RootSpriteObject = SpawnRoomSpriteObject(
            slot.XPosition,
            unchecked((ushort)(slot.YPosition + (slot.Parameter2 == 0 ? -8 : 8))),
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
        ushort cameraX,
        ushort cameraY)
    {
        state.GrabCooldown = unchecked((ushort)(state.GrabCooldown - 1));
        state.IsOffScreen = IsYappingMawPointOffScreen(slot, cameraX, cameraY);

        if (samus is null)
        {
            throw new InvalidOperationException(
                "Yapping Maw main AI requires the active Samus actor for targeting.");
        }

        switch (state.Function)
        {
            case YappingMawAiFunction.WaitingForSamus:
                WaitForSamusWithYappingMaw(slot, state, samus);
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
        PositionYappingMawBodyProjectile(state, 0, state.OriginX, state.OriginY);
        PositionYappingMawBodyProjectile(
            state,
            1,
            unchecked((ushort)(state.OriginX + state.SegmentXOffsets[0])),
            unchecked((ushort)(state.OriginY + state.SegmentYOffsets[0])));
        PositionYappingMawBodyProjectile(
            state,
            2,
            unchecked((ushort)(state.OriginX + state.SegmentXOffsets[1])),
            unchecked((ushort)(state.OriginY + state.SegmentYOffsets[1])));
        PositionYappingMawBodyProjectile(
            state,
            3,
            unchecked((ushort)(state.OriginX + state.SegmentXOffsets[2])),
            unchecked((ushort)(state.OriginY + state.SegmentYOffsets[2])));
    }

    /// <summary>Ports $A8:A235, including the deliberately approximate distance helper.</summary>
    private void WaitForSamusWithYappingMaw(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        (ushort distance, ushort angle) = CalculateYappingMawTarget(
            state.OriginX,
            state.OriginY,
            samus.XPosition,
            samus.YPosition);
        state.DistanceToSamus = distance;

        if (unchecked((short)(distance - YappingMawGrabSafetyDistance)) < 0)
        {
            // A target already inside 32 pixels cannot safely trigger an extension and has
            // its grab gate refreshed to 48 every frame. This is why the cooldown must wrap.
            state.GrabCooldown = YappingMawGrabCooldownFrames;
            return;
        }

        if (unchecked((short)(distance - state.ActivationDistance)) >= 0)
            return;

        if (unchecked((short)(state.DistanceToSamus - YappingMawMinimumTargetDistance)) >= 0)
        {
            // Distances at least 64 are already usable. Smaller accepted distances are
            // expanded to 64 so the four-link curve never collapses into the root art.
        }
        else
        {
            state.DistanceToSamus = YappingMawMinimumTargetDistance;
        }

        state.AimAngle = angle;
        state.Function = YappingMawAiFunction.BeginExtension;
    }

    /// <summary>Ports the one-frame setup function at $A8:A28C.</summary>
    private void BeginYappingMawExtension(RoomEnemySlot slot, YappingMawEnemyState state)
    {
        state.ExtensionWhole = 0;
        state.ExtensionFraction = 0;
        state.QuadraticSpeedByteOffset = 0;
        state.SegmentRadius = unchecked((ushort)(state.DistanceToSamus >> 1));
        state.CurlAngle = unchecked((byte)(64 - state.AimAngle));
        state.MirroredBend = unchecked((short)(state.CurlAngle - 128)) < 0
            ? (ushort)0
            : (ushort)1;

        state.BaselineX = CalculateYappingMawX(0x0080, state.SegmentRadius);
        state.BaselineY = CalculateYappingMawY(
            0x0080,
            unchecked((ushort)(state.SegmentRadius >> 1)));

        ushort directionByteOffset = unchecked((ushort)(
            2 * (unchecked((byte)(state.AimAngle + 16)) >> 5)));
        state.DirectionTableByteOffset = directionByteOffset;
        int direction = directionByteOffset / 2;
        slot.CurrentInstruction = YappingMawDirectionInstructions[direction];
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        (short heldX, short heldY) = YappingMawHeldSamusOffsets[direction];
        state.HeldSamusXOffset = unchecked((ushort)heldX);
        state.HeldSamusYOffset = unchecked((ushort)heldY);
        state.Function = YappingMawAiFunction.ExtendingOrRetracting;
    }

    /// <summary>Ports the curved four-link solver and state transition at $A8:A445.</summary>
    private void AdvanceYappingMawExtension(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        ushort bendStep = unchecked((ushort)(state.ExtensionWhole >> 2));
        for (int segment = 0; segment < 4; segment++)
        {
            ushort multiple = unchecked((ushort)(bendStep * (segment + 1)));
            ushort linkAngle = state.MirroredBend != 0
                ? unchecked((ushort)(128 + multiple))
                : unchecked((ushort)(128 - multiple));

            ushort rawX = unchecked((ushort)(
                CalculateYappingMawX(linkAngle, state.SegmentRadius) - state.BaselineX));
            ushort rawY = unchecked((ushort)(
                CalculateYappingMawY(
                    linkAngle,
                    unchecked((ushort)(state.SegmentRadius >> 1))) - state.BaselineY));

            // Variables 27..2E rotate a correction whose length is the LOW BYTE of raw X.
            // Passing raw Y here is an attractive cleanup and is wrong; the cartridge's
            // lopsided curve is a direct consequence of reusing raw X for both components.
            ushort correctionX = unchecked((ushort)(
                CalculateYappingMawX(state.CurlAngle, rawX) -
                CalculateYappingMawX(0, rawX)));
            ushort correctionY = unchecked((ushort)(
                CalculateYappingMawY(state.CurlAngle, rawX) -
                CalculateYappingMawY(0, rawX)));
            state.CorrectionX[segment] = correctionX;
            state.CorrectionY[segment] = correctionY;
            state.SegmentXOffsets[segment] = unchecked((ushort)(rawX + correctionX));
            state.SegmentYOffsets[segment] = unchecked((ushort)(rawY + correctionY));
        }

        slot.XPosition = unchecked((ushort)(state.OriginX + state.SegmentXOffsets[3]));
        slot.YPosition = unchecked((ushort)(state.OriginY + state.SegmentYOffsets[3]));
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
        state.GrabCooldown = YappingMawGrabCooldownFrames;

        // The six retracted lists are not an eight-way table. Straight-ish sectors share a
        // list, while the vertical sectors at byte offsets four and twelve have dedicated
        // art. Preserve parameter-zero's early-return quirk: on its shared-list branches it
        // skips the final held-Samus placement on this one transition frame.
        bool skipHeldPlacement = false;
        if (slot.Parameter2 != 0)
        {
            slot.CurrentInstruction = state.DirectionTableByteOffset switch
            {
                4 => 0xa01f,
                12 => 0xa03d,
                _ => 0xa025,
            };
        }
        else
        {
            slot.CurrentInstruction = state.DirectionTableByteOffset switch
            {
                4 => 0xa05b,
                12 => 0xa079,
                _ => 0xa061,
            };
            skipHeldPlacement = state.DirectionTableByteOffset is not (4 or 12);
        }
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        if (!skipHeldPlacement && state.HasGrabbedSamus)
            PositionSamusInYappingMaw(slot, state, samus);
    }

    /// <summary>Ports $A8:A63E's split-word addition and eight-byte table advance.</summary>
    private void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        int address = YappingMawQuadraticSpeedTable + state.QuadraticSpeedByteOffset;
        uint position = ((uint)state.ExtensionWhole << 16) | state.ExtensionFraction;
        uint velocity = ((uint)ReadWord(_bus!, address + 2) << 16) |
            ReadWord(_bus!, address);
        uint next = unchecked(position + velocity);
        state.ExtensionWhole = unchecked((ushort)(next >> 16));
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

        state.RetractedDelayTimer = unchecked((ushort)(state.RetractedDelayTimer - 1));
        if (unchecked((short)state.RetractedDelayTimer) >= 0 || samus.DeathSequence.IsActive)
            return;

        samus.InputLocked = false;
        state.HasGrabbedSamus = false;
        state.GrabCooldown = YappingMawGrabCooldownFrames;
        state.RetractedDelayTimer = YappingMawRetractedDelayFrames;
        state.Function = YappingMawAiFunction.WaitingForSamus;
    }

    /// <summary>Ports the observable portions of CallSomeSamusCode(3) plus $A8:A665.</summary>
    private void PositionSamusInYappingMaw(
        RoomEnemySlot slot,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // Samus code three cancels any non-inactive grapple and otherwise normalizes only
        // spin-jump/wall-jump bodies. The input handler itself was already replaced by the
        // touch callback; do not force unrelated falling/running poses to standing.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            samus.Grapple.Phase = GrapplePhase.CancelPending;
        }
        else
        {
            SamusMovementType movement = samus.ReadMovementKind(_bus!);
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // CallSomeSamusCode(3) selects pose $01/$02 from the current direction,
                // then runs the ordinary pose metadata and animation initialization path.
                // Reusing those public ROM-backed helpers prevents a Yapping-Maw-only pose
                // approximation from drifting away from every other Samus transition.
                samus.Pose = samus.IsFacingLeft(_bus!)
                    ? SamusPoseIds.FacingLeftNormalPose
                    : SamusPoseIds.FacingRightNormalPose;
                samus.RefreshCollisionRadii(_bus!);
                samus.InitializeAnimation(_bus!, initialFrame: 0);
                samus.CommitPoseHistory(_bus!);
            }
        }

        samus.XPosition = unchecked((ushort)(slot.XPosition + state.HeldSamusXOffset));
        samus.YPosition = unchecked((ushort)(slot.YPosition + state.HeldSamusYOffset));
    }

    /// <summary>Custom touch callback $A8:A799: grab, never ordinary contact damage.</summary>
    private static void ResolveYappingMawTouch(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (unchecked((short)state.GrabCooldown) >= 0 || state.HasGrabbedSamus)
            return;

        state.GrabCooldown = 0;
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
            if (slot.FrozenTimer == 0)
                return;

            ReleaseSamusFromYappingMaw(state, samus);
            return;
        }

        foreach (RoomEnemyProjectileSlot? body in state.BodyProjectiles)
            body?.Clear();
        state.RootSpriteObject?.Clear();
        ReleaseSamusFromYappingMaw(state, samus);
    }

    private static void ReleaseSamusFromYappingMaw(
        YappingMawEnemyState state,
        SamusState? samus)
    {
        if (samus is not null && !samus.DeathSequence.IsActive)
            samus.InputLocked = false;
        state.HasGrabbedSamus = false;
    }

    /// <summary>Custom frozen tail $A8:A835-$A899 for all five auxiliary actors.</summary>
    private static void RunYappingMawFrozen(
        RoomEnemySlot slot,
        YappingMawEnemyState state)
    {
        ushort paletteBits = SelectYappingMawFrozenPalette(
            state.OriginalPaletteBits,
            slot.FrozenTimer);
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
        return 0x0c00;
    }

    /// <summary>Animation callback used by all six held-Samus offset opcodes.</summary>
    private static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        int directionIndex)
    {
        (short x, short y) = YappingMawHeldSamusOffsets[directionIndex];
        state.HeldSamusXOffset = unchecked((ushort)x);
        state.HeldSamusYOffset = unchecked((ushort)y);
    }

    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            LastYappingMawSoundEffect = YappingMawAttackSound;
    }

    private static void PositionYappingMawBodyProjectile(
        YappingMawEnemyState state,
        int index,
        ushort x,
        ushort y)
    {
        RoomEnemyProjectileSlot projectile = state.BodyProjectiles[index] ??
            throw new InvalidOperationException(
                $"Yapping Maw slot {state.Owner.SlotIndex} lost body projectile {index}.");
        projectile.XPosition = x;
        projectile.YPosition = y;
    }

    private static bool IsYappingMawPointOffScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        unchecked((short)(slot.XPosition - cameraX)) < 0 ||
        unchecked((short)(cameraX + 256 - slot.XPosition)) < 0 ||
        unchecked((short)(slot.YPosition - cameraY)) < 0 ||
        unchecked((short)(cameraY + 256 - slot.YPosition)) < 0;

    /// <summary>Ports EnemyFunc_ACA8 at $A0:ACA8 for the one family that consumes it.</summary>
    private (ushort Distance, ushort Angle) CalculateYappingMawTarget(
        ushort originX,
        ushort originY,
        ushort samusX,
        ushort samusY)
    {
        ushort deltaX = unchecked((ushort)(samusX - originX));
        ushort absoluteX = WrappedMagnitude(deltaX);
        if (unchecked((short)(absoluteX - 255)) >= 0)
            return (absoluteX, 0);

        ushort deltaY = unchecked((ushort)(samusY - originY));
        ushort absoluteY = WrappedMagnitude(deltaY);
        if (unchecked((short)(absoluteY - 255)) >= 0)
            return (absoluteY, 0);

        ushort positiveQuadrantAngle = CalculateCartridgeAngle(
            unchecked((short)absoluteX),
            unchecked((short)absoluteY));
        ushort distance = unchecked((ushort)(
            WrappedMagnitude(unchecked((ushort)ReadEightBitNegativeSineProduct(
                positiveQuadrantAngle,
                absoluteX))) +
            WrappedMagnitude(unchecked((ushort)ReadEightBitCosineProduct(
                positiveQuadrantAngle,
                absoluteY)))));
        ushort signedAngle = CalculateCartridgeAngle(
            unchecked((short)deltaX),
            unchecked((short)deltaY));
        return (distance, signedAngle);
    }

    /// <summary>$A8:A73E: X is the Y helper with a minus-64 angle adjustment.</summary>
    private ushort CalculateYappingMawX(ushort angle, ushort length) =>
        CalculateYappingMawY(unchecked((ushort)(angle - 64)), length);

    /// <summary>
    /// Ports $A8:A742 exactly, including the high-byte sine approximation and the unusual
    /// negative-product construction. Only length's low byte reaches WRMPYB.
    /// </summary>
    private ushort CalculateYappingMawY(ushort angle, ushort length)
    {
        short signedSample = unchecked((short)ReadWord(
            _bus!,
            YappingMawSignedSineTable + unchecked((byte)-angle) * 2));
        bool negative = signedSample < 0;
        ushort magnitude = unchecked((ushort)(negative ? -signedSample : signedSample));
        ushort product = unchecked((ushort)((magnitude >> 8) * unchecked((byte)length)));
        if (product == 0)
            return 0;

        if (!negative)
            return unchecked((ushort)(2 * ((product & 0xff00) >> 8)));

        ushort negatedProduct = unchecked((ushort)-product);
        return unchecked((ushort)((2 * (negatedProduct >> 8)) | 0xff00));
    }

    private YappingMawEnemyState RequireYappingMawState(RoomEnemySlot slot) =>
        _yappingMawStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Yapping Maw slot {slot.SlotIndex} has no initialized native state.");
}
