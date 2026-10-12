namespace SuperMetroid.Core.Game;

/// <summary>The Rinka species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RinkaBehavior : IEnemySpecies
{
    private readonly IEnemyDispatch _dispatch;
    private readonly EnemySlotTable _slotTable;
    private readonly EnemyCollision _collision;
    private readonly EnemyFrameContext _frameContext;
    private readonly EnemyProjectileEngine _projectiles;
    private readonly EnemyRoomBinding _room;
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    internal RinkaBehavior(IEnemyDispatch dispatch, EnemySlotTable slotTable, EnemyCollision collision, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, EnemyRoomBinding room, EnemyDeathsAndDrops deathsAndDrops)
    {
        _dispatch = dispatch;
        _slotTable = slotTable;
        _collision = collision;
        _frameContext = frameContext;
        _projectiles = projectiles;
        _room = room;
        _deathsAndDrops = deathsAndDrops;
    }

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
    private static readonly Position16<RoomPixels>[] RinkaSpawnPoints =
    [
        new(0x03e7, 0x0026),
        new(0x03e7, 0x00a6),
        new(0x0337, 0x0036),
        new(0x0337, 0x00a6),
        new(0x0277, 0x001c),
        new(0x0277, 0x00b6),
        new(0x01b7, 0x0036),
        new(0x01b7, 0x00a6),
        new(0x00f7, 0x001c),
        new(0x00f7, 0x00b6),
        new(0x0080, 0x00a8),
    ];

    internal static RinkaSpawnResource RinkaSpawnResourceAt(int index) =>
        new(RinkaSpawnPoints[index], (ushort)(2 * (index + 1)));

    internal readonly RinkaEnemyState?[] _rinkaStates =
        new RinkaEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    private readonly bool[] _rinkaOccupiedSpawnResources =
        new bool[RinkaMaximumSpecialActors + 8]; // Eleven literal table entries.
    private ushort _rinkaActiveCount;
    internal ushort _rinkaTerminationFlag;
    private Position16<RoomPixels> _rinkaCamera;
    // Special Rinkas whose spawn choice awaits the door loader's camera; see
    // CompleteLoaderTimeCameraReads. Ascending slot order, as Initialise_Enemies runs them.
    private readonly List<int> _deferredRinkaSpawnSlots = new();

    /// <summary>Clears the two shared words and all per-slot/extra-RAM spawn ownership.</summary>
    internal void ResetRinkaRoomState(Position16<RoomPixels> camera)
    {
        Array.Clear(_rinkaStates);
        Array.Clear(_rinkaOccupiedSpawnResources);
        _deferredRinkaSpawnSlots.Clear();
        _rinkaActiveCount = 0;
        _rinkaTerminationFlag = 0;
        _rinkaCamera = camera;
    }

    /// <summary>
    /// Captures layer-1 coordinates used by $A2:B8D3/$B8FF. StepFrame refreshes these before
    /// any Rinka AI; Load supplies the already-positioned destination camera for initialization.
    /// </summary>
    internal void SetRinkaCamera(Position16<RoomPixels> camera)
    {
        _rinkaCamera = camera;
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
            if (_frameContext._deferLoaderTimeCameraReads)
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
        slot.Position = slot.Position.WithWhole(new(slot.Spawn.Population.Position.X, slot.Spawn.Population.Position.Y));
        InitializeRinkaLife(slot, state);
    }

    private void InitializeRinkaLife(RoomEnemySlot slot, RinkaEnemyState state)
    {
        state.Function = RinkaEnemyFunction.WatchForLeavingViewport;
        state.DelayTimer = RinkaInitialDelay;
        state.Velocity = default;

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
        Position16<RoomPixels> camera)
    {
        Ensure.NotNull(loaderInitializedSlot);
        while (_deferredRinkaSpawnSlots.Count != 0 && loaderInitializedSlot(_deferredRinkaSpawnSlots[0]))
        {
            int slotIndex = _deferredRinkaSpawnSlots[0];
            _deferredRinkaSpawnSlots.RemoveAt(0);
            RinkaEnemyState state = _rinkaStates[slotIndex] ?? throw new InvalidOperationException(
                $"Deferred Rinka spawn slot {slotIndex} no longer holds a Rinka.");
            SetRinkaCamera(camera);
            ReserveSpecialRinkaSpawn(_slotTable._slots[slotIndex], state);
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
                if (!EnemyMath.IsNegative16(state.DelayTimer))
                    return;
                if (samus is null)
                    throw new InvalidOperationException("Rinka homing requires the active Samus actor.");

                state.Function = RinkaEnemyFunction.Flying;

                // Special and ordinary branches have different source masks in native code,
                // but both observable results clear $0400 and retain/set $0800 here.
                slot.Properties = slot.Properties
                    .Without(EnemyProperties.IgnoreSamusCollision)
                    .With(EnemyProperties.ProcessOffScreen);

                byte sourceAngle = EnemyMath.CalculateCartridgeAngle(
                    new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
                byte velocityAngle = unchecked((byte)-(sourceAngle + 0x80));
                state.Velocity = new(
                    new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(RinkaSpeed, velocityAngle)),
                    new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
                        RinkaSpeed,
                        unchecked((byte)(velocityAngle + 0x40)))));
                return;

            case RinkaEnemyFunction.Flying:
                slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
                if (RinkaIsOutsideFlightWindow(slot))
                    RecycleRinkaAfterLeavingViewport(slot, state);
                return;

            case RinkaEnemyFunction.DeathRespawnDelay:
                state.DelayTimer = unchecked((ushort)(state.DelayTimer - 1));
                if (!EnemyMath.IsNegative16(state.DelayTimer))
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
        SpawnRinkaDustExplosion(slot.Position.Whole);
        state.Function = RinkaEnemyFunction.DeathRespawnDelay;
        state.DelayTimer = 1;
    }

    /// <summary>Runs Rinka's private frozen-AI prelude/tail around common frozen handling.</summary>
    internal bool RunRinkaFrozenTail(RoomEnemySlot slot)
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
        Position16<RoomPixels> spawn = slot.Spawn.Population.Position;
        int mappedIndex = FindRinkaSpawnResource(spawn);
        bool mustRelocate = RinkaSpawnIsOutsideCamera(spawn) ||
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
                if (!RinkaSpawnIsOutsideCamera(candidate.Position) &&
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
                Position = selected.Position,
            };
            slot.Spawn = slot.Spawn with { Population = rewrittenPopulation };
            slot.Position = slot.Position.WithWhole(selected.Position);
        }

        _rinkaOccupiedSpawnResources[mappedIndex] = true;
        state.SpawnResourceToken = RinkaSpawnResourceAt(mappedIndex).Token;
    }

    private static int FindRinkaSpawnResource(Position16<RoomPixels> position)
    {
        for (int index = 0; index < RinkaSpawnPoints.Length; index++)
        {
            RinkaSpawnResource resource = RinkaSpawnResourceAt(index);
            if (resource.Position == position)
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
        if (unchecked((short)slot.Position.Y.Whole) < 0)
            return true;
        ushort relativeY = unchecked((ushort)(slot.Position.Y.Whole + 16 - _rinkaCamera.Y));
        if (unchecked((short)relativeY) < 0 || !EnemyMath.IsNegative16(relativeY - 256))
            return true;
        if (unchecked((short)slot.Position.X.Whole) < 0)
            return true;
        ushort relativeX = unchecked((ushort)(slot.Position.X.Whole + 16 - _rinkaCamera.X));
        return unchecked((short)relativeX) < 0 || !EnemyMath.IsNegative16(relativeX - 288);
    }

    /// <summary>Ports $A2:B8FF's strict camera-origin test used only for special spawns.</summary>
    private bool RinkaSpawnIsOutsideCamera(Position16<RoomPixels> position)
    {
        if (unchecked((short)position.Y) < 0 ||
            unchecked((short)(position.Y - _rinkaCamera.Y)) < 0 ||
            !EnemyMath.IsNegative16(position.Y - _rinkaCamera.Y - 224))
        {
            return true;
        }
        return unchecked((short)(position.X - _rinkaCamera.X)) < 0 ||
            !EnemyMath.IsNegative16(position.X - _rinkaCamera.X - 256);
    }

    private static bool IsSpecialRinka(RoomEnemySlot slot) => slot.Parameter1 != 0;

    private RinkaEnemyState RequireRinkaState(RoomEnemySlot slot) =>
        _rinkaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rinka state.");

    internal void RegisterRinkaBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.RinkaShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
        });
    }

    internal void RegisterRinkaInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Rinka, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeRinka(slot);
        });
    }

    internal void RegisterRinkaMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Rinka, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, (slot, frame, nmiFrameCounter8) =>
        {
            RunRinkaMain(slot, RequireRinkaState(slot), frame.Samus);
        });
    }

    internal void RegisterRinkaOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessRinkaInstruction(slot, word, ref cursor);
        });
    }

    internal void RegisterRinkaPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.RinkaPowerBomb)
        {
            SkipsEnemy = enemy => enemy.Properties.HasAny(EnemyProperties.Invisible),
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, _) => ResolveRinkaCombatAfterCommon(enemy),
        });
    }

    internal void RegisterRinkaShotReactions(EnemyAiTable table)
    {
        // Rinka's tail follows the damage path only; the immediate-freeze path skips it.
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.RinkaShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (c, froze) =>
            {
                if (!froze)
                    ResolveRinkaCombatAfterCommon(c.Enemy);
            },
        });
    }

    internal void RegisterRinkaTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.RinkaTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
                ResolveRinkaCombatAfterCommon(context.Slot);
            });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetRinkaRoomState(request.Camera);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        SetRinkaCamera(frame.Camera);
    }
    private const ushort RinkaDustAnimationIndex = 3;
    /// <summary>Allocates <c>SpawnEprojWithRoomGfx($E509, 3)</c>.</summary>
    internal void SpawnRinkaDustExplosion(Position16<RoomPixels> position)
        => _projectiles.SpawnRoomGraphicsDustExplosion(position, RinkaDustAnimationIndex);

    /// <summary>Ports <c>RinkasDeathAnimation(0)</c> at $A0:A410.</summary>
    internal void RunRinkaDeathAnimation(RoomEnemySlot slot)
    {
        _deathsAndDrops._enemyDeathRespawnScratch = EnemyDeathsAndDrops.RespawnScratchWord(slot);
        bool respawns = slot.Properties.HasAny(EnemyProperties.RespawnIfKilled);
        RoomEnemySpawnSnapshot survivingSpawnSnapshot = slot.Spawn;

        // The shared spawn helper initially carries the dying enemy's graphics word, but
        // EprojInit_EnemyDeathExplosion immediately replaces eproj_gfx_idx with zero. Pool
        // exhaustion genuinely loses both the effect and the only future respawn instruction.
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is not null)
        {
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.EnemyDeathExplosion,
                graphicsIndex: 0);
            projectile.Position = projectile.Position.WithWhole(slot.Position.Whole);
            projectile.EnemyHeaderPointer = slot.EnemyDefinitionPointer;
            projectile.KilledEnemyNativeIndex = respawns
                ? unchecked((ushort)(slot.NativeIndex | 0x8000))
                : slot.NativeIndex;
            projectile.InstructionPointer = EnemyDeathExplosionDefinitions
                .InstructionPointer((ushort)EnemyDeathAnimation.SmallExplosion);
            projectile.InstructionTimer = 1;
        }

        int slotIndex = slot.SlotIndex;
        slot.Clear();

        // EnemySpawnData occupies a separate WRAM region and is not part of memset's
        // 64-byte target. Preserve it even for a non-respawning special Rinka so debugger
        // state remains structurally faithful after the common record disappears.
        slot.Spawn = survivingSpawnSnapshot;
        _rinkaStates[slotIndex] = null;
        if (respawns)
        {
            _deathsAndDrops.InstallRespawnPlaceholder(slot);
        }
    }

    /// <summary>Runs $86:E4FE's strict 256x256 camera cull for dust/explosion actors.</summary>
    internal static void CullMiscDustOutsideCamera(
        RoomEnemyProjectileSlot projectile,
        Position16<RoomPixels> camera)
    {
        // $86:E6E0 compares the position against camera + $100 and culls on BPL, so
        // a projectile exactly $100 past the camera is already off-screen.
        if (unchecked((short)(projectile.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(projectile.Position.X.Whole - (camera.X + 256))) >= 0 ||
            unchecked((short)(projectile.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(projectile.Position.Y.Whole - (camera.Y + 256))) >= 0)
        {
            projectile.Clear();
        }
    }


    internal static void RegisterMiscDustPreInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            CullMiscDustOutsideCamera(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MiscDust);
    }

    internal void RegisterRinkaFrozenPreemption(EnemyAiTable table) =>
        table.AddFrozenPreemption(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, RunRinkaFrozenTail);
}
