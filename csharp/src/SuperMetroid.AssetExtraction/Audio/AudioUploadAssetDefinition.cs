using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Identity of one upload stream before it is materialized on disk.</summary>
/// <param name="AudioSet">The upload stream's identity in the import pipeline.</param>
/// <param name="DataSet">The music data set the stream holds, or null for the resident driver and shared sounds.</param>
/// <param name="SnesAddress">24-bit LoROM source address; the extraction boundary and the on-disk manifest key.</param>
/// <param name="DataIndex">Byte offset into the music-data pointer table, as stored in room headers.</param>
/// <param name="Name">Stable published asset name.</param>
public sealed record AudioUploadAssetDefinition(AudioBank AudioSet, MusicDataSet? DataSet, int SnesAddress, byte DataIndex, string Name);
