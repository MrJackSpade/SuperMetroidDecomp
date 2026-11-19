namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Reads and writes a <see cref="SamusPoseId"/> in an installed document by its installed key (#2520).</summary>
internal sealed class SamusPoseIdJsonConverter() : InstalledKeyJsonConverter<SamusPoseId>(SamusPoseIdKeys.Installed);
