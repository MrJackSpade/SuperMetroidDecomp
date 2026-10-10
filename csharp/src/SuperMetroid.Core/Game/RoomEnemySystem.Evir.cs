namespace SuperMetroid.Core.Game;

/// <summary>Native bank-$A8 function words stored by the Evir composite actors.</summary>
public enum EvirAiFunction : ushort
{
    /// <summary>$A8:8922, <c>Function_Evir_HandleBodyArms</c>: bobs the body vertically or positions its following arms record.</summary>
    HandleBodyOrArms = 0x8922,
    /// <summary>$A8:8A34, <c>Function_EvirProjectile_Idle</c>: keeps the reusable projectile at the body's mouth until a launch is selected.</summary>
    ProjectileIdle = 0x8a34,
    /// <summary>$A8:8A3B, <c>Function_EvirProjectile_Moving</c>: checks for regeneration beyond the viewport, then advances the aimed projectile's fixed-point position.</summary>
    ProjectileMoving = 0x8a3b,
    /// <summary>$A8:8A78, <c>Function_EvirProjectile_Regenerating</c>: anchors the growing projectile to the mouth plus its animation-controlled X offset; a frozen body holds this state.</summary>
    ProjectileRegenerating = 0x8a78,
}

/// <summary>
/// Typed view of the common and extended enemy RAM used by Evir (also called Mini-Draygon
/// in older source annotations). The cartridge builds each creature from three consecutive
/// physical enemy records: body, arms, and a reusable aimed projectile.
/// </summary>
public sealed class EvirEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal EvirEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Zero for left and one for right; native common variable B.</summary>
    public ushort FacingDirection
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Bank-$A8 indirect function word in native common variable C.</summary>
    public EvirAiFunction Function
    {
        get => (EvirAiFunction)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Body bobbing countdown in native common variable E.</summary>
    public ushort MovementTimer
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Regeneration X offset in native common variable F.</summary>
    public ushort RegenerationXOffset
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    // The remaining words live in the parallel $7E:7800 extended-enemy array. Keeping them
    // on this state object preserves their native ownership without polluting the common
    // 64-byte RoomEnemySlot abstraction with one family's aliases.
    /// <summary>Body bobbing direction: zero selects upward velocity and one downward velocity; toggled when the movement countdown underflows. Native extended offset $00.</summary>
    public ushort MovementDirection { get; internal set; }
    /// <summary>Bank-$A8 base pointer of the last installed animation list, not its advancing instruction cursor; zero forces installation of the requested list. Native extended offset $02.</summary>
    public ushort InstalledInstructionList { get; internal set; }
    /// <summary>Bank-$A8 animation list requested by the actor's current facing or projectile phase; a changed request restarts the instruction timer at one. Native extended offset $04.</summary>
    public ushort RequestedInstructionList { get; internal set; }
    /// <summary>Signed whole-pixel component of the body's downward Y velocity per AI update, paired with <see cref="DownSubvelocity"/> as signed 16.16. Native extended offset $08.</summary>
    public short DownVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the body's downward Y velocity, in 1/65536 pixel per AI update. Native extended offset $06.</summary>
    public ushort DownSubvelocity { get; internal set; }
    /// <summary>Signed whole-pixel component of the body's upward Y velocity per AI update, paired with <see cref="UpSubvelocity"/> as signed 16.16. Native extended offset $0C.</summary>
    public short UpVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the body's upward Y velocity, in 1/65536 pixel per AI update; the whole component carries the negative sign. Native extended offset $0A.</summary>
    public ushort UpSubvelocity { get; internal set; }
    /// <summary>Signed whole-pixel component of the projectile's X velocity per AI update, calculated from its launch angle and paired with <see cref="XSubvelocity"/> as signed 16.16. Native extended offset $0E.</summary>
    public short XVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the projectile's aimed X velocity, in 1/65536 pixel per AI update. Native extended offset $10.</summary>
    public ushort XSubvelocity { get; internal set; }
    /// <summary>Signed whole-pixel component of the projectile's Y velocity per AI update, calculated from its launch angle and paired with <see cref="YSubvelocity"/> as signed 16.16. Native extended offset $12.</summary>
    public short YVelocity { get; internal set; }
    /// <summary>Unsigned fractional component of the projectile's aimed Y velocity, in 1/65536 pixel per AI update. Native extended offset $14.</summary>
    public ushort YSubvelocity { get; internal set; }
    /// <summary>Projectile flight flag, one while moving and zero while idle or regenerating; flight selects the normal projectile animation. Native extended offset $16.</summary>
    public ushort MovingFlag { get; internal set; }
    /// <summary>Projectile growth flag, set to one when distant flight ends and cleared by the regeneration-complete animation command at $A8:87CB; also holds the body's facing during growth. Native extended offset $18.</summary>
    public ushort RegenerationFlag { get; internal set; }
}

