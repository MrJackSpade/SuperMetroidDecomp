using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
internal static partial class Program
{
    private static void VerifyAudioUploadCatalog(ISnesAddressSpace rom)
    {
        string[] names = ["SPCEngine", "Music_TitleSequence", "Music_EmptyCrateria", "Music_LowerCrateria", "Music_UpperCrateria", "Music_GreenBrinstar", "Music_RedBrinstar", "Music_UpperNorfair", "Music_LowerNorfair", "Music_Maridia", "Music_Tourian", "Music_MotherBrain", "Music_BossFight1", "Music_BossFight2", "Music_MiniBossFight", "Music_Ceres", "Music_WreckedShip", "Music_ZebesExplosion", "Music_Intro", "Music_Death", "Music_Credits", "Music_TheLastMetroidIsInCaptivity", "Music_TheGalaxyIsAtPeace", "Music_BabyMetroid_BossFight2", "Music_SamusTheme_UpperCrateria"];
        var all = AudioAssetCatalogData.All.ToArray();
        AssertEqual(25, all.Length, "all upload identities");
        AssertEqual(24, AudioAssetCatalogData.Music.Count, "music view count");
        for (int index = 0; index < 256; index++)
        {
            if (index > 0x48 || index % 3 != 0)
            {
                AssertThrows<InvalidDataException>(() => AudioAssetCatalogData.ResolveDataIndex((byte)index), "invalid upload identity");
                continue;
            }
            int address = 0x8fe7e1 + index;
            int pointer = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8 | rom.ReadByte(address + 2) << 16;
            var actual = AudioAssetCatalogData.ResolveDataIndex((byte)index);
            AssertEqual(pointer, actual.SnesAddress, "original upload pointer");
            AssertEqual((byte)index, actual.DataIndex, "native byte offset identity");
            AssertEqual(names[index / 3], actual.Name, "published asset name");
            AssertEqual(actual, all[index / 3], "ordered enumeration");
            if (index != 0) AssertEqual(actual, AudioAssetCatalogData.Music[index / 3 - 1], "music index view");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => { _ = AudioAssetCatalogData.Music[invalid]; }, "music view bounds");
    }
}