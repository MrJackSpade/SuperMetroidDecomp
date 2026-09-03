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
        new(0xCF8000, 0x00, "SPCEngine");

    /// <summary>Every non-PAL music bank referenced by the retail ROM pointer table.</summary>
    public static readonly IReadOnlyList<AudioUploadAssetDefinition> Music =
    [
        new(0xD0E20D, 0x03, "Music_TitleSequence"),
        new(0xD1B62A, 0x06, "Music_EmptyCrateria"),
        new(0xD288CA, 0x09, "Music_LowerCrateria"),
        new(0xD2D9B6, 0x0C, "Music_UpperCrateria"),
        new(0xD3933C, 0x0F, "Music_GreenBrinstar"),
        new(0xD3E812, 0x12, "Music_RedBrinstar"),
        new(0xD4B86C, 0x15, "Music_UpperNorfair"),
        new(0xD4F420, 0x18, "Music_LowerNorfair"),
        new(0xD5C844, 0x1B, "Music_Maridia"),
        new(0xD698B7, 0x1E, "Music_Tourian"),
        new(0xD6EF9D, 0x21, "Music_MotherBrain"),
        new(0xD7BF73, 0x24, "Music_BossFight1"),
        new(0xD899B2, 0x27, "Music_BossFight2"),
        new(0xD8EA8B, 0x2A, "Music_MiniBossFight"),
        new(0xD9B67B, 0x2D, "Music_Ceres"),
        new(0xD9F5DD, 0x30, "Music_WreckedShip"),
        new(0xDAB650, 0x33, "Music_ZebesExplosion"),
        new(0xDAD63B, 0x36, "Music_Intro"),
        new(0xDBA40F, 0x39, "Music_Death"),
        new(0xDBDF4F, 0x3C, "Music_Credits"),
        new(0xDCAF6C, 0x3F, "Music_TheLastMetroidIsInCaptivity"),
        new(0xDCFAC7, 0x42, "Music_TheGalaxyIsAtPeace"),
        new(0xDDB104, 0x45, "Music_BabyMetroid_BossFight2"),
        new(0xDE81C1, 0x48, "Music_SamusTheme_UpperCrateria"),
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