/// <summary>
/// Literal translation of retail enemies $E63F/$E67F from $A8:8687-$8B58. Evir is a
/// deliberately coupled three-slot actor: the arms follow and face with the body, while the
/// projectile aims at Samus, flies until it is 256 pixels beyond the viewport, then grows
/// back out of the mouth through ROM animation bytecode.
/// </summary>
public sealed partial class RoomEnemySystem
{

    private const ushort EvirTouchAi = EnemyAiCodePointers.BankA8.EvirTouch;
    private const ushort EvirPowerBombAi = EnemyAiCodePointers.BankA8.EvirPowerBomb;
    private const ushort EvirShotAi = EnemyAiCodePointers.BankA8.EvirShot;
    private const ushort EvirSpitSound = 0x005e;
    private const ushort EvirActivationDistance = 0x0080;
    private const ushort EvirProjectileSpeed = 4;
    private const ushort EvirFarOffscreenDistance = 0x0100;

    private readonly EvirEnemyState?[] _evirStates =
        new EvirEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two spit sound requested during this enemy frame.</summary>
    public ushort? LastEvirSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Evir</c> at $A8:87E0.</summary>
    private void InitializeEvir(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new EvirEnemyState(slot);
        _evirStates[slot.SlotIndex] = state;

        if (slot.Parameter1 != 0)
        {
            // The second record is not a second creature. Its nonzero parameter selects the
            // arms role, which always reads facing and position from the immediately prior
            // body record. Retail gives it layer four so the two maps compose correctly.
            PositionEvirArms(slot, state, initializing: true);
            slot.Layer = 4;
        }
        else
        {
            SetEvirBodyFacing(slot, state, samus);

            // Parameter two's low byte indexes two adjacent signed 16.16 entries in the
            // shared linear-speed table: downward first, then upward. The high byte is the
            // steady-state number of frames between direction changes.
            ushort speedOffset = unchecked((ushort)((byte)slot.Parameter2 * 8));
            (state.DownVelocity, state.DownSubvelocity) = ReadLinearEnemySpeed(speedOffset);
            (state.UpVelocity, state.UpSubvelocity) = ReadLinearEnemySpeed(
                unchecked((ushort)(speedOffset + 4)));

            // $A8:8811 indexes Enemy.init1+1 ($0FB7) with Y, the speed-table byte offset,
            // rather than X. The byte read is absolute enemy RAM chosen by the speed index
            // alone: in the 100% movie's Botwoon-hallway Evirs (speed 12) it is the high
            // byte of slot 2's palette word, so the first half-cycle is not always zero.
            state.MovementTimer = (ushort)(ReadAliasedEnemyRamByte(
                EvirInitDefinitions.AliasedTimerAddress + speedOffset) >> 1);
        }

        state.MovementDirection = 0;
        state.InstalledInstructionList = 0;
        state.Function = EvirAiFunction.HandleBodyOrArms;
    }

    /// <summary>Reads one byte of the 32-slot enemy RAM block at absolute WRAM <paramref name="address"/>.</summary>
    private byte ReadAliasedEnemyRamByte(int address)
    {
        int relative = address - EnemyRamDefinitions.FirstSlotAddress;
        int slotIndex = relative / EnemyRamDefinitions.SlotStride;
        if (relative < 0 || slotIndex >= MaximumEnemyCount)
            throw new InvalidOperationException($"WRAM ${address:X4} lies outside enemy RAM.");
        int byteOffset = relative % EnemyRamDefinitions.SlotStride;
        ushort word = _slots[slotIndex].ReadNativeWord(byteOffset & ~1);
        return (byteOffset & 1) == 0 ? (byte)word : (byte)(word >> 8);
    }

