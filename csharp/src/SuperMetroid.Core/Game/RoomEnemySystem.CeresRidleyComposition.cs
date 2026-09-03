using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    private const ushort CeresDoorDefinition = 0xe23f;

    private static readonly ushort[] CeresRidleyWingAnimationDeltas =
        [0x000c, 0x000e, 0x0010, 0x0012, 0x001c, 0x0020, 0x0028, 0x0030];

    /// <summary>Ports $A6:D97D-$D9EC after the common Ridley movement pass.</summary>
    private static void TickRidleyWingAnimation(RidleyEnemyState state)
    {
        int absoluteXVelocity = Math.Abs(unchecked((short)state.HorizontalVelocity));
        int absoluteYVelocity = Math.Abs(unchecked((short)state.VerticalVelocity));
        int largestAxisVelocity = Math.Max(absoluteXVelocity, absoluteYVelocity);
        if (largestAxisVelocity == 0)
        {
            state.WingAnimationTimerDelta = 0;
        }
        else
        {
            // $A6:D9CF isolates bits 8..11 after multiplying the 8.8 velocity by four,
            // converts the resulting word-table byte index, and clamps that index to $E.
            int tableIndex = Math.Min(7, ((largestAxisVelocity << 2) & 0x0f00) >> 8);
            ushort delta = CeresRidleyWingAnimationDeltas[tableIndex];
            if (unchecked((short)state.VerticalVelocity) >= 0)
                delta >>= 1;
            state.WingAnimationTimerDelta = delta;
        }

        state.WingAnimationTimer = unchecked((ushort)(
            state.WingAnimationTimer - state.WingAnimationTimerDelta));
        if (unchecked((short)state.WingAnimationTimer) >= 0)
            return;

        state.WingAnimationTimer = 0x0020;
        state.WingFrame = unchecked((ushort)((state.WingFrame + 1) % 10));
    }

    /// <summary>
    /// Ports the neutral-tail controller at $A6:CBC0-$CCBC, the segment update at $D09F,
    /// and the position/distance chains at $CEBA/$CF5A. The retail routine deliberately
    /// adds the preceding segment's low-byte angle when deriving every later offset;
    /// retaining that documented anomaly is why the tail curls and rotates at the same
    /// rate as the cartridge.
    /// </summary>
    private void TickRidleyTail(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (state.TailSegments.Length != 7)
            throw new InvalidDataException("Ceres Ridley requires seven native tail segments.");

        if (state.TailFunctionIndex == 1)
        {
            state.TailMinimumClockwiseAngle = state.FacingDirection == 2
                ? (ushort)0x3fc0
                : (ushort)0x3ff0;
            state.TailMaximumCounterClockwiseAngle = state.FacingDirection == 2
                ? (ushort)0x4010
                : (ushort)0x4040;

            HandleCeresRidleyNeutralTailControl(slot, state, samus);

            for (int index = 0; index < state.TailSegments.Length; index++)
                TickRidleyTailSegment(state, index);
        }
        else if (state.TailFunctionIndex != 0)
        {
            throw new InvalidDataException(
                $"Ceres Ridley tail function {state.TailFunctionIndex} is not translated.");
        }

        // Tail function zero suppresses angular *motion* before $A6:A4C5 activates every
        // segment; it does not suppress the position solver. The seven initial angles and
        // distances installed by $A6:D2D6 already describe Ridley's resting tail. Leaving
        // their offsets at zero collapsed every piece onto the hip throughout the body fade,
        // then made the complete tail appear abruptly when the pre-liftoff timer expired.
        // Rebuild geometry unconditionally while retaining the native activation boundary.
        for (int index = 0; index < state.TailSegments.Length; index++)
            UpdateRidleyTailSegmentOffset(state, index);

        RidleyTailSegment first = state.TailSegments[0];
        first.YPosition = unchecked((ushort)(slot.YPosition + first.YOffset + 16));
        first.XPosition = state.FacingDirection switch
        {
            0 => unchecked((ushort)(slot.XPosition + 0x0020 + first.XOffset)),
            1 => slot.XPosition,
            2 => unchecked((ushort)(slot.XPosition - 0x0020 + first.XOffset)),
            _ => throw new InvalidDataException(
                $"Ridley facing direction {state.FacingDirection} exceeds the native table."),
        };

        for (int index = 1; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment previous = state.TailSegments[index - 1];
            RidleyTailSegment current = state.TailSegments[index];
            current.YPosition = unchecked((ushort)(previous.YPosition + current.YOffset));
            current.XPosition = state.FacingDirection == 1
                ? slot.XPosition
                : unchecked((ushort)(previous.XPosition + current.XOffset));
        }

        UpdateRidleyTailDistances(state);
    }

    /// <summary>
    /// Replays <c>HandleRidleyTailWhip</c> and <c>HandleRidleyTailFlingTrigger</c>. Ceres
    /// disables random/proximity flings only during the swoop, then sets a one-shot request
    /// at its lowest point so the tail snaps toward Samus as Ridley charges back upward.
    /// </summary>
    private static void HandleCeresRidleyNeutralTailControl(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        bool allActive = state.TailSegments.All(segment => segment.Active);
        if (allActive)
        {
            bool targetsIdle =
                (state.TailWhipTargetClockwiseAngle & 0x8000) != 0 &&
                (state.TailWhipTargetCounterClockwiseAngle & 0x8000) != 0;
            if (targetsIdle)
            {
                if (state.TailWhipRequest != 0)
                {
                    AimCeresRidleyTailWhip(
                        state,
                        samus,
                        unchecked((byte)(state.TailWhipRequest - 1)));
                }
                else if (state.IdleTailWhipEnabled != 0 && samus is not null &&
                         Math.Abs(unchecked((short)(samus.XPosition - slot.XPosition))) < 128)
                {
                    // The other native admission is a low-byte RNG value >= $F0. The
                    // runtime's random delegate advances per call rather than exposing the
                    // cartridge's once-per-frame seed, so consuming it here would alter the
                    // attack selector. Proximity is the deterministic retail branch and is
                    // the one reached during Ceres lunges.
                    AimCeresRidleyTailWhip(state, samus, additionalAngle: 0);
                }
            }

            state.TailWhipRequest = 0;
        }

        if (state.TailSegments.Any(segment => segment.Active))
            return;

        // Once a wave/whip has propagated through all seven parts, neutral AI starts the
        // next wave at the base and retires both old targets. Samus's X half selects the
        // exact one- or two-unit angular delta used by `$A6:CC6B-$CC79`.
        state.TailSegments[0].Active = true;
        state.TailWhipTargetClockwiseAngle = 0xffff;
        state.TailWhipTargetCounterClockwiseAngle = 0xffff;
        state.TailAngleDelta = samus is not null && samus.XPosition < 0x0070
            ? (ushort)1
            : (ushort)2;
    }

    private static void AimCeresRidleyTailWhip(
        RidleyEnemyState state,
        SamusState? samus,
        byte additionalAngle)
    {
        // Facing direction one is the front-facing transition art. `$A6:D1A4` returns
        // without installing a target in that state, and fight mode zero likewise blocks
        // the attack before liftoff.
        if (samus is null || state.FacingDirection == 1 || state.FightMode == 0)
            return;

        RidleyTailSegment root = state.TailSegments[0];
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - root.XPosition)),
            unchecked((short)(samus.YPosition + 24 - root.YPosition)));
        byte targetByte = unchecked((byte)-(cartridgeAngle - 0x80));
        ushort additional = unchecked((ushort)(additionalAngle << 8));

        if (state.FacingDirection == 0)
        {
            // Facing-left Ridley cannot aim through his own body. Angles inside the
            // forbidden forward interval clamp to $E8 before bank-$3F expansion.
            if (targetByte >= 0x18 && targetByte < 0xe8)
                targetByte = 0xe8;
            ushort target = unchecked((ushort)(0x3f00 + targetByte - additional));
            if (target < root.Angle)
            {
                state.TailWhipTargetClockwiseAngle = target;
                state.TailAngleDelta = 8;
            }
        }
        else
        {
            if (targetByte >= 0x18 && targetByte < 0xe8)
                targetByte = 0x18;
            ushort target = unchecked((ushort)(0x4000 + targetByte + additional));
            if (target >= root.Angle)
            {
                state.TailWhipTargetCounterClockwiseAngle = target;
                state.TailAngleDelta = 8;
            }
        }
    }

    private static void TickRidleyTailSegment(RidleyEnemyState state, int index)
    {
        RidleyTailSegment segment = state.TailSegments[index];
        if (segment.Active)
        {
            if (segment.StaggerAngle < state.IdealInterSegmentTailAngle)
            {
                segment.StaggerAngle = unchecked((ushort)(
                    segment.StaggerAngle + state.TailAngleDelta));
                return;
            }

            if (segment.StaggerAngle != 0xffff)
            {
                segment.StaggerAngle = 0xffff;
                if (index + 1 < state.TailSegments.Length)
                {
                    state.TailSegments[index + 1].Active = true;
                    state.TailSegments[index + 1].MovementDirection = segment.MovementDirection;
                }
            }

            if ((segment.MovementDirection & 0x8000) != 0)
            {
                bool whipping = (state.TailWhipTargetClockwiseAngle & 0x8000) == 0;
                if (whipping)
                {
                    segment.TargetDistance = 0x0c00;
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < state.TailWhipTargetClockwiseAngle)
                    {
                        segment.Angle = state.TailWhipTargetClockwiseAngle;
                        DeactivateRidleyTailSegment(segment);
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
                else
                {
                    int next = segment.Angle - state.TailAngleDelta - 1;
                    if (next < state.TailMinimumClockwiseAngle)
                    {
                        segment.MovementDirection = 0;
                        segment.Angle = state.TailMinimumClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
            }
            else
            {
                bool whipping = (state.TailWhipTargetCounterClockwiseAngle & 0x8000) == 0;
                int next = segment.Angle + state.TailAngleDelta;
                if (whipping)
                {
                    segment.TargetDistance = 0x0c00;
                    if (next >= state.TailWhipTargetCounterClockwiseAngle)
                    {
                        segment.Angle = state.TailWhipTargetCounterClockwiseAngle;
                        DeactivateRidleyTailSegment(segment);
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
                else
                {
                    if (next >= state.TailMaximumCounterClockwiseAngle)
                    {
                        segment.MovementDirection = 0x8000;
                        segment.Angle = state.TailMaximumCounterClockwiseAngle;
                    }
                    else
                    {
                        segment.Angle = unchecked((ushort)next);
                    }
                }
            }
        }

    }

    /// <summary>
    /// Converts one segment's current polar angle/distance into the relative Cartesian
    /// offset consumed by $A6:CEBA. This is deliberately independent of the segment's active
    /// flag: inactive tail pieces keep their established pose rather than collapsing.
    /// </summary>
    private void UpdateRidleyTailSegmentOffset(RidleyEnemyState state, int index)
    {
        RidleyTailSegment segment = state.TailSegments[index];
        byte angle = unchecked((byte)segment.Angle);
        if (index != 0)
            angle = unchecked((byte)(angle + state.TailSegments[index - 1].Angle));
        ushort distanceInPixels = unchecked((ushort)(segment.Distance >> 8));
        segment.XOffset = MultiplyCartridgeSinCos(distanceInPixels, angle);
        segment.YOffset = MultiplyCartridgeSinCos(
            distanceInPixels,
            unchecked((byte)(angle + 64)));
    }

    private static void DeactivateRidleyTailSegment(RidleyTailSegment segment)
    {
        segment.Active = false;
        segment.StaggerAngle = 0;
        segment.MovementDirection ^= 0x8000;
    }

    /// <summary>Ports the six extend/shrink records at <c>$A6:CF5A-$D09E</c>.</summary>
    private static void UpdateRidleyTailDistances(RidleyEnemyState state)
    {
        ushort[] maximumDistances = [0, 0x1800, 0x1800, 0x1600, 0x1600, 0x1200, 0x0500];
        ushort[] neutralDistances = [0x0200, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 0x0500];
        for (int index = 1; index < state.TailSegments.Length; index++)
        {
            RidleyTailSegment segment = state.TailSegments[index];
            if (segment.TargetDistance != 0)
            {
                if (segment.TargetDistance < segment.Distance)
                    segment.TargetDistance = 0;
                segment.Distance = unchecked((ushort)Math.Min(
                    maximumDistances[index],
                    segment.Distance + state.TailExtensionSpeed));
            }
            else if (segment.Distance > neutralDistances[index])
            {
                segment.Distance = unchecked((ushort)(segment.Distance - 0x0080));
            }
        }
    }

    /// <summary>Ports DrawRidleyTail $A6:DB2A and DrawRidleyWings $A6:DAD8.</summary>
    private void DrawRidleySupplementalSprites(
        OamBuffer oam,
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY)
    {
        RidleyEnemyState state = RequireRidley(slot);
        if ((slot.EnemyDefinitionPointer == CeresRidleyDefinition && CeresStatus != 0) ||
            state.MovementAnimationEnabled == 0)
            return;

        if (state.TailSegments.Length == 7)
        {
            RidleyTailSegment tip = state.TailSegments[6];
            int tipIndex = unchecked((byte)(
                tip.Angle + state.TailSegments[5].Angle + 8)) & 0xf0;
            ushort tipSpritemap = ReadWord(_bus!, 0xa6dcba + (tipIndex >> 3));
            DrawRidleyWorldSpritemap(
                oam,
                state.SpritemapPaletteIndex,
                tip,
                tipSpritemap,
                cameraX,
                cameraY);

            // $A6:DB59-$DBBF walks back from segment five to the base after emitting the
            // tip. Lower OAM indices win equal-priority overlap, so reversing this visually
            // changes the curl even though every individual piece is otherwise correct.
            ushort[] segmentSpritemaps =
                [0xdc90, 0xdc90, 0xdc97, 0xdc97, 0xdc9e, 0xdc9e];
            for (int index = 5; index >= 0; index--)
                DrawRidleyWorldSpritemap(
                    oam,
                    state.SpritemapPaletteIndex,
                    state.TailSegments[index],
                    segmentSpritemaps[index],
                    cameraX,
                    cameraY);
        }

        if (state.FacingDirection == 1)
            return;
        int wingPointerIndex = (state.FacingDirection == 0 ? 0 : 10) + state.WingFrame;
        ushort wingSpritemap = ReadWord(_bus!, 0xa6db02 + wingPointerIndex * 2);
        oam.AddEnemySpritemap(
            _bus!,
            bank: 0xa6,
            wingSpritemap,
            unchecked((ushort)(slot.XPosition - cameraX)),
            unchecked((ushort)(slot.YPosition - cameraY)),
            state.SpritemapPaletteIndex,
            baseTileIndex: 0,
            clipVerticalWrap: true,
            originYIsOnScreen: unchecked((ushort)(slot.YPosition - cameraY)) < 0x0100);
    }

    /// <summary>
    /// Replays the direct branch to <c>$A6:A2E3</c> taken while Ridley's movement and
    /// animation word is zero. This is deliberately separate from the deferred hook below:
    /// the cartridge executes this path inside Ridley's enemy-main call, before the ordinary
    /// drawing queues, so the Baby appears with the room and before Ridley's composite.
    /// </summary>
    public void DrawCeresRidleyImmediateBabyAndDoor(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();
        if (_ridleyState is null || _ridleyState.MovementAnimationEnabled != 0)
            return;

        DrawCeresRidleyBabyAndDoor(oam, cameraX, cameraY);
    }

    /// <summary>
    /// Replays the enemy-graphics-drawn hook installed by <c>$A6:A2E5</c>. The cartridge
    /// installs it only after Ridley's animation word becomes nonzero; it then emits the
    /// same private Baby instruction list after the ordinary enemy layer loop.
    /// </summary>
    public void DrawCeresRidleyPostEnemyHook(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY)
    {
        ArgumentNullException.ThrowIfNull(oam);
        EnsureLoaded();
        if (_ridleyState is null || _ridleyState.MovementAnimationEnabled == 0)
            return;

        DrawCeresRidleyBabyAndDoor(oam, cameraX, cameraY);
    }

    private void DrawCeresRidleyBabyAndDoor(
        OamBuffer oam,
        ushort cameraX,
        ushort cameraY)
    {
        RidleyEnemyState state = _ridleyState
            ?? throw new InvalidOperationException("Ceres Ridley Baby draw requires active state.");

        if (CeresStatus == 0)
        {
            ushort babySpritemap = AdvanceCeresBabyDrawInstruction(state);
            if (babySpritemap != 0)
            {
                oam.AddEnemySpritemap(
                    _bus!,
                    bank: 0xa6,
                    babySpritemap,
                    unchecked((ushort)(state.BabyXPosition - cameraX)),
                    unchecked((ushort)(state.BabyYPosition - cameraY)),
                    paletteBits: 0,
                    baseTileIndex: 0,
                    clipVerticalWrap: true,
                    originYIsOnScreen:
                        unchecked((ushort)(state.BabyYPosition - cameraY)) < 0x0100);
            }
        }

        // Enemy one is the ordinary Ridley-room door. Its $F69F instruction sets variable
        // B when the common enemy pass intentionally hides the actor; $A6:A2FA then draws
        // this fixed overlay after the Baby. This is a native cross-slot draw hook, not a
        // replacement for the door actor's own instruction interpreter.
        RoomEnemySlot door = _slots[1];
        if (door.EnemyDefinitionPointer == CeresDoorDefinition && door.VariableB != 0)
        {
            // The private Ceres hook does not call WriteEnemyOAM and therefore owns a
            // second, cartridge-authored quake adjustment at `$A6:A2F2-$A314`. Its `Y`
            // index is the low two bits of the earthquake timer, but the table contains
            // words and the assembly omits an ASL. Preserve that retail byte-index read:
            // bytes 00,00,FC,FF become signed X offsets 0,0,-4,-1.
            int quakeTableByte = 0xa6a321 + (EarthquakeTimer & 3);
            int quakeXOffset = unchecked((sbyte)_bus!.ReadByte(quakeTableByte));
            oam.AddEnemySpritemap(
                _bus!,
                bank: 0xa6,
                spritemapPointer: 0xa329,
                unchecked((ushort)(door.XPosition - cameraX + quakeXOffset)),
                unchecked((ushort)(door.YPosition - cameraY)),
                paletteBits: 0x0400,
                baseTileIndex: 0);
        }
    }

    private ushort AdvanceCeresBabyDrawInstruction(RidleyEnemyState state)
    {
        ushort cursor = state.BabyInstruction;
        for (int commandCount = 0; commandCount < 64; commandCount++)
        {
            ushort word = ReadWord(_bus!, 0xa60000 | cursor);
            if ((word & 0x8000) == 0)
            {
                // $A6:DBE7 compares the frame duration with the private elapsed timer. A
                // match advances four bytes and immediately selects the next frame; all
                // other calls increment the timer and retain the current map.
                if (word == state.BabyInstructionTimer)
                {
                    cursor = unchecked((ushort)(cursor + 4));
                    state.BabyInstruction = cursor;
                    state.BabyInstructionTimer = 1;
                    continue;
                }

                state.BabyInstruction = cursor;
                state.BabyInstructionTimer = unchecked((ushort)(state.BabyInstructionTimer + 1));
                state.BabyCurrentSpritemap = ReadWord(
                    _bus!,
                    0xa60000 | unchecked((ushort)(cursor + 2)));
                return state.BabyCurrentSpritemap;
            }

            ushort argument = unchecked((ushort)(cursor + 2));
            switch (word)
            {
                case CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX:
                    // BabyMetroid_Instr_2 calls QueueSfx3_Max6($24) on every execution,
                    // including the random branch that immediately jumps to another list.
                    // Publish before reproducing that branch so its control flow cannot
                    // accidentally suppress the chirp.
                    QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x0024), maximumQueued: 6);
                    // The native “moving” word is the same $8808 velocity accumulator
                    // advanced by TickCeresBaby. While stationary, the cartridge RNG may
                    // branch to the expressive palette-animation list with 50% probability.
                    cursor = state.BabyVerticalVelocity != 0
                        ? unchecked((ushort)(argument + 2))
                        : (_nextRandom!() & 1) != 0
                            ? ReadWord(_bus!, 0xa60000 | argument)
                            : unchecked((ushort)(argument + 2));
                    break;

                case CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_UpdateColors:
                    ushort palettePointer = ReadWord(_bus!, 0xa60000 | argument);
                    _cgram!.LoadFromBus(
                        _bus!,
                        0xa60000 | palettePointer,
                        colorCount: 15,
                        destinationIndex: 0x0162 / 2);
                    cursor = unchecked((ushort)(argument + 2));
                    break;

                case CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling:
                    cursor = state.BabyVerticalVelocity != 0
                        ? ReadWord(_bus!, 0xa60000 | argument)
                        : unchecked((ushort)(argument + 2));
                    break;

                case CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoX:
                    cursor = ReadWord(_bus!, 0xa60000 | argument);
                    break;

                default:
                    throw new InvalidDataException(
                        $"Ceres Baby draw instruction $A6:{cursor:X4} opcode ${word:X4} is not translated.");
            }

            state.BabyInstruction = cursor;
        }

        throw new InvalidDataException(
            "Ceres Baby draw instruction list exceeded 64 commands without selecting a frame.");
    }

    private void SpawnCeresRidleyMode7Walls()
    {
        // SpawnEnemy consumes the first two free native slots and initializes the literal
        // population records embedded at $A6:AA2F/$AA3F. Keeping these as real slots makes
        // their layer-two OAM order, off-screen processing, and $F7A5 visibility dispatcher
        // flow through the same scheduler as the cartridge instead of painting host panels.
        SpawnCeresRidleyMode7Wall(xPosition: 0x0008, parameter1: 5);
        SpawnCeresRidleyMode7Wall(xPosition: 0x00f8, parameter1: 6);
    }

    private void SpawnCeresRidleyMode7Wall(ushort xPosition, ushort parameter1)
    {
        int slotIndex = Array.FindIndex(_slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy slot for a Mode-7 wall.");

        RoomEnemyDefinition definition = ReadDefinition(_bus!, CeresDoorDefinition);
        RoomEnemyPopulationRecord population = new(
            CeresDoorDefinition,
            xPosition,
            YPosition: 0x007f,
            InitializationParameter: 0,
            Properties: 0x2800,
            ExtraProperties: 0,
            Parameter1: parameter1,
            Parameter2: 0);
        RoomEnemySlot wall = _slots[slotIndex];
        InitializeSlotFromDefinition(wall, population, definition);
        RunInitializationAi(wall);
        EnemyCount = unchecked((ushort)Math.Max(EnemyCount, slotIndex + 1));
        FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * NativeSlotSize));
    }

    private void DrawRidleyWorldSpritemap(
        OamBuffer oam,
        ushort paletteIndex,
        RidleyTailSegment segment,
        ushort spritemap,
        ushort cameraX,
        ushort cameraY)
    {
        ushort screenX = unchecked((ushort)(segment.XPosition - cameraX));
        ushort screenY = unchecked((ushort)(segment.YPosition - cameraY));
        oam.AddEnemySpritemap(
            _bus!,
            bank: 0xa6,
            spritemap,
            screenX,
            screenY,
            paletteIndex,
            baseTileIndex: 0,
            clipVerticalWrap: true,
            originYIsOnScreen: screenY < 0x0100);
    }

    /// <summary>
    /// Replays $A0:9A5A's extended-spritemap Samus collision walk for either Ridley. The
    /// shared bank-$A6 tail handler tests the solved tip first; only when that misses does
    /// bank $A0 dispatch the active body's authored $DF59 rectangles. This ordering is
    /// important because the tail has encounter-specific damage and must produce at most
    /// one contact reaction in the frame.
    /// </summary>
    public bool ResolveRidleySamusContact(SamusState samus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(samus);
        EnsureLoaded();
        // $A0:9A8B-$9A98 gates ordinary contact only on the contact-damage mode and
        // Samus's invincibility timer. It does not inspect the bank-$90 knockback-active
        // word. The extra host guard previously made a stale/ongoing aerial knockback
        // suppress every later Ridley overlap even after invincibility had expired.
        RoomEnemySlot slot = _slots[0];
        if (_ridleyState is null ||
            !IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
            slot.EnemyDefinitionPointer == CeresRidleyDefinition && CeresStatus != 0 ||
            samus.InvincibilityTimer != 0)
        {
            return false;
        }

        if (slot.SpritemapPointer == 0 ||
            slot.Properties.HasAny(
                EnemyProperties.Invisible |
                EnemyProperties.Deleted |
                EnemyProperties.IgnoreSamusCollision))
        {
            return false;
        }

        if (_ridleyState.MovementAnimationEnabled != 0 &&
            _ridleyState.TailSegments.Length == 7)
        {
            // Ridley_Func_127 at $A6:DFD9 checks a radius-14 rectangle centered on the
            // solved tail tip before the common extended-body collision pass. Ceres writes
            // damage $000F and Lower Norfair writes $0078; neither value comes from the
            // body's enemy header.
            RidleyTailSegment tip = _ridleyState.TailSegments[6];
            int xDistance = Math.Abs(unchecked((short)(samus.XPosition - tip.XPosition)));
            int yDistance = Math.Abs(unchecked((short)(samus.YPosition - tip.YPosition)));
            if (xDistance < samus.Kinematics.XRadius + 14 &&
                yDistance < samus.Kinematics.YRadius + 14)
            {
                ApplyNormalEnemyTouchDamage(
                    samus,
                    controllerInput,
                    _ridleyState.TailDamage,
                    tip.XPosition);
                return true;
            }
        }

        if (!TryFindExtendedHitboxCallback(
                slot,
                samus.XPosition,
                samus.YPosition,
                samus.Kinematics.XRadius,
                samus.Kinematics.YRadius,
                selectShotCallback: false,
                out ushort touchAi))
        {
            return false;
        }

        if (touchAi != RidleyExtendedTouchAi)
        {
            throw new InvalidDataException(
                $"Ridley extended body selected touch AI $A6:{touchAi:X4}, expected " +
                $"$A6:{RidleyExtendedTouchAi:X4} from map $A6:{slot.SpritemapPointer:X4}.");
        }

        // $A6:DF59 enters the common no-death-check handler. That distinction matters for
        // Screw Attack: a lethal body touch leaves Lower Norfair Ridley alive long enough
        // to select the forced zero-health lunge and grab Samus for the authored death.
        ResolveNormalEnemyTouch(
            slot,
            samus,
            controllerInput,
            skipDeathAnimation: true);
        if (slot.EnemyDefinitionPointer == NorfairRidleyDefinition)
            ResolveNorfairRidleyShotAfterCommon(slot);
        return true;
    }

    /// <summary>
    /// Compatibility entry point retained for the focused Ceres debugger. New runtime code
    /// calls <see cref="ResolveRidleySamusContact"/> so the shared cartridge routine also
    /// covers the real boss without a second bespoke geometry implementation.
    /// </summary>
    public bool ResolveCeresRidleySamusContact(SamusState samus, ushort controllerInput) =>
        _slots[0].EnemyDefinitionPointer == CeresRidleyDefinition &&
        ResolveRidleySamusContact(samus, controllerInput);

    /// <summary>
    /// Shared port of the rectangle walk used by $A0:9A5A (Samus contact) and $A0:9B7F
    /// (projectile contact). Every component offset and every rectangle comes from the
    /// active extended spritemap; the tiny 8x8 radius in Ridley's enemy header is not his
    /// body and must never be substituted for this data.
    /// </summary>
    private bool ExtendedSpritemapOverlapsRectangle(
        RoomEnemySlot slot,
        ushort subjectX,
        ushort subjectY,
        ushort subjectXRadius,
        ushort subjectYRadius)
    {
        int subjectLeft = unchecked((ushort)(subjectX - subjectXRadius));
        int subjectRight = unchecked((ushort)(subjectX + subjectXRadius));
        int subjectTop = unchecked((ushort)(subjectY - subjectYRadius));
        int subjectBottom = unchecked((ushort)(subjectY + subjectYRadius));
        int extendedAddress = 0xa60000 | slot.SpritemapPointer;
        int componentCount = _bus!.ReadByte(extendedAddress);
        if (componentCount > 64)
            throw new InvalidDataException("Extended spritemap contains more than 64 components.");
        int componentAddress = AdvanceBankAddress(extendedAddress, 2);
        for (int component = 0; component < componentCount; component++)
        {
            ushort componentX = unchecked((ushort)(
                slot.XPosition + ReadWord(_bus, componentAddress)));
            ushort componentY = unchecked((ushort)(
                slot.YPosition + ReadWord(_bus, AdvanceBankAddress(componentAddress, 2))));
            ushort hitboxPointer = ReadWord(
                _bus,
                AdvanceBankAddress(componentAddress, 6));
            int hitboxAddress = 0xa60000 | hitboxPointer;
            int hitboxCount = ReadWord(_bus, hitboxAddress);
            if (hitboxCount > 64)
                throw new InvalidDataException("Extended spritemap component contains more than 64 hitboxes.");
            hitboxAddress = AdvanceBankAddress(hitboxAddress, 2);

            for (int hitbox = 0; hitbox < hitboxCount; hitbox++)
            {
                int left = unchecked((ushort)(componentX + ReadWord(_bus, hitboxAddress)));
                int top = unchecked((ushort)(componentY + ReadWord(
                    _bus,
                    AdvanceBankAddress(hitboxAddress, 2))));
                int right = unchecked((ushort)(componentX + ReadWord(
                    _bus,
                    AdvanceBankAddress(hitboxAddress, 4))));
                int bottom = unchecked((ushort)(componentY + ReadWord(
                    _bus,
                    AdvanceBankAddress(hitboxAddress, 6))));

                // $A0:9B14/$9B20/$9B2C/$9B38 use signed comparisons and treat touching
                // right/bottom edges as non-overlap. Ceres coordinates remain in the low
                // positive room range, so these integer comparisons are the same result.
                if (unchecked((short)(left - subjectRight)) <= 0 &&
                    unchecked((short)(right - subjectLeft)) > 0 &&
                    unchecked((short)(top - subjectBottom)) <= 0 &&
                    unchecked((short)(bottom - subjectTop)) > 0)
                {
                    return true;
                }
                hitboxAddress = AdvanceBankAddress(hitboxAddress, 12);
            }
            componentAddress = AdvanceBankAddress(componentAddress, 8);
        }
        return false;
    }

    private void ApplyNormalEnemyTouchDamage(
        SamusState samus,
        ushort controllerInput,
        ushort damageBeforeSuit,
        ushort damageSourceX)
    {
        // Enemy header $A0:E13F publishes damage five. Suit_Damage_Division quarters it
        // for Gravity or halves it for Varia before the normal touch AI installs $60/$05.
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(damageBeforeSuit >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(damageBeforeSuit >> 1))
                : damageBeforeSuit;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = 0x0060;
        ushort direction = samus.XPosition >= damageSourceX ? (ushort)1 : (ushort)0;
        SamusKnockbackMovement.Start(
            _bus!,
            samus,
            controllerInput,
            direction,
            knockbackTimer: 5);
    }
}
