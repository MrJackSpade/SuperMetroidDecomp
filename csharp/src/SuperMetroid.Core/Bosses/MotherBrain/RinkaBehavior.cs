using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The Rinka species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RinkaBehavior : IEnemySpecies
{
    /// <summary>Retained enemy dispatcher supplied by the species composition root.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Locates special Rinkas whose spawn choice was deferred during door loading.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies normal Samus contact damage before the Rinka-specific death tail.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Indicates when door loading must defer camera-sensitive spawn selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Allocates Rinka dust and death explosion projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Retained room binding supplied by the species composition root.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Installs the ordinary Rinka's respawn placeholder after death.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Connects Rinka AI to enemy slots, collision, frame context, and death effects.</summary>
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

    /// <summary>Native countdown before a newly fired Rinka aims at Samus.</summary>
    private const ushort RinkaInitialDelay = 26;
    /// <summary>Health restored to a special Rinka after its death delay.</summary>
    private const ushort RinkaHealth = 10;
    /// <summary>Enemy palette selected for both ordinary and Mother Brain variants.</summary>
    private static readonly ushort RinkaPaletteIndex = EnemyPaletteBits.Palette2;
    /// <summary>Signed 8.8 magnitude used to resolve the cartridge angle into flight velocity.</summary>
    private const ushort RinkaSpeed = 0x0120;
    /// <summary>Number of Mother Brain Rinka actors sharing the eleven spawn resources.</summary>
    private const ushort RinkaMaximumSpecialActors = 3;

    /// <summary>
    /// The eleven authored Japan/USA spawn points at <c>$A2:B75B</c>. Mother Brain's
    /// three special actors compete for these resources and may rewrite their spawn snapshots
    /// when the original point is outside the current screen. The even extra-RAM token is
    /// derived from each point's position in this table.
    /// </summary>
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

    /// <summary>Pairs an authored spawn point with its one-based even extra-RAM resource token.</summary>
    internal static RinkaSpawnResource RinkaSpawnResourceAt(int index) =>
        new(RinkaSpawnPoints[index], (ushort)(2 * (index + 1)));

    /// <summary>Maps enemy slots to their current aim delay, velocity, and spawn ownership.</summary>
    internal readonly RinkaEnemyState?[] _rinkaStates =
        new RinkaEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Marks which of the eleven authored spawn points are currently reserved.</summary>
    private readonly bool[] _rinkaOccupiedSpawnResources =
        new bool[RinkaMaximumSpecialActors + 8]; // Eleven literal table entries.
    /// <summary>Tracks active visible special Rinkas as private room state.</summary>
    private ushort _rinkaActiveCount;
    /// <summary>Causes special Rinkas to be removed instead of launched or respawned.</summary>
    internal ushort _rinkaTerminationFlag;
    /// <summary>Layer-1 camera coordinates used by the native spawn and flight windows.</summary>
    private Position16<RoomPixels> _rinkaCamera;
    /// <summary>
    /// Special Rinka slots awaiting the door loader's camera for spawn selection, in the
    /// ascending order used by <c>Initialise_Enemies</c>.
    /// </summary>
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

    /// <summary>Resets the watch state and animation after initialization or respawn; deletes terminated special actors.</summary>
    private void InitializeRinkaLife(RoomEnemySlot slot, RinkaEnemyState state)
    {
        state.Function = RinkaEnemyFunction.WatchForLeavingViewport;
        state.DelayTimer = NativeCountdown.Start(RinkaInitialDelay);
        state.Velocity = default;

        if (IsSpecialRinka(slot))
        {
            if (_rinkaTerminationFlag != 0)
            {
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                return;
            }
            state.Program = AnimationProgramCursor<RinkaProgram>.Start(RinkaProgram.Special);
        }
        else
        {
            state.Program = AnimationProgramCursor<RinkaProgram>.Start(RinkaProgram.Ordinary);
        }
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
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
                state.DelayTimer = state.DelayTimer.Decrement().Remaining;
                if (!EnemyMath.IsNegative16(state.DelayTimer.RawValue))
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
                state.DelayTimer = state.DelayTimer.Decrement().Remaining;
                if (!EnemyMath.IsNegative16(state.DelayTimer.RawValue))
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
        state.DelayTimer = NativeCountdown.Start(1);
    }

    /// <summary>Runs Rinka's private frozen-AI prelude/tail around common frozen handling.</summary>
    internal bool RunRinkaFrozenTail(RoomEnemySlot slot)
    {
        if (RinkaIsOutsideFlightWindow(slot))
            slot.FrozenTimer = NativeCountdown.Start(0);
        if (_rinkaTerminationFlag == 0)
            return false;

        DecrementVisibleSpecialRinkaCount(slot);
        ReleaseSpecialRinkaSpawn(RequireRinkaState(slot));
        RunRinkaDeathAnimation(slot);
        return true;
    }

    /// <summary>Carries out one cue of the Rinkas' animations: hiding, hiding with off-screen processing, and firing.</summary>
    private RinkaProgram? ApplyRinkaCue(RoomEnemySlot slot, RinkaEnemyState state, RinkaCue cue)
    {
        switch (cue.Kind)
        {
            case RinkaCueKind.Hide:
                slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                break;
            case RinkaCueKind.HideAndProcessOffScreen:
                slot.Properties = slot.Properties.With(
                    EnemyProperties.ProcessOffScreen | EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                break;
            case RinkaCueKind.Fire:
                slot.Properties = slot.Properties.Without(EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
                state.Function = RinkaEnemyFunction.AimDelay;
                _rinkaActiveCount = unchecked((ushort)(_rinkaActiveCount + 1));
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(RinkaCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Releases a special spawn resource and restarts its life unless the room has terminated it.</summary>
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

    /// <summary>Finds the authored point for a population position, using native resource zero for an unknown point.</summary>
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

    /// <summary>Clears the occupied spawn entry identified by a special Rinka's even resource token.</summary>
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

    /// <summary>Decrements the active count only for visible special actors, saturating at zero.</summary>
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

    /// <summary>Distinguishes Mother Brain's parameterized spawn-sharing actors from ordinary room Rinkas.</summary>
    private static bool IsSpecialRinka(RoomEnemySlot slot) => slot.Parameter1 != 0;

    /// <summary>Requires an initialized per-slot Rinka state before running its lifecycle.</summary>
    private RinkaEnemyState RequireRinkaState(RoomEnemySlot slot) =>
        _rinkaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rinka state.");

    /// <summary>Runs the private lethal-health tail after common bomb damage without generic death.</summary>
    internal void RegisterRinkaBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.RinkaShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c => ResolveRinkaCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Installs Rinka initialization for both ordinary and special enemy slots.</summary>
    internal void RegisterRinkaInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Rinka, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeRinka(slot);
        });
    }

    /// <summary>Advances aiming, flight, viewport recycling, and the special respawn delay.</summary>
    internal void RegisterRinkaMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Rinka, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, (slot, frame, nmiFrameCounter8) =>
        {
            RunRinkaMain(slot, RequireRinkaState(slot), frame.Samus);
        });
    }

    /// <summary>Runs the Rinkas' animations as typed programs (#641).</summary>
    internal void RegisterRinkaTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireRinkaState(slot);
            AnimationProgramCursor<RinkaProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, RinkaAnimations.Programs, ref cursor, RinkaFrameDefinitions.Spritemap,
                    (cue, _) => ApplyRinkaCue(slot, state, cue));
            state.Program = cursor;
        });


    /// <summary>Skips invisible actors and applies the private death lifecycle after power-bomb damage.</summary>
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

    /// <summary>Applies the private death tail after projectile damage but not immediate freezing.</summary>
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

    /// <summary>Applies normal contact damage before the Rinka-specific lethal-health lifecycle.</summary>
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
    /// <summary>Room-graphics dust definition parameter used by special Rinka deaths.</summary>
    private const MiscDustAnimation RinkaDustAnimationIndex = MiscDustAnimation.SmallExplosion;
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
                ? unchecked((ushort)(slot.NativeIndex | KilledEnemyIndexMasks.RespawnMarker))
                : slot.NativeIndex;
            projectile.StartAnimation(new EnemyDeathProjectileAnimation(EnemyDeathProjectileProgram.SmallExplosion));
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

    /// <summary>Attaches the native camera cull to miscellaneous dust and explosion projectiles.</summary>
    internal static void RegisterMiscDustPreInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            CullMiscDustOutsideCamera(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MiscDust);
    }

    /// <summary>Runs the Rinka viewport and termination check before common frozen AI.</summary>
    internal void RegisterRinkaFrozenPreemption(EnemyAiTable table) =>
        table.AddFrozenPreemption(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rinka, RunRinkaFrozenTail);
}
