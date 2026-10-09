using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>Compiled player-map anchors used by $82:9028, independent of editable marker artwork.</summary>
public static class FileSelectMapLoadAnchors
{
    /// <summary>Projects a valid saved station from compiled placement and room metadata.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against native $80:C437 load placement and
    /// $82:9040/$82:9098 map projection. First wrap camera plus player offset to a
    /// sixteen-bit world coordinate (including the horizontal128 bias), then take
    /// its high byte. Add the room map coordinate and multiply by eight; Y has one
    /// additional map row. The thirty-four supported station identities retain the
    /// existing marker-validity guard before narrowing the station index to a byte.
    /// Only fixed compiled room metadata is needed, never room-state selection,
    /// cartridge access, editable marker art, or a generated coordinate cache.
    /// </remarks>
    public static FileSelectMapAnchor Get(AreaId area, int stationIndex)
    {
        _ = MapSaveMarkerDefinitions.Id(area, stationIndex);
        LoadStationEntry station = LoadStationDefinitions.Get(area, (byte)stationIndex);
        RoomHeaderDefinition room = RoomHeaderDefinitions.Get(station.RoomPointer);
        return new((ushort)((room.MapX + (station.SamusX >> 8)) << 3),
            (ushort)((room.MapY + (station.SamusY >> 8) + 1) << 3));
    }
}

/// <summary>Player-map position supplied to native initial-scroll clipping, not a marker drawing origin.</summary>
/// <param name="X">Horizontal map-pixel coordinate used to initialize scrolling around the loaded Samus position.</param>
/// <param name="Y">Vertical map-pixel coordinate used for initial scrolling, including the native one-row map projection offset.</param>
public readonly record struct FileSelectMapAnchor(ushort X, ushort Y);

/// <summary>Compiled $81:AAA0 FileSelectMapArea_IndexTable order used by label composition.</summary>
public static class FileSelectMapAreaOrder
{
    /// <summary>Translates a native file-select area identity0..5 to the corresponding game area.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all six words at $81:AAA0 and native
    /// label/selection consumers. The menu and game use different categorical area
    /// numbering: native $81:A398/$81:ADC3 perform the inverse search, while label
    /// composition at $81:A9EC/$81:A9FE resolves this direction. Named result cases
    /// express that identity translation without a stored permutation or numeric fit.
    /// Unsupported int identities preserve ArgumentOutOfRangeException.
    /// </remarks>
    public static AreaId Get(int displayIndex) => displayIndex switch
    {
        0 => AreaId.Crateria,
        1 => AreaId.WreckedShip,
        2 => AreaId.Tourian,
        3 => AreaId.Brinstar,
        4 => AreaId.Maridia,
        5 => AreaId.Norfair,
        _ => throw new ArgumentOutOfRangeException(nameof(displayIndex)),
    };
}
