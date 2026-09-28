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
    /// <summary>Logical area/room pair, kept separate from <see cref="Pointer"/>.</summary>
    public RoomIdentity Identity => new(AreaIndex, RoomIndex);

    /// <summary>Reads a room and resolves the same selector bytecode consumed by $8F:E5D2.</summary>
    public static CartridgeRoomHeader Load(
        IImportCartridgeSource cartridge,
        ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        ushort statePointer = SelectState(
            cartridge,
            roomPointer,
            unchecked((ushort)(roomPointer + RoomHeaderRomData.FixedHeaderByteCount)),
            selection);
        return LoadSelectedFromCartridgeHeader(cartridge, roomPointer, statePointer);
    }

    /// <summary>Builds a room entirely from compiled fixed metadata, selection, and state payloads.</summary>
    public static CartridgeRoomHeader LoadUsingCompiledSelection(
        ushort roomPointer,
        RoomStateSelectionContext selection = default)
    {
        RoomHeaderDefinition header = RoomHeaderDefinitions.Get(roomPointer);
        ushort statePointer = RoomStateSelectionDefinitions.Select(roomPointer, selection);
        return new CartridgeRoomHeader(
            header.Pointer,
            header.RoomIndex,
            header.AreaIndex,
            header.MapX,
            header.MapY,
            header.WidthInScreens,
            header.HeightInScreens,
            header.UpScroller,
            header.DownScroller,
            header.CreBitset,
            header.DoorListPointer,
            RoomStateDefinitions.Get(statePointer));
    }

    /// <summary>Returns a compiled fixed header's area without reading native room data.</summary>
    public static AreaId ReadAreaIndex(ushort roomPointer) =>
        RoomHeaderDefinitions.Get(roomPointer).AreaIndex;

    private static CartridgeRoomHeader LoadSelectedFromCartridgeHeader(
        IImportCartridgeSource cartridge,
        ushort roomPointer,
        ushort statePointer)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        int address = RoomHeaderRomData.BankAddress | roomPointer;
        return new CartridgeRoomHeader(
            roomPointer,
            RoomIndex: cartridge.ReadCartridgeByte(address),
            AreaIndex: AreaIds.FromCartridge(
                cartridge.ReadCartridgeByte(address + 1),
                $"Room header $8F:{roomPointer:X4}"),
            MapX: cartridge.ReadCartridgeByte(address + 2),
            MapY: cartridge.ReadCartridgeByte(address + 3),
            WidthInScreens: cartridge.ReadCartridgeByte(address + 4),
            HeightInScreens: cartridge.ReadCartridgeByte(address + 5),
            UpScroller: cartridge.ReadCartridgeByte(address + 6),
            DownScroller: cartridge.ReadCartridgeByte(address + 7),
            CreBitset: cartridge.ReadCartridgeByte(address + 8),
            DoorListPointer: CartridgeImportWords.ReadWord(cartridge, address + 9),
            State: CartridgeRoomState.Load(cartridge, statePointer));
    }

    private static ushort SelectState(
        IImportCartridgeSource cartridge,
        ushort roomPointer,
        ushort selectorPointer,
        RoomStateSelectionContext selection)
    {
        ushort cursor = selectorPointer;
        for (int guard = 0; guard < 32; guard++)
        {
            ushort selector = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | cursor);
            cursor += 2;
            switch (selector)
            {
                case RoomStateSelectorCodes.Finish:
                    // Finish receives the byte immediately after its own function word;
                    // that byte is the first byte of the inline default state record.
                    return cursor;

                case RoomStateSelectorCodes.MainAreaBossIsDead:
                {
                    ushort selectedPointer = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | cursor);
                    if (selection.IsBossDead(RoomStateSelectorOperands.MainAreaBoss))
                        return selectedPointer;
                    cursor += 2;
                    break;
                }

                case RoomStateSelectorCodes.EventHasBeenSet:
                {
                    byte eventIndex = cartridge.ReadCartridgeByte(RoomHeaderRomData.BankAddress | cursor);
                    ushort selectedPointer = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | unchecked((ushort)(cursor + 1)));
                    if (selection.IsEventSet(eventIndex))
                        return selectedPointer;
                    cursor += 3;
                    break;
                }

                case RoomStateSelectorCodes.BossIsDead:
                {
                    BossBits bossMask = BossBitMasks.FromCartridge(
                        cartridge.ReadCartridgeByte(RoomHeaderRomData.BankAddress | cursor),
                        $"Room $8F:{roomPointer:X4} state selector $8F:{selector:X4}");
                    ushort selectedPointer = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | unchecked((ushort)(cursor + 1)));
                    if (selection.IsBossDead(bossMask))
                        return selectedPointer;
                    cursor += 3;
                    break;
                }

                case RoomStateSelectorCodes.MorphBallAndMissiles:
                {
                    ushort selectedPointer = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | cursor);
                    if (selection.HasMorphBallAndMissiles)
                        return selectedPointer;
                    cursor += 2;
                    break;
                }

                case RoomStateSelectorCodes.PowerBombs:
                {
                    ushort selectedPointer = CartridgeImportWords.ReadWord(cartridge, RoomHeaderRomData.BankAddress | cursor);
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
    /// <summary>Typed mutually exclusive identity of the selected room-main callback.</summary>
    public RoomMainCallback MainCallback =>
        RoomCallbackDefinitions.ResolveMain(MainCodePointer);

    /// <summary>Typed mutually exclusive identity of the selected room-setup callback.</summary>
    public RoomSetupCallback SetupCallback =>
        RoomCallbackDefinitions.ResolveSetup(SetupCodePointer);

    internal static CartridgeRoomState Load(IImportCartridgeSource cartridge, ushort pointer)
    {
        int address = RoomHeaderRomData.BankAddress | pointer;
        ushort packedLayer2Scrolls = CartridgeImportWords.ReadWord(cartridge, address + 12);
        return new CartridgeRoomState(
            pointer,
            CompressedLevelDataAddress: CartridgeImportWords.ReadLong(cartridge, address),
            GraphicsSet: cartridge.ReadCartridgeByte(address + 3),
            MusicDataIndex: cartridge.ReadCartridgeByte(address + 4),
            MusicTrackIndex: cartridge.ReadCartridgeByte(address + 5),
            FxPointer: CartridgeImportWords.ReadWord(cartridge, address + 6),
            EnemyPopulationPointer: CartridgeImportWords.ReadWord(cartridge, address + 8),
            EnemyTilesetPointer: CartridgeImportWords.ReadWord(cartridge, address + 10),
            Layer2ScrollX: (byte)packedLayer2Scrolls,
            Layer2ScrollY: (byte)(packedLayer2Scrolls >> 8),
            ScrollPointer: CartridgeImportWords.ReadWord(cartridge, address + 14),
            XrayPointer: CartridgeImportWords.ReadWord(cartridge, address + 16),
            MainCodePointer: CartridgeImportWords.ReadWord(cartridge, address + 18),
            PlmPointer: CartridgeImportWords.ReadWord(cartridge, address + 20),
            BackgroundDataPointer: CartridgeImportWords.ReadWord(cartridge, address + 22),
            SetupCodePointer: CartridgeImportWords.ReadWord(cartridge, address + 24));
    }
}

/// <summary>Facts inspected by the cartridge's room-state selector functions.</summary>
public readonly record struct RoomStateSelectionContext(
    ReadOnlyMemory<byte> Events,
    BossBits BossBits,
    bool HasMorphBallAndMissiles,
    bool HasPowerBombs)
{
    public bool IsEventSet(byte eventIndex)
    {
        ReadOnlySpan<byte> events = Events.Span;
        int byteIndex = eventIndex >> 3;
        return byteIndex < events.Length && (events[byteIndex] & (1 << (eventIndex & 7))) != 0;
    }

    public bool IsBossDead(BossBits mask)
    {
        BossBitMasks.Validate(mask, "Room-state boss selector");
        return (BossBits & mask) != 0;
    }
}
