using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Lossless cartridge view of bank $8F's eleven-byte room header and its selected
/// twenty-six-byte room-state record.
/// </summary>
/// <remarks>
/// The engine calls <c>HandleRoomDefStateSelect</c> at $8F:E5D2 immediately after reading
/// the fixed header. State selection is data-driven and can depend on events, bosses, or
/// inventory. This first reusable loader accepts those facts explicitly; a fresh Ceres
/// start supplies an all-clear context and therefore follows the exact default branch.
/// </remarks>
public sealed record CartridgeRoomHeader(
    ushort Pointer,
    byte RoomIndex,
    AreaId AreaIndex,
    byte MapX,
    byte MapY,
    byte WidthInScreens,
    byte HeightInScreens,
    byte UpScroller,
    byte DownScroller,
    byte CreBitset,
    ushort DoorListPointer,
    CartridgeRoomState State)
{
    private const int RoomBank = 0x8f0000;

    /// <summary>Logical area/room pair, kept separate from <see cref="Pointer"/>.</summary>
    public RoomIdentity Identity => new(AreaIndex, RoomIndex);

    /// <summary>Reads a room and resolves the same selector bytecode consumed by $8F:E5D2.</summary>
    public static CartridgeRoomHeader Load(
        ISnesAddressSpace bus,
        ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = RoomBank | roomPointer;
        ushort statePointer = SelectState(
            bus,
            roomPointer,
            unchecked((ushort)(roomPointer + 11)),
            selection);
        return new CartridgeRoomHeader(
            roomPointer,
            RoomIndex: bus.ReadByte(address),
            AreaIndex: AreaIds.FromCartridge(
                bus.ReadByte(address + 1),
                $"Room header $8F:{roomPointer:X4}"),
            MapX: bus.ReadByte(address + 2),
            MapY: bus.ReadByte(address + 3),
            WidthInScreens: bus.ReadByte(address + 4),
            HeightInScreens: bus.ReadByte(address + 5),
            UpScroller: bus.ReadByte(address + 6),
            DownScroller: bus.ReadByte(address + 7),
            CreBitset: bus.ReadByte(address + 8),
            DoorListPointer: ReadWord(bus, address + 9),
            State: CartridgeRoomState.Load(bus, statePointer));
    }

    private static ushort SelectState(
        ISnesAddressSpace bus,
        ushort roomPointer,
        ushort selectorPointer,
        RoomStateSelectionContext selection)
    {
        ushort cursor = selectorPointer;
        for (int guard = 0; guard < 32; guard++)
        {
            ushort selector = ReadWord(bus, RoomBank | cursor);
            cursor += 2;
            switch (selector)
            {
                case RoomStateSelectorCodes.Finish:
                    // Finish receives the byte immediately after its own function word;
                    // that byte is the first byte of the inline default state record.
                    return cursor;

                case RoomStateSelectorCodes.MainAreaBossIsDead:
                {
                    ushort selectedPointer = ReadWord(bus, RoomBank | cursor);
                    if (selection.IsBossDead(RoomStateSelectorOperands.MainAreaBossMask))
                        return selectedPointer;
                    cursor += 2;
                    break;
                }

                case RoomStateSelectorCodes.EventHasBeenSet:
                {
                    byte eventIndex = bus.ReadByte(RoomBank | cursor);
                    ushort selectedPointer = ReadWord(bus, RoomBank | unchecked((ushort)(cursor + 1)));
                    if (selection.IsEventSet(eventIndex))
                        return selectedPointer;
                    cursor += 3;
                    break;
                }

                case RoomStateSelectorCodes.BossIsDead:
                {
                    byte bossMask = bus.ReadByte(RoomBank | cursor);
                    ushort selectedPointer = ReadWord(bus, RoomBank | unchecked((ushort)(cursor + 1)));
                    if (selection.IsBossDead(bossMask))
                        return selectedPointer;
                    cursor += 3;
                    break;
                }

                case RoomStateSelectorCodes.MorphBallAndMissiles:
                {
                    ushort selectedPointer = ReadWord(bus, RoomBank | cursor);
                    if (selection.HasMorphBallAndMissiles)
                        return selectedPointer;
                    cursor += 2;
                    break;
                }

                case RoomStateSelectorCodes.PowerBombs:
                {
                    ushort selectedPointer = ReadWord(bus, RoomBank | cursor);
                    if (selection.HasPowerBombs)
                        return selectedPointer;
                    cursor += 2;
                    break;
                }

                case RoomStateSelectorCodes.UnusedDoor:
                case RoomStateSelectorCodes.UnusedMorphBall:
                    throw new NotSupportedException(
                        $"Known unused room-state selector $8F:{selector:X4} is not translated " +
                        $"while reading room $8F:{roomPointer:X4}.");

                default:
                    throw new InvalidDataException(
                        $"Unknown room-state selector $8F:{selector:X4} while reading room $8F:{roomPointer:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Room $8F:{roomPointer:X4} did not terminate its state-selector list.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);
}

/// <summary>The fields copied by <c>LoadStateHeader</c> at $82:DEF2.</summary>
public sealed record CartridgeRoomState(
    ushort Pointer,
    int CompressedLevelDataAddress,
    byte GraphicsSet,
    byte MusicDataIndex,
    byte MusicTrackIndex,
    ushort FxPointer,
    ushort EnemyPopulationPointer,
    ushort EnemyTilesetPointer,
    byte Layer2ScrollX,
    byte Layer2ScrollY,
    ushort ScrollPointer,
    ushort XrayPointer,
    ushort MainCodePointer,
    ushort PlmPointer,
    ushort BackgroundDataPointer,
    ushort SetupCodePointer)
{
    private const int RoomBank = 0x8f0000;

    internal static CartridgeRoomState Load(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomBank | pointer;
        ushort packedLayer2Scrolls = RomDataReader.ReadWordFixedBank(bus, address + 12);
        return new CartridgeRoomState(
            pointer,
            CompressedLevelDataAddress: RomDataReader.ReadLongFixedBank(bus, address),
            GraphicsSet: bus.ReadByte(address + 3),
            MusicDataIndex: bus.ReadByte(address + 4),
            MusicTrackIndex: bus.ReadByte(address + 5),
            FxPointer: RomDataReader.ReadWordFixedBank(bus, address + 6),
            EnemyPopulationPointer: RomDataReader.ReadWordFixedBank(bus, address + 8),
            EnemyTilesetPointer: RomDataReader.ReadWordFixedBank(bus, address + 10),
            Layer2ScrollX: (byte)packedLayer2Scrolls,
            Layer2ScrollY: (byte)(packedLayer2Scrolls >> 8),
            ScrollPointer: RomDataReader.ReadWordFixedBank(bus, address + 14),
            XrayPointer: RomDataReader.ReadWordFixedBank(bus, address + 16),
            MainCodePointer: RomDataReader.ReadWordFixedBank(bus, address + 18),
            PlmPointer: RomDataReader.ReadWordFixedBank(bus, address + 20),
            BackgroundDataPointer: RomDataReader.ReadWordFixedBank(bus, address + 22),
            SetupCodePointer: RomDataReader.ReadWordFixedBank(bus, address + 24));
    }
}

/// <summary>Facts inspected by the cartridge's room-state selector functions.</summary>
public readonly record struct RoomStateSelectionContext(
    ReadOnlyMemory<byte> Events,
    ushort BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    public bool IsEventSet(byte eventIndex)
    {
        ReadOnlySpan<byte> events = Events.Span;
        int byteIndex = eventIndex >> 3;
        return byteIndex < events.Length && (events[byteIndex] & (1 << (eventIndex & 7))) != 0;
    }

    public bool IsBossDead(byte mask) => (BossBits & mask) != 0;
}
