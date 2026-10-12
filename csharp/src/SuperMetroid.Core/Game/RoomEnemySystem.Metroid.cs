using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
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
    /// <summary>Index 0 selects $A3:EC11, Function_Metroid_0_ChaseSamus: proportional 16.16 attraction toward Samus's X and eight pixels above her Y, capped at three pixels per update.</summary>
    Homing = 0,
    /// <summary>Index 1 selects $A3:ECDC, Function_Metroid_1_LatchOntoSamus: byte-oriented close-range seeking entered on touch before the attachment threshold is reached.</summary>
    ClosingOnSamus = 1,
    /// <summary>Index 2 selects $A3:ED8F, Function_Metroid_2_LatchedOntoSamus: hard-lock position to Samus's X and Y minus eight pixels, clearing movement velocities.</summary>
    AttachedToSamus = 2,
    /// <summary>Index 3 selects $A3:EDAB, Function_Metroid_3_BombedOffSamus: perform four displacement steps after bomb collision detaches the Metroid, then resume homing.</summary>
    PowerBombEscape = 3,
}

/// <summary>
/// One requested drop from <c>Enemy_ItemDrop_Metroid</c> at <c>$A0:B968</c>. Actual pickup
/// actors are spawned into the shared enemy-projectile pool. This empty marker counts
/// each of the five requests; it does not retain the pickup's RNG-scattered coordinates.
/// </summary>
public readonly record struct MetroidDropRequest();

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

    /// <summary>
    /// Signed 16.16 velocity in pixels per movement update, positive right and down: variables
    /// B:A (native $0FAA:$0FA8) hold the horizontal whole:fraction words and D:C ($0FAE:$0FAC)
    /// the vertical ones.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableB, _slot.VariableA, _slot.VariableD, _slot.VariableC);
        internal set
        {
            _slot.VariableA = value.X.Fraction;
            _slot.VariableB = value.X.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.Y.WholeWord;
        }
    }

    /// <summary>Variable E; four-frame escape timer and frozen-shake countdown.</summary>
    public ushort EscapeTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Variable F at native $0FB2: zero-based movement dispatcher index 0..3 into $A3:EC09, not an AI routine pointer.</summary>
    public MetroidAiFunction Function
    {
        get => (MetroidAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Live shared-pool electricity sprite object, native selector $32 and reference $7E:7800; the Metroid owner repositions, freezes and terminates it alongside the central body.</summary>
    public RoomSpriteObjectSlot OuterBodyA { get; }
    /// <summary>Live shared-pool shell sprite object, native selector $34 and reference $7E:7802; distinct from both the electricity layer and central enemy spritemap.</summary>
    public RoomSpriteObjectSlot OuterBodyB { get; }

    /// <summary>
    /// Unsigned fractional health-drain accumulator. A borrow from subtracting the suit-
    /// selected rate removes exactly one energy from Samus.
    /// </summary>
    public ushort DrainAccumulator { get; internal set; }
}

/// <summary>
/// Literal translation of ordinary Metroid enemy <c>$DD7F</c>, covering bank-$A3 routines
/// <c>$EA4F-$F06C</c>. This is intentionally distinct from the smaller Mochtroid family:
/// ordinary Metroids have two composited sprite objects, attach to Samus, drain energy,
/// accumulate ice damage in population parameter two, and require missiles after freezing.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Executes Metroid's private animation instructions.</summary>
    private bool TryProcessMetroidInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Metroid ||
            !Enum.IsDefined((MetroidInstruction)word))
            return false;

        switch ((MetroidInstruction)word)
        {
            case MetroidInstruction.PlayDrainingSamusSFX:
                // The attached loop emits library-two sound $50 without consuming
                // an operand; animation parsing resumes at the following word.
                LastMetroidSoundEffectLibrary2 = MetroidAnimationSoundEffect;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MetroidInstruction.PlayRandomMetroidSFX:
                // GenerateRandomNumber advances exactly once and the low three result
                // bits choose one of the eight cartridge-defined idle cries.
                LastMetroidSoundEffectLibrary2 =
                    MetroidBehaviorDefinitions.RandomCrySoundEffect(EnemyRandom.Generate());
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Metroid does not own instruction ${word:X4}.");
        }
    }


    private const ushort MetroidOuterBodyAFrozenInstructionList = 0xc3ba;
    private const ushort MetroidOuterBodyBFrozenInstructionList = 0xc4b6;
    private const ushort MetroidFrozenSpritePalette = 0x0c00;
    private const ushort MetroidIceSoundEffect = 0x000a;
    private const ushort MetroidRecoilSoundEffect = 0x005a;
    private const ushort MetroidAnimationSoundEffect = 0x0050;
    private const ushort MetroidFrozenDuration = 400;
    private const ushort MetroidEscapeDuration = 4;
    private const int MetroidMaximumVelocity = 3;
    private const int MetroidSpecialDropCount = 5;

    private readonly MetroidEnemyState?[] _metroidStates =
        new MetroidEnemyState?[MaximumEnemyCount];
    private readonly List<MetroidDropRequest> _metroidDropRequests = new();

    /// <summary>Most recent library-two Metroid sound requested during this enemy frame.</summary>
    public ushort? LastMetroidSoundEffectLibrary2 { get; private set; }

    /// <summary>Most recent library-three Metroid sound requested during this enemy frame.</summary>
    public ushort? LastMetroidSoundEffectLibrary3 { get; private set; }

    /// <summary>Ports <c>Metroid_Init</c> at <c>$A3:EA4F</c>.</summary>
    private void InitializeMetroid(RoomEnemySlot slot)
    {
        ushort graphicsIndex = unchecked((ushort)(slot.VramTilesIndex | slot.PaletteIndex));
        RoomSpriteObjectSlot outerBodyA = SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyA,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");
        RoomSpriteObjectSlot outerBodyB = SpawnRoomSpriteObject(
            slot.Position.Whole,
            RoomSpriteObjectKind.MetroidOuterBodyB,
            graphicsIndex) ?? throw new InvalidOperationException(
                "Metroid initialization exhausted the retail 32-slot sprite-object pool.");

        var state = new MetroidEnemyState(slot, outerBodyA, outerBodyB)
        {
            DrainAccumulator = 0,
            Function = MetroidAiFunction.Homing,
        };
        _metroidStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = MetroidInstructionProgramDefinitions.ChasingSamus;
    }

    /// <summary>Ports <c>Metroid_Main</c> and its four-entry movement dispatch.</summary>
    private void RunMetroidMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metroid AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Metroid movement requires room level data.");

        MetroidEnemyState state = RequireMetroidState(slot);
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole - 8));
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
        int yDifference = unchecked((short)(slot.Position.Y.Whole - targetY));
        int yVelocity = unchecked(state.Velocity.Y.RawValue - SnesEightEight.FromWhole(yDifference >> 2));
        yVelocity = ClampMetroidVelocity(yVelocity);
        StoreMetroidYVelocity(state, yVelocity);
        if (MoveEnemyVertically(level, slot, yVelocity))
            StoreMetroidYVelocity(state, 0);

        int xDifference = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole));
        int xVelocity = unchecked(state.Velocity.X.RawValue - SnesEightEight.FromWhole(xDifference >> 2));
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
        int yVelocity = CalculateMetroidClosingVelocity(targetY, slot.Position.Y.Whole);
        StoreMetroidYVelocity(state, SnesSixteenSixteen.FromWhole(yVelocity));
        if (MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(yVelocity)))
            StoreMetroidYVelocity(state, 0);

        int xVelocity = CalculateMetroidClosingVelocity(samus.Position.X.Whole, slot.Position.X.Whole);
        StoreMetroidXVelocity(state, SnesSixteenSixteen.FromWhole(xVelocity));
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, SnesSixteenSixteen.FromWhole(xVelocity)))
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
        slot.Position = slot.Position.WithWhole(new(samus.Position.X.Whole, targetY));
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);
    }

    /// <summary>Ports <c>Metroid_Func_4</c>, including post-decrement timer semantics.</summary>
    private static void RunMetroidPowerBombEscape(RoomEnemySlot slot, MetroidEnemyState state)
    {
        slot.Position += MetroidBehaviorDefinitions.EscapeDisplacement(state.EscapeTimer);
        StoreMetroidXVelocity(state, 0);
        StoreMetroidYVelocity(state, 0);

        ushort oldTimer = state.EscapeTimer;
        state.EscapeTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer != 1)
            return;

        state.Function = MetroidAiFunction.Homing;
        SetMetroidInstructionList(slot, MetroidInstructionProgramDefinitions.ChasingSamus);
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
        ushort targetY = unchecked((ushort)(samus.Position.Y.Whole - 8));
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            if (state.Function != MetroidAiFunction.AttachedToSamus)
            {
                // Native overlapping byte writes produce delta/4 in signed 16.16 form.
                StoreMetroidXVelocity(
                    state,
                    unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) << 14);
                StoreMetroidYVelocity(
                    state,
                    unchecked((short)(slot.Position.Y.Whole - targetY)) << 14);
                state.Function = MetroidAiFunction.Homing;
                SetMetroidInstructionList(
                    slot,
                    MetroidInstructionProgramDefinitions.ChasingSamus);
            }
            return;
        }

        if (state.Function != MetroidAiFunction.PowerBombEscape)
        {
            if ((_randomEnemyCounter & 7) == 7 && unchecked((short)(samus.Health - 30)) >= 0)
                QueueEnemySound(SoundEffectLibrary3Sounds.AttachedEnemyDrain, maximumQueued: 6);
            DrainSamusWithMetroid(state, samus);
        }

        if (state.Function >= MetroidAiFunction.AttachedToSamus)
            return;

        state.Function = MetroidAiFunction.ClosingOnSamus;
        if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) >= 8 ||
            Math.Abs(unchecked((short)(slot.Position.Y.Whole - targetY))) >= 8)
        {
            return;
        }

        state.Function = MetroidAiFunction.AttachedToSamus;
        samus.SpecialSuperPaletteFlags = 1;
        SetMetroidInstructionList(slot, MetroidInstructionProgramDefinitions.DrainingSamus);
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
        Position16<RoomPixels> recoilOrigin,
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
            unchecked((short)(slot.Position.X.Whole - recoilOrigin.X)) << 13);
        StoreMetroidYVelocity(
            state,
            unchecked((short)(slot.Position.Y.Whole - recoilOrigin.Y)) << 13);
        state.Function = MetroidAiFunction.Homing;
        SetMetroidInstructionList(slot, MetroidInstructionProgramDefinitions.ChasingSamus);

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
        SetMetroidInstructionList(slot, MetroidInstructionProgramDefinitions.ChasingSamus);
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;
    }

    /// <summary>Kills both composited outer layers and clears Samus's Metroid flash state.</summary>
    private void FinishMetroidDeath(RoomEnemySlot slot, SamusState? samus)
    {
        MetroidEnemyState state = RequireMetroidState(slot);
        state.Velocity = state.Velocity.WithWholeX(0);
        state.OuterBodyA.InstructionPointer = 0;
        state.OuterBodyB.InstructionPointer = 0;
        if (samus is not null)
            samus.SpecialSuperPaletteFlags = 0;
    }

    /// <summary>
    /// <c>MetroidDeathItemDropRoutine</c> ($A0:B968) around the origin $A3:EF31/EF38
    /// recorded before <c>EnemyDeath</c> cleared the slot.
    /// </summary>
    private void RequestMetroidDrops(Position16<RoomPixels> origin)
    {
        for (int dropIndex = 0; dropIndex < MetroidSpecialDropCount; dropIndex++)
        {
            ushort random = EnemyRandom.Generate();
            // Each axis adds a five-bit random byte minus sixteen: -16..+15 pixels.
            var scatter = new Offset16<RoomPixels>(
                unchecked((short)((random & 0x001f) - 16)),
                unchecked((short)(((SnesLittleEndian.HighByte(random) & 0x1f)) - 16)));
            _metroidDropRequests.Add(new MetroidDropRequest());
            SpawnEnemyDropFromEnemyHeader(origin + scatter, EnemyDefinitionId.Metroid);
        }
    }

    private static int ClampMetroidVelocity(int velocity)
    {
        short whole = unchecked((short)(SnesSixteenSixteen.WholeOf(velocity)));
        if (whole >= MetroidMaximumVelocity)
            return SnesSixteenSixteen.FromWhole(MetroidMaximumVelocity);
        if (whole < -MetroidMaximumVelocity)
            return SnesSixteenSixteen.FromWhole(-MetroidMaximumVelocity);
        return velocity;
    }

    private static void StoreMetroidXVelocity(MetroidEnemyState state, int velocity) =>
        state.Velocity = state.Velocity.WithX(SnesSignedSixteenSixteen.FromRaw(velocity));

    private static void StoreMetroidYVelocity(MetroidEnemyState state, int velocity) =>
        state.Velocity = state.Velocity.WithY(SnesSignedSixteenSixteen.FromRaw(velocity));

    private static void PositionMetroidOuterBody(
        RoomSpriteObjectSlot outerBody,
        RoomEnemySlot metroid,
        ushort graphicsIndex)
    {
        outerBody.Position = outerBody.Position.WithWhole(metroid.Position.Whole);
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
        // Native drawing reads the selected record even while animation is disabled.
        // Refresh the cached visual without resetting the retained instruction timer.
        outerBody.SpritemapPointer = RoomSpriteObjectVisualDefinitions.FrameAt(
            unchecked((ushort)(frozenInstructionList + 2)));
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
