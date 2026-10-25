using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The Beetom species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BeetomBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Beetom];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BeetomEntity(this, slot, common);

    /// <summary>Supplies the shared RNG seed and generation used by Beetom action choice.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Beetom ledge, wall, and landing collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues sounds produced during Beetom movement and contact.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the active frame's Samus and room data to callbacks.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Handles common contact before Beetom's attachment behavior.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds room, terrain, audio, and collision services for Beetom AI.</summary>
    internal BeetomBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext, EnemyCollision collision)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Beetom's animations: off-screen processing, or the drain's no-op stub.</summary>
    private static BeetomProgram? ApplyBeetomCue(RoomEnemySlot slot, BeetomCue cue)
    {
        switch (cue.Kind)
        {
            case BeetomCueKind.ProcessOffScreen:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case BeetomCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            case BeetomCueKind.Nothing:
                // Beetom's initial drain animation calls a literal RTS stub before it falls through into the looping
                // blood-spray frames. It changes no state.
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BeetomCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Strict distance around Samus that changes Beetom's action selection.</summary>
    private const ushort BeetomProximityDistance = 96;
    /// <summary>Input changes required to dislodge an attached draining Beetom.</summary>
    private const ushort BeetomMashCount = 64;

    /// <summary>The words each physical slot keeps across Beetom occupants for the current room.</summary>
    private readonly BeetomExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static BeetomExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, EnemySlotTable.MaximumEnemyCount).Select(_ => new BeetomExtensionWords())];

    /// <summary>Ports <c>InitAI_Beetom</c> at $A8:B776.</summary>
    internal void InitializeBeetom(RoomEnemySlot slot, SamusState? samus, SnesButton controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom initialization requires the active Samus actor.");

        var state = new BeetomEnemyState(_extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.FunctionTimer = WrappingCountdown.Start(0);
        state.Function = null;
        state.Falling = false;
        state.AttachedToSamus = false;
        state.ButtonCounter = WrappingCountdown.Start(BeetomMashCount);
        state.PreviousController1Input = controllerInput;

        // Every Beetom initializer writes the global seed, even when several Beetoms share
        // a room. This is observable cartridge behavior, not a per-actor random stream.
        Action<int> setRandomNumber = _room.Random.Seed;
        setRandomNumber(23);

        state.InitialShortLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(12288, 4);
        state.InitialLongLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(16384, 5);
        state.InitialLungeYSpeedIndex = CalculateInitialBeetomYSpeedIndex(12288, 3);

        // The source label's branch name is reversed. A negative Samus-minus-enemy delta
        // selects the left-facing list; nonnegative selects the right-facing list.
        SetBeetomInstructionList(
            slot,
            state,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
                ? BeetomProgram.CrawlingLeft
                : BeetomProgram.CrawlingRight);
        state.Function = BeetomEnemyFunction.DecideAction;
    }

    /// <summary>Ports <c>MainAI_Beetom</c> and all indirect function targets.</summary>
    private void RunBeetomMain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        SnesButton controllerInput)
    {
        switch (state.Function)
        {
            case BeetomEnemyFunction.DecideAction:
                RequireBeetomSamus(samus);
                state.Function = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole))) < BeetomProximityDistance
                    ? BeetomEnemyFunction.DecideActionSamusInProximity
                    : BeetomEnemyFunction.DecideActionSamusNotInProximity;
                return;
            case BeetomEnemyFunction.DecideActionSamusNotInProximity:
                ChooseDistantBeetomAction(state);
                return;
            case BeetomEnemyFunction.StartIdling:
                state.FunctionTimer = WrappingCountdown.Start(32);
                state.Function = BeetomEnemyFunction.Idling;
                return;
            case BeetomEnemyFunction.StartCrawlingLeft:
                StartBeetomCrawling(slot, state, direction: HorizontalDirection.Left);
                return;
            case BeetomEnemyFunction.StartCrawlingRight:
                StartBeetomCrawling(slot, state, direction: HorizontalDirection.Right);
                return;
            case BeetomEnemyFunction.StartShortHopLeft:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, direction: HorizontalDirection.Left, BeetomHopLength.Short);
                return;
            case BeetomEnemyFunction.StartShortHopRight:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, direction: HorizontalDirection.Right, BeetomHopLength.Short);
                return;
            case BeetomEnemyFunction.StartLongHopLeft:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, direction: HorizontalDirection.Left, BeetomHopLength.Long);
                return;
            case BeetomEnemyFunction.StartLongHopRight:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, direction: HorizontalDirection.Right, BeetomHopLength.Long);
                return;
            case BeetomEnemyFunction.DecideActionSamusInProximity:
                RequireBeetomSamus(samus);
                StartBeetomLunge(slot, state, samus);
                return;
            case BeetomEnemyFunction.StartDrainingLeft:
                StartBeetomDrain(slot, state, side: HorizontalDirection.Left);
                return;
            case BeetomEnemyFunction.StartDrainingRight:
                StartBeetomDrain(slot, state, side: HorizontalDirection.Right);
                return;
            case BeetomEnemyFunction.StartDropping:
                StartBeetomDrop(slot, state);
                return;
            case BeetomEnemyFunction.StartBeingFlung:
                state.YSpeedTableIndex = 0;
                state.Function = BeetomEnemyFunction.BeingFlung;
                return;
            case BeetomEnemyFunction.Idling:
                CountdownStep timer = state.FunctionTimer.Decrement();
                state.FunctionTimer = timer.Remaining;
                if (timer.IsNegative)
                    state.Function = BeetomEnemyFunction.DecideAction;
                return;
            case BeetomEnemyFunction.CrawlingLeft:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level, direction: HorizontalDirection.Left);
                return;
            case BeetomEnemyFunction.CrawlingRight:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level, direction: HorizontalDirection.Right);
                return;
            case BeetomEnemyFunction.ShortHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.ShortHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.LongHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LongHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Hop, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LungeLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Left, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.LungeRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level, direction: HorizontalDirection.Right, BeetomArcKind.Lunge, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.DrainingLeft:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus, level, controllerInput, side: HorizontalDirection.Left);
                return;
            case BeetomEnemyFunction.DrainingRight:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus, level, controllerInput, side: HorizontalDirection.Right);
                return;
            case BeetomEnemyFunction.Dropping:
                RequireBeetomLevel(level);
                RunBeetomDrop(slot, state, level);
                return;
            case BeetomEnemyFunction.BeingFlung:
                RequireBeetomLevel(level);
                RunFlungBeetom(slot, state, level);
                return;
            default:
                throw new InvalidDataException($"Beetom function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Uses the shared native RNG's low three bits to choose an idle or hop branch.</summary>
    internal void ChooseDistantBeetomAction(BeetomEnemyState state)
    {
        int random = _room.Random.Generate();
        state.Function = (random & 7) switch
        {
            0 or 1 => BeetomEnemyFunction.StartIdling,
            2 or 4 => BeetomEnemyFunction.StartShortHopLeft,
            3 or 7 => BeetomEnemyFunction.StartLongHopRight,
            5 => BeetomEnemyFunction.StartShortHopRight,
            _ => BeetomEnemyFunction.StartLongHopLeft,
        };
        state.FacingRight = (random & 1) != 0;
    }

    /// <summary>Starts side-specific crawl AI and animation.</summary>
    internal static void StartBeetomCrawling(RoomEnemySlot slot, BeetomEnemyState state, HorizontalDirection direction)
    {
        state.Function = direction == HorizontalDirection.Left ? BeetomEnemyFunction.CrawlingLeft : BeetomEnemyFunction.CrawlingRight;
        SetBeetomInstructionList(
            slot,
            state,
            direction == HorizontalDirection.Left
                ? BeetomProgram.CrawlingLeft
                : BeetomProgram.CrawlingRight);
    }

    /// <summary>Arms a short or long arc from its precomputed initial speed index.</summary>
    internal static void StartBeetomHop(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        int initialYSpeedIndex,
        HorizontalDirection direction,
        BeetomHopLength length)
    {
        state.YSpeedTableIndex = initialYSpeedIndex;
        state.Function = new BeetomHop(direction, length) switch
        {
            (HorizontalDirection.Left, BeetomHopLength.Short) => BeetomEnemyFunction.ShortHopLeft,
            (HorizontalDirection.Right, BeetomHopLength.Short) => BeetomEnemyFunction.ShortHopRight,
            (HorizontalDirection.Left, BeetomHopLength.Long) => BeetomEnemyFunction.LongHopLeft,
            (HorizontalDirection.Right, BeetomHopLength.Long) => BeetomEnemyFunction.LongHopRight,
            _ => throw new ArgumentOutOfRangeException(nameof(length), length, "Unknown Beetom hop."),
        };
        state.Falling = false;
        SetBeetomInstructionList(
            slot,
            state,
            direction == HorizontalDirection.Left
                ? BeetomProgram.HopLeft
                : BeetomProgram.HopRight);
    }

    /// <summary>Aims the lunge toward Samus using the native signed horizontal difference.</summary>
    private static void StartBeetomLunge(RoomEnemySlot slot, BeetomEnemyState state, SamusState samus)
    {
        bool left = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        state.YSpeedTableIndex = state.InitialLungeYSpeedIndex;
        state.FacingRight = !left;
        state.Function = state.FacingRight ? BeetomEnemyFunction.LungeRight : BeetomEnemyFunction.LungeLeft;
        state.Falling = false;
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomProgram.HopLeft
                : BeetomProgram.HopRight);
    }

    /// <summary>Starts the attached blood-drain instruction list for the current facing.</summary>
    internal static void StartBeetomDrain(RoomEnemySlot slot, BeetomEnemyState state, HorizontalDirection side)
    {
        SetBeetomInstructionList(
            slot,
            state,
            side == HorizontalDirection.Left
                ? BeetomProgram.DrainingLeft
                : BeetomProgram.DrainingRight);
        state.Function = side == HorizontalDirection.Left ? BeetomEnemyFunction.DrainingLeft : BeetomEnemyFunction.DrainingRight;
    }

    /// <summary>Switches back to crawl artwork before dropping toward the floor.</summary>
    private static void StartBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state)
    {
        SetBeetomInstructionList(
            slot,
            state,
            state.FacingRight
                ? BeetomProgram.CrawlingRight
                : BeetomProgram.CrawlingLeft);
        state.Falling = false;
        state.Function = BeetomEnemyFunction.Dropping;
    }

    /// <summary>Probes eight pixels ahead for a ledge, then crawls or reverses at terrain.</summary>
    private void RunBeetomCrawl(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level, HorizontalDirection direction)
    {
        CountdownStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNegative)
        {
            state.FunctionTimer = WrappingCountdown.Start(64);
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        // The native ledge probe temporarily moves the center eight pixels ahead, then
        // performs a real one-pixel downward collision move. Restore only the whole words,
        // preserving collision-produced subposition just as bank $A8 does.
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? -8 : 8))));
        if (!_terrain.MoveEnemyVertically(level, slot, 1 << 16))
        {
            state.Function = direction == HorizontalDirection.Left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 1)));
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8))));
            return;
        }

        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (direction == HorizontalDirection.Left ? 8 : -8))));
        int displacement = direction == HorizontalDirection.Left ? -(1 << 14) : 1 << 14;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            state.Function = direction == HorizontalDirection.Left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
    }

    /// <summary>Runs vertical arc before horizontal movement even when the first half changes AI state.</summary>
    private void RunBeetomArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        HorizontalDirection direction,
        BeetomArcKind arc,
        ushort risingDelta,
        ushort fallingDelta)
    {
        RunBeetomVerticalArc(slot, state, level, risingDelta, fallingDelta);

        // The horizontal half still runs on the exact frame that vertical collision changes
        // the function pointer. That ordering is easy to lose when expressing the two axes
        // as a single host movement vector.
        int displacement = arc == BeetomArcKind.Lunge
            ? (direction == HorizontalDirection.Left ? -(3 << 16) : 3 << 16)
            : (direction == HorizontalDirection.Left ? -(1 << 14) : 1 << 14);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        state.FacingRight = !state.FacingRight;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    /// <summary>Advances quadratic ascent/descent indices and switches state at ceiling or ground contact.</summary>
    private void RunBeetomVerticalArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        ushort risingDelta,
        ushort fallingDelta)
    {
        if (!state.Falling)
        {
            if (_terrain.MoveEnemyVertically(level, slot, EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: true)))
            {
                state.Function = BeetomEnemyFunction.StartDropping;
                return;
            }

            state.StepRisingSpeed(risingDelta);
            return;
        }

        if (_terrain.MoveEnemyVertically(level, slot, EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
        {
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        state.StepFallingSpeed(fallingDelta);
    }

    /// <summary>Follows Samus until enough input changes occur, then throws Beetom clear of terrain.</summary>
    private void RunBeetomDrain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        SnesButton controllerInput,
        HorizontalDirection side)
    {
        if (!state.ButtonCounter.IsZero)
        {
            slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole - 4))));
            if (controllerInput != state.PreviousController1Input)
            {
                state.PreviousController1Input = controllerInput;
                state.ButtonCounter = state.ButtonCounter.Decrement().Remaining;
            }
            return;
        }

        // Escape throws the Beetom sixteen pixels away. A wall reverses the direction and
        // applies an additional 32-pixel move, producing the native net sixteen-pixel throw
        // to the other side rather than merely cancelling the first displacement.
        int initialDisplacement = side == HorizontalDirection.Left ? 16 << 16 : -(16 << 16);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, initialDisplacement))
        {
            state.FacingRight = side == HorizontalDirection.Left;
            int reflectedDisplacement = side == HorizontalDirection.Left ? -(32 << 16) : 32 << 16;
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Function = BeetomEnemyFunction.StartBeingFlung;
    }

    /// <summary>Falls three pixels per frame and resumes crawling on floor contact.</summary>
    private void RunBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, slot, 3 << 16))
            return;
        state.Function = state.FacingRight
            ? BeetomEnemyFunction.StartCrawlingRight
            : BeetomEnemyFunction.StartCrawlingLeft;
    }

    /// <summary>Moves opposite the pre-escape facing until a floor or wall selects its next action.</summary>
    private void RunFlungBeetom(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, slot, EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
            state.Function = BeetomEnemyFunction.DecideAction;
        else
            state.StepFallingSpeed(1);

        // The facing is the pre-escape facing. The throw deliberately travels in the
        // opposite direction: facing left moves right and facing right moves left.
        int horizontal = state.FacingRight ? -(2 << 16) : 2 << 16;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontal))
            return;
        state.FacingRight = !state.FacingRight;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    /// <summary>Accumulates the native unaligned quadratic-speed words until a jump height is reached.</summary>
    internal static ushort CalculateInitialBeetomYSpeedIndex(ushort targetHeight, ushort tableIndexDelta)
    {
        ushort tableIndex = 0;
        ushort accumulatedHeight = 0;
        for (int iteration = 0; iteration < ushort.MaxValue; iteration++)
        {
            tableIndex = unchecked((ushort)(tableIndex + tableIndexDelta));
            // The ROM reads at table+1, intentionally combining bytes from adjacent fixed-
            // point fields. An aligned host read changes every resulting jump arc.
            accumulatedHeight = unchecked((ushort)(accumulatedHeight +
                EnemyQuadraticSpeedDefinitions.ReadWord(tableIndex * 8 + 1)));
            if (unchecked((short)(accumulatedHeight - targetHeight)) >= 0)
                return tableIndex;
        }
        throw new InvalidDataException("Beetom initial-speed calculation did not reach its ROM target height.");
    }

    /// <summary>Ports Beetom's custom touch handler at $A8:BE2E.</summary>
    private void ResolveBeetomTouch(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus)
    {
        if (!state.AttachedToSamus)
        {
            state.Function = state.FacingRight
                ? BeetomEnemyFunction.StartDrainingRight
                : BeetomEnemyFunction.StartDrainingLeft;
            state.AttachedToSamus = true;
            state.ButtonCounter = WrappingCountdown.Start(BeetomMashCount);
            slot.Layer = 2;
            state.InitialAttachmentOffset = samus.Position.Whole - slot.Position.Whole;
        }

        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            _collision.ResolveNormalEnemyTouch(slot, samus);
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
            samus.KnockbackTimer = WrappingCountdown.Start(0);
            return;
        }

        if ((_frameContext.RandomEnemyCounter & 7) == 7 && samus.Health >= 30)
            _audio.QueueEnemySound(Library3Sound.IncrementalEnergy, maximumQueued: SoundQueueLimit.Max6);

        // Unlike ordinary touch damage, the once-per-64-frame drain explicitly cancels the
        // invincibility and knockback installed by common AI. Apply the suit-scaled health
        // subtraction directly, then reproduce those two clearing stores.
        if ((slot.FrameCounter & 63) != 63)
            return;
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(slot.Definition.Damage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 1))
                : slot.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? 0
            : samus.Health - damage;
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports the private post-common-shot tail at $A8:BEAC.</summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemySlot slot, BeetomEnemyState state)
    {
        if (!slot.FrozenTimer.IsZero && state.Function is
            BeetomEnemyFunction.DrainingLeft or BeetomEnemyFunction.DrainingRight)
        {
            state.Function = BeetomEnemyFunction.StartDropping;
        }
        state.AttachedToSamus = false;
    }

    /// <summary>Installs a Beetom animation and primes its instruction timer.</summary>
    private static void SetBeetomInstructionList(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        BeetomProgram program)
    {
        state.Program = AnimationProgramCursor<BeetomProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Requires a live Samus actor for attachment and targeting phases.</summary>
    private static void RequireBeetomSamus([NotNull] SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom AI requires the active Samus actor.");
    }

    /// <summary>Requires room collision data before moving Beetom through terrain.</summary>
    private static void RequireBeetomLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Beetom movement requires room level data.");
    }

    /// <summary>Runs Beetom's attachment-clearing tail after common bomb damage.</summary>
    internal static void AddBeetomBombReactions(EnemySharedReactions table)
    {
        // Common damage can clear a lethally hit slot before this private tail runs. Bank $A8 still clears Beetom's
        // separate attached flag after returning from common shot AI, so the identity is matched before dispatch.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Beetom &&
                                                       EnemyCollision.ShotAi(c.Enemy) == EnemyInteractionCallback.BeetomShot &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BeetomShot)
        {
            AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<BeetomEnemyState>()),
        });
    }

    /// <summary>Ports Beetom's initialization AI.</summary>
    internal void RunBeetomInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeBeetom(slot, surroundings.Samus, surroundings.ControllerInput);
    }

    /// <summary>Ports Beetom's main AI.</summary>
    internal void RunBeetomMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBeetomMain(slot, slot.RequireSpeciesState<BeetomEnemyState>(), frame.Samus, frame.Level, frame.ControllerInput);
    }

    /// <summary>Advances Beetom's typed animation.</summary>
    internal static void AdvanceBeetomTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, BeetomAnimations.Programs, BeetomFrameDefinitions.ShownFrames.Of,
            static (BeetomEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyBeetomCue(slot, cue));

    /// <summary>Clears attachment after every accepted common shot, including zero damage.</summary>
    internal static void AddBeetomShotReactions(EnemySharedReactions table)
    {
        // Beetom's private tail runs after common shot AI for every accepted hit, including immune/zero-damage
        // vulnerability results.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Beetom &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BeetomShot)
        {
            AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy, c.Enemy.RequireSpeciesState<BeetomEnemyState>()),
        });
    }

    /// <summary>Attaches Beetom to Samus on its selected custom contact callback.</summary>
    internal void AddBeetomTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Beetom &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BeetomTouch,
            context => ResolveBeetomTouch(context.Slot, context.Slot.RequireSpeciesState<BeetomEnemyState>(), context.Samus));
    }
}
