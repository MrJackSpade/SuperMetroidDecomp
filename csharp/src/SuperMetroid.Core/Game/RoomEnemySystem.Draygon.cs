namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A5 translation of Draygon's four-record loader and opening encounter route.
/// The body owns world movement; eye, tail, and arms are composited records whose positions
/// are copied after every body function exactly like <c>$A5:86FC</c>.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort DraygonBodyDefinition = 0xde3f;
    private const ushort DraygonEyeDefinition = 0xde7f;
    private const ushort DraygonTailDefinition = 0xdebf;
    private const ushort DraygonArmsDefinition = 0xdeff;

    private const int DraygonIntroPalette = 0xa5a217;
    private const int DraygonIntroEvirTiles = 0xb19400;
    private const int DraygonFightIntroDanceData = 0xa5ce07;
    private const int DraygonEvirMovementLatencies = 0xa5a19f;

    private DraygonEnemyState? _draygon;

    /// <summary>The typed encounter extension while the retail Draygon population is loaded.</summary>
    public DraygonEnemyState? Draygon => _draygon;

    private static bool IsDraygonDefinition(ushort definition) => definition is
        DraygonBodyDefinition or DraygonEyeDefinition or
        DraygonTailDefinition or DraygonArmsDefinition;

    private void ResetDraygonRoomState() => _draygon = null;

    /// <summary>Ports <c>InitAI_DraygonBody</c> at <c>$A5:8687</c>.</summary>
    private void InitializeDraygonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
            throw new InvalidDataException("Draygon's body must own native enemy slot $0000.");

        // The native copy targets colors 144..168. This runtime has one concrete CGRAM
        // surface instead of a second fade-target array, so publish those exact 25 colors
        // immediately; the room's normal fade-in controls when they become visible.
        _cgram!.LoadFromBus(_bus!, DraygonIntroPalette, colorCount: 25, destinationIndex: 144);

        // $7E:2000 is the enemy BG2 staging surface and the following NMI copies it to
        // VRAM word $4800. Fill all $800 words, not merely the currently visible page.
        byte[] bg2Bytes = new byte[0x1000];
        for (int byteIndex = 0; byteIndex < bg2Bytes.Length; byteIndex += 2)
        {
            bg2Bytes[byteIndex] = 0x38;
            bg2Bytes[byteIndex + 1] = 0x03;
        }
        _vram!.LoadBytes(0x9000, bg2Bytes); // VRAM word $4800 expressed as a byte address.

        body.PaletteIndex = EnemyPaletteBits.Palette7;
        body.CurrentInstruction = DraygonInstructionLists.Ilist_9889;
        body.InstructionTimer = 1;

        _processAllEnemies = true;
        _draygon = new DraygonEnemyState(body)
        {
            Bg2TilemapSize = 0x0400,
            BackgroundTilemapPrepared = true,
            RoomLoadingIrqCommand = 0x000c,
            MinimapDisabledAndBossTilesExplored = true,
            Function = DraygonAiFunction.IntroInitialDelay,
        };
        _draygon.DisabledCannonWords.Add(DraygonCannonData.UnusedBottomDisabledWord);
    }

    /// <summary>Ports the independent initializers at <c>$A5:C46B/C599/C5AD</c>.</summary>
    private void InitializeDraygonPart(RoomEnemySlot part)
    {
        DraygonEnemyState state = _draygon ??
            throw new InvalidDataException("Draygon part appeared before the body record.");

        part.InstructionTimer = 1;
        switch (part.EnemyDefinitionPointer)
        {
            case DraygonEyeDefinition:
                if (part.SlotIndex != 1)
                    throw new InvalidDataException("Draygon's eye must own native slot $0040.");
                part.CurrentInstruction = DraygonInstructionLists.Ilist_9944;
                part.VariableA = 0x804b; // Literal RTS until the body publishes facing AI.
                state.Eye = part;
                return;

            case DraygonTailDefinition:
                if (part.SlotIndex != 2)
                    throw new InvalidDataException("Draygon's tail must own native slot $0080.");
                part.CurrentInstruction = DraygonInstructionLists.Ilist_99FC;
                part.PaletteIndex = EnemyPaletteBits.Palette7;
                state.Tail = part;
                return;

            case DraygonArmsDefinition:
                if (part.SlotIndex != 3)
                    throw new InvalidDataException("Draygon's arms must own native slot $00C0.");
                // Body init briefly writes $9813 before this record exists. Retail part init
                // clears that lost write and installs the ordinary idle loop at $97E7.
                part.CurrentInstruction = DraygonInstructionLists.Ilist_97E7;
                part.PaletteIndex = EnemyPaletteBits.Palette7;
                part.Layer = 2;
                state.Arms = part;
                return;

            default:
                throw new InvalidDataException(
                    $"Enemy ${part.EnemyDefinitionPointer:X4} is not a Draygon part.");
        }
    }

    /// <summary>Ports <c>MainAI_DraygonBody</c> at <c>$A5:86FC</c>.</summary>
    private void RunDraygonBodyMain(RoomEnemySlot body, SamusState? samus, byte nmiFrameCounter8)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        switch (state.Function)
        {
            case DraygonAiFunction.IntroInitialDelay:
                RunDraygonIntroInitialDelay(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.IntroDance:
                RunDraygonIntroDance(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopRightSetup:
                SetupDraygonRightSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopRightDescending:
                DescendDraygonRightSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopRightApex:
                CrossDraygonRightSwoopApex(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopRightAscending:
                AscendDraygonRightSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopLeftSetup:
                SetupDraygonLeftSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopLeftDescending:
                DescendDraygonLeftSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopLeftApex:
                CrossDraygonLeftSwoopApex(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.SwoopLeftAscending:
                AscendDraygonLeftSwoop(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopRightSetup:
                SetupDraygonGoopPass(state, movingRight: true, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopRight:
                ApproachSamusForDraygonGoop(state, movingRight: true, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopRightTail:
                FireDraygonGoop(state, movingRight: true, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopRightRecovery:
                ExitDraygonGoopPass(state, movingRight: true);
                break;
            case DraygonAiFunction.GoopLeftSetup:
                SetupDraygonGoopPass(state, movingRight: false, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopLeft:
                ApproachSamusForDraygonGoop(state, movingRight: false, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopLeftTail:
                FireDraygonGoop(state, movingRight: false, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GoopLeftRecovery:
                ExitDraygonGoopPass(state, movingRight: false, samus);
                break;
            case DraygonAiFunction.TryGrabSamus:
                ChaseAndTryToGrabSamus(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.GrabbedSamus:
                RepelDraygonWithGrapple(state, samus);
                break;
            case DraygonAiFunction.CarrySamus:
                CarrySamusToDraygonSpiral(state, samus);
                break;
            case DraygonAiFunction.FlailWithSamus:
                CarrySamusInDraygonSpiral(state, samus);
                break;
            case DraygonAiFunction.TailWhipWithSamus:
                RunDraygonTailWhip(state, samus);
                break;
            case DraygonAiFunction.FinalTailWhips:
                BeginDraygonFinalTailWhips(state, samus);
                break;
            case DraygonAiFunction.FinalTailWhipsWait:
                WaitForDraygonFinalTailWhips(state, samus);
                break;
            case DraygonAiFunction.ReleaseSamus:
                ReleaseSamusFromDraygon(state, samus);
                break;
            case DraygonAiFunction.FlyStraightUp:
                FlyDraygonStraightUp(state, samus, nmiFrameCounter8);
                break;
            case DraygonAiFunction.Dying:
                DriftDyingDraygonToBurialPoint(state);
                break;
            case DraygonAiFunction.DyingSink:
                WaitForDraygonBurialEvirs(state, nmiFrameCounter8);
                break;
            case DraygonAiFunction.DyingFinish:
                SinkDraygonBelowTheRoom(state, nmiFrameCounter8);
                break;
            default:
                throw new InvalidDataException(
                    $"Draygon body function $A5:{(ushort)state.Function:X4} is not translated.");
        }

        // These are four physical enemy records, but Draygon_Main makes the body coordinate
        // authoritative after its function returns. Copying here preserves that ordering for
        // the eye AI and each part's later instruction tick in the same enemy frame.
        state.Eye!.XPosition = state.Tail!.XPosition = state.Arms!.XPosition = body.XPosition;
        state.Eye.YPosition = state.Tail.YPosition = state.Arms.YPosition = body.YPosition;
    }

    /// <summary>Ports the no-op tail/arms main AI and the active eye dispatcher.</summary>
    private static void RunDraygonPartMain(RoomEnemySlot part, SamusState? samus)
    {
        if (part.EnemyDefinitionPointer is DraygonTailDefinition or DraygonArmsDefinition)
            return;
        if (part.EnemyDefinitionPointer != DraygonEyeDefinition)
            throw new InvalidDataException("Draygon part dispatcher received another family.");

        switch (part.VariableA)
        {
            case DraygonCodePointers.RTS_A5804B:
                return;
            case DraygonCodePointers.Function_DraygonEye_FacingLeft:
                TrackSamusWithDraygonEye(part, samus, facingRight: false);
                return;
            case DraygonCodePointers.Function_DraygonEye_FacingRight:
                TrackSamusWithDraygonEye(part, samus, facingRight: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Draygon eye function $A5:{part.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Ports the opening 256-frame delay at <c>$A5:871B</c>.</summary>
    private void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        if (state.FunctionTimer < 0x0100)
        {
            if (state.FunctionTimer == 0)
            {
                byte[] tiles = new byte[0x0600];
                for (int index = 0; index < tiles.Length; index++)
                    tiles[index] = _bus!.ReadByte(DraygonIntroEvirTiles + index);
                _vram!.LoadBytes(0xda00, tiles); // Native VRAM word destination $6D00.
                state.IntroEvirGraphicsLoaded = true;

                // CreateSpriteAtPos searches descending, so these become native indexes
                // $3E,$3C,$3A,$38—the exact four slots later traversed by $A5:A13E.
                for (int actor = 0; actor < 4; actor++)
                {
                    if (SpawnRoomSpriteObject(
                            0x0010,
                            0x0180,
                            RoomSpriteObjectKind.DraygonIntroEvir,
                            0x0e00) is not null)
                    {
                        state.IntroEvirsSpawned++;
                    }
                }
            }
            state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
            return;
        }

        state.Function = DraygonAiFunction.IntroDance;
        state.FunctionTimer = 0;
        state.LeftSideResetXPosition = state.Body.XPosition;
        state.RightSideResetXPosition = unchecked((ushort)(state.Body.XPosition + 0x02a0));
        state.ResetYPosition = state.Body.YPosition;
        state.Body.XPosition = state.LeftSideResetXPosition;
        state.Body.YPosition = state.ResetYPosition;
        state.SwoopYAcceleration = 0x0018;
    }

    /// <summary>Ports the 1,232-frame Evir dance at <c>$A5:878B/A13E</c>.</summary>
    private void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        if (state.FunctionTimer >= 0x04d0)
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.FunctionTimer = 0;
            return;
        }

        // The native routine processes only object indexes $3E..$38. Its latency lookup
        // unexpectedly uses the first four of eight entries, producing 128 inert frames;
        // retain that retail quirk rather than choosing the seemingly intended entries.
        for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
        {
            RoomSpriteObjectSlot sprite = _roomSpriteObjects[slotIndex];
            int latencyAddress = DraygonEvirMovementLatencies + (slotIndex - 28) * 2;
            ushort streamIndex = unchecked((ushort)(
                state.FightIntroDanceIndex + ReadWord(_bus!, latencyAddress)));
            if (unchecked((short)streamIndex) < 0 || !sprite.IsActive)
                continue;

            byte xDelta = _bus!.ReadByte(DraygonFightIntroDanceData + streamIndex);
            byte yDelta = _bus.ReadByte(
                DraygonFightIntroDanceData + unchecked((ushort)(streamIndex + 1)));
            if (xDelta == 0x80 && yDelta == 0x80)
            {
                sprite.Clear();
                continue;
            }

            sprite.XPosition = unchecked((ushort)(sprite.XPosition + unchecked((sbyte)xDelta)));
            sprite.YPosition = unchecked((ushort)(sprite.YPosition + unchecked((sbyte)yDelta)));
        }

        state.FightIntroDanceIndex = unchecked((ushort)(state.FightIntroDanceIndex + 4));
        state.IntroDanceFrames++;
        state.FunctionTimer = unchecked((ushort)(state.FunctionTimer + 1));
    }

    /// <summary>Ports swoop-right setup and table construction at <c>$A5:87F4-$88B0</c>.</summary>
    private void SetupDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");

        BuildDraygonSwoopYPositions(state, samus);
        state.Function = DraygonAiFunction.SwoopRightDescending;
        InstallDraygonInstruction(state.Body, DraygonInstructionLists.Ilist_97D1);
        state.FacingRight = true;
    }

    private static void BuildDraygonSwoopYPositions(DraygonEnemyState state, SamusState samus)
    {
        ushort yPosition = 0x0180;
        ushort ySpeed = 0;
        int byteOffset = 0;
        Array.Clear(state.SwoopYPositions);

        while (true)
        {
            ySpeed = unchecked((ushort)(ySpeed + state.SwoopYAcceleration));
            ushort candidate = unchecked((ushort)(yPosition - (ySpeed >> 8)));
            if (unchecked((short)(candidate - state.ResetYPosition)) < 0)
                break;

            yPosition = candidate;
            if (byteOffset >= 0x0800)
                throw new InvalidDataException("Draygon swoop Y table exceeded its $800-byte surface.");
            state.SwoopYPositions[byteOffset / 4] = candidate;
            byteOffset += 4;
        }

        int frameCount = byteOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon generated an empty swoop path.");
        state.SwoopPathEntryCount = frameCount;
        state.SwoopYPositions[frameCount] = state.Body.YPosition;

        // UnsignedDivision_32bit divides the absolute whole-pixel distance promoted to
        // 16.16 by the number of path entries. Split the quotient into the same var_D/E
        // whole/subpixel words consumed by AddToHiLo during descent.
        ushort distance = WrappedMagnitude(unchecked((ushort)(
            state.LeftSideResetXPosition - samus.XPosition)));
        uint velocity = ((uint)distance << 16) / (uint)frameCount;
        state.Body.VariableD = unchecked((ushort)(velocity >> 16));
        state.Body.VariableE = unchecked((ushort)velocity);
        state.Body.VariableB = unchecked((ushort)byteOffset);
        state.Body.VariableC = unchecked((ushort)byteOffset);
    }

    /// <summary>Ports the descending half at <c>$A5:88B1</c>.</summary>
    private void DescendDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = body.VariableB / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon descent path index {pathIndex} is outside WRAM.");
        if (body.VariableB == 0x0068)
            InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_9C06);

        body.YPosition = state.SwoopYPositions[pathIndex];
        body.VariableB = unchecked((ushort)(body.VariableB - 4));
        if (body.VariableB == 0)
        {
            state.Function = DraygonAiFunction.SwoopRightApex;
            return;
        }
        AddDraygonHorizontalVelocity(body);
    }

    /// <summary>Ports the velocity recalculation at the bottom of the swoop, $A5:8922.</summary>
    private void CrossDraygonRightSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        RoomEnemySlot body = state.Body;
        int frameCount = body.VariableC / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the apex.");
        ushort distance = unchecked((ushort)(0x02a0 - body.XPosition));
        uint velocity = ((uint)distance << 16) / (uint)frameCount;
        body.VariableD = unchecked((ushort)(velocity >> 16));
        body.VariableE = unchecked((ushort)velocity);
        state.Function = DraygonAiFunction.SwoopRightAscending;
    }

    /// <summary>Ports the ascending half and random next-attack choice at $A5:8951.</summary>
    private void AscendDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        RoomEnemySlot body = state.Body;
        int pathIndex = body.VariableB / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon ascent path index {pathIndex} is outside WRAM.");
        if (body.VariableB == 0x0068)
            InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_9BDA);

        body.YPosition = state.SwoopYPositions[pathIndex];
        body.VariableB = unchecked((ushort)(body.VariableB + 4));
        if (body.VariableB == body.VariableC)
        {
            body.VariableB = 0;
            ushort random = RequireRandomNumber();
            state.Function = (random & 1) != 0
                ? DraygonAiFunction.SwoopLeftSetup
                : DraygonAiFunction.GoopLeftSetup;
            return;
        }
        AddDraygonHorizontalVelocity(body);
    }

    /// <summary>Ports the mirrored left-swoop setup at <c>$A5:89B3</c>.</summary>
    private void SetupDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");
        int frameCount = state.Body.VariableC / 4;
        if (frameCount == 0 || frameCount != state.SwoopPathEntryCount)
            throw new InvalidDataException("Draygon lost the generated swoop duration.");

        ushort distance = WrappedMagnitude(unchecked((ushort)(
            state.RightSideResetXPosition - samus.XPosition)));
        uint velocity = ((uint)distance << 16) / (uint)frameCount;
        state.Body.VariableD = unchecked((ushort)(velocity >> 16));
        state.Body.VariableE = unchecked((ushort)velocity);
        state.Function = DraygonAiFunction.SwoopLeftDescending;
        InstallDraygonInstruction(state.Body, DraygonInstructionLists.Ilist_97BB);
        state.FacingRight = false;
        state.Body.VariableB = state.Body.VariableC;
    }

    /// <summary>Ports the descending half at <c>$A5:8A00</c>.</summary>
    private void DescendDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = body.VariableB / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon descent path index {pathIndex} is outside WRAM.");
        if (body.VariableB == 0x0068)
            InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_9813);

        body.YPosition = state.SwoopYPositions[pathIndex];
        body.VariableB = unchecked((ushort)(body.VariableB - 4));
        if (body.VariableB == 0)
        {
            state.Function = DraygonAiFunction.SwoopLeftApex;
            return;
        }
        SubtractDraygonHorizontalVelocity(body);
    }

    /// <summary>Ports the mirrored velocity calculation at <c>$A5:8A50</c>.</summary>
    private void CrossDraygonLeftSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        RoomEnemySlot body = state.Body;
        int frameCount = body.VariableC / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the left apex.");

        // The reset coordinate is negative in the retail room. $A5:8A66 explicitly
        // negates it and adds the body's positive arena X, yielding the wrapped distance
        // from the current point back to X=$FFB0.
        ushort distance = state.LeftSideResetXPosition >= 0x8000
            ? unchecked((ushort)(
                unchecked((ushort)-state.LeftSideResetXPosition) + body.XPosition))
            : unchecked((ushort)(body.XPosition - state.LeftSideResetXPosition));
        uint velocity = ((uint)distance << 16) / (uint)frameCount;
        body.VariableD = unchecked((ushort)(velocity >> 16));
        body.VariableE = unchecked((ushort)velocity);
        state.Function = DraygonAiFunction.SwoopLeftAscending;
    }

    /// <summary>Ports the mirrored ascent and reset logic at <c>$A5:8A90</c>.</summary>
    private void AscendDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = body.VariableB / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon ascent path index {pathIndex} is outside WRAM.");
        if (body.VariableB == 0x0068)
            InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_97E7);

        body.YPosition = state.SwoopYPositions[pathIndex];
        body.VariableB = unchecked((ushort)(body.VariableB + 4));
        if (body.VariableB == body.VariableC)
        {
            ushort random = RequireRandomNumber();
            body.XPosition = state.LeftSideResetXPosition;
            if ((random & 1) != 0)
            {
                body.VariableB = 0;
                body.YPosition = state.ResetYPosition;
                state.Function = DraygonAiFunction.SwoopRightSetup;
            }
            else
            {
                state.Function = DraygonAiFunction.GoopRightSetup;
            }
            return;
        }
        SubtractDraygonHorizontalVelocity(body);
    }

    private static void AddDraygonHorizontalVelocity(RoomEnemySlot body)
    {
        uint position = ((uint)body.XPosition << 16) | body.XSubposition;
        uint velocity = ((uint)body.VariableD << 16) | body.VariableE;
        position = unchecked(position + velocity);
        body.XPosition = unchecked((ushort)(position >> 16));
        body.XSubposition = unchecked((ushort)position);
    }

    private static void SubtractDraygonHorizontalVelocity(RoomEnemySlot body)
    {
        uint position = ((uint)body.XPosition << 16) | body.XSubposition;
        uint velocity = ((uint)body.VariableD << 16) | body.VariableE;
        position = unchecked(position - velocity);
        body.XPosition = unchecked((ushort)(position >> 16));
        body.XSubposition = unchecked((ushort)position);
    }

    /// <summary>Ports the two mirrored goop setup functions at $A5:8B0A/$8C8E.</summary>
    private void SetupDraygonGoopPass(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        // Only the rightward setup calls HandleFiringWallTurret; the leftward entry at
        // $8C8E omits it. That asymmetry changes the random seed and is therefore material.
        if (movingRight)
            ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);

        RoomEnemySlot body = state.Body;
        body.XPosition = movingRight ? (ushort)0xffb0 : state.RightSideResetXPosition;
        body.YPosition = 0x0180;
        body.VariableD = 1;
        body.VariableE = 0;
        state.GoopYOscillationAngle = 0;
        // Retail's leftward setup briefly requests the right-facing apex arm list before
        // the body reset opcode replaces all four lists. Preserve the literal write.
        InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_9C06);
        state.Function = movingRight
            ? DraygonAiFunction.GoopRight
            : DraygonAiFunction.GoopLeft;
        InstallDraygonInstruction(
            body,
            movingRight
                ? DraygonInstructionLists.Ilist_97D1
                : DraygonInstructionLists.Ilist_97BB);
        state.FacingRight = movingRight;
    }

    /// <summary>Ports the oscillating approach at $A5:8B52/$8CD4.</summary>
    private void ApproachSamusForDraygonGoop(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        SpawnPeriodicDraygonBreathBubble(state);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot position a goop pass without Samus.");

        if (WrappedMagnitude(unchecked((ushort)(state.Body.XPosition - samus.XPosition))) < 0x00d0)
        {
            state.Function = movingRight
                ? DraygonAiFunction.GoopRightTail
                : DraygonAiFunction.GoopLeftTail;
            state.GoopCounter = 0x0010;
            return;
        }
        MoveDraygonAlongGoopPath(state, movingRight);
    }

    /// <summary>Ports the firing and off-screen selection at $A5:8BAE/$8D30.</summary>
    private void FireDraygonGoop(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        // The right-facing function calls the turret handler; the left-facing copy does not.
        if (movingRight)
            ObserveDraygonTurretCadence(state, samus, nmiFrameCounter8);
        if (samus?.XSpeedDivisor != 0)
        {
            state.Function = DraygonAiFunction.TryGrabSamus;
            return;
        }

        ushort random = RequireRandomNumber();
        if ((random & 0x000f) == 0)
        {
            state.GoopCounter = unchecked((ushort)(state.GoopCounter - 1));
            if (state.GoopCounter == 0)
            {
                FinishDraygonGoopFiring(state, movingRight);
                return;
            }
            InstallDraygonInstruction(
                state.Body,
                movingRight
                    ? DraygonInstructionLists.Ilist_9C90
                    : DraygonInstructionLists.Ilist_98FE);
        }

        MoveDraygonAlongGoopPath(state, movingRight);
        ushort x = state.Body.XPosition;
        bool crossedExit = movingRight
            ? unchecked((short)x) >= 0 && x >= 0x02a0
            : unchecked((short)x) < 0 && unchecked((short)(x - 0xffb0)) < 0;
        if (crossedExit)
            FinishDraygonGoopFiring(state, movingRight);
    }

    private static void FinishDraygonGoopFiring(DraygonEnemyState state, bool movingRight)
    {
        InstallDraygonInstruction(
            state.Arms!,
            movingRight
                ? DraygonInstructionLists.Ilist_9BDA
                : DraygonInstructionLists.Ilist_97E7);
        state.Function = movingRight
            ? DraygonAiFunction.GoopRightRecovery
            : DraygonAiFunction.GoopLeftRecovery;
    }

    /// <summary>Ports the final off-screen movement at $A5:8C33/$8DB2.</summary>
    private void ExitDraygonGoopPass(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus = null)
    {
        SpawnPeriodicDraygonBreathBubble(state);
        // Only the leftward recovery rechecks whether Samus became gooped after firing.
        if (!movingRight && samus?.XSpeedDivisor != 0)
        {
            state.Function = DraygonAiFunction.TryGrabSamus;
            return;
        }

        MoveDraygonAlongGoopPath(state, movingRight);
        ushort x = state.Body.XPosition;
        bool offScreen = movingRight
            ? unchecked((short)x) >= 0 && x >= 0x02a0
            : unchecked((short)x) < 0 && unchecked((short)(x - 0xffb0)) < 0;
        if (!offScreen)
            return;

        state.Body.VariableB = state.Body.VariableC;
        if (movingRight)
        {
            state.Function = DraygonAiFunction.SwoopLeftSetup;
            state.Body.XPosition = 0x0250;
        }
        else
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.Body.XPosition = 0xffb0;
        }
        state.Body.YPosition = 0xffb0;
    }

    private void MoveDraygonAlongGoopPath(DraygonEnemyState state, bool movingRight)
    {
        state.Body.YPosition = unchecked((ushort)(
            0x0180 + ReadEightBitCosineProduct(state.GoopYOscillationAngle, 0x0020)));
        state.GoopYOscillationAngle = unchecked((ushort)(
            (state.GoopYOscillationAngle + 1) & 0x00ff));
        if (movingRight)
            AddDraygonHorizontalVelocity(state.Body);
        else
            SubtractDraygonHorizontalVelocity(state.Body);
    }

    private void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if ((state.Body.FrameCounter & 0x007f) != 0)
            return;
        if (SpawnRoomSpriteObject(
                unchecked((ushort)(state.Body.XPosition - 16)),
                unchecked((ushort)(state.Body.YPosition - 16)),
                RoomSpriteObjectKind.DraygonBreathBubble,
                graphicsIndex: 0) is not null)
        {
            state.BreathBubblesSpawned++;
        }
    }

    private static void InstallDraygonInstruction(RoomEnemySlot part, ushort instruction)
    {
        part.CurrentInstruction = instruction;
        part.InstructionTimer = 1;
    }

    private void ObserveDraygonTurretCadence(
        DraygonEnemyState state,
        SamusState? samus,
        byte nmiFrameCounter8)
    {
        if ((nmiFrameCounter8 & 0x3f) != 0)
            return;

        // $A5:87AA advances the room RNG even when the chosen turret is disabled. Projectile
        // production is intentionally left for the bank-$86 Draygon projectile slice, but
        // preserving this draw now keeps every later attack decision on the cartridge seed.
        ushort random = _nextRandom!();
        state.TurretCadenceChecks++;
        if (samus is not null)
            SpawnDraygonWallTurret(state, samus, random);
    }

    /// <summary>Ports the eye direction partition at <c>$A5:C48D/C513</c>.</summary>
    private static void TrackSamusWithDraygonEye(RoomEnemySlot eye, SamusState? samus, bool facingRight)
    {
        if (samus is null)
            return;

        short deltaX = unchecked((short)(samus.XPosition - unchecked((ushort)(
            eye.XPosition + (facingRight ? 0x0018 : -0x0018)))));
        short deltaY = unchecked((short)(samus.YPosition - unchecked((ushort)(eye.YPosition - 0x0020))));
        byte angle = CalculateCartridgeAngle(deltaX, deltaY);
        if (angle == (byte)eye.VariableF)
            return;

        eye.VariableF = angle;
        eye.CurrentInstruction = angle switch
        {
            < 0x20 => facingRight
                ? DraygonInstructionLists.Ilist_9D5C
                : DraygonInstructionLists.Ilist_99BA,
            < 0x60 => facingRight
                ? DraygonInstructionLists.Ilist_9D50
                : DraygonInstructionLists.Ilist_99B4,
            < 0xa0 => facingRight
                ? DraygonInstructionLists.Ilist_9D62
                : DraygonInstructionLists.Ilist_99C0,
            < 0xe0 => facingRight
                ? DraygonInstructionLists.Ilist_9D56
                : DraygonInstructionLists.Ilist_99AE,
            _ => facingRight
                ? DraygonInstructionLists.Ilist_9D5C
                : DraygonInstructionLists.Ilist_99BA,
        };
        eye.InstructionTimer = 1;
        eye.Timer = 0;
    }

    private DraygonEnemyState RequireCompleteDraygonState(RoomEnemySlot body)
    {
        DraygonEnemyState state = _draygon ??
            throw new InvalidDataException("Draygon body has no encounter state.");
        if (!ReferenceEquals(state.Body, body) || state.Eye is null ||
            state.Tail is null || state.Arms is null)
        {
            throw new InvalidDataException("Draygon's four physical records are not linked.");
        }
        return state;
    }

    /// <summary>Handles the bank-$A5 opcodes reached during load and the opening dance.</summary>
    private bool TryProcessDraygonInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort instruction,
        ref ushort cursor)
    {
        if (!IsDraygonDefinition(slot.EnemyDefinitionPointer))
            return false;

        DraygonEnemyState state = _draygon ??
            throw new InvalidDataException("Draygon instruction ran without encounter state.");
        int bank = slot.Definition.Bank << 16;
        switch (instruction)
        {
            case DraygonCodePointers.Instruction_Draygon_SetInstList_Body_Eye_Tail_Arms:
                state.Body.CurrentInstruction =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                state.Eye!.CurrentInstruction =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 4)));
                state.Tail!.CurrentInstruction =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 6)));
                state.Arms!.CurrentInstruction =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 8)));
                state.Body.InstructionTimer = state.Eye.InstructionTimer =
                    state.Tail.InstructionTimer = state.Arms.InstructionTimer = 1;
                cursor = unchecked((ushort)(cursor + 10));
                return true;

            case DraygonCodePointers.Instruction_Draygon_RoomLoadingInterruptCmd_BeginHUDDraw:
                state.RoomLoadingIrqCommand = 0x000c;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_Draygon_EyeFunctionInY:
                if (state.Eye is null)
                    throw new InvalidDataException("Draygon eye-function opcode ran before eye load.");
                state.Eye.VariableA = ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case DraygonCodePointers.Instruction_Draygon_FunctionInY:
                slot.VariableA = ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case DraygonCodePointers.Instruction_DraygonBody_DisplaceGraphics:
                state.BodyGraphicsXDisplacement =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                state.BodyGraphicsYDisplacement =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 4)));
                cursor = unchecked((ushort)(cursor + 6));
                return true;

            case DraygonCodePointers.Instruction_Draygon_QueueSFXInY_Lib2_Max6:
                state.LastSoundLibrary2 =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case DraygonCodePointers.Instruction_Draygon_QueueSFXInY_Lib3_Max6:
                state.LastSoundLibrary3 =
                    ReadWord(_bus!, bank | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case DraygonCodePointers.Inst_Draygon_SpawnDyingDraygonSpriteObject_BigDustCloud:
                SpawnRandomDyingDraygonObject(state, RoomSpriteObjectKind.DustCloud);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Inst_Draygon_SpawnDyingDraygonSpriteObject_SmallExplosion:
                SpawnRandomDyingDraygonObject(state, RoomSpriteObjectKind.SporeSpawnDyingExplosion);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Inst_Draygon_SpawnDyingDraygonSpriteObject_BigExplosion:
                SpawnRandomDyingDraygonObject(state, RoomSpriteObjectKind.BotwoonLargeExplosion);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Inst_Draygon_SpawnDyingDraygonSpriteObject_BreathBubbles:
                SpawnRandomDyingDraygonObject(state, RoomSpriteObjectKind.DraygonBreathBubble);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_Draygon_ParalyseDraygonTailAndArms:
                InstallDraygonInstruction(state.Tail!, DraygonInstructionLists.Ilist_97B9);
                InstallDraygonInstruction(state.Arms!, DraygonInstructionLists.Ilist_97B9);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_DraygonBody_SetAsIntangible:
                state.Body.Properties =
                    state.Body.Properties.With(EnemyProperties.IgnoreSamusCollision);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_Draygon_BodyFunctionInY:
                state.Function = (DraygonAiFunction)ReadWord(
                    _bus!,
                    bank | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;

            case DraygonCodePointers.Instruction_DraygonTail_TailWhipHit:
                ApplyDraygonTailWhipHit(state, samus);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_Draygon_SpawnGoop_Leftwards:
                SpawnDraygonGoop(state, movingRight: false);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case DraygonCodePointers.Instruction_Draygon_SpawnGoop_Rightwards:
                SpawnDraygonGoop(state, movingRight: true);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }
}
