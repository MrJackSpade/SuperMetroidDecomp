namespace SuperMetroid.Core.Game;

/// <summary>Exact same-bank function words dispatched by Rinka main AI at $A2:B7C4.</summary>
public enum RinkaEnemyFunction : ushort
{
    /// <summary>Function_Rinka_Fire, $A2:B7DF: decrements the initially 26-update timer through signed underflow, then samples Samus once to form signed 8.8 velocities at nominal $0120 speed; movement begins on the following AI update.</summary>
    AimDelay = 0xb7df,
    /// <summary>Function_Rinka_Killed, $A2:B844: hidden Mother Brain variant waits for its timer's signed underflow (1, 0, $FFFF), restores ten health, and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay = 0xb844,
    /// <summary>Function_Rinka_WaitingToFire, $A2:B852: stationary initial state that recycles an actor outside the expanded camera flight rectangle; animation instruction $B9C7, not this function's timer, starts the aim delay.</summary>
    WatchForLeavingViewport = 0xb852,
    /// <summary>Function_Rinka_Moving, $A2:B85B: adds the fixed signed 8.8 velocities to the actor's fractional room position each update, then recycles it outside the camera-relative -16..271 X / -16..239 Y window without re-aiming.</summary>
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

    /// <summary>Common variable A ($7E:0FA8 plus slot offset): same-bank $A2 routine word dispatched by main AI, changed by initialization, the fire instruction, aim completion, or the special death tail.</summary>
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
/// <param name="XPosition">Authored spawn-center X in whole room pixels, tested against the current layer-one camera when reserving a Mother Brain Rinka location.</param>
/// <param name="YPosition">Authored spawn-center Y in whole room pixels, not an OAM or camera-relative coordinate.</param>
/// <param name="Token">Native spawn-availability byte selector plus two: even $0002..$0016 for the eleven table locations, reserved until the owning actor leaves, dies, or is terminated.</param>
public readonly record struct RinkaSpawnResource(
    ushort XPosition,
    ushort YPosition,
    ushort Token);

/// <summary>Literal translation of Rinka enemy AI $A2:B602-$BA0B.</summary>
public sealed partial class RoomEnemySystem
{

    private const ushort RinkaInitialDelay = 26;
    private const ushort RinkaHealth = 10;
    private static readonly ushort RinkaPaletteIndex = EnemyPaletteBits.Palette2;
    private const ushort RinkaSpeed = 0x0120;
    private const ushort RinkaMaximumSpecialActors = 3;

    // This is not a designed host spawn layout. These eleven triples are the untouched
    // Japan/USA cartridge words at $A2:B75B, including their unusual even resource tokens.
    // Mother Brain's three parameterized Rinka records compete for these locations and may
    // rewrite their immutable spawn snapshots when the original point is on another screen.
    // $A2:B75B: the eleven authored spawn points; each record's extra-RAM selector token is
    // calculated from its position in the list.
    private static readonly (ushort X, ushort Y)[] RinkaSpawnPoints =
    [
        (0x03e7, 0x0026),
        (0x03e7, 0x00a6),
        (0x0337, 0x0036),
        (0x0337, 0x00a6),
        (0x0277, 0x001c),
        (0x0277, 0x00b6),
        (0x01b7, 0x0036),
        (0x01b7, 0x00a6),
        (0x00f7, 0x001c),
        (0x00f7, 0x00b6),
        (0x0080, 0x00a8),
    ];

    private static RinkaSpawnResource RinkaSpawnResourceAt(int index) =>
        new(RinkaSpawnPoints[index].X, RinkaSpawnPoints[index].Y, (ushort)(2 * (index + 1)));

    private readonly RinkaEnemyState?[] _rinkaStates =
        new RinkaEnemyState?[MaximumEnemyCount];
    private readonly bool[] _rinkaOccupiedSpawnResources =
        new bool[RinkaMaximumSpecialActors + 8]; // Eleven literal table entries.
    private ushort _rinkaActiveCount;
    private ushort _rinkaTerminationFlag;
    private ushort _rinkaCameraX;
    private ushort _rinkaCameraY;
    // Special Rinkas whose spawn choice awaits the door loader's camera; see
    // CompleteLoaderTimeCameraReads. Ascending slot order, as Initialise_Enemies runs them.
    private readonly List<int> _deferredRinkaSpawnSlots = new();

