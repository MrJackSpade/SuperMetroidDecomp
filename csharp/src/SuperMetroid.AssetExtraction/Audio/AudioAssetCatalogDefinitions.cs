using SuperMetroid.Core.Audio;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// The single catalog mapping each <see cref="AudioBank"/> to the <see cref="MusicDataSet"/> it holds, its cartridge
/// address, room-header data index and stable asset name for every retail SPC upload stream.
/// The data-index values are byte offsets into the ROM's three-byte pointer table, matching
/// the values stored in room headers; they are deliberately not renumbered host indexes. They are the import
/// pipeline's knowledge (#1410): the game names a data set, and only this catalog knows the index a room header or an
/// override's song identity stores for it.
/// </summary>
[EncodingOwner("cartridge SPC upload stream data indices, byte offsets into the room-header music pointer table")]
public static class AudioAssetCatalogDefinitions
{
    /// <summary>The resident driver, sound effects, BRR directory, and shared samples.</summary>
    public static readonly AudioUploadAssetDefinition Common =
        new(AudioBank.SpcEngine, null, (int)AudioUploadSourceDefinitions.SpcEngine, 0x00, "SPCEngine");

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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="audioSet"/> is not a defined bank.</exception>
    public static AudioUploadAssetDefinition Get(AudioBank audioSet) => audioSet switch
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
            AudioBank.SamusThemeUpperCrateria => Music[(int)audioSet - 1],
        _ => throw new ArgumentOutOfRangeException(nameof(audioSet), audioSet, "Undefined audio bank."),
    };

    /// <summary>
    /// Returns the catalog entry of the stream holding <paramref name="dataSet"/>, or the common stream for no data set,
    /// which holds only the shared sounds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataSet"/> is not a defined data set.</exception>
    public static AudioUploadAssetDefinition Get(MusicDataSet? dataSet)
    {
        if (dataSet is not MusicDataSet set)
            return Common;
        foreach (AudioUploadAssetDefinition definition in Music)
        {
            if (definition.DataSet == set)
                return definition;
        }
        throw new ArgumentOutOfRangeException(nameof(dataSet), dataSet, "Undefined music data set.");
    }

    /// <summary>The upload stream that holds <paramref name="dataSet"/>, or the resident driver's for no data set.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataSet"/> is not a defined data set.</exception>
    public static AudioBank BankOf(MusicDataSet? dataSet) => Get(dataSet).AudioSet;

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
    /// <exception cref="InvalidDataException"><paramref name="dataIndex"/> names no retail upload stream.</exception>
    public static AudioUploadAssetDefinition ResolveDataIndex(int dataIndex) => dataIndex switch
    {
        0 => Common,
        0x03 => new(AudioBank.TitleSequence, MusicDataSet.TitleSequence, (int)AudioUploadSourceDefinitions.TitleSequence, dataIndex, "Music_TitleSequence"),
        0x06 => new(AudioBank.EmptyCrateria, MusicDataSet.EmptyCrateria, (int)AudioUploadSourceDefinitions.EmptyCrateria, dataIndex, "Music_EmptyCrateria"),
        0x09 => new(AudioBank.LowerCrateria, MusicDataSet.LowerCrateria, (int)AudioUploadSourceDefinitions.LowerCrateria, dataIndex, "Music_LowerCrateria"),
        0x0C => new(AudioBank.UpperCrateria, MusicDataSet.UpperCrateria, (int)AudioUploadSourceDefinitions.UpperCrateria, dataIndex, "Music_UpperCrateria"),
        0x0F => new(AudioBank.GreenBrinstar, MusicDataSet.GreenBrinstar, (int)AudioUploadSourceDefinitions.GreenBrinstar, dataIndex, "Music_GreenBrinstar"),
        0x12 => new(AudioBank.RedBrinstar, MusicDataSet.RedBrinstar, (int)AudioUploadSourceDefinitions.RedBrinstar, dataIndex, "Music_RedBrinstar"),
        0x15 => new(AudioBank.UpperNorfair, MusicDataSet.UpperNorfair, (int)AudioUploadSourceDefinitions.UpperNorfair, dataIndex, "Music_UpperNorfair"),
        0x18 => new(AudioBank.LowerNorfair, MusicDataSet.LowerNorfair, (int)AudioUploadSourceDefinitions.LowerNorfair, dataIndex, "Music_LowerNorfair"),
        0x1B => new(AudioBank.Maridia, MusicDataSet.Maridia, (int)AudioUploadSourceDefinitions.Maridia, dataIndex, "Music_Maridia"),
        0x1E => new(AudioBank.Tourian, MusicDataSet.Tourian, (int)AudioUploadSourceDefinitions.Tourian, dataIndex, "Music_Tourian"),
        0x21 => new(AudioBank.MotherBrain, MusicDataSet.MotherBrain, (int)AudioUploadSourceDefinitions.MotherBrain, dataIndex, "Music_MotherBrain"),
        0x24 => new(AudioBank.BossFight1, MusicDataSet.BossFight1, (int)AudioUploadSourceDefinitions.BossFight1, dataIndex, "Music_BossFight1"),
        0x27 => new(AudioBank.BossFight2, MusicDataSet.BossFight2, (int)AudioUploadSourceDefinitions.BossFight2, dataIndex, "Music_BossFight2"),
        0x2A => new(AudioBank.MiniBossFight, MusicDataSet.MiniBossFight, (int)AudioUploadSourceDefinitions.MiniBossFight, dataIndex, "Music_MiniBossFight"),
        0x2D => new(AudioBank.Ceres, MusicDataSet.Ceres, (int)AudioUploadSourceDefinitions.Ceres, dataIndex, "Music_Ceres"),
        0x30 => new(AudioBank.WreckedShip, MusicDataSet.WreckedShip, (int)AudioUploadSourceDefinitions.WreckedShip, dataIndex, "Music_WreckedShip"),
        0x33 => new(AudioBank.ZebesExplosion, MusicDataSet.ZebesExplosion, (int)AudioUploadSourceDefinitions.ZebesExplosion, dataIndex, "Music_ZebesExplosion"),
        0x36 => new(AudioBank.Intro, MusicDataSet.Intro, (int)AudioUploadSourceDefinitions.Intro, dataIndex, "Music_Intro"),
        0x39 => new(AudioBank.Death, MusicDataSet.Death, (int)AudioUploadSourceDefinitions.Death, dataIndex, "Music_Death"),
        0x3C => new(AudioBank.Credits, MusicDataSet.Credits, (int)AudioUploadSourceDefinitions.Credits, dataIndex, "Music_Credits"),
        0x3F => new(AudioBank.LastMetroidInCaptivity, MusicDataSet.LastMetroidInCaptivity, (int)AudioUploadSourceDefinitions.LastMetroidInCaptivity, dataIndex, "Music_TheLastMetroidIsInCaptivity"),
        0x42 => new(AudioBank.GalaxyAtPeace, MusicDataSet.GalaxyAtPeace, (int)AudioUploadSourceDefinitions.GalaxyAtPeace, dataIndex, "Music_TheGalaxyIsAtPeace"),
        0x45 => new(AudioBank.BabyMetroidBossFight2, MusicDataSet.BabyMetroidBossFight2, (int)AudioUploadSourceDefinitions.BabyMetroidBossFight2, dataIndex, "Music_BabyMetroid_BossFight2"),
        0x48 => new(AudioBank.SamusThemeUpperCrateria, MusicDataSet.SamusThemeUpperCrateria, (int)AudioUploadSourceDefinitions.SamusThemeUpperCrateria, dataIndex, "Music_SamusTheme_UpperCrateria"),
        _ => throw new InvalidDataException(
            $"Music data index ${dataIndex:X2} is not present in the compiled retail upload catalog."),
    };
}
