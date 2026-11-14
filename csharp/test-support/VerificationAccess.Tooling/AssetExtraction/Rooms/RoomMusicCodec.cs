using SuperMetroid.AssetExtraction.Audio;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Decodes a room state's two music bytes once, at room-state import (#1410): the music-data index, a byte offset into
/// the cartridge's music-data pointer table, and the track byte. The game holds the result as a <see cref="RoomMusic"/>
/// and never sees either byte.
/// </summary>
/// <remarks>
/// A zero data index keeps the loaded data set (<c>$82:E071</c> skips it); any other index names the data set the
/// catalog maps it to. A zero track keeps the playing track (<c>$82:E0E1</c> returns at once); track <c>$80</c> silences
/// the music, because the music handler keeps only the low seven bits of the queued byte, and a track number with its
/// high bit set never equals the playing one; any other byte is a song's track number in the room's data set
/// (<see cref="MusicTrackNumbers"/>), and a room that keeps the loaded set plays a shared song or the loaded set's area theme.
/// A track byte above <c>$80</c>, which would play its low seven bits, a track number no song has, and an index that
/// names no data set appear in no retail room state, so they fail here rather than reach the game.
/// </remarks>
internal static class RoomMusicCodec
{
    /// <summary>The track byte the retail room states use to silence the music.</summary>
    private const byte SilencedTrack = 0x80;

    /// <summary>Decodes a room state's music-data index and track byte.</summary>
    /// <param name="dataIndex">The room state's music-data index byte.</param>
    /// <param name="track">The room state's music track byte.</param>
    /// <returns>The room's music.</returns>
    /// <exception cref="InvalidDataException">A byte holds a value no retail room state holds.</exception>
    public static RoomMusic Decode(byte dataIndex, byte track)
    {
        MusicDataSet? dataSet = dataIndex == 0
            ? null
            : AudioAssetCatalogDefinitions.ResolveDataIndex(dataIndex).DataSet;
        RoomMusicTrack choice = track switch
        {
            0 => RoomMusicTrack.Inherited,
            SilencedTrack => RoomMusicTrack.Silenced,
            // A room that keeps the loaded set and asks for its first own song plays the loaded set's area theme (#1748).
            MusicTrackNumbers.AreaThemeTrack when dataSet is null => RoomMusicTrack.PlaysAreaTheme,
            < SilencedTrack => RoomMusicTrack.Plays(MusicTrackNumbers.Decode(dataSet, track)),
            _ => throw new InvalidDataException(
                $"Room music track ${track:X2} is not one the retail room states hold; only $00 through $80 are."),
        };
        return new RoomMusic(dataSet, choice);
    }
}
