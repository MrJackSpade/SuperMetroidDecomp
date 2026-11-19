namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="MusicTrack"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class MusicTrackKeys
{
    /// <summary>Each MusicTrack member's installed key.</summary>
    internal static InstalledKeys<MusicTrack> Installed { get; } = new(new Dictionary<MusicTrack, string>
    {
        [MusicTrack.SamusFanfare] = "SamusFanfare",
        [MusicTrack.ItemFanfare] = "ItemFanfare",
        [MusicTrack.Elevator] = "Elevator",
        [MusicTrack.StatueHall] = "StatueHall",
        [MusicTrack.TitleOpening] = "TitleOpening",
        [MusicTrack.TitleScreen] = "TitleScreen",
        [MusicTrack.CrateriaArrival] = "CrateriaArrival",
        [MusicTrack.CrateriaArrivalZebesAwake] = "CrateriaArrivalZebesAwake",
        [MusicTrack.BlueBrinstarArrival] = "BlueBrinstarArrival",
        [MusicTrack.LowerCrateria] = "LowerCrateria",
        [MusicTrack.TourianStatues] = "TourianStatues",
        [MusicTrack.SamusTheme] = "SamusTheme",
        [MusicTrack.GreenBrinstar] = "GreenBrinstar",
        [MusicTrack.RedBrinstar] = "RedBrinstar",
        [MusicTrack.UpperNorfair] = "UpperNorfair",
        [MusicTrack.LowerNorfair] = "LowerNorfair",
        [MusicTrack.EastMaridia] = "EastMaridia",
        [MusicTrack.WestMaridia] = "WestMaridia",
        [MusicTrack.WreckedShipPoweredDown] = "WreckedShipPoweredDown",
        [MusicTrack.WreckedShipPoweredUp] = "WreckedShipPoweredUp",
        [MusicTrack.Tourian] = "Tourian",
        [MusicTrack.SilentSong] = "SilentSong",
        [MusicTrack.MotherBrainBattle] = "MotherBrainBattle",
        [MusicTrack.RidleyAndDraygonBattle] = "RidleyAndDraygonBattle",
        [MusicTrack.TorizoAwakening] = "TorizoAwakening",
        [MusicTrack.Escape] = "Escape",
        [MusicTrack.KraidAndPhantoonBattle] = "KraidAndPhantoonBattle",
        [MusicTrack.BossRoomTension] = "BossRoomTension",
        [MusicTrack.MiniBossBattle] = "MiniBossBattle",
        [MusicTrack.BabyMetroidTheme] = "BabyMetroidTheme",
        [MusicTrack.CeresFlight] = "CeresFlight",
        [MusicTrack.CeresStation] = "CeresStation",
        [MusicTrack.CeresExplosion] = "CeresExplosion",
        [MusicTrack.CeresExplosionWithSamusAboard] = "CeresExplosionWithSamusAboard",
        [MusicTrack.ZebesExplosion] = "ZebesExplosion",
        [MusicTrack.Introduction] = "Introduction",
        [MusicTrack.SamusDeath] = "SamusDeath",
        [MusicTrack.Credits] = "Credits",
        [MusicTrack.LastMetroidInCaptivity] = "LastMetroidInCaptivity",
        [MusicTrack.GalaxyAtPeace] = "GalaxyAtPeace",
    });
}
