using SuperMetroid.Core.Assets;
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
        public required TourianStatueAnimatedTileProgramDefinition Definition;
        public ushort Pointer, Timer = 1;
    }
    private readonly List<TileObject> objects = [];
    private int delay = -2;
    private int descent;
    public bool Enabled { get; private set; }
    /// <summary>
    /// The whole word of native HDMAObject_Var1:Var0, which descends by $FFFF:C000 per
    /// frame. As a signed 16.16 value its whole word floors toward negative infinity, so
    /// the first quarter-pixel step already reads -1.
    /// </summary>
    public short VerticalOffset => unchecked((short)((-descent) >> 16));
    /// <summary>BG2 offset accepted with the same NMI as the displayed OBJ buffer.</summary>
    public short DisplayedVerticalOffset { get; private set; }
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    public void Load(SuperMetroidRuntime runtime)
    {
        objects.Clear();
        delay = -2;
        descent = 0;
        DisplayedVerticalOffset = 0;
        Enabled = runtime.ActiveRoom?.State.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        SpawnAnimatedObjects();
        if (runtime.UnlockTourianEnabled || runtime.System.HasEvent(EventNumber.TourianUnlocked))
        {
            descent = TourianStatueRomData.DescentDistance << 16;
            runtime.Enemies.TourianEntranceStatueVerticalOffset = VerticalOffset;
            runtime.Plms.TrySpawnTourianAccess(runtime.LevelData!, clear: true);
            EnableScrolling(runtime);
        }
        else
        {
            runtime.Camera!.Scrolls.SetLogicalState(0, 0, RoomScrollState.Blue);
            runtime.Camera.Scrolls.SetLogicalState(0, 1, RoomScrollState.RedBoundary);
        }
    }

    /// <summary>
    /// Translates the four LDY/JSL calls at $8F:91D7..91F2 directly. Native allocation
    /// and handler traversal both descend through slots, so insertion order also
    /// preserves execution order and which statue can acquire the shared busy bit.
    /// </summary>
    private void SpawnAnimatedObjects()
    {
        Spawn(AnimatedTileObjectPointers.TourianStatueKraid);
        Spawn(AnimatedTileObjectPointers.TourianStatuePhantoon);
        Spawn(AnimatedTileObjectPointers.TourianStatueDraygon);
        Spawn(AnimatedTileObjectPointers.TourianStatueRidley);

        void Spawn(ushort objectPointer)
        {
            if (!TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(
                    objectPointer, out TourianStatueAnimatedTileProgramDefinition definition))
                throw new InvalidDataException(
                    $"Tourian statue animated-tile object $87:{objectPointer:X4} is not cataloged.");

            objects.Add(new() { Definition = definition, Pointer = definition.ProgramStart });
        }
    }

    /// <summary>Runs AnimtilesHandler in native spawn order, retaining bytecode durations and branches.</summary>
    public void StepTiles(SuperMetroidRuntime runtime)
    {
        if (!Enabled) return;
        if (runtime.UnlockTourianEnabled) return;
        var bus = runtime.AddressSpace;
        foreach (TileObject tile in objects)
        {
            if (tile.Pointer == 0 || --tile.Timer != 0) continue;
            for (int guard = 0; ; guard++)
            {
                if (guard == 64) throw new InvalidDataException("Statue animated tile instruction loop exceeded its bound.");
                ushort code = MechanicsWord(tile, tile.Pointer);
                int operand = tile.Pointer + 2;
                if (code < 0x8000)
                {
                    if (code == 0) throw new InvalidDataException("Zero-duration statue tile frame.");
                    tile.Timer = code;
                    int sourceAddress = TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(
                        tile.Definition, unchecked((ushort)operand));
                    runtime.VramWrites.Enqueue(
                        tile.Definition.TransferByteCount,
                        sourceAddress,
                        tile.Definition.EncodedVramDestination);
                    tile.Pointer += 4;
                    break;
                }
                switch (code)
                {
                    case AnimatedTileInstructionCodes.Delete:
                        tile.Pointer = 0;
                        break;
                    case AnimatedTileInstructionCodes.Goto:
                        tile.Pointer = MechanicsWord(tile, operand);
                        continue;
                    case AnimatedTileInstructionCodes.GotoIfEventSet:
                        tile.Pointer = runtime.System.HasEventRaw(MechanicsWord(tile, operand))
                            ? MechanicsWord(tile, operand + 2)
                            : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstructionCodes.GotoIfAnyBossBitsSetForArea:
                        ushort bossTest = MechanicsWord(tile, operand);
                        tile.Pointer = runtime.System.HasAnyBossBits(
                                bossTest >> 8, (BossBits)(bossTest & 255))
                            ? MechanicsWord(tile, operand + 2)
                            : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstructionCodes.SetEvent:
                        runtime.System.SetEventRaw(MechanicsWord(tile, operand));
                        break;
                    case AnimatedTileInstructionCodes.GotoIfTourianStatueBusy:
                        tile.Pointer = (runtime.Enemies.TourianEntranceStatueAnimationState & TourianStatueRomData.Busy) != 0
                            ? MechanicsWord(tile, operand)
                            : (ushort)(operand + 2);
                        continue;
                    case AnimatedTileInstructionCodes.SetTourianStatueAnimationState:
                        runtime.Enemies.TourianEntranceStatueAnimationState |= MechanicsWord(tile, operand);
                        break;
                    case AnimatedTileInstructionCodes.ResetTourianStatueAnimationState:
                        runtime.Enemies.TourianEntranceStatueAnimationState &=
                            (ushort)~MechanicsWord(tile, operand);
                        break;
                    case AnimatedTileInstructionCodes.ClearThreePaletteColors:
                        ushort clearPaletteByteIndex = MechanicsWord(tile, operand);
                        for (int color = 0; color < 3; color++)
                            runtime.Cgram.SetColor(clearPaletteByteIndex / 2 + color, 0);
                        break;
                    case AnimatedTileInstructionCodes.WriteEightTargetPaletteColors:
                        int greyDestination = MechanicsWord(tile, operand) / 2;
                        (runtime.Enemies.TileArtwork?.TourianStatueColors ?? throw new InvalidOperationException(
                            "Tourian statue animation requires installed grey colors."))
                            .ApplyGrey(runtime.Cgram, greyDestination);
                        break;
                    case AnimatedTileInstructionCodes.SpawnPaletteFxObject:
                        runtime.RoomPaletteFx.SpawnDefinition(
                            bus, MechanicsWord(tile, operand), runtime.Samus!.EquippedItems);
                        break;
                    case AnimatedTileInstructionCodes.SpawnTourianStatueEyeGlow:
                    case AnimatedTileInstructionCodes.SpawnTourianStatueSoul:
                        runtime.Enemies.SpawnTourianUnlockEffect(
                            MechanicsWord(tile, operand),
                            code == AnimatedTileInstructionCodes.SpawnTourianStatueSoul);
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
        if (runtime.UnlockTourianEnabled || runtime.System.HasEvent(EventNumber.TourianUnlocked)) { EnableScrolling(runtime); return; }
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
        runtime.RoomLayer3Fx.PublishStatueEarthquakeSound(
            runtime.System.RandomNumber, runtime.System.MainGameLoopCarry);
        if (delay >= 0)
        {
            if (--delay < 0) runtime.Enemies.SpawnTourianDescentDust();
            return;
        }
        if (runtime.TimeIsFrozen) return;
        descent += TourianStatueRomData.DescentStep;
        runtime.Enemies.TourianEntranceStatueVerticalOffset = VerticalOffset;
        // $88:DC90 compares the whole word against $FF10, which it first reaches
        // three quarter-steps before a full 240 pixels.
        if (VerticalOffset == -TourianStatueRomData.DescentDistance)
        {
            runtime.Plms.TrySpawnTourianAccess(runtime.LevelData!, clear: false);
            runtime.System.SetEvent(EventNumber.TourianUnlocked);
        }
    }

    private static void EnableScrolling(SuperMetroidRuntime runtime)
    {
        runtime.Enemies.TourianEntranceStatueFinished = true;
        runtime.Camera!.Scrolls.SetLogicalState(0, 0, RoomScrollState.Green);
        runtime.Camera.Scrolls.SetLogicalState(0, 1, RoomScrollState.Green);
    }
    private static ushort MechanicsWord(TileObject tile, int pointer)
    {
        ushort bankPointer = unchecked((ushort)pointer);
        if (tile.Definition.TryReadMechanicsWord(bankPointer, out ushort value))
            return value;

        throw new InvalidDataException(
            $"Tourian statue object $87:{tile.Definition.ObjectPointer:X4} reached " +
            $"uncataloged mechanics word $87:{bankPointer:X4}.");
    }
}
