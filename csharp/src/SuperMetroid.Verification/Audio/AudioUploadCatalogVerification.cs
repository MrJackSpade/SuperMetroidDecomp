using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;

namespace SuperMetroid.Verification.Audio;

/// <summary>Checks all SPC engine and music upload identities against the native pointer table.</summary>
internal static class AudioUploadCatalogVerification
{
    /// <summary>Compares 25 indexed pointers and names while rejecting non-entry byte offsets and invalid music indexes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the audio upload catalog pointers")]
    internal static void VerifyAudioUploadCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["SPCEngine", "Music_TitleSequence", "Music_EmptyCrateria", "Music_LowerCrateria", "Music_UpperCrateria", "Music_GreenBrinstar", "Music_RedBrinstar", "Music_UpperNorfair", "Music_LowerNorfair", "Music_Maridia", "Music_Tourian", "Music_MotherBrain", "Music_BossFight1", "Music_BossFight2", "Music_MiniBossFight", "Music_Ceres", "Music_WreckedShip", "Music_ZebesExplosion", "Music_Intro", "Music_Death", "Music_Credits", "Music_TheLastMetroidIsInCaptivity", "Music_TheGalaxyIsAtPeace", "Music_BabyMetroid_BossFight2", "Music_SamusTheme_UpperCrateria"];
        var all = AudioAssetCatalogDefinitions.All.ToArray();
        AssertEqual(25, all.Length, "all upload identities");
        AssertEqual(24, AudioAssetCatalogDefinitions.Music.Count, "music view count");
        for (int index = 0; index < 256; index++)
        {
            if (index > 0x48 || index % 3 != 0)
            {
                AssertThrows<InvalidDataException>(() => AudioAssetCatalogDefinitions.ResolveDataIndex((byte)index), "invalid upload identity");
                continue;
            }
            int address = 0x8fe7e1 + index;
            int pointer = (CartridgeImportSource.Require(rom).ReadCartridgeLong(address));
            var actual = AudioAssetCatalogDefinitions.ResolveDataIndex((byte)index);
            AssertEqual(pointer, actual.SnesAddress, "original upload pointer");
            AssertEqual((byte)index, actual.DataIndex, "native byte offset identity");
            AssertEqual(names[index / 3], actual.Name, "published asset name");
            AssertEqual((AudioBank)(index / 3), actual.AudioSet, "stable bank identity in pointer-table order");
            AssertEqual(actual, AudioAssetCatalogDefinitions.Get(actual.AudioSet), "bank identity lookup");
            AssertEqual(actual, AudioAssetCatalogDefinitions.ResolveSnesAddress(pointer), "manifest address lookup");
            AssertEqual(actual, all[index / 3], "ordered enumeration");
            if (index != 0) AssertEqual(actual, AudioAssetCatalogDefinitions.Music[index / 3 - 1], "music index view");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 24, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = AudioAssetCatalogDefinitions.Music[invalid]; }, "music view bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => AudioAssetCatalogDefinitions.Get((AudioBank)25), "undefined bank");
        AssertThrows<InvalidDataException>(() => AudioAssetCatalogDefinitions.ResolveSnesAddress(AudioAssetCatalogDefinitions.Common.SnesAddress + 1), "non-upload address");
    }
}
