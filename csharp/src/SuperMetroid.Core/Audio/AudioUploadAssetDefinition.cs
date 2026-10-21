namespace SuperMetroid.Core.Audio;

/// <summary>Identity of one upload stream before it is materialized on disk.</summary>
/// <param name="AudioSet">Stable runtime identity used by audio lookups.</param>
/// <param name="SnesAddress">24-bit LoROM source address; the extraction boundary and the on-disk manifest key.</param>
/// <param name="DataIndex">Byte offset into the music-data pointer table, as stored in room headers.</param>
/// <param name="Name">Stable published asset name.</param>
public sealed record AudioUploadAssetDefinition(AudioBank AudioSet, int SnesAddress, byte DataIndex, string Name);
