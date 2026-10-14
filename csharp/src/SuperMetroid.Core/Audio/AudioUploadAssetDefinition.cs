namespace SuperMetroid.Core.Audio;

/// <summary>Identity of one upload stream before it is materialized on disk.</summary>
public sealed record AudioUploadAssetDefinition(int SnesAddress, byte DataIndex, string Name);