    /// <summary>Clears the two shared words and all per-slot/extra-RAM spawn ownership.</summary>
    private void ResetRinkaRoomState(ushort cameraX, ushort cameraY)
    {
        Array.Clear(_rinkaStates);
        Array.Clear(_rinkaOccupiedSpawnResources);
        _deferredRinkaSpawnSlots.Clear();
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
            // $A2:B69B tests its spawn points against layer 1. A door's loader runs this
            // while the door IRQ is still scrolling, so that choice waits for the camera
            // of the update in which the loader reaches this slot.
            if (_deferLoaderTimeCameraReads)
                _deferredRinkaSpawnSlots.Add(slot.SlotIndex);
            else
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
            slot.CurrentInstruction = RinkaInstructionProgramDefinitions.SpecialInitial;
        }
        else
        {
            slot.CurrentInstruction = RinkaInstructionProgramDefinitions.OrdinaryInitial;
        }
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>True while a door load still owes a special Rinka its spawn choice.</summary>
    internal bool HasDeferredLoaderTimeCameraReads => _deferredRinkaSpawnSlots.Count != 0;

    /// <summary>
    /// Runs the camera-dependent part of each deferred initialization whose slot the door
    /// loader has reached, against that update's layer-1 position.
    /// </summary>
    internal void CompleteLoaderTimeCameraReads(
        Func<int, bool> loaderInitializedSlot,
        ushort cameraX,
        ushort cameraY)
    {
        ArgumentNullException.ThrowIfNull(loaderInitializedSlot);
        while (_deferredRinkaSpawnSlots.Count != 0 && loaderInitializedSlot(_deferredRinkaSpawnSlots[0]))
        {
            int slotIndex = _deferredRinkaSpawnSlots[0];
            _deferredRinkaSpawnSlots.RemoveAt(0);
            RinkaEnemyState state = _rinkaStates[slotIndex] ?? throw new InvalidOperationException(
                $"Deferred Rinka spawn slot {slotIndex} no longer holds a Rinka.");
            SetRinkaCamera(cameraX, cameraY);
            ReserveSpecialRinkaSpawn(_slots[slotIndex], state);
        }
    }

    /// <summary>Ports <c>Rinka_Main</c> and all four indirect functions.</summary>
    private void RunRinkaMain(
        RoomEnemySlot slot,
        RinkaEnemyState state,
        SamusState? samus)
    {
        if (_deferredRinkaSpawnSlots.Count != 0)
            throw new InvalidOperationException(
                "A Rinka ran before the door loader chose every special Rinka's spawn point.");
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
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Rinka)
            return false;

        if (!Enum.IsDefined((RinkaInstruction)opcode))
            return false;
        switch ((RinkaInstruction)opcode)
        {
            case RinkaInstruction.UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2:
                // No retail Rinka list in the pinned revision invokes this routine, so there
                // is no authored operand to compile. Treating the following native code bytes
                // as one would let a corrupt/restored cursor escape the bounded program.
                throw new InvalidDataException(
                    "Unused Rinka conditional instruction $A2:B9A2 has no retail program operand.");

            case RinkaInstruction.Instruction_Rinka_SetAsIntangibleAndInvisible:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case RinkaInstruction.Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.ProcessOffScreen |
                    EnemyProperties.IgnoreSamusCollision |
                    EnemyProperties.Invisible);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case RinkaInstruction.Instruction_Rinka_FireRinka:
                slot.Properties = slot.Properties.Without(
                    EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                RequireRinkaState(slot).Function = RinkaEnemyFunction.AimDelay;
                _rinkaActiveCount = unchecked((ushort)(_rinkaActiveCount + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            default:
                throw new InvalidOperationException($"Undefined {nameof(RinkaInstruction)} {opcode:X4}.");
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
            for (int index = 0; index < RinkaSpawnPoints.Length; index++)
            {
                RinkaSpawnResource candidate = RinkaSpawnResourceAt(index);
                if (!RinkaSpawnIsOutsideCamera(candidate.XPosition, candidate.YPosition) &&
                    !_rinkaOccupiedSpawnResources[index])
                {
                    selectedIndex = index;
                    break;
                }
            }
            if (selectedIndex < 0)
            {
                for (int index = 0; index < RinkaSpawnPoints.Length; index++)
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
            RinkaSpawnResource selected = RinkaSpawnResourceAt(selectedIndex);
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
        state.SpawnResourceToken = RinkaSpawnResourceAt(mappedIndex).Token;
    }

    private static int FindRinkaSpawnResource(ushort xPosition, ushort yPosition)
    {
        for (int index = 0; index < RinkaSpawnPoints.Length; index++)
        {
            RinkaSpawnResource resource = RinkaSpawnResourceAt(index);
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

        for (int index = 0; index < RinkaSpawnPoints.Length; index++)
        {
            if (RinkaSpawnResourceAt(index).Token == state.SpawnResourceToken)
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
