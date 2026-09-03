using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-facing projection of Fake Kraid's ordinary enemy words and its deliberately
/// aliased bank-$7E:7800 scratch area. “Mini-Kraid” is the name used by the item-drop and
/// projectile routines; both names refer to definition <c>$E0FF</c>.
/// </summary>
public sealed class FakeKraidEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal FakeKraidEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Signed whole-pixel walk delta in native variable B.</summary>
    public short WalkDelta
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>Signed four-pixel facing marker in native variable C.</summary>
    public short FacingDelta
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    /// <summary>Frames/steps remaining before the next forced walk reversal.</summary>
    public ushort WalkStepTimer
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Number of animation decision points before the next spit attack.</summary>
    public ushort SpitDecisionTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>
    /// Three independent spike clocks stored at scratch words three through five. Main AI
    /// visits exactly one clock per frame in the native order 0, 1, 2, 0...
    /// </summary>
    public ushort[] SpikeTimers { get; } = new ushort[3];

    /// <summary>Byte offset zero/two/four selecting the spike clock visited this frame.</summary>
    public ushort SpikeTimerByteOffset { get; internal set; }

    /// <summary>Most recently selected spike row, or <c>null</c> before the first spawn.</summary>
    public int? LastSpikeRow { get; internal set; }

    /// <summary>Number of spike projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpikeCount { get; internal set; }

    /// <summary>Number of spit projectiles successfully allocated by this actor.</summary>
    public int SpawnedSpitCount { get; internal set; }
}

/// <summary>
/// The specialized <c>Enemy_ItemDrop_MiniKraid</c> tail publishes this request after the
/// type-three death explosion. Pickup selection remains owned by the common drop subsystem;
/// retaining the exact origin and table pointer prevents the enemy-specific event from being
/// silently lost while that shared system is translated.
/// </summary>
public readonly record struct FakeKraidDropRequest(
    ushort X,
    ushort Y,
    ushort ItemDropChancesPointer,
    ushort DeathExplosionVariant);

