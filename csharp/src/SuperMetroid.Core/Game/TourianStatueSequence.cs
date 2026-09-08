using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Owns the four bank-$87 statue tile programs and the bank-$88 descending BG2
/// pre-instructions. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
public sealed class TourianStatueSequence
{
    private sealed class TileObject
    {
        public ushort Pointer, Timer = 1, Size, Destination;
    }
    private readonly List<TileObject> objects = [];
    private int delay = -2;
    private int descent;
    public bool Enabled { get; private set; }
    public short VerticalOffset => unchecked((short)-(descent >> 16));
    /// <summary>BG2 offset accepted with the same NMI as the displayed OBJ buffer.</summary>
    public short DisplayedVerticalOffset { get; private set; }
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    public void Load(SuperMetroidRuntime runtime)
    {
        objects.Clear();
        delay = -2;
        descent = 0;
        DisplayedVerticalOffset = 0;
        Enabled = runtime.ActiveRoom?.State.SetupCodePointer == RoomSetupCodePointers.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        foreach (ushort definition in TourianStatueRomData.AnimatedObjects)
            objects.Add(new() { Pointer = Word(runtime.AddressSpace, definition),
                Size = Word(runtime.AddressSpace, definition + 2), Destination = Word(runtime.AddressSpace, definition + 4) });
        if (runtime.System.HasEvent(EventNumber.TourianUnlocked))
        {
            descent = TourianStatueRomData.DescentDistance << 16;
            runtime.Enemies.TourianEntranceStatueVerticalOffset = VerticalOffset;
            runtime.Plms.TrySpawnTourianAccess(runtime.AddressSpace, runtime.LevelData!, clear: true);
            EnableScrolling(runtime);
        }
        else
        {
            runtime.Camera!.Scrolls.SetLogicalState(0, 0, RoomScrollState.Blue);
            runtime.Camera.Scrolls.SetLogicalState(0, 1, RoomScrollState.RedBoundary);
        }
    }

    /// <summary>Runs AnimtilesHandler in native spawn order, retaining bytecode durations and branches.</summary>
    public void StepTiles(SuperMetroidRuntime runtime)
    {
        if (!Enabled) return;
        var bus = runtime.AddressSpace;
        foreach (TileObject tile in objects)
        {
            if (tile.Pointer == 0 || --tile.Timer != 0) continue;
            for (int guard = 0; ; guard++)
            {
                if (guard == 64) throw new InvalidDataException("Statue animated tile instruction loop exceeded its bound.");
                ushort code = Word(bus, tile.Pointer);
                int operand = tile.Pointer + 2;
                ushort value = Word(bus, operand);
                if (code < 0x8000)
                {
                    if (code == 0) throw new InvalidDataException("Zero-duration statue tile frame.");
                    tile.Timer = code;
                    runtime.VramWrites.Enqueue(tile.Size, 0x870000 | value, tile.Destination);
                    tile.Pointer += 4;
                    break;
                }
                switch (code)
                {
                    case AnimatedTileInstructionCodes.Delete:
                        tile.Pointer = 0;
                        break;
                    case AnimatedTileInstructionCodes.Goto:
                        tile.Pointer = value;
                        continue;
                    case AnimatedTileInstructionCodes.GotoIfEventSet:
                        tile.Pointer = runtime.System.HasEventRaw(value) ? Word(bus, operand + 2) : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstructionCodes.GotoIfAnyBossBitsSetForArea:
                        tile.Pointer = runtime.System.HasAnyBossBits(value >> 8, (BossBits)(value & 255))
                            ? Word(bus, operand + 2) : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstructionCodes.SetEvent:
                        runtime.System.SetEventRaw(value);
                        break;
                    case AnimatedTileInstructionCodes.GotoIfTourianStatueBusy:
                        tile.Pointer = (runtime.Enemies.TourianEntranceStatueAnimationState & TourianStatueRomData.Busy) != 0
                            ? value : (ushort)(operand + 2);
                        continue;
                    case AnimatedTileInstructionCodes.SetTourianStatueAnimationState:
                        runtime.Enemies.TourianEntranceStatueAnimationState |= value;
                        break;
                    case AnimatedTileInstructionCodes.ResetTourianStatueAnimationState:
                        runtime.Enemies.TourianEntranceStatueAnimationState &= (ushort)~value;
                        break;
                    case AnimatedTileInstructionCodes.ClearThreePaletteColors:
                        for (int color = 0; color < 3; color++) runtime.Cgram.SetColor(value / 2 + color, 0);
                        break;
                    case AnimatedTileInstructionCodes.WriteEightTargetPaletteColors:
                        runtime.Cgram.LoadFromBus(bus, TourianStatueRomData.GreyColors, 8, value / 2);
                        break;
                    case AnimatedTileInstructionCodes.SpawnPaletteFxObject:
                        runtime.RoomPaletteFx.SpawnDefinition(bus, value, runtime.Samus!.EquippedItems);
                        break;
                    case AnimatedTileInstructionCodes.SpawnTourianStatueEyeGlow:
                    case AnimatedTileInstructionCodes.SpawnTourianStatueSoul:
                        runtime.Enemies.SpawnTourianUnlockEffect(value, code == AnimatedTileInstructionCodes.SpawnTourianStatueSoul);
                        break;
                    default:
                        throw new NotSupportedException($"Statue animated tiles instruction $87:{code:X4} is not translated.");
                }
                if (tile.Pointer == 0) break;
                tile.Pointer = (ushort)(operand + 2);
            }
        }
    }

