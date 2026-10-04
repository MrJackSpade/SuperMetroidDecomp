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

    /// <summary>Cartridge-order view of24 named music cases, without a stored lookup or cache.</summary>
    public static readonly IReadOnlyList<AudioUploadAssetDefinition> Music = new MusicBankView();

    private sealed class MusicBankView : IReadOnlyList<AudioUploadAssetDefinition>
    {
        public int Count => 24;
        public AudioUploadAssetDefinition this[int index] => (uint)index < Count
            ? ResolveDataIndex((byte)(3 * (index + 1))) : throw new IndexOutOfRangeException();
        public IEnumerator<AudioUploadAssetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

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

    /// <summary>
    /// Resolves the cartridge's byte-offset music-data identity to its compiled upload
    /// stream. Retail defines the common driver at zero and 24 music banks at three-byte
    /// intervals through <c>$48</c>; other byte offsets point into overlapping pointer
    /// bytes or subsequent room data and are not authored music-data identities.
    /// </summary>
    public static AudioUploadAssetDefinition ResolveDataIndex(byte dataIndex) => dataIndex switch
    {
        0 => Common,
        0x03 => new(AudioUploadAddresses.TitleSequence, dataIndex, "Music_TitleSequence"),
        0x06 => new(AudioUploadAddresses.EmptyCrateria, dataIndex, "Music_EmptyCrateria"),
        0x09 => new(AudioUploadAddresses.LowerCrateria, dataIndex, "Music_LowerCrateria"),
        0x0C => new(AudioUploadAddresses.UpperCrateria, dataIndex, "Music_UpperCrateria"),
        0x0F => new(AudioUploadAddresses.GreenBrinstar, dataIndex, "Music_GreenBrinstar"),
        0x12 => new(AudioUploadAddresses.RedBrinstar, dataIndex, "Music_RedBrinstar"),
        0x15 => new(AudioUploadAddresses.UpperNorfair, dataIndex, "Music_UpperNorfair"),
        0x18 => new(AudioUploadAddresses.LowerNorfair, dataIndex, "Music_LowerNorfair"),
        0x1B => new(AudioUploadAddresses.Maridia, dataIndex, "Music_Maridia"),
        0x1E => new(AudioUploadAddresses.Tourian, dataIndex, "Music_Tourian"),
        0x21 => new(AudioUploadAddresses.MotherBrain, dataIndex, "Music_MotherBrain"),
        0x24 => new(AudioUploadAddresses.BossFight1, dataIndex, "Music_BossFight1"),
        0x27 => new(AudioUploadAddresses.BossFight2, dataIndex, "Music_BossFight2"),
        0x2A => new(AudioUploadAddresses.MiniBossFight, dataIndex, "Music_MiniBossFight"),
        0x2D => new(AudioUploadAddresses.Ceres, dataIndex, "Music_Ceres"),
        0x30 => new(AudioUploadAddresses.WreckedShip, dataIndex, "Music_WreckedShip"),
        0x33 => new(AudioUploadAddresses.ZebesExplosion, dataIndex, "Music_ZebesExplosion"),
        0x36 => new(AudioUploadAddresses.Intro, dataIndex, "Music_Intro"),
        0x39 => new(AudioUploadAddresses.Death, dataIndex, "Music_Death"),
        0x3C => new(AudioUploadAddresses.Credits, dataIndex, "Music_Credits"),
        0x3F => new(AudioUploadAddresses.LastMetroidInCaptivity, dataIndex, "Music_TheLastMetroidIsInCaptivity"),
        0x42 => new(AudioUploadAddresses.GalaxyAtPeace, dataIndex, "Music_TheGalaxyIsAtPeace"),
        0x45 => new(AudioUploadAddresses.BabyMetroidBossFight2, dataIndex, "Music_BabyMetroid_BossFight2"),
        0x48 => new(AudioUploadAddresses.SamusThemeUpperCrateria, dataIndex, "Music_SamusTheme_UpperCrateria"),
        _ => throw new InvalidDataException(
            $"Music data index ${dataIndex:X2} is not present in the compiled retail upload catalog."),
    };
}

/// <summary>Identity of one upload stream before it is materialized on disk.</summary>
public sealed record AudioUploadAssetDefinition(int SnesAddress, byte DataIndex, string Name);
