namespace SuperMetroid.Core.Audio;

/// <summary>
/// Each music data set's area theme (#1748): the song a room that keeps the loaded data set plays
/// (<see cref="RoomMusicTrack.PlaysAreaTheme"/>), such as the rooms an item room's elevator tune hands back to. The retail
/// rooms that do so are entered with an area's set loaded; a scene's or a boss's set names the song such a room would
/// play from it, so the choice is defined whichever set is loaded.
/// </summary>
public static class AreaThemeDefinitions
{
    /// <summary>The area theme of each data set.</summary>
    public static IReadOnlyDictionary<MusicDataSet, MusicTrack> Themes { get; } = new Dictionary<MusicDataSet, MusicTrack>
    {
        [MusicDataSet.TitleSequence] = MusicTrack.TitleOpening,
        [MusicDataSet.EmptyCrateria] = MusicTrack.CrateriaArrival,
        [MusicDataSet.LowerCrateria] = MusicTrack.LowerCrateria,
        [MusicDataSet.UpperCrateria] = MusicTrack.SamusTheme,
        [MusicDataSet.GreenBrinstar] = MusicTrack.GreenBrinstar,
        [MusicDataSet.RedBrinstar] = MusicTrack.RedBrinstar,
        [MusicDataSet.UpperNorfair] = MusicTrack.UpperNorfair,
        [MusicDataSet.LowerNorfair] = MusicTrack.LowerNorfair,
        [MusicDataSet.Maridia] = MusicTrack.EastMaridia,
        [MusicDataSet.Tourian] = MusicTrack.Tourian,
        [MusicDataSet.MotherBrain] = MusicTrack.MotherBrainBattle,
        [MusicDataSet.RidleyAndDraygonBattles] = MusicTrack.RidleyAndDraygonBattle,
        [MusicDataSet.KraidAndPhantoonBattles] = MusicTrack.KraidAndPhantoonBattle,
        [MusicDataSet.MiniBossFight] = MusicTrack.MiniBossBattle,
        [MusicDataSet.Ceres] = MusicTrack.CeresFlight,
        [MusicDataSet.WreckedShip] = MusicTrack.WreckedShipPoweredDown,
        [MusicDataSet.ZebesExplosion] = MusicTrack.ZebesExplosion,
        [MusicDataSet.Intro] = MusicTrack.Introduction,
        [MusicDataSet.Death] = MusicTrack.SamusDeath,
        [MusicDataSet.Credits] = MusicTrack.Credits,
        [MusicDataSet.LastMetroidInCaptivity] = MusicTrack.LastMetroidInCaptivity,
        [MusicDataSet.GalaxyAtPeace] = MusicTrack.GalaxyAtPeace,
        [MusicDataSet.BabyMetroidEncounter] = MusicTrack.KraidAndPhantoonBattle,
        [MusicDataSet.SamusThemeUpperCrateria] = MusicTrack.SamusTheme,
    };
}
