namespace SuperMetroid.Core.Audio;

/// <summary>
/// The single catalog mapping each <see cref="AudioBank"/> to its cartridge address, room-header
/// data index and stable asset name for every retail SPC upload stream.
/// The data-index values are byte offsets into the ROM's three-byte pointer table, matching
/// the values stored in room headers; they are deliberately not renumbered host indexes.
/// </summary>
public static class AudioAssetCatalogDefinitions
{
    /// <summary>The resident driver, sound effects, BRR directory, and shared samples.</summary>
    public static readonly AudioUploadAssetDefinition Common =
        new(AudioBank.SpcEngine, AudioUploadAddresses.SpcEngine, 0x00, "SPCEngine");

    /// <summary>Cartridge-order view of 24 named music cases, without a stored lookup or cache.</summary>
    public static readonly IReadOnlyList<AudioUploadAssetDefinition> Music = new MusicBankView();

    /// <summary>Calculates the twenty-four aligned music table entries from native byte offsets.</summary>
    private sealed class MusicBankView : IReadOnlyList<AudioUploadAssetDefinition>
    {
        /// <summary>Twenty-four music banks follow the common driver entry.</summary>
        public int Count => 24;
        /// <summary>Converts a zero-based music ordinal to its three-byte pointer-table offset.</summary>
        public AudioUploadAssetDefinition this[int index] => (uint)index < Count
            ? ResolveDataIndex((byte)(3 * (index + 1))) : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields music uploads in cartridge pointer-table order.</summary>
        public IEnumerator<AudioUploadAssetDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same cartridge order to non-generic collection consumers.</summary>
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

    /// <summary>Returns the catalog entry of one bank; <see cref="AudioBank"/> follows the pointer-table order.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bank"/> is not a defined bank.</exception>
    public static AudioUploadAssetDefinition Get(AudioBank bank) => bank switch
    {
        AudioBank.SpcEngine => Common,
        AudioBank.TitleSequence or
            AudioBank.EmptyCrateria or
            AudioBank.LowerCrateria or
            AudioBank.UpperCrateria or
            AudioBank.GreenBrinstar or
            AudioBank.RedBrinstar or
            AudioBank.UpperNorfair or
            AudioBank.LowerNorfair or
            AudioBank.Maridia or
            AudioBank.Tourian or
            AudioBank.MotherBrain or
            AudioBank.BossFight1 or
            AudioBank.BossFight2 or
            AudioBank.MiniBossFight or
            AudioBank.Ceres or
            AudioBank.WreckedShip or
            AudioBank.ZebesExplosion or
            AudioBank.Intro or
            AudioBank.Death or
            AudioBank.Credits or
            AudioBank.LastMetroidInCaptivity or
            AudioBank.GalaxyAtPeace or
            AudioBank.BabyMetroidBossFight2 or
            AudioBank.SamusThemeUpperCrateria => Music[(int)bank - 1],
        _ => throw new ArgumentOutOfRangeException(nameof(bank), bank, "Undefined audio bank."),
    };

    /// <summary>Resolves an on-disk manifest's 24-bit cartridge source address to its catalog entry.</summary>
    /// <exception cref="InvalidDataException"><paramref name="snesAddress"/> is not a retail upload stream.</exception>
    public static AudioUploadAssetDefinition ResolveSnesAddress(int snesAddress)
    {
        foreach (AudioUploadAssetDefinition definition in All)
        {
            if (definition.SnesAddress == snesAddress)
                return definition;
        }
        throw new InvalidDataException($"SNES address ${snesAddress:X6} is not a retail audio upload stream.");
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
        0x03 => new(AudioBank.TitleSequence, AudioUploadAddresses.TitleSequence, dataIndex, "Music_TitleSequence"),
        0x06 => new(AudioBank.EmptyCrateria, AudioUploadAddresses.EmptyCrateria, dataIndex, "Music_EmptyCrateria"),
        0x09 => new(AudioBank.LowerCrateria, AudioUploadAddresses.LowerCrateria, dataIndex, "Music_LowerCrateria"),
        0x0C => new(AudioBank.UpperCrateria, AudioUploadAddresses.UpperCrateria, dataIndex, "Music_UpperCrateria"),
        0x0F => new(AudioBank.GreenBrinstar, AudioUploadAddresses.GreenBrinstar, dataIndex, "Music_GreenBrinstar"),
        0x12 => new(AudioBank.RedBrinstar, AudioUploadAddresses.RedBrinstar, dataIndex, "Music_RedBrinstar"),
        0x15 => new(AudioBank.UpperNorfair, AudioUploadAddresses.UpperNorfair, dataIndex, "Music_UpperNorfair"),
        0x18 => new(AudioBank.LowerNorfair, AudioUploadAddresses.LowerNorfair, dataIndex, "Music_LowerNorfair"),
        0x1B => new(AudioBank.Maridia, AudioUploadAddresses.Maridia, dataIndex, "Music_Maridia"),
        0x1E => new(AudioBank.Tourian, AudioUploadAddresses.Tourian, dataIndex, "Music_Tourian"),
        0x21 => new(AudioBank.MotherBrain, AudioUploadAddresses.MotherBrain, dataIndex, "Music_MotherBrain"),
        0x24 => new(AudioBank.BossFight1, AudioUploadAddresses.BossFight1, dataIndex, "Music_BossFight1"),
        0x27 => new(AudioBank.BossFight2, AudioUploadAddresses.BossFight2, dataIndex, "Music_BossFight2"),
        0x2A => new(AudioBank.MiniBossFight, AudioUploadAddresses.MiniBossFight, dataIndex, "Music_MiniBossFight"),
        0x2D => new(AudioBank.Ceres, AudioUploadAddresses.Ceres, dataIndex, "Music_Ceres"),
        0x30 => new(AudioBank.WreckedShip, AudioUploadAddresses.WreckedShip, dataIndex, "Music_WreckedShip"),
        0x33 => new(AudioBank.ZebesExplosion, AudioUploadAddresses.ZebesExplosion, dataIndex, "Music_ZebesExplosion"),
        0x36 => new(AudioBank.Intro, AudioUploadAddresses.Intro, dataIndex, "Music_Intro"),
        0x39 => new(AudioBank.Death, AudioUploadAddresses.Death, dataIndex, "Music_Death"),
        0x3C => new(AudioBank.Credits, AudioUploadAddresses.Credits, dataIndex, "Music_Credits"),
        0x3F => new(AudioBank.LastMetroidInCaptivity, AudioUploadAddresses.LastMetroidInCaptivity, dataIndex, "Music_TheLastMetroidIsInCaptivity"),
        0x42 => new(AudioBank.GalaxyAtPeace, AudioUploadAddresses.GalaxyAtPeace, dataIndex, "Music_TheGalaxyIsAtPeace"),
        0x45 => new(AudioBank.BabyMetroidBossFight2, AudioUploadAddresses.BabyMetroidBossFight2, dataIndex, "Music_BabyMetroid_BossFight2"),
        0x48 => new(AudioBank.SamusThemeUpperCrateria, AudioUploadAddresses.SamusThemeUpperCrateria, dataIndex, "Music_SamusTheme_UpperCrateria"),
        _ => throw new InvalidDataException(
            $"Music data index ${dataIndex:X2} is not present in the compiled retail upload catalog."),
    };
}