/// <summary>
/// Literal translation of Fake Kraid/Mini-Kraid definition <c>$E0FF</c>, its five bank-$A6
/// animation opcodes, and the three bank-$86 spit/spike projectile definitions it owns.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort FakeKraidDefinition = 0xe0ff;

    private const ushort FakeKraidInitialLeftInstruction = 0x99ae;
    private const ushort FakeKraidInitialRightInstruction = 0x99fc;
    private const ushort FakeKraidLeftWalkInstruction = 0x99ac;
    private const ushort FakeKraidLeftAlternateWalkInstruction = 0x99c4;
    private const ushort FakeKraidLeftSpitInstruction = 0x99dc;
    private const ushort FakeKraidRightWalkInstruction = 0x99fa;
    private const ushort FakeKraidRightAlternateWalkInstruction = 0x9a12;
    private const ushort FakeKraidRightSpitInstruction = 0x9a2a;
    private const ushort FakeKraidSpitSound = 0x0016;
    private const ushort FakeKraidSpikeSound = 0x003f;
    private const int FakeKraidSpitVelocityTable = 0xa69a48;
    private const int FakeKraidSpikeYOffsetTable = 0x869e7d;

    private readonly FakeKraidEnemyState?[] _fakeKraidStates =
        new FakeKraidEnemyState?[MaximumEnemyCount];

    public IReadOnlyList<FakeKraidEnemyState?> FakeKraidStates => _fakeKraidStates;

    /// <summary>Last library-two sound requested by Fake Kraid during this enemy frame.</summary>
    public ushort? LastFakeKraidSoundEffect { get; private set; }

    /// <summary>Last specialized Mini-Kraid death/drop request.</summary>
    public FakeKraidDropRequest? LastFakeKraidDropRequest { get; private set; }

    /// <summary>Ports <c>FakeKraid_Init</c> at <c>$A6:9A58</c>.</summary>
    private void InitializeFakeKraid(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid initialization requires the active Samus position.");
        }

        var state = new FakeKraidEnemyState(slot);
        _fakeKraidStates[slot.SlotIndex] = state;

        // The routine reads the already-published global random word; it does not advance
        // the RNG. Use the read callback when available and reserve NextRandom for older
        // isolated callers that did not expose that native seam.
        ushort random = ReadFakeKraidRandomNumber();
        ushort seedDelay = unchecked((ushort)((random & 3) + 2));
        state.WalkStepTimer = seedDelay;
        state.SpitDecisionTimer = seedDelay;
        state.SpikeTimers[0] = unchecked((ushort)(seedDelay + 64));
        state.SpikeTimers[1] = unchecked((ushort)(seedDelay + 96));
        state.SpikeTimers[2] = unchecked((ushort)(seedDelay + 48));
        state.SpikeTimerByteOffset = 0;

        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.WalkDelta = -4;
        state.FacingDelta = -4;
        slot.CurrentInstruction = FakeKraidInitialLeftInstruction;
        if (unchecked((short)(slot.XPosition - samus.XPosition)) < 0)
        {
            state.WalkDelta = 4;
            state.FacingDelta = 4;
            slot.CurrentInstruction = FakeKraidInitialRightInstruction;
        }
    }

    /// <summary>Ports <c>FakeKraid_Main</c> at <c>$A6:9AC2</c>.</summary>
    private void RunFakeKraidMain(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        // Main AI stores the next selector before acting on the old value. Because the old
        // value is passed as a byte offset into scratch RAM, offsets zero/two/four alias the
        // three adjacent clocks above rather than three enemy slots.
        ushort selectedByteOffset = state.SpikeTimerByteOffset;
        ushort nextByteOffset = unchecked((ushort)(selectedByteOffset + 2));
        if (selectedByteOffset >= 4)
            nextByteOffset = 0;
        state.SpikeTimerByteOffset = nextByteOffset;

        int timerIndex = selectedByteOffset >> 1;
        ushort timer = state.SpikeTimers[timerIndex];
        if (timer != 0)
        {
            state.SpikeTimers[timerIndex] = unchecked((ushort)(timer - 1));
            return;
        }

        state.SpikeTimers[timerIndex] = unchecked((ushort)(
            (ReadFakeKraidRandomNumber() & 0x003f) + 16));
        state.LastSpikeRow = timerIndex;
        if (SpawnFakeKraidSpike(slot, state, timerIndex) &&
            FakeKraidOriginIsOnScreen(slot, cameraX, cameraY))
        {
            LastFakeKraidSoundEffect = FakeKraidSpikeSound;
        }
    }

    /// <summary>Ports animation opcode <c>$A6:9B26</c>.</summary>
    private void ProcessFakeKraidWalkInstruction(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null || level is null)
        {
            throw new InvalidOperationException(
                "Fake Kraid walk bytecode requires Samus and room collision data.");
        }

        if (state.SpitDecisionTimer != 0)
            state.SpitDecisionTimer = unchecked((ushort)(state.SpitDecisionTimer - 1));

        // C's post-decrement comparison is exact here: when the old value is one, reload
        // seven through ten and reverse without moving. Otherwise attempt the signed four-
        // pixel 16.16 displacement and reverse only if the room collision helper reports a
        // wall. A wrapped zero timer therefore still performs the attempted step.
        ushort oldWalkTimer = state.WalkStepTimer;
        state.WalkStepTimer = unchecked((ushort)(oldWalkTimer - 1));
        bool reverse = oldWalkTimer == 1;
        if (reverse)
        {
            state.WalkStepTimer = unchecked((ushort)(
                (ReadFakeKraidRandomNumber() & 3) + 7));
        }
        else
        {
            int displacement = state.WalkDelta << 16;
            reverse = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                displacement);
        }

        if (reverse)
            state.WalkDelta = unchecked((short)-state.WalkDelta);

        state.FacingDelta = unchecked((short)(slot.XPosition - samus.XPosition)) < 0
            ? (short)4
            : (short)-4;
    }

    /// <summary>Ports animation opcode <c>$A6:9B74</c> and returns its direct list target.</summary>
    private ushort SelectFakeKraidInstruction(
        FakeKraidEnemyState state)
    {
        if (state.SpitDecisionTimer != 0)
        {
            if (state.FacingDelta < 0)
            {
                return state.WalkDelta >= 0
                    ? unchecked((ushort)(FakeKraidLeftAlternateWalkInstruction + 2))
                    : unchecked((ushort)(FakeKraidLeftWalkInstruction + 2));
            }

            return state.WalkDelta < 0
                ? unchecked((ushort)(FakeKraidRightAlternateWalkInstruction + 2))
                : unchecked((ushort)(FakeKraidRightWalkInstruction + 2));
        }

        state.SpitDecisionTimer = unchecked((ushort)(
            (ReadFakeKraidRandomNumber() & 3) + 3));
        return state.FacingDelta < 0
            ? FakeKraidLeftSpitInstruction
            : FakeKraidRightSpitInstruction;
    }

    /// <summary>Ports animation opcodes <c>$A6:9BC4</c>/<c>$A6:9C02</c>.</summary>
    private void SpawnFakeKraidSpitPair(
        RoomEnemySlot slot,
        FakeKraidEnemyState state,
        bool movingRight)
    {
        int tableOffset = movingRight ? 8 : 0;
        short xOffset = movingRight ? (short)4 : (short)-4;
        for (int projectile = 0; projectile < 2; projectile++)
        {
            ushort xVelocity = ReadWord(
                _bus!,
                FakeKraidSpitVelocityTable + tableOffset + projectile * 4);
            ushort yVelocity = ReadWord(
                _bus!,
                FakeKraidSpitVelocityTable + tableOffset + projectile * 4 + 2);
            if (SpawnFakeKraidSpit(slot, xOffset, xVelocity, yVelocity))
                state.SpawnedSpitCount++;
        }
    }

    private bool SpawnFakeKraidSpit(
        RoomEnemySlot source,
        short xOffset,
        ushort xVelocity,
        ushort yVelocity)
    {
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.FakeKraidSpit,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        projectile.XPosition = unchecked((ushort)(source.XPosition + xOffset));
        projectile.YPosition = unchecked((ushort)(source.YPosition - 16));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.XVelocity = xVelocity;
        projectile.YVelocity = yVelocity;
        return true;
    }

    private bool SpawnFakeKraidSpike(
        RoomEnemySlot source,
        FakeKraidEnemyState state,
        int row)
    {
        RoomEnemyProjectileKind kind = state.FacingDelta < 0
            ? RoomEnemyProjectileKind.FakeKraidSpikeLeft
            : RoomEnemyProjectileKind.FakeKraidSpikeRight;
        RoomEnemyProjectileSlot? projectile = AllocateEnemyProjectile();
        if (projectile is null)
            return false;

        InitializeEnemyProjectileFromDefinition(
            projectile,
            kind,
            unchecked((ushort)(source.VramTilesIndex | source.PaletteIndex)));
        short yOffset = unchecked((short)ReadWord(
            _bus!,
            FakeKraidSpikeYOffsetTable + row * 2));
        projectile.XPosition = source.XPosition;
        projectile.YPosition = unchecked((ushort)(source.YPosition + yOffset));
        projectile.XSubposition = 0;
        projectile.YSubposition = 0;
        projectile.XVelocity = kind == RoomEnemyProjectileKind.FakeKraidSpikeLeft
            ? unchecked((ushort)-0x0200)
            : (ushort)0x0200;
        projectile.YVelocity = 0;
        state.SpawnedSpikeCount++;
        return true;
    }

    /// <summary>Ports <c>EprojPreInit_MiniKraidSpit</c> at <c>$86:9E1E</c>.</summary>
    private static void RunFakeKraidSpitPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true) ||
            MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
            return;
        }

        short oldVelocity = unchecked((short)projectile.YVelocity);
        short nextVelocity = unchecked((short)(oldVelocity + 64));
        if (nextVelocity >= 0 && oldVelocity >= 960)
            nextVelocity = 1024;
        projectile.YVelocity = unchecked((ushort)nextVelocity);
    }

    /// <summary>Ports <c>EprojPreInstr_MiniKraidSpikes</c> at <c>$86:9E83</c>.</summary>
    private static void RunFakeKraidSpikePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (MoveProjectileAxis(projectile, level, horizontal: true))
            projectile.Clear();
    }

    private ushort ReadFakeKraidRandomNumber() =>
        RequireRandomNumber();

    private static bool FakeKraidOriginIsOnScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        unchecked((short)(slot.XPosition - cameraX)) >= 0 &&
        unchecked((short)(cameraX + 256 - slot.XPosition)) >= 0 &&
        unchecked((short)(slot.YPosition - cameraY)) >= 0 &&
        unchecked((short)(cameraY + 256 - slot.YPosition)) >= 0;

    private FakeKraidEnemyState RequireFakeKraidState(RoomEnemySlot slot) =>
        _fakeKraidStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fake Kraid state.");

    private void RequestFakeKraidDeathDrop(RoomEnemySlot slot)
    {
        ushort originX = slot.XPosition;
        ushort originY = slot.YPosition;
        ushort chancePointer = slot.Definition.ItemDropChancesPointer;
        LastFakeKraidDropRequest = new FakeKraidDropRequest(
            originX,
            originY,
            chancePointer,
            DeathExplosionVariant: 3);
        SpawnEnemyDropScatterAround(
            FakeKraidDefinition,
            count: 4,
            originX,
            originY);
    }
}