    /// <summary>$88:DBD7/DC23/DC69/DCBA, including the authored delay and quarter-pixel descent.</summary>
    public void StepDescent(SuperMetroidRuntime runtime)
    {
        if (!Enabled) return;
        runtime.Enemies.TourianStatueWaterY = runtime.RoomLayer3Fx.CurrentYPosition;
        if (runtime.System.HasEvent(EventNumber.TourianUnlocked)) { EnableScrolling(runtime); return; }
        if (delay == -2)
        {
            if (!runtime.System.HasEvent(EventNumber.PhantoonStatueGrey) || !runtime.System.HasEvent(EventNumber.RidleyStatueGrey)
                || !runtime.System.HasEvent(EventNumber.DraygonStatueGrey) || !runtime.System.HasEvent(EventNumber.KraidStatueGrey)) return;
            runtime.Enemies.TourianEntranceStatueAnimationState |= TourianStatueRomData.AllReleased;
            if ((runtime.Enemies.TourianEntranceStatueAnimationState & TourianStatueRomData.Busy) != 0) return;
            delay = TourianStatueRomData.DescentDelay;
            return;
        }
        runtime.Enemies.EarthquakeType = TourianStatueRomData.DescentEarthquakeType;
        runtime.Enemies.EarthquakeTimer |= TourianStatueRomData.DescentEarthquakeTimer;
        runtime.RoomLayer3Fx.PublishStatueEarthquakeSound(runtime.System.RandomNumber);
        if (delay >= 0)
        {
            if (--delay < 0) runtime.Enemies.SpawnTourianDescentDust();
            return;
        }
        if (runtime.TimeIsFrozen) return;
        descent += TourianStatueRomData.DescentStep;
        runtime.Enemies.TourianEntranceStatueVerticalOffset = VerticalOffset;
        if (descent == TourianStatueRomData.DescentDistance << 16)
        {
            runtime.Plms.TrySpawnTourianAccess(runtime.AddressSpace, runtime.LevelData!, clear: false);
            runtime.System.SetEvent(EventNumber.TourianUnlocked);
        }
    }

    private static void EnableScrolling(SuperMetroidRuntime runtime)
    {
        runtime.Enemies.TourianEntranceStatueFinished = true;
        runtime.Camera!.Scrolls.SetLogicalState(0, 0, RoomScrollState.Green);
        runtime.Camera.Scrolls.SetLogicalState(0, 1, RoomScrollState.Green);
    }
    private static ushort Word(ISnesAddressSpace bus, int pointer) =>
        (ushort)(bus.ReadByte(0x870000 | pointer) | bus.ReadByte(0x870000 | (pointer + 1)) << 8);
}
