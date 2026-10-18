using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using System.Buffers.Binary;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The Draygon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class DraygonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.DraygonArms, EnemyDefinitionId.DraygonBody, EnemyDefinitionId.DraygonEye, EnemyDefinitionId.DraygonTail];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new DraygonEntity(this, slot, common);

    /// <summary>Provides the active room bus, random source, and persistent boss state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Draygon's goop, eye drops, and other enemy projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes the room shaking caused by Draygon's attacks and death.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues encounter sound effects and music through the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Allocates breath bubbles, death smoke, and burial Evir sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Updates the body and eye palette bands used by Draygon's presentation.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies frame timing for the body and linked-part AI callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Evaluates Samus and projectile contacts against Draygon's linked parts.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies common death and drop behavior after Draygon's special sequence.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Tracks room kill state and Samus release during the encounter.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Restricts camera scrolling while Draygon controls the arena.</summary>
    private readonly SamusScrollCap _scrollCap;

    /// <summary>Binds the encounter's room, projectile, presentation, and interaction owners.</summary>
    internal DraygonBehavior(DraygonBehaviorDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _spriteObjects = dependencies.SpriteObjects;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _roomStatus = dependencies.RoomStatus;
        _scrollCap = dependencies.ScrollCap;
    }

    /// <summary>The typed encounter extension while the retail Draygon population is loaded.</summary>
    public DraygonEnemyState? Draygon { get; internal set; }

    /// <summary>Recognizes the four enemy definitions that form one Draygon encounter.</summary>
    internal static bool IsDraygonDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.DraygonBody or EnemyDefinitionId.DraygonEye or
        EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms;

    /// <summary>Discards the linked encounter state before another room is loaded.</summary>
    internal void ResetDraygonRoomState() => Draygon = null;

    /// <summary>Ports <c>InitAI_DraygonBody</c> at <c>$A5:8687</c>.</summary>
    internal void InitializeDraygonBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
            throw new InvalidDataException("Draygon's body must own native enemy slot $0000.");

        // The native copy targets colors 144..168. This runtime has one concrete CGRAM
        // surface instead of a second fade-target array, so publish those exact 25 colors
        // immediately; the room's normal fade-in controls when they become visible.
        (_room.Artwork?.DraygonColors ?? throw new InvalidOperationException(
            "Draygon requires installed color artwork.")).ApplyIntro(_presentation.Cgram);

        // $7E:2000 is the enemy BG2 staging surface and the following NMI copies it to
        // VRAM word $4800. Fill all $800 words, not merely the currently visible page.
        byte[] bg2Bytes = new byte[2 * EnemyBg2SurfaceDefinitions.WordCount];
        for (int byteIndex = 0; byteIndex < bg2Bytes.Length; byteIndex += 2)
            BinaryPrimitives.WriteUInt16LittleEndian(bg2Bytes.AsSpan(byteIndex), EnemyBg2SurfaceDefinitions.BlankTile.Raw);
        new EnemyVram(_presentation.Vram).EnemyBg2Tilemap.Load(bg2Bytes);

        body.PaletteIndex = EnemyPaletteBits.Palette7;
        body.InstructionTimer = NativeCountdown.Start(1);

        _frameContext._processAllEnemies = true;
        Draygon = new DraygonEnemyState(body)
        {
            BackgroundTilemapPrepared = true,
            RoomLoadingIrqCommand = RoomLoadingInterruptCommand.BeginHudDraw,
            MinimapDisabledAndBossTilesExplored = true,
            Function = DraygonAiFunction.IntroInitialDelay,
        };
        body.SetSpeciesState(Draygon, Draygon.ClearWithRecord);
        Draygon.SetCursor(body, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.BodyFacingLeftIdle));
        Draygon.DisabledCannonWords.Add(DraygonCannonControlWord.UnusedBottom);
    }

    /// <summary>Ports the independent initializers at <c>$A5:C46B/C599/C5AD</c>.</summary>
    private void InitializeDraygonPart(RoomEnemySlot part)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part appeared before the body record.");

        part.InstructionTimer = NativeCountdown.Start(1);
        switch (part.EnemyDefinitionPointer)
        {
            case EnemyDefinitionId.DraygonEye:
                if (part.SlotIndex != 1)
                    throw new InvalidDataException("Draygon's eye must own native slot $0040.");
                // Inert until the body publishes facing AI.
                var eyeState = new DraygonEyeState { Function = DraygonEyeFunction.Inert };
                part.SetSpeciesState(eyeState, eyeState.ClearWithRecord);
                state.Eye = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.EyeFacingLeftIdle));
                return;

            case EnemyDefinitionId.DraygonTail:
                if (part.SlotIndex != 2)
                    throw new InvalidDataException("Draygon's tail must own native slot $0080.");
                part.PaletteIndex = EnemyPaletteBits.Palette7;
                state.Tail = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.TailFacingLeftInitialFakeWhip));
                return;

            case EnemyDefinitionId.DraygonArms:
                if (part.SlotIndex != 3)
                    throw new InvalidDataException("Draygon's arms must own native slot $00C0.");
                // Body init briefly writes $9813 before this record exists. Retail part init
                // clears that lost write and installs the ordinary idle loop at $97E7.
                part.PaletteIndex = EnemyPaletteBits.Palette7;
                part.Layer = 2;
                state.Arms = part;
                state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.ArmsFacingLeftIdle));
                return;

            default:
                throw new InvalidDataException(
                    $"Enemy ${(int)part.EnemyDefinitionPointer:X4} is not a Draygon part.");
        }
    }

    /// <summary>Ports <c>MainAI_DraygonBody</c> at <c>$A5:86FC</c>.</summary>
    private void RunDraygonBodyMain(RoomEnemySlot body, SamusState? samus, byte frameCounterLow)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        switch (state.Function)
        {
            case DraygonAiFunction.IntroInitialDelay:
                RunDraygonIntroInitialDelay(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.IntroDance:
                RunDraygonIntroDance(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightSetup:
                SetupDraygonRightSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightDescending:
                DescendDraygonRightSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightApex:
                CrossDraygonRightSwoopApex(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopRightAscending:
                AscendDraygonRightSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftSetup:
                SetupDraygonLeftSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftDescending:
                DescendDraygonLeftSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftApex:
                CrossDraygonLeftSwoopApex(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.SwoopLeftAscending:
                AscendDraygonLeftSwoop(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRightSetup:
                SetupDraygonGoopPass(state, movingRight: true, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRight:
                ApproachSamusForDraygonGoop(state, movingRight: true, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRightTail:
                FireDraygonGoop(state, movingRight: true, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopRightRecovery:
                ExitDraygonGoopPass(state, movingRight: true);
                break;
            case DraygonAiFunction.GoopLeftSetup:
                SetupDraygonGoopPass(state, movingRight: false, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeft:
                ApproachSamusForDraygonGoop(state, movingRight: false, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeftTail:
                FireDraygonGoop(state, movingRight: false, samus, frameCounterLow);
                break;
            case DraygonAiFunction.GoopLeftRecovery:
                ExitDraygonGoopPass(state, movingRight: false, samus);
                break;
            case DraygonAiFunction.TryGrabSamus:
                ChaseAndTryToGrabSamus(state, samus, frameCounterLow);
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
                FlyDraygonStraightUp(state, samus, frameCounterLow);
                break;
            case DraygonAiFunction.Dying:
                DriftDyingDraygonToBurialPoint(state);
                break;
            case DraygonAiFunction.DyingSink:
                WaitForDraygonBurialEvirs(state, frameCounterLow);
                break;
            case DraygonAiFunction.DyingFinish:
                SinkDraygonBelowTheRoom(state, frameCounterLow);
                break;
            default:
                throw new InvalidDataException(
                    $"Draygon body function {state.Function} is not translated.");
        }

        // These are four physical enemy records, but Draygon_Main makes the body coordinate
        // authoritative after its function returns. Copying here preserves that ordering for
        // the eye AI and each part's later instruction tick in the same enemy frame.
        state.Arms.Position = state.Arms.Position.WithWholeX(body.Position.X.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeX(body.Position.X.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeX(body.Position.X.Whole);
        state.Arms.Position = state.Arms.Position.WithWholeY(body.Position.Y.Whole);
        state.Tail.Position = state.Tail.Position.WithWholeY(body.Position.Y.Whole);
        state.Eye.Position = state.Eye.Position.WithWholeY(body.Position.Y.Whole);
    }

    /// <summary>Ports the no-op tail/arms main AI and the active eye dispatcher.</summary>
    private void RunDraygonPartMain(RoomEnemySlot part, SamusState? samus)
    {
        if (part.EnemyDefinitionPointer is EnemyDefinitionId.DraygonTail or EnemyDefinitionId.DraygonArms)
            return;
        if (part.EnemyDefinitionPointer != EnemyDefinitionId.DraygonEye)
            throw new InvalidDataException("Draygon part dispatcher received another family.");

        DraygonEyeFunction function = RequireEyeState(part).Function
            ?? throw new InvalidDataException("Draygon's eye has no function.");
        switch (function)
        {
            case DraygonEyeFunction.Inert:
                return;
            case DraygonEyeFunction.FacingLeft:
                TrackSamusWithDraygonEye(part, samus, facingRight: false);
                return;
            case DraygonEyeFunction.FacingRight:
                TrackSamusWithDraygonEye(part, samus, facingRight: true);
                return;
            default:
                throw new InvalidOperationException($"Undefined Draygon eye function {function}.");
        }
    }

    /// <summary>Ports the opening 256-frame delay at <c>$A5:871B</c>.</summary>
    private void RunDraygonIntroInitialDelay(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (state.FunctionTimer.RawValue < 256)
        {
            if (state.FunctionTimer.IsZero)
            {
                RoomEnemyDefinition evir = RoomEnemyDefinitionCatalog.Get(EnemyDefinitionId.Evir);
                if ((evir.TileDataSize & short.MaxValue) !=
                    DraygonIntroPresentationDefinitions.EvirTilesByteCount)
                    throw new InvalidDataException(
                        "Draygon opening Evir sheet no longer has the compiled transfer length.");
                if (_room.Artwork?.TryResolve(evir.TileDataAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount,
                        out ReadOnlyMemory<byte> installedTiles) != true)
                    throw new InvalidDataException(
                        "Installed Evir graphics are missing for Draygon's opening transfer.");
                new EnemyVram(_presentation.Vram).DraygonEvirTiles.Load(installedTiles.Span);
                state.IntroEvirGraphicsLoaded = true;

                // CreateSpriteAtPos searches descending, so these become native indexes
                // $3E,$3C,$3A,$38—the exact four slots later traversed by $A5:A13E.
                for (int actor = 0; actor < 4; actor++)
                {
                    if (_spriteObjects.SpawnRoomSpriteObject(
                            new(16, 384),
                            RoomSpriteObjectKind.DraygonIntroEvir,
                            EnemyPaletteBits.Palette7) is not null)
                    {
                        state.IntroEvirsSpawned++;
                    }
                }
            }
            state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
            return;
        }

        state.Function = DraygonAiFunction.IntroDance;
        state.FunctionTimer = NativeCountdown.Start(0);
        state.LeftSideReset = state.Body.Position.Whole;
        state.RightSideReset = state.LeftSideReset + DraygonRightSideResetDistance;
        state.Body.Position = state.Body.Position.WithWhole(state.LeftSideReset);
        state.SwoopAcceleration = DraygonInitialSwoopAcceleration;
    }

    /// <summary>Ports the 1,232-frame Evir dance at <c>$A5:878B/A13E</c>.</summary>
    internal void RunDraygonIntroDance(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (state.FunctionTimer.RawValue >= DraygonIntroDanceDefinitions.DurationFrames)
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.FunctionTimer = NativeCountdown.Start(0);
            return;
        }

        // The native routine processes only object indexes $3E..$38. Its latency lookup
        // unexpectedly uses the first four of eight entries, producing 128 inert frames;
        // retain that retail quirk rather than choosing the seemingly intended entries.
        for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
        {
            RoomSpriteObjectSlot sprite = _spriteObjects.Slots[slotIndex];
            int streamIndex = state.FightIntroDanceIndex +
                DraygonIntroDanceDefinitions.MovementLatencyForSlot(slotIndex);
            if (streamIndex < 0 || !sprite.IsActive)
                continue;

            DraygonIntroMovement movement =
                DraygonIntroDanceDefinitions.ResolveMovement(checked((ushort)streamIndex));

            if (movement.DeletesSprite)
            {
                sprite.Clear();
                continue;
            }

            sprite.Position += movement.Delta;
        }

        state.FightIntroDanceIndex += DraygonIntroDanceDefinitions.StreamIndexAdvance;
        state.IntroDanceFrames++;
        state.FunctionTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.FunctionTimer.RawValue + 1)));
    }

    /// <summary>Ports swoop-right setup and table construction at <c>$A5:87F4-$88B0</c>.</summary>
    private void SetupDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");

        BuildDraygonSwoopYPositions(state, samus);
        state.Function = DraygonAiFunction.SwoopRightDescending;
        StartDraygonProgram(state.Body, DraygonProgram.BodyFacingRightReset);
        state.FacingRight = true;
    }

    /// <summary>Captures Samus's height and computes the vertical waypoints for the next swoop.</summary>
    internal static void BuildDraygonSwoopYPositions(DraygonEnemyState state, SamusState samus)
    {
        ushort yPosition = 384;
        EightEightVector<RoomPixels> speed = default;
        int byteOffset = 0;
        Array.Clear(state.SwoopYPositions);

        while (true)
        {
            speed += state.SwoopAcceleration;
            ushort candidate = unchecked((ushort)(yPosition - (SnesEightEight.WholeOf(speed.Y.RawValue))));
            if (unchecked((short)(candidate - state.LeftSideReset.Y)) < 0)
                break;

            yPosition = candidate;
            if (byteOffset >= 2048)
                throw new InvalidDataException("Draygon swoop Y table exceeded its $800-byte surface.");
            state.SwoopYPositions[byteOffset / 4] = candidate;
            byteOffset += 4;
        }

        int frameCount = byteOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon generated an empty swoop path.");
        state.SwoopPathEntryCount = frameCount;
        state.SwoopYPositions[frameCount] = state.Body.Position.Y.Whole;

        // UnsignedDivision_32bit divides the absolute whole-pixel distance promoted to
        // 16.16 by the number of path entries. Split the quotient into the same var_D/E
        // whole/subpixel words consumed by AddToHiLo during descent.
        ushort distance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.LeftSideReset.X - samus.Position.X.Whole)));
        uint velocity = SnesSixteenSixteen.FromWhole((uint)distance) / (uint)frameCount;
        state.SwoopVelocity = velocity;
        state.SwoopOffset = byteOffset;
        state.SwoopEndOffset = byteOffset;
    }

    /// <summary>Ports the descending half at <c>$A5:88B1</c>.</summary>
    private void DescendDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon descent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonSwoopApexArmsByteOffset)
            StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingRightNearSwoopApex);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset - 4;
        if (state.SwoopOffset == 0)
        {
            state.Function = DraygonAiFunction.SwoopRightApex;
            return;
        }
        AddDraygonHorizontalVelocity(state);
    }

    /// <summary>Ports the velocity recalculation at the bottom of the swoop, $A5:8922.</summary>
    private void CrossDraygonRightSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the apex.");
        ushort distance = unchecked((ushort)(672 - body.Position.X.Whole));
        uint velocity = SnesSixteenSixteen.FromWhole((uint)distance) / (uint)frameCount;
        state.SwoopVelocity = velocity;
        state.Function = DraygonAiFunction.SwoopRightAscending;
    }

    /// <summary>Ports the ascending half and random next-attack choice at $A5:8951.</summary>
    private void AscendDraygonRightSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon ascent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonSwoopApexArmsByteOffset)
            StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingRightIdle);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset + 4;
        if (state.SwoopOffset == state.SwoopEndOffset)
        {
            state.SwoopOffset = 0;
            ushort random = _room.Random.Read();
            state.Function = (random & 1) != 0
                ? DraygonAiFunction.SwoopLeftSetup
                : DraygonAiFunction.GoopLeftSetup;
            return;
        }
        AddDraygonHorizontalVelocity(state);
    }

    /// <summary>Ports the mirrored left-swoop setup at <c>$A5:89B3</c>.</summary>
    private void SetupDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot aim a swoop without Samus.");
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0 || frameCount != state.SwoopPathEntryCount)
            throw new InvalidDataException("Draygon lost the generated swoop duration.");

        ushort distance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.RightSideReset.X - samus.Position.X.Whole)));
        uint velocity = SnesSixteenSixteen.FromWhole((uint)distance) / (uint)frameCount;
        state.SwoopVelocity = velocity;
        state.Function = DraygonAiFunction.SwoopLeftDescending;
        StartDraygonProgram(state.Body, DraygonProgram.BodyFacingLeftReset);
        state.FacingRight = false;
        state.SwoopOffset = state.SwoopEndOffset;
    }

    /// <summary>Ports the descending half at <c>$A5:8A00</c>.</summary>
    private void DescendDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon descent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonSwoopApexArmsByteOffset)
            StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingLeftNearSwoopApex);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset - 4;
        if (state.SwoopOffset == 0)
        {
            state.Function = DraygonAiFunction.SwoopLeftApex;
            return;
        }
        SubtractDraygonHorizontalVelocity(state);
    }

    /// <summary>Ports the mirrored velocity calculation at <c>$A5:8A50</c>.</summary>
    private void CrossDraygonLeftSwoopApex(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        int frameCount = state.SwoopEndOffset / 4;
        if (frameCount == 0)
            throw new InvalidDataException("Draygon lost the swoop duration at the left apex.");

        // The reset coordinate is negative in the retail room. $A5:8A66 explicitly
        // negates it and adds the body's positive arena X, yielding the wrapped distance
        // from the current point back to X=$FFB0.
        ushort resetX = state.LeftSideReset.X;
        ushort distance = unchecked((short)resetX) < 0
            ? unchecked((ushort)(
                unchecked((ushort)-resetX) + body.Position.X.Whole))
            : unchecked((ushort)(body.Position.X.Whole - resetX));
        uint velocity = SnesSixteenSixteen.FromWhole((uint)distance) / (uint)frameCount;
        state.SwoopVelocity = velocity;
        state.Function = DraygonAiFunction.SwoopLeftAscending;
    }

    /// <summary>Ports the mirrored ascent and reset logic at <c>$A5:8A90</c>.</summary>
    private void AscendDraygonLeftSwoop(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        SpawnPeriodicDraygonBreathBubble(state);
        RoomEnemySlot body = state.Body;
        int pathIndex = state.SwoopOffset / 4;
        if ((uint)pathIndex >= state.SwoopYPositions.Length)
            throw new InvalidDataException($"Draygon ascent path index {pathIndex} is outside WRAM.");
        if (state.SwoopOffset == DraygonSwoopApexArmsByteOffset)
            StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingLeftIdle);

        body.Position = body.Position.WithWholeY(state.SwoopYPositions[pathIndex]);
        state.SwoopOffset = state.SwoopOffset + 4;
        if (state.SwoopOffset == state.SwoopEndOffset)
        {
            ushort random = _room.Random.Read();
            body.Position = body.Position.WithWholeX(state.LeftSideReset.X);
            if ((random & 1) != 0)
            {
                state.SwoopOffset = 0;
                body.Position = body.Position.WithWholeY(state.LeftSideReset.Y);
                state.Function = DraygonAiFunction.SwoopRightSetup;
            }
            else
            {
                state.Function = DraygonAiFunction.GoopRightSetup;
            }
            return;
        }
        SubtractDraygonHorizontalVelocity(state);
    }

    /// <summary>Advances Draygon's X position using the current signed 16.16 velocity.</summary>
    private static void AddDraygonHorizontalVelocity(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        uint position = body.Position.X.RawValue;
        uint velocity = state.SwoopVelocity;
        position = unchecked(position + velocity);
        body.Position = body.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }

    /// <summary>Moves Draygon left by the current 16.16 velocity without losing its fraction.</summary>
    private static void SubtractDraygonHorizontalVelocity(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        uint position = body.Position.X.RawValue;
        uint velocity = state.SwoopVelocity;
        position = unchecked(position - velocity);
        body.Position = body.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)position)));
    }

    /// <summary>Ports the two mirrored goop setup functions at $A5:8B0A/$8C8E.</summary>
    private void SetupDraygonGoopPass(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte frameCounterLow)
    {
        // Only the rightward setup calls HandleFiringWallTurret; the leftward entry at
        // $8C8E omits it. That asymmetry changes the random seed and is therefore material.
        if (movingRight)
            ObserveDraygonTurretCadence(state, samus, frameCounterLow);

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(new(movingRight ? DraygonOffscreenLeftX : state.RightSideReset.X, 384));
        state.SwoopVelocity = SnesSixteenSixteen.FromWhole(1u);
        state.GoopYOscillationAngle = SnesAngle.Zero;
        // Retail's leftward setup briefly requests the right-facing apex arm list before
        // the body reset opcode replaces all four lists. Preserve the literal write.
        StartDraygonProgram(state.Arms, DraygonProgram.ArmsFacingRightNearSwoopApex);
        state.Function = movingRight
            ? DraygonAiFunction.GoopRight
            : DraygonAiFunction.GoopLeft;
        StartDraygonProgram(
            body,
            movingRight
                ? DraygonProgram.BodyFacingRightReset
                : DraygonProgram.BodyFacingLeftReset);
        state.FacingRight = movingRight;
    }

    /// <summary>Ports the oscillating approach at $A5:8B52/$8CD4.</summary>
    private void ApproachSamusForDraygonGoop(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        SpawnPeriodicDraygonBreathBubble(state);
        if (samus is null)
            throw new InvalidOperationException("Draygon cannot position a goop pass without Samus.");

        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(state.Body.Position.X.Whole - samus.Position.X.Whole))) < 208)
        {
            state.Function = movingRight
                ? DraygonAiFunction.GoopRightTail
                : DraygonAiFunction.GoopLeftTail;
            state.GoopCounter = NativeCountdown.Start(16);
            return;
        }
        MoveDraygonAlongGoopPath(state, movingRight);
    }

    /// <summary>Ports the firing and off-screen selection at $A5:8BAE/$8D30.</summary>
    private void FireDraygonGoop(
        DraygonEnemyState state,
        bool movingRight,
        SamusState? samus,
        byte frameCounterLow)
    {
        // The right-facing function calls the turret handler; the left-facing copy does not.
        if (movingRight)
            ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus?.XSpeedDivisor != 0)
        {
            state.Function = DraygonAiFunction.TryGrabSamus;
            return;
        }

        ushort random = _room.Random.Read();
        if ((random & 15) == 0)
        {
            state.GoopCounter = state.GoopCounter.Decrement().Remaining;
            if (state.GoopCounter.IsZero)
            {
                FinishDraygonGoopFiring(state, movingRight);
                return;
            }
            StartDraygonProgram(
                state.Body,
                movingRight
                    ? DraygonProgram.BodyFacingRightFireGoop
                    : DraygonProgram.BodyFacingLeftFireGoop);
        }

        MoveDraygonAlongGoopPath(state, movingRight);
        ushort x = state.Body.Position.X.Whole;
        bool crossedExit = movingRight
            ? unchecked((short)x) >= 0 && x >= 672
            : unchecked((short)x) < 0 && unchecked((short)(x - DraygonOffscreenLeftX)) < 0;
        if (crossedExit)
            FinishDraygonGoopFiring(state, movingRight);
    }

    /// <summary>Places Draygon offscreen and selects the next swoop after a goop pass exits.</summary>
    private void FinishDraygonGoopFiring(DraygonEnemyState state, bool movingRight)
    {
        StartDraygonProgram(
            state.Arms,
            movingRight
                ? DraygonProgram.ArmsFacingRightIdle
                : DraygonProgram.ArmsFacingLeftIdle);
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
        ushort x = state.Body.Position.X.Whole;
        bool offScreen = movingRight
            ? unchecked((short)x) >= 0 && x >= 672
            : unchecked((short)x) < 0 && unchecked((short)(x - DraygonOffscreenLeftX)) < 0;
        if (!offScreen)
            return;

        state.SwoopOffset = state.SwoopEndOffset;
        if (movingRight)
        {
            state.Function = DraygonAiFunction.SwoopLeftSetup;
            state.Body.Position = state.Body.Position.WithWholeX(592);
        }
        else
        {
            state.Function = DraygonAiFunction.SwoopRightSetup;
            state.Body.Position = state.Body.Position.WithWholeX(DraygonOffscreenLeftX);
        }
        state.Body.Position = state.Body.Position.WithWholeY(DraygonOffscreenLeftX);
    }

    /// <summary>Applies the goop pass's horizontal motion and one-step Y oscillation.</summary>
    internal static void MoveDraygonAlongGoopPath(DraygonEnemyState state, bool movingRight)
    {
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(
            384 + EnemyMath.ReadEightBitCosineProduct(state.GoopYOscillationAngle.TableIndex, 32))));
        state.GoopYOscillationAngle = state.GoopYOscillationAngle.AddTableUnits(1);
        if (movingRight)
            AddDraygonHorizontalVelocity(state);
        else
            SubtractDraygonHorizontalVelocity(state);
    }

    /// <summary>Emits a breath bubble every 128 body frames when a sprite slot is available.</summary>
    private void SpawnPeriodicDraygonBreathBubble(DraygonEnemyState state)
    {
        if ((state.Body.FrameCounter & 127) != 0)
            return;
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(state.Body.Position.X.Whole - 16)), unchecked((ushort)(state.Body.Position.Y.Whole - 16))),
                RoomSpriteObjectKind.DraygonBreathBubble,
                graphicsIndex: 0) is not null)
        {
            state.BreathBubblesSpawned++;
        }
    }

    /// <summary>
    /// <c>$FFB0</c>: the X Draygon parks at off the left edge (-80 as a signed word); a goop pass leftwards ends once
    /// the body crosses it.
    /// </summary>
    private const ushort DraygonOffscreenLeftX = unchecked((ushort)-80);

    /// <summary>Distance from the left-side reset point to the right-side one: 672 pixels to the right.</summary>
    private static readonly Offset16<RoomPixels> DraygonRightSideResetDistance = new(672, 0);

    /// <summary>The swoop acceleration a fight starts with and a tail-whip hit restores: 24/256 pixel per frame per frame downward.</summary>
    private static readonly EightEightVector<RoomPixels> DraygonInitialSwoopAcceleration = new(0, 24);

    /// <summary>The swoop acceleration each eye hit adds, 8/256 pixel per frame per frame, while below the limit.</summary>
    private static readonly EightEightVector<RoomPixels> DraygonShotSwoopAccelerationStep = new(0, 8);

    /// <summary>The swoop acceleration, in 1/256 pixel per frame per frame, an eye hit may not reach (a signed compare of the difference).</summary>
    private const int DraygonSwoopAccelerationLimit = 160;

    /// <summary>The swoop-path byte offset (sample 26) at which the arms reach for the swoop's apex.</summary>
    private const ushort DraygonSwoopApexArmsByteOffset = 104;

    /// <summary>Restarts a linked Draygon part's program on its next update, as a native list install does.</summary>
    private void StartDraygonProgram(RoomEnemySlot part, DraygonProgram program)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon program started without encounter state.");
        state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(program));
        part.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Advances Draygon's typed animation.</summary>
    internal void AdvanceDraygonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceDraygonPartAnimation(slot, context.Samus);
    }

    /// <summary>
    /// Runs a Draygon part's program where the enemy instruction interpreter would: the frame expires on the update
    /// its native timer would, a new frame raises the new-frame flag, waits keep the frame, and a stop deletes the
    /// part. The part's loop counter is its native timer word.
    /// </summary>
    internal void AdvanceDraygonPartAnimation(RoomEnemySlot part, SamusState? samus)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon part ran without encounter state.");
        ushort oldTimer = part.InstructionTimer.RawValue;
        part.InstructionTimer = NativeCountdown.FromRaw(unchecked((ushort)(oldTimer - 1)));
        if (oldTimer != 1)
        {
            part.ExtraProperties = part.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        AnimationProgramCursor<DraygonProgram> cursor = state.Cursor(part);
        NativeCountdown counter = part.Timer;
        AnimationProgramAdvance<DraygonFrame> advance = DraygonAnimations.Programs.Advance(ref cursor, ref counter,
            (cue, continuation) => ApplyDraygonCue(state, part, samus, cue));
        part.Timer = counter;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                state.SetCursor(part, cursor);
                part.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                part.SpritemapPointer = advance.Frame.Spritemap;
                part.ExtraProperties = part.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            case AnimationProgramAdvanceKind.Wait:
                state.SetCursor(part, cursor);
                part.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                return;
            case AnimationProgramAdvanceKind.Hold:
                // The native sleep: the timer wraps from zero and the last frame stays.
                state.SetCursor(part, cursor);
                return;
            case AnimationProgramAdvanceKind.Stop:
                // The native stop deletes the part and leaves its instruction pointer unwritten.
                part.Properties = part.Properties.With(EnemyProperties.Deleted);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(AnimationProgramAdvanceKind)} {advance.Kind}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a Draygon program run by <paramref name="part"/>. Program starts write the parts'
    /// stored cursors, so the running part's own write is replaced when its update ends, as the native interpreter's
    /// final instruction-pointer store replaces it.
    /// </summary>
    /// <returns>Always null: Draygon's cues never redirect the running program.</returns>
    private DraygonProgram? ApplyDraygonCue(DraygonEnemyState state, RoomEnemySlot part, SamusState? samus, DraygonCue cue)
    {
        switch (cue)
        {
            case DraygonCue.StartPartPrograms start:
                state.SetCursor(state.Body, AnimationProgramCursor<DraygonProgram>.Start(start.Body));
                state.SetCursor(RequirePart(state.Eye), AnimationProgramCursor<DraygonProgram>.Start(start.Eye));
                state.SetCursor(RequirePart(state.Tail), AnimationProgramCursor<DraygonProgram>.Start(start.Tail));
                state.SetCursor(RequirePart(state.Arms), AnimationProgramCursor<DraygonProgram>.Start(start.Arms));
                state.Arms.InstructionTimer = NativeCountdown.Start(1);
                state.Tail.InstructionTimer = NativeCountdown.Start(1);
                state.Eye.InstructionTimer = NativeCountdown.Start(1);
                state.Body.InstructionTimer = NativeCountdown.Start(1);
                return null;
            case DraygonCue.BeginHudDraw:
                state.RoomLoadingIrqCommand = RoomLoadingInterruptCommand.BeginHudDraw;
                return null;
            case DraygonCue.SetEyeFunction eyeFunction:
                RequireEyeState(RequirePart(state.Eye)).Function = eyeFunction.Function;
                return null;
            case DraygonCue.SetOwnEyeFunction ownFunction:
                RequireEyeState(part).Function = ownFunction.Function;
                return null;
            case DraygonCue.DisplaceBodyGraphics displace:
                state.BodyGraphicsDisplacement = displace.Offset;
                return null;
            case DraygonCue.QueueSound { Sound.Library: SoundEffectLibrary.Library2 } sound:
                state.LastSoundLibrary2 = sound.Sound;
                return null;
            case DraygonCue.QueueSound { Sound.Library: SoundEffectLibrary.Library3 } sound:
                state.LastSoundLibrary3 = sound.Sound;
                return null;
            case DraygonCue.SpawnDyingObject dying:
                SpawnRandomDyingDraygonObject(state, dying.Kind);
                return null;
            case DraygonCue.ParalyseTailAndArms:
                StartDraygonProgram(RequirePart(state.Tail), DraygonProgram.Sleep);
                StartDraygonProgram(RequirePart(state.Arms), DraygonProgram.Sleep);
                return null;
            case DraygonCue.SetBodyIntangible:
                state.Body.Properties = state.Body.Properties.With(EnemyProperties.IgnoreSamusCollision);
                return null;
            case DraygonCue.SetBodyFunction bodyFunction:
                state.Function = bodyFunction.Function;
                return null;
            case DraygonCue.TailWhipHit:
                ApplyDraygonTailWhipHit(state, samus);
                return null;
            case DraygonCue.SpawnGoop goop:
                SpawnDraygonGoop(state, goop.MovingRight);
                return null;
            default:
                throw new InvalidOperationException($"Undefined Draygon cue {cue}.");
        }
    }

    /// <summary>A linked Draygon part, failing if its record never loaded.</summary>
    private static RoomEnemySlot RequirePart(RoomEnemySlot? part) =>
        part ?? throw new InvalidDataException("A Draygon program ran before all four parts loaded.");

    /// <summary>The eye's state, failing if the eye never initialized.</summary>
    private static DraygonEyeState RequireEyeState(RoomEnemySlot eye) =>
        eye.SpeciesState<DraygonEyeState>() ?? throw new InvalidDataException("Draygon's eye has no state.");

    /// <summary>Checks Draygon's frame cadence for turret and eye direction updates.</summary>
    private void ObserveDraygonTurretCadence(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        if ((frameCounterLow & 63) != 0)
            return;

        // $A5:87AA tests NMI_FrameCounter ($05B6), then advances the room RNG even when the
        // chosen turret is disabled; the spawn itself is SpawnDraygonWallTurret.
        ushort random = _room.Random.Generate();
        state.TurretCadenceChecks++;
        if (samus is not null)
            SpawnDraygonWallTurret(state, samus, random);
    }

    /// <summary>Ports the eye direction partition at <c>$A5:C48D/C513</c>.</summary>
    internal void TrackSamusWithDraygonEye(RoomEnemySlot eye, SamusState? samus, bool facingRight)
    {
        // Native emission precedes the angle-change early return, so a stationary
        // target must not suppress the periodic particle. It shares the real pool.
        if ((eye.FrameCounter & DraygonEyeEffects.CadenceMask) == 0)
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(unchecked((ushort)(eye.Position.X.Whole + (facingRight
                    ? DraygonEyeEffects.HorizontalOffset : -DraygonEyeEffects.HorizontalOffset))), unchecked((ushort)(eye.Position.Y.Whole + DraygonEyeEffects.VerticalOffset))),
                DraygonEyeEffects.DustVariant);
        if (samus is null)
            return;

        short deltaX = unchecked((short)(samus.Position.X.Whole - unchecked((ushort)(
            eye.Position.X.Whole + (facingRight ? 24 : -24)))));
        short deltaY = unchecked((short)(samus.Position.Y.Whole - unchecked((ushort)(eye.Position.Y.Whole - 32))));
        byte angle = EnemyMath.CalculateCartridgeAngle(new(deltaX, deltaY));
        DraygonEyeState eyeState = RequireEyeState(eye);
        if (angle == (byte)eyeState.LookAngle)
            return;

        eyeState.LookAngle = angle;
        DraygonProgram look = angle switch
        {
            < 32 => facingRight
                ? DraygonProgram.EyeFacingRightLookingUp
                : DraygonProgram.EyeFacingLeftLookingUp,
            < 96 => facingRight
                ? DraygonProgram.EyeFacingRightLookingRight
                : DraygonProgram.EyeFacingLeftLookingRight,
            < 160 => facingRight
                ? DraygonProgram.EyeFacingRightLookingDown
                : DraygonProgram.EyeFacingLeftLookingDown,
            < 224 => facingRight
                ? DraygonProgram.EyeFacingRightLookingLeft
                : DraygonProgram.EyeFacingLeftLookingLeft,
            _ => facingRight
                ? DraygonProgram.EyeFacingRightLookingUp
                : DraygonProgram.EyeFacingLeftLookingUp,
        };
        StartDraygonProgram(eye, look);
        eye.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Returns the encounter state owned by <paramref name="body"/>; each part property requires its linked slot when read.</summary>
    internal DraygonEnemyState RequireCompleteDraygonState(RoomEnemySlot body)
    {
        DraygonEnemyState state = Draygon ??
            throw new InvalidDataException("Draygon body has no encounter state.");
        if (!ReferenceEquals(state.Body, body))
            throw new InvalidDataException("Draygon's encounter state belongs to a different body slot.");
        return state;
    }

    /// <summary>Routes body bomb hits through Draygon's special shot reaction.</summary>
    internal void RegisterDraygonBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DraygonShot)
        {
            RunsGenericDeath = false,
            BeforeCommon = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            AfterCommon = c => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>Registers goop flight and attachment instructions with the projectile interpreter.</summary>
    internal void RegisterDraygonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunAttachedDraygonGoop(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus);

        table.RegisterPreInstruction(context =>
        {
            // $86:8DFF deletes a power-bombed shot, then still moves the released slot.
            DeleteEnemyProjectileIfPowerBombed(context.Projectile, context.Samus);
            MoveDraygonProjectile(context.Projectile);
            DeleteDraygonProjectileOutsideRoom(context.Projectile);
        }, EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired);

        table.RegisterPreInstruction(context =>
        {
            RunFlyingDraygonGoop(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.DraygonGoop);

        // A shot wall turret runs the shared misc-dust small explosion, the list its definition names.
        table.RegisterShotAnimation(RoomEnemyProjectileKind.DraygonWallTurret,
            () => new MiscDustProjectileAnimation(MiscDustProjectileProgram.SmallExplosion));
        table.RegisterTouchAnimation(RoomEnemyProjectileKind.DraygonGoop,
            () => new DraygonProjectileAnimation(DraygonProjectileProgram.GoopTouch));
        table.RegisterShotAnimation(RoomEnemyProjectileKind.DraygonGoop,
            () => new DraygonProjectileAnimation(DraygonProjectileProgram.GoopShot));
        table.RegisterCue<DraygonProjectileCue>((projectile, samus, cue) =>
        {
            switch (cue)
            {
                case DraygonProjectileCue.StickToSamus:
                    AttachDraygonGoopToSamus(projectile, samus);
                    return;
                case DraygonProjectileCue.SpawnDrops:
                    // The drop is allocated before the deletion releases this projectile, preserving slot order.
                    _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(projectile.Position.Whole, EnemyDefinitionId.DraygonEye);
                    return;
                case DraygonProjectileCue.Delete:
                    projectile.Clear();
                    return;
                case DraygonProjectileCue.Fire:
                    projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.DraygonsWallTurretProjectile_Fired;
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DraygonProjectileCue)} {cue}.");
            }
        });
    }

    /// <summary>Ports DraygonBody's initialization AI.</summary>
    internal void RunDraygonBodyInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeDraygonBody(slot);
    }

    /// <summary>Ports DraygonEye's initialization AI.</summary>
    internal void RunDraygonEyeInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeDraygonPart(slot);
    }

    /// <summary>Ports DraygonBody's main AI.</summary>
    internal void RunDraygonBodyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        // Bank $A5 times turrets and smoke from NMI_FrameCounter ($05B6), never
        // the separate 8-bit counter at $05B5; only its low bits are tested.
        RunDraygonBodyMain(slot, frame.Samus, unchecked((byte)_frameContext._enemyFrameNmiFrameCounter));
    }

    /// <summary>Ports DraygonEye's main AI.</summary>
    internal void RunDraygonEyeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDraygonPartMain(slot, frame.Samus);
    }

    /// <summary>Ports DraygonTail's main AI.</summary>
    internal void RunDraygonTailMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunDraygonPartMain(slot, frame.Samus);
    }

    /// <summary>Routes power-bomb damage for Draygon's body through its encounter reaction.</summary>
    internal void RegisterDraygonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.DraygonPowerBomb)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, samus) => ResolveDraygonReaction(enemy, samus),
        });
    }

    /// <summary>Registers the body and linked-part contact behavior against Samus.</summary>
    internal void RegisterDraygonTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.DraygonTouch,
            context =>
            {
                if (!context.UsesExtendedHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended touch hitboxes.");
                if (context.HitboxTouchAi == EnemyInteractionCallback.NoOp)
                    return; // The vulnerable eye rectangle is harmless to Samus.
                if (context.HitboxTouchAi != EnemyInteractionCallback.DraygonTouch)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox touch AI $A5:{context.HitboxTouchAiPointer:X4} is not translated.");
                }
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
                ResolveDraygonReaction(context.Slot, context.Samus);
            });
    }

    /// <summary>
    /// Ports hurt AI <c>$A5:954D</c>. The body is rendered partly through BG2 palette five
    /// and partly through sprite palette seven, so a valid flash must update both surfaces.
    /// </summary>
    internal void ApplyDraygonHurt(
        RoomEnemySlot body,
        DraygonEnemyState state,
        SamusState? samus)
    {
        bool whiteFrame = (body.FlashTimer.RawValue & 2) != 0;
        (_room.Artwork?.DraygonColors ?? throw new InvalidOperationException(
            "Draygon damage requires installed color artwork."))
            .ApplyHurt(_presentation.Cgram, whiteFrame, RequireHealthPaletteBand(state));

        // Grapple-connected flag bit zero is the retail electrocution channel. It is
        // sampled only while hurt AI owns the body and subtracts exactly 256 HP on every
        // eighth body frame before running the same fatal/nonfatal reaction as a shot.
        if (samus is null || !IsDraygonGrappleConnected(samus) || (body.FrameCounter & 7) != 0)
            return;

        body.Health = body.Health <= 256
            ? 0
            : body.Health - 256;
        ResolveDraygonReaction(body, samus);
    }

    /// <summary>
    /// The vulnerable eye callback increases future swoop curvature before common shot AI.
    /// A signed comparison prevents the stored value from ever advancing beyond <c>$98</c>.
    /// </summary>
    internal static void AdvanceDraygonShotAcceleration(DraygonEnemyState state)
    {
        EightEightVector<RoomPixels> candidate = state.SwoopAcceleration + DraygonShotSwoopAccelerationStep;
        if (unchecked((short)(candidate.Y.RawValue - DraygonSwoopAccelerationLimit)) < 0)
            state.SwoopAcceleration = candidate;
    }

    /// <summary>Ports the common post-touch/shot/power-bomb reaction at <c>$A5:960D</c>.</summary>
    internal void ResolveDraygonReaction(RoomEnemySlot body, SamusState? samus)
    {
        DraygonEnemyState state = RequireCompleteDraygonState(body);
        if (body.Health != 0)
        {
            UpdateDraygonHealthPalette(state);
            return;
        }

        BeginDraygonDeath(state, samus);
    }

    /// <remarks>
    /// `DraygonReaction_Common`'s dead branch ($A5:9618) has no "already dying" test. Every
    /// touch, shot or power-bomb reaction on a zero-health body runs it again: the dying
    /// arm and eye lists restart, the drift function is reinstalled and Samus is released.
    /// </remarks>
    private void BeginDraygonDeath(DraygonEnemyState state, SamusState? samus)
    {
        RoomEnemySlot body = state.Body;
        samus?.Grapple.Phase = GrapplePhase.Dropped;
        StartDraygonProgram(
            body,
            state.FacingRight
                ? DraygonProgram.ArmsFacingRightDying
                : DraygonProgram.ArmsFacingLeftDying);
        StartDraygonProgram(
            state.Eye,
            state.FacingRight
                ? DraygonProgram.EyeFacingRightDying
                : DraygonProgram.EyeFacingLeftDying);
        RequireEyeState(RequirePart(state.Eye)).Function = DraygonEyeFunction.Inert;
        state.Function = DraygonAiFunction.Dying;

        // The native death callback releases Samus unconditionally, even during an
        // unrelated movement handler. In particular, this can interrupt a shinespark.
        // The caller then clears the grapple word, including the owner-release signal.
        if (samus is not null)
        {
            ReleaseSamusDuringEnemyMain(samus);
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
    }

    /// <summary>Changes Draygon's health color band only when the health threshold changes.</summary>
    internal void UpdateDraygonHealthPalette(DraygonEnemyState state)
    {
        int band = DraygonHealthPaletteDefinitions.BandForHealth(state.Body.Health);

        if (band == state.HealthPaletteBand)
            return;
        state.HealthPaletteBand = band;
        CopyDraygonHealthColors(state);
    }

    /// <summary>The installed health band; native code reads $7E:781C only after initialization installs one.</summary>
    private static int RequireHealthPaletteBand(DraygonEnemyState state) => state.HealthPaletteBand
        ?? throw new InvalidOperationException("Draygon has no installed health palette band.");

    /// <summary>Copies the selected installed health palette band into live CGRAM.</summary>
    internal void CopyDraygonHealthColors(DraygonEnemyState state)
    {
        (_room.Artwork?.DraygonColors ?? throw new InvalidDataException(
            "Draygon health colors require installed artwork."))
            .ApplyHealthBand(_presentation.Cgram, RequireHealthPaletteBand(state));
    }
    /// <summary>Arena X coordinate to which Draygon drifts before burial.</summary>
    private const ushort DraygonDeathTargetX = 256;
    /// <summary>Arena Y coordinate at which the death drift hands off to burial.</summary>
    private const ushort DraygonDeathTargetY = 480;
    /// <summary>Y threshold below the room where Draygon's burial is complete.</summary>
    private const ushort DraygonBurialFinishedY = 576;
    /// <summary>Native wait before the burial Evirs and sinking phase proceed.</summary>
    private const ushort DraygonDeathWaitFramesUs = 416;

    /// <summary>Moves the dying body toward the burial point and changes its linked eye pose.</summary>
    private void DriftDyingDraygonToBurialPoint(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        if ((body.FrameCounter & 15) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.Position.X.Whole + (state.FacingRight ? 32 : -32)));
            _spriteObjects.SpawnRoomSpriteObject(
                new(foamX, unchecked((ushort)(body.Position.Y.Whole - 16))),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        // Retail quarters both coordinates before calculating this angle. That is not an
        // algebraic simplification: logical shifts discard low coordinate bits and thereby
        // select slightly different table vectors near the destination.
        short deltaX = unchecked((short)(64 - (body.Position.X.Whole >> 2)));
        short deltaY = unchecked((short)(120 - (body.Position.Y.Whole >> 2)));
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(new(deltaX, deltaY));
        byte movementAngle = unchecked((byte)(SnesAngle.QuarterTurn.TableIndex - cartridgeAngle));
        MoveDraygonAtMovementAngle(body, movementAngle, speed: 1);

        if (!EnemyMath.IsWithinStrictModularDistance(body.Position.X.Whole, DraygonDeathTargetX, 4) ||
            !EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonDeathTargetY, 4))
        {
            return;
        }

        SpawnDraygonDeathEvirs(state);
        state.Function = DraygonAiFunction.DyingSink;
        state.MusicRequest = MusicCommand.SelectTrack(3);
        state.FunctionTimer = NativeCountdown.Start(DraygonDeathWaitFramesUs);
        StartDraygonProgram(body, DraygonProgram.Sleep);

        EnemyPropertyWord deletedPartProperties = body.Properties.With(EnemyProperties.Deleted);
        state.Tail.Properties = deletedPartProperties;
        state.Arms.Properties = deletedPartProperties;
        RoomEnemySlot eye = state.Eye;
        StartDraygonProgram(
            eye,
            state.FacingRight
                ? DraygonProgram.EyeFacingRightDead
                : DraygonProgram.EyeFacingLeftDead);
        RequireEyeState(eye).Function = DraygonEyeFunction.Inert;
    }

    /// <summary>Advances burial sprite motion while the pre-sink countdown expires.</summary>
    private void WaitForDraygonBurialEvirs(
        DraygonEnemyState state,
        byte frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsZero)
        {
            state.Function = DraygonAiFunction.DyingFinish;
            return;
        }

        MoveDraygonDeathEvirs();
    }

    /// <summary>Sinks the body past the room floor, then persists boss defeat and requests its drop.</summary>
    private void SinkDraygonBelowTheRoom(
        DraygonEnemyState state,
        byte frameCounterLow)
    {
        SpawnDyingDraygonSmoke(state, frameCounterLow);
        MoveDraygonDeathEvirs();
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole + 1)));
        if (unchecked((short)(body.Position.Y.Whole - DraygonBurialFinishedY)) < 0)
            return;

        StartDraygonProgram(body, DraygonProgram.Delete);

        // $A0:B917 uses Draygon's body header for sixteen independent drops over the
        // lower half of the arena. Do this before deleting the body so the native event is
        // represented by real pickup actors rather than only a debugger-visible flag.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.DraygonBody,
            count: 16,
            xBase: 128,
            xMask: byte.MaxValue,
            yBase: 352,
            yMask: SnesLittleEndian.Word(0, 63));
        EnemyPropertyWord deletedProperties = body.Properties.With(EnemyProperties.Deleted);
        body.Properties = deletedProperties;
        state.Eye.Properties = deletedProperties;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        state.ItemDropRequested = true;
        ClearRoomSpriteObjectPool();
    }

    /// <summary>Consumes room RNG and spawns smoke at the dying body every eighth frame.</summary>
    private void SpawnDyingDraygonSmoke(DraygonEnemyState state, byte frameCounterLow)
    {
        if ((frameCounterLow & 7) != 0)
            return;

        ushort random = _room.Random.Generate();
        ushort x = unchecked((ushort)((random & 127) + 192));
        ushort y = unchecked((ushort)((SnesLittleEndian.HighByte(random) & 63) + 400));
        if (_spriteObjects.SpawnRoomSpriteObject(new(x, y), RoomSpriteObjectKind.DustCloud, graphicsIndex: 0) is not null)
            state.DeathSmokeObjectsSpawned++;
    }

    /// <summary>Places a death-animation sprite at a room-RNG offset from the body.</summary>
    private void SpawnRandomDyingDraygonObject(
        DraygonEnemyState state,
        RoomSpriteObjectKind kind)
    {
        ushort random = _room.Random.Generate();
        short xOffset = unchecked((short)((random & 127) - 64));
        short yOffset = unchecked((short)((SnesLittleEndian.HighByte(random) & 127) - 64));
        if (_spriteObjects.SpawnRoomSpriteObject(
                new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(state.Body.Position.Y.Whole + yOffset))),
                kind,
                graphicsIndex: 0) is not null)
        {
            state.DeathAnimationObjectsSpawned++;
        }
    }

    /// <summary>Clears the room sprite pool and allocates six burial Evirs in native slot order.</summary>
    private void SpawnDraygonDeathEvirs(DraygonEnemyState state)
    {
        ClearRoomSpriteObjectPool();

        // The two native loops share one descending position-table index. Allocation also
        // descends, so these calls deterministically occupy slots 31..26 in movement-table
        // order: three left-facing objects followed by three right-facing objects.
        for (int entry = 5; entry >= 3; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonIntroEvir);
        for (int entry = 2; entry >= 0; entry--)
            SpawnDraygonDeathEvir(entry, RoomSpriteObjectKind.DraygonDeathEvirFacingRight);

        state.DeathEvirsSpawned = true;
    }

    /// <summary>Allocates one burial Evir from its definition-table position and kind.</summary>
    internal void SpawnDraygonDeathEvir(int entry, RoomSpriteObjectKind kind)
    {
        DraygonBurialEvirDefinition definition =
            DraygonBurialEvirDefinitions.ForEntry(entry);
        RoomSpriteObjectSlot? sprite = _spriteObjects.SpawnRoomSpriteObject(
            definition.Initial,
            kind,
            graphicsIndex: EnemyPaletteBits.Palette7);
        if (sprite is null)
        {
            throw new InvalidOperationException(
                "Draygon cleared the sprite-object pool but could not allocate a burial Evir.");
        }
    }

    /// <summary>Advances the six burial Evir sprite slots by their table-defined velocities.</summary>
    internal void MoveDraygonDeathEvirs()
    {
        for (int entry = 5, slotIndex = RoomSpriteObjectPool.RoomSpriteObjectSlotCount - 1;
             entry >= 0;
             entry--, slotIndex--)
        {
            RoomSpriteObjectSlot sprite = _spriteObjects.Slots[slotIndex];
            if (!sprite.IsActive || sprite.Kind is not (
                    RoomSpriteObjectKind.DraygonIntroEvir or
                    RoomSpriteObjectKind.DraygonDeathEvirFacingRight))
            {
                throw new InvalidDataException(
                    $"Draygon burial movement expected Evir in sprite slot {slotIndex}.");
            }

            DraygonBurialEvirDefinition definition =
                DraygonBurialEvirDefinitions.ForEntry(entry);
            sprite.Position = sprite.Position.Advance(definition.Velocity);
        }
    }

    /// <summary>Moves the body along a native sine-table angle at the supplied speed.</summary>
    private static void MoveDraygonAtMovementAngle(RoomEnemySlot body, byte movementAngle, ushort speed)
    {
        uint xMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: SnesAngle.QuarterTurn.TableIndex));
        uint yMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: SnesAngle.HalfTurn.TableIndex));
        body.Position = body.Position.WithX(AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: EnemyMath.CosineIsNegative(movementAngle)));
        body.Position = body.Position.WithY(AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: EnemyMath.SineIsNegative(movementAngle)));
    }

    /// <summary>Releases room sprite slots before allocating the ordered burial Evir set.</summary>
    private void ClearRoomSpriteObjectPool()
    {
        foreach (RoomSpriteObjectSlot sprite in _spriteObjects.Slots)
            sprite.Clear();
    }
    /// <summary>Whole-pixel speed of Draygon's direct chase before the carry spiral.</summary>
    private const ushort DraygonChaseSpeed = 2;
    /// <summary>The carry spiral's target and initial center, ($100, $180).</summary>
    private static readonly Position16<RoomPixels> DraygonSpiralTarget = new(256, 384);
    /// <summary>Initial angular position used when the Samus-carry spiral begins: three quarters of a turn.</summary>
    private static readonly SnesAngle DraygonInitialSpiralAngle = SnesAngle.ThreeQuarterTurn;
    /// <summary>Initial per-frame angular step of the Samus-carry spiral: eight table units, slowed by 1/256 unit each frame.</summary>
    private static readonly SnesAngle DraygonInitialSpiralAngleDelta = SnesAngle.FromTableIndex(8);
    /// <summary>The carry spiral's per-frame radius growth: one eighth of a pixel.</summary>
    private static readonly SnesSignedSixteenSixteen DraygonSpiralRadiusGrowth = SnesSignedSixteenSixteen.FromParts(0, 8192);
    /// <summary>Largest radius reached by the expanding carry spiral.</summary>
    private const ushort DraygonMaximumSpiralRadius = 160;
    /// <summary>Upper room boundary for the spiral center's vertical lift.</summary>
    private const ushort DraygonMinimumSpiralCenterY = 64;
    /// <summary>The spiral center's per-frame 16.16 lift, -$0000.4000.</summary>
    private static readonly SnesSignedSixteenSixteen DraygonSpiralCenterLift = SnesSignedSixteenSixteen.FromRaw(-16384);
    /// <summary>Number of AI calls in Draygon's tail-whip phase.</summary>
    private const ushort DraygonTailWhipDuration = 64;
    /// <summary>Countdown value that triggers the first tail-whip animation step.</summary>
    private const ushort DraygonTailWhipStartFrame = 63;

    /// <summary>
    /// Ports <c>Function_DraygonBody_ChaseSamus</c> at <c>$A5:8E19</c>. The speed-divisor
    /// test is the real communication channel from bank-$86 goop; no host-only encounter
    /// flag or proximity shortcut is used to start this branch.
    /// </summary>
    internal void ChaseAndTryToGrabSamus(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        if (samus is null)
            throw new InvalidOperationException("Draygon chase AI requires the active Samus actor.");

        // Attached goop actors own this word. Once the final actor disappears, retail
        // abandons the chase immediately and exits through the same upward flight used
        // after a completed grab.
        if (samus.XSpeedDivisor == 0)
        {
            state.Function = DraygonAiFunction.FlyStraightUp;
            return;
        }

        // Despite older labels calling $0400 “tangible”, this bit tells the common enemy
        // collision pass to ignore Samus. Draygon's claw-window test below replaces the
        // ordinary body collision while he is deliberately chasing the gooped player.
        RoomEnemySlot body = state.Body;
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);

        short clawOffset = state.FacingRight ? (short)8 : (short)-8;
        ushort clawX = unchecked((ushort)(body.Position.X.Whole + clawOffset));
        bool insideGrabWindow =
            EnemyMath.IsWithinStrictModularDistance(clawX, samus.Position.X.Whole, 8) &&
            EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, samus.Position.Y.Whole, 8);
        if (!insideGrabWindow)
        {
            MoveDraygonToward(body, samus.Position.Whole, DraygonChaseSpeed);
            return;
        }

        StartDraygonProgram(
            state.Arms,
            state.FacingRight
                ? DraygonProgram.ArmsFacingRightGrab
                : DraygonProgram.ArmsFacingLeftGrab);

        // Samus command $0D returns nonzero whenever the grapple-beam function is anything
        // other than its inactive entry. In that case Draygon drops the beam and retreats;
        // it must not enter a grabbed pose and then undo it with a bespoke correction.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            state.Function = DraygonAiFunction.GrabbedSamus;
            return;
        }

        samus.DraygonGrabbed.Begin(samus, state.FacingRight);
        _frameResults.DraygonDiscardedSamusPoseRequests = true;
        // `$A5:8EDB-$8EE5` set the whole spiral center only; the Y fraction (`$7E:7814`) carries over from
        // the previous spiral of this fight, so the first lift after a re-grab borrows on the native frame.
        state.SpiralCenter = state.SpiralCenter.WithWhole(DraygonSpiralTarget);
        state.SpiralXRadius = SnesSignedSixteenSixteen.Zero;
        state.SpiralAngle = DraygonInitialSpiralAngle;
        state.SpiralAngleDelta = DraygonInitialSpiralAngleDelta;
        state.Function = DraygonAiFunction.CarrySamus;
        state.SuccessfulGrabs++;
    }

    /// <summary>Ports the one-frame grapple rejection at <c>$A5:8F10</c>.</summary>
    private static void RepelDraygonWithGrapple(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon grapple rejection requires Samus state.");

        samus.Grapple.Phase = GrapplePhase.Dropped;
        state.Function = DraygonAiFunction.ReleaseSamus;
    }

    /// <summary>
    /// Ports <c>$A5:8F1E</c>: fly the grabbed pair toward the authored spiral center, then
    /// install the facing-specific roar before the expanding orbit begins.
    /// </summary>
    private void CarrySamusToDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        RoomEnemySlot body = state.Body;
        bool reachedTarget =
            EnemyMath.IsWithinStrictModularDistance(body.Position.X.Whole, DraygonSpiralTarget.X, 2) &&
            EnemyMath.IsWithinStrictModularDistance(body.Position.Y.Whole, DraygonSpiralTarget.Y, 2);
        if (reachedTarget)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            StartDraygonProgram(
                body,
                state.FacingRight
                    ? DraygonProgram.BodyFacingRightRoar
                    : DraygonProgram.BodyFacingLeftRoar);
            body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
            return;
        }

        MoveDraygonToward(body, DraygonSpiralTarget, DraygonChaseSpeed);
        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>
    /// Ports the expanding, rising spiral at <c>$A5:8FD6</c>. Radius growth, angular
    /// deceleration, and center lift stay as separate native fixed-point word pairs.
    /// </summary>
    internal void CarrySamusInDraygonSpiral(DraygonEnemyState state, SamusState? samus)
    {
        SamusState activeSamus = RequireDraygonGrabbedSamus(samus);
        if (IsDraygonGrappleConnected(activeSamus))
        {
            MarkDraygonGrappleShock(state);
            return;
        }

        // The current RNG word is sampled without advancing it. Turret cadence and other
        // actors are the producers; a low byte of zero inserts one 64-frame tail whip.
        if ((SnesLittleEndian.LowByte(_room.Random.Read())) == 0)
        {
            state.TailWhipTimer = NativeCountdown.Start(DraygonTailWhipDuration);
            state.Function = DraygonAiFunction.TailWhipWithSamus;
            return;
        }

        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWhole(state.SpiralCenter.Whole + new Offset16<RoomPixels>(
            unchecked((short)EnemyMath.ReadEightBitCosineProduct(state.SpiralAngle.TableIndex, state.SpiralXRadius.WholeWord)),
            unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
                state.SpiralAngle.TableIndex,
                unchecked((ushort)(state.SpiralXRadius.WholeWord >> 2))))));

        if ((body.FrameCounter & 7) == 0)
        {
            ushort foamX = unchecked((ushort)(
                body.Position.X.Whole + (state.FacingRight ? 32 : -32)));
            _spriteObjects.SpawnRoomSpriteObject(
                new(foamX, unchecked((ushort)(body.Position.Y.Whole - 16))),
                RoomSpriteObjectKind.DraygonSpiralFoam,
                graphicsIndex: 0);
        }

        state.SpiralXRadius += DraygonSpiralRadiusGrowth;
        if (unchecked((short)(state.SpiralXRadius.WholeWord - DraygonMaximumSpiralRadius)) >= 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        state.SpiralAngleDelta = state.SpiralAngleDelta.AddRaw(-1);
        state.SpiralAngle = state.SpiralAngle.AddTableUnits(state.SpiralAngleDelta.TableIndex);

        state.SpiralCenter = state.SpiralCenter.AdvanceY(DraygonSpiralCenterLift);
        if (unchecked((short)(state.SpiralCenter.Y.Whole - DraygonMinimumSpiralCenterY)) < 0)
        {
            state.Function = DraygonAiFunction.FinalTailWhips;
            return;
        }

        MoveSamusWithDraygon(state, activeSamus);
    }

    /// <summary>Ports the incidental 64-frame tail-whip pause at <c>$A5:90D4</c>.</summary>
    private void RunDraygonTailWhip(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        state.TailWhipTimer = state.TailWhipTimer.Decrement().Remaining;
        if (state.TailWhipTimer.IsZero)
        {
            state.Function = DraygonAiFunction.FlailWithSamus;
            return;
        }

        if (state.TailWhipTimer.RawValue == DraygonTailWhipStartFrame)
        {
            StartDraygonProgram(
                state.Tail,
                state.FacingRight
                    ? DraygonProgram.TailFacingRightWhip
                    : DraygonProgram.TailFacingLeftWhip);
        }
    }

    /// <summary>Installs the four-repeat finishing whip list at <c>$A5:9105</c>.</summary>
    private void BeginDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus)
    {
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));
        StartDraygonProgram(
            state.Tail,
            state.FacingRight
                ? DraygonProgram.TailFacingRightFinalWhips
                : DraygonProgram.TailFacingLeftFinalWhips);
        state.Function = DraygonAiFunction.FinalTailWhipsWait;
    }

    /// <summary>
    /// Ports <c>$A5:9124</c>. The tail's cartridge list owns the duration and eventually
    /// executes opcode <c>$9F57</c>, which changes the body function to release Samus.
    /// </summary>
    private void WaitForDraygonFinalTailWhips(DraygonEnemyState state, SamusState? samus) =>
        MoveSamusWithDraygon(state, RequireDraygonGrabbedSamus(samus));

    /// <summary>Ports release, collision-bit cleanup, and tail flail at <c>$A5:9128</c>.</summary>
    private void ReleaseSamusFromDraygon(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon release requires the active Samus actor.");

        // Grapple rejection reaches this function without ever installing a grabbed pose.
        // The normal route does own one, so invoke the bank-$90 release only in that case.
        if (samus.DraygonGrabbed.IsActive)
        {
            ReleaseSamusDuringEnemyMain(samus);
            // ReleaseSamusFromDraygon publishes bit one, and $A5:9128 immediately clears
            // the complete native grapple-flags word. Consume the host projection here so
            // a later encounter cannot observe a stale owner-release request.
            samus.DraygonGrabbed.ConsumeOwnerReleaseSignal();
        }

        state.Function = DraygonAiFunction.FlyStraightUp;
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        StartDraygonProgram(
            state.Tail,
            state.FacingRight
                ? DraygonProgram.TailFacingRightFlail
                : DraygonProgram.TailFacingLeftFlail);
    }

    /// <summary>Ports the regional four-pixel upward retreat at <c>$A5:9154</c>.</summary>
    private void FlyDraygonStraightUp(
        DraygonEnemyState state,
        SamusState? samus,
        byte frameCounterLow)
    {
        ObserveDraygonTurretCadence(state, samus, frameCounterLow);
        RoomEnemySlot body = state.Body;
        body.Position = body.Position.WithWholeY(unchecked((ushort)(body.Position.Y.Whole - 4)));
        if (unchecked((short)body.Position.Y.Whole) >= 0)
            return;

        body.Properties = body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Function = DraygonAiFunction.SwoopRightSetup;
        state.SwoopOffset = 0;
        body.Position = body.Position.WithWhole(state.LeftSideReset);
    }

    /// <summary>
    /// <c>MoveSamusWithDraygon</c> ($A5:94A9). It places Samus without checking that she is
    /// still held, and only then tests the escape bit bank $90 publishes. The first carry
    /// call after an escape therefore drags the released Samus once more before Draygon
    /// switches to flying straight up.
    /// </summary>
    internal void MoveSamusWithDraygon(DraygonEnemyState state, SamusState samus)
    {
        samus.DraygonGrabbed.ApplyOwnerPosition(
            samus,
            state.Body.Position.Whole,
            state.FacingRight);
        _scrollCap.CapSamusScrollingSpeed(samus);
        if (samus.DraygonGrabbed.ConsumeOwnerReleaseSignal())
            state.Function = DraygonAiFunction.FlyStraightUp;
    }

    /// <summary>
    /// Implements the shared <c>ConvertAngleToXy</c> plus
    /// <c>MoveEnemyAccordingToAngleAndXYSpeeds</c> pair used by chase and carry.
    /// </summary>
    private static void MoveDraygonToward(
        RoomEnemySlot body,
        Position16<RoomPixels> target,
        ushort speed)
    {
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(target - body.Position.Whole);
        byte movementAngle = unchecked((byte)(SnesAngle.QuarterTurn.TableIndex - cartridgeAngle));

        uint xMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: SnesAngle.QuarterTurn.TableIndex));
        uint yMagnitude = unchecked((uint)EnemyMath.ReadUnsignedSineMagnitudeProduct(
            movementAngle,
            speed,
            angleOffset: SnesAngle.HalfTurn.TableIndex));

        body.Position = body.Position.WithX(AddDraygonAngleMagnitude(
            body.Position.X,
            xMagnitude,
            subtract: EnemyMath.CosineIsNegative(movementAngle)));
        body.Position = body.Position.WithY(AddDraygonAngleMagnitude(
            body.Position.Y,
            yMagnitude,
            subtract: EnemyMath.SineIsNegative(movementAngle)));
    }

    /// <summary>Adds or subtracts a sine-derived magnitude with native 16.16 wraparound.</summary>
    private static SnesFixedPosition AddDraygonAngleMagnitude(
        SnesFixedPosition position,
        uint magnitude,
        bool subtract)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = subtract
            ? unchecked(fixedPosition - magnitude)
            : unchecked(fixedPosition + magnitude);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }

    /// <summary>Recognizes grapple phases that let Samus electrocute a connected Draygon.</summary>
    private static bool IsDraygonGrappleConnected(SamusState samus) =>
        samus.Grapple.Phase is GrapplePhase.ConnectedSwinging or
            GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab;

    /// <summary>Sets the hurt-flash timer and handler bit after a grapple shock.</summary>
    private static void MarkDraygonGrappleShock(DraygonEnemyState state)
    {
        RoomEnemySlot body = state.Body;
        body.FlashTimer = NativeCountdown.Start(body.HurtAiTime + 8);
        body.AiStates = body.AiStates | EnemyAiStates.Hurt;
    }

    /// <summary>
    /// Runs <c>ReleaseSamusFromDraygon</c> ($90:E2DE) from Draygon's AI and publishes it so the
    /// runtime clears the prospective poses alpha already chose, as the grab does.
    /// </summary>
    private void ReleaseSamusDuringEnemyMain(SamusState samus)
    {
        samus.DraygonGrabbed.Release(samus);
        _frameResults.DraygonDiscardedSamusPoseRequests = true;
    }

    /// <summary>Requires the active Samus actor for Draygon's carry and release phases.</summary>
    private static SamusState RequireDraygonGrabbedSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon carry AI requires the active Samus actor.");
        return samus;
    }

    /// <summary>Ports tail instruction <c>$A5:9B9A</c>, including suit damage.</summary>
    private void ApplyDraygonTailWhipHit(DraygonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Draygon's tail-hit instruction requires Samus state.");

        state.SwoopAcceleration = DraygonInitialSwoopAcceleration;
        ushort rawDamage = state.Body.Definition.Damage;
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(rawDamage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(rawDamage >> 1))
                : rawDamage;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;

        // `$A5:9B9A` also publishes the same 32-frame, type-seven quake used by the
        // cartridge's grabbed-tail impact. Keeping it on the shared room-enemy surface lets
        // the ordinary camera renderer observe the hit instead of reducing it to HP loss.
        _earthquake.EarthquakeTimer = NativeCountdown.Start(32);
        _earthquake.EarthquakeType = 7;

        state.TailWhipHits++;
        _spriteObjects.SpawnRoomSpriteObject(
            new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + 16))),
            RoomSpriteObjectKind.DustCloud,
            graphicsIndex: 0);
    }
    /// <summary>Ports <c>HandleFiringWallTurret</c> and initializer <c>$86:8D40</c>.</summary>
    internal void SpawnDraygonWallTurret(
        DraygonEnemyState state,
        SamusState samus,
        ushort random)
    {
        DraygonCannonTarget target = DraygonCannonData.FiringTarget(
            ClosedNativeWords.Decode<DraygonFiringCannon>((byte)(random & 3), "Draygon firing cannon"));
        if (state.DisabledCannonWords.Contains(target.DisabledWord))
            return;

        RoomEnemyProjectileSlot? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.DraygonWallTurret,
            graphicsIndex: EnemyPaletteBits.Palette5);
        turret.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.WallTurretBloom));
        turret.Position = FixedPosition<RoomPixels>.FromWhole(target.Origin);

        // PlaceAndAim negates the common zero-up angle, then adds $40 before retaining the
        // low byte. ConvertAngleToXy stores unsigned magnitudes; the pre-instruction applies
        // signs from this retained angle on every frame.
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - turret.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - turret.Position.Y.Whole))));
        byte flightAngle = unchecked((byte)(-cartridgeAngle + SnesAngle.QuarterTurn.TableIndex));
        SetDraygonProjectileVelocity(turret, flightAngle, DraygonProjectileSpeeds.WallTurret);

        // The definition says $8DFF, but init replaces it with RTS until the 84-frame muzzle
        // bloom reaches instruction $8CF6 and explicitly enables flight.
        turret.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_868D54;
        state.WallTurretsSpawned++;
    }

    /// <summary>
    /// Implements bank $84's write through <c>PLM_Variable</c>. Cannon setup names one of
    /// Draygon's cannon-control words, then the damage instruction disables it here.
    /// </summary>
    public void DisableDraygonCannon(DraygonCannonControlWord word)
    {
        // The defeated room still has cannon PLMs even though its enemy population no
        // longer includes Draygon; only a live Draygon reads the disabled words.
        _room.EnsureLoaded();
        Draygon?.DisabledCannonWords.Add(word);
    }

    /// <summary>Ports the two body instruction producers at <c>$A5:9F7C/9FAE</c>.</summary>
    internal void SpawnDraygonGoop(DraygonEnemyState state, bool movingRight)
    {
        RoomEnemyProjectileSlot? goop = _projectiles.AllocateEnemyProjectile();
        if (goop is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            goop,
            RoomEnemyProjectileKind.DraygonGoop,
            graphicsIndex: EnemyPaletteBits.Palette2);
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.Goop));
        goop.Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(state.Body.Position.X.Whole + (movingRight ? 24 : -28))), unchecked((ushort)(state.Body.Position.Y.Whole - 16))));

        ushort random = _room.Random.Generate();
        byte angle = unchecked((byte)((random & 63) + (movingRight ? 192 : 128)));
        SetDraygonProjectileVelocity(goop, angle, DraygonProjectileSpeeds.Goop);
        state.GoopProjectilesSpawned++;
    }

    /// <summary>Splits sine-derived projectile velocity into native 8.8 and fraction words.</summary>
    private static void SetDraygonProjectileVelocity(
        RoomEnemyProjectileSlot projectile,
        byte angle,
        ushort speed)
    {
        projectile.DirectionParameter = angle;
        int xMagnitude = EnemyMath.ReadAbsoluteCosineProduct(angle, speed);
        int yMagnitude = EnemyMath.ReadAbsoluteSineProduct(angle, speed);
        projectile.Velocity = projectile.Velocity.WithX(new SnesSignedEightEight(SnesFixedPosition.WholeOf(xMagnitude)));
        projectile.Variable0 = unchecked((ushort)xMagnitude);
        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(SnesFixedPosition.WholeOf(yMagnitude)));
        projectile.Variable1 = unchecked((ushort)yMagnitude);
    }

    /// <summary>Moves a Draygon projectile in 16.16 space by its angle and speed.</summary>
    internal static void MoveDraygonProjectile(RoomEnemyProjectileSlot projectile)
    {
        int xMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.X.RawValue, projectile.Variable0)));
        int yMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(projectile.Velocity.Y.RawValue, projectile.Variable1)));
        int xDisplacement = EnemyMath.CosineIsNegative(projectile.DirectionParameter) ? -xMagnitude : xMagnitude;
        int yDisplacement = EnemyMath.SineIsNegative(projectile.DirectionParameter) ? -yMagnitude : yMagnitude;
        projectile.Position = projectile.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        projectile.Position = projectile.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(yDisplacement));
    }

    /// <summary>Deletes a projectile that has left Draygon's room (<c>$86:E722</c>).</summary>
    private static void DeleteDraygonProjectileOutsideRoom(RoomEnemyProjectileSlot projectile)
    {
        if (IsOutsideDraygonRoom(projectile))
            projectile.Clear();
    }

    /// <summary>Advances airborne goop, including power-bomb deletion and Samus contact setup.</summary>
    internal void RunFlyingDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8E0F: a power-bombed goop loses its ID but the routine still moves it and
        // tests Samus. Contact is tested before the room boundary, so a goop that touches
        // Samus outside the boundary still runs its touch list instead of being deleted.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        MoveDraygonProjectile(goop);
        if (samus is null)
        {
            DeleteDraygonProjectileOutsideRoom(goop);
            return;
        }

        ushort xDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - goop.Position.X.Whole)));
        ushort yDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.Y.Whole - goop.Position.Y.Whole)));
        if (xDistance < 16 && yDistance < 20)
        {
            goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.GoopTouch));
            return;
        }
        DeleteDraygonProjectileOutsideRoom(goop);
    }

    /// <summary>Attaches goop to Samus, increasing her X-speed divisor until it expires.</summary>
    internal void AttachDraygonGoopToSamus(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8D99 runs the power-bomb deletion first, then attaches regardless.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        ushort nextDivisor = unchecked((ushort)(samus.XSpeedDivisor + 1));
        if (unchecked((short)(nextDivisor - 6)) >= 0)
            return;

        samus.XSpeedDivisor = nextDivisor;
        goop.Variable1 = nextDivisor;
        goop.Variable0 = 256;
        goop.PreInstruction =
            (ushort)EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus;
        goop.BlocksSamusProjectiles = false;
        goop.CanDamageSamus = false;
        goop.PersistsOnSamusContact = true;
        samus.InvincibilityTimer = NativeCountdown.Start(0);
        samus.KnockbackTimer = NativeCountdown.Start(0);
    }

    /// <summary>Follows Samus with attached goop and counts down its removal timer.</summary>
    internal void RunAttachedDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState? samus)
    {
        // $86:8DCA: the power-bomb deletion does not end this pre-instruction.
        DeleteEnemyProjectileIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            RemoveAttachedDraygonGoop(goop, samus);
            return;
        }

        goop.Position = goop.Position.WithWhole(new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole + goop.Variable1 * 4 - 12))));
        // The attachment lifetime counts down in variable zero (DEC then BNE): the goop falls off when it reaches zero.
        NativeWordCounterStep lifetime = NativeCountdown.FromRaw(goop.Variable0).Decrement();
        goop.Variable0 = lifetime.Remaining.RawValue;
        if (lifetime.IsZero)
            RemoveAttachedDraygonGoop(goop, samus);
    }

    /// <summary>Releases one attached goop projectile and restores one X-speed divisor step.</summary>
    private static void RemoveAttachedDraygonGoop(
        RoomEnemyProjectileSlot goop,
        SamusState samus)
    {
        goop.Clear();
        samus.XSpeedDivisor = samus.XSpeedDivisor == 0
            ? (ushort)0
            : unchecked((ushort)(samus.XSpeedDivisor - 1));
    }

    /// <summary>
    /// Ports <c>Delete_EnemyProjectile_IfPowerBombed</c> at $86:8D5C for the Draygon goop
    /// and wall-turret shot. Inside the explosion's ellipse it clears the projectile ID and
    /// <c>XSpeedDivisor</c> only; the calling routine continues on the released slot.
    /// </summary>
    internal void DeleteEnemyProjectileIfPowerBombed(RoomEnemyProjectileSlot projectile, SamusState? samus)
    {
        // A standalone projectile pass has no bomb owner, which is the radius-zero case.
        if (_audio.PowerBomb is not { } powerBomb)
            return;
        int horizontalRadius = powerBomb.ExplosionRadius.WholePixels;
        if (horizontalRadius == 0)
            return;
        // `LSR; ADC $12; LSR` carries the first shift's low bit into the addition.
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + (horizontalRadius & 1)) >> 1;
        int xDistance = Math.Abs(unchecked((short)(powerBomb.Position.X - projectile.Position.X.Whole)));
        int yDistance = Math.Abs(unchecked((short)(powerBomb.Position.Y - projectile.Position.Y.Whole)));
        if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
            return;
        projectile.ReleaseIdentityOnly();
        if (samus is not null)
            samus.XSpeedDivisor = 0;
    }

    /// <summary>Tests the projectile against the 512-by-512-pixel Draygon arena bounds.</summary>
    private static bool IsOutsideDraygonRoom(RoomEnemyProjectileSlot projectile) =>
        unchecked((short)projectile.Position.X.Whole) < 0 || projectile.Position.X.Whole >= 512 ||
        unchecked((short)projectile.Position.Y.Whole) < 0 || projectile.Position.Y.Whole >= 512;

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Draygon is not null)
        {
            Draygon.LastSoundLibrary2 = null;
            Draygon.LastSoundLibrary3 = null;
            Draygon.MusicRequest = null;
        }
        ResetDraygonRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // Legacy publications are momentary like their native QueueSfx/QueueMusic calls; clearing them at the
        // EnemyMain frame boundary keeps a paused or frozen actor from replaying its previous request.
        if (Draygon is not null)
        {
            Draygon.LastSoundLibrary2 = null;
            Draygon.LastSoundLibrary3 = null;
            Draygon.MusicRequest = null;
        }
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (Draygon?.LastSoundLibrary2 is SoundEffectId draygonLibrary2)
            audio.QueueEnemySound(draygonLibrary2, maximumQueued: 6);
        if (Draygon?.LastSoundLibrary3 is SoundEffectId draygonLibrary3)
            audio.QueueEnemySound(draygonLibrary3, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(Draygon?.MusicRequest, MusicCommandDelay.EightFrames);
    }

    /// <summary>Registers Draygon's shot response, including acceleration and special death exemption.</summary>
    internal void RegisterDraygonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DraygonShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                if (!c.UsesExtendedHitboxes)
                    throw new InvalidDataException("Draygon requires his authored extended shot hitboxes.");
                if (c.HitboxShotAi == EnemyInteractionCallback.DudShot)
                {
                    // Body, claws, and shell use shared `$A0:8046`; only the first
                    // eye rectangle in maps $1A/$2D reaches Draygon's damage callback.
                    c.Projectiles.EnemyImpacts.ApplyExtendedEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                    _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                    return true;
                }
                if (c.HitboxShotAi != EnemyInteractionCallback.DraygonShot)
                {
                    throw new InvalidDataException(
                        $"Draygon hitbox shot AI $A5:{c.HitboxShotAiPointer:X4} is not translated.");
                }
                return false;
            },
            AfterImpact = c => AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(c.Enemy)),
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) => ResolveDraygonReaction(c.Enemy, c.Samus),
        });
    }

    /// <summary>
    /// Registers $A5:954D hurt AI: flashes body, eye, and tail together, while connected
    /// grapple electrocution may drain another 256 health every eighth frame.
    /// </summary>
    internal void RegisterDraygonHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody,
            (slot, frame) => ApplyDraygonHurt(slot, RequireCompleteDraygonState(slot), frame.Samus));

    /// <summary>The callback of the first hitbox the probe overlaps on these frames, or null when they are not this part's.</summary>
    internal static bool? FindDraygonHitboxCallback(RoomEnemySlot enemy, EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        if (!(DraygonBehavior.IsDraygonDefinition(enemy.EnemyDefinitionPointer) &&
            enemy.Definition.Bank == DraygonBg2FrameDefinitions.Bank))
            return null;

        // Both OAM and BG2 art are replaceable presentation. Their native
        // component offsets and hitbox pointers remain fixed gameplay data.
        foreach (DraygonCollisionComponent component in
                 DraygonCollisionDefinitions.ComponentsAt(
                     enemy.SpritemapPointer))
        {
            Position16<RoomPixels> componentOrigin = enemy.Position.Whole + component.Offset;
            foreach (DraygonCollisionHitbox hitbox in component.Hitboxes)
            {
                ushort left = unchecked((ushort)(componentOrigin.X + hitbox.Left));
                ushort top = unchecked((ushort)(componentOrigin.Y + hitbox.Top));
                ushort right = unchecked((ushort)(componentOrigin.X + hitbox.Right));
                ushort bottom = unchecked((ushort)(componentOrigin.Y + hitbox.Bottom));
                if (!probe.Overlaps(left, top, right, bottom))
                    continue;
                callback = probe.SelectShotCallback
                    ? DraygonHitboxCallbackCodec.ToCallbackWord(hitbox.Shot)
                    : DraygonHitboxCallbackCodec.ToCallbackWord(hitbox.Touch);
                return true;
            }
        }
        return false;
    
    }
}