    /// <summary>Ports <c>InitAI_EvirProjectile</c> at $A8:88B0.</summary>
    private void InitializeEvirProjectile(RoomEnemySlot slot)
    {
        var state = new EvirEnemyState(slot)
        {
            RequestedInstructionList = EvirInstructionProgramDefinitions.ProjectileNormal,
            InstalledInstructionList = 0,
            RegenerationFlag = 0,
            MovingFlag = 0,
            Function = EvirAiFunction.ProjectileIdle,
            RegenerationXOffset = 0,
        };
        _evirStates[slot.SlotIndex] = state;
        InstallEvirInstruction(slot, state);

        RoomEnemySlot body = RequireEvirRelativeSlot(slot, -2, EnemyDefinitionId.Evir, "projectile body", initializing: true);
        // The projectile definition has no graphics-set entry of its own. Native copies the
        // body's resolved palette/tile indexes so the projectile map addresses the Evir art.
        slot.PaletteIndex = body.PaletteIndex;
        slot.VramTilesIndex = body.VramTilesIndex;
        ResetEvirProjectilePosition(slot, state);

        // Native clears installed-list after having installed the normal list. That oddity
        // forces the first main-AI request to refresh the bytecode timer exactly once.
        state.InstalledInstructionList = 0;
    }

    private void RunEvirMain(RoomEnemySlot slot, EvirEnemyState state, SamusState? samus)
    {
        if (state.Function != EvirAiFunction.HandleBodyOrArms)
        {
            throw new InvalidDataException(
                $"Evir body/arms function $A8:{(ushort)state.Function:X4} is not translated.");
        }

        if (slot.Parameter1 != 0)
        {
            PositionEvirArms(slot, state);
            return;
        }

        RoomEnemySlot projectile = RequireEvirRelativeSlot(
            slot,
            2,
            EnemyDefinitionId.EvirProjectile,
            "body projectile");
        EvirEnemyState projectileState = RequireEvirState(projectile);

        // Direction is allowed to follow Samus only while the projectile is fully present.
        // During regeneration the mouth, arms, and projectile retain their launch facing.
        if (projectileState.RegenerationFlag == 0)
            SetEvirBodyFacing(slot, state, samus);

        int displacement = state.MovementDirection == 0
            ? ComposeEvirFixed(state.UpVelocity, state.UpSubvelocity)
            : ComposeEvirFixed(state.DownVelocity, state.DownSubvelocity);
        (slot.YPosition, slot.YSubposition) = AddEvirPosition(
            slot.YPosition,
            slot.YSubposition,
            displacement);

        // DEC/BPL means zero expires immediately into $FFFF. Every later reset uses the
        // unshifted high parameter byte, despite initialization's unrelated aliasing bug.
        state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
        if ((state.MovementTimer & 0x8000) != 0)
        {
            state.MovementTimer = unchecked((byte)(slot.Parameter2 >> 8));
            state.MovementDirection ^= 1;
        }
    }

