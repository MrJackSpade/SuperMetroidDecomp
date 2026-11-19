namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads and writes a <see cref="BlockMirroring"/> in an installed document by its installed key (#2520).</summary>
internal sealed class BlockMirroringJsonConverter() : InstalledKeyJsonConverter<BlockMirroring>(BlockMirroringKeys.Installed);
