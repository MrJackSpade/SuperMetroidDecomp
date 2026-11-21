namespace SuperMetroid.Core.Audio;

/// <summary>Reads and writes a <see cref="MusicDataSet"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class MusicDataSetJsonConverter() : InstalledKeyJsonConverter<MusicDataSet>(MusicDataSetKeys.Installed);
