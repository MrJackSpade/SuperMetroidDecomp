namespace SuperMetroid.Core.Game;

/// <summary>Exact same-bank function words dispatched by Rinka main AI at $A2:B7C4.</summary>
public enum RinkaEnemyFunction : ushort
{
    AimDelay = 0xb7df,
    DeathRespawnDelay = 0xb844,
    WatchForLeavingViewport = 0xb852,
    Flying = 0xb85b,
}

/// <summary>
/// Named projection of Rinka's six common AI variables. Keeping these properties backed by
/// the physical enemy slot makes debugger watches agree with WRAM $0FA8-$0FB2 while avoiding
/// unexplained VariableA/VariableB accesses throughout the translated state machine.
/// </summary>
public sealed class RinkaEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal RinkaEnemyState(RoomEnemySlot slot) => _slot = slot;

    public RinkaEnemyFunction Function
    {
        get => (RinkaEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Signed 8.8 horizontal velocity produced by Math_MultBySin.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Signed 8.8 vertical velocity produced by Math_MultByCos.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Native extra-RAM selector stored in variable D. Special Mother Brain Rinkas use the
    /// even values $0002..$0016 from $A2:B75B; zero means no spawn resource is reserved.
    /// </summary>
    public ushort SpawnResourceToken
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Wrapping 16-bit countdown used both before flight and before special respawn.</summary>
    public ushort DelayTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>One literal (X, Y, extra-RAM selector) record from $A2:B75B.</summary>
public readonly record struct RinkaSpawnResource(
    ushort XPosition,
    ushort YPosition,
    ushort Token);

/// <summary>Literal translation of Rinka enemy AI $A2:B602-$BA0B.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort RinkaDefinition = 0xd23f;
    internal const ushort RinkaTouchAi = EnemyAiCodePointers.BankA2.RinkaTouch;
    internal const ushort RinkaShotAi = EnemyAiCodePointers.BankA2.RinkaShot;
    internal const ushort RinkaPowerBombAi = EnemyAiCodePointers.BankA2.RinkaPowerBomb;

    private const ushort RinkaOrdinaryInstructionList = 0xb9e0;
    private const ushort RinkaSpecialInstructionList = 0xba0c;
    private const ushort RinkaInitialDelay = 26;
    private const ushort RinkaHealth = 10;
    private const ushort RinkaPaletteIndex = 0x0400;
    private const ushort RinkaSpeed = 0x0120;
    private const ushort RinkaMaximumSpecialActors = 3;

    // This is not a designed host spawn layout. These eleven triples are the untouched
    // Japan/USA cartridge words at $A2:B75B, including their unusual even resource tokens.
    // Mother Brain's three parameterized Rinka records compete for these locations and may
    // rewrite their immutable spawn snapshots when the original point is on another screen.
    private static readonly RinkaSpawnResource[] RinkaSpawnResources =
    [
        new(0x03e7, 0x0026, 0x0002),
        new(0x03e7, 0x00a6, 0x0004),
        new(0x0337, 0x0036, 0x0006),
        new(0x0337, 0x00a6, 0x0008),
        new(0x0277, 0x001c, 0x000a),
        new(0x0277, 0x00b6, 0x000c),
        new(0x01b7, 0x0036, 0x000e),
        new(0x01b7, 0x00a6, 0x0010),
        new(0x00f7, 0x001c, 0x0012),
        new(0x00f7, 0x00b6, 0x0014),
        new(0x0080, 0x00a8, 0x0016),
    ];

    private readonly RinkaEnemyState?[] _rinkaStates =
        new RinkaEnemyState?[MaximumEnemyCount];
    private readonly bool[] _rinkaOccupiedSpawnResources =
        new bool[RinkaMaximumSpecialActors + 8]; // Eleven literal table entries.
    private ushort _rinkaActiveCount;
    private ushort _rinkaTerminationFlag;
    private ushort _rinkaCameraX;
    private ushort _rinkaCameraY;

    /// <summary>Typed state for every physical enemy slot currently owned by a Rinka.</summary>
    public IReadOnlyList<RinkaEnemyState?> RinkaStates => _rinkaStates;

    /// <summary>
    /// Shared $7E:783C word (Rinka variable $1E) incremented by instruction $B9C7. Special
    /// Rinkas decrement it when a visible actor dies or leaves the viewport.
    /// </summary>
    public ushort RinkaActiveCount => _rinkaActiveCount;

    /// <summary>
    /// Shared $7E:783A word (Rinka variable $1D). Mother Brain sets this during the room's
    /// terminal transition; live, frozen, and off-screen special Rinkas all consume it.
    /// </summary>
    public ushort RinkaTerminationFlag
    {
        get => _rinkaTerminationFlag;
        set => _rinkaTerminationFlag = value;
    }

    /// <summary>Occupancy bits corresponding one-for-one with the eleven ROM spawn triples.</summary>
    public IReadOnlyList<bool> RinkaOccupiedSpawnResources => _rinkaOccupiedSpawnResources;

    /// <summary>Clears the two shared words and all per-slot/extra-RAM spawn ownership.</summary>
    private void ResetRinkaRoomState(ushort cameraX, ushort cameraY)
    {
        Array.Clear(_rinkaStates);
        Array.Clear(_rinkaOccupiedSpawnResources);
        _rinkaActiveCount = 0;
        _rinkaTerminationFlag = 0;
        _rinkaCameraX = cameraX;
        _rinkaCameraY = cameraY;
    }

    /// <summary>
    /// Captures layer-1 coordinates used by $A2:B8D3/$B8FF. StepFrame refreshes these before
    /// any Rinka AI; Load supplies the already-positioned destination camera for initialization.
    /// </summary>
    private void SetRinkaCamera(ushort cameraX, ushort cameraY)
    {
        _rinkaCameraX = cameraX;
        _rinkaCameraY = cameraY;
    }

    /// <summary>Ports <c>Rinka_Init</c> at $A2:B602.</summary>
    private void InitializeRinka(RoomEnemySlot slot)
    {
        var state = new RinkaEnemyState(slot);
        _rinkaStates[slot.SlotIndex] = state;

        // Both branches first clear the four native lifecycle/collision bits. The special
        // Mother Brain variant is always processed off-screen but never uses generic death
        // respawn; ordinary room Rinkas retain $4000 so their death explosion resurrects
        // the physical slot from its population snapshot.
        EnemyProperties resetMask =
            EnemyProperties.RespawnIfKilled |
            EnemyProperties.ProcessInstructions |
            EnemyProperties.ProcessOffScreen |
            EnemyProperties.IgnoreSamusCollision;
        slot.Properties = slot.Properties.Without(resetMask);
        if (IsSpecialRinka(slot))
        {
            ReserveSpecialRinkaSpawn(slot, state);
            slot.Properties = slot.Properties.With(
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.IgnoreSamusCollision);
        }
        else
        {
            slot.Properties = slot.Properties.With(
                EnemyProperties.RespawnIfKilled |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.IgnoreSamusCollision);
        }

        slot.PaletteIndex = RinkaPaletteIndex;
        InitializeRinkaLife(slot, state);
    }

    /// <summary>Ports <c>Rinka_Init2</c> and <c>Rinka_Init3</c> at $A2:B63E-$B69A.</summary>
    private void ReinitializeRinka(RoomEnemySlot slot, RinkaEnemyState state)
    {
        if (IsSpecialRinka(slot))
            ReserveSpecialRinkaSpawn(slot, state);

        // EnemySpawnData is a separate WRAM array and survives clearing the common 64-byte
        // actor. Special spawn selection deliberately mutates that snapshot; ordinary
        // Rinkas continue to return to their original population coordinate.
        slot.XPosition = slot.Spawn.Population.XPosition;
        slot.YPosition = slot.Spawn.Population.YPosition;
        InitializeRinkaLife(slot, state);
    }

    private void InitializeRinkaLife(RoomEnemySlot slot, RinkaEnemyState state)
    {
        state.Function = RinkaEnemyFunction.WatchForLeavingViewport;
        state.DelayTimer = RinkaInitialDelay;
        state.XVelocity = 0;
        state.YVelocity = 0;

        if (IsSpecialRinka(slot))
        {
            if (_rinkaTerminationFlag != 0)
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                return;
            }
            slot.CurrentInstruction = RinkaSpecialInstructionList;
        }
        else
        {
            slot.CurrentInstruction = RinkaOrdinaryInstructionList;
        }
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports <c>Rinka_Main</c> and all four indirect functions.</summary>
    private void RunRinkaMain(
        RoomEnemySlot slot,
        RinkaEnemyState state,
        SamusState? samus)
    {
        if (IsSpecialRinka(slot) && _rinkaTerminationFlag != 0)
        {
            DecrementVisibleSpecialRinkaCount(slot);
            ReleaseSpecialRinkaSpawn(state);
            RunRinkaDeathAnimation(slot);
            return;
        }

        switch (state.Function)
        {
            case RinkaEnemyFunction.WatchForLeavingViewport:
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaEnemyFunction.AimDelay:
                state.DelayTimer = unchecked((ushort)(state.DelayTimer - 1));
                if (!IsNegative16(state.DelayTimer))
                    return;
                if (samus is null)
                    throw new InvalidOperationException("Rinka homing requires the active Samus actor.");

                state.Function = RinkaEnemyFunction.Flying;

                // Special and ordinary branches have different source masks in native code,
                // but both observable results clear $0400 and retain/set $0800 here.
                slot.Properties = slot.Properties
                    .Without(EnemyProperties.IgnoreSamusCollision)
                    .With(EnemyProperties.ProcessOffScreen);

                byte sourceAngle = CalculateCartridgeAngle(
                    unchecked((short)(samus.XPosition - slot.XPosition)),
                    unchecked((short)(samus.YPosition - slot.YPosition)));
                byte velocityAngle = unchecked((byte)-(sourceAngle + 0x80));
                state.XVelocity = MultiplyCartridgeSinCos(RinkaSpeed, velocityAngle);
                state.YVelocity = MultiplyCartridgeSinCos(
                    RinkaSpeed,
                    unchecked((byte)(velocityAngle + 0x40)));
                return;

            case RinkaEnemyFunction.Flying:
                (slot.XPosition, slot.XSubposition) = AddEightBitVelocity(
                    slot.XPosition,
                    slot.XSubposition,
                    state.XVelocity);
                (slot.YPosition, slot.YSubposition) = AddEightBitVelocity(
                    slot.YPosition,
                    slot.YSubposition,
                    state.YVelocity);
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaEnemyFunction.DeathRespawnDelay:
                state.DelayTimer = unchecked((ushort)(state.DelayTimer - 1));
                if (!IsNegative16(state.DelayTimer))
                    return;
                slot.Health = RinkaHealth;
                ReinitializeRinka(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Rinka function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Ports the family-specific tail run after common touch, shot, or power-bomb damage.
    /// Common damage deliberately leaves lethal Rinkas alive long enough for this method to
    /// select their two distinct native death lifecycles.
    /// </summary>
    private void ResolveRinkaCombatAfterCommon(RoomEnemySlot slot)
    {
        if (slot.Health != 0)
            return;

        RinkaEnemyState state = RequireRinkaState(slot);
        DecrementVisibleSpecialRinkaCount(slot);
        ReleaseSpecialRinkaSpawn(state);
        if (!IsSpecialRinka(slot))
        {
            RunRinkaDeathAnimation(slot);
            return;
        }

        // Special Mother Brain Rinkas do not clear their enemy slot. They hide, emit the
        // room-graphics dust/explosion definition $86:E509 with parameter three, wait for
        // the wrapping 1 -> 0 -> FFFF countdown, restore ten health, and select a spawn.
        slot.Properties = slot.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
        SpawnRinkaDustExplosion(slot.XPosition, slot.YPosition);
        state.Function = RinkaEnemyFunction.DeathRespawnDelay;
        state.DelayTimer = 1;
    }

    /// <summary>Runs Rinka's private frozen-AI prelude/tail around common frozen handling.</summary>
    private bool RunRinkaFrozenTail(RoomEnemySlot slot)
    {
        if (RinkaIsOutsideFlightWindow(slot))
            slot.FrozenTimer = 0;
        if (_rinkaTerminationFlag == 0)
            return false;

        DecrementVisibleSpecialRinkaCount(slot);
        ReleaseSpecialRinkaSpawn(RequireRinkaState(slot));
        RunRinkaDeathAnimation(slot);
        return true;
    }

    /// <summary>Ports private animation instructions $B9A2/$B9B3/$B9BD/$B9C7.</summary>
    private bool TryProcessRinkaInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != RinkaDefinition)
            return false;

        switch (opcode)
        {
            case RinkaInstructionCodes.UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2:
                // The routine receives a pointer to its two-byte operand. Below three live
                // actors it skips that operand; otherwise it returns the operand as a direct
                // same-bank destination. This opcode is retained even though the retail
                // BA0C list in the user's revision does not currently reference it.
                cursor = _rinkaActiveCount < RinkaMaximumSpecialActors
                    ? unchecked((ushort)(cursor + 4))
                    : ReadWord(_bus!, 0xa20000 | unchecked((ushort)(cursor + 2)));
                return true;

            case RinkaInstructionCodes.Instruction_Rinka_SetAsIntangibleAndInvisible:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case RinkaInstructionCodes.Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case RinkaInstructionCodes.Instruction_Rinka_FireRinka:
                slot.Properties = slot.Properties.Without(
                    EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                RequireRinkaState(slot).Function = RinkaEnemyFunction.AimDelay;
                _rinkaActiveCount = unchecked((ushort)(_rinkaActiveCount + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    private void RecycleRinkaAfterLeavingViewport(
        RoomEnemySlot slot,
        RinkaEnemyState state)
    {
        if (IsSpecialRinka(slot))
        {
            ReleaseSpecialRinkaSpawn(state);
            if (_rinkaTerminationFlag != 0)
            {
                DecrementVisibleSpecialRinkaCount(slot);
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                return;
            }
        }

        DecrementVisibleSpecialRinkaCount(slot);
        ReinitializeRinka(slot, state);
    }

    /// <summary>Ports $A2:B69B/$B79D's fixed-resource selection without WRAM alias tricks.</summary>
    private void ReserveSpecialRinkaSpawn(RoomEnemySlot slot, RinkaEnemyState state)
    {
        ushort spawnX = slot.Spawn.Population.XPosition;
        ushort spawnY = slot.Spawn.Population.YPosition;
        int mappedIndex = FindRinkaSpawnResource(spawnX, spawnY);
        bool mustRelocate = RinkaSpawnIsOutsideCamera(spawnX, spawnY) ||
            _rinkaOccupiedSpawnResources[mappedIndex];

        if (mustRelocate)
        {
            // The first pass prefers a free point on the current camera. If all eleven are
            // off-camera or occupied, native code performs a second pass that ignores the
            // camera but still refuses to steal an occupied resource.
            int selectedIndex = -1;
            for (int index = 0; index < RinkaSpawnResources.Length; index++)
            {
                RinkaSpawnResource candidate = RinkaSpawnResources[index];
                if (!RinkaSpawnIsOutsideCamera(candidate.XPosition, candidate.YPosition) &&
                    !_rinkaOccupiedSpawnResources[index])
                {
                    selectedIndex = index;
                    break;
                }
            }
            if (selectedIndex < 0)
            {
                for (int index = 0; index < RinkaSpawnResources.Length; index++)
                {
                    if (!_rinkaOccupiedSpawnResources[index])
                    {
                        selectedIndex = index;
                        break;
                    }
                }
            }

            // All eleven resources being occupied is a real native failure path: Rinka_1
            // returns without inventing a twelfth location or overwriting another owner.
            if (selectedIndex < 0)
                return;

            mappedIndex = selectedIndex;
            RinkaSpawnResource selected = RinkaSpawnResources[selectedIndex];
            RoomEnemyPopulationRecord rewrittenPopulation = slot.Spawn.Population with
            {
                XPosition = selected.XPosition,
                YPosition = selected.YPosition,
            };
            slot.Spawn = slot.Spawn with { Population = rewrittenPopulation };
            slot.XPosition = selected.XPosition;
            slot.YPosition = selected.YPosition;
        }

        _rinkaOccupiedSpawnResources[mappedIndex] = true;
        state.SpawnResourceToken = RinkaSpawnResources[mappedIndex].Token;
    }

    private static int FindRinkaSpawnResource(ushort xPosition, ushort yPosition)
    {
        for (int index = 0; index < RinkaSpawnResources.Length; index++)
        {
            RinkaSpawnResource resource = RinkaSpawnResources[index];
            if (resource.XPosition == xPosition && resource.YPosition == yPosition)
                return index;
        }

        // Rinka_2 falls back to the first table resource when a population coordinate does
        // not match. Preserve that odd behavior instead of rejecting a synthetic fixture.
        return 0;
    }

    private void ReleaseSpecialRinkaSpawn(RinkaEnemyState state)
    {
        if (state.SpawnResourceToken == 0)
            return;

        for (int index = 0; index < RinkaSpawnResources.Length; index++)
        {
            if (RinkaSpawnResources[index].Token == state.SpawnResourceToken)
            {
                _rinkaOccupiedSpawnResources[index] = false;
                break;
            }
        }
        state.SpawnResourceToken = 0;
    }

    private void DecrementVisibleSpecialRinkaCount(RoomEnemySlot slot)
    {
        if (!IsSpecialRinka(slot) || slot.Properties.HasAny(EnemyProperties.Invisible))
            return;
        _rinkaActiveCount = _rinkaActiveCount == 0
            ? (ushort)0
            : unchecked((ushort)(_rinkaActiveCount - 1));
    }

    /// <summary>Ports $A2:B8D3's asymmetric (-16..271, -16..239) flight rectangle.</summary>
    private bool RinkaIsOutsideFlightWindow(RoomEnemySlot slot)
    {
        if (unchecked((short)slot.YPosition) < 0)
            return true;
        ushort relativeY = unchecked((ushort)(slot.YPosition + 16 - _rinkaCameraY));
        if (unchecked((short)relativeY) < 0 || !IsNegative16(relativeY - 256))
            return true;
        if (unchecked((short)slot.XPosition) < 0)
            return true;
        ushort relativeX = unchecked((ushort)(slot.XPosition + 16 - _rinkaCameraX));
        return unchecked((short)relativeX) < 0 || !IsNegative16(relativeX - 288);
    }

    /// <summary>Ports $A2:B8FF's strict camera-origin test used only for special spawns.</summary>
    private bool RinkaSpawnIsOutsideCamera(ushort xPosition, ushort yPosition)
    {
        if (unchecked((short)yPosition) < 0 ||
            unchecked((short)(yPosition - _rinkaCameraY)) < 0 ||
            !IsNegative16(yPosition - _rinkaCameraY - 224))
        {
            return true;
        }
        return unchecked((short)(xPosition - _rinkaCameraX)) < 0 ||
            !IsNegative16(xPosition - _rinkaCameraX - 256);
    }

    private static bool IsSpecialRinka(RoomEnemySlot slot) => slot.Parameter1 != 0;

    private RinkaEnemyState RequireRinkaState(RoomEnemySlot slot) =>
        _rinkaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rinka state.");
}
