using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A3 function pointer stored in a creepy-crawly enemy's native variable F.</summary>
public enum CrawlerEnemyFunction : ushort
{
    HZoomerInstructionPending = 0xe08a,
    HZoomerCrawlingVertically = 0xe091,
    HZoomerCrawlingHorizontally = 0xe168,
    InstructionPending = 0xe6c1,
    CrawlingVertically = 0xe6c8,
    Falling = 0xe785,
    CrawlingHorizontally = 0xe7f2,
}

/// <summary>
/// Typed debugger view of the shared wall-crawler state used by Zoomer, Zeela, Sova, Viola,
/// Sciser, Zero, and their variants. Variables A/B/F remain physically owned by the common
/// enemy slot; the four fields after parameter two model the native parallel WRAM tables.
/// </summary>
public sealed class CrawlerEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal CrawlerEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort XVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public ushort YVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    public CrawlerEnemyFunction Function
    {
        get => (CrawlerEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    public ushort FallingYSubvelocity { get; internal set; }
    public ushort FallingYVelocity { get; internal set; }
    public CrawlerEnemyFunction NonFallingFunction { get; internal set; }
    public ushort ConsecutiveTurnCounter { get; internal set; }
}

/// <summary>
/// Literal shared crawler translation from $A3:E660-$E92E. The first live consumer is enemy
/// $DCFF (Zoomer), but the dispatch is deliberately shared because seven other cartridge
/// families enter the exact same initializer/main state machine.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort SciserDefinition = 0xd77f;
    internal const ushort ZeroDefinition = 0xd7bf;
    internal const ushort ViolaDefinition = 0xdabf;
    internal const ushort ZeelaDefinition = 0xdc7f;
    internal const ushort SovaDefinition = 0xdcbf;
    internal const ushort HZoomerDefinition = 0xdc3f;
    internal const ushort ZoomerDefinition = 0xdcff;
    internal const ushort StoneZoomerDefinition = 0xdd3f;

    private const int SciserInitialInstructionTable = 0xa396db;
    private const int ZeroInitialInstructionTable = 0xa3992b;
    private const int ViolaInitialInstructionTable = 0xa3b667;
    private const int SharedCrawlerInitialInstructionTable = 0xa3e2cc;
    private const int CrawlerSpeedTable = 0xa3e5f0;
    private const int CrawlerUpsideDownInstructionTable = 0xa3e630;
    private const int CrawlerUpsideUpInstructionTable = 0xa3e63c;
    private const int CrawlerUpsideRightInstructionTable = 0xa3e648;
    private const int CrawlerUpsideLeftInstructionTable = 0xa3e654;
    private const int CrawlerSlopeSpeedMultiplierTable = 0xa3e931;
    private const int HZoomerInitialInstructionTable = 0xa3e03b;
    private const ushort HZoomerUpsideRightInstructionList = 0xdfcb;
    private const ushort HZoomerUpsideLeftInstructionList = 0xdfe7;
    private const ushort HZoomerUpsideDownInstructionList = 0xe003;
    private const ushort HZoomerUpsideUpInstructionList = 0xe01f;

    private readonly CrawlerEnemyState?[] _crawlerStates =
        new CrawlerEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by a crawler family.</summary>
    public IReadOnlyList<CrawlerEnemyState?> CrawlerStates => _crawlerStates;

    /// <summary>
    /// Ports the species wrappers at $A3:96E3/$993B/$B66F/$E2D4/$E59C/$E669 and their
    /// shared initializer at $A3:E67A. Each wrapper differs only by its four-list table and
    /// the species byte offset later consumed by the shared surface-orientation tables.
    /// </summary>
    private void InitializeCrawler(
        RoomEnemySlot slot,
        int initialInstructionTable,
        ushort? speciesInstructionOffset = null)
    {
        var state = new CrawlerEnemyState(slot);
        _crawlerStates[slot.SlotIndex] = state;

        if (speciesInstructionOffset is ushort offset)
            slot.Parameter2 = offset;

        // The population initialization parameter is an orientation index, not an
        // instruction pointer. Masking to two bits before the ROM table lookup is native.
        int orientation = slot.CurrentInstruction & 3;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            initialInstructionTable + orientation * 2);
        slot.SpritemapPointer = 0x804d; // Spritemap_Common_Nothing.
        slot.InstructionTimer = 1;
        state.Function = CrawlerEnemyFunction.InstructionPending;

        if (slot.Parameter1 != 0x00ff)
        {
            if (slot.Parameter1 >= 32)
            {
                throw new InvalidDataException(
                    $"Crawler speed parameter ${slot.Parameter1:X4} exceeds $A3:E5F0.");
            }
            ushort velocity = ReadWord(_bus!, CrawlerSpeedTable + slot.Parameter1 * 2);
            state.XVelocity = velocity;
            state.YVelocity = velocity;
        }

        // The low two population-property bits encode which side of the starting block the
        // crawler occupies. They are not independent EnemyProperties flags.
        switch (slot.Properties & 3)
        {
            case 0:
                state.XVelocity = Negate16(state.XVelocity);
                break;
            case 2:
                state.YVelocity = Negate16(state.YVelocity);
                break;
        }
    }

    private static bool IsSharedCrawlerDefinition(ushort definitionPointer) =>
        definitionPointer is
            SciserDefinition or
            ZeroDefinition or
            ViolaDefinition or
            ZeelaDefinition or
            SovaDefinition or
            ZoomerDefinition or
            StoneZoomerDefinition;

    /// <summary>Ports the separate orange-Zoomer initializer at $A3:E043.</summary>
    private void InitializeHZoomer(RoomEnemySlot slot)
    {
        if (slot.Parameter1 >= 32)
        {
            throw new InvalidDataException(
                $"HZoomer speed parameter ${slot.Parameter1:X4} exceeds $A3:E5F0.");
        }

        var state = new CrawlerEnemyState(slot)
        {
            Function = CrawlerEnemyFunction.HZoomerInstructionPending,
        };
        _crawlerStates[slot.SlotIndex] = state;
        int orientation = slot.CurrentInstruction & 3;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            HZoomerInitialInstructionTable + orientation * 2);
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
        ushort velocity = ReadWord(_bus!, CrawlerSpeedTable + slot.Parameter1 * 2);
        state.XVelocity = velocity;
        state.YVelocity = velocity;
        switch (slot.Properties & 3)
        {
            case 0:
                state.XVelocity = Negate16(state.XVelocity);
                break;
            case 2:
                state.YVelocity = Negate16(state.YVelocity);
                break;
        }
    }

    /// <summary>
    /// Ports $A3:E08B-$E23B. This is not an alias of common crawler main: while attached,
    /// orange Zoomer reverses its tangent to pursue Samus and reacts to earthquake $14/$1E.
    /// </summary>
    private void RunHZoomerMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("HZoomer AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("HZoomer movement requires room collision data.");
        CrawlerEnemyState state = RequireCrawlerState(slot);
        switch (state.Function)
        {
            case CrawlerEnemyFunction.HZoomerInstructionPending:
                return;
            case CrawlerEnemyFunction.HZoomerCrawlingVertically:
                RunHZoomerVertical(slot, state, samus, level);
                return;
            case CrawlerEnemyFunction.HZoomerCrawlingHorizontally:
                RunHZoomerHorizontal(slot, state, samus, level);
                return;
            case CrawlerEnemyFunction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            default:
                throw new NotSupportedException(
                    $"HZoomer function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunHZoomerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerHZoomerEarthquakeFall(state);
        int wallProbe = Shift8AddMagnitude(state.XVelocity, 1);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            AlignEnemyYWithNonSquareSlope(level, slot);
            int tangent = unchecked((short)state.YVelocity) << 8;
            if (MoveEnemyVertically(level, slot, tangent))
            {
                state.XVelocity = Negate16(state.XVelocity);
                SetHZoomerVerticalInstruction(slot, state);
                return;
            }

            bool samusBelow = unchecked((short)(samus.YPosition - slot.YPosition)) >= 0;
            bool movingDown = unchecked((short)state.YVelocity) >= 0;
            if (samusBelow != movingDown)
                state.YVelocity = Negate16(state.YVelocity);
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }
        state.YVelocity = Negate16(state.YVelocity);
        SetHZoomerVerticalInstruction(slot, state);
    }

    private void RunHZoomerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        TriggerHZoomerEarthquakeFall(state);
        int surfaceProbe = Shift8AddMagnitude(state.YVelocity, 1);
        if (MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            int tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                state.YVelocity = Negate16(state.YVelocity);
                SetHZoomerHorizontalInstruction(slot, state);
                return;
            }

            AlignEnemyYWithNonSquareSlope(level, slot);
            bool samusRight = unchecked((short)(samus.XPosition - slot.XPosition)) >= 0;
            bool movingRight = unchecked((short)state.XVelocity) >= 0;
            if (samusRight != movingRight)
                state.XVelocity = Negate16(state.XVelocity);
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if (unchecked((short)(state.ConsecutiveTurnCounter - 4)) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }
        state.XVelocity = Negate16(state.XVelocity);
        SetHZoomerHorizontalInstruction(slot, state);
    }

    private void TriggerHZoomerEarthquakeFall(CrawlerEnemyState state)
    {
        if (EarthquakeTimer == 0x001e && EarthquakeType == 0x0014)
            BeginCrawlerFall(state);
    }

    private static void SetHZoomerVerticalInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerInstructionList(
            slot,
            unchecked((short)state.YVelocity) < 0
                ? HZoomerUpsideDownInstructionList
                : HZoomerUpsideUpInstructionList);

    private static void SetHZoomerHorizontalInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state) =>
        SetHZoomerInstructionList(
            slot,
            unchecked((short)state.XVelocity) < 0
                ? HZoomerUpsideRightInstructionList
                : HZoomerUpsideLeftInstructionList);

    private static void SetHZoomerInstructionList(
        RoomEnemySlot slot,
        ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports shared crawler main AI at $A3:E6C2.</summary>
    private void RunCrawlerMain(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Crawler movement requires active room collision data.");
        CrawlerEnemyState state = RequireCrawlerState(slot);
        switch (state.Function)
        {
            case CrawlerEnemyFunction.InstructionPending:
                return;
            case CrawlerEnemyFunction.CrawlingVertically:
                RunCrawlerVertical(slot, state, level);
                return;
            case CrawlerEnemyFunction.Falling:
                RunCrawlerFalling(slot, state, level);
                return;
            case CrawlerEnemyFunction.CrawlingHorizontally:
                RunCrawlerHorizontal(slot, state, level);
                return;
            default:
                throw new NotSupportedException(
                    $"Crawler function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports $A3:E6C8: move while attached to a vertical wall.</summary>
    private void RunCrawlerVertical(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        int wallProbe = Shift8AddMagnitude(state.XVelocity, 1);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, wallProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            AlignEnemyYWithNonSquareSlope(level, slot);
            int tangent = unchecked((short)state.YVelocity) << 8;
            if (!MoveEnemyVertically(level, slot, tangent))
                return;

            // Inside corner: the wall normal changes from X to Y.
            state.XVelocity = Negate16(state.XVelocity);
            SetCrawlerVerticalSurfaceInstruction(slot, state);
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if ((short)(state.ConsecutiveTurnCounter - 4) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }

        // Outside corner: reverse the tangent for up to three frames while the instruction
        // list rotates the actor around the exposed block edge.
        state.YVelocity = Negate16(state.YVelocity);
        SetCrawlerVerticalSurfaceInstruction(slot, state);
    }

    /// <summary>Ports $A3:E7F2: move while attached to a floor or ceiling.</summary>
    private void RunCrawlerHorizontal(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        int surfaceProbe = Shift8AddMagnitude(state.YVelocity, 1);
        if (MoveEnemyVertically(level, slot, surfaceProbe))
        {
            state.ConsecutiveTurnCounter = 0;
            int tangent = GetCrawlerSlopeAdjustedHorizontalDisplacement(slot, state, level);
            if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, tangent))
            {
                // Inside corner: the surface normal changes from Y to X.
                state.YVelocity = Negate16(state.YVelocity);
                SetCrawlerHorizontalSurfaceInstruction(slot, state);
            }
            else
            {
                AlignEnemyYWithNonSquareSlope(level, slot);
            }
            return;
        }

        state.ConsecutiveTurnCounter = unchecked((ushort)(state.ConsecutiveTurnCounter + 1));
        if ((short)(state.ConsecutiveTurnCounter - 4) >= 0)
        {
            BeginCrawlerFall(state);
            return;
        }

        state.XVelocity = Negate16(state.XVelocity);
        SetCrawlerHorizontalSurfaceInstruction(slot, state);
    }

    /// <summary>Ports the earthquake/outside-corner falling owner at $A3:E785.</summary>
    private void RunCrawlerFalling(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level)
    {
        int displacement = unchecked(
            ((short)state.FallingYVelocity << 16) | state.FallingYSubvelocity);
        if (MoveEnemyVertically(level, slot, displacement))
        {
            // Parameter $FF is the native "choose default speed after first landing" path.
            if (slot.Parameter1 == 0x00ff)
            {
                state.XVelocity = 0x0080;
                state.YVelocity = 0x0080;
            }
            state.FallingYSubvelocity = 0;
            state.FallingYVelocity = 0;
            state.ConsecutiveTurnCounter = 0;
            state.Function = state.NonFallingFunction;
            return;
        }

        if ((short)(state.FallingYVelocity - 4) < 0)
        {
            uint velocity = ((uint)state.FallingYVelocity << 16) | state.FallingYSubvelocity;
            velocity = unchecked(velocity + 0x00008000);
            state.FallingYVelocity = unchecked((ushort)(velocity >> 16));
            state.FallingYSubvelocity = unchecked((ushort)velocity);
        }

        // The native routine contains this zero-velocity escape even though ordinary falls
        // immediately gain gravity. Retain it for ROM-modified parameter/state combinations.
        if (state.FallingYSubvelocity == 0 && state.FallingYVelocity == 0)
            state.Function = CrawlerEnemyFunction.CrawlingVertically;
    }

    private static void BeginCrawlerFall(CrawlerEnemyState state)
    {
        state.NonFallingFunction = state.Function;
        state.Function = CrawlerEnemyFunction.Falling;
    }

    private void SetCrawlerVerticalSurfaceInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        int table = (short)state.YVelocity < 0
            ? CrawlerUpsideDownInstructionTable
            : CrawlerUpsideUpInstructionTable;
        SetCrawlerInstructionFromTable(slot, table);
    }

    private void SetCrawlerHorizontalSurfaceInstruction(
        RoomEnemySlot slot,
        CrawlerEnemyState state)
    {
        int table = (short)state.XVelocity < 0
            ? CrawlerUpsideRightInstructionTable
            : CrawlerUpsideLeftInstructionTable;
        SetCrawlerInstructionFromTable(slot, table);
    }

    private void SetCrawlerInstructionFromTable(RoomEnemySlot slot, int tableAddress)
    {
        // Parameter two is already a byte offset into six-word species tables. Retail
        // Zoomers use zero; preserving byte addressing is required for the shared families.
        if ((slot.Parameter2 & 1) != 0 || slot.Parameter2 > 10)
        {
            throw new InvalidDataException(
                $"Crawler instruction-table offset ${slot.Parameter2:X4} is invalid.");
        }
        slot.CurrentInstruction = ReadWord(_bus!, tableAddress + slot.Parameter2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private int GetCrawlerSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        CrawlerEnemyState state,
        RoomLevelData level) =>
        GetSurfaceSlopeAdjustedHorizontalDisplacement(
            slot,
            state.XVelocity,
            state.YVelocity,
            level);

    /// <summary>
    /// Shared geometry from <c>MoveEnemyRightBy_14_12_ProcessSlopes</c>. Both the common
    /// crawlers and Yard multiply their tangent by the cartridge's non-square-slope factor.
    /// </summary>
    private int GetSurfaceSlopeAdjustedHorizontalDisplacement(
        RoomEnemySlot slot,
        ushort xVelocity,
        ushort yVelocity,
        RoomLevelData level)
    {
        ushort surfaceY = unchecked((short)yVelocity) < 0
            ? unchecked((ushort)(slot.YPosition - slot.YRadius))
            : unchecked((ushort)(slot.YPosition + slot.YRadius - 1));
        int blockX = slot.XPosition >> 4;
        int blockY = surfaceY >> 4;
        if ((uint)blockX < (uint)level.WidthInBlocks &&
            (uint)blockY < (uint)level.HeightInBlocks)
        {
            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
            int slopeShape = block.Behavior & 0x1f;
            if (block.CollisionType == 1 && slopeShape >= 5)
            {
                ushort multiplier = ReadWord(
                    _bus!,
                    CrawlerSlopeSpeedMultiplierTable + slopeShape * 4);
                int speed = unchecked((short)xVelocity);
                int product = Math.Abs(speed) * multiplier;
                return speed < 0 ? -product : product;
            }
        }
        return unchecked((short)xVelocity) << 8;
    }

    private CrawlerEnemyState RequireCrawlerState(RoomEnemySlot slot) =>
        _crawlerStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized crawler state.");

    private static int Shift8AddMagnitude(ushort velocity, int wholePixels)
    {
        int signedVelocity = unchecked((short)velocity);
        int bias = wholePixels << 16;
        return unchecked((signedVelocity << 8) + (signedVelocity < 0 ? -bias : bias));
    }

    private static ushort Negate16(ushort value) => unchecked((ushort)-value);
}
