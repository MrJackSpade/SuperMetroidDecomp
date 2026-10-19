using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// The persistent-progress slice of the cartridge's live SRAM mirror, from the event bitset at <c>$7E:D820</c> through
/// the saved dispatcher word at <c>$7E:D914</c>, held as owned bytes in the native layout. SpaceTime's malformed palette
/// copy overwrites progression as raw bytes in this slice (#1318): the progression owners publish into it before the
/// copy and reclaim from it afterwards. Bytes that no owner holds keep whatever the copy last wrote, as the cartridge's
/// WRAM does.
/// </summary>
[CpuBusBoundary("native layout: persistent-progress SRAM mirror slice that SpaceTime's palette copy overwrites")]
public sealed class SaveMirrorImage
{
    /// <summary>Offset of the event bitset, the slice's first byte.</summary>
    private const int EventsOffset = 0;

    /// <summary>Offset of the per-area boss bitset.</summary>
    private const int BossBitsOffset = EventsOffset + Bank80SystemState.EventByteCount;

    /// <summary>Offset of the room-indexed opened-Chozo-orb bitset.</summary>
    private const int RoomChozoBitsOffset = BossBitsOffset + Bank80SystemState.AreaCount;

    /// <summary>Offset of the room-indexed collected-item bitset.</summary>
    private const int CollectedItemBitsOffset = RoomChozoBitsOffset + Bank80SystemState.RoomChozoBitByteCount;

    /// <summary>Offset of the room-indexed opened-door bitset.</summary>
    private const int OpenedDoorBitsOffset = CollectedItemBitsOffset + Bank80SystemState.ItemBitByteCount;

    /// <summary>Mirror bytes between the opened-door bits and the used save stations that no progression owner holds.</summary>
    private const int UnownedByteCount = 8;

    /// <summary>Offset of the per-area used save-station and elevator bitset.</summary>
    private const int UsedSaveStationsOffset = OpenedDoorBitsOffset + Bank80SystemState.DoorBitByteCount + UnownedByteCount;

    /// <summary>Offset of the per-area acquired map-station flags.</summary>
    private const int MapStationsOffset = UsedSaveStationsOffset + Bank80SystemState.UsedSaveStationByteCount;

    /// <summary>Offset of the saved dispatcher word, the slice's last two bytes.</summary>
    private const int LoadingGameStateOffset = MapStationsOffset + Bank80SystemState.MapStationByteCount;

    /// <summary>Bytes in the slice.</summary>
    private const int ByteCount = LoadingGameStateOffset + sizeof(ushort);

    /// <summary>The slice's bytes, in native order.</summary>
    private readonly byte[] _bytes = new byte[ByteCount];

    /// <summary>The persistent event bitset.</summary>
    public Span<byte> Events => _bytes.AsSpan(EventsOffset, Bank80SystemState.EventByteCount);

    /// <summary>The per-area boss bitset.</summary>
    public Span<byte> BossBits => _bytes.AsSpan(BossBitsOffset, Bank80SystemState.AreaCount);

    /// <summary>The room-indexed opened-Chozo-orb bitset.</summary>
    public Span<byte> RoomChozoBits => _bytes.AsSpan(RoomChozoBitsOffset, Bank80SystemState.RoomChozoBitByteCount);

    /// <summary>The room-indexed collected-item bitset.</summary>
    public Span<byte> CollectedItemBits => _bytes.AsSpan(CollectedItemBitsOffset, Bank80SystemState.ItemBitByteCount);

    /// <summary>The room-indexed opened-door bitset.</summary>
    public Span<byte> OpenedDoorBits => _bytes.AsSpan(OpenedDoorBitsOffset, Bank80SystemState.DoorBitByteCount);

    /// <summary>The per-area used save-station and elevator bitset.</summary>
    public Span<byte> UsedSaveStations => _bytes.AsSpan(UsedSaveStationsOffset, Bank80SystemState.UsedSaveStationByteCount);

    /// <summary>The per-area acquired map-station flags.</summary>
    public Span<byte> MapStations => _bytes.AsSpan(MapStationsOffset, Bank80SystemState.MapStationByteCount);

    /// <summary>
    /// The saved startup dispatcher route, including values the dispatcher does not name; writing a value outside
    /// <see cref="SaveFieldValue"/>'s range fails instead of wrapping.
    /// </summary>
    public int LoadingGameState
    {
        get => SnesLittleEndian.Word(_bytes[LoadingGameStateOffset], _bytes[LoadingGameStateOffset + 1]);
        set
        {
            ushort word = SaveFieldValue.Encode(value, nameof(LoadingGameState));
            _bytes[LoadingGameStateOffset] = SnesLittleEndian.LowByte(word);
            _bytes[LoadingGameStateOffset + 1] = SnesLittleEndian.HighByte(word);
        }
    }

    /// <summary>Whether <paramref name="offset"/> from the event bitset lies in the slice.</summary>
    internal static bool HoldsOffset(int offset) => offset is >= 0 and < ByteCount;

    /// <summary>
    /// Writes one raw byte at <paramref name="offset"/> from the event bitset, which must lie in the slice; SpaceTime's
    /// malformed palette copy overwrites progression this way (#1318).
    /// </summary>
    internal void WriteRawByte(int offset, byte value)
    {
        if (!HoldsOffset(offset))
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "The offset lies outside the progression mirror.");
        _bytes[offset] = value;
    }
}
