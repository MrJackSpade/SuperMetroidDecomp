using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Owns the four bank-$87 statue tile programs and the bank-$88 descending BG2
/// pre-instructions. Boss victories and grey-statue events are deliberately separate:
/// visiting the room must play each release before opening the elevator route.
/// </summary>
public sealed class TourianStatueSequence
{
    /// <summary>Address space passed to palette FX spawning while statue tile mechanics remain compiled.</summary>
    private readonly ISnesAddressSpace _addressSpace;
    /// <summary>Receives the black clear and installed grey-color palette writes from tile instructions.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Holds statue busy/released bits, effect spawning, vertical offset, and descent earthquake state.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Supplies equipped items to palette FX created during the release animation.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Spawns the access obstruction's clearing or opening PLM at the descent endpoint.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Identifies the statue room setup and controls its two logical scroll cells.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Feeds water height and authored earthquake sound into the descending statue effects.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Creates the palette effect selected by a statue animated-tile instruction.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Tracks boss and statue events, area boss bits, RNG, and the final Tourian unlock event.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>The unlock option skips tile playback and starts the statue at its completed offset.</summary>
    private readonly TesterTools _tester;
    /// <summary>Pauses quarter-pixel descent while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Queues the selected bank-$87 animated tile art for VRAM transfer.</summary>
    private readonly VramWriteQueue _vramWrites;

    /// <summary>Captures the room services needed to run the native tile and BG2 descent programs.</summary>
    internal TourianStatueSequence(TourianStatueSequenceDependencies dependencies)
    {
        _addressSpace = dependencies.AddressSpace;
        _cgram = dependencies.Cgram;
        _enemies = dependencies.Enemies;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _roomPaletteFx = dependencies.RoomPaletteFx;
        _system = dependencies.System;
        _tester = dependencies.Tester;
        _freeze = dependencies.Freeze;
        _vramWrites = dependencies.VramWrites;
    }

    /// <summary>One spawned statue animation's compiled program, next instruction pointer, and frame timer.</summary>
    internal sealed class TileObject
    {
        /// <summary>Program identity and installed transfer dimensions for this statue.</summary>
        public required TourianStatueAnimatedTileProgramDefinition Definition;
        /// <summary>Next bank-$87 mechanics word.</summary>
        public ushort Pointer;
        /// <summary>Remaining duration of the current tile frame.</summary>
        public NativeCountdown Timer = NativeCountdown.Start(1);
    }
    /// <summary>Kraid, Phantoon, Draygon, then Ridley order matches native slot traversal and busy-bit ownership.</summary>
    internal readonly List<TileObject> objects = [];
    /// <summary>Minus two waits for all grey events; nonnegative values count down the authored pre-descent hold.</summary>
    private int delay = -2;
    /// <summary>Accumulated positive 16.16 travel, exposed as a negative BG2 vertical offset.</summary>
    internal int descent;
    /// <summary>Whether the last loaded room uses <c>RunStatueUnlockingAnimations</c>; remains true after the descent finishes and is reset by the next <see cref="Load"/>.</summary>
    public bool Enabled { get; private set; }
    /// <summary>
    /// The whole word of native HDMAObject_Var1:Var0, which descends by $FFFF:C000 per
    /// frame. As a signed 16.16 value its whole word floors toward negative infinity, so
    /// the first quarter-pixel step already reads -1.
    /// </summary>
    public short VerticalOffset => unchecked((short)SnesSixteenSixteen.WholeOf(-descent));
    /// <summary>BG2 offset accepted with the same NMI as the displayed OBJ buffer.</summary>
    public short DisplayedVerticalOffset { get; private set; }
    /// <summary>Captures the current BG2 position for presentation with the corresponding NMI OBJ buffer.</summary>
    internal void LatchDisplay() => DisplayedVerticalOffset = VerticalOffset;

