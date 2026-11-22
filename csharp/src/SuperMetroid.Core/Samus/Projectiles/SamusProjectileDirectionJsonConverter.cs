namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Reads and writes a <see cref="SamusProjectileDirection"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class SamusProjectileDirectionJsonConverter() : InstalledKeyJsonConverter<SamusProjectileDirection>(SamusProjectileDirectionKeys.Installed);
