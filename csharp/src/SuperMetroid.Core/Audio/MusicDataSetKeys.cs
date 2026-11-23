namespace SuperMetroid.Core.Audio;

/// <summary>
/// The installed key of each <see cref="MusicDataSet"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class MusicDataSetKeys
{
    /// <summary>Each MusicDataSet member's installed key.</summary>
    internal static InstalledKeys<MusicDataSet> Installed { get; } = new(new Dictionary<MusicDataSet, string>
    {
        [MusicDataSet.TitleSequence] = "TitleSequence",
        [MusicDataSet.EmptyCrateria] = "EmptyCrateria",
        [MusicDataSet.LowerCrateria] = "LowerCrateria",
        [MusicDataSet.UpperCrateria] = "UpperCrateria",
        [MusicDataSet.GreenBrinstar] = "GreenBrinstar",
        [MusicDataSet.RedBrinstar] = "RedBrinstar",
        [MusicDataSet.UpperNorfair] = "UpperNorfair",
        [MusicDataSet.LowerNorfair] = "LowerNorfair",
        [MusicDataSet.Maridia] = "Maridia",
        [MusicDataSet.Tourian] = "Tourian",
        [MusicDataSet.MotherBrain] = "MotherBrain",
        [MusicDataSet.RidleyAndDraygonBattles] = "RidleyAndDraygonBattles",
        [MusicDataSet.KraidAndPhantoonBattles] = "KraidAndPhantoonBattles",
        [MusicDataSet.MiniBossFight] = "MiniBossFight",
        [MusicDataSet.Ceres] = "Ceres",
        [MusicDataSet.WreckedShip] = "WreckedShip",
        [MusicDataSet.ZebesExplosion] = "ZebesExplosion",
        [MusicDataSet.Intro] = "Intro",
        [MusicDataSet.Death] = "Death",
        [MusicDataSet.Credits] = "Credits",
        [MusicDataSet.LastMetroidInCaptivity] = "LastMetroidInCaptivity",
        [MusicDataSet.GalaxyAtPeace] = "GalaxyAtPeace",
        [MusicDataSet.BabyMetroidEncounter] = "BabyMetroidEncounter",
        [MusicDataSet.SamusThemeUpperCrateria] = "SamusThemeUpperCrateria",
    });
}
