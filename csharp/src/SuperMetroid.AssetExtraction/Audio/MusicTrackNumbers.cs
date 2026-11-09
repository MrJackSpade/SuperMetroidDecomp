using System.Collections.Frozen;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Decodes the sound driver's track number, read from a data set's track table, a room state or a recorded music
/// request, into the song it names, and encodes a song back to the number the driver plays it by (#1553). This is the
/// only place the game's songs meet their cartridge numbering: the shared songs are tracks one through four of every
/// data set, and a set's own songs follow from five.
/// </summary>
[EncodingOwner("track numbers of the sound driver's music data sets")]
internal static class MusicTrackNumbers
{
    /// <summary>Each song's track number.</summary>
    private static readonly FrozenDictionary<MusicTrack, int> Numbers = new Dictionary<MusicTrack, int>
    {
        [MusicTrack.SamusFanfare] = 1,
        [MusicTrack.ItemFanfare] = 2,
        [MusicTrack.Elevator] = 3,
        [MusicTrack.StatueHall] = 4,
        [MusicTrack.FirstSetSong] = 5,
        [MusicTrack.SecondSetSong] = 6,
        [MusicTrack.ThirdSetSong] = 7,
        [MusicTrack.FourthSetSong] = 8,
    }.ToFrozenDictionary();

    /// <summary>Each track number's song.</summary>
    private static readonly FrozenDictionary<int, MusicTrack> Tracks = Numbers.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>The song track <paramref name="number"/> plays.</summary>
    /// <exception cref="InvalidDataException">No song has the number.</exception>
    internal static MusicTrack Decode(int number) => Tracks.TryGetValue(number, out MusicTrack track)
        ? track
        : throw new InvalidDataException($"Music track {number} names no song; tracks 1 through {Tracks.Count} do.");

    /// <summary>The track number the sound driver plays <paramref name="track"/> by.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="track"/> is not a song.</exception>
    internal static int Number(MusicTrack track) => Numbers.TryGetValue(track, out int number)
        ? number
        : throw new ArgumentOutOfRangeException(nameof(track), track, "Not a song.");
}
