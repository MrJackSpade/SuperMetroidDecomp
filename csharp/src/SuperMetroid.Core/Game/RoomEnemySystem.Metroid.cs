using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Native dispatch-table indexes stored in ordinary Metroid variable F. These are indexes,
/// not bank-$A3 function addresses: main AI uses the word directly to select one of four
/// movement routines at <c>$A3:EC09</c>.
/// </summary>
public enum MetroidAiFunction : ushort
{
    Homing = 0,
    ClosingOnSamus = 1,
    AttachedToSamus = 2,
    PowerBombEscape = 3,
}

/// <summary>
/// One requested drop from <c>Enemy_ItemDrop_Metroid</c> at <c>$A0:B968</c>. Actual pickup
/// actors remain owned by the shared enemy-projectile pool; retaining all five authored
/// requests makes their source, RNG scatter, and eventual integration explicit.
/// </summary>
public readonly record struct MetroidDropRequest(
    ushort XPosition,
    ushort YPosition,
    ushort EnemyDefinitionPointer,
    ushort SourceSpriteObjectIndex);

/// <summary>
/// Typed view of ordinary Metroid's six common enemy variables plus its three parallel
/// extension words. The two bank-$B4 sprite-object references are the managed equivalent
/// of native indexes <c>metroid_var_00/01</c>; the drain accumulator is <c>metroid_var_02</c>.
/// </summary>
public sealed class MetroidEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal MetroidEnemyState(
        RoomEnemySlot slot,
        RoomSpriteObjectSlot outerBodyA,
        RoomSpriteObjectSlot outerBodyB)
    {
        _slot = slot;
        OuterBodyA = outerBodyA;
        OuterBodyB = outerBodyB;
    }

    public ushort XSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public short XVelocity
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    public ushort YSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public short YVelocity
    {
        get => unchecked((short)_slot.VariableD);
        internal set => _slot.VariableD = unchecked((ushort)value);
    }

    /// <summary>Variable E; four-frame escape timer and frozen-shake countdown.</summary>
    public ushort EscapeTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public MetroidAiFunction Function
    {
        get => (MetroidAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    public RoomSpriteObjectSlot OuterBodyA { get; }
    public RoomSpriteObjectSlot OuterBodyB { get; }

    /// <summary>
    /// Unsigned fractional health-drain accumulator. A borrow from subtracting the suit-
    /// selected rate removes exactly one energy from Samus.
    /// </summary>
    public ushort DrainAccumulator { get; internal set; }

    public int SignedXVelocity => unchecked((XVelocity << 16) | XSubvelocity);
    public int SignedYVelocity => unchecked((YVelocity << 16) | YSubvelocity);
}

/// <summary>
/// Literal translation of ordinary Metroid enemy <c>$DD7F</c>, covering bank-$A3 routines
/// <c>$EA4F-$F06C</c>. This is intentionally distinct from the smaller Mochtroid family:
/// ordinary Metroids have two composited sprite objects, attach to Samus, drain energy,
/// accumulate ice damage in population parameter two, and require missiles after freezing.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort MetroidDefinition = 0xdd7f;

    private const ushort MetroidIdleInstructionList = 0xe9cf;
    private const ushort MetroidAttachedInstructionList = 0xea25;
    private const ushort MetroidOuterBodyAFrozenInstructionList = 0xc3ba;
    private const ushort MetroidOuterBodyBFrozenInstructionList = 0xc4b6;
    private const ushort MetroidFrozenSpritePalette = 0x0c00;
    private const ushort MetroidAttachedSoundEffect = 0x002d;
    private const ushort MetroidIceSoundEffect = 0x000a;
    private const ushort MetroidRecoilSoundEffect = 0x005a;
    private const ushort MetroidAnimationSoundEffect = 0x0050;
    private const ushort MetroidFrozenDuration = 400;
    private const ushort MetroidEscapeDuration = 4;
    private const int MetroidMaximumVelocity = 3;
    private const int MetroidShakeOffsetTable = 0xa3ea3f;
    private const int MetroidRandomSoundTable = 0xa3ead6;
    private const int MetroidSpecialDropCount = 5;

    private readonly MetroidEnemyState?[] _metroidStates =
        new MetroidEnemyState?[MaximumEnemyCount];
    private readonly List<MetroidDropRequest> _metroidDropRequests = new();

    /// <summary>Typed state for each physical enemy slot currently owned by a Metroid.</summary>
    public IReadOnlyList<MetroidEnemyState?> MetroidStates => _metroidStates;

    /// <summary>Five ROM-authored scatter requests emitted by the most recent special death.</summary>
    public IReadOnlyList<MetroidDropRequest> MetroidDropRequests => _metroidDropRequests;

    /// <summary>Most recent library-two Metroid sound requested during this enemy frame.</summary>
    public ushort? LastMetroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Most recent library-three Metroid sound requested during this enemy frame.</summary>
    public ushort? LastMetroidSoundEffectLibrary3 { get; private set; }

    /// <summary>Ports <c>Metroid_Init</c> at <c>$A3:EA4F</c>.</summary>
    private void InitializeMetroid(RoomEnemySlot slot)
    {
        ushort graphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));
        RoomSpriteObjectSlot outerBodyA = SpawnRoomSpriteObject(
            slot.XPosition,
            slot.YPosition,
            RoomSpriteObjectKind.MetroidOuterBodyA,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");
        RoomSpriteObjectSlot outerBodyB = SpawnRoomSpriteObject(
            slot.XPosition,
            slot.YPosition,
            RoomSpriteObjectKind.MetroidOuterBodyB,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");

        var state = new MetroidEnemyState(slot, outerBodyA, outerBodyB)
        {
            DrainAccumulator = 0,
            Function = MetroidAiFunction.Homing,
        };
        _metroidStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = MetroidIdleInstructionList;
    }

    /// <summary>Ports <c>Metroid_Main</c> and its four-entry movement dispatch.</summary>
    private void RunMetroidMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Metroid movement requires room level data.");

        MetroidEnemyState state = RequireMetroidState(slot);
        ushort targetY = unchecked((ushort)(samus.YPosition - 8));
        switch (state.Function)
        {
            case MetroidAiFunction.Homing:
                RunMetroidHoming(slot, state, samus, targetY, level);
                break;
            case MetroidAiFunction.ClosingOnSamus:
                RunMetroidClosing(slot, state, samus, targetY, level);
                break;
            case MetroidAiFunction.AttachedToSamus:
                RunMetroidAttached(slot, state, samus, targetY);
                break;
            case MetroidAiFunction.PowerBombEscape:
                RunMetroidPowerBombEscape(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Metroid movement index {(ushort)state.Function} exceeds its four-entry ROM table.");
        }

        // The ordinary body supplies the central map. Both outer bank-$B4 objects are
        // repositioned and re-enabled every non-frozen main call, exactly after movement.
        ushort graphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));
        PositionMetroidOuterBody(state.OuterBodyA, slot, graphicsIndex);
        PositionMetroidOuterBody(state.OuterBodyB, slot, graphicsIndex);
    }

    /// <summary>Ports proportional 16.16 attraction in <c>Metroid_Func_1</c>.</summary>
    private void RunMetroidHoming(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY,
        RoomLevelData level)
    {
        int yDifference = unchecked((short)(slot.YPosition - targetY));
        int yVelocity = unchecked(state.SignedYVelocity - ((yDifference >> 2) << 8));
        yVelocity = ClampMetroidVelocity(yVelocity);
        StoreMetroidYVelocity(state, yVelocity);
        if (MoveEnemyVertically(level, slot, yVelocity))
            StoreMetroidYVelocity(state, 0);

        int xDifference = unchecked((short)(slot.XPosition - samus.XPosition));
        int xVelocity = unchecked(state.SignedXVelocity - ((xDifference >> 2) << 8));
        xVelocity = ClampMetroidVelocity(xVelocity);
        StoreMetroidXVelocity(state, xVelocity);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity))
            StoreMetroidXVelocity(state, 0);
    }

    /// <summary>Ports the byte-oriented fixed-speed seek in <c>Metroid_Func_2</c>.</summary>
    private void RunMetroidClosing(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY,
        RoomLevelData level)
    {
        // The 65816 constructs a temporary word from the coordinate low-byte subtraction,
        // shifts it right three, sign-extends bit twelve, then clamps the whole result.
        // This is observably not a clean full-word delta: crossing a 256-pixel boundary can
        // reverse the short route. Preserve the byte arithmetic instead of “fixing” it.
        int yVelocity = CalculateMetroidClosingVelocity(targetY, slot.YPosition);
        StoreMetroidYVelocity(state, yVelocity << 16);
        if (MoveEnemyVertically(level, slot, yVelocity << 16))
            StoreMetroidYVelocity(state, 0);

        int xVelocity = CalculateMetroidClosingVelocity(samus.XPosition, slot.XPosition);
        StoreMetroidXVelocity(state, xVelocity << 16);
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, xVelocity << 16))
            StoreMetroidXVelocity(state, 0);
    }

    private static int CalculateMetroidClosingVelocity(ushort target, ushort current)
    {
        int lowByteDifference = unchecked((byte)(target - (byte)current));
        int shifted = lowByteDifference << 5;
        if ((shifted & 0x1000) != 0)
            shifted |= unchecked((int)0xffffe000);
        return Math.Clamp(
            (int)unchecked((short)shifted),
            -MetroidMaximumVelocity,
            MetroidMaximumVelocity);
    }

    /// <summary>Ports <c>Metroid_Func_3</c>; attachment is a hard positional lock.</summary>
    private static void RunMetroidAttached(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState samus,
        ushort targetY)
    {
        slot.XPosition = samus.XPosition;
        slot.YPosition = targetY;
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);
    }

    /// <summary>Ports <c>Metroid_Func_4</c>, including post-decrement timer semantics.</summary>
    private void RunMetroidPowerBombEscape(RoomEnemySlot slot, MetroidEnemyState state)
    {
        int tableIndex = state.EscapeTimer & 3;
        slot.XPosition = unchecked((ushort)(slot.XPosition + ReadWord(
            _bus!,
            MetroidShakeOffsetTable + tableIndex * 2)));
        slot.YPosition = unchecked((ushort)(slot.YPosition + ReadWord(
            _bus!,
            MetroidShakeOffsetTable + (tableIndex + 4) * 2)));
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);

        ushort oldTimer = state.EscapeTimer;
        state.EscapeTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer != 1)
            return;

        state.Function = MetroidAiFunction.Homing;
        SetMetroidInstructionList(slot, MetroidIdleInstructionList);
    }

    /// <summary>Ports the family-specific tail of <c>Metroid_Frozen</c>.</summary>
    private void RunMetroidFrozen(RoomEnemySlot slot)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        if (state.EscapeTimer != 0)
        {
            state.EscapeTimer = unchecked((ushort)(state.EscapeTimer - 1));
            slot.FlashTimer = 2;
        }

        FreezeMetroidOuterBody(state.OuterBodyA, MetroidOuterBodyAFrozenInstructionList);
        FreezeMetroidOuterBody(state.OuterBodyB, MetroidOuterBodyBFrozenInstructionList);
    }

    /// <summary>Ports the palette-bit modulation in custom hurt AI <c>$A3:EB33</c>.</summary>
    private void ApplyMetroidHurt(RoomEnemySlot slot)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        if ((slot.FlashTimer & 2) != 0)
        {
            state.OuterBodyA.GraphicsIndex =
                new SnesObjAttributeWord(state.OuterBodyA.GraphicsIndex)
                    .WithPaletteBits(new SnesObjAttributeWord(slot.PaletteIndex).PaletteBits);
            state.OuterBodyB.GraphicsIndex =
                new SnesObjAttributeWord(state.OuterBodyB.GraphicsIndex)
                    .WithPaletteBits(new SnesObjAttributeWord(slot.PaletteIndex).PaletteBits);
            return;
        }

        state.OuterBodyA.GraphicsIndex =
            new SnesObjAttributeWord(state.OuterBodyA.GraphicsIndex).WithPaletteIndex(0);
        state.OuterBodyB.GraphicsIndex =
            new SnesObjAttributeWord(state.OuterBodyB.GraphicsIndex).WithPaletteIndex(0);
    }

    /// <summary>Ports ordinary/contact-damage branches of <c>Metroid_Touch</c>.</summary>
    private void ResolveMetroidTouch(RoomEnemySlot slot, SamusState samus)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        ushort targetY = unchecked((ushort)(samus.YPosition - 8));
        if (samus.HorizontalSpeed.ContactDamageIndex != 0)
        {
            if (state.Function != MetroidAiFunction.AttachedToSamus)
            {
                // Native overlapping byte writes produce delta/4 in signed 16.16 form.
                StoreMetroidXVelocity(
                    state,
                    unchecked((short)(slot.XPosition - samus.XPosition)) << 14);
                StoreMetroidYVelocity(
                    state,
                    unchecked((short)(slot.YPosition - targetY)) << 14);
                state.Function = MetroidAiFunction.Homing;
                SetMetroidInstructionList(slot, MetroidIdleInstructionList);
            }
            return;
        }

        if (state.Function != MetroidAiFunction.PowerBombEscape)
        {
            if ((_randomEnemyCounter & 7) == 7 && unchecked((short)(samus.Health - 30)) >= 0)
                LastMetroidSoundEffectLibrary3 = MetroidAttachedSoundEffect;
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Function >= MetroidAiFunction.AttachedToSamus)
            return;

        state.Function = MetroidAiFunction.ClosingOnSamus;
        if (Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) >= 8 ||
            Math.Abs(unchecked((short)(slot.YPosition - targetY))) >= 8)
        {
            return;
        }

        state.Function = MetroidAiFunction.AttachedToSamus;
        samus.SpecialSuperPaletteFlags = 1;
        SetMetroidInstructionList(slot, MetroidAttachedInstructionList);
    }

    private static void DrainSamusWithMetroid(MetroidEnemyState state, SamusState samus)
    {
        ushort drainRate = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? (ushort)0x3000
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? (ushort)0x6000
                : (ushort)0xc000;
        ushort oldAccumulator = state.DrainAccumulator;
        state.DrainAccumulator = unchecked((ushort)(oldAccumulator - drainRate));
        if (oldAccumulator >= drainRate)
            return;

        // Samus_DealDamage(1) performs a saturating subtract and deliberately does not
        // start ordinary hurt flash, invincibility, knockback, or suit division.
        samus.Health = samus.Health == 0 ? (ushort)0 : unchecked((ushort)(samus.Health - 1));
    }

    /// <summary>Handles non-frozen recoil/ice accumulation after bank-$A0 accepts a shot.</summary>
    private void ResolveMetroidNonFrozenShot(
        RoomEnemySlot slot,
        ushort collisionProjectileType,
        ushort collisionProjectileDamage,
        ushort recoilOriginX,
        ushort recoilOriginY,
        SamusState? samus)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        SamusProjectileFamily family = new SamusProjectileTypeWord(collisionProjectileType).Family;
        if (state.Function == MetroidAiFunction.AttachedToSamus)
        {
            // The bomb collision pass presents the exploding power bomb as family $0500;
            // family $0300 is its earlier expansion/controller record and is not the value
            // tested by Metroid_Shot at $A3:EF38.
            if (family == SamusProjectileFamily.Bomb)
                DetachMetroidWithPowerBomb(slot, state, samus);
            return;
        }

        // Metroid_Shot intentionally reads projectile position slot zero, not the current
        // collision slot. The overlapping byte stores encode delta/8 as signed 16.16.
        StoreMetroidXVelocity(
            state,
            unchecked((short)(slot.XPosition - recoilOriginX)) << 13);
        StoreMetroidYVelocity(
            state,
            unchecked((short)(slot.YPosition - recoilOriginY)) << 13);
        state.Function = MetroidAiFunction.Homing;
        SetMetroidInstructionList(slot, MetroidIdleInstructionList);

        if ((collisionProjectileType & 0x0002) != 0)
        {
            LastMetroidSoundEffectLibrary3 = MetroidIceSoundEffect;
            slot.FlashTimer = 4;
            if (collisionProjectileDamage >= slot.Parameter2)
            {
                slot.Parameter2 = 0;
                slot.FrozenTimer = MetroidFrozenDuration;
                slot.AiHandlerBits |= 0x0004;
                return;
            }
            slot.Parameter2 = unchecked((ushort)(slot.Parameter2 - collisionProjectileDamage));
        }

        LastMetroidSoundEffectLibrary2 = MetroidRecoilSoundEffect;
    }

    private static void DetachMetroidWithPowerBomb(
        RoomEnemySlot slot,
        MetroidEnemyState state,
        SamusState? samus)
    {
        state.EscapeTimer = MetroidEscapeDuration;
        state.Function = MetroidAiFunction.PowerBombEscape;
        SetMetroidInstructionList(slot, MetroidIdleInstructionList);
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    private void FinishMetroidDeath(RoomEnemySlot slot, SamusState? samus, bool requestDrops)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        state.XVelocity = 0;
        state.OuterBodyA.InstructionPointer = 0;
        state.OuterBodyB.InstructionPointer = 0;
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;

        if (requestDrops)
            RequestMetroidDrops(slot, state);
    }

    private void RequestMetroidDrops(RoomEnemySlot slot, MetroidEnemyState state)
    {
        for (int dropIndex = 0; dropIndex < MetroidSpecialDropCount; dropIndex++)
        {
            ushort random = _nextRandom!();
            ushort x = unchecked((ushort)(slot.XPosition + (random & 0x001f) - 16));
            ushort y = unchecked((ushort)(slot.YPosition + ((random & 0x1f00) >> 8) - 16));
            _metroidDropRequests.Add(new MetroidDropRequest(
                x,
                y,
                MetroidDefinition,
                state.OuterBodyB.NativeIndex));
            SpawnEnemyDropFromEnemyHeader(x, y, MetroidDefinition);
        }
    }

    private static int ClampMetroidVelocity(int velocity)
    {
        short whole = unchecked((short)(velocity >> 16));
        if (whole >= MetroidMaximumVelocity)
            return MetroidMaximumVelocity << 16;
        if (whole < -MetroidMaximumVelocity)
            return -MetroidMaximumVelocity << 16;
        return velocity;
    }

    private static void StoreMetroidXVelocity(MetroidEnemyState state, int velocity)
    {
        state.XVelocity = unchecked((short)(velocity >> 16));
        state.XSubvelocity = unchecked((ushort)velocity);
    }

    private static void StoreMetroidYVelocity(MetroidEnemyState state, int velocity)
    {
        state.YVelocity = unchecked((short)(velocity >> 16));
        state.YSubvelocity = unchecked((ushort)velocity);
    }

    private static void PositionMetroidOuterBody(
        RoomSpriteObjectSlot outerBody,
        RoomEnemySlot metroid,
        ushort graphicsIndex)
    {
        outerBody.XPosition = metroid.XPosition;
        outerBody.YPosition = metroid.YPosition;
        outerBody.GraphicsIndex = graphicsIndex;
        outerBody.DisableFlags = 0;
    }

    private static void FreezeMetroidOuterBody(
        RoomSpriteObjectSlot outerBody,
        ushort frozenInstructionList)
    {
        outerBody.GraphicsIndex = MetroidFrozenSpritePalette;
        outerBody.DisableFlags = 1;
        outerBody.InstructionPointer = frozenInstructionList;
    }

    private static void SetMetroidInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
    }

    private MetroidEnemyState RequireMetroidState(RoomEnemySlot slot) =>
        _metroidStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Metroid state.");
}
