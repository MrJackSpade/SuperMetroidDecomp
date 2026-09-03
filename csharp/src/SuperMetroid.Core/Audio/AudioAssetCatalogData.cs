namespace SuperMetroid.Core.Audio;

/// <summary>
/// Cartridge addresses and stable asset names for every retail SPC upload stream.
/// The data-index values are byte offsets into the ROM's three-byte pointer table, matching
/// the values stored in room headers; they are deliberately not renumbered host indexes.
/// </summary>
public static class AudioAssetCatalogData
{
    /// <summary>The resident driver, sound effects, BRR directory, and shared samples.</summary>
    public static readonly AudioUploadAssetDefinition Common =
        new(AudioUploadAddresses.SpcEngine, 0x00, "SPCEngine");

    /// <summary>Every non-PAL music bank referenced by the retail ROM pointer table.</summary>
    public static readonly IReadOnlyList<AudioUploadAssetDefinition> Music =
    [
        new(AudioUploadAddresses.TitleSequence, 0x03, "Music_TitleSequence"),
        new(AudioUploadAddresses.EmptyCrateria, 0x06, "Music_EmptyCrateria"),
        new(AudioUploadAddresses.LowerCrateria, 0x09, "Music_LowerCrateria"),
        new(AudioUploadAddresses.UpperCrateria, 0x0C, "Music_UpperCrateria"),
        new(AudioUploadAddresses.GreenBrinstar, 0x0F, "Music_GreenBrinstar"),
        new(AudioUploadAddresses.RedBrinstar, 0x12, "Music_RedBrinstar"),
        new(AudioUploadAddresses.UpperNorfair, 0x15, "Music_UpperNorfair"),
        new(AudioUploadAddresses.LowerNorfair, 0x18, "Music_LowerNorfair"),
        new(AudioUploadAddresses.Maridia, 0x1B, "Music_Maridia"),
        new(AudioUploadAddresses.Tourian, 0x1E, "Music_Tourian"),
        new(AudioUploadAddresses.MotherBrain, 0x21, "Music_MotherBrain"),
        new(AudioUploadAddresses.BossFight1, 0x24, "Music_BossFight1"),
        new(AudioUploadAddresses.BossFight2, 0x27, "Music_BossFight2"),
        new(AudioUploadAddresses.MiniBossFight, 0x2A, "Music_MiniBossFight"),
        new(AudioUploadAddresses.Ceres, 0x2D, "Music_Ceres"),
        new(AudioUploadAddresses.WreckedShip, 0x30, "Music_WreckedShip"),
        new(AudioUploadAddresses.ZebesExplosion, 0x33, "Music_ZebesExplosion"),
        new(AudioUploadAddresses.Intro, 0x36, "Music_Intro"),
        new(AudioUploadAddresses.Death, 0x39, "Music_Death"),
        new(AudioUploadAddresses.Credits, 0x3C, "Music_Credits"),
        new(AudioUploadAddresses.LastMetroidInCaptivity, 0x3F, "Music_TheLastMetroidIsInCaptivity"),
        new(AudioUploadAddresses.GalaxyAtPeace, 0x42, "Music_TheGalaxyIsAtPeace"),
        new(AudioUploadAddresses.BabyMetroidBossFight2, 0x45, "Music_BabyMetroid_BossFight2"),
        new(AudioUploadAddresses.SamusThemeUpperCrateria, 0x48, "Music_SamusTheme_UpperCrateria"),
    ];

    /// <summary>Common stream followed by every music stream, in cartridge table order.</summary>
    public static IEnumerable<AudioUploadAssetDefinition> All
    {
        get
        {
            yield return Common;
            foreach (AudioUploadAssetDefinition definition in Music)
                yield return definition;
        }
    }
}

/// <summary>Identity of one upload stream before it is materialized on disk.</summary>
public sealed record AudioUploadAssetDefinition(int SnesAddress, byte DataIndex, string Name);
