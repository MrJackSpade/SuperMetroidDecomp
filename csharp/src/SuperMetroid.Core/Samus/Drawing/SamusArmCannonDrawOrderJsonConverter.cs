namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Reads and writes a <see cref="SamusArmCannonDrawOrder"/> in an installed document by its installed key (#2520).</summary>
internal sealed class SamusArmCannonDrawOrderJsonConverter() : InstalledKeyJsonConverter<SamusArmCannonDrawOrder>(SamusArmCannonDrawOrderKeys.Installed);