    /// <summary>Resets room-local animation, delay, and displayed descent state, enabling the four statue tile programs only for the statue-unlocking room setup.</summary>
    /// <remarks>
    /// A prior unlock event or host unlock option starts at the fully descended offset, clears the access obstruction through its PLM, and enables vertical scrolling.
    /// Otherwise the sequence starts raised and constrains scrolling until the authored release and descent finish. Disabled rooms retain no tile objects.
    /// </remarks>
    public void Load()
    {
        objects.Clear();
        delay = -2;
        descent = 0;
        DisplayedVerticalOffset = 0;
        Enabled = _room.ActiveRoom?.State.SetupCallback == RoomSetupCallback.RunStatueUnlockingAnimations;
        if (!Enabled) return;
        SpawnAnimatedObjects();
        if (_tester.UnlockTourianEnabled || _system.HasEvent(EventNumber.TourianUnlocked))
        {
            descent = SnesSixteenSixteen.FromWhole(TourianStatueRomData.DescentDistance);
            _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueVerticalOffset = VerticalOffset;
            _plms.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, clear: true);
            EnableScrolling();
        }
        else
        {
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
            _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 1), RoomScrollState.RedBoundary);
        }
    }

    /// <summary>
    /// Translates the four LDY/JSL calls at $8F:91D7..91F2 directly. Native allocation
    /// and handler traversal both descend through slots, so insertion order also
    /// preserves execution order and which statue can acquire the shared busy bit.
    /// </summary>
    internal void SpawnAnimatedObjects()
    {
        Spawn(AnimatedTileObject.TourianStatueKraid);
        Spawn(AnimatedTileObject.TourianStatuePhantoon);
        Spawn(AnimatedTileObject.TourianStatueDraygon);
        Spawn(AnimatedTileObject.TourianStatueRidley);

        // Resolve each native object header and start it at its compiled program entry.
        void Spawn(AnimatedTileObject objectPointer)
        {
            if (!TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader(
                    objectPointer, out TourianStatueAnimatedTileProgramDefinition? definition))
                throw new InvalidDataException(
                    $"Tourian statue animated-tile object $87:{(int)objectPointer:X4} is not cataloged.");

            objects.Add(new() { Definition = definition, Pointer = definition.ProgramStart });
        }
    }

    /// <summary>Runs AnimtilesHandler in native spawn order, retaining bytecode durations and branches.</summary>
    [PpuLayoutBoundary("codec: animated-tile instruction CGRAM index operand")]
    public void StepTiles()
    {
        if (!Enabled) return;
        if (_tester.UnlockTourianEnabled) return;
        var bus = _addressSpace;
        foreach (TileObject tile in objects)
        {
            if (tile.Pointer == 0 || !(tile.Timer = tile.Timer.Decrement().Remaining).IsZero) continue;
            for (int guard = 0; ; guard++)
            {
                if (guard == 64) throw new InvalidDataException("Statue animated tile instruction loop exceeded its bound.");
                ushort code = MechanicsWord(tile, tile.Pointer);
                int operand = tile.Pointer + 2;
                if (code < 0x8000)
                {
                    if (code == 0) throw new InvalidDataException("Zero-duration statue tile frame.");
                    tile.Timer = NativeCountdown.FromRaw(code);
                    int sourceAddress = TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(
                        tile.Definition, unchecked((ushort)operand));
                    _vramWrites.Enqueue(
                        tile.Definition.TransferByteCount,
                        sourceAddress,
                        tile.Definition.Destination);
                    tile.Pointer += 4;
                    break;
                }
                switch (ClosedNativeWords.Decode<AnimatedTileInstruction>(code, "animated-tile instruction"))
                {
                    case AnimatedTileInstruction.Delete:
                        tile.Pointer = 0;
                        break;
                    case AnimatedTileInstruction.Goto:
                        tile.Pointer = MechanicsWord(tile, operand);
                        continue;
                    case AnimatedTileInstruction.GotoIfEventSet:
                        tile.Pointer = _system.HasEventRaw(MechanicsWord(tile, operand))
                            ? MechanicsWord(tile, operand + 2)
                            : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstruction.GotoIfAnyBossBitsSetForArea:
                        ushort bossTest = MechanicsWord(tile, operand);
                        tile.Pointer = _system.HasAnyBossBits(
                                SnesLittleEndian.HighByte(bossTest), (BossBits)(SnesLittleEndian.LowByte(bossTest)))
                            ? MechanicsWord(tile, operand + 2)
                            : (ushort)(operand + 4);
                        continue;
                    case AnimatedTileInstruction.SetEvent:
                        _system.SetEventRaw(MechanicsWord(tile, operand));
                        break;
                    case AnimatedTileInstruction.GotoIfTourianStatueBusy:
                        tile.Pointer = (_enemies.TourianEntranceStatueBehavior.TourianEntranceStatueAnimationState & TourianStatueRomData.Busy) != 0
                            ? MechanicsWord(tile, operand)
                            : (ushort)(operand + 2);
                        continue;
                    case AnimatedTileInstruction.SetTourianStatueAnimationState:
                        _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueAnimationState |= MechanicsWord(tile, operand);
                        break;
                    case AnimatedTileInstruction.ResetTourianStatueAnimationState:
                        _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueAnimationState &=
                            (ushort)~MechanicsWord(tile, operand);
                        break;
                    case AnimatedTileInstruction.ClearThreePaletteColors:
                        ushort clearPaletteByteIndex = MechanicsWord(tile, operand);
                        // The colour index is animated-tile instruction data.
                        new CgramPalettes(_cgram).All.Slice(clearPaletteByteIndex / 2, 3).Fill(Bgr555.Black);
                        break;
                    case AnimatedTileInstruction.WriteEightTargetPaletteColors:
                        int greyDestination = MechanicsWord(tile, operand) / 2;
                        (_enemies.TileArtwork?.TourianStatueColors ?? throw new InvalidOperationException(
                            "Tourian statue animation requires installed grey colors."))
                            .ApplyGrey(_cgram, greyDestination);
                        break;
                    case AnimatedTileInstruction.SpawnPaletteFxObject:
                        _roomPaletteFx.SpawnDefinition(
                            bus, MechanicsWord(tile, operand), _player.IntroducedSamus.EquippedItems);
                        break;
                    case AnimatedTileInstruction.SpawnTourianStatueEyeGlow:
                    case AnimatedTileInstruction.SpawnTourianStatueSoul:
                        _enemies.TourianUnlockBehavior.SpawnTourianUnlockEffect(
                            MechanicsWord(tile, operand),
                            code == (ushort)AnimatedTileInstruction.SpawnTourianStatueSoul);
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
    public void StepDescent()
    {
        if (!Enabled) return;
        _enemies.TourianUnlockBehavior.TourianStatueWaterY = _roomLayer3Fx.CurrentYPosition;
        if (_tester.UnlockTourianEnabled || _system.HasEvent(EventNumber.TourianUnlocked)) { EnableScrolling(); return; }
        if (delay == -2)
        {
            if (!_system.HasEvent(EventNumber.PhantoonStatueGrey) || !_system.HasEvent(EventNumber.RidleyStatueGrey)
                || !_system.HasEvent(EventNumber.DraygonStatueGrey) || !_system.HasEvent(EventNumber.KraidStatueGrey)) return;
            _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueAnimationState |= TourianStatueRomData.AllReleased;
            if ((_enemies.TourianEntranceStatueBehavior.TourianEntranceStatueAnimationState & TourianStatueRomData.Busy) != 0) return;
            delay = TourianStatueRomData.DescentDelay;
            return;
        }
        _enemies.Earthquake.EarthquakeType = TourianStatueRomData.DescentEarthquakeType;
        _enemies.Earthquake.AddTimerBits(TourianStatueRomData.DescentEarthquakeTimer);
        _roomLayer3Fx.PublishStatueEarthquakeSound(
            _system.RandomNumber, _system.MainGameLoopCarry);
        if (delay >= 0)
        {
            if (--delay < 0) _enemies.TourianUnlockBehavior.SpawnTourianDescentDust();
            return;
        }
        if (_freeze.TimeIsFrozen) return;
        descent += TourianStatueRomData.DescentStep;
        _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueVerticalOffset = VerticalOffset;
        // $88:DC90 compares the whole word against $FF10, which it first reaches
        // three quarter-steps before a full 240 pixels.
        if (VerticalOffset == -TourianStatueRomData.DescentDistance)
        {
            _plms.Tourian.TrySpawnTourianAccess(_room.LoadedLevelData, clear: false);
            _system.SetEvent(EventNumber.TourianUnlocked);
        }
    }

    /// <summary>Marks the entrance statue finished and makes both access-room scroll cells freely traversable.</summary>
    private void EnableScrolling()
    {
        _enemies.TourianEntranceStatueBehavior.TourianEntranceStatueFinished = true;
        _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 0), RoomScrollState.Green);
        _room.LoadedCamera.Scrolls.SetLogicalState(new(0, 1), RoomScrollState.Green);
    }
    /// <summary>Reads an owned bank-$87 control word from the object's compiled program and rejects uncataloged addresses.</summary>
    private static ushort MechanicsWord(TileObject tile, int pointer)
    {
        ushort bankPointer = unchecked((ushort)pointer);
        if (tile.Definition.TryReadMechanicsWord(bankPointer, out ushort value))
            return value;

        throw new InvalidDataException(
            $"Tourian statue object $87:{(int)tile.Definition.ObjectPointer:X4} reached " +
            $"uncataloged mechanics word $87:{bankPointer:X4}.");
    }
}
