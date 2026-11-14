using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// Decodes the sound driver's track number, read from a data set's track table, a room state or a recorded music
/// request, into the song it names in the loaded data set, and encodes a song back to the number the driver plays it by
/// (#1553, #1748). This is the only place the game's songs meet their cartridge numbering: the shared songs are tracks
/// one through four of every data set, and each set's own songs follow from <see cref="AreaThemeTrack"/>, so the same
/// number names a different song in each set.
/// </summary>
[EncodingOwner("track numbers of the sound driver's music data sets")]
internal static class MusicTrackNumbers
{
    /// <summary>The number of each data set's first own song, which a room that keeps the loaded set plays as its area theme.</summary>
    internal const int AreaThemeTrack = 5;

    /// <summary>Each shared song's track number, the same in every data set.</summary>
    private static readonly FrozenDictionary<MusicTrack, int> SharedNumbers = new Dictionary<MusicTrack, int>
    {
        [MusicTrack.SamusFanfare] = 1,
        [MusicTrack.ItemFanfare] = 2,
        [MusicTrack.Elevator] = 3,
        [MusicTrack.StatueHall] = 4,
    }.ToFrozenDictionary();

    /// <summary>Each shared track number's song.</summary>
    private static readonly FrozenDictionary<int, MusicTrack> SharedSongs = SharedNumbers.ToFrozenDictionary(entry => entry.Value, entry => entry.Key);

    /// <summary>Each data set's own songs, in track order from <see cref="AreaThemeTrack"/>, as the disassembly's room states and the game's requests use them.</summary>
    private static readonly FrozenDictionary<MusicDataSet, ImmutableArray<MusicTrack>> OwnSongs = new Dictionary<MusicDataSet, ImmutableArray<MusicTrack>>
    {
        [MusicDataSet.TitleSequence] = [MusicTrack.TitleOpening, MusicTrack.TitleScreen],
        [MusicDataSet.EmptyCrateria] = [MusicTrack.CrateriaArrival, MusicTrack.CrateriaArrivalZebesAwake, MusicTrack.BlueBrinstarArrival],
        [MusicDataSet.LowerCrateria] = [MusicTrack.LowerCrateria, MusicTrack.TourianStatues],
        [MusicDataSet.UpperCrateria] = [MusicTrack.SamusTheme],
        [MusicDataSet.GreenBrinstar] = [MusicTrack.GreenBrinstar],
        [MusicDataSet.RedBrinstar] = [MusicTrack.RedBrinstar],
        [MusicDataSet.UpperNorfair] = [MusicTrack.UpperNorfair],
        [MusicDataSet.LowerNorfair] = [MusicTrack.LowerNorfair],
        [MusicDataSet.Maridia] = [MusicTrack.EastMaridia, MusicTrack.WestMaridia],
        [MusicDataSet.Tourian] = [MusicTrack.Tourian, MusicTrack.SilentSong],
        [MusicDataSet.MotherBrain] = [MusicTrack.MotherBrainBattle],
        [MusicDataSet.RidleyAndDraygonBattles] = [MusicTrack.RidleyAndDraygonBattle, MusicTrack.TorizoAwakening, MusicTrack.Escape],
        [MusicDataSet.KraidAndPhantoonBattles] = [MusicTrack.KraidAndPhantoonBattle, MusicTrack.BossRoomTension],
        [MusicDataSet.MiniBossFight] = [MusicTrack.MiniBossBattle],
        [MusicDataSet.Ceres] = [MusicTrack.CeresFlight, MusicTrack.CeresStation, MusicTrack.CeresExplosion, MusicTrack.CeresExplosionWithSamusAboard],
        [MusicDataSet.WreckedShip] = [MusicTrack.WreckedShipPoweredDown, MusicTrack.WreckedShipPoweredUp],
        [MusicDataSet.ZebesExplosion] = [MusicTrack.ZebesExplosion],
        [MusicDataSet.Intro] = [MusicTrack.Introduction],
        [MusicDataSet.Death] = [MusicTrack.SamusDeath],
        [MusicDataSet.Credits] = [MusicTrack.Credits],
        [MusicDataSet.LastMetroidInCaptivity] = [MusicTrack.LastMetroidInCaptivity],
        [MusicDataSet.GalaxyAtPeace] = [MusicTrack.GalaxyAtPeace],
        [MusicDataSet.BabyMetroidEncounter] = [MusicTrack.KraidAndPhantoonBattle, MusicTrack.BabyMetroidTheme, MusicTrack.SilentSong],
        [MusicDataSet.SamusThemeUpperCrateria] = [MusicTrack.SamusTheme],
    }.ToFrozenDictionary();

    /// <summary>The track numbers of the shared songs, which name the same song whichever data set is loaded.</summary>
    internal static IEnumerable<int> SharedTrackNumbers => SharedNumbers.Values;

    /// <summary>The song track <paramref name="number"/> plays with <paramref name="dataSet"/> loaded, or with only the shared songs for none.</summary>
    /// <exception cref="InvalidDataException">No song of the data set, or no shared song, has the number.</exception>
    internal static MusicTrack Decode(MusicDataSet? dataSet, int number)
    {
        if (SharedSongs.TryGetValue(number, out MusicTrack shared))
            return shared;
        if (dataSet is MusicDataSet set)
        {
            ImmutableArray<MusicTrack> own = OwnSongs[set];
            int index = number - AreaThemeTrack;
            if (index >= 0 && index < own.Length)
                return own[index];
        }
        throw new InvalidDataException($"Music track {number} names no song of {(dataSet is MusicDataSet named ? $"the {named} data set" : "the shared songs")}.");
    }

    /// <summary>The song track <paramref name="number"/> plays with <paramref name="bank"/> loaded after the shared upload.</summary>
    /// <exception cref="InvalidDataException">No song the bank's data set holds has the number.</exception>
    internal static MusicTrack Decode(AudioBank bank, int number) => Decode(AudioAssetCatalogDefinitions.Get(bank).DataSet, number);

    /// <summary>The track number the sound driver plays <paramref name="track"/> by with <paramref name="dataSet"/> loaded.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Neither the data set nor the shared songs hold <paramref name="track"/>.</exception>
    internal static int Number(MusicDataSet? dataSet, MusicTrack track)
    {
        if (SharedNumbers.TryGetValue(track, out int shared))
            return shared;
        int index = dataSet is MusicDataSet set ? OwnSongs[set].IndexOf(track) : -1;
        return index >= 0
            ? AreaThemeTrack + index
            : throw new ArgumentOutOfRangeException(nameof(track), track, $"The loaded music ({dataSet?.ToString() ?? "only the shared songs"}) holds no such song.");
    }
}
