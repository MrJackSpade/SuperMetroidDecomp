using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Beetom species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BeetomBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores audio.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Stores frame context.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Stores collision.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Initializes a new beetom behavior instance.</summary>
    internal BeetomBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext, EnemyCollision collision)
    {
        _room = room;
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
    }

    /// <summary>Executes Beetom's private animation instruction.</summary>
    private static bool TryProcessBeetomInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Beetom ||
            !Enum.IsDefined((BeetomInstruction)word))
            return false;

        switch ((BeetomInstruction)word)
        {
            case BeetomInstruction.Nothing:
                // Beetom's initial drain animation calls a literal RTS stub before it
                // falls through into the looping blood-spray frames. It consumes no
                // operand and changes no state beyond advancing the instruction cursor.
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Beetom does not own instruction ${word:X4}.");
        }
    }


    /// <summary>Defines beetom proximity distance.</summary>
    private const ushort BeetomProximityDistance = 0x0060;
    /// <summary>Defines beetom mash count.</summary>
    private const ushort BeetomMashCount = 0x0040;
    /// <summary>Defines beetom maximum yspeed index.</summary>
    private const ushort BeetomMaximumYSpeedIndex = 0x0040;

    /// <summary>Stores beetom installed instruction lists.</summary>
    internal readonly ushort[] _beetomInstalledInstructionLists = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom initial short leap yspeed indexes.</summary>
    internal readonly ushort[] _beetomInitialShortLeapYSpeedIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom initial long leap yspeed indexes.</summary>
    internal readonly ushort[] _beetomInitialLongLeapYSpeedIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom initial lunge yspeed indexes.</summary>
    internal readonly ushort[] _beetomInitialLungeYSpeedIndexes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom falling flags.</summary>
    internal readonly ushort[] _beetomFallingFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom attached to samus flags.</summary>
    internal readonly ushort[] _beetomAttachedToSamusFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom directions.</summary>
    internal readonly ushort[] _beetomDirections = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom initial attachment offsets.</summary>
    internal readonly Offset16<RoomPixels>[] _beetomInitialAttachmentOffsets = new Offset16<RoomPixels>[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Stores beetom states.</summary>
    internal readonly BeetomEnemyState?[] _beetomStates = new BeetomEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Beetom</c> at $A8:B776.</summary>
    internal void InitializeBeetom(RoomEnemySlot slot, SamusState? samus, ushort controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom initialization requires the active Samus actor.");

        var state = new BeetomEnemyState(slot, new BeetomSharedTables { InstalledInstructionLists = _beetomInstalledInstructionLists, InitialShortLeapYSpeedIndexes = _beetomInitialShortLeapYSpeedIndexes, InitialLongLeapYSpeedIndexes = _beetomInitialLongLeapYSpeedIndexes, InitialLungeYSpeedIndexes = _beetomInitialLungeYSpeedIndexes, FallingFlags = _beetomFallingFlags, AttachedFlags = _beetomAttachedToSamusFlags, Directions = _beetomDirections, InitialAttachmentOffsets = _beetomInitialAttachmentOffsets });
        _beetomStates[slot.SlotIndex] = state;

        state.FunctionTimer = 0;
        state.Function = 0;
        state.Falling = false;
        state.AttachedToSamus = false;
        slot.VariableA = 0;
        state.ButtonCounter = BeetomMashCount;
        state.PreviousController1Input = controllerInput;

        // Every Beetom initializer writes the global seed, even when several Beetoms share
        // a room. This is observable cartridge behavior, not a per-actor random stream.
        Action<ushort> setRandomNumber = _room.Random.Seed;
        setRandomNumber(0x0017);

        state.InitialShortLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x3000, 4);
        state.InitialLongLeapYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x4000, 5);
        state.InitialLungeYSpeedIndex = CalculateInitialBeetomYSpeedIndex(0x3000, 3);

        // The source label's branch name is reversed. A negative Samus-minus-enemy delta
        // selects the left-facing list; nonnegative selects the right-facing list.
        SetBeetomInstructionList(
            slot,
            state,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
        state.Function = BeetomEnemyFunction.DecideAction;
    }

    /// <summary>Ports <c>MainAI_Beetom</c> and all indirect function targets.</summary>
    private void RunBeetomMain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort controllerInput)
    {
        switch (state.Function)
        {
            case BeetomEnemyFunction.DecideAction:
                RequireBeetomSamus(samus);
                state.Function = EnemyMath.WrappedMagnitude(unchecked((ushort)(samus!.Position.X.Whole - slot.Position.X.Whole))) < BeetomProximityDistance
                    ? BeetomEnemyFunction.DecideActionSamusInProximity
                    : BeetomEnemyFunction.DecideActionSamusNotInProximity;
                return;
            case BeetomEnemyFunction.DecideActionSamusNotInProximity:
                ChooseDistantBeetomAction(state);
                return;
            case BeetomEnemyFunction.StartIdling:
                state.FunctionTimer = 0x0020;
                state.Function = BeetomEnemyFunction.Idling;
                return;
            case BeetomEnemyFunction.StartCrawlingLeft:
                StartBeetomCrawling(slot, state, left: true);
                return;
            case BeetomEnemyFunction.StartCrawlingRight:
                StartBeetomCrawling(slot, state, left: false);
                return;
            case BeetomEnemyFunction.StartShortHopLeft:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, left: true, longHop: false);
                return;
            case BeetomEnemyFunction.StartShortHopRight:
                StartBeetomHop(slot, state, state.InitialShortLeapYSpeedIndex, left: false, longHop: false);
                return;
            case BeetomEnemyFunction.StartLongHopLeft:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, left: true, longHop: true);
                return;
            case BeetomEnemyFunction.StartLongHopRight:
                StartBeetomHop(slot, state, state.InitialLongLeapYSpeedIndex, left: false, longHop: true);
                return;
            case BeetomEnemyFunction.DecideActionSamusInProximity:
                RequireBeetomSamus(samus);
                StartBeetomLunge(slot, state, samus!);
                return;
            case BeetomEnemyFunction.StartDrainingLeft:
                StartBeetomDrain(slot, state, left: true);
                return;
            case BeetomEnemyFunction.StartDrainingRight:
                StartBeetomDrain(slot, state, left: false);
                return;
            case BeetomEnemyFunction.StartDropping:
                StartBeetomDrop(slot, state);
                return;
            case BeetomEnemyFunction.StartBeingFlung:
                state.YSpeedTableIndex = 0;
                state.Function = BeetomEnemyFunction.BeingFlung;
                return;
            case BeetomEnemyFunction.Idling:
                NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
                state.FunctionTimer = timer.Value;
                if (timer.IsNegative)
                    state.Function = BeetomEnemyFunction.DecideAction;
                return;
            case BeetomEnemyFunction.CrawlingLeft:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level!, left: true);
                return;
            case BeetomEnemyFunction.CrawlingRight:
                RequireBeetomLevel(level);
                RunBeetomCrawl(slot, state, level!, left: false);
                return;
            case BeetomEnemyFunction.ShortHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: false, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.ShortHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: false, risingDelta: 4, fallingDelta: 4);
                return;
            case BeetomEnemyFunction.LongHopLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: false, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LongHopRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: false, risingDelta: 5, fallingDelta: 5);
                return;
            case BeetomEnemyFunction.LungeLeft:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: true, lunge: true, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.LungeRight:
                RequireBeetomLevel(level);
                RunBeetomArc(slot, state, level!, left: false, lunge: true, risingDelta: 5, fallingDelta: 3);
                return;
            case BeetomEnemyFunction.DrainingLeft:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus!, level!, controllerInput, left: true);
                return;
            case BeetomEnemyFunction.DrainingRight:
                RequireBeetomSamus(samus);
                RequireBeetomLevel(level);
                RunBeetomDrain(slot, state, samus!, level!, controllerInput, left: false);
                return;
            case BeetomEnemyFunction.Dropping:
                RequireBeetomLevel(level);
                RunBeetomDrop(slot, state, level!);
                return;
            case BeetomEnemyFunction.BeingFlung:
                RequireBeetomLevel(level);
                RunFlungBeetom(slot, state, level!);
                return;
            default:
                throw new InvalidDataException($"Beetom function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Choose distant beetom action.</summary>
    internal void ChooseDistantBeetomAction(BeetomEnemyState state)
    {
        ushort random = _room.Random.Generate();
        state.Function = (random & 7) switch
        {
            0 or 1 => BeetomEnemyFunction.StartIdling,
            2 or 4 => BeetomEnemyFunction.StartShortHopLeft,
            3 or 7 => BeetomEnemyFunction.StartLongHopRight,
            5 => BeetomEnemyFunction.StartShortHopRight,
            _ => BeetomEnemyFunction.StartLongHopLeft,
        };
        state.Direction = unchecked((ushort)(random & 1));
    }

    /// <summary>Starts beetom crawling.</summary>
    internal static void StartBeetomCrawling(RoomEnemySlot slot, BeetomEnemyState state, bool left)
    {
        state.Function = left ? BeetomEnemyFunction.CrawlingLeft : BeetomEnemyFunction.CrawlingRight;
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
    }

    /// <summary>Starts beetom hop.</summary>
    internal static void StartBeetomHop(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        ushort initialYSpeedIndex,
        bool left,
        bool longHop)
    {
        state.YSpeedTableIndex = initialYSpeedIndex;
        state.Function = (left, longHop) switch
        {
            (true, false) => BeetomEnemyFunction.ShortHopLeft,
            (false, false) => BeetomEnemyFunction.ShortHopRight,
            (true, true) => BeetomEnemyFunction.LongHopLeft,
            _ => BeetomEnemyFunction.LongHopRight,
        };
        state.Falling = false;
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.HopLeft
                : BeetomInstructionProgramDefinitions.HopRight);
    }

    /// <summary>Starts beetom lunge.</summary>
    private static void StartBeetomLunge(RoomEnemySlot slot, BeetomEnemyState state, SamusState samus)
    {
        bool left = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        state.YSpeedTableIndex = state.InitialLungeYSpeedIndex;
        state.Direction = left ? (ushort)0 : (ushort)1;
        state.Function = left ? BeetomEnemyFunction.LungeLeft : BeetomEnemyFunction.LungeRight;
        state.Falling = false;
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.HopLeft
                : BeetomInstructionProgramDefinitions.HopRight);
    }

    /// <summary>Starts beetom drain.</summary>
    internal static void StartBeetomDrain(RoomEnemySlot slot, BeetomEnemyState state, bool left)
    {
        SetBeetomInstructionList(
            slot,
            state,
            left
                ? BeetomInstructionProgramDefinitions.DrainingLeft
                : BeetomInstructionProgramDefinitions.DrainingRight);
        state.Function = left ? BeetomEnemyFunction.DrainingLeft : BeetomEnemyFunction.DrainingRight;
    }

    /// <summary>Starts beetom drop.</summary>
    private static void StartBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state)
    {
        SetBeetomInstructionList(
            slot,
            state,
            state.Direction == 0
                ? BeetomInstructionProgramDefinitions.CrawlingLeft
                : BeetomInstructionProgramDefinitions.CrawlingRight);
        state.Falling = false;
        state.Function = BeetomEnemyFunction.Dropping;
    }

    /// <summary>Runs beetom crawl.</summary>
    private void RunBeetomCrawl(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level, bool left)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNegative)
        {
            state.FunctionTimer = 0x0040;
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        // The native ledge probe temporarily moves the center eight pixels ahead, then
        // performs a real one-pixel downward collision move. Restore only the whole words,
        // preserving collision-produced subposition just as bank $A8 does.
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (left ? -8 : 8))));
        if (!_terrain.MoveEnemyVertically(level, slot, 1 << 16))
        {
            state.Function = left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
            slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - 1)));
            slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (left ? 8 : -8))));
            return;
        }

        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole + (left ? 8 : -8))));
        int displacement = left ? unchecked((int)0xffffc000) : 0x00004000;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            state.Function = left ? BeetomEnemyFunction.StartCrawlingRight : BeetomEnemyFunction.StartCrawlingLeft;
    }

    /// <summary>Runs beetom arc.</summary>
    private void RunBeetomArc(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        RoomLevelData level,
        bool left,
        bool lunge,
        ushort risingDelta,
        ushort fallingDelta)
    {
        RunBeetomVerticalArc(slot, state, level, risingDelta, fallingDelta);

        // The horizontal half still runs on the exact frame that vertical collision changes
        // the function pointer. That ordering is easy to lose when expressing the two axes
        // as a single host movement vector.
        int displacement = lunge
            ? (left ? unchecked((int)0xfffd0000) : 0x00030000)
            : (left ? unchecked((int)0xffffc000) : 0x00004000);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        state.Direction ^= 1;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    /// <summary>Runs beetom vertical arc.</summary>
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

            state.YSpeedTableIndex = unchecked((ushort)(state.YSpeedTableIndex - risingDelta));
            if (unchecked((short)state.YSpeedTableIndex) >= 0)
                return;

            state.YSpeedTableIndex = 0;
            state.Falling = true;
            return;
        }

        if (_terrain.MoveEnemyVertically(level, slot, EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
        {
            state.Function = BeetomEnemyFunction.DecideAction;
            return;
        }

        state.YSpeedTableIndex = unchecked((ushort)Math.Min(
            BeetomMaximumYSpeedIndex,
            state.YSpeedTableIndex + fallingDelta));
    }

    /// <summary>Runs beetom drain.</summary>
    private void RunBeetomDrain(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort controllerInput,
        bool left)
    {
        if (state.ButtonCounter != 0)
        {
            slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, unchecked((ushort)(samus.Position.Y.Whole - 4))));
            if (controllerInput != state.PreviousController1Input)
            {
                state.PreviousController1Input = controllerInput;
                state.ButtonCounter = unchecked((ushort)(state.ButtonCounter - 1));
            }
            return;
        }

        // Escape throws the Beetom sixteen pixels away. A wall reverses the direction and
        // applies an additional 32-pixel move, producing the native net sixteen-pixel throw
        // to the other side rather than merely cancelling the first displacement.
        int initialDisplacement = left ? 16 << 16 : unchecked((int)0xfff00000);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, initialDisplacement))
        {
            state.Direction = left ? (ushort)1 : (ushort)0;
            int reflectedDisplacement = left ? unchecked((int)0xffe00000) : 32 << 16;
            _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, reflectedDisplacement);
        }
        state.AttachedToSamus = false;
        state.Function = BeetomEnemyFunction.StartBeingFlung;
    }

    /// <summary>Runs beetom drop.</summary>
    private void RunBeetomDrop(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (!_terrain.MoveEnemyVertically(level, slot, 3 << 16))
            return;
        state.Function = state.Direction == 0
            ? BeetomEnemyFunction.StartCrawlingLeft
            : BeetomEnemyFunction.StartCrawlingRight;
    }

    /// <summary>Runs flung beetom.</summary>
    private void RunFlungBeetom(RoomEnemySlot slot, BeetomEnemyState state, RoomLevelData level)
    {
        if (_terrain.MoveEnemyVertically(level, slot, EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: false)))
            state.Function = BeetomEnemyFunction.DecideAction;
        else
            state.YSpeedTableIndex = unchecked((ushort)Math.Min(BeetomMaximumYSpeedIndex, state.YSpeedTableIndex + 1));

        // Direction describes the pre-escape facing. The throw deliberately travels in the
        // opposite direction: zero moves right and one moves left.
        int horizontal = state.Direction == 0 ? 2 << 16 : unchecked((int)0xfffe0000);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontal))
            return;
        state.Direction ^= 1;
        state.Function = BeetomEnemyFunction.StartDropping;
    }

    /// <summary>Calculates initial beetom yspeed index.</summary>
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
            state.Function = state.Direction == 0
                ? BeetomEnemyFunction.StartDrainingLeft
                : BeetomEnemyFunction.StartDrainingRight;
            state.AttachedToSamus = true;
            state.ButtonCounter = BeetomMashCount;
            slot.Layer = 2;
            state.InitialAttachmentOffset = samus.Position.Whole - slot.Position.Whole;
        }

        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            _collision.ResolveNormalEnemyTouch(slot, samus);
            samus.InvincibilityTimer = 0;
            samus.KnockbackTimer = 0;
            return;
        }

        if ((_frameContext._randomEnemyCounter & 7) == 7 && samus.Health >= 30)
            _audio.QueueEnemySound(SoundEffectLibrary3Sounds.AttachedEnemyDrain, maximumQueued: 6);

        // Unlike ordinary touch damage, the once-per-64-frame drain explicitly cancels the
        // invincibility and knockback installed by common AI. Apply the suit-scaled health
        // subtraction directly, then reproduce those two clearing stores.
        if ((slot.FrameCounter & 0x003f) != 0x003f)
            return;
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(slot.Definition.Damage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(slot.Definition.Damage >> 1))
                : slot.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
    }

    /// <summary>Ports the private post-common-shot tail at $A8:BEAC.</summary>
    private static void ResolveBeetomShotAfterCommon(RoomEnemySlot slot, BeetomEnemyState state)
    {
        if (slot.FrozenTimer != 0 && state.Function is
            BeetomEnemyFunction.DrainingLeft or BeetomEnemyFunction.DrainingRight)
        {
            state.Function = BeetomEnemyFunction.StartDropping;
        }
        state.AttachedToSamus = false;
    }

    /// <summary>Sets beetom instruction list.</summary>
    private static void SetBeetomInstructionList(
        RoomEnemySlot slot,
        BeetomEnemyState state,
        ushort instructionList)
    {
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Requires beetom samus.</summary>
    private static void RequireBeetomSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Beetom AI requires the active Samus actor.");
    }

    /// <summary>Requires beetom level.</summary>
    private static void RequireBeetomLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Beetom movement requires room level data.");
    }

    /// <summary>Requires beetom state.</summary>
    private BeetomEnemyState RequireBeetomState(RoomEnemySlot slot) =>
        _beetomStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Beetom state.");

    /// <summary>Register beetom bomb reactions.</summary>
    internal void RegisterBeetomBombReactions(EnemyAiTable table)
    {
        // Common damage can clear a lethally hit slot before this private tail runs. Bank $A8 still clears Beetom's
        // separate attached flag after returning from common shot AI, so the identity is matched before dispatch.
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Beetom &&
                                                       EnemyCollision.ShotAi(c.Enemy) == EnemyInteractionCallback.BeetomShot &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BeetomShot)
        {
            AfterCommon = c => ResolveBeetomShotAfterCommon(c.Enemy, RequireBeetomState(c.Enemy)),
        });
    }

    /// <summary>Register beetom initialization ai.</summary>
    internal void RegisterBeetomInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Beetom, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBeetom(slot, samus, controllerInput);
        });
    }

    /// <summary>Register beetom main ai.</summary>
    internal void RegisterBeetomMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Beetom, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom, (slot, frame, nmiFrameCounter8) =>
        {
            RunBeetomMain(slot, RequireBeetomState(slot), frame.Samus, frame.Level, frame.ControllerInput);
        });
    }

    /// <summary>Register beetom owned instructions.</summary>
    internal static void RegisterBeetomOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessBeetomInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Register beetom shot reactions.</summary>
    internal void RegisterBeetomShotReactions(EnemyAiTable table)
    {
        // Beetom's private tail runs after common shot AI for every accepted hit, including immune/zero-damage
        // vulnerability results.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Beetom &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BeetomShot)
        {
            AfterCommon = (c, _) => ResolveBeetomShotAfterCommon(c.Enemy, RequireBeetomState(c.Enemy)),
        });
    }

    /// <summary>Register beetom touch.</summary>
    internal void RegisterBeetomTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Beetom &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BeetomTouch,
            context => ResolveBeetomTouch(context.Slot, RequireBeetomState(context.Slot), context.Samus));
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_beetomStates);
        Array.Clear(_beetomInstalledInstructionLists);
        Array.Clear(_beetomInitialShortLeapYSpeedIndexes);
        Array.Clear(_beetomInitialLongLeapYSpeedIndexes);
        Array.Clear(_beetomInitialLungeYSpeedIndexes);
        Array.Clear(_beetomFallingFlags);
        Array.Clear(_beetomAttachedToSamusFlags);
        Array.Clear(_beetomDirections);
        Array.Clear(_beetomInitialAttachmentOffsets);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