    private void RunEvirProjectileMain(
        RoomEnemySlot slot,
        EvirEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        if (slot.FrozenTimer == 0)
        {
            if (state.MovingFlag != 0)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirInstructionProgramDefinitions.ProjectileNormal);
            }
            else if (state.RegenerationFlag != 0)
            {
                RequestEvirInstruction(
                    slot,
                    state,
                    EvirInstructionProgramDefinitions.ProjectileRegenerating);
            }
            else
            {
                TryLaunchEvirProjectile(slot, state, samus);
            }
        }

        switch (state.Function)
        {
            case EvirAiFunction.ProjectileIdle:
                ResetEvirProjectilePosition(slot, state);
                break;
            case EvirAiFunction.ProjectileMoving:
                RunMovingEvirProjectile(slot, state, cameraX, cameraY);
                break;
            case EvirAiFunction.ProjectileRegenerating:
                RunRegeneratingEvirProjectile(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Evir projectile function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void SetEvirBodyFacing(
        RoomEnemySlot body,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir facing AI requires the active Samus actor.");
        state.FacingDirection = unchecked((short)(activeSamus.XPosition - body.XPosition)) < 0
            ? (ushort)0
            : (ushort)1;
        RequestEvirInstruction(
            body,
            state,
            state.FacingDirection == 0
                ? EvirInstructionProgramDefinitions.BodyFacingLeft
                : EvirInstructionProgramDefinitions.BodyFacingRight);
    }

    private void PositionEvirArms(RoomEnemySlot arms, EvirEnemyState state, bool initializing = false)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(arms, -1, EnemyDefinitionId.Evir, "arms body", initializing);
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        arms.XPosition = unchecked((ushort)(
            body.XPosition + (state.FacingDirection == 0 ? -4 : 4)));
        arms.YPosition = unchecked((ushort)(body.YPosition + 10));
        RequestEvirInstruction(
            arms,
            state,
            state.FacingDirection == 0
                ? EvirInstructionProgramDefinitions.ArmsFacingLeft
                : EvirInstructionProgramDefinitions.ArmsFacingRight);
    }

    private void ResetEvirProjectilePosition(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        EvirEnemyState bodyState = RequireEvirState(body);
        state.FacingDirection = bodyState.FacingDirection;
        projectile.XPosition = unchecked((ushort)(
            body.XPosition + (state.FacingDirection == 0 ? -4 : 4)));
        projectile.YPosition = unchecked((ushort)(body.YPosition + 18));
    }

    private void TryLaunchEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Evir projectile aiming requires the active Samus actor.");
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (!IsWithinStrictModularDistance(
                activeSamus.XPosition,
                body.XPosition,
                EvirActivationDistance))
        {
            return;
        }

        // CalculateAngleOfSamusFromEnemy returns zero up/clockwise. The two native angle
        // transforms below feed the buggy table multipliers and produce signed 16.16 speed.
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(activeSamus.XPosition - projectile.XPosition)),
            unchecked((short)(activeSamus.YPosition - projectile.YPosition)));
        byte transformedAngle = unchecked((byte)-(byte)(cartridgeAngle - 0x40));
        (state.XVelocity, state.XSubvelocity) = ReadEightBitCosineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        (state.YVelocity, state.YSubvelocity) = ReadEightBitNegativeSineFixedProduct(
            transformedAngle,
            EvirProjectileSpeed);
        RequestEvirInstruction(
            projectile,
            state,
            EvirInstructionProgramDefinitions.ProjectileNormal);
        state.MovingFlag = 1;
        state.Function = EvirAiFunction.ProjectileMoving;
    }

    private void RunMovingEvirProjectile(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        StartEvirRegenerationIfFarOffscreen(projectile, state, cameraX, cameraY);
        (projectile.XPosition, projectile.XSubposition) = AddEvirPosition(
            projectile.XPosition,
            projectile.XSubposition,
            ComposeEvirFixed(state.XVelocity, state.XSubvelocity));
        (projectile.YPosition, projectile.YSubposition) = AddEvirPosition(
            projectile.YPosition,
            projectile.YSubposition,
            ComposeEvirFixed(state.YVelocity, state.YSubvelocity));
    }

    private void StartEvirRegenerationIfFarOffscreen(
        RoomEnemySlot projectile,
        EvirEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        bool farOffscreen =
            IsNegative16(projectile.XPosition + EvirFarOffscreenDistance - cameraX) ||
            IsNegative16(cameraX + 256 + EvirFarOffscreenDistance - projectile.XPosition) ||
            IsNegative16(projectile.YPosition + EvirFarOffscreenDistance - cameraY) ||
            IsNegative16(cameraY + 256 + EvirFarOffscreenDistance - projectile.YPosition);
        if (!farOffscreen)
            return;

        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        state.MovingFlag = 0;
        state.RegenerationFlag = 1;
        state.Function = EvirAiFunction.ProjectileRegenerating;
        RequestEvirInstruction(
            projectile,
            state,
            EvirInstructionProgramDefinitions.ProjectileRegenerating);
    }

    private void RunRegeneratingEvirProjectile(RoomEnemySlot projectile, EvirEnemyState state)
    {
        RoomEnemySlot body = RequireEvirRelativeSlot(
            projectile,
            -2,
            EnemyDefinitionId.Evir,
            "projectile body");
        if (body.FrozenTimer != 0)
            return;

        if (state.RegenerationFlag == 0)
        {
            RequestEvirInstruction(
                projectile,
                state,
                EvirInstructionProgramDefinitions.ProjectileNormal);
            state.MovingFlag = 0;
            state.Function = EvirAiFunction.ProjectileIdle;
            return;
        }

        ResetEvirProjectilePosition(projectile, state);
        projectile.XPosition = unchecked((ushort)(
            projectile.XPosition + state.RegenerationXOffset));
    }

    /// <summary>Instruction $A8:878F.</summary>
    private void QueueEvirSpitSound() => LastEvirSoundEffect = EvirSpitSound;

    /// <summary>Instruction $A8:879B.</summary>
    private void SetInitialEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = EvirBodyFacesRight(slot)
            ? unchecked((ushort)-8)
            : (ushort)8;

    /// <summary>Instruction $A8:87B6.</summary>
    private void AdvanceEvirRegenerationOffset(RoomEnemySlot slot, EvirEnemyState state) =>
        state.RegenerationXOffset = unchecked((ushort)(
            state.RegenerationXOffset + (EvirBodyFacesRight(slot) ? 1 : -1)));

    /// <summary>
    /// Both regeneration instructions test <c>Evir.instList-$80,X</c>: the installed list
    /// of the body two slots before the projectile, not any facing word of its own.
    /// </summary>
    private bool EvirBodyFacesRight(RoomEnemySlot projectile)
    {
        EvirEnemyState body = _evirStates[projectile.SlotIndex - 2] ?? throw new InvalidDataException(
            $"Evir projectile slot {projectile.SlotIndex} has no body two slots before it.");
        return body.InstalledInstructionList == EvirInstructionProgramDefinitions.BodyFacingRight;
    }

    /// <summary>Instruction $A8:87CB.</summary>
    private static void FinishEvirRegeneration(EvirEnemyState state)
    {
        state.RegenerationFlag = 0;
        state.MovingFlag = 0;
        state.Function = EvirAiFunction.ProjectileIdle;
    }

    private static int ComposeEvirFixed(short whole, ushort fraction) =>
        unchecked((whole << 16) | fraction);

    private static (ushort Position, ushort Subposition) AddEvirPosition(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private static void RequestEvirInstruction(
        RoomEnemySlot slot,
        EvirEnemyState state,
        ushort instructionList)
    {
        state.RequestedInstructionList = instructionList;
        InstallEvirInstruction(slot, state);
    }

    private static void InstallEvirInstruction(RoomEnemySlot slot, EvirEnemyState state)
    {
        if (state.RequestedInstructionList == state.InstalledInstructionList)
            return;
        slot.CurrentInstruction = state.RequestedInstructionList;
        state.InstalledInstructionList = state.RequestedInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private RoomEnemySlot RequireEvirRelativeSlot(
        RoomEnemySlot owner,
        int relativeSlot,
        EnemyDefinitionId expectedDefinition,
        string relationship,
        bool initializing = false)
    {
        int targetIndex = owner.SlotIndex + relativeSlot;
        // Native links are physical offsets, not live-header references. Contact can
        // clear the body after the active list was built; its deleted arms and spit
        // still execute once and read the cleared common words. Validate authored
        // identities at initialization, then retain the established physical alias.
        if ((uint)targetIndex >= _slots.Length ||
            (_slots[targetIndex].EnemyDefinitionPointer != expectedDefinition &&
                (initializing || _evirStates[targetIndex] is null)))
        {
            throw new InvalidDataException(
                $"Evir {relationship} expected definition ${(int)expectedDefinition:X4} " +
                $"at relative slot {relativeSlot:+#;-#;0} from {owner.SlotIndex}.");
        }
        return _slots[targetIndex];
    }

    private EvirEnemyState RequireEvirState(RoomEnemySlot slot) =>
        _evirStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Evir state.");

    /// <summary>
    /// Ports $A8:8B16 after common touch/shot/power-bomb damage. Native applies its +$40
    /// and +$80 physical-slot aliases relative to whichever $E63F record received the
    /// callback. Ordinary shots/touch can only reach the body because the arms carry
    /// property $0400, but the power-bomb walker deliberately ignores that property and can
    /// invoke the same code on the arms. Preserve that odd relative aliasing instead of
    /// replacing it with a body-only composite helper.
    /// </summary>
    private void ResolveEvirCombatAfterCommon(RoomEnemySlot actor)
    {
        int firstRelativeIndex = actor.SlotIndex + 1;
        int secondRelativeIndex = actor.SlotIndex + 2;
        if ((uint)secondRelativeIndex >= _slots.Length)
        {
            throw new InvalidDataException(
                $"Evir callback at slot {actor.SlotIndex} exceeds the native +$80 slot alias.");
        }
        RoomEnemySlot firstRelative = _slots[firstRelativeIndex];
        RoomEnemySlot secondRelative = _slots[secondRelativeIndex];

        if (actor.Health == 0)
        {
            firstRelative.Properties = firstRelative.Properties.With(EnemyProperties.Deleted);
            secondRelative.Properties = secondRelative.Properties.With(EnemyProperties.Deleted);
        }

        if (actor.FrozenTimer == 0)
            return;
        firstRelative.FrozenTimer = actor.FrozenTimer;
        firstRelative.AiHandlerBits = unchecked((ushort)(firstRelative.AiHandlerBits | 0x0004));
        if (secondRelative.VariableC != (ushort)EvirAiFunction.ProjectileMoving)
        {
            secondRelative.FrozenTimer = actor.FrozenTimer;
            secondRelative.AiHandlerBits = unchecked((ushort)(
                secondRelative.AiHandlerBits | 0x0004));
        }
    }
}
